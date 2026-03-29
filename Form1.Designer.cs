namespace FileEncodingScanner;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        topPanel = new TableLayoutPanel();
        folderLabel = new Label();
        folderTextBox = new TextBox();
        browseFolderButton = new Button();
        regexLabel = new Label();
        regexTextBox = new TextBox();
        scanFilesButton = new Button();
        cancelScanButton = new Button();
        convertUtf8BomButton = new Button();
        resultsDataGridView = new DataGridView();
        fileNameColumn = new DataGridViewTextBoxColumn();
        fullPathColumn = new DataGridViewTextBoxColumn();
        encodingColumn = new DataGridViewTextBoxColumn();
        sizeColumn = new DataGridViewTextBoxColumn();
        modifiedColumn = new DataGridViewTextBoxColumn();
        statusStrip = new StatusStrip();
        stateToolStripStatusLabel = new ToolStripStatusLabel();
        summaryToolStripStatusLabel = new ToolStripStatusLabel();
        topPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)resultsDataGridView).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // topPanel
        // 
        topPanel.AutoSize = true;
        topPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        topPanel.ColumnCount = 5;
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.Controls.Add(folderLabel, 0, 0);
        topPanel.Controls.Add(folderTextBox, 1, 0);
        topPanel.Controls.Add(browseFolderButton, 2, 0);
        topPanel.Controls.Add(regexLabel, 0, 1);
        topPanel.Controls.Add(regexTextBox, 1, 1);
        topPanel.Controls.Add(scanFilesButton, 2, 1);
        topPanel.Controls.Add(cancelScanButton, 3, 1);
        topPanel.Controls.Add(convertUtf8BomButton, 4, 1);
        topPanel.Dock = DockStyle.Top;
        topPanel.Location = new Point(0, 0);
        topPanel.Name = "topPanel";
        topPanel.Padding = new Padding(12);
        topPanel.RowCount = 2;
        topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topPanel.Size = new Size(1104, 88);
        topPanel.TabIndex = 0;
        // 
        // folderLabel
        // 
        folderLabel.Anchor = AnchorStyles.Left;
        folderLabel.AutoSize = true;
        folderLabel.Location = new Point(15, 20);
        folderLabel.Margin = new Padding(3, 8, 8, 0);
        folderLabel.Name = "folderLabel";
        folderLabel.Size = new Size(32, 15);
        folderLabel.TabIndex = 0;
        folderLabel.Text = "目录";
        // 
        // folderTextBox
        // 
        folderTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        folderTextBox.Location = new Point(58, 15);
        folderTextBox.Margin = new Padding(3, 3, 8, 0);
        folderTextBox.Name = "folderTextBox";
        folderTextBox.PlaceholderText = "请选择要扫描的根目录";
        folderTextBox.Size = new Size(690, 23);
        folderTextBox.TabIndex = 1;
        // 
        // browseFolderButton
        // 
        browseFolderButton.AutoSize = true;
        browseFolderButton.Location = new Point(756, 15);
        browseFolderButton.Margin = new Padding(0, 3, 8, 0);
        browseFolderButton.Name = "browseFolderButton";
        browseFolderButton.Size = new Size(51, 25);
        browseFolderButton.TabIndex = 2;
        browseFolderButton.Text = "浏览...";
        browseFolderButton.UseVisualStyleBackColor = true;
        browseFolderButton.Click += browseFolderButton_Click;
        // 
        // regexLabel
        // 
        regexLabel.Anchor = AnchorStyles.Left;
        regexLabel.AutoSize = true;
        regexLabel.Location = new Point(15, 56);
        regexLabel.Margin = new Padding(3, 12, 8, 0);
        regexLabel.Name = "regexLabel";
        regexLabel.Size = new Size(68, 15);
        regexLabel.TabIndex = 3;
        regexLabel.Text = "文件名匹配";
        // 
        // regexTextBox
        // 
        regexTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        regexTextBox.Location = new Point(58, 51);
        regexTextBox.Margin = new Padding(3, 8, 8, 0);
        regexTextBox.Name = "regexTextBox";
        regexTextBox.PlaceholderText = "请输入用于匹配文件名的正则表达式";
        regexTextBox.Size = new Size(690, 23);
        regexTextBox.TabIndex = 4;
        // 
        // scanFilesButton
        // 
        scanFilesButton.AutoSize = true;
        scanFilesButton.Location = new Point(756, 51);
        scanFilesButton.Margin = new Padding(0, 8, 8, 0);
        scanFilesButton.Name = "scanFilesButton";
        scanFilesButton.Size = new Size(51, 25);
        scanFilesButton.TabIndex = 5;
        scanFilesButton.Text = "扫描";
        scanFilesButton.UseVisualStyleBackColor = true;
        scanFilesButton.Click += scanFilesButton_Click;
        // 
        // cancelScanButton
        // 
        cancelScanButton.AutoSize = true;
        cancelScanButton.Enabled = false;
        cancelScanButton.Location = new Point(815, 51);
        cancelScanButton.Margin = new Padding(0, 8, 0, 0);
        cancelScanButton.Name = "cancelScanButton";
        cancelScanButton.Size = new Size(51, 25);
        cancelScanButton.TabIndex = 6;
        cancelScanButton.Text = "取消";
        cancelScanButton.UseVisualStyleBackColor = true;
        cancelScanButton.Click += cancelScanButton_Click;
        // 
        // convertUtf8BomButton
        // 
        convertUtf8BomButton.AutoSize = true;
        convertUtf8BomButton.Enabled = false;
        convertUtf8BomButton.Location = new Point(874, 51);
        convertUtf8BomButton.Margin = new Padding(8, 8, 0, 0);
        convertUtf8BomButton.Name = "convertUtf8BomButton";
        convertUtf8BomButton.Size = new Size(143, 25);
        convertUtf8BomButton.TabIndex = 7;
        convertUtf8BomButton.Text = "全部转为 UTF-8 BOM";
        convertUtf8BomButton.UseVisualStyleBackColor = true;
        convertUtf8BomButton.Click += convertUtf8BomButton_Click;
        // 
        // resultsDataGridView
        // 
        resultsDataGridView.AllowUserToAddRows = false;
        resultsDataGridView.AllowUserToDeleteRows = false;
        resultsDataGridView.AllowUserToOrderColumns = true;
        resultsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        resultsDataGridView.BackgroundColor = SystemColors.Window;
        resultsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        resultsDataGridView.Columns.AddRange(new DataGridViewColumn[] { fileNameColumn, fullPathColumn, encodingColumn, sizeColumn, modifiedColumn });
        resultsDataGridView.Dock = DockStyle.Fill;
        resultsDataGridView.Location = new Point(0, 88);
        resultsDataGridView.MultiSelect = true;
        resultsDataGridView.Name = "resultsDataGridView";
        resultsDataGridView.ReadOnly = true;
        resultsDataGridView.RowHeadersVisible = false;
        resultsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        resultsDataGridView.Size = new Size(1104, 518);
        resultsDataGridView.TabIndex = 1;
        resultsDataGridView.CellDoubleClick += resultsDataGridView_CellDoubleClick;
        // 
        // fileNameColumn
        // 
        fileNameColumn.DataPropertyName = "FileName";
        fileNameColumn.HeaderText = "文件名";
        fileNameColumn.MinimumWidth = 140;
        fileNameColumn.Name = "fileNameColumn";
        fileNameColumn.ReadOnly = true;
        fileNameColumn.Width = 180;
        // 
        // fullPathColumn
        // 
        fullPathColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        fullPathColumn.DataPropertyName = "FullPath";
        fullPathColumn.HeaderText = "完整路径";
        fullPathColumn.MinimumWidth = 280;
        fullPathColumn.Name = "fullPathColumn";
        fullPathColumn.ReadOnly = true;
        // 
        // encodingColumn
        // 
        encodingColumn.DataPropertyName = "EncodingName";
        encodingColumn.HeaderText = "编码";
        encodingColumn.MinimumWidth = 120;
        encodingColumn.Name = "encodingColumn";
        encodingColumn.ReadOnly = true;
        encodingColumn.Width = 130;
        // 
        // sizeColumn
        // 
        sizeColumn.DataPropertyName = "Size";
        sizeColumn.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" };
        sizeColumn.HeaderText = "大小 (字节)";
        sizeColumn.MinimumWidth = 100;
        sizeColumn.Name = "sizeColumn";
        sizeColumn.ReadOnly = true;
        sizeColumn.Width = 110;
        // 
        // modifiedColumn
        // 
        modifiedColumn.DataPropertyName = "LastWriteTime";
        modifiedColumn.DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" };
        modifiedColumn.HeaderText = "修改时间";
        modifiedColumn.MinimumWidth = 150;
        modifiedColumn.Name = "modifiedColumn";
        modifiedColumn.ReadOnly = true;
        modifiedColumn.Width = 160;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { stateToolStripStatusLabel, summaryToolStripStatusLabel });
        statusStrip.Location = new Point(0, 606);
        statusStrip.Name = "statusStrip";
        statusStrip.SizingGrip = false;
        statusStrip.Size = new Size(1104, 22);
        statusStrip.TabIndex = 2;
        // 
        // stateToolStripStatusLabel
        // 
        stateToolStripStatusLabel.Name = "stateToolStripStatusLabel";
        stateToolStripStatusLabel.Size = new Size(31, 17);
        stateToolStripStatusLabel.Text = "就绪";
        // 
        // summaryToolStripStatusLabel
        // 
        summaryToolStripStatusLabel.Name = "summaryToolStripStatusLabel";
        summaryToolStripStatusLabel.Spring = true;
        summaryToolStripStatusLabel.Text = "扫描: 0 | 匹配: 0 | 错误: 0";
        summaryToolStripStatusLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // Form1
        // 
        AcceptButton = scanFilesButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelScanButton;
        ClientSize = new Size(1104, 628);
        Controls.Add(resultsDataGridView);
        Controls.Add(topPanel);
        Controls.Add(statusStrip);
        MinimumSize = new Size(960, 520);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "文件编码扫描器";
        topPanel.ResumeLayout(false);
        topPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)resultsDataGridView).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel topPanel;
    private Label folderLabel;
    private TextBox folderTextBox;
    private Button browseFolderButton;
    private Label regexLabel;
    private TextBox regexTextBox;
    private Button scanFilesButton;
    private Button cancelScanButton;
    private Button convertUtf8BomButton;
    private DataGridView resultsDataGridView;
    private DataGridViewTextBoxColumn fileNameColumn;
    private DataGridViewTextBoxColumn fullPathColumn;
    private DataGridViewTextBoxColumn encodingColumn;
    private DataGridViewTextBoxColumn sizeColumn;
    private DataGridViewTextBoxColumn modifiedColumn;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel stateToolStripStatusLabel;
    private ToolStripStatusLabel summaryToolStripStatusLabel;
}
