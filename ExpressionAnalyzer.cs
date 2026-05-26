using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GUIshka
{
    public class ArithmeticAnalyzer
    {
        private string input;
        private int pos;
        private int currentCharPos;
        private List<ErrorInfo> errors;
        private List<ArithmeticTriple> triples;
        private int tempVarCounter;
        private bool hasUnmatchedClosingBracket;
        private bool hasEmptyParenthesesError;
        private bool hasMissingOperandError;
        private bool hasUnmatchedOpeningBracket;
        private int unmatchedOpeningBracketPos;
        private bool isParsingInsideParentheses;

        public class ErrorInfo
        {
            public int Position { get; set; }
            public string Fragment { get; set; }
            public string Description { get; set; }
        }

        private char Current => pos < input.Length ? input[pos] : '\0';
        private char Peek => pos + 1 < input.Length ? input[pos + 1] : '\0';

        public class ArithmeticTriple
        {
            public string Op { get; set; }
            public string Arg1 { get; set; }
            public string Arg2 { get; set; }
            public string Result { get; set; }

            public ArithmeticTriple(string op, string arg1, string arg2, string result)
            {
                Op = op;
                Arg1 = arg1;
                Arg2 = arg2;
                Result = result;
            }

            public override string ToString()
            {
                return $"({Op}, {Arg1}, {Arg2}, {Result})";
            }
        }

        public class ArithmeticResult
        {
            public bool Success { get; set; }
            public List<ErrorInfo> Errors { get; set; } = new List<ErrorInfo>();
            public List<ArithmeticTriple> Triples { get; set; } = new List<ArithmeticTriple>();
            public string Polski { get; set; }
            public int? CalculatedValue { get; set; }
        }

        public ArithmeticResult Analyze(string text)
        {
            var result = new ArithmeticResult();
            errors = new List<ErrorInfo>();
            triples = new List<ArithmeticTriple>();
            tempVarCounter = 1;
            hasUnmatchedClosingBracket = false;
            hasEmptyParenthesesError = false;
            hasMissingOperandError = false;
            hasUnmatchedOpeningBracket = false;
            unmatchedOpeningBracketPos = -1;
            isParsingInsideParentheses = false;

            input = text;
            pos = 0;
            currentCharPos = 1;

            if (string.IsNullOrWhiteSpace(input))
            {
                result.Success = false;
                result.Errors.Add(new ErrorInfo { Position = 1, Fragment = "", Description = "Введите арифметическое выражение" });
                return result;
            }

            try
            {
                var astRoot = ParseE();
                if (hasUnmatchedOpeningBracket && unmatchedOpeningBracketPos >= 0)
                {
                    AddError(unmatchedOpeningBracketPos, "(", "Непарная открывающая скобка: отсутствует соответствующая ')'");
                }

                if (!hasMissingOperandError && !hasUnmatchedOpeningBracket && !hasEmptyParenthesesError)
                {
                    CheckRemainingCharacters();
                }

                if (errors.Count == 0 && astRoot != null)
                {
                    if (astRoot.IsLeaf)
                    {
                        result.Triples = new List<ArithmeticTriple>();
                        result.Polski = astRoot.Value;
                        if (int.TryParse(astRoot.Value, out int num))
                        {
                            result.CalculatedValue = num;
                        }
                        result.Success = true;
                        result.Errors = errors;
                        return result;
                    }

                    GenerateTriples(astRoot);
                    result.Triples = triples;
                    result.Polski = BuildPolski(astRoot);

                    if (!ContainsIdentifiers(astRoot))
                    {
                        result.CalculatedValue = EvaluateAst(astRoot);
                    }

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                if (errors.Count == 0)
                {
                    AddError(Math.Max(0, pos - 1), "", $"Ошибка: {ex.Message}");
                }
                result.Success = false;
            }

            result.Errors = errors
                .GroupBy(e => $"{e.Position}:{e.Fragment}:{e.Description}")
                .Select(g => g.First())
                .ToList();

            return result;
        }

        private void CheckRemainingCharacters()
        {
            if (pos >= input.Length) return;

            int tempPos = pos;
            while (tempPos < input.Length && char.IsWhiteSpace(input[tempPos]))
            {
                tempPos++;
            }

            if (tempPos >= input.Length) return;

            if (hasEmptyParenthesesError) return;

            if (input[tempPos] == ')')
            {
                if (!hasUnmatchedClosingBracket)
                {
                    AddError(tempPos, ")", "Непарная закрывающая скобка: отсутствует соответствующая '('");
                    hasUnmatchedClosingBracket = true;
                }
                return;
            }

            string fragment = input.Substring(tempPos);
            if (fragment.Length > 30) fragment = fragment.Substring(0, 30) + "...";
            AddError(tempPos, fragment, "Лишние символы после завершения выражения");
        }

        private void AddError(int position, string fragment, string msg)
        {
            if (errors.Any(e => e.Position == position + 1 && e.Description == msg))
                return;

            errors.Add(new ErrorInfo
            {
                Position = position + 1,
                Fragment = fragment.Length > 30 ? fragment.Substring(0, 30) + "..." : fragment,
                Description = msg
            });
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        private void SkipWhitespace()
        {
            while (pos < input.Length && char.IsWhiteSpace(input[pos]))
            {
                if (input[pos] == ' ') currentCharPos++;
                pos++;
            }
        }

        private string ReadNumber()
        {
            int start = pos;
            while (pos < input.Length && IsDigit(input[pos]))
            {
                currentCharPos++;
                pos++;
            }
            return input.Substring(start, pos - start);
        }

        private string ReadIdentifier()
        {
            int start = pos;
            while (pos < input.Length && (IsLetter(input[pos]) || IsDigit(input[pos]) || input[pos] == '_'))
            {
                currentCharPos++;
                pos++;
            }
            return input.Substring(start, pos - start);
        }

        private class AstNode
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string NodeKind { get; set; }
            public List<AstNode> Children { get; set; }
            public bool IsLeaf => Children.Count == 0;

            public AstNode()
            {
                Children = new List<AstNode>();
            }
        }

        private AstNode ParseE()
        {
            SkipWhitespace();

            var node = new AstNode { Type = "E" };

            if (pos >= input.Length)
            {
                if (!isParsingInsideParentheses)
                {
                    if (!hasMissingOperandError && !hasUnmatchedOpeningBracket && !hasEmptyParenthesesError)
                    {
                        AddError(pos, "", "Пустое выражение");
                    }
                }
                return null;
            }

            var tNode = ParseT();
            if (tNode == null)
            {
                if (pos < input.Length && !IsDigit(Current) && !IsLetter(Current) && Current != '(' && Current != ')')
                {
                    AddError(pos, Current.ToString(), $"Недопустимый символ '{Current}'");
                    pos++;
                    currentCharPos++;
                    return ParseE();
                }
                return null;
            }
            node.Children.Add(tNode);

            var aNode = ParseA();
            if (aNode != null)
                node.Children.Add(aNode);

            return node;
        }

        private bool IsNextTokenOpenParenthesis()
        {
            int tempPos = pos;
            while (tempPos < input.Length && char.IsWhiteSpace(input[tempPos]))
                tempPos++;

            if (tempPos >= input.Length) return false;
            return input[tempPos] == '(';
        }

        private AstNode ParseA()
        {
            SkipWhitespace();

            if (Current == '+')
            {
                int opPos = pos;
                pos++;
                currentCharPos++;

                SkipWhitespace();

                if (!IsNextTokenOperand())
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "+", "Ожидается операнд после '+'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var tNode = ParseT();
                if (tNode == null)
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "+", "Ожидается операнд после '+'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var node = new AstNode { Type = "A", Value = "+" };
                node.Children.Add(tNode);

                var aNode = ParseA();
                if (aNode != null) node.Children.Add(aNode);

                return node;
            }
            else if (Current == '-')
            {
                int opPos = pos;
                pos++;
                currentCharPos++;

                SkipWhitespace();

                if (!IsNextTokenOperand())
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "-", "Ожидается операнд после '-'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var tNode = ParseT();
                if (tNode == null)
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "-", "Ожидается операнд после '-'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var node = new AstNode { Type = "A", Value = "-" };
                node.Children.Add(tNode);

                var aNode = ParseA();
                if (aNode != null) node.Children.Add(aNode);

                return node;
            }

            return null;
        }

        private AstNode ParseT()
        {
            SkipWhitespace();

            var node = new AstNode { Type = "T" };
            var fNode = ParseF();
            if (fNode == null) return null;
            node.Children.Add(fNode);

            var bNode = ParseB();
            if (bNode != null)
                node.Children.Add(bNode);

            return node;
        }

        private bool IsNextTokenOperand()
        {
            int tempPos = pos;
            while (tempPos < input.Length && char.IsWhiteSpace(input[tempPos]))
                tempPos++;

            if (tempPos >= input.Length) return false;
            char c = input[tempPos];
            return IsDigit(c) || IsLetter(c) || c == '_' || c == '(';
        }

        private AstNode ParseB()
        {
            SkipWhitespace();

            char op = Current;

            if (op == '*')
            {
                if (Peek == '*')
                {
                    int opPos = pos;
                    pos += 2;
                    currentCharPos += 2;

                    SkipWhitespace();

                    if (!IsNextTokenOperand())
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "**", "Ожидается операнд после '**'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var fNode = ParseF();
                    if (fNode == null)
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "**", "Ожидается операнд после '**'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var node = new AstNode { Type = "B", Value = "**" };
                    node.Children.Add(fNode);

                    var bNode = ParseB();
                    if (bNode != null) node.Children.Add(bNode);

                    return node;
                }
                else
                {
                    int opPos = pos;
                    pos++;
                    currentCharPos++;

                    SkipWhitespace();

                    if (!IsNextTokenOperand())
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "*", "Ожидается операнд после '*'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var fNode = ParseF();
                    if (fNode == null)
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "*", "Ожидается операнд после '*'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var node = new AstNode { Type = "B", Value = "*" };
                    node.Children.Add(fNode);

                    var bNode = ParseB();
                    if (bNode != null) node.Children.Add(bNode);

                    return node;
                }
            }
            else if (op == '/')
            {
                if (Peek == '/')
                {
                    int opPos = pos;
                    pos += 2;
                    currentCharPos += 2;

                    SkipWhitespace();

                    if (!IsNextTokenOperand())
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "//", "Ожидается операнд после '//'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var fNode = ParseF();
                    if (fNode == null)
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "//", "Ожидается операнд после '//'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var node = new AstNode { Type = "B", Value = "//" };
                    node.Children.Add(fNode);

                    var bNode = ParseB();
                    if (bNode != null) node.Children.Add(bNode);

                    return node;
                }
                else
                {
                    int opPos = pos;
                    pos++;
                    currentCharPos++;

                    SkipWhitespace();

                    if (!IsNextTokenOperand())
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "/", "Ожидается операнд после '/'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var fNode = ParseF();
                    if (fNode == null)
                    {
                        if (!IsNextTokenOpenParenthesis())
                        {
                            if (!hasMissingOperandError)
                            {
                                AddError(opPos, "/", "Ожидается операнд после '/'");
                                hasMissingOperandError = true;
                            }
                        }
                        return null;
                    }

                    var node = new AstNode { Type = "B", Value = "/" };
                    node.Children.Add(fNode);

                    var bNode = ParseB();
                    if (bNode != null) node.Children.Add(bNode);

                    return node;
                }
            }
            else if (op == '%')
            {
                int opPos = pos;
                pos++;
                currentCharPos++;

                SkipWhitespace();

                if (!IsNextTokenOperand())
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "%", "Ожидается операнд после '%'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var fNode = ParseF();
                if (fNode == null)
                {
                    if (!IsNextTokenOpenParenthesis())
                    {
                        if (!hasMissingOperandError)
                        {
                            AddError(opPos, "%", "Ожидается операнд после '%'");
                            hasMissingOperandError = true;
                        }
                    }
                    return null;
                }

                var node = new AstNode { Type = "B", Value = "%" };
                node.Children.Add(fNode);

                var bNode = ParseB();
                if (bNode != null) node.Children.Add(bNode);

                return node;
            }

            return null;
        }

        private AstNode ParseF()
        {
            SkipWhitespace();

            var node = new AstNode { Type = "F" };

            if (IsDigit(Current))
            {
                string num = ReadNumber();
                node.Value = num;
                node.NodeKind = "Number";
                return node;
            }
            else if (IsLetter(Current) || Current == '_')
            {
                string id = ReadIdentifier();
                node.Value = id;
                node.NodeKind = "Identifier";
                return node;
            }
            else if (Current == '(')
            {
                int parenPos = pos;
                pos++;
                currentCharPos++;

                SkipWhitespace();

                if (Current == ')')
                {
                    if (!hasEmptyParenthesesError && !hasMissingOperandError)
                    {
                        AddError(parenPos, "()", "Ожидается выражение в скобках");
                        hasEmptyParenthesesError = true;
                    }
                    pos++;
                    currentCharPos++;
                    return null;
                }

                int openingBracketPos = parenPos;

                bool previousInsideParentheses = isParsingInsideParentheses;
                isParsingInsideParentheses = true;

                var eNode = ParseE();

                isParsingInsideParentheses = previousInsideParentheses;

                if (eNode == null)
                {
                    if (!hasEmptyParenthesesError && !hasMissingOperandError)
                    {
                        AddError(parenPos, "(", "Ожидается выражение после '('");
                        if (!hasUnmatchedOpeningBracket)
                        {
                            hasUnmatchedOpeningBracket = true;
                            unmatchedOpeningBracketPos = openingBracketPos;
                        }
                    }
                    return null;
                }
                node.Children.Add(eNode);

                SkipWhitespace();

                if (Current != ')')
                {
                    if (!hasUnmatchedOpeningBracket)
                    {
                        AddError(parenPos, "(", "Непарная открывающая скобка: отсутствует соответствующая ')'");
                        hasUnmatchedOpeningBracket = true;
                        unmatchedOpeningBracketPos = openingBracketPos;
                    }
                    return null;
                }

                pos++;
                currentCharPos++;
                return node;
            }
            else if (Current == ')')
            {
                if (!hasMissingOperandError && !hasUnmatchedOpeningBracket)
                {
                    AddError(pos, ")", "Непарная закрывающая скобка: отсутствует соответствующая '('");
                    hasUnmatchedClosingBracket = true;
                }
                pos++;
                currentCharPos++;
                return ParseF();
            }
            else if (Current == '+' || Current == '-' || Current == '*' || Current == '/' || Current == '%')
            {
                string op = Current.ToString();
                if (Peek == '*') op = "**";
                if (Peek == '/') op = "//";

                if (!hasMissingOperandError && !hasUnmatchedOpeningBracket)
                {
                    AddError(pos, op, $"Ожидается операнд после '{op}'");
                    hasMissingOperandError = true;
                }
                pos++;
                currentCharPos++;
                if (op.Length == 2) { pos++; currentCharPos++; }
                return ParseF();
            }
            else if (char.IsWhiteSpace(Current))
            {
                SkipWhitespace();
                return ParseF();
            }
            else if (Current == '\0')
            {
                if (!hasMissingOperandError && !hasEmptyParenthesesError && !hasUnmatchedOpeningBracket)
                {
                    AddError(pos, "", "Неожиданный конец выражения");
                }
                return null;
            }
            else
            {
                AddError(pos, Current.ToString(), $"Недопустимый символ '{Current}'");
                pos++;
                currentCharPos++;
                return ParseF();
            }
        }

        private bool ContainsIdentifiers(AstNode node)
        {
            if (node == null) return false;
            if (node.NodeKind == "Identifier") return true;
            foreach (var child in node.Children)
                if (ContainsIdentifiers(child)) return true;
            return false;
        }

        private void GenerateTriples(AstNode node)
        {
            if (node == null) return;

            if (node.Type == "E")
            {
                var tNode = node.Children.FirstOrDefault(c => c.Type == "T");
                var aNode = node.Children.FirstOrDefault(c => c.Type == "A");

                string left = GenerateTriplesForTerm(tNode);
                if (aNode != null) GenerateTriplesForArith(aNode, left);
            }
            else
            {
                foreach (var child in node.Children)
                    GenerateTriples(child);
            }
        }

        private string GenerateTriplesForTerm(AstNode tNode)
        {
            if (tNode == null || tNode.Children.Count == 0) return null;

            var fNode = tNode.Children.FirstOrDefault(c => c.Type == "F");
            var bNode = tNode.Children.FirstOrDefault(c => c.Type == "B");

            string result = GenerateTriplesForFactor(fNode);
            if (bNode != null) result = GenerateTriplesForBinOp(bNode, result);
            return result;
        }

        private string GenerateTriplesForFactor(AstNode fNode)
        {
            if (fNode == null) return null;

            if (fNode.NodeKind == "Number" || fNode.NodeKind == "Identifier")
                return fNode.Value;

            if (fNode.Children.Count > 0)
            {
                var eNode = fNode.Children[0];
                var tNode = eNode.Children.FirstOrDefault(c => c.Type == "T");
                var aNode = eNode.Children.FirstOrDefault(c => c.Type == "A");

                string left = GenerateTriplesForTerm(tNode);
                if (aNode != null) left = GenerateTriplesForArith(aNode, left);
                return left;
            }
            return null;
        }

        private string GenerateTriplesForBinOp(AstNode bNode, string left)
        {
            if (bNode == null) return left;

            string op = bNode.Value;
            var fNode = bNode.Children.FirstOrDefault(c => c.Type == "F");
            string right = GenerateTriplesForFactor(fNode);

            string temp = $"t{tempVarCounter++}";
            triples.Add(new ArithmeticTriple(op, left, right, temp));

            var nextB = bNode.Children.Count > 1 ? bNode.Children[1] : null;
            if (nextB != null && nextB.Type == "B")
                return GenerateTriplesForBinOp(nextB, temp);
            return temp;
        }

        private string GenerateTriplesForArith(AstNode aNode, string left)
        {
            if (aNode == null) return left;

            string op = aNode.Value;
            var tNode = aNode.Children.FirstOrDefault(c => c.Type == "T");
            string right = GenerateTriplesForTerm(tNode);

            string temp = $"t{tempVarCounter++}";
            triples.Add(new ArithmeticTriple(op, left, right, temp));

            var nextA = aNode.Children.Count > 1 ? aNode.Children[1] : null;
            if (nextA != null && nextA.Type == "A")
                return GenerateTriplesForArith(nextA, temp);
            return temp;
        }

        private string BuildPolski(AstNode node)
        {
            if (node == null) return "";

            if (node.NodeKind == "Number" || node.NodeKind == "Identifier")
                return node.Value;

            if (node.Type == "E")
            {
                var tNode = node.Children.FirstOrDefault(c => c.Type == "T");
                var aNode = node.Children.FirstOrDefault(c => c.Type == "A");

                string tPol = BuildPolski(tNode);
                string aPol = BuildPolski(aNode);
                return string.IsNullOrEmpty(aPol) ? tPol : tPol + " " + aPol;
            }

            if (node.Type == "T")
            {
                var fNode = node.Children.FirstOrDefault(c => c.Type == "F");
                var bNode = node.Children.FirstOrDefault(c => c.Type == "B");

                string fPol = BuildPolski(fNode);
                string bPol = BuildPolski(bNode);
                return string.IsNullOrEmpty(bPol) ? fPol : fPol + " " + bPol;
            }

            if (node.Type == "A" && node.Value != null)
            {
                string op = node.Value;
                var tNode = node.Children.FirstOrDefault(c => c.Type == "T");
                var aNode = node.Children.Count > 1 ? node.Children[1] : null;

                string tPol = BuildPolski(tNode);
                string aPol = BuildPolski(aNode);
                return string.IsNullOrEmpty(aPol) ? tPol + " " + op : tPol + " " + aPol + " " + op;
            }

            if (node.Type == "B" && node.Value != null)
            {
                string op = node.Value;
                var fNode = node.Children.FirstOrDefault(c => c.Type == "F");
                var bNode = node.Children.Count > 1 ? node.Children[1] : null;

                string fPol = BuildPolski(fNode);
                string bPol = BuildPolski(bNode);
                return string.IsNullOrEmpty(bPol) ? fPol + " " + op : fPol + " " + bPol + " " + op;
            }

            if (node.Type == "F" && node.Children.Count > 0)
                return BuildPolski(node.Children[0]);

            return "";
        }

        private int? EvaluateAst(AstNode node)
        {
            if (node == null) return null;

            if (node.NodeKind == "Number")
                return int.Parse(node.Value);
            if (node.NodeKind == "Identifier")
                return null;

            if (node.Type == "E")
            {
                var tNode = node.Children.FirstOrDefault(c => c.Type == "T");
                var aNode = node.Children.FirstOrDefault(c => c.Type == "A");

                int? left = EvaluateAst(tNode);
                if (aNode != null)
                {
                    int? right = EvaluateArith(aNode, left);
                    return right ?? left;
                }
                return left;
            }

            if (node.Type == "T")
            {
                var fNode = node.Children.FirstOrDefault(c => c.Type == "F");
                var bNode = node.Children.FirstOrDefault(c => c.Type == "B");

                int? left = EvaluateAst(fNode);
                if (bNode != null)
                {
                    int? right = EvaluateBinOp(bNode, left);
                    return right ?? left;
                }
                return left;
            }

            if (node.Type == "F" && node.Children.Count > 0)
                return EvaluateAst(node.Children[0]);

            return null;
        }

        private int? EvaluateArith(AstNode aNode, int? left)
        {
            if (aNode == null || left == null) return left;

            var tNode = aNode.Children.FirstOrDefault(c => c.Type == "T");
            int? right = EvaluateAst(tNode);
            if (right == null) return null;

            int result = aNode.Value == "+" ? left.Value + right.Value : left.Value - right.Value;

            var nextA = aNode.Children.Count > 1 ? aNode.Children[1] : null;
            if (nextA != null)
                return EvaluateArith(nextA, result);
            return result;
        }

        private int? EvaluateBinOp(AstNode bNode, int? left)
        {
            if (bNode == null || left == null) return left;

            var fNode = bNode.Children.FirstOrDefault(c => c.Type == "F");
            int? right = EvaluateAst(fNode);
            if (right == null) return null;

            int result = 0;
            switch (bNode.Value)
            {
                case "*": result = left.Value * right.Value; break;
                case "/":
                    if (right.Value == 0) { AddError(0, "", "Деление на ноль"); return null; }
                    result = left.Value / right.Value; break;
                case "//":
                    if (right.Value == 0) { AddError(0, "", "Деление на ноль"); return null; }
                    result = left.Value / right.Value; break;
                case "%":
                    if (right.Value == 0) { AddError(0, "", "Деление на ноль"); return null; }
                    result = left.Value % right.Value; break;
                case "**": result = (int)Math.Pow(left.Value, right.Value); break;
            }

            var nextB = bNode.Children.Count > 1 ? bNode.Children[1] : null;
            if (nextB != null)
                return EvaluateBinOp(nextB, result);
            return result;
        }
    }
}