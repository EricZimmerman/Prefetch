using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
   public class Version30:IPrefetch
    {
        public byte[] RawBytes { get; }
        public Version Version { get; private set; }
        public string Signature { get; private set; }
        public string SourceFilename { get; }
        public int FileSize { get; }
        public string ExecutableFilename { get; }
        public string Hash { get; }
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
       public void SavePrefetch(string file)
       {
        System.IO.File.WriteAllBytes(file,RawBytes);
       }

       public Version30(byte[] rawBytes,string sourceFilename)
       {
           SourceFilename = sourceFilename;
           RawBytes = rawBytes;
           Version = Version.Win10;
           Signature = "SCCA";

            var index = 0;

            index += 4; //version
            index += 4; //signature
            index += 4; //unknown

            FileSize = BitConverter.ToInt32(rawBytes, index);
            index += 4;

            var tempName = Encoding.Unicode.GetString(rawBytes, index, 60);
            ExecutableFilename = tempName.Substring(0, tempName.IndexOf('\0')).Trim();

            index += 60;

            Hash = BitConverter.ToInt32(rawBytes, index).ToString("X");



        }
    }
}
