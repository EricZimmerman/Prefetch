using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
   public class TraceChain17
    {
        public int NextArrayEntryIndex { get; }
        public int TotalBlockLoadCount { get; }
        public byte Unknown0 { get; }
        public byte Unknown1 { get; }
        public short Unknown2 { get; }

       public TraceChain17(byte[] rawBytes)
       {
           NextArrayEntryIndex = BitConverter.ToInt32(rawBytes, 0);
           TotalBlockLoadCount = BitConverter.ToInt32(rawBytes, 4);
           Unknown0 = rawBytes[8];
           Unknown1 = rawBytes[9];
           Unknown2 = BitConverter.ToInt16(rawBytes, 10);
        }
    }
}
