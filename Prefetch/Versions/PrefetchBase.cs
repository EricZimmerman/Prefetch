using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prefetch.Other;
using Serilog;

namespace Prefetch.Versions;

/// <summary>
///     Parsing shared by every prefetch format version. The versions lay out the same structures with different
///     sizes; <see cref="Layout" /> holds those differences.
/// </summary>
/// <remarks>
///     The constructor never throws. Each section is parsed independently, so a corrupt section leaves its list
///     empty (or partly filled) while the others are still parsed. Every problem is added to
///     <see cref="ParseErrors" /> and sets <see cref="ParsingError" />.
/// </remarks>
public abstract class PrefetchBase : IPrefetch
{
    private const int HeaderSize = 84;

    private protected PrefetchBase(byte[] rawBytes, string sourceFilename, Layout layout)
    {
        SourceFilename = sourceFilename;
        RawBytes = rawBytes ?? throw new ArgumentNullException(nameof(rawBytes));

        try
        {
            var fi = new FileInfo(sourceFilename);
            SourceCreatedOn = new DateTimeOffset(fi.CreationTimeUtc);
            SourceModifiedOn = new DateTimeOffset(fi.LastWriteTimeUtc);
            SourceAccessedOn = new DateTimeOffset(fi.LastAccessTimeUtc);
        }
        catch (Exception)
        {
            // Not a file on disk (e.g. opened from a stream with a display name)
        }

        // The header and file information block hold every offset and count, so nothing else can be parsed without them
        if (!TryParse("Header", () => Header = new Header(Helpers.Slice(rawBytes, 0, HeaderSize, "Header"))) ||
            !TryParse("File information", () => ParseFileInformation(layout)))
        {
            return;
        }

        TryParse("File metrics", () => ParseFileMetrics(layout));
        TryParse("Trace chains", () => ParseTraceChains(layout));
        TryParse("Filename strings", ParseFilenames);
        TryParse("Volume information", () => ParseVolumes(layout));
    }

    public byte[] RawBytes { get; }

    public string SourceFilename { get; }
    public DateTimeOffset SourceCreatedOn { get; }
    public DateTimeOffset SourceModifiedOn { get; }
    public DateTimeOffset SourceAccessedOn { get; }
    public Header Header { get; private set; }

    public int FileMetricsOffset { get; private set; }
    public int FileMetricsCount { get; private set; }
    public int TraceChainsOffset { get; private set; }
    public int TraceChainsCount { get; private set; }
    public int FilenameStringsOffset { get; private set; }
    public int FilenameStringsSize { get; private set; }
    public int VolumesInfoOffset { get; private set; }
    public int VolumeCount { get; private set; }
    public int VolumesInfoSize { get; private set; }
    public int TotalDirectoryCount { get; private set; }
    public List<DateTimeOffset> LastRunTimes { get; } = new();
    public List<VolumeInfo> VolumeInformation { get; } = new();
    public int RunCount { get; private set; }

    public bool ParsingError => ParseErrors.Count > 0;

    public List<string> ParseErrors { get; } = new();

    public List<string> Filenames { get; } = new();
    public List<FileMetric> FileMetrics { get; } = new();
    public List<TraceChain> TraceChains { get; } = new();

    private bool TryParse(string section, Action parse)
    {
        try
        {
            parse();
            return true;
        }
        catch (Exception ex)
        {
            AddError($"{section}: {ex.Message}");
            return false;
        }
    }

    private void AddError(string message)
    {
        Log.Error("Error processing {File}. {Message}", SourceFilename, message);
        ParseErrors.Add(message);
    }

    private void ParseFileInformation(Layout layout)
    {
        var info = Helpers.Slice(RawBytes, HeaderSize, layout.FileInformationSize, "File information");

        FileMetricsOffset = BitConverter.ToInt32(info, 0);
        FileMetricsCount = BitConverter.ToInt32(info, 4);

        TraceChainsOffset = BitConverter.ToInt32(info, 8);
        TraceChainsCount = BitConverter.ToInt32(info, 12);

        FilenameStringsOffset = BitConverter.ToInt32(info, 16);
        FilenameStringsSize = BitConverter.ToInt32(info, 20);

        VolumesInfoOffset = BitConverter.ToInt32(info, 24);
        VolumeCount = BitConverter.ToInt32(info, 28);
        VolumesInfoSize = BitConverter.ToInt32(info, 32);

        // Version 17 has no directory count (its last run time is at offset 36)
        TotalDirectoryCount = layout.HasTotalDirectoryCount ? BitConverter.ToInt32(info, 36) : -1;

        for (var i = 0; i < layout.LastRunTimesCount; i++)
        {
            var rawTime = BitConverter.ToInt64(info, layout.LastRunTimesOffset + i * 8);

            // Versions with a single run time always report it; with 8 slots, unused ones are 0
            if (rawTime == 0 && layout.LastRunTimesCount > 1)
            {
                continue;
            }

            var runTime = Helpers.FromFileTime(rawTime);

            if (runTime == null)
            {
                AddError($"Last run time {i} is out of range (0x{rawTime:X})");
                continue;
            }

            LastRunTimes.Add(runTime.Value);
        }

        RunCount = BitConverter.ToInt32(info, layout.RunCountOffset);

        // Newer Windows 10/11 builds moved the run count back 8 bytes; the old location then holds a nonzero value 4 bytes before it
        if (layout.RunCountMayMove && BitConverter.ToInt32(info, layout.RunCountOffset - 4) != 0)
        {
            RunCount = BitConverter.ToInt32(info, layout.RunCountOffset - 8);
        }
    }

