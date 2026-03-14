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

            // Создаем экземпляр лексического анализатора
            lexicalAnalyzer = new LexicalAnalyzer();

            // Настраиваем DataGridView для отображения результатов
            SetupDataGridView();

            this.Resize += MainForm_Resize;
        }

        private void InitializeEventHandlers()
        {
            // Меню "Файл"
            this.создатьToolStripMenuItem.Click += CreateNewDocument;
            this.открытьToolStripMenuItem.Click += OpenDocument;
            this.сохранитьToolStripMenuItem.Click += SaveDocument;
            this.сохранитьКакToolStripMenuItem.Click += SaveDocumentAs;
            this.выходToolStripMenuItem.Click += ExitApplication;

            // Меню "Правка"
            this.отменитьToolStripMenuItem.Click += UndoLastAction;
            this.повторитьToolStripMenuItem.Click += RedoLastAction;
            this.вырезатьToolStripMenuItem.Click += CutText;
            this.копироватьToolStripMenuItem.Click += CopyText;
            this.вставитьToolStripMenuItem.Click += PasteText;
            this.удалитьToolStripMenuItem.Click += DeleteSelectedText;

            // Меню "Справка"
            this.вызовСправкиToolStripMenuItem.Click += ShowHelp;
            this.оПрограммеToolStripMenuItem.Click += ShowAboutBox;

            // Кнопки на панели инструментов
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

            // Кнопка "Пуск" для запуска анализатора
            this.AnalisButton.Click += RunLexicalAnalysis;
            this.пускToolStripMenuItem.Click += RunLexicalAnalysis;

            // Отслеживание изменений текста
            this.richTextBox1.TextChanged += (s, e) =>
            {
                isTextModified = true;
                UpdateFormTitleAndButtons();
            };

            // Обработка клика по DataGridView для навигации к ошибкам
            this.dataGridView1.CellClick += DataGridView1_CellClick;
        }

        /// <summary>
        /// Настройка DataGridView для отображения результатов анализа
        /// </summary>
        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            // Создаем колонки
            DataGridViewTextBoxColumn colCode = new DataGridViewTextBoxColumn();
            colCode.HeaderText = "Условный код";
            colCode.DataPropertyName = "Code";
            colCode.Width = 80;
            dataGridView1.Columns.Add(colCode);

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.HeaderText = "Тип лексемы";
            colType.DataPropertyName = "Type";
            colType.Width = 150;
            dataGridView1.Columns.Add(colType);

            DataGridViewTextBoxColumn colValue = new DataGridViewTextBoxColumn();
            colValue.HeaderText = "Лексема";
            colValue.DataPropertyName = "Value";
            colValue.Width = 100;
            dataGridView1.Columns.Add(colValue);

            DataGridViewTextBoxColumn colLocation = new DataGridViewTextBoxColumn();
            colLocation.HeaderText = "Местоположение";
            colLocation.DataPropertyName = "Location";
            colLocation.Width = 150;
            dataGridView1.Columns.Add(colLocation);

            // Колонка для сообщения об ошибке (скрытая, используется для подсветки)
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

        /// <summary>
        /// Запуск лексического анализа
        /// </summary>
        private void RunLexicalAnalysis(object sender, EventArgs e)
        {
            try
            {
                // Получаем текст из RichTextBox
                string text = richTextBox1.Text;

                // Запускаем анализ
                currentLexemes = lexicalAnalyzer.Analyze(text);

                // Отображаем результаты в DataGridView
                DisplayResults(currentLexemes);

                // Подсвечиваем строки с ошибками
                HighlightErrorRows();

                // Если есть ошибки, показываем сообщение
                int errorCount = currentLexemes.FindAll(l => l.IsError).Count;
                if (errorCount > 0)
                {
                    MessageBox.Show($"Обнаружено ошибок: {errorCount}. Щелкните на строке с ошибкой для перехода к проблемному месту.",
                        "Результат анализа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Отображение результатов в DataGridView
        /// </summary>
        private void DisplayResults(List<Lexeme> lexemes)
        {
            // Создаем список для привязки данных
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

            // Настраиваем цвета для строк
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.DataBoundItem != null)
                {
                    bool isError = (bool)row.DataBoundItem.GetType().GetProperty("IsError")?.GetValue(row.DataBoundItem);
                    if (isError)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.White;
                    }
                }
            }
        }

        /// <summary>
        /// Подсвечивает строки с ошибками
        /// </summary>
        private void HighlightErrorRows()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.DataBoundItem != null)
                {
                    bool isError = (bool)row.DataBoundItem.GetType().GetProperty("IsError")?.GetValue(row.DataBoundItem);
                    if (isError)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.White;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        /// <summary>
        /// Обработка клика по ячейке DataGridView
        /// </summary>
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && currentLexemes != null && e.RowIndex < currentLexemes.Count)
            {
                Lexeme selectedLexeme = currentLexemes[e.RowIndex];

                // Устанавливаем курсор в RichTextBox на позицию лексемы
                int charIndex = GetCharIndexFromPosition(selectedLexeme.Line, selectedLexeme.StartPos);

                if (charIndex >= 0)
                {
                    richTextBox1.Focus();
                    richTextBox1.SelectionStart = charIndex;
                    richTextBox1.SelectionLength = selectedLexeme.EndPos - selectedLexeme.StartPos + 1;
                    richTextBox1.ScrollToCaret();

                    // Если это ошибка, показываем сообщение
                    if (selectedLexeme.IsError)
                    {
                        MessageBox.Show(selectedLexeme.ErrorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Получает индекс символа в RichTextBox по строке и позиции
        /// </summary>
        private int GetCharIndexFromPosition(int line, int position)
        {
            string text = richTextBox1.Text;
            int currentLine = 1;
            int charIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (currentLine == line)
                {
                    // Нашли нужную строку, ищем позицию
                    int posInLine = i - charIndex + 1;
                    if (posInLine == position)
                    {
                        return i;
                    }
                }

                if (text[i] == '\n')
                {
                    currentLine++;
                    charIndex = i + 1;
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

        // ========== ФАЙЛ ==========

        private void CreateNewDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            richTextBox1.Clear();
            currentFilePath = string.Empty;
            isTextModified = false;
            dataGridView1.DataSource = null;
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
                        richTextBox1.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.PlainText);
                        currentFilePath = openFileDialog.FileName;
                        isTextModified = false;
                        UpdateFormTitleAndButtons();
                        UpdateScrollBars();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    richTextBox1.SaveFile(currentFilePath, RichTextBoxStreamType.PlainText);
                    isTextModified = false;
                    UpdateFormTitleAndButtons();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        richTextBox1.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.PlainText);
                        currentFilePath = saveFileDialog.FileName;
                        isTextModified = false;
                        UpdateFormTitleAndButtons();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // ========== ПРАВКА ==========

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
            richTextBox1.Cut();
        }

        private void CopyText(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void PasteText(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                richTextBox1.Paste();
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
            }
        }

        // ========== СПРАВКА ==========

        private void ShowHelp(object sender, EventArgs e)
        {
            string helpText = "ЛАБОРАТОРНАЯ РАБОТА №2: Лексический анализатор\n\n" +
                              "Вариант: Объявление комплексного числа с инициализацией на языке Python\n\n" +
                              "Пример корректного кода:\n" +
                              "z3 = complex(0, 2.5);\n\n" +
                              "Распознаваемые лексемы:\n" +
                              "• Идентификаторы (например, z3)\n" +
                              "• Ключевое слово complex\n" +
                              "• Числа: целые (0) и вещественные (2.5)\n" +
                              "• Операторы: = (присваивание)\n" +
                              "• Разделители: (, ), ,, ;\n\n" +
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
                               "Версия: 2.0\n" +
                               "© 2024, GUIshka\n\n" +
                               "Функционал:\n" +
                               "✓ Текстовый редактор с базовыми операциями\n" +
                               "✓ Лексический анализ кода\n" +
                               "✓ Подсветка ошибок\n" +
                               "✓ Навигация по ошибкам\n" +
                               "✓ Многострочная поддержка";

            MessageBox.Show(aboutText, "О программе",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
