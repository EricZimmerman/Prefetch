using System;

namespace Prefetch.Other;

public class TraceChain
{
    public TraceChain(byte[] rawBytes, bool ver30)
    {
        if (ver30)
        {
            TotalBlockLoadCount = BitConverter.ToInt32(rawBytes, 0);
            Unknown0 = rawBytes[4];
            Unknown1 = rawBytes[5];
            Unknown2 = BitConverter.ToInt16(rawBytes, 6);
        }
        else
        {
            NextArrayEntryIndex = BitConverter.ToInt32(rawBytes, 0);
            TotalBlockLoadCount = BitConverter.ToInt32(rawBytes, 4);
            Unknown0 = rawBytes[8];
            Unknown1 = rawBytes[9];
            Unknown2 = BitConverter.ToInt16(rawBytes, 10);
        }
    }

    public int NextArrayEntryIndex { get; }
    public int TotalBlockLoadCount { get; }
    public byte Unknown0 { get; }
    public byte Unknown1 { get; }
    public short Unknown2 { get; }

    public override string ToString()
    {
        return $"Next index: {NextArrayEntryIndex}, Total block load count: {TotalBlockLoadCount}";
    }
}