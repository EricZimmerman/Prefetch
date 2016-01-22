using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion23
    {
        [Test]
        public void TestVistaExplorerPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.WinVistaPath, @"EXPLORER.EXE-7A3328DA.pf");
            var pf = Prefetch.Open(file);

            pf.Header.ExecutableFilename.Should().Be("EXPLORER.EXE");
            pf.Header.Hash.Should().Be("7A3328DA");
            pf.Header.FileSize.Should().Be(38470);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T13: 02:00.8326765-07:00"));
            pf.RunCount.Should().Be(1);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME1");
            pf.VolumeInformation[0].SerialNumber.Should().Be("E8EAB8B5");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T13:53:13.1093750-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(13);
            pf.VolumeInformation[0].DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\USERS\PUBLIC");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(84);

            pf.Filenames.Count.Should().Be(66);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\ADVAPI32.DLL");

            pf.VolumeInformation[0].FileReferences[1].MFTEntryNumber.Should().Be((ulong) 352);
            pf.VolumeInformation[0].FileReferences[1].MFTSequenceNumber.Should().Be(null);
        }

        [Test]
        public void TestWin7DCodePfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win7Path, @"DCODEDCODEDCODEDCODEDCODEDCOD-9054DA3F.pf");
            var pf = Prefetch.Open(file);

           // Prefetch.DumpToJson(pf, true, @"D:\temp\win7DCODEDCODEDCODEDCODEDCODEDCOD.json");

            pf.Header.ExecutableFilename.Should().Be("DCODEDCODEDCODEDCODEDCODEDCOD");
            pf.Header.Hash.Should().Be("9054DA3F");
            pf.Header.FileSize.Should().Be(29746);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-22T09: 23:16.3416250-07:00"));
            pf.RunCount.Should().Be(5);

            pf.VolumeCount.Should().Be(2);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("88008C2F");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T14:15:18.1093750-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(14);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\USERS\E\APPDATA\LOCAL");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(63);

            pf.VolumeInformation[1].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME3");
            pf.VolumeInformation[1].SerialNumber.Should().Be("E892367F");
            pf.VolumeInformation[1].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-22T09:11:36.5781250-07:00"));

            pf.VolumeInformation[1].DirectoryNames.Count.Should().Be(2);
            pf.VolumeInformation[1].DirectoryNames[1].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME3\TEMP\222");

            pf.VolumeInformation[1].FileReferences.Count.Should().Be(3);

            pf.Filenames.Count.Should().Be(50);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\WOW64CPU.DLL");

            pf.VolumeInformation[1].FileReferences[1].MFTEntryNumber.Should().Be((ulong)37);
            pf.VolumeInformation[1].FileReferences[1].MFTSequenceNumber.Should().Be(1);
        }

        [Test]
        public void TestWin7CalcPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win7Path, @"CALC.EXE-77FDF17F.pf");
            var pf = Prefetch.Open(file);

            //Prefetch.DumpToJson(pf, true, @"D:\temp\win7calc.json");

            pf.Header.ExecutableFilename.Should().Be("CALC.EXE");
            pf.Header.Hash.Should().Be("77FDF17F");
            pf.Header.FileSize.Should().Be(23538);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T13: 27:01.1967500-07:00"));
            pf.RunCount.Should().Be(2);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("88008C2F");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T14:15:18.1093750-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(8);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(45);

            pf.Filenames.Count.Should().Be(37);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\KERNELBASE.DLL");

            pf.VolumeInformation[0].FileReferences[2].MFTEntryNumber.Should().Be((ulong) 25654);
            pf.VolumeInformation[0].FileReferences[2].MFTSequenceNumber.Should().Be(1);
        }

        [Test]
        public void Windows7ShouldHaveVersionNumber23()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win7Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.VistaOrWin7);
            }
        }

        [Test]
        public void WindowsVistaShouldHaveVersionNumber23()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.WinVistaPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                var totalDirs = 0;
                foreach (var volumeInfo in pf.VolumeInformation)
                {
                    totalDirs += volumeInfo.DirectoryNames.Count;
                }

                pf.TotalDirectoryCount.Should().Be(totalDirs);

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.VistaOrWin7);
            }
        }
    }
}