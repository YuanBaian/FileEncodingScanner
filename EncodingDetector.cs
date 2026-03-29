namespace FileEncodingScanner;

internal static class EncodingDetector
{
    private const int SampleSize = 64 * 1024;

    public static string DetectFileEncoding(string filePath)
    {
        return DetectFileEncodingInfo(filePath).DisplayName;
    }

    public static FileEncodingInfo DetectFileEncodingInfo(string filePath)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (stream.Length == 0)
        {
            return FileEncodings.UnknownAnsi;
        }

        var buffer = new byte[(int)Math.Min(stream.Length, SampleSize)];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);
        return DetectEncodingInfo(buffer.AsSpan(0, bytesRead));
    }

    internal static FileEncodingInfo DetectEncodingInfo(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return FileEncodings.UnknownAnsi;
        }

        if (HasPrefix(bytes, 0xEF, 0xBB, 0xBF))
        {
            return FileEncodings.Utf8Bom;
        }

        if (HasPrefix(bytes, 0xFF, 0xFE, 0x00, 0x00))
        {
            return FileEncodings.Utf32Le;
        }

        if (HasPrefix(bytes, 0x00, 0x00, 0xFE, 0xFF))
        {
            return FileEncodings.Utf32Be;
        }

        if (HasPrefix(bytes, 0xFF, 0xFE))
        {
            return FileEncodings.Utf16Le;
        }

        if (HasPrefix(bytes, 0xFE, 0xFF))
        {
            return FileEncodings.Utf16Be;
        }

        if (LooksLikeUtf32Le(bytes))
        {
            return FileEncodings.Utf32Le;
        }

        if (LooksLikeUtf32Be(bytes))
        {
            return FileEncodings.Utf32Be;
        }

        if (LooksLikeUtf16Le(bytes))
        {
            return FileEncodings.Utf16Le;
        }

        if (LooksLikeUtf16Be(bytes))
        {
            return FileEncodings.Utf16Be;
        }

        if (IsValidUtf8(bytes, out var hasMultibyteSequence) && hasMultibyteSequence)
        {
            return FileEncodings.Utf8;
        }

        return FileEncodings.UnknownAnsi;
    }

    private static bool HasPrefix(ReadOnlySpan<byte> bytes, params byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
        {
            return false;
        }

        return bytes[..prefix.Length].SequenceEqual(prefix);
    }

    private static bool LooksLikeUtf16Le(ReadOnlySpan<byte> bytes)
    {
        var pairCount = bytes.Length / 2;
        if (pairCount < 2)
        {
            return false;
        }

        var zeroHighBytes = 0;
        var zeroLowBytes = 0;
        var asciiLowBytes = 0;

        for (var index = 0; index + 1 < bytes.Length; index += 2)
        {
            if (bytes[index] == 0)
            {
                zeroLowBytes++;
            }

            if (bytes[index + 1] == 0)
            {
                zeroHighBytes++;
            }

            if (IsLikelyAsciiTextByte(bytes[index]))
            {
                asciiLowBytes++;
            }
        }

        return zeroHighBytes >= Math.Max(2, pairCount / 2)
            && zeroLowBytes <= Math.Max(1, pairCount / 5)
            && asciiLowBytes >= Math.Max(1, pairCount / 3);
    }

    private static bool LooksLikeUtf16Be(ReadOnlySpan<byte> bytes)
    {
        var pairCount = bytes.Length / 2;
        if (pairCount < 2)
        {
            return false;
        }

        var zeroHighBytes = 0;
        var zeroLowBytes = 0;
        var asciiLowBytes = 0;

        for (var index = 0; index + 1 < bytes.Length; index += 2)
        {
            if (bytes[index] == 0)
            {
                zeroHighBytes++;
            }

            if (bytes[index + 1] == 0)
            {
                zeroLowBytes++;
            }

            if (IsLikelyAsciiTextByte(bytes[index + 1]))
            {
                asciiLowBytes++;
            }
        }

        return zeroHighBytes >= Math.Max(2, pairCount / 2)
            && zeroLowBytes <= Math.Max(1, pairCount / 5)
            && asciiLowBytes >= Math.Max(1, pairCount / 3);
    }

    private static bool LooksLikeUtf32Le(ReadOnlySpan<byte> bytes)
    {
        var codePointCount = bytes.Length / 4;
        if (codePointCount < 2)
        {
            return false;
        }

        var zeroTriplets = 0;
        var asciiLowBytes = 0;

        for (var index = 0; index + 3 < bytes.Length; index += 4)
        {
            if (bytes[index + 1] == 0 && bytes[index + 2] == 0 && bytes[index + 3] == 0)
            {
                zeroTriplets++;
            }

            if (IsLikelyAsciiTextByte(bytes[index]))
            {
                asciiLowBytes++;
            }
        }

        return zeroTriplets >= Math.Max(2, codePointCount * 3 / 4)
            && asciiLowBytes >= Math.Max(1, codePointCount / 2);
    }

    private static bool LooksLikeUtf32Be(ReadOnlySpan<byte> bytes)
    {
        var codePointCount = bytes.Length / 4;
        if (codePointCount < 2)
        {
            return false;
        }

        var zeroTriplets = 0;
        var asciiLowBytes = 0;

        for (var index = 0; index + 3 < bytes.Length; index += 4)
        {
            if (bytes[index] == 0 && bytes[index + 1] == 0 && bytes[index + 2] == 0)
            {
                zeroTriplets++;
            }

            if (IsLikelyAsciiTextByte(bytes[index + 3]))
            {
                asciiLowBytes++;
            }
        }

        return zeroTriplets >= Math.Max(2, codePointCount * 3 / 4)
            && asciiLowBytes >= Math.Max(1, codePointCount / 2);
    }

    private static bool IsLikelyAsciiTextByte(byte value)
    {
        return value is 0x09 or 0x0A or 0x0D || (value >= 0x20 && value <= 0x7E);
    }

    private static bool IsValidUtf8(ReadOnlySpan<byte> bytes, out bool hasMultibyteSequence)
    {
        hasMultibyteSequence = false;

        for (var index = 0; index < bytes.Length; index++)
        {
            var current = bytes[index];
            if (current <= 0x7F)
            {
                continue;
            }

            hasMultibyteSequence = true;

            var continuationCount = current switch
            {
                >= 0xC2 and <= 0xDF => 1,
                >= 0xE0 and <= 0xEF => 2,
                >= 0xF0 and <= 0xF4 => 3,
                _ => -1
            };

            if (continuationCount < 0 || index + continuationCount >= bytes.Length)
            {
                return false;
            }

            for (var offset = 1; offset <= continuationCount; offset++)
            {
                if ((bytes[index + offset] & 0xC0) != 0x80)
                {
                    return false;
                }
            }

            if (continuationCount == 2)
            {
                var next = bytes[index + 1];
                if ((current == 0xE0 && next < 0xA0) || (current == 0xED && next >= 0xA0))
                {
                    return false;
                }
            }

            if (continuationCount == 3)
            {
                var next = bytes[index + 1];
                if ((current == 0xF0 && next < 0x90) || (current == 0xF4 && next >= 0x90))
                {
                    return false;
                }
            }

            index += continuationCount;
        }

        return true;
    }
}
