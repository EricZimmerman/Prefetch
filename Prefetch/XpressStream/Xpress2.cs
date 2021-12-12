using System.Runtime.InteropServices;

namespace Prefetch.XpressStream;

public class Xpress2
{
//        const ushort COMPRESSION_FORMAT_LZNT1 = 2;
//        const ushort COMPRESSION_FORMAT_XPRESS = 3;
    private const ushort CompressionFormatXpressHuff = 4;

    [DllImport("ntdll.dll")]
    private static extern uint RtlGetCompressionWorkSpaceSize(ushort compressionFormat,
        ref ulong compressBufferWorkSpaceSize, ref ulong compressFragmentWorkSpaceSize);

    [DllImport("ntdll.dll")]
    private static extern uint RtlDecompressBufferEx(ushort compressionFormat, byte[] uncompressedBuffer,
        int uncompressedBufferSize, byte[] compressedBuffer, int compressedBufferSize, ref int finalUncompressedSize,
        byte[] workSpace);

    public static byte[] Decompress(byte[] buffer, ulong decompressedSize)
    {
        // our uncompressed data will go here
        var outBuf = new byte[decompressedSize];
        ulong compressBufferWorkSpaceSize = 0;
        ulong compressFragmentWorkSpaceSize = 0;

        //get the size of what our workspace needs to be
        var ret = RtlGetCompressionWorkSpaceSize(CompressionFormatXpressHuff, ref compressBufferWorkSpaceSize,
            ref compressFragmentWorkSpaceSize);
        if (ret != 0)
        {
            return null;
        }

        var workSpace = new byte[compressFragmentWorkSpaceSize];
        var dstSize = 0;

        ret = RtlDecompressBufferEx(CompressionFormatXpressHuff, outBuf, outBuf.Length, buffer, buffer.Length,
            ref dstSize, workSpace);
        //if (ret == 0)
        // {
        return outBuf;
        //   }

        //return null;
    }
}