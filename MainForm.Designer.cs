namespace GUIshka
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

            this.dataGridView1 = new System.Windows.Forms.DataGridView();

            this.richTextBoxAst = new System.Windows.Forms.RichTextBox();

            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьКакToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.правкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отменитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.повторитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.вырезатьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.копироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.вставитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.текстToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.постановкаЗадачиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.грамматикаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.классификацияГрамматикиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.методАнализаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.тестовыйПримерToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.списокЛитературыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.исходныйКодПрограммыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.пускToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.справкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.вызовСправкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.языкToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.русскийToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.CreateButton = new System.Windows.Forms.Button();
            this.OpenButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.ForwardButton = new System.Windows.Forms.Button();
            this.CopyButton = new System.Windows.Forms.Button();
            this.CutButton = new System.Windows.Forms.Button();
            this.InputButton = new System.Windows.Forms.Button();
            this.AnalisButton = new System.Windows.Forms.Button();
            this.AnalisExprButton = new System.Windows.Forms.Button();
            this.RefButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();

            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.tabPageAst = new System.Windows.Forms.TabPage();
            this.tabControlMain = new System.Windows.Forms.TabControl();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.tabPageAst.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStripPanel.SuspendLayout();
            this.SuspendLayout();

            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(4, 4);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(1305, 359);
            this.dataGridView1.TabIndex = 0;

            this.richTextBoxAst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxAst.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBoxAst.Location = new System.Drawing.Point(4, 4);
            this.richTextBoxAst.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBoxAst.Name = "richTextBoxAst";
            this.richTextBoxAst.ReadOnly = true;
            this.richTextBoxAst.Size = new System.Drawing.Size(1305, 359);
            this.richTextBoxAst.TabIndex = 0;
            this.richTextBoxAst.Text = "";

            this.tabPageErrors.Controls.Add(this.dataGridView1);
            this.tabPageErrors.Location = new System.Drawing.Point(4, 25);
            this.tabPageErrors.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageErrors.Name = "tabPageErrors";
            this.tabPageErrors.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageErrors.Size = new System.Drawing.Size(1313, 367);
            this.tabPageErrors.TabIndex = 0;
            this.tabPageErrors.Text = "Ошибки";

            this.tabPageAst.Controls.Add(this.richTextBoxAst);
            this.tabPageAst.Location = new System.Drawing.Point(4, 25);
            this.tabPageAst.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageAst.Name = "tabPageAst";
            this.tabPageAst.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageAst.Size = new System.Drawing.Size(1313, 367);
            this.tabPageAst.TabIndex = 1;
            this.tabPageAst.Text = "AST / JSON";

            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPageErrors);
            this.tabControlMain.Controls.Add(this.tabPageAst);
            this.tabControlMain.Location = new System.Drawing.Point(16, 213);
            this.tabControlMain.Margin = new System.Windows.Forms.Padding(4);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1321, 396);
            this.tabControlMain.TabIndex = 15;
            this.tabControlMain.Visible = false;

            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.правкаToolStripMenuItem,
            this.текстToolStripMenuItem,
            this.пускToolStripMenuItem,
            this.справкаToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1353, 28);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";

            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.создатьToolStripMenuItem,
            this.открытьToolStripMenuItem,
            this.сохранитьToolStripMenuItem,
            this.сохранитьКакToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.файлToolStripMenuItem.Text = "Файл";

            this.создатьToolStripMenuItem.Name = "создатьToolStripMenuItem";
            this.создатьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.создатьToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.создатьToolStripMenuItem.Text = "Создать";

            this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            this.открытьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.открытьToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.открытьToolStripMenuItem.Text = "Открыть";

            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.сохранитьToolStripMenuItem.Text = "Сохранить";

            this.сохранитьКакToolStripMenuItem.Name = "сохранитьКакToolStripMenuItem";
            this.сохранитьКакToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
            | System.Windows.Forms.Keys.S)));
            this.сохранитьКакToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.сохранитьКакToolStripMenuItem.Text = "Сохранить как";

            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.настройкиToolStripMenuItem.Text = "Настройки";

            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(268, 26);
            this.выходToolStripMenuItem.Text = "Выход";

            this.правкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.отменитьToolStripMenuItem,
            this.повторитьToolStripMenuItem,
            this.вырезатьToolStripMenuItem,
            this.копироватьToolStripMenuItem,
            this.вставитьToolStripMenuItem,
            this.удалитьToolStripMenuItem});
            this.правкаToolStripMenuItem.Name = "правкаToolStripMenuItem";
            this.правкаToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
            this.правкаToolStripMenuItem.Text = "Правка";

            this.отменитьToolStripMenuItem.Name = "отменитьToolStripMenuItem";
            this.отменитьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.отменитьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.отменитьToolStripMenuItem.Text = "Отменить";

            this.повторитьToolStripMenuItem.Name = "повторитьToolStripMenuItem";
            this.повторитьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.повторитьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.повторитьToolStripMenuItem.Text = "Повторить";

            this.вырезатьToolStripMenuItem.Name = "вырезатьToolStripMenuItem";
            this.вырезатьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.вырезатьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.вырезатьToolStripMenuItem.Text = "Вырезать";

            this.копироватьToolStripMenuItem.Name = "копироватьToolStripMenuItem";
            this.копироватьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.копироватьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.копироватьToolStripMenuItem.Text = "Копировать";

            this.вставитьToolStripMenuItem.Name = "вставитьToolStripMenuItem";
            this.вставитьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.вставитьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.вставитьToolStripMenuItem.Text = "Вставить";

            this.удалитьToolStripMenuItem.Name = "удалитьToolStripMenuItem";
            this.удалитьToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.удалитьToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.удалитьToolStripMenuItem.Text = "Удалить";

            this.текстToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.постановкаЗадачиToolStripMenuItem,
            this.грамматикаToolStripMenuItem,
            this.классификацияГрамматикиToolStripMenuItem,
            this.методАнализаToolStripMenuItem,
            this.тестовыйПримерToolStripMenuItem,
            this.списокЛитературыToolStripMenuItem,
            this.исходныйКодПрограммыToolStripMenuItem});
            this.текстToolStripMenuItem.Name = "текстToolStripMenuItem";
            this.текстToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.текстToolStripMenuItem.Text = "Текст";

            this.постановкаЗадачиToolStripMenuItem.Name = "постановкаЗадачиToolStripMenuItem";
            this.постановкаЗадачиToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.постановкаЗадачиToolStripMenuItem.Text = "Постановка задачи";

            this.грамматикаToolStripMenuItem.Name = "грамматикаToolStripMenuItem";
            this.грамматикаToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.грамматикаToolStripMenuItem.Text = "Грамматика";

            this.классификацияГрамматикиToolStripMenuItem.Name = "классификацияГрамматикиToolStripMenuItem";
            this.классификацияГрамматикиToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.классификацияГрамматикиToolStripMenuItem.Text = "Классификация грамматики";

            this.методАнализаToolStripMenuItem.Name = "методАнализаToolStripMenuItem";
            this.методАнализаToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.методАнализаToolStripMenuItem.Text = "Метод анализа";

            this.тестовыйПримерToolStripMenuItem.Name = "тестовыйПримерToolStripMenuItem";
            this.тестовыйПримерToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.тестовыйПримерToolStripMenuItem.Text = "Тестовый пример";

            this.списокЛитературыToolStripMenuItem.Name = "списокЛитературыToolStripMenuItem";
            this.списокЛитературыToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.списокЛитературыToolStripMenuItem.Text = "Список литературы";

            this.исходныйКодПрограммыToolStripMenuItem.Name = "исходныйКодПрограммыToolStripMenuItem";
            this.исходныйКодПрограммыToolStripMenuItem.Size = new System.Drawing.Size(288, 26);
            this.исходныйКодПрограммыToolStripMenuItem.Text = "Исходный код программы";

            this.пускToolStripMenuItem.Name = "пускToolStripMenuItem";
            this.пускToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.пускToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.пускToolStripMenuItem.Text = "Пуск";

            this.справкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.вызовСправкиToolStripMenuItem,
            this.языкToolStripMenuItem,
            this.оПрограммеToolStripMenuItem});
            this.справкаToolStripMenuItem.Name = "справкаToolStripMenuItem";
            this.справкаToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.справкаToolStripMenuItem.Text = "Справка";

            this.вызовСправкиToolStripMenuItem.Name = "вызовСправкиToolStripMenuItem";
            this.вызовСправкиToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.вызовСправкиToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.вызовСправкиToolStripMenuItem.Text = "Вызов справки";

            this.языкToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.русскийToolStripMenuItem,
            this.englishToolStripMenuItem});
            this.языкToolStripMenuItem.Name = "языкToolStripMenuItem";
            this.языкToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.языкToolStripMenuItem.Text = "Язык / Language";

            this.русскийToolStripMenuItem.Name = "русскийToolStripMenuItem";
            this.русскийToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
            this.русскийToolStripMenuItem.Text = "Русский";

            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
            this.englishToolStripMenuItem.Text = "English";

            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.оПрограммеToolStripMenuItem.Text = "О программе";

            this.toolStripPanel.AutoSize = true;
            this.toolStripPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.toolStripPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.toolStripPanel.Controls.Add(this.CreateButton);
            this.toolStripPanel.Controls.Add(this.OpenButton);
            this.toolStripPanel.Controls.Add(this.SaveButton);
            this.toolStripPanel.Controls.Add(this.BackButton);
            this.toolStripPanel.Controls.Add(this.ForwardButton);
            this.toolStripPanel.Controls.Add(this.CopyButton);
            this.toolStripPanel.Controls.Add(this.CutButton);
            this.toolStripPanel.Controls.Add(this.InputButton);
            this.toolStripPanel.Controls.Add(this.AnalisButton);
            this.toolStripPanel.Controls.Add(this.AnalisExprButton);
            this.toolStripPanel.Controls.Add(this.RefButton);
            this.toolStripPanel.Controls.Add(this.button1);
            this.toolStripPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripPanel.Location = new System.Drawing.Point(0, 28);
            this.toolStripPanel.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripPanel.Name = "toolStripPanel";
            this.toolStripPanel.Padding = new System.Windows.Forms.Padding(13, 6, 13, 6);
            this.toolStripPanel.Size = new System.Drawing.Size(1353, 88);
            this.toolStripPanel.TabIndex = 14;

            this.CreateButton.AutoSize = true;
            this.CreateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CreateButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CreateButton.BackgroundImage")));
            this.CreateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CreateButton.Location = new System.Drawing.Point(17, 10);
            this.CreateButton.Margin = new System.Windows.Forms.Padding(4);
            this.CreateButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(73, 68);
            this.CreateButton.TabIndex = 0;
            this.CreateButton.UseVisualStyleBackColor = true;

            this.OpenButton.AutoSize = true;
            this.OpenButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OpenButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OpenButton.BackgroundImage")));
            this.OpenButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.OpenButton.Location = new System.Drawing.Point(98, 10);
            this.OpenButton.Margin = new System.Windows.Forms.Padding(4);
            this.OpenButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(73, 68);
            this.OpenButton.TabIndex = 1;
            this.OpenButton.UseVisualStyleBackColor = true;

            this.SaveButton.AutoSize = true;
            this.SaveButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SaveButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveButton.BackgroundImage")));
            this.SaveButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SaveButton.Location = new System.Drawing.Point(179, 10);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(4);
            this.SaveButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(73, 68);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.UseVisualStyleBackColor = true;

            this.BackButton.AutoSize = true;
            this.BackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BackButton.BackgroundImage")));
            this.BackButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.BackButton.Location = new System.Drawing.Point(260, 10);
            this.BackButton.Margin = new System.Windows.Forms.Padding(4);
            this.BackButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(73, 68);
            this.BackButton.TabIndex = 3;
            this.BackButton.UseVisualStyleBackColor = true;

            this.ForwardButton.AutoSize = true;
            this.ForwardButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ForwardButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ForwardButton.BackgroundImage")));
            this.ForwardButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ForwardButton.Location = new System.Drawing.Point(341, 10);
            this.ForwardButton.Margin = new System.Windows.Forms.Padding(4);
            this.ForwardButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.ForwardButton.Name = "ForwardButton";
            this.ForwardButton.Size = new System.Drawing.Size(73, 68);
            this.ForwardButton.TabIndex = 4;
            this.ForwardButton.UseVisualStyleBackColor = true;

            this.CopyButton.AutoSize = true;
            this.CopyButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CopyButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CopyButton.BackgroundImage")));
            this.CopyButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CopyButton.Location = new System.Drawing.Point(422, 10);
            this.CopyButton.Margin = new System.Windows.Forms.Padding(4);
            this.CopyButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(73, 68);
            this.CopyButton.TabIndex = 5;
            this.CopyButton.UseVisualStyleBackColor = true;

            this.CutButton.AutoSize = true;
            this.CutButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CutButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CutButton.BackgroundImage")));
            this.CutButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CutButton.Location = new System.Drawing.Point(503, 10);
            this.CutButton.Margin = new System.Windows.Forms.Padding(4);
            this.CutButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.CutButton.Name = "CutButton";
            this.CutButton.Size = new System.Drawing.Size(73, 68);
            this.CutButton.TabIndex = 6;
            this.CutButton.UseVisualStyleBackColor = true;

            this.InputButton.AutoSize = true;
            this.InputButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.InputButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("InputButton.BackgroundImage")));
            this.InputButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.InputButton.Location = new System.Drawing.Point(584, 10);
            this.InputButton.Margin = new System.Windows.Forms.Padding(4);
            this.InputButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.InputButton.Name = "InputButton";
            this.InputButton.Size = new System.Drawing.Size(73, 68);
            this.InputButton.TabIndex = 7;
            this.InputButton.UseVisualStyleBackColor = true;

            this.AnalisButton.AutoSize = true;
            this.AnalisButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AnalisButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AnalisButton.BackgroundImage")));
            this.AnalisButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.AnalisButton.Location = new System.Drawing.Point(665, 10);
            this.AnalisButton.Margin = new System.Windows.Forms.Padding(4);
            this.AnalisButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.AnalisButton.Name = "AnalisButton";
            this.AnalisButton.Size = new System.Drawing.Size(73, 68);
            this.AnalisButton.TabIndex = 8;
            this.AnalisButton.UseVisualStyleBackColor = true;
            this.AnalisButton.Text = "Анализ";
            this.AnalisButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            this.AnalisExprButton.AutoSize = true;
            this.AnalisExprButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AnalisExprButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AnalisButton.BackgroundImage")));
            this.AnalisExprButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.AnalisExprButton.Location = new System.Drawing.Point(746, 10);
            this.AnalisExprButton.Margin = new System.Windows.Forms.Padding(4);
            this.AnalisExprButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.AnalisExprButton.Name = "AnalisExprButton";
            this.AnalisExprButton.Size = new System.Drawing.Size(73, 68);
            this.AnalisExprButton.TabIndex = 11;
            this.AnalisExprButton.UseVisualStyleBackColor = true;
            this.AnalisExprButton.Text = "Арифметика";
            this.AnalisExprButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            this.RefButton.AutoSize = true;
            this.RefButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RefButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RefButton.BackgroundImage")));
            this.RefButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.RefButton.Location = new System.Drawing.Point(827, 10);
            this.RefButton.Margin = new System.Windows.Forms.Padding(4);
            this.RefButton.MinimumSize = new System.Drawing.Size(73, 68);
            this.RefButton.Name = "RefButton";
            this.RefButton.Size = new System.Drawing.Size(73, 68);
            this.RefButton.TabIndex = 9;
            this.RefButton.UseVisualStyleBackColor = true;

            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button1.Location = new System.Drawing.Point(908, 10);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.MinimumSize = new System.Drawing.Size(73, 68);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(73, 68);
            this.button1.TabIndex = 10;
            this.button1.UseVisualStyleBackColor = true;

            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1353, 624);
            this.Controls.Add(this.toolStripPanel);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1061, 481);
            this.Name = "MainForm";
            this.Text = "Компилятор";
            this.AllowDrop = true;

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageErrors.ResumeLayout(false);
            this.tabPageAst.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStripPanel.ResumeLayout(false);
            this.toolStripPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.RichTextBox richTextBoxAst;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.FlowLayoutPanel toolStripPanel;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.Button OpenButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button ForwardButton;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.Button CutButton;
        private System.Windows.Forms.Button InputButton;
        private System.Windows.Forms.Button AnalisButton;
        private System.Windows.Forms.Button AnalisExprButton;
        private System.Windows.Forms.Button RefButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageErrors;
        private System.Windows.Forms.TabPage tabPageAst;

        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem создатьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьКакToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem правкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отменитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem повторитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem вырезатьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem копироватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem вставитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem текстToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem постановкаЗадачиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem грамматикаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem классификацияГрамматикиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem методАнализаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem тестовыйПримерToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem списокЛитературыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem исходныйКодПрограммыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem пускToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem справкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem вызовСправкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem языкToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem русскийToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
    }
}
