using System;
using System.Collections.Generic;
using System.Linq;

namespace GUIshka
{
    public class SyntaxError
    {
        public string Fragment { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
    }

    public class SyntaxResult
    {
        public bool Success { get; set; }
        public List<SyntaxError> Errors { get; set; } = new List<SyntaxError>();
        public int ErrorCount => Errors.Count;
        public ProgramNode Ast { get; set; }
    }

    public class SyntaxAnalyzer
    {
        private enum RecoveryResult
        {
            NotRecovered,
            PrimaryAnchor,
            SecondaryAnchor
        }

        private SyntaxResult result;
        private List<Lexeme> tokens;
        private int pos;
        private List<(int Line, int Start, int End)> lexicalErrorSpans;
        private HashSet<string> emittedErrorKeys;
        private ProgramNode programRoot;

        private Lexeme Current => pos < tokens.Count ? tokens[pos] : null;

        public SyntaxResult Analyze(List<Lexeme> lexemes)
        {
            result = new SyntaxResult();
            emittedErrorKeys = new HashSet<string>();
            programRoot = new ProgramNode();

            lexicalErrorSpans = lexemes
                .Where(l => l != null && l.IsError)
                .Select(l => (l.Line, l.StartPos, l.EndPos))
                .ToList();

            tokens = lexemes
                .Where(l => l.Code != 35 && !l.IsError)
                .ToList();

            pos = 0;

            while (pos < tokens.Count)
            {
                if (IsSemicolon(Current))
                {
                    pos++;
                    continue;
                }

                int startPos = pos;
                ParseStatement();

                if (pos == startPos)
                {
                    pos++;
                }
            }

            result.Success = result.ErrorCount == 0;
            result.Ast = programRoot;
            return result;
        }

        private void ParseStatement()
        {
            if (Current == null)
            {
                return;
            }

            Lexeme nameLex = null;
            OperandNode op1 = null;
            OperandNode op2 = null;

            bool hasIdentifier = RequireIdentifierAtStatementStart();
            if (Current == null)
            {
                return;
            }

            if (hasIdentifier)
            {
                nameLex = Current;
                pos++;
            }
            else if (IsSemicolon(Current))
            {
                pos++;
                return;
            }

            if (!TryMatch(30))
            {
                AddError(Current, "Пропущен оператор присваивания '='");

                RecoveryResult recovery = RecoverToAnchor(IsKeywordComplex, IsStatementBoundary);
                if (recovery == RecoveryResult.NotRecovered || recovery == RecoveryResult.SecondaryAnchor)
                {
                    return;
                }
            }

            while (IsAssign(Current))
            {
                AddError(Current, "Лишний оператор присваивания '='");
                pos++;
            }

            bool hasComplexKeyword = IsKeywordComplex(Current);
            if (!hasComplexKeyword)
            {
                AddError(Current, "Ожидалось ключевое слово 'complex'");

                RecoveryResult recovery = RecoverToAnchor(
                    l => IsKeywordComplex(l) || IsLeftParenthesis(l) || IsOperandStart(l) || IsComma(l) || IsRightParenthesis(l),
                    IsSemicolon);

                if (recovery == RecoveryResult.SecondaryAnchor && IsIdentifier(Current))
                {
                    return;
                }

                hasComplexKeyword = IsKeywordComplex(Current);
            }

            if (hasComplexKeyword)
            {
                pos++;
            }

            if (!TryMatch(31))
            {
                AddError(Current, "Ожидалась '('");

                RecoveryResult recovery = RecoverToAnchor(
                    IsOperandStart,
                    l => IsComma(l) || IsRightParenthesis(l) || IsStatementBoundary(l));

                if (recovery == RecoveryResult.NotRecovered)
                {
                    return;
                }
            }

            if (!ParseOperand("первое число или идентификатор", out op1))
            {
                if (Current == null)
                {
                    AddError(Current, "Ожидалась ')'");
                    AddError(Current, "Ожидалась ';'");
                    return;
                }

                RecoveryResult recovery = RecoverToAnchor(l => IsComma(l) || IsRightParenthesis(l), IsStatementBoundary);
                if (recovery == RecoveryResult.SecondaryAnchor && IsIdentifier(Current))
                {
                    return;
                }
            }

            if (!TryMatch(33))
            {
                AddError(Current, "Ожидалась запятая ','");

                RecoveryResult recovery = RecoverToAnchor(l => IsOperandStart(l) || IsRightParenthesis(l), IsStatementBoundary);
                if (recovery == RecoveryResult.SecondaryAnchor && IsIdentifier(Current))
                {
                    return;
                }
            }

            if (!ParseOperand("второе число или идентификатор", out op2))
            {
                RecoveryResult recovery = RecoverToAnchor(IsRightParenthesis, IsStatementBoundary);
                if (recovery == RecoveryResult.SecondaryAnchor && IsIdentifier(Current))
                {
                    return;
                }
            }

            if (!TryMatch(32))
            {
                AddError(Current, "Ожидалась ')'");

                RecoveryResult recovery = RecoverToAnchor(IsRightParenthesis, IsStatementBoundary);
                if (recovery == RecoveryResult.SecondaryAnchor)
                {
                    return;
                }

                if (recovery == RecoveryResult.PrimaryAnchor)
                {
                    TryMatch(32);
                }
            }

            if (!TryMatch(34))
            {
                AddError(Current, "Ожидалась ';'");

                RecoveryResult recovery = RecoverToAnchor(IsSemicolon, IsIdentifier);
                if (recovery == RecoveryResult.NotRecovered)
                {
                    return;
                }

                TryMatch(34);
            }

            if (nameLex != null && op1 != null && op2 != null)
            {
                programRoot.Statements.Add(new ComplexDeclNode(nameLex, op1, op2));
            }
        }

