using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ScintillaNET;

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

        private class DocumentTab
        {
            public Scintilla Editor { get; set; }
            public TabPage TabPage { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public bool IsModified { get; set; }
            public SyntaxResult SyntaxResult { get; set; }
            public List<AnalysisIssue> AnalysisIssues { get; set; }
        }

        private string currentFilePath = string.Empty;
        private bool isTextModified = false;
        private bool isInternalTextUpdate = false;

        private LexicalAnalyzer lexicalAnalyzer;
        private SyntaxAnalyzer syntaxAnalyzer;
        private SemanticAnalyzer semanticAnalyzer;
        private SyntaxResult currentSyntaxResult;
        private List<AnalysisIssue> currentAnalysisIssues;

        private List<DocumentTab> documentTabs = new List<DocumentTab>();
        private int currentTabIndex = -1;
        private string currentLanguage = "ru-RU";
        private Dictionary<string, Dictionary<string, string>> translations;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel cursorPositionLabel;
        private ToolStripStatusLabel fileInfoLabel;
        private float currentFontSize = 13.8f;
        private float currentResultFontSize = 10f;
        private StatusStrip statusStrip;
        private TabControl tabControlEditor;
        private TabControl tabControlResults;
        private SplitContainer mainSplitContainer;
        private bool isClosingTab = false;

        public MainForm()
        {
            InitializeComponent();

            LoadTranslations();
            ApplyLanguage();

            InitializeCustomComponents();
            InitializeEventHandlers();

            lexicalAnalyzer = new LexicalAnalyzer();
            syntaxAnalyzer = new SyntaxAnalyzer();
            semanticAnalyzer = new SemanticAnalyzer();

            SetupDataGridView();
            UpdateFormTitleAndButtons();

            this.Resize += MainForm_Resize;
        }

        private void InitializeCustomComponents()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Готов");
            cursorPositionLabel = new ToolStripStatusLabel("Строка: 1, Столбец: 1");
            fileInfoLabel = new ToolStripStatusLabel("Новый документ");

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(cursorPositionLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(fileInfoLabel);
            statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(statusStrip);

            tabControlEditor = new TabControl();
            tabControlEditor.Dock = DockStyle.Fill;
            tabControlEditor.TabIndex = 16;
            tabControlEditor.SelectedIndexChanged += TabControlEditor_SelectedIndexChanged;

            tabControlResults = new TabControl();
            tabControlResults.Dock = DockStyle.Fill;
            tabControlResults.TabIndex = 17;

            TabPage errorsPage = new TabPage("Ошибки");
            TabPage astPage = new TabPage("AST / JSON");
            TabPage diagnosticsPage = new TabPage("Диагностика");
            TabPage statisticsPage = new TabPage("Статистика");
            TabPage triplesPage = new TabPage("Тетрады");
            TabPage polskiPage = new TabPage("ПОЛИЗ / Вычисление");

            errorsPage.Controls.Add(dataGridView1);
            astPage.Controls.Add(richTextBoxAst);

            RichTextBox triplesTextBox = new RichTextBox();
            triplesTextBox.Dock = DockStyle.Fill;
            triplesTextBox.ReadOnly = true;
            triplesTextBox.Font = new Font("Consolas", currentResultFontSize);
            triplesTextBox.Tag = "triples";
            triplesPage.Controls.Add(triplesTextBox);

            RichTextBox polskiTextBox = new RichTextBox();
            polskiTextBox.Dock = DockStyle.Fill;
            polskiTextBox.ReadOnly = true;
            polskiTextBox.Font = new Font("Consolas", currentResultFontSize);
            polskiTextBox.Tag = "polski";
            polskiPage.Controls.Add(polskiTextBox);

            tabControlResults.TabPages.Add(errorsPage);
            tabControlResults.TabPages.Add(astPage);
            tabControlResults.TabPages.Add(diagnosticsPage);
            tabControlResults.TabPages.Add(statisticsPage);
            tabControlResults.TabPages.Add(triplesPage);
            tabControlResults.TabPages.Add(polskiPage);

            mainSplitContainer = new SplitContainer();
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Orientation = Orientation.Vertical;
            mainSplitContainer.SplitterDistance = 300;
            mainSplitContainer.Panel1.Controls.Add(tabControlEditor);
            mainSplitContainer.Panel2.Controls.Add(tabControlResults);

            if (tabControlMain != null)
                this.Controls.Remove(tabControlMain);
            this.Controls.Add(mainSplitContainer);
            mainSplitContainer.BringToFront();

            CreateNewTab();

            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;

            RichTextBox diagnosticsTextBox = new RichTextBox();
            diagnosticsTextBox.Dock = DockStyle.Fill;
            diagnosticsTextBox.ReadOnly = true;
            diagnosticsTextBox.Font = new Font("Consolas", currentResultFontSize);
            diagnosticsTextBox.Tag = "diagnostics";
            diagnosticsPage.Controls.Add(diagnosticsTextBox);

            RichTextBox statisticsTextBox = new RichTextBox();
            statisticsTextBox.Dock = DockStyle.Fill;
            statisticsTextBox.ReadOnly = true;
            statisticsTextBox.Font = new Font("Consolas", currentResultFontSize);
            statisticsTextBox.Tag = "statistics";
            statisticsPage.Controls.Add(statisticsTextBox);
        }

        private void SetupScintillaSyntax(Scintilla editor)
        {
            editor.Lexer = Lexer.Python;

            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = (int)currentFontSize;
            editor.Styles[Style.Default].BackColor = Color.White;
            editor.Styles[Style.Default].ForeColor = Color.Black;

            editor.StyleClearAll();

            editor.Styles[Style.Python.Word].ForeColor = Color.Blue;
            editor.Styles[Style.Python.Word].Bold = true;

            editor.Styles[Style.Python.Number].ForeColor = Color.Magenta;

            editor.Styles[Style.Python.String].ForeColor = Color.Green;
            editor.Styles[Style.Python.StringEol].ForeColor = Color.Green;

            editor.Styles[Style.Python.Operator].ForeColor = Color.Red;

            editor.SetKeywords(0, "complex int float double string bool if else for while return class public private void null true false");

            editor.IndentationGuides = IndentView.LookBoth;
            editor.TabWidth = 4;
            editor.UseTabs = false;
            editor.IndentWidth = 4;

            editor.Margins[0].Width = 40;
            editor.Margins[0].Type = MarginType.Number;

            editor.CaretLineVisible = true;
            editor.CaretLineBackColor = Color.FromArgb(240, 240, 240);

            editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click;
        }

        private void LoadTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();

            translations["ru-RU"] = new Dictionary<string, string>
            {
                {"ready", "Готов"},
                {"line", "Строка"},
                {"column", "Столбец"},
                {"new_document", "Новый документ"},
                {"file_menu", "Файл"},
                {"edit_menu", "Правка"},
                {"test_menu", "Текст"},
                {"analysis_menu", "Анализ"},
                {"help_menu", "Справка"},
                {"run_menu", "Пуск"},
                {"create", "Создать"},
                {"open", "Открыть"},
                {"save", "Сохранить"},
                {"save_as", "Сохранить как"},
                {"exit", "Выход"},
                {"undo", "Отменить"},
                {"redo", "Повторить"},
                {"cut", "Вырезать"},
                {"copy", "Копировать"},
                {"paste", "Вставить"},
                {"delete", "Удалить"},
                {"about", "О программе"},
                {"help", "Справка"},
                {"analysis", "Анализ"},
                {"task_statement", "Постановка задачи"},
                {"grammar", "Грамматика"},
                {"grammar_classification", "Классификация грамматики"},
                {"analysis_method", "Метод анализа"},
                {"test_example", "Тестовый пример"},
                {"literature", "Список литературы"},
                {"source_code", "Исходный код программы"},
                {"settings", "Настройки"},
                {"language", "Язык"},
                {"russian", "Русский"},
                {"english", "Английский"},
                {"no_errors", "Ошибок не обнаружено"},
                {"errors_found", "Обнаружено ошибок"},
                {"lexical", "Лексика"},
                {"syntax", "Синтаксис"},
                {"semantic", "Семантика"},
                {"errors_tab", "Ошибки"},
                {"ast_tab", "AST / JSON"},
                {"diagnostics_tab", "Диагностика"},
                {"statistics_tab", "Статистика"},
                {"triples_tab", "Тетрады"},
                {"polski_tab", "ПОЛИЗ / Вычисление"},
                {"editor_font_size", "Шрифт редактора"},
                {"result_font_size", "Шрифт результатов"},
                {"ok", "OK"},
                {"cancel", "Отмена"},
                {"save_changes_prompt", "Сохранить изменения?"},
                {"confirm", "Подтверждение"},
                {"empty_input", "Входной текст пуст"},
                {"analysis_result", "Результат анализа"},
                {"analysis_error", "Ошибка анализа"},
                {"error", "Ошибка"},
                {"save_error", "Ошибка сохранения"},
                {"saved", "Сохранено"},
                {"analyzing", "Выполняется анализ..."},
                {"font_size", "Размер шрифта"},
                {"fragment", "Фрагмент"},
                {"location", "Местоположение"},
                {"description", "Описание"},
                {"statistics", "Статистика"},
                {"save_all_changes", "Сохранить изменения в файле"},
                {"about_text", "Компилятор (лексика, синтаксис, AST, семантика)\nВерсия: 5.0\n© 2024-2026"},
                {"help_text", "Справка по использованию программы"},
                {"close", "Закрыть"},
                {"no_errors_message", "Лексических, синтаксических и семантических ошибок не обнаружено."},
                {"analysis_expr", "Арифметика"}
            };

            translations["en-US"] = new Dictionary<string, string>
            {
                {"ready", "Ready"},
                {"line", "Line"},
                {"column", "Col"},
                {"new_document", "New document"},
                {"file_menu", "File"},
                {"edit_menu", "Edit"},
                {"test_menu", "Test"},
                {"analysis_menu", "Analysis"},
                {"help_menu", "Help"},
                {"run_menu", "Run"},
                {"create", "Create"},
                {"open", "Open"},
                {"save", "Save"},
                {"save_as", "Save As"},
                {"exit", "Exit"},
                {"undo", "Undo"},
                {"redo", "Redo"},
                {"cut", "Cut"},
                {"copy", "Copy"},
                {"paste", "Paste"},
                {"delete", "Delete"},
                {"about", "About"},
                {"help", "Help"},
                {"analysis", "Analysis"},
                {"task_statement", "Task Statement"},
                {"grammar", "Grammar"},
                {"grammar_classification", "Grammar Classification"},
                {"analysis_method", "Analysis Method"},
                {"test_example", "Test Example"},
                {"literature", "Literature"},
                {"source_code", "Source Code"},
                {"settings", "Settings"},
                {"language", "Language"},
                {"russian", "Russian"},
                {"english", "English"},
                {"no_errors", "No errors found"},
                {"errors_found", "Errors found"},
                {"lexical", "Lexical"},
                {"syntax", "Syntax"},
                {"semantic", "Semantic"},
                {"errors_tab", "Errors"},
                {"ast_tab", "AST / JSON"},
                {"diagnostics_tab", "Diagnostics"},
                {"statistics_tab", "Statistics"},
                {"triples_tab", "Triples"},
                {"polski_tab", "Polish / Evaluation"},
                {"editor_font_size", "Editor font size"},
                {"result_font_size", "Result font size"},
                {"ok", "OK"},
                {"cancel", "Cancel"},
                {"save_changes_prompt", "Save changes?"},
                {"confirm", "Confirm"},
                {"empty_input", "Input text is empty"},
                {"analysis_result", "Analysis result"},
                {"analysis_error", "Analysis error"},
                {"error", "Error"},
                {"save_error", "Save error"},
                {"saved", "Saved"},
                {"analyzing", "Analyzing..."},
                {"font_size", "Font size"},
                {"fragment", "Fragment"},
                {"location", "Location"},
                {"description", "Description"},
                {"statistics", "Statistics"},
                {"save_all_changes", "Save changes in file"},
                {"about_text", "Compiler (lexical, syntax, AST, semantics)\nVersion: 5.0\n© 2024-2026"},
                {"help_text", "Help information"},
                {"close", "Close"},
                {"no_errors_message", "No lexical, syntax or semantic errors found."},
                {"analysis_expr", "Expression"}
            };
        }

        private string GetTranslation(string key)
        {
            if (translations == null) return key;
            if (translations.ContainsKey(currentLanguage) && translations[currentLanguage].ContainsKey(key))
                return translations[currentLanguage][key];
            if (currentLanguage != "ru-RU" && translations.ContainsKey("ru-RU") && translations["ru-RU"].ContainsKey(key))
                return translations["ru-RU"][key];
            return key;
        }

        private void ApplyLanguage()
        {
            if (translations == null) return;

            if (файлToolStripMenuItem != null) файлToolStripMenuItem.Text = GetTranslation("file_menu");
            if (правкаToolStripMenuItem != null) правкаToolStripMenuItem.Text = GetTranslation("edit_menu");
            if (текстToolStripMenuItem != null) текстToolStripMenuItem.Text = GetTranslation("test_menu");
            if (пускToolStripMenuItem != null) пускToolStripMenuItem.Text = GetTranslation("analysis_menu");
            if (справкаToolStripMenuItem != null) справкаToolStripMenuItem.Text = GetTranslation("help_menu");

            if (создатьToolStripMenuItem != null) создатьToolStripMenuItem.Text = GetTranslation("create");
            if (открытьToolStripMenuItem != null) открытьToolStripMenuItem.Text = GetTranslation("open");
            if (сохранитьToolStripMenuItem != null) сохранитьToolStripMenuItem.Text = GetTranslation("save");
            if (сохранитьКакToolStripMenuItem != null) сохранитьКакToolStripMenuItem.Text = GetTranslation("save_as");
            if (настройкиToolStripMenuItem != null) настройкиToolStripMenuItem.Text = GetTranslation("settings");
            if (выходToolStripMenuItem != null) выходToolStripMenuItem.Text = GetTranslation("exit");

            if (отменитьToolStripMenuItem != null) отменитьToolStripMenuItem.Text = GetTranslation("undo");
            if (повторитьToolStripMenuItem != null) повторитьToolStripMenuItem.Text = GetTranslation("redo");
            if (вырезатьToolStripMenuItem != null) вырезатьToolStripMenuItem.Text = GetTranslation("cut");
            if (копироватьToolStripMenuItem != null) копироватьToolStripMenuItem.Text = GetTranslation("copy");
            if (вставитьToolStripMenuItem != null) вставитьToolStripMenuItem.Text = GetTranslation("paste");
            if (удалитьToolStripMenuItem != null) удалитьToolStripMenuItem.Text = GetTranslation("delete");

            if (постановкаЗадачиToolStripMenuItem != null) постановкаЗадачиToolStripMenuItem.Text = GetTranslation("task_statement");
            if (грамматикаToolStripMenuItem != null) грамматикаToolStripMenuItem.Text = GetTranslation("grammar");
            if (классификацияГрамматикиToolStripMenuItem != null) классификацияГрамматикиToolStripMenuItem.Text = GetTranslation("grammar_classification");
            if (методАнализаToolStripMenuItem != null) методАнализаToolStripMenuItem.Text = GetTranslation("analysis_method");
            if (тестовыйПримерToolStripMenuItem != null) тестовыйПримерToolStripMenuItem.Text = GetTranslation("test_example");
            if (списокЛитературыToolStripMenuItem != null) списокЛитературыToolStripMenuItem.Text = GetTranslation("literature");
            if (исходныйКодПрограммыToolStripMenuItem != null) исходныйКодПрограммыToolStripMenuItem.Text = GetTranslation("source_code");

            if (вызовСправкиToolStripMenuItem != null) вызовСправкиToolStripMenuItem.Text = GetTranslation("help");
            if (языкToolStripMenuItem != null) языкToolStripMenuItem.Text = GetTranslation("language");
            if (русскийToolStripMenuItem != null) русскийToolStripMenuItem.Text = GetTranslation("russian");
            if (englishToolStripMenuItem != null) englishToolStripMenuItem.Text = GetTranslation("english");
            if (оПрограммеToolStripMenuItem != null) оПрограммеToolStripMenuItem.Text = GetTranslation("about");
            if (пускToolStripMenuItem != null) пускToolStripMenuItem.Text = GetTranslation("analysis");

            if (AnalisButton != null) AnalisButton.Text = GetTranslation("analysis");
            if (AnalisExprButton != null) AnalisExprButton.Text = GetTranslation("analysis_expr");

            if (tabControlResults != null && tabControlResults.TabPages.Count >= 6)
            {
                tabControlResults.TabPages[0].Text = GetTranslation("errors_tab");
                tabControlResults.TabPages[1].Text = GetTranslation("ast_tab");
                tabControlResults.TabPages[2].Text = GetTranslation("diagnostics_tab");
                tabControlResults.TabPages[3].Text = GetTranslation("statistics_tab");
                tabControlResults.TabPages[4].Text = GetTranslation("triples_tab");
                tabControlResults.TabPages[5].Text = GetTranslation("polski_tab");
            }

            if (statusLabel != null) statusLabel.Text = GetTranslation("ready");

            UpdateFormTitleAndButtons();
        }

        private void UpdateTabTitle(int index)
        {
            if (index < 0 || index >= documentTabs.Count) return;

            var tab = documentTabs[index];
            string title;

            if (!string.IsNullOrEmpty(tab.FileName))
            {
                title = tab.FileName + (tab.IsModified ? "*" : "");
            }
            else
            {
                title = $"{GetTranslation("new_document")} {index + 1}" + (tab.IsModified ? "*" : "");
            }

            if (tab.TabPage != null)
            {
                tab.TabPage.Text = title;
            }
        }

        private void CreateNewTab()
        {
            int tabNumber = documentTabs.Count + 1;
            var tabPage = new TabPage($"{GetTranslation("new_document")} {tabNumber}");

            var editor = new Scintilla();
            editor.Dock = DockStyle.Fill;
            editor.TextChanged += Editor_TextChanged;
            editor.UpdateUI += Editor_UpdateUI;
            editor.KeyDown += Editor_KeyDown;

            SetupScintillaSyntax(editor);

            tabPage.Controls.Add(editor);
            tabControlEditor.TabPages.Add(tabPage);

            documentTabs.Add(new DocumentTab
            {
                Editor = editor,
                TabPage = tabPage,
                FilePath = string.Empty,
                FileName = string.Empty,
                IsModified = false,
                SyntaxResult = null,
                AnalysisIssues = null
            });

            tabControlEditor.SelectedTab = tabPage;
            currentTabIndex = tabControlEditor.SelectedIndex;
        }

        private void CloseTab(int index)
        {
            if (isClosingTab) return;
            if (index < 0 || index >= documentTabs.Count) return;

            isClosingTab = true;

            try
            {
                var tab = documentTabs[index];
                if (tab.IsModified)
                {
                    var result = MessageBox.Show(
                        GetTranslation("save_changes_prompt"),
                        GetTranslation("confirm"),
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveDocumentLogic(tab);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                if (tab.Editor != null && !tab.Editor.IsDisposed)
                {
                    tab.Editor.TextChanged -= Editor_TextChanged;
                    tab.Editor.UpdateUI -= Editor_UpdateUI;
                    tab.Editor.KeyDown -= Editor_KeyDown;
                    tab.Editor.Dispose();
                }

                tabControlEditor.TabPages.Remove(tab.TabPage);
                documentTabs.RemoveAt(index);

                if (documentTabs.Count == 0)
                {
                    CreateNewTab();
                }

                if (index >= documentTabs.Count && documentTabs.Count > 0)
                {
                    currentTabIndex = documentTabs.Count - 1;
                    tabControlEditor.SelectedIndex = currentTabIndex;
                }
                else if (documentTabs.Count > 0)
                {
                    currentTabIndex = index;
                    if (currentTabIndex >= documentTabs.Count) currentTabIndex = documentTabs.Count - 1;
                    if (currentTabIndex >= 0)
                        tabControlEditor.SelectedIndex = currentTabIndex;
                }

                UpdateFormTitleAndButtons();
            }
            finally
            {
                isClosingTab = false;
            }
        }

        private void Editor_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            var editor = sender as Scintilla;
            if (editor != null && cursorPositionLabel != null && !editor.IsDisposed)
            {
                try
                {
                    int line = editor.CurrentLine + 1;
                    int column = editor.CurrentPosition - editor.Lines[editor.CurrentLine].Position + 1;
                    cursorPositionLabel.Text = $"{GetTranslation("line")}: {line}, {GetTranslation("column")}: {column}";
                }
                catch { }
            }
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Add)
            {
                IncreaseFontSize();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Subtract)
            {
                DecreaseFontSize();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.W)
            {
                CloseTab(currentTabIndex);
                e.Handled = true;
            }
        }

        private void IncreaseFontSize()
        {
            currentFontSize += 1;
            foreach (var tab in documentTabs)
            {
                if (tab.Editor != null && !tab.Editor.IsDisposed)
                {
                    tab.Editor.Styles[Style.Default].Size = (int)currentFontSize;
                    tab.Editor.StyleClearAll();
                    SetupScintillaSyntax(tab.Editor);
                }
            }
            if (statusLabel != null)
                statusLabel.Text = $"{GetTranslation("font_size")}: {currentFontSize}pt";
        }

        private void DecreaseFontSize()
        {
            if (currentFontSize > 8)
            {
                currentFontSize -= 1;
                foreach (var tab in documentTabs)
                {
                    if (tab.Editor != null && !tab.Editor.IsDisposed)
                    {
                        tab.Editor.Styles[Style.Default].Size = (int)currentFontSize;
                        tab.Editor.StyleClearAll();
                        SetupScintillaSyntax(tab.Editor);
                    }
                }
                if (statusLabel != null)
                    statusLabel.Text = $"{GetTranslation("font_size")}: {currentFontSize}pt";
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                OpenFileInNewTab(file);
            }
        }

        private void OpenFileInNewTab(string filePath)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);
                string fileName = Path.GetFileName(filePath);

                CreateNewTab();
                var currentEditor = GetCurrentEditor();
                if (currentEditor != null && !currentEditor.IsDisposed)
                {
                    currentEditor.Text = fileContent;
                }
                var tab = GetCurrentDocumentTab();
                if (tab != null)
                {
                    tab.FilePath = filePath;
                    tab.FileName = fileName;
                    tab.IsModified = false;
                    UpdateTabTitle(currentTabIndex);
                }

                if (fileInfoLabel != null)
                    fileInfoLabel.Text = fileName;
                UpdateFormTitleAndButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            var editor = sender as Scintilla;
            if (editor == null || editor.IsDisposed) return;

            if (!isInternalTextUpdate)
            {
                var tab = GetCurrentDocumentTab();
                if (tab != null)
                {
                    tab.IsModified = true;
                    UpdateTabTitle(currentTabIndex);
                }
                isTextModified = true;
            }
            UpdateFormTitleAndButtons();
        }

        private void TabControlEditor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isClosingTab) return;
            if (tabControlEditor == null) return;

            if (tabControlEditor.SelectedIndex >= 0 && tabControlEditor.SelectedIndex < documentTabs.Count)
            {
                currentTabIndex = tabControlEditor.SelectedIndex;
                var tab = documentTabs[currentTabIndex];

                if (tab != null)
                {
                    currentFilePath = tab.FilePath;
                    isTextModified = tab.IsModified;
                    currentSyntaxResult = tab.SyntaxResult;
                    currentAnalysisIssues = tab.AnalysisIssues;

                    UpdateFormTitleAndButtons();

                    if (currentAnalysisIssues != null && dataGridView1 != null)
                    {
                        DisplaySyntaxResults(currentAnalysisIssues);
                    }
                    else if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = null;
                    }

                    if (fileInfoLabel != null)
                    {
                        fileInfoLabel.Text = string.IsNullOrEmpty(tab.FileName) ?
                            GetTranslation("new_document") : tab.FileName;
                    }
                }
            }
        }

        private Scintilla GetCurrentEditor()
        {
            if (currentTabIndex >= 0 && currentTabIndex < documentTabs.Count)
                return documentTabs[currentTabIndex].Editor;
            return null;
        }

        private DocumentTab GetCurrentDocumentTab()
        {
            if (currentTabIndex >= 0 && currentTabIndex < documentTabs.Count)
                return documentTabs[currentTabIndex];
            return null;
        }

        private void SetupDataGridView()
        {
            ConfigureGridForSyntaxResults();
        }

        private void ConfigureGridForSyntaxResults()
        {
            if (dataGridView1 == null) return;

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
                HeaderText = GetTranslation("lexical"),
                DataPropertyName = "Stage",
                Width = 110
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = GetTranslation("fragment"),
                DataPropertyName = "Fragment",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = GetTranslation("location"),
                DataPropertyName = "Location",
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = GetTranslation("description"),
                DataPropertyName = "Description",
                Width = 400
            });
        }

        private void RunAnalysis(object sender, EventArgs e)
        {
            try
            {
                if (statusLabel != null)
                    statusLabel.Text = GetTranslation("analyzing");

                var currentEditor = GetCurrentEditor();
                if (currentEditor == null || currentEditor.IsDisposed) return;

                string text = currentEditor.Text;

                if (dataGridView1 != null)
                    dataGridView1.DataSource = null;
                if (richTextBoxAst != null)
                    richTextBoxAst.Clear();

                ClearTriplesAndPolskiTabs();

                currentSyntaxResult = null;
                currentAnalysisIssues = null;

                ConfigureGridForSyntaxResults();

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show(
                        GetTranslation("empty_input"),
                        GetTranslation("analysis"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    if (statusLabel != null)
                        statusLabel.Text = GetTranslation("ready");
                    return;
                }

                var lexemes = lexicalAnalyzer.Analyze(text);

                var lexicalErrors = lexemes.Where(l => l != null && l.IsError).ToList();
                if (lexicalErrors.Any())
                {
                    var allIssues = BuildCombinedIssues(lexemes, null, null);
                    currentAnalysisIssues = allIssues;
                    DisplaySyntaxResults(allIssues);
                    if (statusLabel != null)
                        statusLabel.Text = $"{GetTranslation("errors_found")}: {allIssues.Count}";
                    ClearTriplesAndPolskiTabs();
                    return;
                }

                currentSyntaxResult = syntaxAnalyzer.Analyze(lexemes);
                SemanticResult semanticResult = new SemanticResult();

                var allIssues2 = BuildCombinedIssues(lexemes, currentSyntaxResult, semanticResult);
                currentAnalysisIssues = allIssues2;

                var tab = GetCurrentDocumentTab();
                if (tab != null)
                {
                    tab.SyntaxResult = currentSyntaxResult;
                    tab.AnalysisIssues = currentAnalysisIssues;
                }

                UpdateAstView(currentSyntaxResult);
                UpdateStatistics(currentSyntaxResult, semanticResult, lexemes);
                UpdateDiagnostics(currentSyntaxResult, semanticResult, lexemes);

                if (allIssues2.Count == 0 && currentSyntaxResult != null && currentSyntaxResult.Success)
                {
                    if (dataGridView1 != null)
                        dataGridView1.DataSource = null;
                    if (statusLabel != null)
                        statusLabel.Text = GetTranslation("no_errors");
                    MessageBox.Show(
                        GetTranslation("no_errors_message"),
                        GetTranslation("analysis_result"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    DisplaySyntaxResults(allIssues2);
                    if (statusLabel != null)
                        statusLabel.Text = $"{GetTranslation("errors_found")}: {allIssues2.Count}";
                    MessageBox.Show(
                        $"{GetTranslation("errors_found")}: {allIssues2.Count}",
                        GetTranslation("analysis_result"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                if (statusLabel != null)
                    statusLabel.Text = GetTranslation("error");
                MessageBox.Show(
                    $"{GetTranslation("analysis_error")}: {ex.Message}",
                    GetTranslation("error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RunExpressionAnalysis(object sender, EventArgs e)
        {
            try
            {
                if (statusLabel != null)
                    statusLabel.Text = "Анализ арифметического выражения...";

                var currentEditor = GetCurrentEditor();
                if (currentEditor == null || currentEditor.IsDisposed) return;

                string fullText = currentEditor.Text;

                string[] lines = fullText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                {
                    MessageBox.Show(
                        "Введите арифметические выражения, каждое на новой строке, например:\n" +
                        "3 * 2\n6 / 3\n2 + 7\n1 - 3",
                        "Анализ выражения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (dataGridView1 != null)
                    dataGridView1.DataSource = null;
                if (richTextBoxAst != null)
                    richTextBoxAst.Clear();

                ClearTriplesAndPolskiTabs();

                var allTriples = new List<ArithmeticAnalyzer.ArithmeticTriple>();
                var allPolski = new List<string>();
                var allResults = new List<int?>();
                var allDetailedErrors = new List<AnalysisIssue>();
                bool hasAnyError = false;

                for (int lineNum = 0; lineNum < lines.Length; lineNum++)
                {
                    string text = lines[lineNum].Trim();
                    if (string.IsNullOrEmpty(text)) continue;

                    var analyzer = new ArithmeticAnalyzer();
                    var result = analyzer.Analyze(text);

                    if (result.Success)
                    {
                        allTriples.AddRange(result.Triples);
                        allPolski.Add($"{text} -> {result.Polski}");
                        allResults.Add(result.CalculatedValue);
                    }
                    else
                    {
                        hasAnyError = true;

                        foreach (var err in result.Errors)
                        {
                            string fragment = err.Fragment;
                            if (string.IsNullOrEmpty(fragment))
                            {
                                fragment = text.Length > 30 ? text.Substring(0, 30) + "..." : text;
                            }

                            allDetailedErrors.Add(new AnalysisIssue
                            {
                                Fragment = fragment,
                                Line = lineNum + 1,
                                Position = err.Position,
                                Description = err.Description,
                                Stage = "Синтаксис"
                            });
                        }
                    }
                }

                DisplayTriples(allTriples);
                DisplayPolski(allPolski, allResults, false);

                if (hasAnyError)
                {
                    currentAnalysisIssues = allDetailedErrors;
                    DisplaySyntaxResults(allDetailedErrors);
                    if (statusLabel != null)
                        statusLabel.Text = $"Ошибок: {allDetailedErrors.Count}";

                    MessageBox.Show(
                        $"Обнаружены ошибки в {allDetailedErrors.Count} месте(ах).\n\n" +
                        "Смотрите таблицу ошибок для деталей.",
                        "Ошибки анализа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    if (dataGridView1 != null)
                        dataGridView1.DataSource = null;
                    if (statusLabel != null)
                        statusLabel.Text = "Анализ завершен успешно";

                    string resultsMsg = "";
                    for (int i = 0; i < allPolski.Count; i++)
                    {
                        resultsMsg += $"{allPolski[i]}";
                        if (allResults[i].HasValue)
                            resultsMsg += $" = {allResults[i].Value}";
                        resultsMsg += "\n";
                    }

                    MessageBox.Show(
                        $"Анализ арифметических выражений выполнен успешно!\n\n" +
                        $"Обработано выражений: {allPolski.Count}\n" +
                        $"Тетрад сгенерировано: {allTriples.Count}\n\n" +
                        resultsMsg,
                        "Результат",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (statusLabel != null)
                    statusLabel.Text = "Ошибка";
                MessageBox.Show(
                    $"Ошибка при анализе выражения: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateStatistics(SyntaxResult syntaxResult, SemanticResult semanticResult, List<Lexeme> lexemes)
        {
            if (tabControlResults == null || tabControlResults.TabPages.Count < 4) return;

            var statisticsPage = tabControlResults.TabPages[3];
            RichTextBox statsTextBox = null;

            foreach (Control ctrl in statisticsPage.Controls)
            {
                if (ctrl is RichTextBox && (string)ctrl.Tag == "statistics")
                {
                    statsTextBox = ctrl as RichTextBox;
                    break;
                }
            }

            if (statsTextBox == null || statsTextBox.IsDisposed) return;

            statsTextBox.Text = $"=== {GetTranslation("statistics")} ===\n\n";
            statsTextBox.Text += $"Лексем: {lexemes.Count}\n";
            statsTextBox.Text += $"Корректных лексем: {lexemes.Count(l => !l.IsError)}\n";
            statsTextBox.Text += $"Ошибочных лексем: {lexemes.Count(l => l.IsError)}\n\n";

            if (syntaxResult != null)
            {
                statsTextBox.Text += $"=== {GetTranslation("syntax")} ===\n";
                statsTextBox.Text += $"Синтаксических ошибок: {syntaxResult.ErrorCount}\n\n";
            }

            if (semanticResult != null)
            {
                statsTextBox.Text += $"=== {GetTranslation("semantic")} ===\n";
                statsTextBox.Text += $"Семантических ошибок: {semanticResult.ErrorCount}\n";
            }
        }

        private void UpdateDiagnostics(SyntaxResult syntaxResult, SemanticResult semanticResult, List<Lexeme> lexemes)
        {
            if (tabControlResults == null || tabControlResults.TabPages.Count < 3) return;

            var diagnosticsPage = tabControlResults.TabPages[2];
            RichTextBox diagnosticsTextBox = null;

            foreach (Control ctrl in diagnosticsPage.Controls)
            {
                if (ctrl is RichTextBox && (string)ctrl.Tag == "diagnostics")
                {
                    diagnosticsTextBox = ctrl as RichTextBox;
                    break;
                }
            }

            if (diagnosticsTextBox == null || diagnosticsTextBox.IsDisposed) return;

            diagnosticsTextBox.Text = $"=== Диагностика ===\n\n";
            diagnosticsTextBox.Text += $"Время анализа: {DateTime.Now.ToString("HH:mm:ss")}\n";
            diagnosticsTextBox.Text += $"Размер входного текста: {GetCurrentEditor()?.Text.Length ?? 0} символов\n\n";

            diagnosticsTextBox.Text += $"=== Лексический анализ ===\n";
            diagnosticsTextBox.Text += $"Найдено лексем: {lexemes.Count}\n";
            diagnosticsTextBox.Text += $"Из них ошибок: {lexemes.Count(l => l.IsError)}\n\n";

            diagnosticsTextBox.Text += $"=== Синтаксический анализ ===\n";
            if (syntaxResult?.Errors != null && syntaxResult.Errors.Count > 0)
            {
                diagnosticsTextBox.Text += $"Синтаксических ошибок: {syntaxResult.Errors.Count}\n";
                foreach (var error in syntaxResult.Errors.Take(5))
                {
                    diagnosticsTextBox.Text += $"  - {error.Description}\n";
                }
                if (syntaxResult.Errors.Count > 5)
                    diagnosticsTextBox.Text += $"  ... и еще {syntaxResult.Errors.Count - 5}\n";
            }
            else
            {
                diagnosticsTextBox.Text += $"Синтаксических ошибок нет\n";
            }
            diagnosticsTextBox.Text += "\n";

            diagnosticsTextBox.Text += $"=== Семантический анализ ===\n";
            if (semanticResult?.Errors != null && semanticResult.Errors.Count > 0)
            {
                diagnosticsTextBox.Text += $"Семантических ошибок: {semanticResult.Errors.Count}\n";
                foreach (var error in semanticResult.Errors.Take(5))
                {
                    diagnosticsTextBox.Text += $"  - {error.Description}\n";
                }
            }
            else
            {
                diagnosticsTextBox.Text += $"Семантических ошибок нет\n";
            }
        }

        private void UpdateAstView(SyntaxResult syntaxResult)
        {
            if (richTextBoxAst == null || richTextBoxAst.IsDisposed) return;

            if (syntaxResult?.Ast == null)
            {
                richTextBoxAst.Clear();
                return;
            }

            string tree = AstPrinter.ToTreeText(syntaxResult.Ast);
            string json = AstJson.ToJson(syntaxResult.Ast);
            richTextBoxAst.Text = tree + "\r\n\r\n--- JSON ---\r\n\r\n" + json;
        }

        private List<AnalysisIssue> BuildCombinedIssues(
            List<Lexeme> lexemes,
            SyntaxResult syntaxResult,
            SemanticResult semanticResult)
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

                lexicalErrorSpans.Add(new ErrorSpan { Line = line, Start = start, End = end });

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
                    if (error == null) continue;
                    if (IsCoveredByLexicalError(error.Line, error.Position, lexicalErrorSpans))
                        continue;

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
                foreach (var error in semanticResult.Errors)
                {
                    if (error == null) continue;
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
                if (lineCompare != 0) return lineCompare;
                int positionCompare = a.Position.CompareTo(b.Position);
                if (positionCompare != 0) return positionCompare;
                return string.Compare(a.Fragment, b.Fragment, StringComparison.Ordinal);
            });

            return issues;
        }

        private void AddIssueDistinct(List<AnalysisIssue> issues, HashSet<string> issueKeys, AnalysisIssue issue)
        {
            string key = $"{issue.Stage}:{issue.Line}:{issue.Position}:{issue.Fragment}:{issue.Description}";
            if (issueKeys.Contains(key)) return;
            issueKeys.Add(key);
            issues.Add(issue);
        }

        private bool IsCoveredByLexicalError(int line, int position, List<ErrorSpan> spans)
        {
            foreach (var span in spans)
            {
                if (span.Line == line && position >= span.Start && position <= span.End)
                    return true;
            }
            return false;
        }

        private void DisplaySyntaxResults(List<AnalysisIssue> issues)
        {
            if (dataGridView1 == null) return;

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

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = displayList;
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || currentAnalysisIssues == null || e.RowIndex >= currentAnalysisIssues.Count) return;

            var selectedError = currentAnalysisIssues[e.RowIndex];
            if (selectedError.Line <= 0 || selectedError.Position <= 0) return;

            var editor = GetCurrentEditor();
            if (editor == null || editor.IsDisposed) return;

            int charIndex = GetCharIndexFromPosition(editor, selectedError.Line, selectedError.Position);
            if (charIndex < 0) return;

            editor.Focus();
            editor.SelectionStart = charIndex;
            int highlightLength = string.IsNullOrEmpty(selectedError.Fragment) ? 1 : selectedError.Fragment.Length;
            if (charIndex + highlightLength > editor.TextLength)
            {
                highlightLength = Math.Max(1, editor.TextLength - charIndex);
            }
            editor.SelectionEnd = charIndex + highlightLength;
            editor.ScrollCaret();

            editor.IndicatorCurrent = 0;
            editor.IndicatorValue = 1;
            editor.IndicatorFillRange(charIndex, highlightLength);

            Timer resetTimer = new Timer();
            resetTimer.Interval = 1500;
            resetTimer.Tick += (s, args) =>
            {
                if (!editor.IsDisposed)
                {
                    editor.IndicatorClearRange(charIndex, highlightLength);
                }
                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();
        }

        private int GetCharIndexFromPosition(Scintilla editor, int line, int position)
        {
            if (line < 1 || line > editor.Lines.Count) return -1;

            var targetLine = editor.Lines[line - 1];
            if (position < 1 || position > targetLine.Length + 1) return -1;

            return targetLine.Position + position - 1;
        }

        private void ClearTriplesAndPolskiTabs()
        {
            if (tabControlResults == null) return;

            for (int i = 4; i < tabControlResults.TabPages.Count && i < 6; i++)
            {
                foreach (Control ctrl in tabControlResults.TabPages[i].Controls)
                {
                    if (ctrl is RichTextBox rtb)
                    {
                        rtb.Clear();
                    }
                }
            }
        }

        private void ClearResultTabs()
        {
            if (tabControlResults == null) return;
            foreach (TabPage page in tabControlResults.TabPages)
            {
                foreach (Control ctrl in page.Controls)
                {
                    if (ctrl is RichTextBox rtb)
                    {
                        rtb.Clear();
                    }
                }
            }
        }

        private void DisplayTriples(List<ArithmeticAnalyzer.ArithmeticTriple> triples)
        {
            if (tabControlResults == null || tabControlResults.TabPages.Count < 5) return;

            var triplesPage = tabControlResults.TabPages[4];
            RichTextBox triplesTextBox = null;

            foreach (Control ctrl in triplesPage.Controls)
            {
                if (ctrl is RichTextBox rtb)
                {
                    triplesTextBox = rtb;
                    break;
                }
            }

            if (triplesTextBox == null || triplesTextBox.IsDisposed) return;

            if (triples == null || triples.Count == 0)
            {
                triplesTextBox.Text = "Нет тетрад для отображения";
                return;
            }

            triplesTextBox.Text = "========== ТЕТРАДЫ ==========\n\n";
            triplesTextBox.Text += "┌─────┬────────────┬────────────┬────────────┬────────────┐\n";
            triplesTextBox.Text += "│  №  │  Операция  │  Аргумент1 │  Аргумент2 │  Результат │\n";
            triplesTextBox.Text += "├─────┼────────────┼────────────┼────────────┼────────────┤\n";

            for (int i = 0; i < triples.Count; i++)
            {
                var t = triples[i];
                triplesTextBox.Text += $"│ {i + 1,3} │ {t.Op,-10} │ {t.Arg1,-10} │ {t.Arg2,-10} │ {t.Result,-10} │\n";
            }

            triplesTextBox.Text += "└─────┴────────────┴────────────┴────────────┴────────────┘\n";
        }

        private void DisplayPolski(List<string> polskiList, List<int?> results, bool hasIdentifiers)
        {
            if (tabControlResults == null || tabControlResults.TabPages.Count < 6) return;

            var polskiPage = tabControlResults.TabPages[5];
            RichTextBox polskiTextBox = null;

            foreach (Control ctrl in polskiPage.Controls)
            {
                if (ctrl is RichTextBox rtb)
                {
                    polskiTextBox = rtb;
                    break;
                }
            }

            if (polskiTextBox == null || polskiTextBox.IsDisposed) return;

            polskiTextBox.Text = "========== ПОЛИЗ (Польская инверсная запись) ==========\n\n";

            if (polskiList == null || polskiList.Count == 0)
            {
                polskiTextBox.Text += "Нет ПОЛИЗа для отображения";
                return;
            }

            for (int i = 0; i < polskiList.Count; i++)
            {
                polskiTextBox.Text += $"{polskiList[i]}";
                if (results != null && i < results.Count && results[i].HasValue)
                {
                    polskiTextBox.Text += $" = {results[i].Value}";
                }
                polskiTextBox.Text += "\n\n";
            }
        }

        private string ComputePolskiStepByStep(string polski)
        {
            var tokens = polski.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>();
            var result = new System.Text.StringBuilder();

            result.AppendLine("Шаг | Стек | Действие");
            result.AppendLine("----|------|---------");

            int step = 1;
            foreach (var token in tokens)
            {
                string stackStr = stack.Count > 0 ? string.Join(", ", stack) : "пусто";

                if (int.TryParse(token, out _))
                {
                    stack.Push(token);
                    result.AppendLine($"{step,3} | {stackStr,-20} | Поместить {token} в стек");
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2)
                    {
                        result.AppendLine($"{step,3} | {stackStr,-20} | ОШИБКА: недостаточно операндов");
                        break;
                    }
                    string b = stack.Pop();
                    string a = stack.Pop();
                    int res = 0;

                    switch (token)
                    {
                        case "+": res = int.Parse(a) + int.Parse(b); break;
                        case "-": res = int.Parse(a) - int.Parse(b); break;
                        case "*": res = int.Parse(a) * int.Parse(b); break;
                        case "/": res = int.Parse(a) / int.Parse(b); break;
                        case "//": res = int.Parse(a) / int.Parse(b); break;
                        case "%": res = int.Parse(a) % int.Parse(b); break;
                        case "**": res = (int)Math.Pow(int.Parse(a), int.Parse(b)); break;
                    }

                    stack.Push(res.ToString());
                    result.AppendLine($"{step,3} | {stackStr,-20} | {a} {token} {b} = {res}");
                }
                else
                {
                    stack.Push(token);
                    result.AppendLine($"{step,3} | {stackStr,-20} | Поместить {token} в стек (переменная)");
                }
                step++;
            }

            return result.ToString();
        }

        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" ||
                   token == "//" || token == "%" || token == "**";
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void UpdateFormTitleAndButtons()
        {
            var tab = GetCurrentDocumentTab();
            if (tab == null) return;

            string title = "Компилятор";
            if (!string.IsNullOrEmpty(tab.FileName))
            {
                title = tab.FileName + (tab.IsModified ? "*" : "") + " - " + title;
            }
            else
            {
                title = GetTranslation("new_document") + (tab.IsModified ? "*" : "") + " - " + title;
            }
            Text = title;

            bool canUndo = false;
            bool canRedo = false;

            if (tab.Editor != null && !tab.Editor.IsDisposed)
            {
                canUndo = tab.Editor.CanUndo;
                canRedo = tab.Editor.CanRedo;
            }

            if (отменитьToolStripMenuItem != null)
                отменитьToolStripMenuItem.Enabled = canUndo;
            if (повторитьToolStripMenuItem != null)
                повторитьToolStripMenuItem.Enabled = canRedo;
            if (BackButton != null)
                BackButton.Enabled = canUndo;
            if (ForwardButton != null)
                ForwardButton.Enabled = canRedo;
        }

        private void CreateNewDocument(object sender, EventArgs e)
        {
            CreateNewTab();
        }

        private void OpenDocument(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Python файлы (*.py)|*.py|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        OpenFileInNewTab(file);
                    }
                }
            }
        }

        private void SaveDocumentLogic(DocumentTab tab = null)
        {
            if (tab == null) tab = GetCurrentDocumentTab();
            if (tab == null || tab.Editor == null || tab.Editor.IsDisposed) return;

            if (string.IsNullOrEmpty(tab.FilePath))
            {
                SaveDocumentAsLogic(tab);
            }
            else
            {
                try
                {
                    File.WriteAllText(tab.FilePath, tab.Editor.Text);
                    tab.IsModified = false;
                    UpdateTabTitle(currentTabIndex);
                    if (tab == GetCurrentDocumentTab())
                    {
                        isTextModified = false;
                    }
                    UpdateFormTitleAndButtons();
                    if (statusLabel != null)
                        statusLabel.Text = GetTranslation("saved");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"{GetTranslation("save_error")}: {ex.Message}",
                        GetTranslation("error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void SaveDocumentAsLogic(DocumentTab tab = null)
        {
            if (tab == null) tab = GetCurrentDocumentTab();
            if (tab == null || tab.Editor == null || tab.Editor.IsDisposed) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Python файлы (*.py)|*.py|Все файлы (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string fileName = Path.GetFileName(saveFileDialog.FileName);
                        File.WriteAllText(saveFileDialog.FileName, tab.Editor.Text);
                        tab.FilePath = saveFileDialog.FileName;
                        tab.FileName = fileName;
                        tab.IsModified = false;
                        UpdateTabTitle(currentTabIndex);
                        if (tab == GetCurrentDocumentTab())
                        {
                            currentFilePath = saveFileDialog.FileName;
                            isTextModified = false;
                        }
                        UpdateFormTitleAndButtons();
                        if (statusLabel != null)
                            statusLabel.Text = GetTranslation("saved");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"{GetTranslation("save_error")}: {ex.Message}",
                            GetTranslation("error"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveDocument(object sender, EventArgs e)
        {
            SaveDocumentLogic();
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

        private bool PromptSaveIfModified()
        {
            var tab = GetCurrentDocumentTab();
            if (tab == null || !tab.IsModified) return true;

            DialogResult result = MessageBox.Show(
                GetTranslation("save_changes_prompt"),
                GetTranslation("confirm"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveDocumentLogic(tab);
                return true;
            }
            if (result == DialogResult.No) return true;
            return false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                foreach (var tab in documentTabs)
                {
                    if (tab.IsModified)
                    {
                        string name = string.IsNullOrEmpty(tab.FileName) ? "Новый документ" : tab.FileName;
                        var result = MessageBox.Show(
                            $"Сохранить изменения в файле {name}?",
                            GetTranslation("confirm"),
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            SaveDocumentLogic(tab);
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }
        }

        private void UndoLastAction(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && editor.CanUndo)
            {
                editor.Undo();
                UpdateFormTitleAndButtons();
            }
        }

        private void RedoLastAction(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && editor.CanRedo)
            {
                editor.Redo();
                UpdateFormTitleAndButtons();
            }
        }

        private void CutText(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && editor.SelectionStart != editor.SelectionEnd)
            {
                editor.Cut();
                UpdateFormTitleAndButtons();
            }
        }

        private void CopyText(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && editor.SelectionStart != editor.SelectionEnd)
            {
                editor.Copy();
            }
        }

        private void PasteText(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && Clipboard.ContainsText())
            {
                editor.Paste();
                UpdateFormTitleAndButtons();
            }
        }

        private void DeleteSelectedText(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed && editor.SelectionStart != editor.SelectionEnd)
            {
                editor.ReplaceSelection("");
                UpdateFormTitleAndButtons();
            }
        }

        private void ShowSettingsDialog()
        {
            Form settingsForm = new Form();
            settingsForm.Text = GetTranslation("settings");
            settingsForm.Size = new System.Drawing.Size(400, 280);
            settingsForm.StartPosition = FormStartPosition.CenterParent;
            settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 4;
            tableLayout.Padding = new Padding(10);

            tableLayout.Controls.Add(new Label() { Text = GetTranslation("language") + ":", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            ComboBox langCombo = new ComboBox();
            langCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            langCombo.Items.AddRange(new[] { "Русский", "English" });
            langCombo.SelectedIndex = currentLanguage == "ru-RU" ? 0 : 1;
            tableLayout.Controls.Add(langCombo, 1, 0);

            tableLayout.Controls.Add(new Label() { Text = GetTranslation("editor_font_size") + ":", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            NumericUpDown fontSizeEditor = new NumericUpDown();
            fontSizeEditor.Minimum = 8;
            fontSizeEditor.Maximum = 24;
            fontSizeEditor.Value = (decimal)currentFontSize;
            tableLayout.Controls.Add(fontSizeEditor, 1, 1);

            tableLayout.Controls.Add(new Label() { Text = GetTranslation("result_font_size") + ":", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            NumericUpDown fontSizeResult = new NumericUpDown();
            fontSizeResult.Minimum = 8;
            fontSizeResult.Maximum = 16;
            fontSizeResult.Value = (decimal)currentResultFontSize;
            tableLayout.Controls.Add(fontSizeResult, 1, 2);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;

            Button okButton = new Button() { Text = GetTranslation("ok"), Width = 80 };
            Button cancelButton = new Button() { Text = GetTranslation("cancel"), Width = 80 };

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);
            tableLayout.Controls.Add(buttonPanel, 1, 3);

            settingsForm.Controls.Add(tableLayout);

            okButton.Click += (s, args) =>
            {
                string newLang = langCombo.SelectedIndex == 0 ? "ru-RU" : "en-US";
                if (newLang != currentLanguage)
                {
                    currentLanguage = newLang;
                    ApplyLanguage();
                }

                currentFontSize = (float)fontSizeEditor.Value;
                currentResultFontSize = (float)fontSizeResult.Value;

                foreach (var tab in documentTabs)
                {
                    if (tab.Editor != null && !tab.Editor.IsDisposed)
                    {
                        tab.Editor.Styles[Style.Default].Size = (int)currentFontSize;
                        tab.Editor.StyleClearAll();
                        SetupScintillaSyntax(tab.Editor);
                    }
                }
                if (richTextBoxAst != null && !richTextBoxAst.IsDisposed)
                    richTextBoxAst.Font = new Font("Consolas", currentResultFontSize);

                if (tabControlResults != null)
                {
                    foreach (TabPage page in tabControlResults.TabPages)
                    {
                        foreach (Control ctrl in page.Controls)
                        {
                            if (ctrl is RichTextBox rtb && !rtb.IsDisposed)
                            {
                                rtb.Font = new Font("Consolas", currentResultFontSize);
                            }
                        }
                    }
                }

                settingsForm.Close();
            };

            cancelButton.Click += (s, args) => settingsForm.Close();

            settingsForm.ShowDialog();
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            string helpText = GetTranslation("help_text") +
                "\n\nГорячие клавиши:\n" +
                "Ctrl+N - Новый документ\n" +
                "Ctrl+O - Открыть файл\n" +
                "Ctrl+S - Сохранить\n" +
                "Ctrl+Z - Отменить\n" +
                "Ctrl+Y - Повторить\n" +
                "Ctrl+X - Вырезать\n" +
                "Ctrl+C - Копировать\n" +
                "Ctrl+V - Вставить\n" +
                "Ctrl+W - Закрыть вкладку\n" +
                "Ctrl+Tab - Следующая вкладка\n" +
                "F1 - Справка\n" +
                "F5 - Анализ (синтаксис)\n" +
                "Ctrl++ - Увеличить шрифт\n" +
                "Ctrl+- - Уменьшить шрифт\n\n" +
                "Кнопка 'Арифметика' - анализ арифметических выражений (тетрады, ПОЛИЗ, вычисление)";
            MessageBox.Show(helpText, GetTranslation("help"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAboutBox(object sender, EventArgs e)
        {
            string aboutText = GetTranslation("about_text") +
                "\n\nЛабораторная работа №6:\n" +
                "- Генерация тетрад\n" +
                "- Построение ПОЛИЗа\n" +
                "- Вычисление арифметических выражений";
            MessageBox.Show(aboutText, GetTranslation("about"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChangeLanguage(object sender, EventArgs e)
        {
            if (sender == русскийToolStripMenuItem)
            {
                currentLanguage = "ru-RU";
            }
            else if (sender == englishToolStripMenuItem)
            {
                currentLanguage = "en-US";
            }
            ApplyLanguage();
        }

        private void ShowPostanovkaZadachi(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Постановка задачи",
                "Комплексное число - это числовое значение, состоящее из двух частей: " +
                "действительной и мнимой. В языке Python для создания комплексного числа " +
                "может использоваться встроенная функция complex(), принимающая два " +
                "аргумента: первый аргумент задаёт действительную часть числа, второй - " +
                "мнимую часть.\n\n" +
                "В рамках данной работы рассматривается упрощённая конструкция объявления " +
                "комплексного числа с инициализацией. Анализируемая строка имеет " +
                "следующий общий формат: идентификатор = complex (операнд1, операнд2);\n\n" +
                "В связи с разработанной автоматной грамматикой G[‹START›] " +
                "синтаксический анализатор (парсер) объявления комплексного числа будет " +
                "считать верными следующие записи:\n" +
                "1. z1 = complex (1, 2.3);\n" +
                "2. x = complex (-5, 3.14);\n" +
                "3. y = complex (1.5, -2.5);\n" +
                "4. z1 = complex (1, 2);\n\n" +
                "Справка (руководство пользователя) представлена в Приложении А. " +
                "Информация о программе представлена в Приложении Б.");
        }

        private void ShowGrammar(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Грамматика",
                "Определим грамматику объявлений строковых констант языка Python " +
                "G[‹START›] в нотации Хомского с продукциями P:\n\n" +
                "1. ‹START› -> letter ‹ID›\n" +
                "2. ‹ID› -> letter ‹ID› | digit ‹ID› | '_' ‹ID› | '=' ‹EQUALS›\n" +
                "3. ‹EQUALS› -> 'complex' ‹LPAREN›\n" +
                "4. ‹LPAREN› -> '(' ‹OPERAND1›\n" +
                "5. ‹OPERAND1› -> '-' ‹INT1› | digit ‹INTREM1›\n" +
                "6. ‹INT1› -> digit ‹INTREM1›\n" +
                "7. ‹INTREM1› -> digit ‹INTREM1› | ',' ‹OPERAND2› | '.' ‹FLOAT1›\n" +
                "8. ‹FLOAT1› -> digit ‹FLOATREM1›\n" +
                "9. ‹FLOATREM1› -> digit ‹FLOATREM1› | ',' ‹OPERAND2›\n" +
                "10. ‹OPERAND2› -> '-' ‹INT2› | digit ‹INTREM2›\n" +
                "11. ‹INT2› -> digit ‹INTREM2›\n" +
                "12. ‹INTREM2› -> digit ‹INTREM2› | ')' ‹RPAREN› | '.' ‹FLOAT2›\n" +
                "13. ‹FLOAT2› -> digit ‹FLOATREM2› | ')' ‹RPAREN›\n" +
                "14. ‹RPAREN› -> ')' ‹SEMICOLON›\n" +
                "15. ‹SEMICOLON› -> ';'\n\n" +
                "Следуя введенному формальному определению грамматики, представим " +
                "G[‹START›] ее составляющими:\n" +
                "- Z = ‹START›;\n" +
                "- Vt = {a....z, A....Z, 0....9, =, (, ), ,, ;, -}\n" +
                "- Vn = {‹ID›, ‹EQUALS›, ‹COMPLEX›, ‹LPAREN›, ‹OPERAND1›, ‹OPERAND2›, " +
                "‹RPAREN›, ‹SEMICOLON›, ‹INT1›, ‹INTREM1›, ‹FLOAT1›, ‹FLOATREM1›, ‹INT2›, " +
                "‹INTREM2›, ‹FLOAT2›, ‹FLOATREM2›}");
        }

        private void ShowClassification(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Классификация грамматики",
                "Согласно классификации Хомского, грамматика G[‹START›] является автоматной.\n\n" +
                "Правила (1) - (15) относятся к классу праворекурсивных продукций (A → aB | a | ε):\n\n" +
                "1. ‹START› -> letter ‹ID›\n" +
                "2. ‹ID› -> letter ‹ID› | digit ‹ID› | '_' ‹ID› | '=' ‹EQUALS›\n" +
                "3. ‹EQUALS› -> 'complex' ‹LPAREN›\n" +
                "4. ‹LPAREN› -> '(' ‹OPERAND1›\n" +
                "5. ‹OPERAND1› -> '-' ‹INT1› | digit ‹INTREM1›\n" +
                "6. ‹INT1› -> digit ‹INTREM1›\n" +
                "7. ‹INTREM1› -> digit ‹INTREM1› | ',' ‹OPERAND2› | '.' ‹FLOAT1›\n" +
                "8. ‹FLOAT1› -> digit ‹FLOATREM1›\n" +
                "9. ‹FLOATREM1› -> digit ‹FLOATREM1› | ',' ‹OPERAND2›\n" +
                "10. ‹OPERAND2› -> '-' ‹INT2› | digit ‹INTREM2›\n" +
                "11. ‹INT2› -> digit ‹INTREM2›\n" +
                "12. ‹INTREM2› -> digit ‹INTREM2› | ')' ‹RPAREN› | '.' ‹FLOAT2›\n" +
                "13. ‹FLOAT2› -> digit ‹FLOATREM2› | ')' ‹RPAREN›\n" +
                "14. ‹RPAREN› -> ')' ‹SEMICOLON›\n" +
                "15. ‹SEMICOLON› -> ';'");
        }

        private void ShowTestMethod(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Метод анализа",
                "Грамматика G[‹START›] является автоматной.\n" +
                "Правила (1) - (15) для G[‹START›] реализованы на графе (см. рисунок 1).\n" +
                "Сплошные стрелки на графе характеризуют синтаксически верный разбор " +
                "объявлений комплексных чисел языка Python.\n" +
                "Конечное состояние автомата символизирует успешное завершение разбора конструкции.\n\n" +
                "[Рис. 1 -- Граф G[‹START›]]");
        }

        private void SetTestExample(object sender, EventArgs e)
        {
            var editor = GetCurrentEditor();
            if (editor != null && !editor.IsDisposed)
            {
                editor.Text = "z1 = complex (1, 2.3);";
                isTextModified = true;
                UpdateFormTitleAndButtons();
            }
        }

        private void ShowLiterature(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Список литературы",
                "1. Шорников Ю.В. Теория и практика языковых процессоров: учеб. пособие / " +
                "Ю.В. Шорников. -- Новосибирск: Изд-во НГТУ, 2022.\n\n" +
                "2. Python documentation: official website. - URL: " +
                "https://docs.python.org/3/library/cmath.html (дата обращения: 08.04.2026). " +
                "- Текст: электронный.\n\n" +
                "3. Теория формальных языков и компиляторов [Электронный ресурс] / " +
                "Электрон. дан. URL: https://dispace.edu.nstu.ru/didesk/course/show/8594, " +
                "свободный. Яз.рус. (дата обращения 10.04.2026).");
        }

        private void ShowSourceCode(object sender, EventArgs e)
        {
            ShowScrollableMessageBox("Исходный код программы",
                "Листинг программной части разработанного синтаксического анализатора " +
                "объявлений и инициализации комплексного числа языка Python представлен в приложении В.\n\n" +
                "Для просмотра полного исходного кода откройте вкладки в решении:\n" +
                "- LexicalAnalyzer.cs\n" +
                "- SyntaxAnalyzer.cs\n" +
                "- MainForm.cs (файл интерфейса и логики)");
        }

        private void ShowScrollableMessageBox(string title, string content)
        {
            Form messageForm = new Form();
            messageForm.Text = title;
            messageForm.Size = new System.Drawing.Size(600, 450);
            messageForm.StartPosition = FormStartPosition.CenterParent;
            messageForm.MinimizeBox = false;
            messageForm.MaximizeBox = false;
            messageForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            RichTextBox rtb = new RichTextBox();
            rtb.Text = content;
            rtb.ReadOnly = true;
            rtb.Dock = DockStyle.Fill;
            rtb.Font = new System.Drawing.Font("Consolas", 10);
            rtb.BackColor = System.Drawing.Color.White;

            Button closeButton = new Button();
            closeButton.Text = GetTranslation("close");
            closeButton.Dock = DockStyle.Bottom;
            closeButton.Height = 30;
            closeButton.Click += (sender2, e2) => messageForm.Close();

            messageForm.Controls.Add(rtb);
            messageForm.Controls.Add(closeButton);
            messageForm.ShowDialog();
        }

        private void InitializeEventHandlers()
        {
            if (создатьToolStripMenuItem != null)
                создатьToolStripMenuItem.Click += CreateNewDocument;
            if (открытьToolStripMenuItem != null)
                открытьToolStripMenuItem.Click += OpenDocument;
            if (сохранитьToolStripMenuItem != null)
                сохранитьToolStripMenuItem.Click += SaveDocument;
            if (сохранитьКакToolStripMenuItem != null)
                сохранитьКакToolStripMenuItem.Click += SaveDocumentAs;
            if (настройкиToolStripMenuItem != null)
                настройкиToolStripMenuItem.Click += (s, e) => ShowSettingsDialog();
            if (выходToolStripMenuItem != null)
                выходToolStripMenuItem.Click += ExitApplication;

            if (отменитьToolStripMenuItem != null)
                отменитьToolStripMenuItem.Click += UndoLastAction;
            if (повторитьToolStripMenuItem != null)
                повторитьToolStripMenuItem.Click += RedoLastAction;
            if (вырезатьToolStripMenuItem != null)
                вырезатьToolStripMenuItem.Click += CutText;
            if (копироватьToolStripMenuItem != null)
                копироватьToolStripMenuItem.Click += CopyText;
            if (вставитьToolStripMenuItem != null)
                вставитьToolStripMenuItem.Click += PasteText;
            if (удалитьToolStripMenuItem != null)
                удалитьToolStripMenuItem.Click += DeleteSelectedText;

            if (постановкаЗадачиToolStripMenuItem != null)
                постановкаЗадачиToolStripMenuItem.Click += ShowPostanovkaZadachi;
            if (грамматикаToolStripMenuItem != null)
                грамматикаToolStripMenuItem.Click += ShowGrammar;
            if (классификацияГрамматикиToolStripMenuItem != null)
                классификацияГрамматикиToolStripMenuItem.Click += ShowClassification;
            if (методАнализаToolStripMenuItem != null)
                методАнализаToolStripMenuItem.Click += ShowTestMethod;
            if (тестовыйПримерToolStripMenuItem != null)
                тестовыйПримерToolStripMenuItem.Click += SetTestExample;
            if (списокЛитературыToolStripMenuItem != null)
                списокЛитературыToolStripMenuItem.Click += ShowLiterature;
            if (исходныйКодПрограммыToolStripMenuItem != null)
                исходныйКодПрограммыToolStripMenuItem.Click += ShowSourceCode;

            if (пускToolStripMenuItem != null)
                пускToolStripMenuItem.Click += RunAnalysis;
            if (AnalisButton != null)
                AnalisButton.Click += RunAnalysis;
            if (AnalisExprButton != null)
                AnalisExprButton.Click += RunExpressionAnalysis;

            if (вызовСправкиToolStripMenuItem != null)
                вызовСправкиToolStripMenuItem.Click += ShowHelp;
            if (оПрограммеToolStripMenuItem != null)
                оПрограммеToolStripMenuItem.Click += ShowAboutBox;
            if (русскийToolStripMenuItem != null)
                русскийToolStripMenuItem.Click += ChangeLanguage;
            if (englishToolStripMenuItem != null)
                englishToolStripMenuItem.Click += ChangeLanguage;

            if (CreateButton != null)
                CreateButton.Click += CreateNewDocument;
            if (OpenButton != null)
                OpenButton.Click += OpenDocument;
            if (SaveButton != null)
                SaveButton.Click += SaveDocument;
            if (BackButton != null)
                BackButton.Click += UndoLastAction;
            if (ForwardButton != null)
                ForwardButton.Click += RedoLastAction;
            if (CopyButton != null)
                CopyButton.Click += CopyText;
            if (CutButton != null)
                CutButton.Click += CutText;
            if (InputButton != null)
                InputButton.Click += PasteText;
            if (RefButton != null)
                RefButton.Click += ShowHelp;
            if (button1 != null)
                button1.Click += ShowAboutBox;

            if (dataGridView1 != null)
                dataGridView1.CellClick += DataGridView1_CellClick;

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N) CreateNewDocument(null, null);
            else if (e.Control && e.KeyCode == Keys.O) OpenDocument(null, null);
            else if (e.Control && e.KeyCode == Keys.S) SaveDocument(null, null);
            else if (e.Control && e.KeyCode == Keys.F5) RunAnalysis(null, null);
            else if (e.Control && e.KeyCode == Keys.Z) UndoLastAction(null, null);
            else if (e.Control && e.KeyCode == Keys.Y) RedoLastAction(null, null);
            else if (e.Control && e.KeyCode == Keys.X) CutText(null, null);
            else if (e.Control && e.KeyCode == Keys.C) CopyText(null, null);
            else if (e.Control && e.KeyCode == Keys.V) PasteText(null, null);
            else if (e.Control && e.KeyCode == Keys.W) CloseTab(currentTabIndex);
            else if (e.Control && e.KeyCode == Keys.Tab && tabControlEditor != null && tabControlEditor.TabPages.Count > 1)
            {
                int newIndex = (currentTabIndex + 1) % tabControlEditor.TabPages.Count;
                tabControlEditor.SelectedIndex = newIndex;
            }
            else if (e.KeyCode == Keys.F1) ShowHelp(null, null);
            else if (e.KeyCode == Keys.F5) RunAnalysis(null, null);
        }
    }
}
