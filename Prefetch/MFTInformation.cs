using System;

namespace Prefetch
{
    public class MFTInformation
    {
        public MFTInformation()
        {
            Note = string.Empty;
        }

        public MFTInformation(byte[] rawMFTInfo)
        {
            if (rawMFTInfo.Length != 8)
            {
                throw new ArgumentException("rawMFTInfo must be 8 bytes long!");
            }

            var sequenceNumber = BitConverter.ToUInt16(rawMFTInfo, 6);

            ulong entryIndex = 0;

            ulong entryIndex1 = BitConverter.ToUInt32(rawMFTInfo, 0);
            ulong entryIndex2 = BitConverter.ToUInt16(rawMFTInfo, 4);

            if (entryIndex2 == 0)
            {
                entryIndex = entryIndex1;
            }
            else
            {
                entryIndex2 = (entryIndex2 * 16777216); //2^24
                entryIndex = (entryIndex1 + entryIndex2);
            }

            MFTEntryNumber = entryIndex;
            MFTSequenceNumber = sequenceNumber;

            if (sequenceNumber == 0)
            {
                MFTSequenceNumber = null;
            }


            if (entryIndex > 0 && sequenceNumber > 0)
            {
                Note = "NTFS";
            }

            if (entryIndex > 0 && sequenceNumber == 0)
            {
                Note = "FAT";
            }

            if (entryIndex == 0 && sequenceNumber == 0)
            {
                Note = "Network/special item";
            }
        }

        public ulong? MFTEntryNumber { get; set; }

        public int? MFTSequenceNumber { get; set; }

        public string Note { get; set; }
    }
}