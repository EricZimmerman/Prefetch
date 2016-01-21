using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefetch.Other;

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
            TotalDirectoryCount = BitConverter.ToInt32(fileInfoBytes, 36);

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


            var fileMetricsBytes = rawBytes.Skip(FileMetricsOffset).Take(FileMetricsCount*32).ToArray();
            var tempIndex = 0;

            FileMetrics = new List<FileMetric>();

            while (tempIndex < fileMetricsBytes.Length)
            {
                FileMetrics.Add(new FileMetric(fileMetricsBytes.Skip(tempIndex).Take(32).ToArray(), false));
                tempIndex += 32;
            }

            TraceChains = new List<TraceChain>();

            var traceChainBytes = rawBytes.Skip(TraceChainsOffset).Take(12*TraceChainsCount).ToArray();
            var traceIndex = 0;
            while (traceIndex < traceChainBytes.Length)
            {
                TraceChains.Add(new TraceChain(traceChainBytes.Skip(traceIndex).Take(12).ToArray(), false));
                traceIndex += 12;
            }

            var filenameStringsBytes = rawBytes.Skip(FilenameStringsOffset).Take(FilenameStringsSize).ToArray();

            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
            var fileNames = filenamesRaw.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

            Filenames = new List<string>();

            Filenames.AddRange(fileNames);

            var volumeInfoBytes = rawBytes.Skip(VolumesInfoOffset).Take(VolumesInfoSize).ToArray();

            VolumeInformation = new List<VolumeInfo>();

            for (var j = 0; j < VolumeCount; j++)
            {
                var skipSize = j*104;
                var volBytes = volumeInfoBytes.Skip(skipSize).Take(104).ToArray();

                var volDevOffset = BitConverter.ToInt32(volBytes, 0);
                var volDevNumChar = BitConverter.ToInt32(volBytes, 4);

                var ct = BitConverter.ToInt64(volBytes, 8);

                var devName = Encoding.Unicode.GetString(
                    rawBytes.Skip(VolumesInfoOffset + volDevOffset).Take(volDevNumChar*2).ToArray());

                var sn = BitConverter.ToInt32(volBytes, 16).ToString("X");

                VolumeInformation.Add(new VolumeInfo(volDevOffset, DateTimeOffset.FromFileTime(ct), sn, devName));

                var fileRefOffset = BitConverter.ToInt32(volBytes, 20);
                var fileRefSize = BitConverter.ToInt32(volBytes, 24);

                var dirStringsOffset = BitConverter.ToInt32(volBytes, 28);
                var numDirectoryStrings = BitConverter.ToInt32(volBytes, 32);

                //filerefs are at VolumesInfoOffset + fileRefOffset
                var fileRefsIndex = VolumesInfoOffset + fileRefOffset;
                var fileRefBytes = rawBytes.Skip(fileRefsIndex).Take(fileRefSize).ToArray();

                var fileRefVer = BitConverter.ToInt32(fileRefBytes, 0);
                var numFileRefs = BitConverter.ToInt32(fileRefBytes, 4);

                tempIndex = 8;

                while (tempIndex < fileRefBytes.Length && VolumeInformation.Last().FileReferences.Count < numFileRefs)
                {
                    VolumeInformation.Last()
                        .FileReferences.Add(new MFTInformation(fileRefBytes.Skip(tempIndex).Take(8).ToArray()));
                    tempIndex += 8;
                }

                var dirStringsIndex = VolumesInfoOffset + dirStringsOffset;
                var dirStringsBytes = rawBytes.Skip(dirStringsIndex).ToArray();

                tempIndex = 0;
                for (var k = 0; k < numDirectoryStrings; k++)
                {
                    var dirCharCount = BitConverter.ToInt16(dirStringsBytes, tempIndex)*2 + 2;
                    // double the count since its Unicode and add 2 extra for null char
                    tempIndex += 2;
                    var dirName = Encoding.Unicode.GetString(dirStringsBytes, tempIndex, dirCharCount).Trim('\0');
                    VolumeInformation.Last().DirectoryNames.Add(dirName);

                    tempIndex += dirCharCount;
                }
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
        public int TotalDirectoryCount { get; }
        public List<DateTimeOffset> LastRunTimes { get; }
        public List<VolumeInfo> VolumeInformation { get; }
        public int RunCount { get; }
        public List<string> Filenames { get; }
        public List<FileMetric> FileMetrics { get; }
        public List<TraceChain> TraceChains { get; }
    }
}