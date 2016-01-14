using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
    public class Version17 : IPrefetch
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

        public void SavePrefetch(string file)
        {
            System.IO.File.WriteAllBytes(file, RawBytes);
        }

        public Version17(byte[] rawBytes, string sourceFilename)
        {
            SourceFilename = sourceFilename;

            RawBytes = rawBytes;
            
            Header = new Header(rawBytes.Take(84).ToArray());

            var fileInfoBytes = rawBytes.Skip(84).Take(68).ToArray();

            FileMetricsOffset = BitConverter.ToInt32(fileInfoBytes, 0);
            FileMetricsCount = BitConverter.ToInt32(fileInfoBytes, 4);

            TraceChainsOffset = BitConverter.ToInt32(fileInfoBytes, 8);
            TraceChainsCount = BitConverter.ToInt32(fileInfoBytes, 12);

            FilenameStringsOffset = BitConverter.ToInt32(fileInfoBytes, 16);
            FilenameStringsSize = BitConverter.ToInt32(fileInfoBytes, 20);

            VolumesInfoOffset = BitConverter.ToInt32(fileInfoBytes, 24);
            VolumeCount = BitConverter.ToInt32(fileInfoBytes, 28);

            VolumesInfoSize = BitConverter.ToInt32(fileInfoBytes, 32);

            var rawTime = BitConverter.ToInt64(fileInfoBytes, 36);

            LastRunTimes = new List<DateTimeOffset>();

            LastRunTimes.Add(DateTimeOffset.FromFileTime(rawTime));

            RunCount = BitConverter.ToInt32(fileInfoBytes, 60);


            //need to jump around now to get bytes for
            //FileMetrics
            //TraceChains
            //FilenameString
            //VolumesInfo


            //TODO do something with stuff below here. relevant stuff must be moved to interface
            
            var fileMetricsBytes = rawBytes.Skip(FileMetricsOffset).Take(FileMetricsCount * 20).ToArray();
            var tempIndex = 0;

            var fileMetrics = new List<FileMetric17>();

            while (tempIndex < fileMetricsBytes.Length)
            {
                fileMetrics.Add(new FileMetric17(fileMetricsBytes.Skip(tempIndex).Take(20).ToArray()));
                tempIndex += 20;
            }


            var traceChains = new List<TraceChain17>();

            var traceChainBytes = rawBytes.Skip(TraceChainsOffset).Take(12 * TraceChainsCount).ToArray();
            var traceIndex = 0;
            while (traceIndex < traceChainBytes.Length)
            {
                traceChains.Add(new TraceChain17(traceChainBytes.Skip(traceIndex).Take(20).ToArray()));

                traceIndex += 12;
            }

            var filenameStringsBytes = rawBytes.Skip(FilenameStringsOffset).Take(FilenameStringsSize).ToArray();

            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
            var fileNames = filenamesRaw.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            Filenames = new List<string>();

            Filenames.AddRange(fileNames);

            var volumeInfoBytes = rawBytes.Skip(VolumesInfoOffset).Take(VolumesInfoSize).ToArray();
            var volumeDevicePathOffset = BitConverter.ToInt32(volumeInfoBytes, 0);
            var volDevicePathNumChars = BitConverter.ToInt32(volumeInfoBytes, 4);

            VolumeDeviceName =
                Encoding.Unicode.GetString(
                    rawBytes.Skip(VolumesInfoOffset + volumeDevicePathOffset).Take(volDevicePathNumChars * 2).ToArray());

            var ct = BitConverter.ToInt64(volumeInfoBytes, 8);
           VolumeCreatedOn = DateTimeOffset.FromFileTime(ct);
            
           VolumeSerialNumber = BitConverter.ToInt32(volumeInfoBytes, 16).ToString("X");

            var fileRefOffset = BitConverter.ToInt32(volumeInfoBytes, 20);
            var fileRefSize = BitConverter.ToInt32(volumeInfoBytes, 24);


            var dirStringsOffset = BitConverter.ToInt32(volumeInfoBytes, 28);
            var numDirectoryStrings = BitConverter.ToInt32(volumeInfoBytes, 32);


            //filerefs are at VolumesInfoOffset + fileRefOffset
            var fileRefsIndex = VolumesInfoOffset + fileRefOffset;
            var fileRefBytes = rawBytes.Skip(fileRefsIndex).Take(fileRefSize).ToArray();

            var fileRefVer = BitConverter.ToInt32(rawBytes, 0);
            var numFileRefs = BitConverter.ToInt32(rawBytes, 4);

            tempIndex = 8;

            FileReferences = new List<MFTInformation>();

            while (tempIndex<fileRefBytes.Length)
            {
                FileReferences.Add(new MFTInformation(fileRefBytes.Skip(tempIndex).Take(8).ToArray()));
                tempIndex += 8;
            }

            DirectoryNames = new List<string>();

            var dirStringsIndex = VolumesInfoOffset + dirStringsOffset;
            var dirStringsBytes = rawBytes.Skip(dirStringsIndex).ToArray();

            tempIndex = 0;
            for (int i = 0; i < numDirectoryStrings; i++)
            {
                var dirCharCount = (BitConverter.ToInt16(dirStringsBytes, tempIndex) * 2) + 2; // double the count since its unicode and add 2 extra for null char
                tempIndex += 2;
                var dirName = Encoding.Unicode.GetString(dirStringsBytes,tempIndex,dirCharCount).Trim('\0');
                DirectoryNames.Add(dirName);

                tempIndex += dirCharCount;
            }

        }
    }
}
