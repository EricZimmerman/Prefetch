using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion30
    {
        [Test]
        public void TestWin10ChromePfProperties()
        {
            var file = Path.Combine(TestPrefetchMain.Win10Path, @"CHROME.EXE-B3BA7868.pf");
            var pf = Prefetch.Open(file);

            pf.Header.ExecutableFilename.Should().Be("CHROME.EXE");
            pf.Header.Hash.Should().Be("B3BA7868");
            pf.Header.FileSize.Should().Be(116042);
            pf.LastRunTimes[0].Should().Be(DateTimeOffset.Parse("2016-01-13T11: 06:55.3344577-07:00"));
            pf.RunCount.Should().Be(20);

            pf.VolumeCount.Should().Be(1);
            pf.VolumeDeviceName.Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}");
            pf.VolumeSerialNumber.Should().Be("8C9F49EC");
            pf.VolumeCreatedOn.Should().Be(DateTimeOffset.Parse("2015-11-17T13:57:46.2434681-07:00"));

            pf.DirectoryNames.Count.Should().Be(23);
            pf.DirectoryNames[3].Should()
                .Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\PROGRAM FILES (X86)\GOOGLE\CHROME\APPLICATION");

            pf.FileReferences.Count.Should().Be(284);

            pf.Filenames.Count.Should().Be(282);
            pf.Filenames[3].Should().Be(@"\VOLUME{01d1217a9c4c6779-8c9f49ec}\WINDOWS\SYSTEM32\KERNEL32.DLL");

            pf.FileReferences[5].MFTEntryNumber.Should().Be((ulong) 55125);
            pf.FileReferences[5].MFTSequenceNumber.Should().Be(1);

            pf.FileReferences[9].MFTEntryNumber.Should().Be((ulong) 117682);
            pf.FileReferences[9].MFTSequenceNumber.Should().Be(2);
        }

        [Test]
        public void Windows10ShouldHaveVersionNumber30()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win10Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);
                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win10);
            }
        }
    }
}