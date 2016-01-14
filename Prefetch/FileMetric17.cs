using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
    public class FileMetric17
    {
        public int Unknown0 { get; }
        public int Unknown1 { get; }
        public int FilenameStringOffset { get; }
        public int FilenameStringSize { get; }
        public int Unknown2 { get; }

        public FileMetric17(byte[] rawBytes)
        {
            Unknown0 = BitConverter.ToInt32(rawBytes, 0);
            Unknown1 = BitConverter.ToInt32(rawBytes, 4);

            FilenameStringOffset = BitConverter.ToInt32(rawBytes, 8);
            FilenameStringSize = BitConverter.ToInt32(rawBytes, 12);

            Unknown2 = BitConverter.ToInt32(rawBytes, 16);
        }
    }
}
