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
            { "INTEGER", (6, "целое без знака") },
            { "FLOAT", (7, "вещественное число") },
            { "NEGATIVE_INTEGER", (8, "целое отрицательное") },
            { "NEGATIVE_FLOAT", (9, "вещественное отрицательное") },
            { "EXPONENT_NUMBER", (10, "число с экспонентой") },
            { "COMPLEX_NUMBER", (11, "комплексное число") },
            
            { "IDENTIFIER", (1, "идентификатор") },
            { "COMPLEX_KEYWORD", (5, "ключевое слово complex") },
            { "INVALID_KEYWORD", (15, "неправильное ключевое слово") },
            
            { "ASSIGN", (3, "оператор присваивания") },
            { "LPAREN", (4, "разделитель (") },
            { "RPAREN", (12, "разделитель )") },
            { "COMMA", (13, "разделитель ,") },
            { "SEMICOLON", (14, "конец оператора") },
            { "SPACE", (2, "разделитель (пробел)") },
            { "PLUS", (16, "оператор сложения") },
            { "MINUS", (17, "оператор вычитания") },
            
            { "ERROR", (99, "недопустимый символ") }
        };

        private const string CORRECT_COMPLEX = "complex";

        private enum State
        {
            Start,
            InNumber,
            InFraction,
            InExponent,
            InExponentSign,
            InExponentNumber,
            InComplexJ,
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
            bool isNegativeNumber = false;

            string processedText = text + "\n";

            for (int i = 0; i < processedText.Length; i++)
            {
                char c = processedText[i];
                position = i - lineStartPos + 1;

                if (c == '\n')
                {
                    if (currentLexeme.Length > 0)
                    {
                        ProcessComplexLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                            lineNumber, position - 1, lexemes, currentState, isNegativeNumber);
                        currentLexeme = "";
                        currentState = State.Start;
                        isNegativeNumber = false;
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
                        else if (c == '-' && IsNextCharDigit(processedText, i + 1))
                        {
                            currentLexeme += c;
                            currentState = State.InNumber;
                            isNegativeNumber = true;
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
                        else if (c == '-')
                        {
                            lexemes.Add(CreateLexeme(lexemeTypes["MINUS"], c.ToString(), lineNumber, position, position));
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
                        else if (c == 'e' || c == 'E')
                        {
                            currentLexeme += c;
                            currentState = State.InExponent;
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            currentLexeme += c;
                            currentState = State.InComplexJ;
                        }
                        else
                        {
                            ProcessComplexLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                lineNumber, position - 1, lexemes, currentState, isNegativeNumber);
                            currentLexeme = "";
                            currentState = State.Start;
                            isNegativeNumber = false;
                            i--;
                        }
                        break;

                    case State.InFraction:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else if (c == 'e' || c == 'E')
                        {
                            currentLexeme += c;
                            currentState = State.InExponent;
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            currentLexeme += c;
                            currentState = State.InComplexJ;
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
                                ProcessComplexLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                    lineNumber, position - 1, lexemes, currentState, isNegativeNumber);
                            }
                            currentLexeme = "";
                            currentState = State.Start;
                            isNegativeNumber = false;
                            i--;
                        }
                        break;

                    case State.InExponent:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                            currentState = State.InExponentNumber;
                        }
                        else if (c == '+' || c == '-')
                        {
                            currentLexeme += c;
                            currentState = State.InExponentSign;
                        }
                        else
                        {
                            lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                $"Недопустимая запись числа с экспонентой: '{currentLexeme}'"));
                            currentLexeme = "";
                            currentState = State.Start;
                            isNegativeNumber = false;
                            i--;
                        }
                        break;

                    case State.InExponentSign:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                            currentState = State.InExponentNumber;
                        }
                        else
                        {
                            lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                $"Недопустимая запись числа с экспонентой: '{currentLexeme}'"));
                            currentLexeme = "";
                            currentState = State.Start;
                            isNegativeNumber = false;
                            i--; 
                        }
                        break;

                    case State.InExponentNumber:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else if (c == 'j' || c == 'J')
                        {
                            currentLexeme += c;
                            currentState = State.InComplexJ;
                        }
                        else
                        {
                            ProcessComplexLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                lineNumber, position - 1, lexemes, currentState, isNegativeNumber);
                            currentLexeme = "";
                            currentState = State.Start;
                            isNegativeNumber = false;
                            i--;
                        }
                        break;

                    case State.InComplexJ:
                        ProcessComplexLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                            lineNumber, position - 1, lexemes, currentState, isNegativeNumber);
                        currentLexeme = "";
                        currentState = State.Start;
                        isNegativeNumber = false;
                        i--;
                        break;

                    case State.InIdentifier:
                        if (char.IsLetterOrDigit(c) || c == '_')
                        {
                            currentLexeme += c;
                        }
                        else
                        {
                            ProcessIdentifier(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);
                            currentLexeme = "";
                            currentState = State.Start;
                            i--;
                        }
                        break;
                }
            }

            return lexemes;
        }

        private bool IsNextCharDigit(string text, int nextIndex)
        {
            return nextIndex < text.Length && char.IsDigit(text[nextIndex]);
        }

        private void ProcessComplexLexeme(string lexeme, int startLine, int startPos, int endLine, int endPos,
            List<Lexeme> lexemes, State state, bool isNegative)
        {
            if (string.IsNullOrEmpty(lexeme))
                return;

            if (state == State.InComplexJ)
            {
                lexemes.Add(CreateLexeme(lexemeTypes["COMPLEX_NUMBER"], lexeme, startLine, startPos, endPos));
            }
            else if (state == State.InExponentNumber || state == State.InExponent)
            {
                lexemes.Add(CreateLexeme(lexemeTypes["EXPONENT_NUMBER"], lexeme, startLine, startPos, endPos));
            }
            else if (lexeme.Contains("."))
            {
                if (isNegative)
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["NEGATIVE_FLOAT"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["FLOAT"], lexeme, startLine, startPos, endPos));
                }
            }
            else
            {
                if (isNegative)
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["NEGATIVE_INTEGER"], lexeme, startLine, startPos, endPos));
                }
                else
                {
                    lexemes.Add(CreateLexeme(lexemeTypes["INTEGER"], lexeme, startLine, startPos, endPos));
                }
            }
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
            else if (IsSimilarToComplex(lexeme))
            {
                lexemes.Add(CreateErrorLexeme(lexeme, startLine, startPos,
                    $"Неправильное написание ключевого слова: '{lexeme}'. Правильно: '{CORRECT_COMPLEX}'"));
            }
            else
            {
                lexemes.Add(CreateLexeme(lexemeTypes["IDENTIFIER"], lexeme, startLine, startPos, endPos));
            }
        }

        private bool IsSimilarToComplex(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 3)
                return false;

            word = word.ToLower();

            bool startsWithC = word.StartsWith("c");
            bool endsWithX = word.EndsWith("x");
            bool hasAppropriateLength = word.Length >= 5 && word.Length <= 8;

            bool hasCommonLetters = word.Contains("o") && word.Contains("m") && word.Contains("p");

            return (startsWithC && endsWithX && hasAppropriateLength) ||
                   (hasCommonLetters && hasAppropriateLength) ||
                   (word.Length == 7 && ComputeSimilarity(word, CORRECT_COMPLEX) > 0.5); 
        }

        private double ComputeSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            int maxLength = Math.Max(s1.Length, s2.Length);
            if (maxLength == 0)
                return 1;

            int distance = LevenshteinDistance(s1, s2);
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] dp = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[s1.Length, s2.Length];
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
