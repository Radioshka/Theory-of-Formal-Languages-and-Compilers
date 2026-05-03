using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GUIshka
{
    public class Lexeme
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class LexicalAnalyzer
    {
        private readonly Dictionary<string, (int Code, string Type)> lexemeTypes = new Dictionary<string, (int, string)>()
        {
            { "INTEGER", (1, "целое без знака") },
            { "INTEGER_NEGATIVE", (2, "целое отрицательное") },
            { "FLOAT", (3, "вещественное число") },
            { "FLOAT_NEGATIVE", (4, "вещественное отрицательное") },
            { "COMPLEX", (7, "комплексное число") },
            { "COMPLEX_NEGATIVE", (8, "отрицательное комплексное число") },

            { "IDENTIFIER", (20, "идентификатор") },
            { "COMPLEX_KEYWORD", (21, "ключевое слово complex") },
            { "BOOLEAN_LITERAL", (22, "логический литерал") },
            { "STRING_LITERAL", (23, "строковый литерал") },

            { "ASSIGN", (30, "оператор присваивания") },
            { "LPAREN", (31, "разделитель (") },
            { "RPAREN", (32, "разделитель )") },
            { "COMMA", (33, "разделитель ,") },
            { "SEMICOLON", (34, "конец оператора") },
            { "SPACE", (35, "разделитель (пробел)") },
            { "PLUS", (36, "оператор сложения") },
            { "MINUS", (37, "оператор вычитания") },

            { "ERROR", (99, "недопустимый символ") }
        };

        private const string CORRECT_COMPLEX = "complex";

        private enum State
        {
            Start,
            InNumber,
            InFraction,
            InIdentifier,
            AfterMinus,
            InComplex,
            Error
        }

        public List<Lexeme> Analyze(string text)
        {
            List<Lexeme> lexemes = new List<Lexeme>();

            if (string.IsNullOrEmpty(text))
                return lexemes;

            int lineNumber = 1;
            int position = 0;
            int lineStartPos = 0;

            State currentState = State.Start;
            string currentLexeme = "";
            int lexemeStartLine = 1;
            int lexemeStartPos = 0;
            bool isNegative = false;
            bool hasDecimalPoint = false;

            string processedText = text + "\n";

            for (int i = 0; i < processedText.Length; i++)
            {
                char c = processedText[i];
                position = i - lineStartPos + 1;

                if (c == '\n')
                {
                    if (currentLexeme.Length > 0)
                    {
                        if (currentState == State.InFraction && currentLexeme.EndsWith("."))
                        {
                            lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                "Ожидалась цифра после десятичной точки"));
                        }
                        else
                        {
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1,
                                lexemes, isNegative, hasDecimalPoint, currentState == State.InComplex);
                        }

                        currentLexeme = "";
                        currentState = State.Start;
                        isNegative = false;
                        hasDecimalPoint = false;
                    }

                    lineNumber++;
                    lineStartPos = i + 1;
                    continue;
                }

                switch (currentState)
                {
                    case State.Start:
                        lexemeStartLine = lineNumber;
                        lexemeStartPos = position;

                        if (c == '"')
                        {
                            int stringStartLine = lineNumber;
                            int stringStartPos = position;
                            string stringValue = "\"";
                            bool closed = false;

                            int j = i + 1;

                            while (j < processedText.Length)
                            {
                                char sc = processedText[j];

                                if (sc == '\n')
                                {
                                    break;
                                }

                                stringValue += sc;

                                if (sc == '"')
                                {
                                    closed = true;
                                    break;
                                }

                                j++;
                            }

                            if (closed)
                            {
                                lexemes.Add(CreateLexeme(
                                    lexemeTypes["STRING_LITERAL"],
                                    stringValue,
                                    stringStartLine,
                                    stringStartPos,
                                    stringStartPos + stringValue.Length - 1));

                                i = j;
                            }
                            else
                            {
                                lexemes.Add(CreateErrorLexeme(
                                    stringValue,
                                    stringStartLine,
                                    stringStartPos,
                                    "Незакрытый строковый литерал"));

                                i = j - 1;
                            }
                        }
                        else if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                            currentState = State.InNumber;
                        }
                        else if (c == '-' && IsNextCharDigit(processedText, i + 1))
                        {
                            currentLexeme += c;
                            currentState = State.InNumber;
                            isNegative = true;
                        }
                        else if (c == '-')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["MINUS"], c.ToString(), lineNumber, position, position));
                        }
                        else if (char.IsLetter(c) || c == '_')
                        {
                            currentLexeme += c;
                            currentState = State.InIdentifier;
                        }
                        else if (c == '=')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["ASSIGN"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == '(')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["LPAREN"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == ')')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["RPAREN"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == ',')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["COMMA"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == ';')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["SEMICOLON"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == '+')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["PLUS"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX"], c.ToString(), lineNumber, position, position));
                        }
                        else if (c == '.')
                        {
                            lexemes.Add(CreateErrorLexeme(c.ToString(), lineNumber, position,
                                "Ожидалась цифра перед десятичной точкой"));
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["SPACE"], "(пробел)", lineNumber, position, position));
                        }
                        else
                        {
                            lexemes.Add(CreateErrorLexeme(c.ToString(), lineNumber, position, $"Недопустимый символ '{c}'"));
                        }
                        break;

                    case State.InNumber:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else if (char.IsLetter(c) || c == '_')
                        {
                            string invalidIdentifier = currentLexeme + c;

                            while (i + 1 < processedText.Length &&
                                   (char.IsLetterOrDigit(processedText[i + 1]) || processedText[i + 1] == '_'))
                            {
                                i++;
                                invalidIdentifier += processedText[i];
                            }

                            lexemes.Add(CreateErrorLexeme(
                                invalidIdentifier,
                                lexemeStartLine,
                                lexemeStartPos,
                                "Идентификатор не может начинаться с цифры"));

                            currentLexeme = "";
                            currentState = State.Start;
                            isNegative = false;
                            hasDecimalPoint = false;
                        }
                        else if (c == '.')
                        {
                            currentLexeme += c;
                            currentState = State.InFraction;
                            hasDecimalPoint = true;
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            currentLexeme += c;
                            currentState = State.InComplex;
                        }
                        else
                        {
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1,
                                lexemes, isNegative, hasDecimalPoint);

                            currentLexeme = "";
                            currentState = State.Start;
                            isNegative = false;
                            hasDecimalPoint = false;
                            i--;
                        }
                        break;

                    case State.InFraction:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            currentLexeme += c;
                            currentState = State.InComplex;
                        }
                        else
                        {
                            if (currentLexeme.EndsWith("."))
                            {
                                lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                    "Ожидалась цифра после десятичной точки"));
                            }
                            else
                            {
                                ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1,
                                    lexemes, isNegative, hasDecimalPoint);
                            }

                            currentLexeme = "";
                            currentState = State.Start;
                            isNegative = false;
                            hasDecimalPoint = false;
                            i--;
                        }
                        break;

                    case State.InComplex:
                        ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1,
                            lexemes, isNegative, hasDecimalPoint, true);

                        currentLexeme = "";
                        currentState = State.Start;
                        isNegative = false;
                        hasDecimalPoint = false;
                        i--;
                        break;

                    case State.InIdentifier:
                        if (char.IsLetterOrDigit(c) || c == '_')
                        {
                            currentLexeme += c;
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            ProcessIdentifier(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);

                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        else if (IsIdentifierErrorDelimiter(c))
                        {
                            ProcessIdentifier(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);

                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        else
                        {
                            ProcessIdentifier(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);

                            lexemes.Add(CreateErrorLexeme(c.ToString(), lineNumber, position, $"Недопустимый символ '{c}'"));

                            currentLexeme = "";
                            currentState = State.Start;
                        }
                        break;
                }
            }

            return lexemes;
        }

        private bool IsNextCharDigit(string text, int nextIndex)
        {
            if (nextIndex < text.Length)
            {
                return char.IsDigit(text[nextIndex]);
            }

            return false;
        }

        private bool IsIdentifierErrorDelimiter(char c)
        {
            return c == '=' || c == '(' || c == ')' || c == ',' || c == ';';
        }

        private void ProcessIdentifier(string lexeme, int startLine, int startPos, int endLine, int endPos,
            List<Lexeme> lexemes)
        {
            if (string.IsNullOrEmpty(lexeme))
                return;

            if (string.Equals(lexeme, CORRECT_COMPLEX, StringComparison.OrdinalIgnoreCase))
            {
                lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX_KEYWORD"], lexeme, startLine, startPos, endPos));
            }
            else if (string.Equals(lexeme, "true", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(lexeme, "false", StringComparison.OrdinalIgnoreCase))
            {
                lexemes.Add(CreateLexeme(lexemeTypes["BOOLEAN_LITERAL"], lexeme, startLine, startPos, endPos));
            }
            else
            {
                lexemes.Add(CreateLexeme(lexemeTypes["IDENTIFIER"], lexeme, startLine, startPos, endPos));
            }
        }

        private void ProcessLexeme(string lexeme, int startLine, int startPos, int endLine, int endPos,
            List<Lexeme> lexemes, bool isNegative = false, bool hasDecimalPoint = false,
            bool isComplex = false)
        {
            if (string.IsNullOrEmpty(lexeme))
                return;

            if (lexeme == "-" || lexeme == "+" || lexeme == ".")
            {
                lexemes.Add(CreateErrorLexeme(lexeme, startLine, startPos,
                    $"Ожидалось число, но найдено '{lexeme}'"));
                return;
            }

            if (isComplex)
            {
                if (isNegative || (lexeme.StartsWith("-") && lexeme.Length > 1))
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX_NEGATIVE"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX"], lexeme, startLine, startPos, endPos));
                }
            }
            else if (hasDecimalPoint)
            {
                if (isNegative || (lexeme.StartsWith("-") && lexeme.Length > 1))
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["FLOAT_NEGATIVE"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["FLOAT"], lexeme, startLine, startPos, endPos));
                }
            }
            else
            {
                if (isNegative || (lexeme.StartsWith("-") && lexeme.Length > 1))
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["INTEGER_NEGATIVE"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["INTEGER"], lexeme, startLine, startPos, endPos));
                }
            }
        }

        private Lexeme CreateLexeme((int Code, string Type) typeInfo, string value, int line, int startPos, int endPos)
        {
            return new Lexeme
            {
                Code = typeInfo.Code,
                Type = typeInfo.Type,
                Value = value,
                Line = line,
                StartPos = startPos,
                EndPos = endPos,
                IsError = false
            };
        }

        private Lexeme CreateErrorLexeme(string value, int line, int pos, string errorMessage)
        {
            return new Lexeme
            {
                Code = lexemeTypes["ERROR"].Code,
                Type = lexemeTypes["ERROR"].Type,
                Value = value,
                Line = line,
                StartPos = pos,
                EndPos = pos + value.Length - 1,
                IsError = true,
                ErrorMessage = errorMessage
            };
        }
    }
}