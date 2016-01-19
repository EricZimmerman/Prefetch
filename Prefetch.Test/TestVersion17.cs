using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion17
    {
        [Test]
        public void Windows2k3ShouldHaveVersionNumber17()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.Win2k3Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
            }
        }

        [Test]
        public void WindowsXPShouldHaveVersionNumber17()
        {
            foreach (var file in Directory.GetFiles(TestPrefetchMain.WinXpPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
            }
        }
    }
}