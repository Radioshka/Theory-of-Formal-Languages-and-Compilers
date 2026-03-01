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

        public MainForm()
        {
            InitializeComponent();
            InitializeEventHandlers();
            UpdateFormTitleAndButtons();

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

            this.richTextBox1.TextChanged += (s, e) =>
            {
                isTextModified = true;
                UpdateFormTitleAndButtons();
            };
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
            string title = "Компилятор, не онлайн";
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


        private void CreateNewDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            richTextBox1.Clear();
            currentFilePath = string.Empty;
            isTextModified = false;
            UpdateFormTitleAndButtons();
        }

        private void OpenDocument(object sender, EventArgs e)
        {
            if (!PromptSaveIfModified())
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
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
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
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


        private void ShowHelp(object sender, EventArgs e)
        {
            string helpText = "Реализованные функции:\n\n" +
                              "Файл: Создать, Открыть, Сохранить, Сохранить как, Выход\n" +
                              "Правка: Отмена, Повтор, Вырезать, Копировать, Вставить, Удалить\n" +
                              "Интерфейс: Все элементы подстраиваются под размер окна\n" +
                              "Полосы прокрутки появляются автоматически\n\n" +
                              "Область результатов (таблица внизу) предназначена для будущего анализатора кода.";

            MessageBox.Show(helpText, "Справка - Руководство пользователя",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAboutBox(object sender, EventArgs e)
        {
            string aboutText = "Текстовый редактор с расширением до языкового процессора\n\n" +
                               "Версия: 1.0\n" +
                               "© 2026, GUIshka\n\n" +
                               "Программа является самодостаточной и не требует установки IDE или дополнительных библиотек.";

            MessageBox.Show(aboutText, "О программе",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
