using System;
using System.IO;
using System.Linq;
using System.Text;
using Prefetch.XpressStream;
using ServiceStack;
using ServiceStack.Text;

namespace Prefetch
{
    public class Prefetch
    {
        private const int Signature = 0x41434353;

        public static void DumpToJson(IPrefetch pf, bool pretty, string outFile)
        {
            if (pretty)
            {
                File.WriteAllText(outFile, pf.Dump());
            }
            else
            {
                File.WriteAllText(outFile, pf.ToJson());
            }
        }

        public static void SavePrefetch(string file, IPrefetch pf)
        {
            File.WriteAllBytes(file, pf.RawBytes);
        }

        public static IPrefetch Open(string file)
        {
            IPrefetch pf = null;

            var rawBytes = File.ReadAllBytes(file);

            var tempSig = Encoding.ASCII.GetString(rawBytes, 0, 3);

            if (tempSig.Equals("MAM"))
            {
                //windows 10, so we need to decompress

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
    }
}