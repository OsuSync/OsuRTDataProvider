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

        public Beatmap GetCurrentBeatmap()
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_offset, out int value);

            var info = GetBeatmapInfo();
            string filename = GetCurrentBeatmapFilename();
            var beatmap = new Beatmap(value, info.Item3, filename);

            return beatmap;
        }

        public BeatmapSet GetCurrentBeatmapSet(int client_id)
        {
            int id = 0;
            int try_count = 0;
            do
            {
                bool s = true;
                s = TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
                s = s && TryReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset, out id);

                if (OsuProcess.HasExited) break;
                if (id == 0 || !s)
                {
                    if (try_count < MAX_RETRY_COUNT)
                    {
                        Thread.Sleep(500);
                        try_count++;
                    }
                    else
                    {
                        return null;
                    }
                }
                else break;

            } while (true);


            var info = GetBeatmapInfo();
            string folder = GetCurrentBeatmapFolder();
            var set = new BeatmapSet(id, client_id, info.Item1, info.Item2, folder);

            return set;
        }

        #region Beatmap Info
        private string GetTitleFullName()
        {
            int try_count = 0;
            string str;

            do
            {
                TryReadIntPtrFromMemory(m_beatmap_address, out var cur_beatmap_address);

                bool success = TryReadStringFromMemory(cur_beatmap_address + s_title_offset, out str);

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

        ///artist title diff
        private Tuple<string, string, string> GetBeatmapInfo()
        {
            string str = GetTitleFullName();

            int pos1 = str.IndexOf(" - ");
            int pos2 = str.LastIndexOf("[");

            string artist = str.Substring(0, pos1);

            if (artist.Contains("(") && artist.Contains(")"))
            {
                int pos3 = artist.IndexOf('(');
                artist = artist.Substring(pos3 + 1, artist.Length - pos3 - 2);
            }

            var tuple = new Tuple<string, string, string>(
                artist,
                str.Substring(pos1 + 3, pos2 - (pos1 + 4)),
                str.Substring(pos2 + 1, str.Length - pos2 - 2));

            return tuple;
        }
        #endregion
    }
}
