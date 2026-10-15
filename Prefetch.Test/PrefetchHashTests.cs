using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Prefetch.Test;

[TestFixture]
public class PrefetchHashTests
{
    // Hosting processes and packaged (Store) apps hash part of their command line or package, so no loaded path
    // reproduces their hash
    private static readonly HashSet<string> HostingProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "DLLHOST.EXE", "MMC.EXE", "TASKHOST.EXE", "WWAHOST.EXE", "LIVECOMM.EXE", "CALCULATOR.EXE"
    };

    [Test]
    public void HashOfExecutablePathMatchesHeaderForEverySample()
    {
        var root = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        var checkedFiles = 0;

        foreach (var file in Directory.GetFiles(root, "*.pf", SearchOption.AllDirectories)
                     .Where(f => Path.GetFileName(Path.GetDirectoryName(f)) != "Bad"))
        {
            var pf = PrefetchFile.Open(file);

            if (HostingProcesses.Contains(pf.Header.ExecutableFilename))
            {
                continue;
            }

            var match = pf.Filenames
                .Where(n => Path.GetFileName(n.Replace('\\', '/')).StartsWith(pf.Header.ExecutableFilename, StringComparison.OrdinalIgnoreCase))
                .SelectMany(DevicePaths)
                .FirstOrDefault(p => PrefetchHash.Calculate(p, pf.Header.Version) == pf.Header.HashValue);

            match.Should().NotBeNull($"{file} should contain a path hashing to {pf.Header.Hash}");
            checkedFiles++;
        }

        checkedFiles.Should().BeGreaterThan(40);
    }

    [TestCase(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\CMD.EXE", 0x087B4001u)]
    [TestCase(@"\device\harddiskvolume1\windows\system32\cmd.exe", 0x087B4001u)]
    public void XpHash(string path, uint expected)
    {
        PrefetchHash.CalculateXp(path).Should().Be(expected);
        PrefetchHash.Calculate(path, Version.WinXpOrWin2K3).Should().Be(expected);
    }

    [TestCase(@"\DEVICE\HARDDISKVOLUME2\WINDOWS\SYSTEM32\CMD.EXE", 0x4A81B364u)]
    [TestCase(@"\DEVICE\HARDDISKVOLUME1\WINDOWS\SYSTEM32\CMD.EXE", 0x89305D47u)]
    public void VistaHash(string path, uint expected)
    {
        PrefetchHash.CalculateVista(path).Should().Be(expected);
        PrefetchHash.Calculate(path, Version.Win10OrWin11).Should().Be(expected);
    }

    [Test]
    public void HeaderHashValueMatchesHashString()
    {
        var pf = PrefetchFile.Open(Path.Combine(TestPrefetchMain.Win2k3Path, "CMD.EXE-087B4001.pf"));

        pf.Header.HashValue.Should().Be(0x087B4001u);
        pf.Header.Hash.Should().Be("87B4001");
    }

    /// <summary>
    ///     Windows 10/11 store \VOLUME{...} paths; the hash uses the device path, whose volume number isn't recorded.
    /// </summary>
    private static IEnumerable<string> DevicePaths(string path)
    {
        yield return path;

        if (path.StartsWith(@"\VOLUME{", StringComparison.OrdinalIgnoreCase))
        {
            var rest = path.Substring(path.IndexOf('}') + 1);
            for (var n = 1; n <= 10; n++)
            {
                yield return $@"\DEVICE\HARDDISKVOLUME{n}{rest}";
            }
        }
    }
}
