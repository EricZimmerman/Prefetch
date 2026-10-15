namespace Prefetch.Versions;

/// <summary>
///     Format version 26: Windows 8.0, Windows 8.1 and Windows Server 2012 (R2).
/// </summary>
public class Version26 : PrefetchBase
{
    private static readonly Layout FormatLayout = new()
    {
        FileInformationSize = 224,
        HasTotalDirectoryCount = true,
        LastRunTimesOffset = 44,
        LastRunTimesCount = 8,
        RunCountOffset = 124,
        RunCountMayMove = false,
        FileMetricSize = 32,
        TraceChainSize = 12,
        VolumeEntrySize = 104
    };

    public Version26(byte[] rawBytes, string sourceFilename) : base(rawBytes, sourceFilename, FormatLayout)
    {
    }
}
