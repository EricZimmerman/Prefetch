using System;

namespace Prefetch
{
    public class TraceChain30
    {
        public TraceChain30(byte[] rawBytes)
        {  
            TotalBlockLoadCount = BitConverter.ToInt32(rawBytes, 0);
            Unknown0 = rawBytes[4];
            Unknown1 = rawBytes[5];
            Unknown2 = BitConverter.ToInt16(rawBytes, 6);
        }

        public int TotalBlockLoadCount { get; }
        public byte Unknown0 { get; }
        public byte Unknown1 { get; }
        public short Unknown2 { get; }

        public override string ToString()
        {
            return $"Total block load count: {TotalBlockLoadCount}";
        }
    }
}