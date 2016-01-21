using System;
using System.Collections.Generic;

namespace Prefetch.Other
{
    public class VolumeInfo
    {
        public int DeviceOffset { get; }
        public DateTimeOffset CreationTime { get; }
        public string SerialNumber { get; }
        public string DeviceName { get; }

        
        public List<MFTInformation> FileReferences { get; }

        public List<string> DirectoryNames { get; }


        public VolumeInfo(int offset, DateTimeOffset createdOn, string serialNum, string deviceName)
        {
            DeviceOffset = offset;
            CreationTime = createdOn;
            SerialNumber = serialNum;
            DeviceName = deviceName;

            FileReferences = new List<MFTInformation>();
            DirectoryNames = new List<string>();
        }

    }
}
