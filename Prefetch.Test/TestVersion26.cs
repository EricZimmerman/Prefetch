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
    }
}