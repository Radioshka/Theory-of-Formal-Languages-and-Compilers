using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GUIshka
{
    public partial class MainForm : Form
    {
        private class AnalysisIssue
        {
            public string Fragment { get; set; }
            public int Line { get; set; }
            public int Position { get; set; }
            public string Description { get; set; }
            public string Stage { get; set; }
        }

        private class ErrorSpan
        {
            public int Line { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
        }

        private string currentFilePath = string.Empty;
        private bool isTextModified = false;
        private bool isInternalTextUpdate = false;

        private LexicalAnalyzer lexicalAnalyzer;
        private SyntaxAnalyzer syntaxAnalyzer;
        private SemanticAnalyzer semanticAnalyzer;
        private SyntaxResult currentSyntaxResult;
        private List<AnalysisIssue> currentAnalysisIssues;

        public MainForm()
        {
            InitializeComponent();
            InitializeEventHandlers();

            lexicalAnalyzer = new LexicalAnalyzer();
            syntaxAnalyzer = new SyntaxAnalyzer();
            semanticAnalyzer = new SemanticAnalyzer();

            SetupDataGridView();
            UpdateFormTitleAndButtons();

            this.Resize += MainForm_Resize;
        }

        private void InitializeEventHandlers()
        {
            создатьToolStripMenuItem.Click += CreateNewDocument;
            открытьToolStripMenuItem.Click += OpenDocument;
            сохранитьToolStripMenuItem.Click += SaveDocument;
            сохранитьКакToolStripMenuItem.Click += SaveDocumentAs;
            выходToolStripMenuItem.Click += ExitApplication;

            отменитьToolStripMenuItem.Click += UndoLastAction;
            повторитьToolStripMenuItem.Click += RedoLastAction;
            вырезатьToolStripMenuItem.Click += CutText;
            копироватьToolStripMenuItem.Click += CopyText;
            вставитьToolStripMenuItem.Click += PasteText;
            удалитьToolStripMenuItem.Click += DeleteSelectedText;

            вызовСправкиToolStripMenuItem.Click += ShowHelp;
            оПрограммеToolStripMenuItem.Click += ShowAboutBox;

            CreateButton.Click += CreateNewDocument;
            OpenButton.Click += OpenDocument;
            SaveButton.Click += SaveDocument;
            BackButton.Click += UndoLastAction;
            ForwardButton.Click += RedoLastAction;
            CutButton.Click += CutText;
            CopyButton.Click += CopyText;
            InputButton.Click += PasteText;
            RefButton.Click += ShowHelp;
            button1.Click += ShowAboutBox;

            AnalisButton.Click += RunAnalysis;
            пускToolStripMenuItem.Click += RunAnalysis;

            richTextBox1.TextChanged += RichTextBox1_TextChanged;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!isInternalTextUpdate)
            {
                isTextModified = true;
            }

            UpdateFormTitleAndButtons();
        }

        private void SetupDataGridView()
        {
            ConfigureGridForSyntaxResults();
        }

        private void ConfigureGridForSyntaxResults()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowHeadersVisible = false;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Этап",
                DataPropertyName = "Stage",
                Width = 110
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Неверный фрагмент",
                DataPropertyName = "Fragment",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Местоположение",
                DataPropertyName = "Location",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Описание",
                DataPropertyName = "Description",
                Width = 400
            });
        }

        private void RunAnalysis(object sender, EventArgs e)
        {
            try
            {
                string text = richTextBox1.Text;

                ClearEditorHighlighting();
                dataGridView1.DataSource = null;
                richTextBoxAst.Clear();
                currentSyntaxResult = null;
                currentAnalysisIssues = null;

                ConfigureGridForSyntaxResults();

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show(
                        "Входной текст пуст. Введите строку для анализа.",
                        "Анализ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var lexemes = lexicalAnalyzer.Analyze(text);
                currentSyntaxResult = syntaxAnalyzer.Analyze(lexemes);
                SemanticResult semanticResult = semanticAnalyzer.Analyze(currentSyntaxResult.Ast);
                var allIssues = BuildCombinedIssues(lexemes, currentSyntaxResult, semanticResult);
                currentAnalysisIssues = allIssues;

                UpdateAstView(currentSyntaxResult);

                if (allIssues.Count == 0)
                {
                    dataGridView1.DataSource = null;

                    MessageBox.Show(
                        "Лексических, синтаксических и семантических ошибок не обнаружено.",
                        "Результат анализа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    DisplaySyntaxResults(allIssues);

                    MessageBox.Show(
                        $"Обнаружено ошибок: {allIssues.Count}",
                        "Результат анализа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при выполнении анализа: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateAstView(SyntaxResult syntaxResult)
        {
            if (syntaxResult?.Ast == null)
            {
                richTextBoxAst.Clear();
                return;
            }

            string tree = AstPrinter.ToTreeText(syntaxResult.Ast);
            string json = AstJson.ToJson(syntaxResult.Ast);
            richTextBoxAst.Text = tree + "\r\n\r\n--- JSON ---\r\n\r\n" + json;
        }

        private List<AnalysisIssue> BuildCombinedIssues(List<Lexeme> lexemes, SyntaxResult syntaxResult, SemanticResult semanticResult)
        {
            var issues = new List<AnalysisIssue>();
            var issueKeys = new HashSet<string>();
            var lexicalErrorSpans = new List<ErrorSpan>();

            var lexicalErrors = lexemes
                .Where(l => l != null && l.IsError)
                .OrderBy(l => l.Line)
                .ThenBy(l => l.StartPos)
                .ToList();

            int index = 0;
            while (index < lexicalErrors.Count)
            {
                var current = lexicalErrors[index];
                int line = current.Line;
                int start = current.StartPos;
                int end = current.EndPos;
                string fragment = current.Value ?? string.Empty;
                string description = current.ErrorMessage ?? "Лексическая ошибка";
                bool mixedMessages = false;

                index++;
                while (index < lexicalErrors.Count &&
                       lexicalErrors[index].Line == line &&
                       lexicalErrors[index].StartPos <= end)
                {
                    var next = lexicalErrors[index];
                    fragment += next.Value ?? string.Empty;
                    end = Math.Max(end, next.EndPos);
                    if (!string.Equals(description, next.ErrorMessage, StringComparison.Ordinal))
                    {
                        mixedMessages = true;
                    }
                    index++;
                }

                if (mixedMessages)
                {
                    description = "Недопустимая последовательность символов";
                }

                lexicalErrorSpans.Add(new ErrorSpan
                {
                    Line = line,
                    Start = start,
                    End = end
                });

                AddIssueDistinct(issues, issueKeys, new AnalysisIssue
                {
                    Fragment = fragment,
                    Line = line,
                    Position = start,
                    Description = description,
                    Stage = "Лексика"
                });
            }

            if (syntaxResult?.Errors != null)
            {
                foreach (var error in syntaxResult.Errors)
                {
                    if (error == null)
                    {
                        continue;
                    }

                    if (IsCoveredByLexicalError(error.Line, error.Position, lexicalErrorSpans))
                    {
                        continue;
                    }

                    AddIssueDistinct(issues, issueKeys, new AnalysisIssue
                    {
                        Fragment = error.Fragment ?? string.Empty,
                        Line = error.Line,
                        Position = error.Position,
                        Description = error.Description ?? "Синтаксическая ошибка",
                        Stage = "Синтаксис"
                    });
                }
            }

            if (semanticResult?.Errors != null)
            {
                foreach (SemanticError error in semanticResult.Errors)
                {
                    if (error == null)
                    {
                        continue;
                    }

                    AddIssueDistinct(issues, issueKeys, new AnalysisIssue
                    {
                        Fragment = error.Fragment ?? string.Empty,
                        Line = error.Line,
                        Position = error.Position,
                        Description = error.Description ?? "Семантическая ошибка",
                        Stage = "Семантика"
                    });
                }
            }

            issues.Sort((a, b) =>
            {
                int lineCompare = a.Line.CompareTo(b.Line);
                if (lineCompare != 0)
                {
                    return lineCompare;
                }

                int positionCompare = a.Position.CompareTo(b.Position);
                if (positionCompare != 0)
                {
                    return positionCompare;
                }

                int stageCompare = string.Compare(a.Stage, b.Stage, StringComparison.Ordinal);
                if (stageCompare != 0)
                {
                    return stageCompare;
                }

                return string.Compare(a.Fragment, b.Fragment, StringComparison.Ordinal);
            });

            return issues;
        }

        private void AddIssueDistinct(List<AnalysisIssue> issues, HashSet<string> issueKeys, AnalysisIssue issue)
        {
            string key = $"{issue.Stage}:{issue.Line}:{issue.Position}:{issue.Fragment}:{issue.Description}";
            if (issueKeys.Contains(key))
            {
                return;
            }

            issueKeys.Add(key);
            issues.Add(issue);
        }

        private bool IsCoveredByLexicalError(int line, int position, List<ErrorSpan> spans)
        {
            foreach (var span in spans)
            {
                if (span.Line == line && position >= span.Start && position <= span.End)
                {
                    return true;
                }
            }

            return false;
        }

        private void DisplaySyntaxResults(List<AnalysisIssue> issues)
        {
            var displayList = new List<dynamic>();

            foreach (var issue in issues)
            {
                string location = issue.Position > 0
                    ? $"строка {issue.Line}, символ {issue.Position}"
                    : $"строка {issue.Line}";

                displayList.Add(new
                {
                    issue.Stage,
                    issue.Fragment,
                    Location = location,
                    Description = issue.Description
                });
            }

            if (issues.Count > 0)
            {
                displayList.Add(new
                {
                    Stage = "",
                    Fragment = "",
                    Location = "Общее количество ошибок:",
                    Description = issues.Count.ToString()
                });
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayList;

            if (issues.Count > 0)
            {
                int lastRowIndex = dataGridView1.Rows.Count - 1;
                if (lastRowIndex >= 0)
                {
                    dataGridView1.Rows[lastRowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                    dataGridView1.Rows[lastRowIndex].DefaultCellStyle.Font =
                        new Font(dataGridView1.Font, FontStyle.Bold);
                }
            }

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (currentAnalysisIssues == null)
                return;

            if (e.RowIndex >= currentAnalysisIssues.Count)
                return;

            var selectedError = currentAnalysisIssues[e.RowIndex];

            if (selectedError.Line <= 0 || selectedError.Position <= 0)
                return;

            int charIndex = GetCharIndexFromPosition(selectedError.Line, selectedError.Position);
            if (charIndex < 0)
                return;

            ClearEditorHighlighting();

            richTextBox1.Focus();
            richTextBox1.SelectionStart = charIndex;

            int highlightLength = 1;
            if (!string.IsNullOrEmpty(selectedError.Fragment))
            {
                highlightLength = selectedError.Fragment.Length;
            }

            if (charIndex + highlightLength > richTextBox1.TextLength)
            {
                highlightLength = Math.Max(1, richTextBox1.TextLength - charIndex);
            }

            richTextBox1.SelectionLength = highlightLength;
            richTextBox1.SelectionBackColor = Color.Yellow;
            richTextBox1.ScrollToCaret();
        }

        private void ClearEditorHighlighting()
        {
            int savedStart = richTextBox1.SelectionStart;
            int savedLength = richTextBox1.SelectionLength;

            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = Color.White;

            richTextBox1.SelectionStart = savedStart;
            richTextBox1.SelectionLength = savedLength;
        }

        private int GetCharIndexFromPosition(int line, int position)
        {
            string text = richTextBox1.Text;
            int currentLine = 1;
            int lineStartIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (currentLine == line)
                {
                    int posInLine = i - lineStartIndex + 1;
                    if (posInLine == position)
                    {
                        return i;
                    }
                }

                if (text[i] == '\n')
                {
                    currentLine++;
                    lineStartIndex = i + 1;
                }
            }

            return -1;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateScrollBars();
        }

        private void UpdateScrollBars()
        {
            if (richTextBox1.Lines.Length > 0)
            {
                using (Graphics g = richTextBox1.CreateGraphics())
                {
                    int maxWidth = 0;
                    foreach (string line in richTextBox1.Lines)
                    {
                        int width = (int)g.MeasureString(line, richTextBox1.Font).Width;
                        if (width > maxWidth)
                            maxWidth = width;
                    }

                    richTextBox1.ScrollBars = maxWidth > richTextBox1.ClientSize.Width
                        ? RichTextBoxScrollBars.Both
                        : RichTextBoxScrollBars.Vertical;
                }
            }
            else
            {
                richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            }
        }

        private void UpdateFormTitleAndButtons()
        {
            string title = "Компилятор";

            if (!string.IsNullOrEmpty(currentFilePath))
            {
                title = Path.GetFileName(currentFilePath) + (isTextModified ? "*" : "") + " - " + title;
            }
            else
            {
                title = "Новый документ" + (isTextModified ? "*" : "") + " - " + title;
            }

            Text = title;

            UpdateUndoRedoButtons();

            отменитьToolStripMenuItem.Enabled = richTextBox1.CanUndo;
            повторитьToolStripMenuItem.Enabled = richTextBox1.CanRedo;

            CreateButton.Enabled = true;
            OpenButton.Enabled = true;
            SaveButton.Enabled = true;
            CutButton.Enabled = true;
            CopyButton.Enabled = true;
            InputButton.Enabled = true;
            AnalisButton.Enabled = true;
            RefButton.Enabled = true;
            button1.Enabled = true;
        }

        private void UpdateUndoRedoButtons()
        {
            bool canUndo = richTextBox1.CanUndo;
            bool canRedo = richTextBox1.CanRedo;

            BackButton.Enabled = canUndo;
            ForwardButton.Enabled = canRedo;
        }

        private bool PromptSaveIfModified()
        {
            if (!isTextModified)
                return true;

            DialogResult result = MessageBox.Show(
                "Сохранить изменения в файле?",
                "Компилятор, не онлайн",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return SaveDocumentLogic();

            if (result == DialogResult.No)
                return true;

            return false;
        }

        private void CreateNewDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            isInternalTextUpdate = true;
            richTextBox1.Clear();
            richTextBox1.ClearUndo();
            isInternalTextUpdate = false;

            currentFilePath = string.Empty;
            isTextModified = false;
            currentSyntaxResult = null;
            currentAnalysisIssues = null;
            dataGridView1.DataSource = null;
            richTextBoxAst.Clear();

            ClearEditorHighlighting();
            UpdateFormTitleAndButtons();
        }

        private void OpenDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Python файлы (*.py)|*.py|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(openFileDialog.FileName);

                        isInternalTextUpdate = true;
                        richTextBox1.Text = fileContent;
                        richTextBox1.ClearUndo();
                        isInternalTextUpdate = false;

                        currentFilePath = openFileDialog.FileName;
                        isTextModified = false;
                        currentSyntaxResult = null;
                        currentAnalysisIssues = null;
                        dataGridView1.DataSource = null;
                        richTextBoxAst.Clear();

                        ClearEditorHighlighting();
                        UpdateFormTitleAndButtons();
                        UpdateScrollBars();
                    }
                    catch (Exception ex)
                    {
                        isInternalTextUpdate = false;

                        MessageBox.Show(
                            $"Ошибка при открытии файла: {ex.Message}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool SaveDocumentLogic()
        {
            if (string.IsNullOrEmpty(currentFilePath))
                return SaveDocumentAsLogic();

            try
            {
                File.WriteAllText(currentFilePath, richTextBox1.Text);
                isTextModified = false;
                UpdateFormTitleAndButtons();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveDocument(object sender, EventArgs e)
        {
            SaveDocumentLogic();
        }

        private bool SaveDocumentAsLogic()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Python файлы (*.py)|*.py|Все файлы (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, richTextBox1.Text);
                        currentFilePath = saveFileDialog.FileName;
                        isTextModified = false;
                        UpdateFormTitleAndButtons();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при сохранении файла: {ex.Message}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            return false;
        }

        private void SaveDocumentAs(object sender, EventArgs e)
        {
            SaveDocumentAsLogic();
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            if (PromptSaveIfModified())
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!PromptSaveIfModified())
                {
                    e.Cancel = true;
                }
            }
        }

        private void UndoLastAction(object sender, EventArgs e)
        {
            if (richTextBox1.CanUndo)
            {
                string textBeforeUndo = richTextBox1.Text;
                int guard = 0;

                do
                {
                    richTextBox1.Undo();
                    guard++;
                }
                while (richTextBox1.CanUndo &&
                       richTextBox1.Text == textBeforeUndo &&
                       guard < 20);

                UpdateFormTitleAndButtons();
            }
            else
            {
                UpdateUndoRedoButtons();
            }
        }

        private void RedoLastAction(object sender, EventArgs e)
        {
            if (richTextBox1.CanRedo)
            {
                string textBeforeRedo = richTextBox1.Text;
                int guard = 0;

                do
                {
                    richTextBox1.Redo();
                    guard++;
                }
                while (richTextBox1.CanRedo &&
                       richTextBox1.Text == textBeforeRedo &&
                       guard < 20);

                UpdateFormTitleAndButtons();
            }
            else
            {
                UpdateUndoRedoButtons();
            }
        }

        private void CutText(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionLength > 0)
            {
                richTextBox1.Cut();
                UpdateFormTitleAndButtons();
            }
        }

        private void CopyText(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionLength > 0)
            {
                richTextBox1.Copy();
            }
        }

        private void PasteText(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                richTextBox1.Paste();
                UpdateFormTitleAndButtons();
            }
        }

        private void DeleteSelectedText(object sender, EventArgs e)
        {
            int selectionStart = richTextBox1.SelectionStart;
            int selectionLength = richTextBox1.SelectionLength;

            if (selectionLength > 0)
            {
                richTextBox1.Text = richTextBox1.Text.Remove(selectionStart, selectionLength);
                richTextBox1.SelectionStart = selectionStart;
                isTextModified = true;
                UpdateFormTitleAndButtons();
            }
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            string helpText =
                "ЛАБОРАТОРНАЯ РАБОТА №5: AST и семантический анализ\n\n" +
                "Вариант: объявление комплексного числа с инициализацией (синтаксис Python-подобный)\n\n" +
                "СИНТАКСИС ОПЕРАТОРА:\n" +
                "идентификатор = complex(операнд, операнд);\n" +
                "операнд — целый или вещественный литерал либо идентификатор ранее объявленной переменной.\n\n" +
                "СЕМАНТИКА ОПЕРАНДА-ИДЕНТИФИКАТОРА:\n" +
                "значение берётся как действительная часть (Real) ранее объявленного комплексного числа.\n\n" +
                "ПРАВИЛА:\n" +
                "1) Имя слева не должно повторяться в программе.\n" +
                "2) В аргументах complex допускаются только скалярные литералы; комплексные литералы (…j) недопустимы.\n" +
                "3) Целые литералы — в диапазоне Int32; вещественные — конечные double.\n" +
                "4) Идентификатор в аргументе должен быть объявлен выше по тексту.\n\n" +
                "ВЫВОД:\n" +
                "Вкладка «Ошибки» — лексика, синтаксис, семантика (столбец «Этап»). Формат позиции: строка N, символ M.\n" +
                "Вкладка «AST / JSON» — дерево узлов и JSON-представление.\n\n" +
                "ПРИМЕРЫ КОРРЕКТНЫХ СТРОК:\n" +
                "✓ z1 = complex(0, 2.5);\n" +
                "✓ x = complex(-5, 3.14);\n" +
                "✓ y = complex(1.5, -2.5);\n" +
                "✓ z1 = complex(1, 2); z2 = complex(z1, 0);\n\n" +
                "ПРИМЕРЫ С ОШИБКАМИ (синтаксис / лексика):\n" +
                "✗ z1 = complex(, 2.5);\n" +
                "✗ z1 = complex(2, );\n" +
                "✗ z1 = compex(2, 3);\n\n" +
                "НАВИГАЦИЯ ПО ТАБЛИЦЕ ОШИБОК:\n" +
                "Щёлкните строку — курсор перейдёт к фрагменту, он будет подсвечен.";

            MessageBox.Show(
                helpText,
                "Справка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowAboutBox(object sender, EventArgs e)
        {
            string aboutText =
                "Компилятор (лексика, синтаксис, AST, семантика)\n" +
                "Вариант: инициализация комплексного числа complex(re, im)\n\n" +
                "Версия: 5.0\n" +
                "© 2024–2026, GUIshka\n\n" +
                "Функционал:\n" +
                "✓ Лексический анализ\n" +
                "✓ Синтаксический анализ и построение AST\n" +
                "✓ Семантический анализ и таблица символов\n" +
                "✓ Вывод дерева и JSON, таблица ошибок с этапом и позицией\n" +
                "✓ Подсветка фрагментов в редакторе";

            MessageBox.Show(
                aboutText,
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
