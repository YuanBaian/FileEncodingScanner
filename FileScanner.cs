using System.Security;
using System.Text.RegularExpressions;

namespace FileEncodingScanner;

internal static class FileScanner
{
    public static FileScanSummary Scan(string rootFolder, Regex fileNameRegex, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootFolder);
        ArgumentNullException.ThrowIfNull(fileNameRegex);

        var results = new List<FileScanResult>();
        var counters = new ScanCounters();

        ScanDirectory(rootFolder, fileNameRegex, results, counters, cancellationToken);

        results.Sort(static (left, right) => string.Compare(left.FullPath, right.FullPath, StringComparison.OrdinalIgnoreCase));
        return new FileScanSummary(results, counters.ScannedFiles, results.Count, counters.ErrorCount);
    }

    private static void ScanDirectory(
        string directoryPath,
        Regex fileNameRegex,
        List<FileScanResult> results,
        ScanCounters counters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(directoryPath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                counters.ScannedFiles++;

                if (!fileNameRegex.IsMatch(Path.GetFileName(filePath)))
                {
                    continue;
                }

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var encodingName = EncodingDetector.DetectFileEncodingInfo(filePath).DisplayName;
                    results.Add(new FileScanResult(
                        fileInfo.Name,
                        fileInfo.FullName,
                        encodingName,
                        fileInfo.Length,
                        fileInfo.LastWriteTime));
                }
                catch (Exception ex) when (IsExpectedFileSystemException(ex))
                {
                    counters.ErrorCount++;
                }
            }
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            counters.ErrorCount++;
            return;
        }

        try
        {
            foreach (var childDirectory in Directory.EnumerateDirectories(directoryPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (IsReparsePoint(childDirectory, counters))
                {
                    continue;
                }

                ScanDirectory(childDirectory, fileNameRegex, results, counters, cancellationToken);
            }
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            counters.ErrorCount++;
        }
    }

    private static bool IsReparsePoint(string directoryPath, ScanCounters counters)
    {
        try
        {
            return (File.GetAttributes(directoryPath) & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            counters.ErrorCount++;
            return true;
        }
    }

    private static bool IsExpectedFileSystemException(Exception exception)
    {
        return exception is IOException
            or UnauthorizedAccessException
            or SecurityException
            or NotSupportedException;
    }

    private sealed class ScanCounters
    {
        public int ScannedFiles { get; set; }

        public int ErrorCount { get; set; }
    }
}