        private bool ParseOperand(string label, out OperandNode operand)
        {
            operand = null;

            if (Current == null)
            {
                AddError(Current, $"Ожидалось {label}");
                return false;
            }

            if (IsIdentifier(Current))
            {
                Lexeme id = Current;
                pos++;
                operand = new IdentifierRefNode(id);
                return true;
            }

            if (IsUnsupportedValueLexeme(Current))
            {
                Lexeme badLiteral = Current;
                pos++;
                operand = new LiteralNode(new List<Lexeme> { badLiteral });
                return true;
            }

            var parts = new List<Lexeme>();

            if (IsSign(Current))
            {
                Lexeme sign = Current;
                parts.Add(sign);
                pos++;

                if (Current == null)
                {
                    AddError(Current, $"Ожидалось {label}");
                    return false;
                }

                if (IsScalarNumericLexeme(Current))
                {
                    parts.Add(Current);
                    pos++;
                    operand = new LiteralNode(parts);
                    return true;
                }

                if (IsComplexLiteralLexeme(Current))
                {
                    AddError(Current, "Комплексный литерал недопустим в аргументе complex()");
                    return false;
                }

                if (IsUnsupportedValueLexeme(Current))
                {
                    parts.Add(Current);
                    pos++;
                    operand = new LiteralNode(parts);
                    return true;
                }

                AddError(sign, $"После знака '{sign.Value}' должно идти {label}");
                return false;
            }

            if (IsScalarNumericLexeme(Current))
            {
                parts.Add(Current);
                pos++;
                operand = new LiteralNode(parts);
                return true;
            }

            if (IsComplexLiteralLexeme(Current))
            {
                AddError(Current, "Комплексный литерал недопустим в аргументе complex()");
                return false;
            }

            AddError(Current, $"Ожидалось {label}");
            return false;
        }

        private bool TryMatch(int code)
        {
            if (Current != null && Current.Code == code)
            {
                pos++;
                return true;
            }

            return false;
        }

