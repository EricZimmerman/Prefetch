using System;

namespace Prefetch;

internal static class Helpers
{
    /// <summary>
    ///     Throws <see cref="PrefetchParseException" /> unless [offset, offset + count) lies inside <paramref name="buffer" />.
    /// </summary>
    public static void EnsureRange(byte[] buffer, long offset, long count, string field)
    {
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            throw new PrefetchParseException(
                $"Invalid {field} data (offset {offset}, length {count}, available {buffer.Length}) - is this file corrupt?",
                (int) Math.Min(Math.Max(offset, -1), int.MaxValue), field);
        }
    }

    public static ushort ReadUInt16(byte[] buffer, long offset, string field)
    {
        EnsureRange(buffer, offset, 2, field);
        return BitConverter.ToUInt16(buffer, (int) offset);
    }

    public static int ReadInt32(byte[] buffer, long offset, string field)
    {
        EnsureRange(buffer, offset, 4, field);
        return BitConverter.ToInt32(buffer, (int) offset);
    }

    public static long ReadInt64(byte[] buffer, long offset, string field)
    {
        EnsureRange(buffer, offset, 8, field);
        return BitConverter.ToInt64(buffer, (int) offset);
    }

    public static byte[] Slice(byte[] buffer, long offset, long count, string field)
    {
        EnsureRange(buffer, offset, count, field);
        var b = new byte[count];
        Buffer.BlockCopy(buffer, (int) offset, b, 0, (int) count);
        return b;
    }

    /// <summary>
    ///     Converts a FILETIME to UTC, or returns null when it is outside the range <see cref="DateTimeOffset" /> supports.
    /// </summary>
    public static DateTimeOffset? FromFileTime(long fileTime)
    {
        // DateTime.MaxValue as a FILETIME
        const long maxFileTime = 2650467743999999999;

        if (fileTime < 0 || fileTime > maxFileTime)
        {
            return null;
        }

        return new DateTimeOffset(DateTime.FromFileTimeUtc(fileTime));
    }
}
