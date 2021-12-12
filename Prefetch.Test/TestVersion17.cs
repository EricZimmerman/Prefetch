using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test;

[TestFixture]
public class TestVersion17
{
    [Test]
    public void Test2k3CmdPfProperties()
    {
        var file = Path.Combine(TestPrefetchMain.Win2k3Path, @"CMD.EXE-087B4001.pf");
        var pf = PrefetchFile.Open(file);

        pf.Header.ExecutableFilename.Should().Be("CMD.EXE");
        pf.Header.Hash.Should().Be("87B4001");
        pf.Header.FileSize.Should().Be(6002);
        pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-15T16: 01:40.8750000-07:00"));
        pf.RunCount.Should().Be(3);

        pf.VolumeCount.Should().Be(1);
        pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME1");
        pf.VolumeInformation[0].SerialNumber.Should().Be("64BB3469");
        pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-15T08:45:15.8906250-07:00"));

        pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(4);
        pf.VolumeInformation[0].DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\");

        pf.VolumeInformation[0].FileReferences.Count.Should().Be(20);

        pf.Filenames.Count.Should().Be(16);
        pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\LOCALE.NLS");

        pf.VolumeInformation[0].FileReferences[5].MFTEntryNumber.Should().Be((ulong) 250);
        pf.VolumeInformation[0].FileReferences[5].MFTSequenceNumber.Should().Be(1);
    }

    [Test]
    public void TestXPCalcPfProperties()
    {
        var file = Path.Combine(TestPrefetchMain.WinXpPath, @"CALC.EXE-02CD573A.pf");
        var pf = PrefetchFile.Open(file);

//            PrefetchFile.DumpToJson(pf,true,@"D:\temp\out.json");

        pf.Header.ExecutableFilename.Should().Be("CALC.EXE");
        pf.Header.Hash.Should().Be("2CD573A");
        pf.Header.FileSize.Should().Be(11332);
        pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-13T15: 05:51.2812500-07:00"));
        pf.RunCount.Should().Be(3);

        pf.VolumeCount.Should().Be(1);
        pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME1");
        pf.VolumeInformation[0].SerialNumber.Should().Be("E0F7E847");
        pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-13T04:17:18.7187500-07:00"));

        pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(6);
        pf.VolumeInformation[0].DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\");

        pf.VolumeInformation[0].FileReferences.Count.Should().Be(36);

        pf.Filenames.Count.Should().Be(30);
        pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\LOCALE.NLS");

        pf.VolumeInformation[0].FileReferences[34].MFTEntryNumber.Should().Be((ulong) 126);
        pf.VolumeInformation[0].FileReferences[34].MFTSequenceNumber.Should().Be(1);
    }

    [Test]
    public void Windows2k3ShouldHaveVersionNumber17()
    {
        foreach (var file in Directory.GetFiles(TestPrefetchMain.Win2k3Path, "*.pf"))
        {
            var pf = PrefetchFile.Open(file);

            pf.SourceFilename.Should().Be(file);
            pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
        }
    }

    [Test]
    public void WindowsXPShouldHaveVersionNumber17()
    {
        foreach (var file in Directory.GetFiles(TestPrefetchMain.WinXpPath, "*.pf"))
        {
            var pf = PrefetchFile.Open(file);

            pf.TotalDirectoryCount.Should().Be(-1);

            pf.SourceFilename.Should().Be(file);
            pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
        }
    }
}