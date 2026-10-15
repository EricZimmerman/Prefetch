using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Prefetch.Other;
using Prefetch.XpressStream;

namespace Prefetch.Test;

/// <summary>
///     Checks over every sample file, plus in-memory prefetch files that exercise malformed input.
/// </summary>
[TestFixture]
public class RobustnessTests
{
    private const int FileInfoOffset = 84;

    private static IEnumerable<string> AllSamples()
    {
        var root = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        return Directory.GetFiles(root, "*.pf", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(Path.GetDirectoryName(f)) != "Bad");
    }

    [Test]
    public void EverySampleParsesWithoutErrors()
    {
        foreach (var file in AllSamples())
        {
            var pf = PrefetchFile.Open(file);

            pf.ParseErrors.Should().BeEmpty(file);
            pf.ParsingError.Should().BeFalse(file);

            pf.Header.FileSize.Should().Be(pf.RawBytes.Length, file);
            pf.FileMetrics.Count.Should().Be(pf.FileMetricsCount, file);
            pf.TraceChains.Count.Should().Be(pf.TraceChainsCount, file);
            pf.VolumeInformation.Count.Should().Be(pf.VolumeCount, file);
        }
    }

    [Test]
    public void EachFileMetricDescribesItsOwnFilename()
    {
        // Regression: version 30 used to build every metric from the first entry
        foreach (var file in AllSamples())
        {
            var pf = PrefetchFile.Open(file);

            for (var i = 0; i < pf.FileMetrics.Count; i++)
            {
                pf.FileMetrics[i].FilenameStringSize.Should().Be(pf.Filenames[i].Length, $"{file} metric {i}");
            }

            pf.FileMetrics.Select(m => m.FilenameStringOffset).Distinct().Count()
                .Should().Be(pf.FileMetrics.Count, file);
        }
    }

    [Test]
    public void OpenFromStreamReturningOneByteAtATime()
    {
        var file = Path.Combine(TestPrefetchMain.Win10Path, "CHROME.EXE-B3BA7868.pf");
        var expected = PrefetchFile.Open(file);

        using var stream = new TrickleStream(File.ReadAllBytes(file));
        var pf = PrefetchFile.Open(stream, "CHROME.EXE-B3BA7868.pf");

        pf.RawBytes.Should().Equal(expected.RawBytes);
        pf.Filenames.Should().Equal(expected.Filenames);
    }

    [Test]
    public void MftEntryNumberUsesAll48Bits()
    {
        var bytes = new byte[] {0x04, 0x03, 0x02, 0x01, 0x02, 0x00, 0x05, 0x00};
        var mft = new MFTInformation(bytes);

        mft.MFTEntryNumber.Should().Be(0x0002_0102_0304UL);
        mft.MFTSequenceNumber.Should().Be(5);
    }

    [Test]
    public void ExecutableNameFillingTheWholeFieldIsRead()
    {
        var bytes = Build(17);
        var name = new string('X', 30);
        Encoding.Unicode.GetBytes(name).CopyTo(bytes, 16);

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.Header.ExecutableFilename.Should().Be(name);
        pf.ParsingError.Should().BeFalse();
    }

    [TestCase(17)]
    [TestCase(23)]
    public void BuiltFileParses(int version)
    {
        var pf = PrefetchFile.Open(new MemoryStream(Build(version)), "test");

        pf.ParseErrors.Should().BeEmpty();
        pf.Header.ExecutableFilename.Should().Be("TEST.EXE");
        pf.RunCount.Should().Be(7);
        pf.LastRunTimes.Should().Equal(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero));
        pf.Filenames.Should().Equal(@"\VOLUME\TEST.EXE");
        pf.FileMetrics.Should().HaveCount(1);
        pf.TraceChains.Should().HaveCount(1);

        var volume = pf.VolumeInformation.Should().ContainSingle().Subject;
        volume.DeviceName.Should().Be(@"\VOLUME");
        volume.SerialNumber.Should().Be("1234ABCD");
        volume.DirectoryNames.Should().Equal(@"\VOLUME\DIR");

