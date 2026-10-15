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

        Parse(rawMFTInfo, 0);
    }

    internal MFTInformation(byte[] buffer, int offset)
    {
        Helpers.EnsureRange(buffer, offset, 8, "MFT reference");
        Parse(buffer, offset);
    }

    private void Parse(byte[] buffer, int offset)
    {
        // 48-bit entry number followed by a 16-bit sequence number
        ulong entryLow = BitConverter.ToUInt32(buffer, offset);
        ulong entryHigh = BitConverter.ToUInt16(buffer, offset + 4);
        var sequenceNumber = BitConverter.ToUInt16(buffer, offset + 6);

        MFTEntryNumber = entryLow | (entryHigh << 32);
        MFTSequenceNumber = sequenceNumber == 0 ? null : sequenceNumber;
    }

    public ulong? MFTEntryNumber { get; set; }

    public int? MFTSequenceNumber { get; set; }


    public override string ToString()
    {
        return $"Entry: {MFTEntryNumber}, Seq: {MFTSequenceNumber}";
    }
}