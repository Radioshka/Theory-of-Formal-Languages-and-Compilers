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
            { "FLOAT", (2, "вещественное число") },
            
            { "IDENTIFIER", (3, "идентификатор") },
            { "COMPLEX_KEYWORD", (4, "ключевое слово complex") },
            
            { "ASSIGN", (10, "оператор присваивания") },
            { "LPAREN", (11, "разделитель (") },
            { "RPAREN", (12, "разделитель )") },
            { "COMMA", (13, "разделитель ,") },
            { "SEMICOLON", (14, "конец оператора") },
            { "SPACE", (15, "разделитель (пробел)") },
            
            { "ERROR", (99, "недопустимый символ") }
        };

        private readonly HashSet<string> keywords = new HashSet<string>()
        {
            "complex"
        };

        private enum State
        {
            Start,
            InNumber,
            InFraction,
            InIdentifier,
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

            string processedText = text + "\n";

            for (int i = 0; i < processedText.Length; i++)
            {
                char c = processedText[i];
                position = i - lineStartPos + 1;

                if (c == '\n')
                {
                    if (currentLexeme.Length > 0)
                    {
                        ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);
                        currentLexeme = "";
                        currentState = State.Start;
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

                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                            currentState = State.InNumber;
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
                        else if (c == '.')
                        {
                            currentLexeme += c;
                            currentState = State.InFraction;
                        }
                        else
                        {
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);
                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        break;

                    case State.InFraction:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else
                        {
                            if (currentLexeme.EndsWith("."))
                            {
                                lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                    $"Недопустимое число: '{currentLexeme}' должно содержать цифры после точки"));
                            }
                            else
                            {
                                ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes, isFloat: true);
                            }
                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        break;

                    case State.InIdentifier:
                        if (char.IsLetterOrDigit(c) || c == '_')
                        {
                            currentLexeme += c;
                        }
                        else
                        {
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes, isIdentifier: true);
                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        break;
                }
            }

            return lexemes;
        }

        private void ProcessLexeme(string lexeme, int startLine, int startPos, int endLine, int endPos,
            List<Lexeme> lexemes, bool isFloat = false, bool isIdentifier = false)
        {
            if (string.IsNullOrEmpty(lexeme))
                return;

            if (isFloat)
            {
                lexemes.Add(CreateLexeme(lexemeTypes["FLOAT"], lexeme, startLine, startPos, endPos));
            }
            else if (isIdentifier)
            {
                if (keywords.Contains(lexeme))
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX_KEYWORD"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["IDENTIFIER"], lexeme, startLine, startPos, endPos));
                }
            }
            else
            {
                lexemes.Add(CreateLexeme(lexemeTypes["INTEGER"], lexeme, startLine, startPos, endPos));
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
                EndPos = pos,
                IsError = true,
                ErrorMessage = errorMessage
            };
        }
    }
}
