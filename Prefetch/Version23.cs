using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
    public class Version23 : IPrefetch
    {
        public byte[] RawBytes { get; }

        public string SourceFilename { get; }
        public Header Header { get; }

        public int FileMetricsOffset { get; }
        public int FileMetricsCount { get; }
        public int TraceChainsOffset { get; }
        public int TraceChainsCount { get; }
        public int FilenameStringsOffset { get; }
        public int FilenameStringsSize { get; }
        public int VolumesInfoOffset { get; }
        public int VolumeCount { get; }
        public int VolumesInfoSize { get; }
        public List<DateTimeOffset> LastRunTimes { get; }
        public int RunCount { get; }
        public List<string> Filenames { get; }
        public string VolumeDeviceName { get; }
        public DateTimeOffset VolumeCreatedOn { get; }
        public string VolumeSerialNumber { get; }
        public List<MFTInformation> FileReferences { get; }
        public List<string> DirectoryNames { get; }



        public Version23(byte[] rawBytes, string sourceFilename)
        {
            SourceFilename = sourceFilename;

            RawBytes = rawBytes;

            Header = new Header(rawBytes.Take(84).ToArray());



        }
    }
}
