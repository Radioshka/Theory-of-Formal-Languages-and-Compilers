using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIshka
{
    public partial class MainForm : Form
    {
        private string currentFilePath = string.Empty;
        private bool isTextModified = false;
        private LexicalAnalyzer lexicalAnalyzer;
        private List<Lexeme> currentLexemes;

        public MainForm()
        {
            InitializeComponent();
            InitializeEventHandlers();
            UpdateFormTitleAndButtons();

            lexicalAnalyzer = new LexicalAnalyzer();

            SetupDataGridView();

            this.Resize += MainForm_Resize;
        }

        private void InitializeEventHandlers()
        {
            this.создатьToolStripMenuItem.Click += CreateNewDocument;
            this.открытьToolStripMenuItem.Click += OpenDocument;
            this.сохранитьToolStripMenuItem.Click += SaveDocument;
            this.сохранитьКакToolStripMenuItem.Click += SaveDocumentAs;
            this.выходToolStripMenuItem.Click += ExitApplication;

            this.отменитьToolStripMenuItem.Click += UndoLastAction;
            this.повторитьToolStripMenuItem.Click += RedoLastAction;
            this.вырезатьToolStripMenuItem.Click += CutText;
            this.копироватьToolStripMenuItem.Click += CopyText;
            this.вставитьToolStripMenuItem.Click += PasteText;
            this.удалитьToolStripMenuItem.Click += DeleteSelectedText;

            this.вызовСправкиToolStripMenuItem.Click += ShowHelp;
            this.оПрограммеToolStripMenuItem.Click += ShowAboutBox;

            this.CreateButton.Click += CreateNewDocument;
            this.OpenButton.Click += OpenDocument;
            this.SaveButton.Click += SaveDocument;
            this.BackButton.Click += UndoLastAction;
            this.ForwardButton.Click += RedoLastAction;
            this.CutButton.Click += CutText;
            this.CopyButton.Click += CopyText;
            this.InputButton.Click += PasteText;
            this.RefButton.Click += ShowHelp;
            this.button1.Click += ShowAboutBox;

            this.AnalisButton.Click += RunLexicalAnalysis;
            this.пускToolStripMenuItem.Click += RunLexicalAnalysis;

            this.richTextBox1.TextChanged += (s, e) =>
            {
                isTextModified = true;
                UpdateFormTitleAndButtons();
            };

            this.dataGridView1.CellClick += DataGridView1_CellClick;

            this.richTextBox1.KeyDown += RichTextBox1_KeyDown;
        }

        private void RichTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                richTextBox1.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                RunLexicalAnalysis(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowHeadersVisible = true; 

            DataGridViewTextBoxColumn colCode = new DataGridViewTextBoxColumn();
            colCode.HeaderText = "Условный код";
            colCode.DataPropertyName = "Code";
            colCode.Width = 90;
            dataGridView1.Columns.Add(colCode);

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.HeaderText = "Тип лексемы";
            colType.DataPropertyName = "Type";
            colType.Width = 180;
            dataGridView1.Columns.Add(colType);

            DataGridViewTextBoxColumn colValue = new DataGridViewTextBoxColumn();
            colValue.HeaderText = "Лексема";
            colValue.DataPropertyName = "Value";
            colValue.Width = 120;
            dataGridView1.Columns.Add(colValue);

            DataGridViewTextBoxColumn colLocation = new DataGridViewTextBoxColumn();
            colLocation.HeaderText = "Местоположение";
            colLocation.DataPropertyName = "Location";
            colLocation.Width = 150;
            dataGridView1.Columns.Add(colLocation);

            DataGridViewTextBoxColumn colError = new DataGridViewTextBoxColumn();
            colError.HeaderText = "Ошибка";
            colError.DataPropertyName = "ErrorMessage";
            colError.Visible = false;
            dataGridView1.Columns.Add(colError);

            DataGridViewCheckBoxColumn colIsError = new DataGridViewCheckBoxColumn();
            colIsError.HeaderText = "IsError";
            colIsError.DataPropertyName = "IsError";
            colIsError.Visible = false;
            dataGridView1.Columns.Add(colIsError);
        }

        private void RunLexicalAnalysis(object sender, EventArgs e)
        {
            try
            {
                string text = richTextBox1.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Введите текст для анализа.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                currentLexemes = lexicalAnalyzer.Analyze(text);

                DisplayResults(currentLexemes);

                HighlightErrorRows();

                int errorCount = currentLexemes.FindAll(l => l.IsError).Count;
                if (errorCount > 0)
                {
                    MessageBox.Show($"Обнаружено ошибок: {errorCount}. Щелкните на строке с ошибкой для перехода к проблемному месту.",
                        "Результат анализа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Анализ завершен успешно. Найдено лексем: {currentLexemes.Count}",
                        "Результат анализа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults(List<Lexeme> lexemes)
        {
            var displayList = new List<dynamic>();

            foreach (var lex in lexemes)
            {
                string location = $"строка {lex.Line}, {lex.StartPos}-{lex.EndPos}";

                displayList.Add(new
                {
                    lex.Code,
                    lex.Type,
                    lex.Value,
                    Location = location,
                    lex.ErrorMessage,
                    lex.IsError
                });
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayList;

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void HighlightErrorRows()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.DataBoundItem != null)
                {
                    bool isError = (bool)row.DataBoundItem.GetType().GetProperty("IsError")?.GetValue(row.DataBoundItem);
                    if (isError)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200); // Светло-красный
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                        row.DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Regular);
                    }
                }
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && currentLexemes != null && e.RowIndex < currentLexemes.Count)
            {
                Lexeme selectedLexeme = currentLexemes[e.RowIndex];

                int charIndex = GetCharIndexFromPosition(selectedLexeme.Line, selectedLexeme.StartPos);

                if (charIndex >= 0)
                {
                    richTextBox1.Focus();
                    richTextBox1.SelectionStart = charIndex;
                    richTextBox1.SelectionLength = selectedLexeme.EndPos - selectedLexeme.StartPos + 1;
                    richTextBox1.ScrollToCaret();

                    if (selectedLexeme.IsError)
                    {
                        richTextBox1.SelectionBackColor = Color.FromArgb(255, 200, 200);

                        MessageBox.Show(selectedLexeme.ErrorMessage, "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Timer timer = new Timer();
                        timer.Interval = 1000;
                        timer.Tick += (s, args) =>
                        {
                            richTextBox1.SelectionBackColor = Color.White;
                            timer.Stop();
                        };
                        timer.Start();
                    }
                }
            }
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

                    if (maxWidth > richTextBox1.ClientSize.Width)
                    {
                        richTextBox1.ScrollBars = RichTextBoxScrollBars.Both;
                    }
                    else
                    {
                        richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
                    }
                }
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
            this.Text = title;

            отменитьToolStripMenuItem.Enabled = richTextBox1.CanUndo;
            BackButton.Enabled = richTextBox1.CanUndo;
            повторитьToolStripMenuItem.Enabled = richTextBox1.CanRedo;
            ForwardButton.Enabled = richTextBox1.CanRedo;

            bool hasSelection = richTextBox1.SelectionLength > 0;
            вырезатьToolStripMenuItem.Enabled = hasSelection;
            CutButton.Enabled = hasSelection;
            копироватьToolStripMenuItem.Enabled = hasSelection;
            CopyButton.Enabled = hasSelection;
            удалитьToolStripMenuItem.Enabled = hasSelection;

            bool canPaste = Clipboard.ContainsText();
            вставитьToolStripMenuItem.Enabled = canPaste;
            InputButton.Enabled = canPaste;
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
            {
                return SaveDocumentLogic();
            }
            else if (result == DialogResult.No)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CreateNewDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            richTextBox1.Clear();
            currentFilePath = string.Empty;
            isTextModified = false;
            dataGridView1.DataSource = null;
            currentLexemes = null;
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
                openFileDialog.Title = "Открыть файл";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(openFileDialog.FileName);
                        richTextBox1.Text = fileContent;
                        currentFilePath = openFileDialog.FileName;
                        isTextModified = false;
                        UpdateFormTitleAndButtons();
                        UpdateScrollBars();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool SaveDocumentLogic()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return SaveDocumentAsLogic();
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFilePath, richTextBox1.Text);
                    isTextModified = false;
                    UpdateFormTitleAndButtons();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
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
                saveFileDialog.Title = "Сохранить файл как";

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
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                richTextBox1.Undo();
            }
            UpdateFormTitleAndButtons();
        }

        private void RedoLastAction(object sender, EventArgs e)
        {
            if (richTextBox1.CanRedo)
            {
                richTextBox1.Redo();
            }
            UpdateFormTitleAndButtons();
        }

        private void CutText(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionLength > 0)
            {
                richTextBox1.Cut();
            }
            UpdateFormTitleAndButtons();
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
            string helpText = "ЛАБОРАТОРНАЯ РАБОТА\n\n" +
                              "Вариант: Объявление комплексного числа с инициализацией на языке Python\n\n" +
                              "Пример корректного кода:\n" +
                              "z3 = complex(0, 2.5);\n\n" +
                              "РАСШИРЕННАЯ ОБРАБОТКА ЧИСЕЛ:\n\n" +
                              "✓ Целые числа:\n" +
                              "   123 \n" +
                              "   -456 \n\n" +
                              "✓ Вещественные числа:\n" +
                              "   3.14 \n" +
                              "   -2.5 \n\n" +
                              "✓ Числа с экспонентой:\n" +
                              "   1.5e-10 \n" +
                              "   -2.5E+3 \n" +
                              "   1e6 \n\n" +
                              "✓ Комплексные числа (Python-формат):\n" +
                              "   3+4j \n" +
                              "   -2-5j \n" +
                              "   1.5+2.5j \n" +
                              "   1e-3+4j \n\n" +
                              "ДРУГИЕ ЛЕКСЕМЫ:\n" +
                              "• Идентификаторы (код 20): z3, x, y\n" +
                              "• Ключевое слово complex (код 21)\n" +
                              "• Неправильное ключевое слово (код 22): komplex, compLex\n" +
                              "• Операторы: = (30), + (36), - (37), * (38), / (39)\n" +
                              "• Разделители: ( (31), ) (32), , (33), ; (34)\n" +
                              "• Пробел (35)\n\n" +
                              "ГОРЯЧИЕ КЛАВИШИ:\n" +
                              "• Ctrl+A - выделить все\n" +
                              "• Ctrl+F - запустить анализ\n" +
                              "• Ctrl+Z - отменить\n" +
                              "• Ctrl+Y - повторить\n" +
                              "• Ctrl+X - вырезать\n" +
                              "• Ctrl+C - копировать\n" +
                              "• Ctrl+V - вставить\n\n" +
                              "Навигация по ошибкам:\n" +
                              "• Щелкните на строке с ошибкой в таблице\n" +
                              "• Курсор автоматически перейдет к проблемному месту\n" +
                              "• Ошибки подсвечиваются красным в таблице";

            MessageBox.Show(helpText, "Справка - Лексический анализатор",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAboutBox(object sender, EventArgs e)
        {
            string aboutText = "Лексический анализатор для объявления комплексного числа на Python\n\n" +
                               "Функционал:\n" +
                               "✓ Текстовый редактор с базовыми операциями\n" +
                               "✓ Лексический анализ кода\n" +
                               "✓ Расширенная обработка чисел\n" +
                               "   - Целые и отрицательные целые\n" +
                               "   - Вещественные и отрицательные вещественные\n" +
                               "   - Числа с экспонентой\n" +
                               "   - Комплексные числа в Python-формате\n" +
                               "✓ Проверка правильности ключевого слова complex\n" +
                               "✓ Подсветка ошибок\n" +
                               "✓ Навигация по ошибкам\n" +
                               "✓ Многострочная поддержка\n" +
                               "✓ Горячие клавиши";

            MessageBox.Show(aboutText, "О программе",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearResults()
        {
            dataGridView1.DataSource = null;
            currentLexemes = null;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                if (PromptSaveIfModified())
                {
                    try
                    {
                        string fileContent = File.ReadAllText(files[0]);
                        richTextBox1.Text = fileContent;
                        currentFilePath = files[0];
                        isTextModified = false;
                        UpdateFormTitleAndButtons();
                        UpdateScrollBars();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
