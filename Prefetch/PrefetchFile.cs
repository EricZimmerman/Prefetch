using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Prefetch.Versions;
using Prefetch.XpressStream;


namespace Prefetch;

public class PrefetchFile
{
    private const int Signature = 0x41434353;
        
    public static void SavePrefetch(string file, IPrefetch pf)
    {
        File.WriteAllBytes(file, pf.RawBytes);
    }

    public static IPrefetch Open(Stream stream, string file)
    {
        IPrefetch pf = null;

        var rawBytes = new byte[stream.Length];
        stream.Read(rawBytes, (int) 0, (int) (rawBytes.Length));

        var tempSig = Encoding.ASCII.GetString(rawBytes, 0, 3);

        if (tempSig.Equals("MAM"))
        {
            //windows 10/11, so we need to decompress
            
            //Size of decompressed data is at offset 4
            var size = BitConverter.ToUInt32(rawBytes, 4);

            //get our compressed bytes (skipping signature and uncompressed size)
            var compressedBytes = rawBytes.Skip(8).ToArray();
            var decom = Xpress2.Decompress(compressedBytes, size);

            //update rawBytes with decompressed bytes so the rest works
            rawBytes = decom;
        }

        //at this point we have prefetch bytes we can process

        var fileVer = (Version) BitConverter.ToInt32(rawBytes, 0);

        var sig = BitConverter.ToInt32(rawBytes, 4);

        if (sig != Signature)
        {
            throw new Exception("Invalid signature! Should be 'SCCA'");
        }

        switch (fileVer)
        {
            case Version.WinXpOrWin2K3:
                pf = new Version17(rawBytes, file);
                break;
            case Version.VistaOrWin7:
                pf = new Version23(rawBytes, file);
                break;
            case Version.Win8xOrWin2012x:
                pf = new Version26(rawBytes, file);
                break;
            case Version.Win10:
                pf = new Version30(rawBytes, file);
                break;
            default:
                throw new Exception($"Unknown version '{fileVer:X}'");
        }

        return pf;
    }

    public static IPrefetch Open(string file)
    {
        using (var fs = new FileStream(file,FileMode.Open,FileAccess.Read))
        {
            return Open(fs,file);
        }

    }
}