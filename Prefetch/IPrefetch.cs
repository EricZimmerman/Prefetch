using System;
using System.Collections.Generic;
using System.ComponentModel;
using Prefetch.Other;

namespace Prefetch
{
    public enum Version
    {
        [Description("Windows XP or Windows Server 2003")]
        WinXpOrWin2K3 = 17,
        [Description("Windows Vista or Windows 7")]
        VistaOrWin7 = 23,
        [Description("Windows 8.0, Windows 8.1, or Windows Server 2012(R2)")]
        Win8xOrWin2012x = 26,
        [Description("Windows 10")]
        Win10 = 30
    }

    public interface IPrefetch
    {
        byte[] RawBytes { get; }
        string SourceFilename { get; }

        Header Header { get; }

        int FileMetricsOffset { get; }

        int FileMetricsCount { get; }

        int TraceChainsOffset { get; }

        int TraceChainsCount { get; }

        int FilenameStringsOffset { get; }

        int FilenameStringsSize { get; }

        int VolumesInfoOffset { get; }

        int VolumeCount { get; }

        int VolumesInfoSize { get; }

        int TotalDirectoryCount { get; }

        List<DateTimeOffset> LastRunTimes { get; }
        List<VolumeInfo> VolumeInformation { get; }

        int RunCount { get; }

        List<string> Filenames { get; }

        List<FileMetric> FileMetrics { get; }

        List<TraceChain> TraceChains { get; }
    }
}