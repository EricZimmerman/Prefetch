using System;
using System.Collections.Generic;
using Prefetch.Other;

namespace Prefetch
{
    public enum Version
    {
        WinXpOrWin2K3 = 17,
        VistaOrWin7 = 23,
        Win8xOrWin2012x = 26,
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