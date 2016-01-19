using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestVersion30
    {
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