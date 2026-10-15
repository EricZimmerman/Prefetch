using System;
using System.IO;
using Prefetch.Versions;
using Prefetch.XpressStream;
using Serilog;


namespace Prefetch;

public class PrefetchFile
{
    private const int Signature = 0x41434353;

    // Real prefetch files are at most a few MB; these stop a corrupt size field or a large non-prefetch
    // stream from forcing a huge allocation
    private const uint MaxDecompressedSize = 64 * 1024 * 1024;
    private const long MaxFileSize = 64 * 1024 * 1024;

    public static void SavePrefetch(string file, IPrefetch pf)
    {
        Log.Debug("Writing raw bytes to {File}",file);
        File.WriteAllBytes(file, pf.RawBytes);
    }

    /// <summary>
    ///     Reads a prefetch file from <paramref name="stream" />, decompressing it first if it is a Windows 10/11 (MAM) file.
    /// </summary>
    /// <param name="stream">Readable, seekable stream positioned at the start of the file.</param>
    /// <param name="file">Name to report in <see cref="IPrefetch.SourceFilename" />; source timestamps come from it if it exists on disk.</param>
    /// <exception cref="PrefetchParseException">
    ///     The data is not a prefetch file, uses an unknown format version, or cannot be decompressed.
    ///     Problems after that are reported in <see cref="IPrefetch.ParseErrors" /> instead.
    /// </exception>
    public static IPrefetch Open(Stream stream, string file)
    {
        var rawBytes = ReadPrefetchBytes(stream);

        if (IsCompressed(rawBytes))
        {
            //windows 10/11, so we need to decompress
            Log.Debug("Found Windows 1x prefetch file. Decompressing {File}",file);
            rawBytes = Decompress(rawBytes);
            CheckSignature(rawBytes);
        }

        //at this point we have prefetch bytes we can process

        var fileVer = (Version) BitConverter.ToInt32(rawBytes, 0);

        return fileVer switch
        {
            Version.WinXpOrWin2K3 => new Version17(rawBytes, file),
            Version.VistaOrWin7 => new Version23(rawBytes, file),
            Version.Win8xOrWin2012x => new Version26(rawBytes, file),
            Version.Win10OrWin11 or Version.Win11 => new Version30or31(rawBytes, file),
            _ => throw new PrefetchParseException($"Unknown version '{fileVer:X}'", 0, "Version")
        };
    }

    public static IPrefetch Open(string file)
    {
        Log.Debug("Opening {File}",file);
        using var fs = new FileStream(file,FileMode.Open,FileAccess.Read);
        return Open(fs,file);
    }

    /// <summary>
    ///     Reads the rest of <paramref name="stream" />, checking the first 8 bytes before reading everything else so
    ///     that non-prefetch data (e.g. when probing alternate data streams) is rejected cheaply.
    /// </summary>
    private static byte[] ReadPrefetchBytes(Stream stream)
    {
        var length = stream.Length - stream.Position;

        if (length < 8)
        {
            throw new PrefetchParseException($"File is too small to be a prefetch file ({length} bytes)", 0, "Header");
        }

        if (length > MaxFileSize)
        {
            throw new PrefetchParseException($"File size {length} is larger than the {MaxFileSize} byte limit", 0, "Header");
        }

        var rawBytes = new byte[length];
        ReadExactly(stream, rawBytes, 0, 8);

        if (!IsCompressed(rawBytes))
        {
            CheckSignature(rawBytes);
        }

        ReadExactly(stream, rawBytes, 8, rawBytes.Length - 8);

        return rawBytes;
    }

    private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
    {
        // Read can return fewer bytes than asked for
        var read = 0;
        while (read < count)
        {
            var n = stream.Read(buffer, offset + read, count - read);
            if (n == 0)
            {
                throw new PrefetchParseException($"Stream ended after {offset + read} of {buffer.Length} bytes", offset + read);
            }

            read += n;
        }
    }

    private static bool IsCompressed(byte[] rawBytes)
    {
        return rawBytes[0] == 'M' && rawBytes[1] == 'A' && rawBytes[2] == 'M';
    }

    private static void CheckSignature(byte[] rawBytes)
    {
        if (rawBytes.Length < 8)
        {
            throw new PrefetchParseException($"File is too small to be a prefetch file ({rawBytes.Length} bytes)", 0, "Header");
        }

        if (BitConverter.ToInt32(rawBytes, 4) != Signature)
        {
            throw new PrefetchParseException("Invalid signature! Should be 'SCCA'", 4, "Signature");
        }
    }

    /// <summary>
    ///     Decompresses a MAM file: "MAM", a compression byte, the decompressed size (UInt32), an optional CRC32,
    ///     then the compressed data.
    /// </summary>
    private static byte[] Decompress(byte[] rawBytes)
    {
        // Low nibble is the compression format (4 = XPRESS Huffman); the high bit means a CRC32 follows the size
        var format = rawBytes[3] & 0x0F;
        var hasChecksum = (rawBytes[3] & 0x80) != 0;

        if (format != 4)
        {
            throw new PrefetchParseException($"Unsupported MAM compression format {format}", 3, "Compression format");
        }

        var size = BitConverter.ToUInt32(rawBytes, 4);
        if (size > MaxDecompressedSize)
        {
            throw new PrefetchParseException($"Decompressed size {size} is larger than the {MaxDecompressedSize} byte limit", 4,
                "Decompressed size");
        }

        var dataOffset = hasChecksum ? 12 : 8;
        if (rawBytes.Length < dataOffset)
        {
            throw new PrefetchParseException("Compressed file is truncated", rawBytes.Length, "Compressed data");
        }

        var compressed = new byte[rawBytes.Length - dataOffset];
        Buffer.BlockCopy(rawBytes, dataOffset, compressed, 0, compressed.Length);

        try
        {
            return Xpress2.Decompress(compressed, size);
        }
        catch (InvalidDataException ex)
        {
            throw new PrefetchParseException($"Decompression failed: {ex.Message}", dataOffset, "Compressed data", ex);
        }
    }
}
