using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Prefetch.Other;
using Serilog;

namespace Prefetch.Versions;

public class Version23 : IPrefetch
{
    public Version23(byte[] rawBytes, string sourceFilename)
    {
        SourceFilename = sourceFilename;

        RawBytes = rawBytes;

        try
        {

            var headerBytes = new byte[84];
            Buffer.BlockCopy(rawBytes, 0, headerBytes, 0, 84);

            Header = new Header(headerBytes);

            try
            {
                var fi = new FileInfo(sourceFilename);
                SourceCreatedOn = new DateTimeOffset(fi.CreationTimeUtc);
                SourceModifiedOn = new DateTimeOffset(fi.LastWriteTimeUtc);
                SourceAccessedOn = new DateTimeOffset(fi.LastAccessTimeUtc);
            }
            catch (Exception )
            {
                
            }

            var fileInfoBytes = new byte[156];
            Buffer.BlockCopy(rawBytes, 84, fileInfoBytes, 0, 156);

            FileMetricsOffset = BitConverter.ToInt32(fileInfoBytes, 0);
            FileMetricsCount = BitConverter.ToInt32(fileInfoBytes, 4);

            TraceChainsOffset = BitConverter.ToInt32(fileInfoBytes, 8);
            TraceChainsCount = BitConverter.ToInt32(fileInfoBytes, 12);

            FilenameStringsOffset = BitConverter.ToInt32(fileInfoBytes, 16);
            FilenameStringsSize = BitConverter.ToInt32(fileInfoBytes, 20);

            VolumesInfoOffset = BitConverter.ToInt32(fileInfoBytes, 24);
            VolumeCount = BitConverter.ToInt32(fileInfoBytes, 28);

            VolumesInfoSize = BitConverter.ToInt32(fileInfoBytes, 32);

            //at offset 36 there are 8 unknown bytes
            TotalDirectoryCount = BitConverter.ToInt32(fileInfoBytes, 36);

            var rawTime = BitConverter.ToInt64(fileInfoBytes, 44);

            LastRunTimes = new List<DateTimeOffset>();

            LastRunTimes.Add(DateTimeOffset.FromFileTime(rawTime).ToUniversalTime());

            //at offset 52 there are 16 unknown bytes

            RunCount = BitConverter.ToInt32(fileInfoBytes, 68);

            //at offset 72, there are 4 unknown bytes
            //at offset 76, there are 80 unknown bytes

            var fileMetricsBytes = new byte[FileMetricsCount * 32];
            Buffer.BlockCopy(rawBytes, FileMetricsOffset, fileMetricsBytes, 0, FileMetricsCount * 32);
            var tempIndex = 0;

            FileMetrics = new List<FileMetric>();

            var fileMetricsTempBuffer = new byte[32];
            while (tempIndex < fileMetricsBytes.Length)
            {
                Buffer.BlockCopy(fileMetricsBytes, tempIndex, fileMetricsTempBuffer, 0, 32);
                FileMetrics.Add(new FileMetric(fileMetricsTempBuffer, false));
                tempIndex += 32;
            }

            TraceChains = new List<TraceChain>();

            var traceChainBytes = new byte[12 * TraceChainsCount];
            Buffer.BlockCopy(rawBytes, TraceChainsOffset, traceChainBytes, 0, 12 * TraceChainsCount);
            var traceIndex = 0;
            var traceChainTempBuffer = new byte[12];
            while (traceIndex < traceChainBytes.Length)
            {
                Buffer.BlockCopy(traceChainBytes, traceIndex, traceChainTempBuffer, 0, 12);
                TraceChains.Add(new TraceChain(traceChainTempBuffer, false));
                traceIndex += 12;
            }

            var filenameStringsBytes = new byte[FilenameStringsSize];
            Buffer.BlockCopy(rawBytes, FilenameStringsOffset, filenameStringsBytes, 0, FilenameStringsSize);

            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
            var fileNames = filenamesRaw.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);

            Filenames = new List<string>();

            Filenames.AddRange(fileNames);

            var volumeInfoBytes = new byte[VolumesInfoSize];
            Buffer.BlockCopy(rawBytes, VolumesInfoOffset, volumeInfoBytes, 0, VolumesInfoSize);

            VolumeInformation = new List<VolumeInfo>();

            var volBytes = new byte[104];
            for (var j = 0; j < VolumeCount; j++)
            {
                var skipSize = j*104;
                Buffer.BlockCopy(volumeInfoBytes, skipSize, volBytes, 0, 104);

                var volDevOffset = BitConverter.ToInt32(volBytes, 0);
                var volDevNumChar = BitConverter.ToInt32(volBytes, 4);

                var ct = BitConverter.ToInt64(volBytes, 8);

                var devNameBytes = new byte[volDevNumChar * 2];
                Buffer.BlockCopy(rawBytes, VolumesInfoOffset + volDevOffset, devNameBytes, 0, volDevNumChar * 2);
                var devName = Encoding.Unicode.GetString(devNameBytes);

                var sn = BitConverter.ToInt32(volBytes, 16).ToString("X");

                VolumeInformation.Add(new VolumeInfo(volDevOffset, DateTimeOffset.FromFileTime(ct).ToUniversalTime(), sn, devName));

                var fileRefOffset = BitConverter.ToInt32(volBytes, 20);
                var fileRefSize = BitConverter.ToInt32(volBytes, 24);

                var dirStringsOffset = BitConverter.ToInt32(volBytes, 28);
                var numDirectoryStrings = BitConverter.ToInt32(volBytes, 32);

                //filerefs are at VolumesInfoOffset + fileRefOffset
                var fileRefsIndex = VolumesInfoOffset + fileRefOffset;
                var fileRefBytes = new byte[fileRefSize];
                Buffer.BlockCopy(rawBytes, fileRefsIndex, fileRefBytes, 0, fileRefSize);

                var fileRefVer = BitConverter.ToInt32(fileRefBytes, 0);
                var numFileRefs = BitConverter.ToInt32(fileRefBytes, 4);

                tempIndex = 8;

                var tempFileRefBytes = new byte[8];
                while (tempIndex < fileRefBytes.Length && VolumeInformation.Last().FileReferences.Count < numFileRefs)
                {
                    Buffer.BlockCopy(fileRefBytes, tempIndex, tempFileRefBytes, 0, 8);
                    VolumeInformation.Last()
                        .FileReferences.Add(new MFTInformation(tempFileRefBytes));
                    tempIndex += 8;
                }

                var dirStringsIndex = VolumesInfoOffset + dirStringsOffset;
                var dirStringsBytes = new byte[rawBytes.Length - dirStringsIndex];
                Buffer.BlockCopy(rawBytes, dirStringsIndex, dirStringsBytes, 0, rawBytes.Length - dirStringsIndex);

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
        catch (Exception ex)
        {
            Log.Error(ex,"Error processing {File}. Message: {Message}",sourceFilename,ex.Message);
            ParsingError = true;
        }

    }


    public byte[] RawBytes { get; }

    public string SourceFilename { get; }
    public DateTimeOffset SourceCreatedOn { get; }
    public DateTimeOffset SourceModifiedOn { get; }
    public DateTimeOffset SourceAccessedOn { get; }
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
    public bool ParsingError { get; }
    public List<FileMetric> FileMetrics { get; }
    public List<TraceChain> TraceChains { get; }
}