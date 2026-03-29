using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FileEncodingScanner;

public partial class Form1 : Form
{
    private const string NotepadPlusPlusExecutableName = "notepad++.exe";
    private const string DefaultFolderPath = @"D:\Mir2";
    private const string DefaultFileNamePattern = @"(?i).*\.pas$";

    private readonly BindingSource _resultsBindingSource = new();
    private readonly BindingList<FileScanResult> _results = new();
    private readonly FolderBrowserDialog _folderBrowserDialog = new();
    private readonly ContextMenuStrip _resultsContextMenuStrip;
    private readonly ToolStripMenuItem _openInEditorToolStripMenuItem;
    private readonly ToolStripMenuItem _convertEncodingToolStripMenuItem;

    private CancellationTokenSource? _scanCancellation;
    private bool _isConverting;
    private int _lastScannedFiles;
    private int _lastErrorCount;

    public Form1()
    {
        InitializeComponent();

        _folderBrowserDialog.Description = "请选择要扫描的根目录";
        _folderBrowserDialog.UseDescriptionForTitle = true;

        resultsDataGridView.AutoGenerateColumns = false;
        _resultsBindingSource.DataSource = _results;
        resultsDataGridView.DataSource = _resultsBindingSource;
        resultsDataGridView.SelectionChanged += resultsDataGridView_SelectionChanged;
        resultsDataGridView.CellMouseDown += resultsDataGridView_CellMouseDown;
        resultsDataGridView.MouseDown += resultsDataGridView_MouseDown;

        _resultsContextMenuStrip = new ContextMenuStrip();
        _resultsContextMenuStrip.Opening += resultsContextMenuStrip_Opening;

        _openInEditorToolStripMenuItem = new ToolStripMenuItem("用编辑器打开", null, openInEditorToolStripMenuItem_Click);
        _convertEncodingToolStripMenuItem = new ToolStripMenuItem("转换编码");
        _convertEncodingToolStripMenuItem.DropDownItems.AddRange(
            new ToolStripItem[]
            {
                new ToolStripMenuItem("ANSI", null, convertToAnsiToolStripMenuItem_Click),
                new ToolStripMenuItem("UTF-8", null, convertToUtf8ToolStripMenuItem_Click),
                new ToolStripMenuItem("UTF-8 BOM", null, convertToUtf8BomToolStripMenuItem_Click)
            });

        _resultsContextMenuStrip.Items.AddRange(new ToolStripItem[]
        {
            _openInEditorToolStripMenuItem,
            _convertEncodingToolStripMenuItem
        });

        resultsDataGridView.ContextMenuStrip = _resultsContextMenuStrip;

        folderTextBox.Text = DefaultFolderPath;
        regexTextBox.Text = DefaultFileNamePattern;

        UpdateStatus("就绪。", BuildScanSummaryText());
        UpdateCommandState();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _scanCancellation?.Cancel();
        base.OnFormClosing(e);
    }

    private bool IsScanning => _scanCancellation is not null;

    private bool IsBusy => IsScanning || _isConverting;

    private void browseFolderButton_Click(object? sender, EventArgs e)
    {
        var currentFolder = folderTextBox.Text.Trim();
        if (Directory.Exists(currentFolder))
        {
            _folderBrowserDialog.SelectedPath = currentFolder;
        }

        if (_folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
        {
            folderTextBox.Text = _folderBrowserDialog.SelectedPath;
        }
    }

    private async void scanFilesButton_Click(object? sender, EventArgs e)
    {
        await StartScanAsync();
    }

    private async void convertUtf8BomButton_Click(object? sender, EventArgs e)
    {
        await ConvertAllResultsAsync(FileEncodings.Utf8Bom);
    }

    private void cancelScanButton_Click(object? sender, EventArgs e)
    {
        _scanCancellation?.Cancel();
    }

    private void resultsDataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (resultsDataGridView.Rows[e.RowIndex].DataBoundItem is not FileScanResult result)
        {
            return;
        }

        OpenResultFile(result);
    }

