using OsuRTDataProvider.BeatmapInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            EncryptLog($"Playing Beatmap Base Address:0x{(int)m_beatmap_address:X8}");

            SigScan.ResetRegion();

            return m_beatmap_address != IntPtr.Zero;
        }

        public Beatmap GetCurrentBeatmap(int osu_id)
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_offset, out int id);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset, out int set_id);

            string filename = GetCurrentBeatmapFilename();
            string folder = GetCurrentBeatmapFolder();

            Beatmap beatmap = Beatmap.Empty;

            bool failed = true;

            try
            {
                if (!(string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(folder)))
                {
                    string folder_full = Path.Combine(Setting.SongsPath, folder);
                    string filename_full = Path.Combine(folder_full, filename);
                    if (File.Exists(filename_full))
                    {
                        beatmap = new Beatmap(osu_id, set_id, id, folder_full, filename_full);
                        failed = false;
                    }
                    else if (Setting.DebugMode)
                        Sync.Tools.IO.CurrentIO.WriteColor($"[OsuRTDataProvider]Can't found beatmap!({filename_full})", ConsoleColor.Yellow);
                }
            }
            catch(Exception e)
            {
                if(Setting.DebugMode)
                {
                    Sync.Tools.IO.CurrentIO.WriteColor("-------------Exception---------------", ConsoleColor.Yellow);
                }
            }

            if (Setting.DebugMode&&failed)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("--------------ORTDP(Detail)----------------", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor($"Songs Path:{Setting.SongsPath}", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor($"Filename:{filename}", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor($"Folder:{folder}", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor($"BeatmapID:{id}", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor($"BeatmapSetID:{set_id}", ConsoleColor.Yellow);
                Sync.Tools.IO.CurrentIO.WriteColor("------------------------------------------", ConsoleColor.Yellow);
            }

            return beatmap;
        }

        #region Beatmap Info
        private string GetCurrentBeatmapFolder()
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out var cur_beatmap_address);
            bool success = TryReadStringFromMemory(cur_beatmap_address + s_beatmap_folder_offset, out string str);
            if (!success) return "";
            return str;
        }

        private string GetCurrentBeatmapFilename()
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out var cur_beatmap_address);
            bool success = TryReadStringFromMemory(cur_beatmap_address + s_beatmap_filename_offset, out string str);
            if (!success) return "";
            return str;
        }
        #endregion
    }
}
