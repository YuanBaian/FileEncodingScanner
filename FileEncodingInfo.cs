using System.Text;
using System.Runtime.InteropServices;

namespace FileEncodingScanner;

internal sealed record FileEncodingInfo(string DisplayName, Encoding Encoding);

internal static class FileEncodings
{
    private static readonly int WindowsAnsiCodePage = checked((int)NativeMethods.GetACP());

    public static FileEncodingInfo Ansi { get; } = new("ANSI", CreateStrictEncoding(WindowsAnsiCodePage));
    public static FileEncodingInfo UnknownAnsi { get; } = new("ANSI/Unknown", CreateStrictEncoding(WindowsAnsiCodePage));
    public static FileEncodingInfo Utf8 { get; } = new("UTF-8", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
    public static FileEncodingInfo Utf8Bom { get; } = new("UTF-8 BOM", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true));
    public static FileEncodingInfo Utf16Le { get; } = new("UTF-16 LE", new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
    public static FileEncodingInfo Utf16Be { get; } = new("UTF-16 BE", new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true));
    public static FileEncodingInfo Utf32Le { get; } = new("UTF-32 LE", new UTF32Encoding(bigEndian: false, byteOrderMark: true, throwOnInvalidCharacters: true));
    public static FileEncodingInfo Utf32Be { get; } = new("UTF-32 BE", new UTF32Encoding(bigEndian: true, byteOrderMark: true, throwOnInvalidCharacters: true));

    private static Encoding CreateStrictEncoding(int codePage)
    {
        return Encoding.GetEncoding(codePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }
}

internal static class NativeMethods
{
    [DllImport("kernel32.dll")]
    internal static extern uint GetACP();
}
