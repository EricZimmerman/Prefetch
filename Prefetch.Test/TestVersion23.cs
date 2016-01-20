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
            pf.VolumeDeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME1");
            pf.VolumeSerialNumber.Should().Be("E8EAB8B5");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2016-01-16T13:53:13.1093750-07:00"));

            pf.DirectoryNames.Count.Should().Be(13);
            pf.DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\USERS\PUBLIC");

            pf.FileReferences.Count.Should().Be(84);

            pf.Filenames.Count.Should().Be(66);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\ADVAPI32.DLL");

            pf.FileReferences[1].MFTEntryNumber.Should().Be((ulong) 352);
            pf.FileReferences[1].MFTSequenceNumber.Should().Be(null);
        }

        [Test]
        public void TestWin7CalcPfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win7Path, @"CALC.EXE-77FDF17F.pf");
            var pf = Prefetch.Open(file);

            //Prefetch.DumpToJson(pf,true,@"D:\temp\out.json");

            pf.Header.ExecutableFilename.Should().Be("CALC.EXE");
            pf.Header.Hash.Should().Be("77FDF17F");
            pf.Header.FileSize.Should().Be(23538);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-16T13: 27:01.1967500-07:00"));
            pf.RunCount.Should().Be(2);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeDeviceName.Should().Be(@"\DEVICE\HARDDISKVOLUME2");
            pf.VolumeSerialNumber.Should().Be("88008C2F");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2016-01-16T14:15:18.1093750-07:00"));

            pf.DirectoryNames.Count.Should().Be(8);
            pf.DirectoryNames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\GLOBALIZATION\SORTING");

            pf.FileReferences.Count.Should().Be(45);

            pf.Filenames.Count.Should().Be(37);
            pf.Filenames[3].Should().Be(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\KERNELBASE.DLL");

            pf.FileReferences[2].MFTEntryNumber.Should().Be((ulong) 25654);
            pf.FileReferences[2].MFTSequenceNumber.Should().Be(1);
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

                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.VistaOrWin7);
            }
        }
    }
}