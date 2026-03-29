namespace FileEncodingScanner;

public sealed record FileScanResult(
    string FileName,
    string FullPath,
    string EncodingName,
    long Size,
    DateTime LastWriteTime);

public sealed record FileScanSummary(
    IReadOnlyList<FileScanResult> Results,
    int ScannedFiles,
    int MatchedFiles,
    int ErrorCount);
