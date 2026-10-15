using System;
using System.IO;

namespace Prefetch.XpressStream;

/// <summary>
///     Managed LZ77+Huffman (XPRESS Huffman) decompressor, as used by Windows 10/11 compressed prefetch files.
///     Implements the plain LZ77+Huffman decompression algorithm from [MS-XCA] section 2.2.4.
/// </summary>
public static class Xpress2
{
    private const int TableBytes = 256;
    private const int SymbolCount = 512;
    private const int MaxCodeLength = 15;
    private const int BlockSize = 65536;

    /// <summary>
    ///     Decompresses <paramref name="buffer" /> into exactly <paramref name="decompressedSize" /> bytes.
    /// </summary>
    /// <exception cref="InvalidDataException">The compressed data is corrupt or truncated.</exception>
    public static byte[] Decompress(byte[] buffer, ulong decompressedSize)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (decompressedSize > int.MaxValue)
        {
            throw new InvalidDataException($"Decompressed size {decompressedSize} is too large");
        }

        var output = new byte[decompressedSize];
        var outPos = 0;
        var inPos = 0;

        var lengths = new byte[SymbolCount];
        var table = new ushort[1 << MaxCodeLength];

        while (outPos < output.Length)
        {
            if (inPos + TableBytes + 4 > buffer.Length)
            {
                throw new InvalidDataException(
                    $"Compressed data ends at offset {buffer.Length} before {output.Length} bytes were produced (got {outPos})");
            }

            // 256 bytes hold 512 4-bit code lengths; the low nibble is the even symbol
            for (var i = 0; i < TableBytes; i++)
            {
                lengths[i * 2] = (byte) (buffer[inPos + i] & 0x0F);
                lengths[i * 2 + 1] = (byte) (buffer[inPos + i] >> 4);
            }

            BuildDecodingTable(lengths, table);

            var pos = inPos + TableBytes;
            uint nextBits = (uint) (ReadUInt16(buffer, pos) << 16) | ReadUInt16(buffer, pos + 2);
            pos += 4;
            var extraBits = 16;

            var blockEnd = Math.Min(outPos + BlockSize, output.Length);

            while (outPos < blockEnd)
            {
                var symbol = table[nextBits >> (32 - MaxCodeLength)];
                Consume(buffer, ref pos, ref nextBits, ref extraBits, lengths[symbol]);

                if (symbol < 256)
                {
                    output[outPos++] = (byte) symbol;
                    continue;
                }

                symbol -= 256;
                var matchLength = symbol & 0x0F;
                var offsetBits = symbol >> 4;

                if (matchLength == 15)
                {
                    if (pos >= buffer.Length)
                    {
                        throw new InvalidDataException($"Match length byte past end of data at offset {pos}");
                    }

                    matchLength = buffer[pos++];

                    if (matchLength == 255)
                    {
                        if (pos + 2 > buffer.Length)
                        {
                            throw new InvalidDataException($"Match length word past end of data at offset {pos}");
                        }

                        matchLength = ReadUInt16(buffer, pos);
                        pos += 2;

                        if (matchLength < 15)
                        {
                            throw new InvalidDataException($"Invalid extended match length {matchLength} at offset {pos - 2}");
                        }

                        matchLength -= 15;
                    }

                    matchLength += 15;
                }

                matchLength += 3;

                // A shift by 32 is a no-op in C#, so a 0-bit offset needs its own case
                var matchOffset = offsetBits == 0 ? 1 : (int) (nextBits >> (32 - offsetBits)) + (1 << offsetBits);
                Consume(buffer, ref pos, ref nextBits, ref extraBits, offsetBits);

                if (matchOffset > outPos)
                {
                    throw new InvalidDataException(
                        $"Match offset {matchOffset} points before the start of the output (position {outPos})");
                }

                // Matches may overlap their own output and may run past the block end; stop at the declared size
                var copyEnd = Math.Min(outPos + matchLength, output.Length);
                while (outPos < copyEnd)
                {
                    output[outPos] = output[outPos - matchOffset];
                    outPos++;
                }
            }

            // Any bits still buffered are discarded; the next block's table starts at the next unread byte
            inPos = pos;
        }

        return output;
    }

    private static void Consume(byte[] buffer, ref int pos, ref uint nextBits, ref int extraBits, int count)
    {
        if (count == 0)
        {
            return;
        }

        nextBits <<= count;
        extraBits -= count;

        if (extraBits < 0)
        {
            if (pos + 2 > buffer.Length)
            {
                throw new InvalidDataException($"Bit stream runs past the end of the data at offset {pos}");
            }

            nextBits |= (uint) ReadUInt16(buffer, pos) << -extraBits;
            extraBits += 16;
            pos += 2;
        }
    }

    private static ushort ReadUInt16(byte[] buffer, int pos)
    {
        return (ushort) (buffer[pos] | (buffer[pos + 1] << 8));
    }

    /// <summary>
    ///     Builds a canonical Huffman lookup table indexed by the next 15 bits of input.
    /// </summary>
    private static void BuildDecodingTable(byte[] lengths, ushort[] table)
    {
        var next = 0;

        for (var length = 1; length <= MaxCodeLength; length++)
        {
            var span = 1 << (MaxCodeLength - length);

            for (var symbol = 0; symbol < SymbolCount; symbol++)
            {
                if (lengths[symbol] != length)
                {
                    continue;
                }

                if (next + span > table.Length)
                {
                    throw new InvalidDataException("Huffman code lengths are over-subscribed");
                }

                for (var i = 0; i < span; i++)
                {
                    table[next++] = (ushort) symbol;
                }
            }
        }

        if (next == 0)
        {
            throw new InvalidDataException("Huffman table has no symbols");
        }

        // An incomplete code leaves unreachable entries; point them at a symbol with a nonzero length
        // so a corrupt stream can't make the decoder stop consuming input
        for (var i = next; i < table.Length; i++)
        {
            table[i] = table[next - 1];
        }
    }
}
