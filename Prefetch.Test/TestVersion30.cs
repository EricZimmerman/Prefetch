using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test;

[TestFixture]
public class TestVersion30
{
    [Test]
    public void TestWin10ChromePfProperties()
    {
        var file = Path.Combine(TestPrefetchMain.Win10Path, @"CHROME.EXE-B3BA7868.pf");
        var pf = PrefetchFile.Open(file);

        pf.Header.ExecutableFilename.Should().Be("CHROME.EXE");
        pf.Header.Hash.Should().Be("B3BA7868");
        pf.Header.FileSize.Should().Be(116042);
        pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-13T11: 06:55.3344577-07:00"));
        pf.RunCount.Should().Be(20);

        pf.VolumeCount.Should().Be(1);
        pf.VolumeInformation[0].DeviceName.Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}");
        pf.VolumeInformation[0].SerialNumber.Should().Be("8C9F49EC");
        pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2015-11-17T13:57:46.2434681-07:00"));

        pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(23);
        pf.VolumeInformation[0].DirectoryNames[3].Should()
            .Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\PROGRAM FILES (X86)\GOOGLE\CHROME\APPLICATION");

        pf.VolumeInformation[0].FileReferences.Count.Should().Be(284);

        pf.LastRunTimes.Count.Should().Be(8);

        pf.Filenames.Count.Should().Be(282);
        pf.Filenames[3].Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\WINDOWS\SYSTEM32\KERNEL32.DLL");

        pf.VolumeInformation[0].FileReferences[5].MFTEntryNumber.Should().Be((ulong) 55125);
        pf.VolumeInformation[0].FileReferences[5].MFTSequenceNumber.Should().Be(1);

        pf.VolumeInformation[0].FileReferences[9].MFTEntryNumber.Should().Be((ulong) 117682);
        pf.VolumeInformation[0].FileReferences[9].MFTSequenceNumber.Should().Be(2);
    }

    [Test]
    public void TestWin10DcodeDecodePfProperties()
    {
        var file = Path.Combine(TestPrefetchMain.Win10Path, @"DCODEDCODEDCODEDCODEDCODEDCOD-E65B9FE8.pf");
        var pf = PrefetchFile.Open(file);

        //PrefetchFile.DumpToJson(pf, true, @"D:\temp\DCODEDCODEDCODEDCODEDCODEDCOD.json");

        pf.Header.ExecutableFilename.Should().Be("DCODEDCODEDCODEDCODEDCODEDCOD");
        pf.Header.Hash.Should().Be("E65B9FE8");
        pf.Header.FileSize.Should().Be(33606);
        pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-13T15: 47:25.7480759-07:00"));
        pf.RunCount.Should().Be(2);

        pf.VolumeCount.Should().Be(2);
        pf.VolumeInformation[0].DeviceName.Should().Be(@"\VOLUME{01d12173f395296c-66f451bc}");
        pf.VolumeInformation[0].SerialNumber.Should().Be("66F451BC");
        pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2015-11-17T13:10:06.2049644-07:00"));

        pf.VolumeInformation[1].DeviceName.Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}");
        pf.VolumeInformation[1].SerialNumber.Should().Be("8C9F49EC");
        pf.VolumeInformation[1].CreationTime.Should().Be(DateTimeOffset.Parse("2015-11-17T13:57:46.2434681-07:00"));


        pf.VolumeInformation[1].DirectoryNames.Count.Should().Be(19);

        pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(1);
        pf.VolumeInformation[0].DirectoryNames[0].Should()
            .Be(@"\VOLUME{01d12173f395296c-66f451bc}\TEMP");

        pf.VolumeInformation[0].FileReferences.Count.Should().Be(2);
        pf.VolumeInformation[1].FileReferences.Count.Should().Be(85);


        pf.Filenames.Count.Should().Be(57);
        pf.Filenames[3].Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\WINDOWS\SYSTEM32\KERNEL32.DLL");

        pf.VolumeInformation[1].FileReferences[12].MFTEntryNumber.Should().Be((ulong) 357876);
        pf.VolumeInformation[1].FileReferences[12].MFTSequenceNumber.Should().Be(1);

        pf.VolumeInformation[0].FileReferences[1].MFTEntryNumber.Should().Be((ulong) 305846);
        pf.VolumeInformation[0].FileReferences[1].MFTSequenceNumber.Should().Be(2);
    }

    [Test]
    public void TestWin10DevEnvPfProperties()
    {
        var file = Path.Combine(TestPrefetchMain.Win10Path, @"DEVENV.EXE-854D7862.pf");
        var pf = PrefetchFile.Open(file);

        //PrefetchFile.DumpToJson(pf, true, @"D:\temp\DEVENV.json");

        pf.Header.ExecutableFilename.Should().Be("DEVENV.EXE");
        pf.Header.Hash.Should().Be("854D7862");
        pf.Header.FileSize.Should().Be(380690);
        pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-13T09: 50:34.6578416-07:00,"));
        pf.RunCount.Should().Be(54);

        pf.VolumeCount.Should().Be(1);
        pf.VolumeInformation[0].DeviceName.Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}");
        pf.VolumeInformation[0].SerialNumber.Should().Be("8C9F49EC");
        pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2015-11-17T13:57:46.2434681-07:00"));

        pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(516);
        pf.VolumeInformation[0].DirectoryNames[3].Should()
            .Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\PROGRAM FILES (X86)\COMMON FILES\MICROSOFT SHARED\MSENV");

        pf.VolumeInformation[0].FileReferences.Count.Should().Be(681);

        pf.Filenames.Count.Should().Be(403);
        pf.Filenames[3].Should()
            .Be(
                @"\VOLUME{01d1217a9c4c6779-8c9f49ec}\PROGRAM FILES (X86)\MICROSOFT VISUAL STUDIO 14.0\COMMON7\IDE\MICROSOFT.VISUALSTUDIO.ACTIVITIES.DLL");

        pf.VolumeInformation[0].FileReferences[6].MFTEntryNumber.Should().Be((ulong) 148922);
        pf.VolumeInformation[0].FileReferences[6].MFTSequenceNumber.Should().Be(1);

        pf.VolumeInformation[0].FileReferences[8].MFTEntryNumber.Should().Be((ulong) 219686);
        pf.VolumeInformation[0].FileReferences[8].MFTSequenceNumber.Should().Be(2);
    }

    [Test]
    public void Windows10ShouldHaveVersionNumber30()
    {
        foreach (var file in Directory.GetFiles(TestPrefetchMain.Win10Path, "*.pf"))
        {
            var pf = PrefetchFile.Open(file);

            var totalDirs = 0;
            foreach (var volumeInfo in pf.VolumeInformation)
            {
                totalDirs += volumeInfo.DirectoryNames.Count;
            }

            pf.TotalDirectoryCount.Should().Be(totalDirs);


            pf.SourceFilename.Should().Be(file);
            pf.Header.Version.Should().Be(Version.Win10OrWin11);
        }
    }
}