        private bool RequireIdentifierAtStatementStart()
        {
            if (IsIdentifier(Current))
            {
                return true;
            }

            AddError(Current, "Ожидался идентификатор в начале оператора");

            RecoveryResult recovery = RecoverToAnchor(
                l => IsIdentifier(l) || IsAssign(l) || IsKeywordComplex(l) || IsLeftParenthesis(l),
                IsSemicolon);

            if (recovery == RecoveryResult.SecondaryAnchor || Current == null)
            {
                return false;
            }

            return IsIdentifier(Current);
        }

        private RecoveryResult RecoverToAnchor(Func<Lexeme, bool> primaryAnchor, Func<Lexeme, bool> secondaryAnchor)
        {
            while (Current != null)
            {
                if (primaryAnchor != null && primaryAnchor(Current))
                {
                    return RecoveryResult.PrimaryAnchor;
                }

                if (secondaryAnchor != null && secondaryAnchor(Current))
                {
                    return RecoveryResult.SecondaryAnchor;
                }

                pos++;
            }

            return RecoveryResult.NotRecovered;
        }

        private bool IsStatementBoundary(Lexeme l)
        {
            return IsSemicolon(l) || IsIdentifier(l);
        }

        private bool IsOperandStart(Lexeme l)
        {
            return IsScalarNumericLexeme(l) || IsSign(l) || IsIdentifier(l) || IsUnsupportedValueLexeme(l);
        }

        private bool IsSign(Lexeme l)
        {
            return l != null && (l.Code == 36 || l.Code == 37);
        }

        private bool IsSemicolon(Lexeme l)
        {
            return l != null && l.Code == 34;
        }

        private bool IsAssign(Lexeme l)
        {
            return l != null && l.Code == 30;
        }

        private bool IsComma(Lexeme l)
        {
            return l != null && l.Code == 33;
        }

        private bool IsLeftParenthesis(Lexeme l)
        {
            return l != null && l.Code == 31;
        }

        private bool IsRightParenthesis(Lexeme l)
        {
            return l != null && l.Code == 32;
        }

        private bool IsIdentifier(Lexeme l)
        {
            return l != null && l.Code == 20;
        }

        private bool IsKeywordComplex(Lexeme l)
        {
            return l != null && l.Code == 21;
        }

        private static bool IsScalarNumericLexeme(Lexeme l)
        {
            return l != null && l.Code >= 1 && l.Code <= 4;
        }

        private static bool IsComplexLiteralLexeme(Lexeme l)
        {
            return l != null && (l.Code == 7 || l.Code == 8);
        }

        private static bool IsUnsupportedValueLexeme(Lexeme l)
        {
            return l != null && (l.Code == 22 || l.Code == 23);
        }

        private void AddError(Lexeme lex, string msg)
        {
            int line;
            int position;
            string fragment;

            if (lex != null)
            {
                line = lex.Line;
                position = lex.StartPos;
                fragment = lex.Value ?? string.Empty;
            }
            else if (pos > 0 && pos - 1 < tokens.Count && tokens[pos - 1] != null)
            {
                Lexeme previous = tokens[pos - 1];
                line = previous.Line;
                position = previous.EndPos + 1;
                fragment = string.Empty;
            }
            else
            {
                line = 1;
                position = 1;
                fragment = string.Empty;
            }

            if (IsCoveredByLexicalError(line, position))
            {
                return;
            }

            string dedupKey = $"{line}:{position}:{fragment}:{msg}";
            if (emittedErrorKeys.Contains(dedupKey))
            {
                return;
            }

            emittedErrorKeys.Add(dedupKey);

            result.Errors.Add(new SyntaxError
            {
                Fragment = fragment,
                Line = line,
                Position = position,
                Description = msg
            });
        }

        private bool IsCoveredByLexicalError(int line, int position)
        {
            foreach (var span in lexicalErrorSpans)
            {
                if (span.Line == line && position >= span.Start && position <= span.End)
                {
                    return true;
                }
            }

            return false;
        }
    }
}