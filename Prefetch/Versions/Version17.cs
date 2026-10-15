namespace Prefetch.Versions;

/// <summary>
///     Format version 17: Windows XP and Windows Server 2003.
/// </summary>
public class Version17 : PrefetchBase
{
    private static readonly Layout FormatLayout = new()
    {
        FileInformationSize = 68,
        HasTotalDirectoryCount = false,
        LastRunTimesOffset = 36,
        LastRunTimesCount = 1,
        RunCountOffset = 60,
        RunCountMayMove = false,
        FileMetricSize = 20,
        TraceChainSize = 12,
        VolumeEntrySize = 40
    };

    public Version17(byte[] rawBytes, string sourceFilename) : base(rawBytes, sourceFilename, FormatLayout)
    {
    }
}
