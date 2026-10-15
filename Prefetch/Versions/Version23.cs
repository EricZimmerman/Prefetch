namespace Prefetch.Versions;

/// <summary>
///     Format version 23: Windows Vista and Windows 7.
/// </summary>
public class Version23 : PrefetchBase
{
    private static readonly Layout FormatLayout = new()
    {
        FileInformationSize = 156,
        HasTotalDirectoryCount = true,
        LastRunTimesOffset = 44,
        LastRunTimesCount = 1,
        RunCountOffset = 68,
        RunCountMayMove = false,
        FileMetricSize = 32,
        TraceChainSize = 12,
        VolumeEntrySize = 104
    };

    public Version23(byte[] rawBytes, string sourceFilename) : base(rawBytes, sourceFilename, FormatLayout)
    {
    }
}
