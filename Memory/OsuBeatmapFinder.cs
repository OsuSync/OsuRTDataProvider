using OsuRTDataProvider.BeatmapInfo;
using System;
using System.Diagnostics;
using System.IO;

namespace OsuRTDataProvider.Memory
{
    internal class OsuBeatmapFinder : OsuFinderBase
    {
        //0x83,0x3d,0x0,0x0,0x0,0x0,0x01,0x74,0x0a,0x8b,0x35,0x0,0x0,0x0,0x0,0x85,0xf6,0x75,0x04
        private static readonly string s_beatmap_pattern = "\x83\x3d\x0\x0\x0\x0\x01\x74\x0a\x8b\x35\x0\x0\x0\x0\x85\xf6\x75\x04";

        private static readonly string s_beatmap_mask = "xx????xxxxx????xxxx";

        private static readonly int s_beatmap_offset = 0xc4;
        private static readonly int s_beatmap_set_offset = 0xc8;

        private static readonly int s_beatmap_folder_offset = 0x74;
        private static readonly int s_beatmap_filename_offset = 0x8c;

        private const int MAX_RETRY_COUNT = 10;

        private IntPtr m_beatmap_address;

        public OsuBeatmapFinder(Process osu) : base(osu)
        {
        }

        public override bool TryInit()
        {
            bool success = false;
            SigScan.Reload();
            {
                //Find Beatmap ID Address
                m_beatmap_address = SigScan.FindPattern(StringToByte(s_beatmap_pattern), s_beatmap_mask, 11);
                LogHelper.EncryptLog($"Beatmap Base Address (0):0x{(int)m_beatmap_address:X8}");

                success = TryReadIntPtrFromMemory(m_beatmap_address, out m_beatmap_address);
                LogHelper.EncryptLog($"Beatmap Base Address (1):0x{(int)m_beatmap_address:X8}");
            }
            SigScan.ResetRegion();

            if (m_beatmap_address == IntPtr.Zero)
                success = false;

            return success;
        }

        public Beatmap GetCurrentBeatmap(int osu_id)
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_offset, out int id);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset, out int set_id);

            string filename = GetCurrentBeatmapFilename();
            string folder = GetCurrentBeatmapFolder();

            Beatmap beatmap = Beatmap.Empty;

            try
            {
                if (!(string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(folder)))
                {
                    string folder_full = Path.Combine(Setting.SongsPath, folder);
                    string filename_full = Path.Combine(folder_full, filename);
                    using (var fs = File.OpenRead(filename_full))
                    {
                        beatmap = new Beatmap(osu_id, set_id, id, fs);
                    }
                }
            }
            catch (Exception e)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("-------------ORTDP(Exception)---------------", ConsoleColor.Red);
                Sync.Tools.IO.CurrentIO.WriteColor(e.ToString(), ConsoleColor.Yellow);
                if (Setting.DebugMode)
                {
                    Sync.Tools.IO.CurrentIO.WriteColor("--------------ORTDP(Detail)-----------------", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor($"Songs Path:{Setting.SongsPath}", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor($"Filename:{filename}", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor($"Folder:{folder}", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor($"BeatmapID:{id}", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor($"BeatmapSetID:{set_id}", ConsoleColor.Yellow);
                    Sync.Tools.IO.CurrentIO.WriteColor("--------------------------------------------", ConsoleColor.Yellow);
                }
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

        #endregion Beatmap Info
    }
}