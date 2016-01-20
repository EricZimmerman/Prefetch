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

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }
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
            pf.VolumeDeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeSerialNumber.Should().Be("C6EE7444");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2016-01-16T15:04:54.3519546-07:00"));

            pf.DirectoryNames.Count.Should().Be(7);
            pf.DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.FileReferences.Count.Should().Be(46);

            pf.Filenames.Count.Should().Be(37);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\KERNELBASE.DLL");

            pf.FileReferences[5].MFTEntryNumber.Should().Be((ulong)43858);
            pf.FileReferences[5].MFTSequenceNumber.Should().Be(1);

            pf.FileReferences[9].MFTEntryNumber.Should().Be((ulong)46917);
            pf.FileReferences[9].MFTSequenceNumber.Should().Be(1);

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
            pf.VolumeDeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeSerialNumber.Should().Be("2E25F20A");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2016-01-16T15:20:46.1666157-07:00"));

            pf.DirectoryNames.Count.Should().Be(12);
            pf.DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\USERS\ADMINISTRATOR\APPDATA\LOCAL");

            pf.FileReferences.Count.Should().Be(62);

            pf.Filenames.Count.Should().Be(42);

            //For whatever reason the sequence # is 1 for both of these when looking at the entry #
            pf.FileReferences[5].MFTEntryNumber.Should().Be((ulong)27324);
            pf.FileReferences[5].MFTSequenceNumber.Should().Be(null);

            pf.FileReferences[9].MFTEntryNumber.Should().Be((ulong)29316);
            pf.FileReferences[9].MFTSequenceNumber.Should().Be(null);

        }

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
            pf.VolumeDeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeSerialNumber.Should().Be("7450B65F");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2016-01-16T15:21:57.7889266-07:00"));

            pf.DirectoryNames.Count.Should().Be(7);
            pf.DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.FileReferences.Count.Should().Be(35);

            pf.Filenames.Count.Should().Be(26);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\ADVAPI32.DLL");

            //For whatever reason the sequence # is 1 for both of these when looking at the entry # in the MFT using X-Ways
            pf.FileReferences[5].MFTEntryNumber.Should().Be((ulong)0);
            pf.FileReferences[5].MFTSequenceNumber.Should().Be(null);

            pf.FileReferences[1].MFTEntryNumber.Should().Be((ulong)18972);
            pf.FileReferences[1].MFTSequenceNumber.Should().Be(null);

        }
    }
}