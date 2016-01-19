using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion23
    {
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