    private void resultsDataGridView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right || e.RowIndex < 0)
        {
            return;
        }

        var row = resultsDataGridView.Rows[e.RowIndex];
        if (!row.Selected)
        {
            resultsDataGridView.ClearSelection();
            row.Selected = true;
        }

        if (e.ColumnIndex >= 0)
        {
            resultsDataGridView.CurrentCell = row.Cells[e.ColumnIndex];
        }
    }

    private void resultsDataGridView_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        var hitTest = resultsDataGridView.HitTest(e.X, e.Y);
        if (hitTest.RowIndex >= 0)
        {
            return;
        }

        resultsDataGridView.ClearSelection();
        UpdateCommandState();
    }

    private void resultsDataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateCommandState();
    }

    private void resultsContextMenuStrip_Opening(object? sender, CancelEventArgs e)
    {
        var hasSelection = GetSelectedResults().Count > 0;
        var canUseMenu = !IsBusy && hasSelection;

        _openInEditorToolStripMenuItem.Enabled = canUseMenu;
        _convertEncodingToolStripMenuItem.Enabled = canUseMenu;
        e.Cancel = !canUseMenu;
    }

    private void openInEditorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OpenResultFiles(GetSelectedResults());
    }

    private async void convertToAnsiToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        await ConvertSelectedResultsAsync(FileEncodings.Ansi);
    }

    private async void convertToUtf8ToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        await ConvertSelectedResultsAsync(FileEncodings.Utf8);
    }

    private async void convertToUtf8BomToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        await ConvertSelectedResultsAsync(FileEncodings.Utf8Bom);
    }

    private async Task StartScanAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var folderPath = folderTextBox.Text.Trim();
        if (!Directory.Exists(folderPath))
        {
            MessageBox.Show(
                this,
                "请先选择一个有效的扫描目录。",
                "目录无效",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var pattern = string.IsNullOrWhiteSpace(regexTextBox.Text) ? ".*" : regexTextBox.Text;
        Regex fileNameRegex;
        try
        {
            fileNameRegex = new Regex(pattern, RegexOptions.Compiled);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(
                this,
                $"文件名匹配表达式无效。{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "正则表达式无效",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _lastScannedFiles = 0;
        _lastErrorCount = 0;
        ReplaceResults(Array.Empty<FileScanResult>());
        UpdateStatus("正在扫描...", BuildScanSummaryText());

        _scanCancellation = new CancellationTokenSource();
        UpdateCommandState();

        try
        {
            var summary = await Task.Run(
                () => FileScanner.Scan(folderPath, fileNameRegex, _scanCancellation.Token),
                _scanCancellation.Token);

            _lastScannedFiles = summary.ScannedFiles;
            _lastErrorCount = summary.ErrorCount;
            ReplaceResults(summary.Results);
            UpdateStatus("扫描完成。", BuildScanSummaryText());
        }
        catch (OperationCanceledException)
        {
            _lastScannedFiles = 0;
            _lastErrorCount = 0;
            ReplaceResults(Array.Empty<FileScanResult>());
            UpdateStatus("已取消扫描。", BuildScanSummaryText());
        }
        catch (Exception ex)
        {
            _lastScannedFiles = 0;
            _lastErrorCount = 1;
            ReplaceResults(Array.Empty<FileScanResult>());
            UpdateStatus("扫描失败。", BuildScanSummaryText());
            MessageBox.Show(
                this,
                ex.Message,
                "扫描失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _scanCancellation?.Dispose();
            _scanCancellation = null;
            UpdateCommandState();
        }
    }

    private async Task ConvertAllResultsAsync(FileEncodingInfo targetEncoding)
    {
        var targets = _results.ToList();
        if (targets.Count == 0 || IsBusy)
        {
            return;
        }

        var confirmationResult = MessageBox.Show(
            this,
            $"是否将当前列表中的 {targets.Count:N0} 个文件全部转换为 {targetEncoding.DisplayName}？{Environment.NewLine}{Environment.NewLine}此操作会直接覆盖原文件。",
            "确认批量转换",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmationResult != DialogResult.Yes)
        {
            return;
        }

        await ConvertResultsAsync(targets, targetEncoding);
    }

    private async Task ConvertSelectedResultsAsync(FileEncodingInfo targetEncoding)
    {
        var targets = GetSelectedResults();
        if (targets.Count == 0 || IsBusy)
        {
            return;
        }

        await ConvertResultsAsync(targets, targetEncoding);
    }

    private async Task ConvertResultsAsync(IReadOnlyList<FileScanResult> targets, FileEncodingInfo targetEncoding)
    {
        if (targets.Count == 0)
        {
            return;
        }

        var selectedPaths = GetSelectedPaths();
        _isConverting = true;
        UpdateCommandState();
        UpdateStatus($"正在转换为 {targetEncoding.DisplayName}...", BuildScanSummaryText());

        try
        {
            var conversionResults = await Task.Run(() => FileEncodingConverter.ConvertFiles(targets, targetEncoding));
            ApplyConversionResults(conversionResults);

            var successCount = conversionResults.Count(static result => result.Succeeded);
            var failureCount = conversionResults.Count - successCount;

            UpdateStatus(
                failureCount == 0 ? "转换完成。" : "转换完成，部分文件失败。",
                BuildConversionSummaryText(successCount, failureCount));

            if (failureCount > 0)
            {
                MessageBox.Show(
                    this,
                    BuildConversionFailureMessage(conversionResults, targetEncoding),
                    "转换结果",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        finally
        {
            _isConverting = false;
            UpdateCommandState();
            RestoreSelection(selectedPaths);
        }
    }

    private void UpdateCommandState()
    {
        var hasResults = _results.Count > 0;
        var hasSelection = GetSelectedResults().Count > 0;
        var isBusy = IsBusy;

        folderTextBox.Enabled = !isBusy;
        regexTextBox.Enabled = !isBusy;
        browseFolderButton.Enabled = !isBusy;
        scanFilesButton.Enabled = !isBusy;
        cancelScanButton.Enabled = IsScanning;
        convertUtf8BomButton.Enabled = !isBusy && hasResults;
        resultsDataGridView.Enabled = !isBusy;

        _openInEditorToolStripMenuItem.Enabled = !isBusy && hasSelection;
        _convertEncodingToolStripMenuItem.Enabled = !isBusy && hasSelection;

        UseWaitCursor = isBusy;
    }

    private void ReplaceResults(IEnumerable<FileScanResult> results)
    {
        _results.RaiseListChangedEvents = false;
        _results.Clear();

        foreach (var result in results)
        {
            _results.Add(result);
        }

        _results.RaiseListChangedEvents = true;
        _resultsBindingSource.ResetBindings(metadataChanged: false);
        UpdateCommandState();
    }

    private void ApplyConversionResults(IReadOnlyList<FileConversionResult> conversionResults)
    {
        var updatedResults = conversionResults
            .Where(static result => result.Succeeded && result.UpdatedResult is not null)
            .ToDictionary(
                static result => result.FilePath,
                static result => result.UpdatedResult!,
                StringComparer.OrdinalIgnoreCase);

        if (updatedResults.Count == 0)
        {
            return;
        }

        _results.RaiseListChangedEvents = false;

        for (var index = 0; index < _results.Count; index++)
        {
            var currentResult = _results[index];
            if (updatedResults.TryGetValue(currentResult.FullPath, out var updatedResult))
            {
                _results[index] = updatedResult;
            }
        }

        _results.RaiseListChangedEvents = true;
        _resultsBindingSource.ResetBindings(metadataChanged: false);
    }

    private List<FileScanResult> GetSelectedResults()
    {
        return resultsDataGridView.SelectedRows
            .Cast<DataGridViewRow>()
            .OrderBy(static row => row.Index)
            .Select(static row => row.DataBoundItem as FileScanResult)
            .Where(static result => result is not null)
            .Select(static result => result!)
            .DistinctBy(static result => result.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private HashSet<string> GetSelectedPaths()
    {
        return GetSelectedResults()
            .Select(static result => result.FullPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void RestoreSelection(HashSet<string> selectedPaths)
    {
        if (selectedPaths.Count == 0 || _results.Count == 0)
        {
            return;
        }

        resultsDataGridView.ClearSelection();

        DataGridViewCell? firstSelectedCell = null;
        foreach (DataGridViewRow row in resultsDataGridView.Rows)
        {
            if (row.DataBoundItem is not FileScanResult result || !selectedPaths.Contains(result.FullPath))
            {
                continue;
            }

            row.Selected = true;
            firstSelectedCell ??= row.Cells[0];
        }

        if (firstSelectedCell is not null)
        {
            resultsDataGridView.CurrentCell = firstSelectedCell;
        }
    }

    private void OpenResultFiles(IReadOnlyList<FileScanResult> results)
    {
        foreach (var result in results)
        {
            OpenResultFile(result);
        }
    }

    private void UpdateStatus(string state, string summary)
    {
        stateToolStripStatusLabel.Text = state;
        summaryToolStripStatusLabel.Text = summary;
    }

    private string BuildScanSummaryText()
    {
        return $"扫描: {_lastScannedFiles:N0} | 匹配: {_results.Count:N0} | 错误: {_lastErrorCount:N0}";
    }

    private string BuildConversionSummaryText(int successCount, int failureCount)
    {
        return $"{BuildScanSummaryText()} | 转换成功: {successCount:N0} | 转换失败: {failureCount:N0}";
    }

    private static string BuildConversionFailureMessage(
        IReadOnlyList<FileConversionResult> conversionResults,
        FileEncodingInfo targetEncoding)
    {
        var failures = conversionResults
            .Where(static result => !result.Succeeded)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"以下文件未能转换为 {targetEncoding.DisplayName}：");
        builder.AppendLine();

        foreach (var failure in failures.Take(10))
        {
            builder.AppendLine(failure.FilePath);
            builder.AppendLine($"原因：{failure.ErrorMessage}");
            builder.AppendLine();
        }

        if (failures.Count > 10)
        {
            builder.AppendLine($"其余 {failures.Count - 10:N0} 个失败文件未显示。");
        }

        return builder.ToString().TrimEnd();
    }

    private void OpenResultFile(FileScanResult result)
    {
        var filePath = result.FullPath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            MessageBox.Show(
                this,
                "所选结果没有有效的文件路径。",
                "打开文件",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(filePath))
        {
            MessageBox.Show(
                this,
                $"文件不存在。{Environment.NewLine}{Environment.NewLine}{filePath}",
                "文件不存在",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (TryOpenWithNotepadPlusPlus(filePath))
            {
                return;
            }

            StartProcess(
                new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"打开文件失败。{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "打开文件",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static bool TryOpenWithNotepadPlusPlus(string filePath)
    {
        var executablePath = ResolveNotepadPlusPlusPath();
        if (string.IsNullOrEmpty(executablePath))
        {
            return false;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(filePath);

        StartProcess(startInfo);
        return true;
    }

    private static string? ResolveNotepadPlusPlusPath()
    {
        foreach (var candidatePath in EnumerateNotepadPlusPlusCandidatePaths())
        {
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateNotepadPlusPlusCandidatePaths()
    {
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pathDirectory in EnumeratePathDirectories())
        {
            var candidatePath = Path.Combine(pathDirectory, NotepadPlusPlusExecutableName);
            if (seenPaths.Add(candidatePath))
            {
                yield return candidatePath;
            }
        }

        var baseDirectories = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
        };

        foreach (var baseDirectory in baseDirectories)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                continue;
            }

            var candidatePath = Path.Combine(baseDirectory, "Notepad++", NotepadPlusPlusExecutableName);
            if (seenPaths.Add(candidatePath))
            {
                yield return candidatePath;
            }
        }
    }

    private static IEnumerable<string> EnumeratePathDirectories()
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            yield break;
        }

        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                yield return directory;
            }
        }
    }

    private static void StartProcess(ProcessStartInfo startInfo)
    {
        var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException("目标进程未能成功启动。");
        }

        process.Dispose();
    }
}
