using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Mods;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayFinder : OsuFinderBase
    {
        #region Address Arguments

        //0x83,0x3d,0x0,0x0,0x0,0x0,0x01,0x74,0x0a,0x8b,0x35,0x0,0x0,0x0,0x0,0x85,0xf6,0x75,0x04
        private static readonly string s_beatmap_pattern = "\x83\x3d\x0\x0\x0\x0\x01\x74\x0a\x8b\x35\x0\x0\x0\x0\x85\xf6\x75\x04";

        private static readonly string s_beatmap_mask = "xx????xxxxx????xxxx";

        private static readonly int s_beatmap_offset = 0xc0;
        private static readonly int s_beatmap_set_offset = 0xc4;
        private static readonly int s_title_offset = 0x7c;

        //0xbf,0x01,0x00,0x00,0x00,0xeb,0x03,0x83,0xcf,0xff,0xa1,0,0,0,0,0x83,0x3d,0,0,0,0,0x02,0x0f,0x85
        private static readonly string s_acc_patterm = "\xbf\x01\x00\x00\x00\xeb\x03\x83\xcf\xff\xa1\x0\x0\x0\x0\x83\x3d\x0\x0\x0\x0\x02\x0f\x85";
        //0x73,0x7a,0x8b,0x0d,0x0,0x0,0x0,0x0,0x85,0xc9,0x74,0x1f
        private static readonly string s_acc_patterm2 = "\x73\x7a\x8b\x0d\x0\x0\x0\x0\x85\xc9\x74\x1f";

        private static readonly string s_acc_mask = "xxxxxxxxxxx????xx????xxx";
        private static readonly string s_acc_mask2 = "xxxx????xxxx";

        private bool m_use_acc_address2 = false;

        //0x5e,0x5f,0x5d,0xc3,0xa1,0x0,0x0,0x0,0x0,0x89,0x0,0x04
        private static readonly string s_time_patterm = "\x5e\x5f\x5d\xc3\xa1\x0\x0\x0\x0\x89\x0\x04";

        private static readonly string s_time_mask = "xxxxx????x?x";

        #endregion Address Arguments

        private IntPtr m_beatmap_address;
        private IntPtr m_acc_address;//acc,combo,hp,mods,300hit,100hit,50hit,miss Base Address
        private IntPtr m_time_address;

        public bool BeatmapAddressSuccess { get; private set; }
        public bool AccuracyAddressSuccess  { get; private set; }
        public bool TimeAddressSuccess  { get; private set; }

        public OsuPlayFinder(Process osu) : base(osu)
        {
        }

        public bool TryInit()
        {
            SigScan.Reload();

            //Find Beatmap ID Address
            m_beatmap_address = SigScan.FindPattern(StringToByte(s_beatmap_pattern), s_beatmap_mask, 11);
            BeatmapAddressSuccess = TryReadIntPtrFromMemory(m_beatmap_address,out m_beatmap_address);

            //Find acc Address
            m_acc_address = SigScan.FindPattern(StringToByte(s_acc_patterm), s_acc_mask, 11);
            AccuracyAddressSuccess = TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
            if (!AccuracyAddressSuccess)
            {
                m_acc_address = SigScan.FindPattern(StringToByte(s_acc_patterm2), s_acc_mask2, 4);
                AccuracyAddressSuccess=TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
                m_use_acc_address2 = true;
            }

        //Find Time Address
            m_time_address = SigScan.FindPattern(StringToByte(s_time_patterm), s_time_mask, 5);
            TimeAddressSuccess=TryReadIntPtrFromMemory(m_time_address,out m_time_address);

            SigScan.ResetRegion();

#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Playing Beatmap Base Address:0x{(int)m_beatmap_address:X8}");
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Playing Accuracy Base Address:0x{(int)m_acc_address:X8}");
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Playing Time Base Address:0x{(int)m_time_address:X8}");

#endif
            return TimeAddressSuccess && BeatmapAddressSuccess && AccuracyAddressSuccess;
        }

        public Beatmap GetCurrentBeatmap()
        {
            TryReadIntPtrFromMemory(m_beatmap_address, out IntPtr cur_beatmap_address);
            TryReadIntFromMemory(cur_beatmap_address + s_beatmap_offset, out int value);

            var beatmap = new Beatmap(value);
            var info = GetBeatmapInfo();
            beatmap.Diff = info.Item3;

            return beatmap;
        }

        public BeatmapSet GetCurrentBeatmapSet()
        {
            int id = 0;
            do
            {
                TryReadIntPtrFromMemory(m_beatmap_address,out IntPtr cur_beatmap_address);
                TryReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset,out id);

                if (OsuProcess.HasExited) break;
                if (id == 0) Thread.Sleep(500);

                else break;
            } while (true);

            var set = new BeatmapSet(id);
            var info = GetBeatmapInfo();
            set.Artist = info.Item1;
            set.Title = info.Item2;

            return set;
        }

        public double GetCurrentAccuracy()
        {
            TryReadIntPtrFromMemory(m_acc_address,out IntPtr tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60,out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x48,out tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr +0x14,out double value);
            return value;
        }

        public int GetCurrentCombo()
        {
            TryReadIntPtrFromMemory(m_acc_address, out IntPtr tmp_ptr);
            if(!m_use_acc_address2)TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38,out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x90,out var value);
            return value;
        }

        public int GetMissCount()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60,out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x8e,out var value);
            return value;
        }

        public int Get300Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x86,out ushort value);
            return value;
        }

        public int Get100Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38,out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x84,out var value);
            return value;
        }

        public int Get50Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + +0x88, out var value);
            return value;
        }

        public int GetPlayingTime()
        {
            TryReadIntFromMemory(m_time_address,out int value);
            return value;
        }

        public double GetCurrentHP()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x40,out tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr + 0x1c,out double value);
            return value;
        }

        public ModsInfo GetCurrentMods()
        {
            TryReadIntPtrFromMemory(m_acc_address,out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60,out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38,out tmp_ptr);

            TryReadIntPtrFromMemory(tmp_ptr + 0x1c,out var salt_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x1c,out var mods_ptr);

            if(TryReadIntFromMemory(salt_ptr + 0x8,out int salt)&&
                TryReadIntFromMemory(mods_ptr + 0xc,out int mods))
            {
                return new ModsInfo()
                {
                    Mod = (ModsInfo.Mods)(mods ^ salt)
                };
            }
            return ModsInfo.Empty;
        }

        private string GetTitleFullName()
        {
            string str;

            do
            {
                TryReadIntPtrFromMemory(m_beatmap_address,out var cur_beatmap_address);

                bool success = TryReadStringFromMemory(cur_beatmap_address + s_title_offset, out str);

                if (OsuProcess.HasExited) return string.Empty;

                if (!success ||
                    string.IsNullOrEmpty(str) ||
                    (!Regex.IsMatch(str, @".+\(.+\) - .+ \[.+\]") && !Regex.IsMatch(str, @".+ - .+ \[.+\]")))
                    Thread.Sleep(100);
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
    }
}