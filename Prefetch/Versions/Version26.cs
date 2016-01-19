using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prefetch
{
    public class Version26 : IPrefetch
    {
        public Version26(byte[] rawBytes, string sourceFilename)
        {
            SourceFilename = sourceFilename;

            RawBytes = rawBytes;

            Header = new Header(rawBytes.Take(84).ToArray());

            //TODO factor out creation of File info blocks
            var fileInfoBytes = rawBytes.Skip(84).Take(224).ToArray();

            FileMetricsOffset = BitConverter.ToInt32(fileInfoBytes, 0);
            FileMetricsCount = BitConverter.ToInt32(fileInfoBytes, 4);

            TraceChainsOffset = BitConverter.ToInt32(fileInfoBytes, 8);
            TraceChainsCount = BitConverter.ToInt32(fileInfoBytes, 12);

            FilenameStringsOffset = BitConverter.ToInt32(fileInfoBytes, 16);
            FilenameStringsSize = BitConverter.ToInt32(fileInfoBytes, 20);

            VolumesInfoOffset = BitConverter.ToInt32(fileInfoBytes, 24);
            VolumeCount = BitConverter.ToInt32(fileInfoBytes, 28);

            VolumesInfoSize = BitConverter.ToInt32(fileInfoBytes, 32);

            //at offset 36 there are 8 unknown values, seemingly empty

            var runtimeBytes = fileInfoBytes.Skip(44).Take(64).ToArray();

            LastRunTimes = new List<DateTimeOffset>();

            for (var i = 0; i < 7; i++)
            {
                var rawTime = BitConverter.ToInt64(runtimeBytes, i*8);

                if (rawTime > 0)
                {
                    LastRunTimes.Add(DateTimeOffset.FromFileTime(rawTime));
                }
            }

            //at offset 108 there are 16 bytes of unknown, possibly previous data from runtimes

            RunCount = BitConverter.ToInt32(fileInfoBytes, 124);

            var unknown0 = BitConverter.ToInt32(fileInfoBytes, 128);
            var unknown1 = BitConverter.ToInt32(fileInfoBytes, 132);
            //at offset 136 there is 88 bytes of unknown, empty values
            var unknown2 = BitConverter.ToInt32(fileInfoBytes, 128);

            //TODO do something with stuff below here. relevant stuff must be moved to interface

            var fileMetricsBytes = rawBytes.Skip(FileMetricsOffset).Take(FileMetricsCount*32).ToArray();
            var tempIndex = 0;

            //TODO must end up as a property
            var fileMetrics = new List<FileMetric23>();

            while (tempIndex < fileMetricsBytes.Length)
            {
                fileMetrics.Add(new FileMetric23(fileMetricsBytes.Skip(tempIndex).Take(32).ToArray()));
                tempIndex += 32;
            }

            var traceChains = new List<TraceChain17>();

            var traceChainBytes = rawBytes.Skip(TraceChainsOffset).Take(12*TraceChainsCount).ToArray();
            var traceIndex = 0;
            while (traceIndex < traceChainBytes.Length)
            {
                traceChains.Add(new TraceChain17(traceChainBytes.Skip(traceIndex).Take(12).ToArray()));
                traceIndex += 12;
            }

            var filenameStringsBytes = rawBytes.Skip(FilenameStringsOffset).Take(FilenameStringsSize).ToArray();

            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
            var fileNames = filenamesRaw.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

            Filenames = new List<string>();

            Filenames.AddRange(fileNames);

            var volumeInfoBytes = rawBytes.Skip(VolumesInfoOffset).Take(VolumesInfoSize).ToArray();
            var volumeDevicePathOffset = BitConverter.ToInt32(volumeInfoBytes, 0);
            var volDevicePathNumChars = BitConverter.ToInt32(volumeInfoBytes, 4);

            //TODO for all versions, need to loop to get all volumes, not just first one, based on VolumeCount

            VolumeDeviceName =
                Encoding.Unicode.GetString(
                    rawBytes.Skip(VolumesInfoOffset + volumeDevicePathOffset).Take(volDevicePathNumChars*2).ToArray());

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

            var fileRefVer = BitConverter.ToInt32(fileRefBytes, 0);
            var numFileRefs = BitConverter.ToInt32(fileRefBytes, 4);

            tempIndex = 8;

            FileReferences = new List<MFTInformation>();

            while (tempIndex < fileRefBytes.Length && FileReferences.Count < numFileRefs)
            {
                FileReferences.Add(new MFTInformation(fileRefBytes.Skip(tempIndex).Take(8).ToArray()));
                tempIndex += 8;
            }

            DirectoryNames = new List<string>();

            var dirStringsIndex = VolumesInfoOffset + dirStringsOffset;
            var dirStringsBytes = rawBytes.Skip(dirStringsIndex).ToArray();

            tempIndex = 0;
            for (var i = 0; i < numDirectoryStrings; i++)
            {
                var dirCharCount = BitConverter.ToInt16(dirStringsBytes, tempIndex)*2 + 2;
                // double the count since its unicode and add 2 extra for null char
                tempIndex += 2;
                var dirName = Encoding.Unicode.GetString(dirStringsBytes, tempIndex, dirCharCount).Trim('\0');
                DirectoryNames.Add(dirName);

                tempIndex += dirCharCount;
            }
        }

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
    }
}