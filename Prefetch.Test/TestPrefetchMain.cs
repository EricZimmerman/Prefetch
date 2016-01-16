using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test
{
    [TestFixture]
    public class TestPrefetchMain
    {
        const string BadPath = @"..\..\TestFiles\Bad";
        const string Win7Path = @"..\..\TestFiles\Win7";
        const string Win10Path = @"..\..\TestFiles\Win10";
        const string WinXpPath = @"..\..\TestFiles\XPPro";
        const string Win8xPath = @"..\..\TestFiles\Win8x";
        const string Win2k3Path = @"..\..\TestFiles\Win2k3";        
        const string Win2012Path = @"..\..\TestFiles\Win2012";
        const string WinVistaPath = @"..\..\TestFiles\Vista";
        const string Win2012R2Path = @"..\..\TestFiles\Win2012R2";
        private List<string> _allPaths = new List<string>() {
	    Win10Path,WinXpPath,WinVistaPath,Win7Path,Win8xPath,Win2k3Path,Win2012Path,Win2012R2Path

	};
        

        [Test]
        public void InvalidFileShouldThrowException()
        {
            var badFile = Path.Combine(BadPath, "notAPrefetch.pf");
            Action action = () => Prefetch.Open(badFile);

            action.ShouldThrow<Exception>().WithMessage("Invalid signature! Should be 'SCCA'");
        }

        [Test]
        public void Windows10ShouldHaveVersionNumber30()
        {
            foreach (var file in Directory.GetFiles(Win10Path,"*.pf"))
            {
                var pf = Prefetch.Open(file);
                pf.SourceFilename.Should().Be(file);
                pf.Header.Version.Should().Be(Version.Win10);
            }
        }

        [Test]
        public void WindowsXPShouldHaveVersionNumber17()
        {
            foreach (var file in Directory.GetFiles(WinXpPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
            }
        }

        [Test]
        public void Windows2k3ShouldHaveVersionNumber17()
        {
            foreach (var file in Directory.GetFiles(Win2k3Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.WinXpOrWin2K3);
            }
        }

        [Test]
        public void WindowsVistaShouldHaveVersionNumber23()
        {
            foreach (var file in Directory.GetFiles(WinVistaPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.VistaOrWin7);
            }
        }

        [Test]
        public void Windows7ShouldHaveVersionNumber23()
        {
            foreach (var file in Directory.GetFiles(Win7Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.VistaOrWin7);
            }
        }

        [Test]
        public void Windows2012ShouldHaveVersionNumber26()
        {
            foreach (var file in Directory.GetFiles(Win2012Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }

            foreach (var file in Directory.GetFiles(Win2012R2Path, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }
        }



        [Test]
        public void Windows8xShouldHaveVersionNumber26()
        {
            foreach (var file in Directory.GetFiles(Win8xPath, "*.pf"))
            {
                var pf = Prefetch.Open(file);

                pf.SourceFilename.Should().Be(file);

                pf.Header.Version.Should().Be(Version.Win8xOrWin2012x);
            }
        }

        [Test]
        public void SignatureShouldBeSCCA()
        {
            foreach (var allPath in _allPaths)
            {
                foreach (var file in Directory.GetFiles(allPath, "*.pf"))
                {
                    var pf = Prefetch.Open(file);

                    pf.Header.Signature.Should().Be("SCCA");
                }
            }

        }



    }
}
