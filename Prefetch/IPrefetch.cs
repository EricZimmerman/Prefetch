using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefetch
{
    public enum Version
    {
        WinXpOrWin2K3 = 17,
        VistaOrWin7 = 23,
        Win8x = 26,
        Win10 = 30
    }

 public   interface IPrefetch
    {
        byte[] RawBytes { get; }
        string SourceFilename { get; }

    Header Header { get; }

        int FileMetricsOffset { get; }

        int FileMetricsCount { get; }

        int TraceChainsOffset { get; }

        int TraceChainsCount { get; }

        int FilenameStringsOffset { get; }

        int FilenameStringsSize { get; }

        int VolumesInfoOffset { get; }

        int VolumeCount { get; }

        int VolumesInfoSize { get; }

        List<DateTimeOffset> LastRunTimes { get; }
        
        int RunCount { get; }

        //FileMetricsArray list
        //TraceChainsArray list

        List<string> Filenames { get; }

        //VolumesInfo list
        //contains file reference list
        //contains directory strings list

     void SavePrefetch(string file);
    }
}
