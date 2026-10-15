using System;

namespace Prefetch;

/// <summary>
///     Calculates the hash that Windows puts in a prefetch file's name and header (<see cref="Other.Header.Hash" />).
/// </summary>
/// <remarks>
///     Windows hashes the executable's upper-case NT device path, e.g. <c>\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\CMD.EXE</c>.
///     Windows 10/11 store <c>\VOLUME{...}</c> paths in the file instead, so the device path has to be rebuilt to check them.
///     Hosting processes (dllhost.exe, mmc.exe, rundll32.exe, ...) and <c>/prefetch:N</c> launches also hash part of the
///     command line, so their hash cannot be reproduced from the path alone.
/// </remarks>
public static class PrefetchHash
{
    /// <summary>
    ///     Calculates the hash of <paramref name="devicePath" /> with the algorithm used by <paramref name="version" />.
    /// </summary>
    public static uint Calculate(string devicePath, Version version)
    {
        return version == Version.WinXpOrWin2K3 ? CalculateXp(devicePath) : CalculateVista(devicePath);
    }

    /// <summary>
    ///     Windows XP and Server 2003 (format version 17).
    /// </summary>
    public static uint CalculateXp(string devicePath)
    {
        if (devicePath == null)
        {
            throw new ArgumentNullException(nameof(devicePath));
        }

        uint hash = 0;

        unchecked
        {
            foreach (var c in devicePath.ToUpperInvariant())
            {
                // UTF-16LE bytes, low byte first
                hash = hash * 37 + (byte) c;
                hash = hash * 37 + (byte) (c >> 8);
            }

            hash *= 314159269;

            if (hash > 0x80000000)
            {
                hash = 0 - hash;
            }
        }

        return hash % 1000000007;
    }

    /// <summary>
    ///     Windows Vista and later (format versions 23 to 31). The unrolled "2008" variant gives the same result.
    /// </summary>
    public static uint CalculateVista(string devicePath)
    {
        if (devicePath == null)
        {
            throw new ArgumentNullException(nameof(devicePath));
        }

        uint hash = 314159;

        unchecked
        {
            foreach (var c in devicePath.ToUpperInvariant())
            {
                hash = hash * 37 + (byte) c;
                hash = hash * 37 + (byte) (c >> 8);
            }
        }

        return hash;
    }
}
