using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion26
    {
        [Test]
        public void TestWin2012R2RegEditPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win2012R2Path, @"NOTEPAD.EXE-D8414F97.pf");
            var pf = Prefetch.Open(file);

            // Prefetch.DumpToJson(pf, true, @"D:\temp\out.json");

            pf.Header.ExecutableFilename.Should().Be("NOTEPAD.EXE");
            pf.Header.Hash.Should().Be("D8414F97");
            pf.Header.FileSize.Should().Be(15320);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T14: 40:31.2944718-07:00"));
            pf.RunCount.Should().Be(2);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("7450B65F");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T15:21:57.7889266-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(7);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(35);

            pf.Filenames.Count.Should().Be(26);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\ADVAPI32.DLL");

            //For whatever reason the sequence # is 1 for both of these when looking at the entry # in the MFT using X-Ways
            pf.VolumeInformation[0].FileReferences[5].MFTEntryNumber.Should().Be((ulong) 0);
            pf.VolumeInformation[0].FileReferences[5].MFTSequenceNumber.Should().Be(null);

            pf.VolumeInformation[0].FileReferences[1].MFTEntryNumber.Should().Be((ulong) 18972);
            pf.VolumeInformation[0].FileReferences[1].MFTSequenceNumber.Should().Be(null);
        }

        [Test]
        public void TestWin2012RegEditPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win2012Path, @"REGEDIT.EXE-90FEEA06.pf");
            var pf = Prefetch.Open(file);

            pf.Header.ExecutableFilename.Should().Be("REGEDIT.EXE");
            pf.Header.Hash.Should().Be("90FEEA06");
            pf.Header.FileSize.Should().Be(22982);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T14: 36:18.7186980-07:00"));
            pf.RunCount.Should().Be(1);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("2E25F20A");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T15:20:46.1666157-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(12);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\USERS\ADMINISTRATOR\APPDATA\LOCAL");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(62);

            pf.Filenames.Count.Should().Be(42);

            //For whatever reason the sequence # is 1 for both of these when looking at the entry #
            pf.VolumeInformation[0].FileReferences[5].MFTEntryNumber.Should().Be((ulong) 27324);
            pf.VolumeInformation[0].FileReferences[5].MFTSequenceNumber.Should().Be(null);

            pf.VolumeInformation[0].FileReferences[9].MFTEntryNumber.Should().Be((ulong) 29316);
            pf.VolumeInformation[0].FileReferences[9].MFTSequenceNumber.Should().Be(null);
        }

        [Test]
        public void TestWin8ChromePfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win8xPath, @"CALC.EXE-77FDF17F.pf");
            var pf = Prefetch.Open(file);

            pf.Header.ExecutableFilename.Should().Be("CALC.EXE");
            pf.Header.Hash.Should().Be("77FDF17F");
            pf.Header.FileSize.Should().Be(22048);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T14: 10:26.0583417-07:00"));
            pf.RunCount.Should().Be(2);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("C6EE7444");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T15:04:54.3519546-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(7);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(46);

            pf.Filenames.Count.Should().Be(37);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\KERNELBASE.DLL");

            pf.VolumeInformation[0].FileReferences[5].MFTEntryNumber.Should().Be((ulong) 43858);
            pf.VolumeInformation[0].FileReferences[5].MFTSequenceNumber.Should().Be(1);

            pf.VolumeInformation[0].FileReferences[9].MFTEntryNumber.Should().Be((ulong) 46917);
            pf.VolumeInformation[0].FileReferences[9].MFTSequenceNumber.Should().Be(1);
        }

        [Test]
        public void TestWin8CmdPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win8xPath, @"_CMD.EXE-4A81B364.pf");
            var pf = Prefetch.Open(file);

            pf.Header.ExecutableFilename.Should().Be("CMD.EXE");
            pf.Header.Hash.Should().Be("4A81B364");
            pf.Header.FileSize.Should().Be(8590);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T14: 25:41.5341178-07:00"));
            pf.RunCount.Should().Be(2);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeInformation[0].DeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeInformation[0].SerialNumber.Should().Be("A26E529A");
            pf.VolumeInformation[0].CreationTime.Should().Be(DateTimeOffset.Parse("2016-01-16T15:15:38.2977678-07:00"));

            pf.VolumeInformation[0].DirectoryNames.Count.Should().Be(8);
            pf.VolumeInformation[0].DirectoryNames[3].Should()
                .Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\BRANDING\BASEBRD\EN-US");

            pf.VolumeInformation[0].FileReferences.Count.Should().Be(20);

            pf.Filenames.Count.Should().Be(12);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\KERNELBASE.DLL");

            pf.VolumeInformation[0].FileReferences[1].MFTEntryNumber.Should().Be((ulong) 44760);
            pf.VolumeInformation[0].FileReferences[1].MFTSequenceNumber.Should().Be(null);
        }

        [Test]
        public void Windows2012ShouldHaveVersionNumber26()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win2012Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }

            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win2012R2Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }
        }

        [Test]
        public void Windows8xShouldHaveVersionNumber26()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win8xPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                var totalDirs = 0;
                foreach (var volumeInfo in pf.VolumeInformation)
                {
                    totalDirs += volumeInfo.DirectoryNames.Count;
                }

                pf.TotalDirectoryCount.Should().Be(totalDirs);

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }
        }
    }
}