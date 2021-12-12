using System;

namespace Prefetch.Other;

public class MFTInformation
{
    public MFTInformation()
    {
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
            entryIndex2 = entryIndex2*16777216; //2^24
            entryIndex = entryIndex1 + entryIndex2;
        }

        MFTEntryNumber = entryIndex;
        MFTSequenceNumber = sequenceNumber;

        if (sequenceNumber == 0)
        {
            MFTSequenceNumber = null;
        }

            
    }

    public ulong? MFTEntryNumber { get; set; }

    public int? MFTSequenceNumber { get; set; }


    public override string ToString()
    {
        return $"Entry: {MFTEntryNumber}, Seq: {MFTSequenceNumber}";
    }
}