        // Version 3 references (prefetch 23+) have 8 extra header bytes before the array
        var reference = volume.FileReferences.Should().ContainSingle().Subject;
        reference.MFTEntryNumber.Should().Be(1234UL);
        reference.MFTSequenceNumber.Should().Be(2);
    }

    [Test]
    public void TruncatedFileReportsErrorInsteadOfThrowing()
    {
        var bytes = Build(23).Take(FileInfoOffset + 20).ToArray();

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.ParsingError.Should().BeTrue();
        pf.ParseErrors.Should().ContainSingle().Which.Should().StartWith("File information:");
        pf.Header.ExecutableFilename.Should().Be("TEST.EXE");
        pf.Filenames.Should().BeEmpty();
        pf.FileMetrics.Should().BeEmpty();
        pf.VolumeInformation.Should().BeEmpty();
    }

    [Test]
    public void BadSectionOffsetOnlyLosesThatSection()
    {
        var bytes = Build(23);
        BitConverter.GetBytes(int.MaxValue).CopyTo(bytes, FileInfoOffset + 8); // trace chains offset

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.ParseErrors.Should().ContainSingle().Which.Should().StartWith("Trace chains:");
        pf.TraceChains.Should().BeEmpty();
        pf.FileMetrics.Should().HaveCount(1);
        pf.Filenames.Should().HaveCount(1);
        pf.VolumeInformation.Should().HaveCount(1);
    }

    [Test]
    public void HugeCountDoesNotAllocate()
    {
        var bytes = Build(17);
        BitConverter.GetBytes(int.MaxValue).CopyTo(bytes, FileInfoOffset + 4); // file metrics count

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.ParseErrors.Should().ContainSingle().Which.Should().StartWith("File metrics:");
        pf.FileMetrics.Should().BeEmpty();
    }

    [Test]
    public void OutOfRangeRunTimeIsReportedAndSkipped()
    {
        var bytes = Build(17);
        BitConverter.GetBytes(long.MaxValue).CopyTo(bytes, FileInfoOffset + 36);

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.ParseErrors.Should().ContainSingle().Which.Should().Contain("Last run time 0 is out of range");
        pf.LastRunTimes.Should().BeEmpty();
        pf.RunCount.Should().Be(7);
    }

    [Test]
    public void BadDirectoryStringKeepsVolumeAndReferences()
    {
        var bytes = Build(23);
        var volumesOffset = BitConverter.ToInt32(bytes, FileInfoOffset + 24);
        BitConverter.GetBytes(5000).CopyTo(bytes, volumesOffset + 32); // number of directory strings

        var pf = PrefetchFile.Open(new MemoryStream(bytes), "test");

        pf.ParseErrors.Should().ContainSingle().Which.Should().StartWith("Volume 0 directory strings:");
        pf.VolumeInformation.Should().ContainSingle().Which.FileReferences.Should().HaveCount(1);
    }

    [TestCase(new byte[] {})]
    [TestCase(new byte[] {0x11, 0x00, 0x00})]
    public void TooSmallFileThrows(byte[] bytes)
    {
        Action action = () => PrefetchFile.Open(new MemoryStream(bytes), "test");

        action.Should().Throw<PrefetchParseException>().WithMessage("File is too small*");
    }

    [Test]
    public void NonPrefetchStreamIsRejectedAfterReadingEightBytes()
    {
        var stream = new CountingStream(new byte[1024 * 1024]);

        Action action = () => PrefetchFile.Open(stream, "host.txt:Zone.Identifier");

        action.Should().Throw<PrefetchParseException>().WithMessage("Invalid signature*");
        stream.BytesRead.Should().Be(8);
    }

    [Test]
    public void UnknownVersionThrows()
    {
        var bytes = Build(17);
        bytes[0] = 99;

        Action action = () => PrefetchFile.Open(new MemoryStream(bytes), "test");

        action.Should().Throw<PrefetchParseException>().WithMessage("Unknown version '00000063'");
    }

    [Test]
    public void UnsupportedCompressionFormatThrows()
    {
        var bytes = new byte[] {(byte) 'M', (byte) 'A', (byte) 'M', 0x02, 0x10, 0, 0, 0, 0, 0};

        Action action = () => PrefetchFile.Open(new MemoryStream(bytes), "test");

        action.Should().Throw<PrefetchParseException>().WithMessage("Unsupported MAM compression format 2");
    }

    [Test]
    public void HugeDecompressedSizeThrows()
    {
        var bytes = new byte[] {(byte) 'M', (byte) 'A', (byte) 'M', 0x04, 0xFF, 0xFF, 0xFF, 0x7F, 0, 0};

        Action action = () => PrefetchFile.Open(new MemoryStream(bytes), "test");

        action.Should().Throw<PrefetchParseException>().WithMessage("Decompressed size * is larger than *");
    }

    [Test]
    public void TruncatedCompressedDataThrows()
    {
        var file = Path.Combine(TestPrefetchMain.Win10Path, "CMD.EXE-D269B812.pf");
        var bytes = File.ReadAllBytes(file);
        var truncated = bytes.Take(bytes.Length / 2).ToArray();

        Action action = () => PrefetchFile.Open(new MemoryStream(truncated), "test");

        action.Should().Throw<PrefetchParseException>().WithMessage("Decompression failed: *");
    }

    [Test]
    public void XpressHuffmanLiteralThenZeroBitOffsetMatch()
    {
        // 'A' (code 0) then symbol 258 (code 1): match length 2 + 3, offset bits 0 => offset 1
        var data = new byte[260];
        data[32] = 0x10; // symbol 65 ('A'): length 1, high nibble of byte 32
        data[129] = 0x01; // symbol 258: length 1, low nibble of byte 129
        data[256] = 0x00;
        data[257] = 0x40; // bits 0, 1

        Encoding.ASCII.GetString(Xpress2.Decompress(data, 6)).Should().Be("AAAAAA");
    }

    [Test]
    public void XpressHuffmanMatchWithOffsetBits()
    {
        // 'A' = 0, 'B' = 10, symbol 273 = 11 (length 1 + 3, 1 offset bit), offset bit 0 => offset 2
        var data = new byte[260];
        data[32] = 0x10; // symbol 65: length 1
        data[33] = 0x02; // symbol 66: length 2
        data[136] = 0x20; // symbol 273: length 2
        data[256] = 0x00;
        data[257] = 0x58; // bits 0 10 11 0

        Encoding.ASCII.GetString(Xpress2.Decompress(data, 6)).Should().Be("ABABAB");
    }

    /// <summary>
    ///     Builds a minimal version 17 or 23 prefetch file with one of everything.
    /// </summary>
    private static byte[] Build(int version)
    {
        var v17 = version == 17;
        var fileInfoSize = v17 ? 68 : 156;
        var metricSize = v17 ? 20 : 32;
        var volumeEntrySize = v17 ? 40 : 104;

        var metricsOffset = FileInfoOffset + fileInfoSize;
        var traceOffset = metricsOffset + metricSize;
        var stringsOffset = traceOffset + 12;
        var strings = Encoding.Unicode.GetBytes("\\VOLUME\\TEST.EXE\0");
        var volumesOffset = stringsOffset + strings.Length;

        // Volume information: entry, device path, file references, directory strings
        var device = Encoding.Unicode.GetBytes("\\VOLUME");
        var devOffset = volumeEntrySize;
        var refsOffset = devOffset + device.Length + 2;
        var refsHeader = v17 ? 8 : 16;
        var refsSize = refsHeader + 8;
        var dirsOffset = refsOffset + refsSize;
        var dir = Encoding.Unicode.GetBytes("\\VOLUME\\DIR\0");
        var volumesSize = dirsOffset + 2 + dir.Length;

        var bytes = new byte[volumesOffset + volumesSize];

        void Int(int offset, int value) => BitConverter.GetBytes(value).CopyTo(bytes, offset);

        Int(0, version);
        Encoding.ASCII.GetBytes("SCCA").CopyTo(bytes, 4);
        Int(12, bytes.Length);
        Encoding.Unicode.GetBytes("TEST.EXE").CopyTo(bytes, 16);
        Int(76, 0x12345678);

        Int(FileInfoOffset, metricsOffset);
        Int(FileInfoOffset + 4, 1);
        Int(FileInfoOffset + 8, traceOffset);
        Int(FileInfoOffset + 12, 1);
        Int(FileInfoOffset + 16, stringsOffset);
        Int(FileInfoOffset + 20, strings.Length);
        Int(FileInfoOffset + 24, volumesOffset);
        Int(FileInfoOffset + 28, 1);
        Int(FileInfoOffset + 32, volumesSize);

        var runTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero).ToFileTime();
        BitConverter.GetBytes(runTime).CopyTo(bytes, FileInfoOffset + (v17 ? 36 : 44));
        Int(FileInfoOffset + (v17 ? 60 : 68), 7);

        // File metric: filename offset and character count
        Int(metricsOffset + (v17 ? 8 : 12), 0);
        Int(metricsOffset + (v17 ? 12 : 16), 16);

        strings.CopyTo(bytes, stringsOffset);

        var vol = volumesOffset;
        Int(vol, devOffset);
        Int(vol + 4, device.Length / 2);
        BitConverter.GetBytes(runTime).CopyTo(bytes, vol + 8);
        Int(vol + 16, 0x1234ABCD);
        Int(vol + 20, refsOffset);
        Int(vol + 24, refsSize);
        Int(vol + 28, dirsOffset);
        Int(vol + 32, 1);

        device.CopyTo(bytes, vol + devOffset);

        Int(vol + refsOffset, v17 ? 1 : 3);
        Int(vol + refsOffset + 4, 1);
        if (!v17)
        {
            // Unknown header bytes that used to be read as the first reference
            Int(vol + refsOffset + 8, unchecked((int) 0xDEADBEEF));
        }

        Int(vol + refsOffset + refsHeader, 1234);
        BitConverter.GetBytes((ushort) 2).CopyTo(bytes, vol + refsOffset + refsHeader + 6);

        BitConverter.GetBytes((ushort) (dir.Length / 2 - 1)).CopyTo(bytes, vol + dirsOffset);
        dir.CopyTo(bytes, vol + dirsOffset + 2);

        return bytes;
    }

    private sealed class CountingStream : MemoryStream
    {
        public CountingStream(byte[] buffer) : base(buffer)
        {
        }

        public long BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var n = base.Read(buffer, offset, count);
            BytesRead += n;
            return n;
        }
    }

    /// <summary>
    ///     Returns at most one byte per Read call.
    /// </summary>
    private sealed class TrickleStream : MemoryStream
    {
        public TrickleStream(byte[] buffer) : base(buffer)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, Math.Min(count, 1));
        }
    }
}
