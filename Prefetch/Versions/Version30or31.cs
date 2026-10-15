namespace Prefetch.Versions;

/// <summary>
///     Format versions 30 and 31: Windows 10 and Windows 11 (decompressed before parsing).
/// </summary>
public class Version30or31 : PrefetchBase
{
    private static readonly Layout FormatLayout = new()
    {
        FileInformationSize = 224,
        HasTotalDirectoryCount = true,
        LastRunTimesOffset = 44,
        LastRunTimesCount = 8,
        RunCountOffset = 124,
        RunCountMayMove = true,
        FileMetricSize = 32,
        TraceChainSize = 8,
        VolumeEntrySize = 96
    };

    public Version30or31(byte[] rawBytes, string sourceFilename) : base(rawBytes, sourceFilename, FormatLayout)
    {
    }
}