    private void ParseFileMetrics(Layout layout)
    {
        var bytes = Helpers.Slice(RawBytes, FileMetricsOffset, (long) FileMetricsCount * layout.FileMetricSize,
            "File metrics");

        for (var i = 0; i < FileMetricsCount; i++)
        {
            FileMetrics.Add(new FileMetric(Helpers.Slice(bytes, i * layout.FileMetricSize, layout.FileMetricSize, "File metric"),
                layout.FileMetricSize == 20));
        }
    }

    private void ParseTraceChains(Layout layout)
    {
        var bytes = Helpers.Slice(RawBytes, TraceChainsOffset, (long) TraceChainsCount * layout.TraceChainSize,
            "Trace chains");

        for (var i = 0; i < TraceChainsCount; i++)
        {
            TraceChains.Add(new TraceChain(Helpers.Slice(bytes, i * layout.TraceChainSize, layout.TraceChainSize, "Trace chain"),
                layout.TraceChainSize == 8));
        }
    }

    private void ParseFilenames()
    {
        Helpers.EnsureRange(RawBytes, FilenameStringsOffset, FilenameStringsSize, "Filename strings");

        var raw = Encoding.Unicode.GetString(RawBytes, FilenameStringsOffset, FilenameStringsSize);
        Filenames.AddRange(raw.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries));
    }

    private void ParseVolumes(Layout layout)
    {
        var volumes = Helpers.Slice(RawBytes, VolumesInfoOffset, VolumesInfoSize, "Volume information");

        for (var j = 0; j < VolumeCount; j++)
        {
            var entry = Helpers.Slice(volumes, (long) j * layout.VolumeEntrySize, layout.VolumeEntrySize, $"Volume {j}");

            // Offsets inside a volume entry are relative to the start of the volume information
            var devOffset = BitConverter.ToInt32(entry, 0);
            var devNumChars = BitConverter.ToInt32(entry, 4);
            var rawCreated = BitConverter.ToInt64(entry, 8);
            var serial = BitConverter.ToInt32(entry, 16).ToString("X");

            var devNameBytes = Helpers.Slice(volumes, devOffset, (long) devNumChars * 2, $"Volume {j} device path");
            var devName = Encoding.Unicode.GetString(devNameBytes);

            var created = Helpers.FromFileTime(rawCreated);
            if (created == null)
            {
                AddError($"Volume {j}: creation time is out of range (0x{rawCreated:X})");
            }

            var volume = new VolumeInfo(devOffset, created ?? DateTimeOffset.MinValue, serial, devName);
            VolumeInformation.Add(volume);

            var fileRefOffset = BitConverter.ToInt32(entry, 20);
            var fileRefSize = BitConverter.ToInt32(entry, 24);
            var dirStringsOffset = BitConverter.ToInt32(entry, 28);
            var numDirStrings = BitConverter.ToInt32(entry, 32);

            TryParse($"Volume {j} file references", () => ParseFileReferences(volume, fileRefOffset, fileRefSize));
            TryParse($"Volume {j} directory strings", () => ParseDirectoryStrings(volume, dirStringsOffset, numDirStrings));
        }
    }

    private void ParseFileReferences(VolumeInfo volume, int offset, int size)
    {
        if (size == 0)
        {
            return;
        }

        var refs = Helpers.Slice(RawBytes, (long) VolumesInfoOffset + offset, size, "File references");

        // Version 1 (prefetch version 17) has an 8-byte header; version 3 (23 and later) adds 8 unknown bytes
        var refsVersion = Helpers.ReadInt32(refs, 0, "File references version");
        var count = Helpers.ReadInt32(refs, 4, "File references count");
        var start = refsVersion >= 3 ? 16 : 8;

        Helpers.EnsureRange(refs, start, (long) count * 8, "File references");

        for (var i = 0; i < count; i++)
        {
            volume.FileReferences.Add(new MFTInformation(refs, start + i * 8));
        }
    }

    private void ParseDirectoryStrings(VolumeInfo volume, int offset, int count)
    {
        // Each string is a UInt16 character count, then the UTF-16 characters and a NUL terminator
        long pos = (long) VolumesInfoOffset + offset;

        for (var k = 0; k < count; k++)
        {
            var numChars = Helpers.ReadUInt16(RawBytes, pos, $"Directory string {k}");
            pos += 2;

            var byteCount = numChars * 2 + 2;
            Helpers.EnsureRange(RawBytes, pos, byteCount, $"Directory string {k}");
            volume.DirectoryNames.Add(Encoding.Unicode.GetString(RawBytes, (int) pos, byteCount).Trim('\0'));

            pos += byteCount;
        }
    }

    private protected sealed class Layout
    {
        public int FileInformationSize { get; set; }
        public bool HasTotalDirectoryCount { get; set; }
        public int LastRunTimesOffset { get; set; }
        public int LastRunTimesCount { get; set; }
        public int RunCountOffset { get; set; }
        public bool RunCountMayMove { get; set; }
        public int FileMetricSize { get; set; }
        public int TraceChainSize { get; set; }
        public int VolumeEntrySize { get; set; }
    }
}
