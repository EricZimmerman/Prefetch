using System;

namespace Prefetch
{
    public class TraceChain17
    {
        public TraceChain17(byte[] rawBytes)
        {
            NextArrayEntryIndex = BitConverter.ToInt32(rawBytes, 0);
            TotalBlockLoadCount = BitConverter.ToInt32(rawBytes, 4);
            Unknown0 = rawBytes[8];
            Unknown1 = rawBytes[9];
            Unknown2 = BitConverter.ToInt16(rawBytes, 10);
        }

        public int NextArrayEntryIndex { get; }
        public int TotalBlockLoadCount { get; }
        public byte Unknown0 { get; }
        public byte Unknown1 { get; }
        public short Unknown2 { get; }
    }
}