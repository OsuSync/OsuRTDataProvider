using OsuRTDataProvider.BeatmapInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Memory
{
    class OsuBeatmapFinder: OsuFinderBase
    {
        //0x83,0x3d,0x0,0x0,0x0,0x0,0x01,0x74,0x0a,0x8b,0x35,0x0,0x0,0x0,0x0,0x85,0xf6,0x75,0x04
        private static readonly string s_beatmap_pattern = "\x83\x3d\x0\x0\x0\x0\x01\x74\x0a\x8b\x35\x0\x0\x0\x0\x85\xf6\x75\x04";

        private static readonly string s_beatmap_mask = "xx????xxxxx????xxxx";

        private static readonly int s_beatmap_offset = 0xc0;
        private static readonly int s_beatmap_set_offset = 0xc4;

        private static readonly int s_title_offset = 0x7c;
        private static readonly int s_beatmap_folder_offset = 0x70;
        private static readonly int s_beatmap_filename_offset = 0x88;

        private const int MAX_RETRY_COUNT = 10;

        private IntPtr m_beatmap_address;
        public bool BeatmapAddressSuccess { get; private set; }

        public OsuBeatmapFinder(Process osu) : base(osu)
        {

        }

        public bool TryInit()
        {
            SigScan.Reload();

            //Find Beatmap ID Address
            m_beatmap_address = SigScan.FindPattern(StringToByte(s_beatmap_pattern), s_beatmap_mask, 11);
            BeatmapAddressSuccess = TryReadIntPtrFromMemory(m_beatmap_address, out m_beatmap_address);

#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Playing Beatmap Base Address:0x{(int)m_beatmap_address:X8}");
#endif

            SigScan.ResetRegion();

            return m_beatmap_address == IntPtr.Zero;
        }

        public Beatmap GetCurrentBeatmap(int osu_id)
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_offset, out int id);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset, out int set_id);

            string filename = GetCurrentBeatmapFilename();
            string folder = GetCurrentBeatmapFolder();
            var beatmap = new Beatmap(osu_id,set_id,id,folder,filename);

            return beatmap;
        }

        #region Beatmap Info
        private string GetCurrentBeatmapFolder()
        {
            int try_count = 0;
            string str;

            do
            {
                TryReadIntPtrFromMemory(m_beatmap_address, out var cur_beatmap_address);

                bool success = TryReadStringFromMemory(cur_beatmap_address + s_beatmap_folder_offset, out str);

                if (OsuProcess.HasExited) return string.Empty;

                if ((!success || string.IsNullOrEmpty(str)) && try_count < MAX_RETRY_COUNT)
                {
                    try_count++;
                    Thread.Sleep(100);
                }
                else break;
            } while (true);

            return str;
        }

        private string GetCurrentBeatmapFilename()
        {
            int try_count = 0;
            string str;

            do
            {
                TryReadIntPtrFromMemory(m_beatmap_address, out var cur_beatmap_address);

                bool success = TryReadStringFromMemory(cur_beatmap_address + s_beatmap_filename_offset, out str);

                if (OsuProcess.HasExited) return string.Empty;

                if ((!success || string.IsNullOrEmpty(str)) && try_count < MAX_RETRY_COUNT)
                {
                    try_count++;
                    Thread.Sleep(100);
                }
                else break;
            } while (true);

            return str;
        }
        #endregion
    }
}
