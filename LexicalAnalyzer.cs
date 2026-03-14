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
        public int Code { get; set; }              // Условный код
        public string Type { get; set; }            // Тип лексемы
        public string Value { get; set; }           // Значение лексемы
        public int Line { get; set; }               // Номер строки
        public int StartPos { get; set; }           // Начальная позиция
        public int EndPos { get; set; }             // Конечная позиция
        public bool IsError { get; set; }           // Флаг ошибки
        public string ErrorMessage { get; set; }    // Сообщение об ошибке
    }

    /// <summary>
    /// Лексический анализатор для объявления комплексного числа на Python
    /// </summary>
    public class LexicalAnalyzer
    {
        // Таблица типов лексем с кодами
        private readonly Dictionary<string, (int Code, string Type)> lexemeTypes = new Dictionary<string, (int, string)>()
        {
            // Числа
            { "INTEGER", (1, "целое без знака") },
            { "FLOAT", (2, "вещественное число") },
            
            // Идентификаторы и ключевые слова
            { "IDENTIFIER", (3, "идентификатор") },
            { "COMPLEX_KEYWORD", (4, "ключевое слово complex") },
            
            // Операторы и разделители
            { "ASSIGN", (10, "оператор присваивания") },
            { "LPAREN", (11, "разделитель (") },
            { "RPAREN", (12, "разделитель )") },
            { "COMMA", (13, "разделитель ,") },
            { "SEMICOLON", (14, "конец оператора") },
            { "SPACE", (15, "разделитель (пробел)") },
            
            // Ошибка
            { "ERROR", (99, "недопустимый символ") }
        };

        // Ключевые слова
        private readonly HashSet<string> keywords = new HashSet<string>()
        {
            "complex"
        };

        // Состояния конечного автомата
        private enum State
        {
            Start,
            InNumber,
            InFraction,
            InIdentifier,
            Error
        }

        /// <summary>
        /// Анализирует входной текст и возвращает список лексем
        /// </summary>
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

            // Добавляем символ перевода строки в конец для обработки последней лексемы
            string processedText = text + "\n";

            for (int i = 0; i < processedText.Length; i++)
            {
                char c = processedText[i];
                position = i - lineStartPos + 1; // Позиция в текущей строке (1-индексация)

                // Обработка перевода строки
                if (c == '\n')
                {
                    // Завершаем текущую лексему, если она есть
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

                // Основной автомат
                switch (currentState)
                {
                    case State.Start:
                        // Начало новой лексемы
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
                            // Пробелы тоже добавляем как лексемы (для наглядности)
                            lexemes.Add(CreateLexeme(lexemeTypes["SPACE"], "(пробел)", lineNumber, position, position));
                        }
                        else
                        {
                            // Недопустимый символ
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
                            // Завершаем число и обрабатываем текущий символ заново
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes);
                            currentLexeme = "";
                            currentState = State.Start;
                            i--; // Откатываем индекс для обработки текущего символа
                        }
                        break;

                    case State.InFraction:
                        if (char.IsDigit(c))
                        {
                            currentLexeme += c;
                        }
                        else
                        {
                            // Завершаем вещественное число
                            if (currentLexeme.EndsWith("."))
                            {
                                // Ошибка: число заканчивается точкой
                                lexemes.Add(CreateErrorLexeme(currentLexeme, lexemeStartLine, lexemeStartPos,
                                    $"Недопустимое число: '{currentLexeme}' должно содержать цифры после точки"));
                            }
                            else
                            {
                                ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes, isFloat: true);
                            }
                            currentLexeme = "";
                            currentState = State.Start;
                            i--; // Откатываем индекс
                        }
                        break;

                    case State.InIdentifier:
                        if (char.IsLetterOrDigit(c) || c == '_')
                        {
                            currentLexeme += c;
                        }
                        else
                        {
                            // Завершаем идентификатор
                            ProcessLexeme(currentLexeme, lexemeStartLine, lexemeStartPos, lineNumber, position - 1, lexemes, isIdentifier: true);
                            currentLexeme = "";
                            currentState = State.Start;
                            i--; // Откатываем индекс
                        }
                        break;
                }
            }

            return lexemes;
        }

        /// <summary>
        /// Обрабатывает накопленную лексему и добавляет её в список
        /// </summary>
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
                // Проверяем, не ключевое ли это слово
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
                // Целое число
                lexemes.Add(CreateLexeme(lexemeTypes["INTEGER"], lexeme, startLine, startPos, endPos));
            }
        }

        /// <summary>
        /// Создает лексему
        /// </summary>
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

        /// <summary>
        /// Создает лексему-ошибку
        /// </summary>
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
