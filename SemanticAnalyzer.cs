using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GUIshka
{
    public sealed class SemanticError
    {
        public string Fragment { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
    }

    public sealed class SemanticResult
    {
        public bool Success { get; set; }
        public List<SemanticError> Errors { get; set; } = new List<SemanticError>();
        public int ErrorCount => Errors.Count;
    }

    public sealed class SemanticAnalyzer
    {
        private const double MaxAbsFloat = 1e308;

        public SemanticResult Analyze(ProgramNode program)
        {
            var result = new SemanticResult();

            if (program == null)
            {
                result.Success = true;
                return result;
            }

            var table = new SymbolTable();

            foreach (ComplexDeclNode stmt in program.Statements)
            {
                string name = stmt.NameLexeme.Value ?? string.Empty;
                int nameLine = stmt.NameLexeme.Line;
                int namePos = stmt.NameLexeme.StartPos;

                bool okRe = TryEvaluateOperand(
                    stmt.RealPart,
                    table,
                    result.Errors,
                    out double reVal,
                    out _,
                    out _,
                    out _);

                bool okIm = TryEvaluateOperand(
                    stmt.ImagPart,
                    table,
                    result.Errors,
                    out double imVal,
                    out _,
                    out _,
                    out _);

                if (!okRe || !okIm)
                {
                    continue;
                }

                var entry = new SymbolEntry
                {
                    Name = name,
                    DeclLine = nameLine,
                    DeclStartPos = namePos,
                    Real = reVal,
                    Imag = imVal
                };

                if (!table.TryDeclare(entry, out SymbolEntry duplicate))
                {
                    result.Errors.Add(new SemanticError
                    {
                        Fragment = name,
                        Line = nameLine,
                        Position = namePos,
                        Description =
                            $"Ошибка: идентификатор \"{name}\" уже объявлен ранее (строка {duplicate.DeclLine})"
                    });
                }
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        private bool TryEvaluateOperand(
            OperandNode operand,
            SymbolTable table,
            List<SemanticError> errors,
            out double value,
            out int reportLine,
            out int reportPos,
            out string fragment)
        {
            value = 0;
            reportLine = 1;
            reportPos = 1;
            fragment = string.Empty;

            if (operand is IdentifierRefNode idNode)
            {
                string id = idNode.IdentifierLexeme.Value ?? string.Empty;
                reportLine = idNode.IdentifierLexeme.Line;
                reportPos = idNode.IdentifierLexeme.StartPos;
                fragment = id;

                if (!table.TryGet(id, out SymbolEntry sym))
                {
                    errors.Add(new SemanticError
                    {
                        Fragment = id,
                        Line = reportLine,
                        Position = reportPos,
                        Description = $"Ошибка: идентификатор \"{id}\" использован до объявления или не объявлен"
                    });

                    return false;
                }

                value = sym.Real;
                return true;
            }

            if (operand is LiteralNode lit)
            {
                reportLine = lit.Line;
                reportPos = lit.StartPos;
                fragment = lit.GetRawText();

                foreach (Lexeme lx in lit.Lexemes)
                {
                    if (lx != null && IsStringLiteralLexeme(lx))
                    {
                        errors.Add(new SemanticError
                        {
                            Fragment = fragment,
                            Line = reportLine,
                            Position = reportPos,
                            Description =
                                "Ошибка: строковое значение не может использоваться как числовой аргумент функции complex"
                        });

                        return false;
                    }

                    if (lx != null && IsBooleanLiteralLexeme(lx))
                    {
                        errors.Add(new SemanticError
                        {
                            Fragment = fragment,
                            Line = reportLine,
                            Position = reportPos,
                            Description =
                                "Ошибка: логическое значение не может использоваться как числовой аргумент функции complex"
                        });

                        return false;
                    }

                    if (lx != null && IsComplexLiteralLexeme(lx))
                    {
                        errors.Add(new SemanticError
                        {
                            Fragment = fragment,
                            Line = reportLine,
                            Position = reportPos,
                            Description =
                                "Ошибка: тип инициализирующего значения не совместим со скалярным операндом complex, так как комплексный литерал здесь недопустим"
                        });

                        return false;
                    }
                }

                if (!TryParseLiteralScalar(lit, out value, out string rangeMsg))
                {
                    errors.Add(new SemanticError
                    {
                        Fragment = fragment,
                        Line = reportLine,
                        Position = reportPos,
                        Description = rangeMsg
                    });

                    return false;
                }

                return true;
            }

            errors.Add(new SemanticError
            {
                Fragment = fragment,
                Line = reportLine,
                Position = reportPos,
                Description = "Ошибка: неподдерживаемый операнд"
            });

            return false;
        }

        private static bool IsComplexLiteralLexeme(Lexeme l)
        {
            return l != null && (l.Code == 7 || l.Code == 8);
        }

        private static bool IsBooleanLiteralLexeme(Lexeme l)
        {
            return l != null && l.Code == 22;
        }

        private static bool IsStringLiteralLexeme(Lexeme l)
        {
            return l != null && l.Code == 23;
        }

        private static bool IsIntegerLexeme(Lexeme l)
        {
            return l != null && (l.Code == 1 || l.Code == 2);
        }

        private static bool IsFloatLexeme(Lexeme l)
        {
            return l != null && (l.Code == 3 || l.Code == 4);
        }

        private bool TryParseLiteralScalar(LiteralNode lit, out double value, out string errorMessage)
        {
            value = 0;
            errorMessage = null;

            string raw = lit.GetRawText().Trim();

            if (string.IsNullOrEmpty(raw))
            {
                errorMessage = "Ошибка: пустой числовой литерал";
                return false;
            }

            Lexeme primaryNumber = lit.Lexemes.LastOrDefault(lx =>
                lx != null && (IsIntegerLexeme(lx) || IsFloatLexeme(lx)));

            if (primaryNumber == null)
            {
                errorMessage = "Ошибка: не удалось определить тип числового литерала";
                return false;
            }

            if (IsFloatLexeme(primaryNumber))
            {
                if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    errorMessage = "Ошибка: не удалось разобрать вещественный литерал";
                    return false;
                }

                if (!IsFiniteDouble(parsed) || Math.Abs(parsed) > MaxAbsFloat)
                {
                    errorMessage = "Ошибка: значение вещественного литерала вне допустимого диапазона";
                    return false;
                }

                value = parsed;
                return true;
            }

            if (IsIntegerLexeme(primaryNumber))
            {
                if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longVal))
                {
                    errorMessage = "Ошибка: не удалось разобрать целый литерал";
                    return false;
                }

                if (longVal < int.MinValue || longVal > int.MaxValue)
                {
                    errorMessage = "Ошибка: целый литерал выходит за пределы диапазона Int32";
                    return false;
                }

                value = longVal;
                return true;
            }

            errorMessage = "Ошибка: не удалось проверить тип числового литерала";
            return false;
        }

        private static bool IsFiniteDouble(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}