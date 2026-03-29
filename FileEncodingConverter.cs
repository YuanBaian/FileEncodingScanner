using System.Security;
using System.Text;

namespace FileEncodingScanner;

internal sealed record FileConversionResult(
    string FilePath,
    FileScanResult? UpdatedResult,
    string? ErrorMessage)
{
    public bool Succeeded => UpdatedResult is not null;
}

internal static class FileEncodingConverter
{
    public static IReadOnlyList<FileConversionResult> ConvertFiles(
        IReadOnlyList<FileScanResult> sourceResults,
        FileEncodingInfo targetEncoding)
    {
        ArgumentNullException.ThrowIfNull(sourceResults);
        ArgumentNullException.ThrowIfNull(targetEncoding);

        var conversionResults = new List<FileConversionResult>(sourceResults.Count);
        foreach (var sourceResult in sourceResults)
        {
            conversionResults.Add(ConvertFile(sourceResult, targetEncoding));
        }

        return conversionResults;
    }

    private static FileConversionResult ConvertFile(FileScanResult sourceResult, FileEncodingInfo targetEncoding)
    {
        ArgumentNullException.ThrowIfNull(sourceResult);
        ArgumentNullException.ThrowIfNull(targetEncoding);

        var filePath = sourceResult.FullPath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new FileConversionResult(string.Empty, null, "文件路径无效。");
        }

        if (!File.Exists(filePath))
        {
            return new FileConversionResult(filePath, null, "文件不存在。");
        }

        string? temporaryFilePath = null;

        try
        {
            var sourceEncoding = EncodingDetector.DetectFileEncodingInfo(filePath);
            var fileContent = ReadText(filePath, sourceEncoding.Encoding);

            temporaryFilePath = Path.Combine(
                Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("无法确定文件所在目录。"),
                $"{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

            WriteText(temporaryFilePath, fileContent, targetEncoding.Encoding);
            File.Move(temporaryFilePath, filePath, overwrite: true);

            var fileInfo = new FileInfo(filePath);
            return new FileConversionResult(
                filePath,
                sourceResult with
                {
                    EncodingName = targetEncoding.DisplayName,
                    Size = fileInfo.Length,
                    LastWriteTime = fileInfo.LastWriteTime
                },
                null);
        }
        catch (Exception ex) when (IsExpectedConversionException(ex))
        {
            TryDeleteFile(temporaryFilePath);
            return new FileConversionResult(filePath, null, GetLocalizedErrorMessage(ex, targetEncoding));
        }
    }

    private static string ReadText(string filePath, Encoding sourceEncoding)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, sourceEncoding, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void WriteText(string filePath, string content, Encoding targetEncoding)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        var preamble = targetEncoding.GetPreamble();
        if (preamble.Length > 0)
        {
            stream.Write(preamble);
        }

        var bytes = targetEncoding.GetBytes(content);
        stream.Write(bytes);
    }

    private static bool IsExpectedConversionException(Exception exception)
    {
        return exception is IOException
            or UnauthorizedAccessException
            or SecurityException
            or NotSupportedException
            or ArgumentException
            or InvalidOperationException
            or EncoderFallbackException
            or DecoderFallbackException;
    }

    private static string GetLocalizedErrorMessage(Exception exception, FileEncodingInfo targetEncoding)
    {
        return exception switch
        {
            FileNotFoundException => "文件不存在。",
            DirectoryNotFoundException => "文件所在目录不存在。",
            UnauthorizedAccessException => "没有足够的权限访问该文件。",
            EncoderFallbackException => $"文件中包含无法写入 {targetEncoding.DisplayName} 的字符。",
            DecoderFallbackException => "无法按检测到的源编码读取文件内容。",
            InvalidOperationException => exception.Message,
            IOException => exception.Message,
            NotSupportedException => exception.Message,
            ArgumentException => exception.Message,
            _ => exception.Message
        };
    }

    private static void TryDeleteFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch
        {
        }
    }
}
