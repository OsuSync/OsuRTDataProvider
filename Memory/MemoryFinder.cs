using MemoryReader.BeatmapInfo;
using MemoryReader.Mods;
using System;
using System.Diagnostics;
using static MemoryReader.DefaultLanguage;

namespace MemoryReader.Memory
{
    internal class MemoryFinder
    {
        private SigScan m_sig_scan;
        private Process m_osu_process;

        #region Address Arguments

        private static readonly byte[] s_beatmap_pattern = new byte[] {
            0x8b,0xf1,0x8b,0xfa,0x8b,0x0d,0x0,0x0,0x0,0x0,0x85,0xc9,0x74,0x35
        };

        private static readonly string s_beatmap_mask = "xxxxxx????xxxx";
        private static readonly int s_beatmap_offset = 0xc0;
        private static readonly int s_beatmap_set_offset = 0xc4;

        private static readonly byte[] s_acc_patterm = new byte[]
        {
            0x73,0x7a,0x8b,0x0d,0x0,0x0,0x0,0x0,0x85,0xc9,0x74,0x1f
        };

        private static readonly string s_acc_mask = "xxxx????xxxx";

        private static readonly byte[] s_time_patterm = new byte[]
{
            0x5e,0x5f,0x5d,0xc3,0xa1,0x0,0x0,0x0,0x0,0x89,0x0,0x04
};

        private static readonly string s_time_mask = "xxxxx????x?x";

        #endregion

        private IntPtr m_beatmap_address;
        private IntPtr m_acc_address;//acc,combo,hp,mods,300hit,100hit,50hit,miss Base Address
        private IntPtr m_time_address;

        public MemoryFinder(Process osu)
        {
            m_osu_process = osu;
            m_sig_scan = new SigScan(osu);

            //Find Beatmap ID Address
            m_beatmap_address = m_sig_scan.FindPattern(s_beatmap_pattern, s_beatmap_mask, 6);
            m_beatmap_address = (IntPtr)ReadIntFromMemory(m_beatmap_address);

            //Find acc Address
            m_acc_address = m_sig_scan.FindPattern(s_acc_patterm, s_acc_mask, 4);
            m_acc_address = (IntPtr)ReadIntFromMemory(m_acc_address);

            //Find Time Address
            m_time_address = m_sig_scan.FindPattern(s_time_patterm, s_time_mask, 5);
            m_time_address = (IntPtr)ReadIntFromMemory(m_time_address);

            m_sig_scan.ResetRegion();

#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]Playing Beatmap Base Address:0x{(int)m_beatmap_address:X8}");
            Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]Playing Accuracy Base Address:0x{(int)m_acc_address:X8}");
            Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]Playing Time Base Address:0x{(int)m_time_address:X8}");
#endif
            if(m_time_address==IntPtr.Zero||m_acc_address==IntPtr.Zero||m_beatmap_address==IntPtr.Zero)
            {
                throw new NoFoundAddressException();
            }
        }

        public Beatmap GetCurrentBeatmap()
        {
            var cur_beatmap_address = (IntPtr)ReadIntFromMemory(m_beatmap_address);
            return new Beatmap(ReadIntFromMemory(cur_beatmap_address + s_beatmap_offset));
        }

        public BeatmapSet GetCurrentBeatmapSet()
        {
            var cur_beatmap_address = (IntPtr)ReadIntFromMemory(m_beatmap_address);
            return new BeatmapSet(ReadIntFromMemory(cur_beatmap_address + s_beatmap_set_offset));
        }

        public double GetCurrentAccuracy()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x48) + 0x14;

            return ReadDoubleFromMemory(tmp_ptr);
        }

        public int GetCurrentCombo()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x34) + 0x18;

            return ReadIntFromMemory(tmp_ptr);
        }

        public int GetMissCount()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x38) + 0x8e;

            return ReadShortFromMemory(tmp_ptr);
        }

        public int Get300Count()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x38) + 0x86;

            return ReadShortFromMemory(tmp_ptr);
        }

        public int Get100Count()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x38) + 0x84;

            return ReadShortFromMemory(tmp_ptr);
        }

        public int Get50Count()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x38) + 0x88;

            return ReadShortFromMemory(tmp_ptr);
        }

        public int GetPlayingTime()
        {
            return ReadIntFromMemory(m_time_address);
        }

        public double GetCurrentHP()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x40) + 0x1c;

            return ReadDoubleFromMemory(tmp_ptr);
        }

        public ModsInfo GetCurrentMods()
        {
            var tmp_ptr = (IntPtr)ReadIntFromMemory(m_acc_address);
            tmp_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x38);
            IntPtr salt_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x1c) + 0x8;
            IntPtr mods_ptr = (IntPtr)ReadIntFromMemory(tmp_ptr + 0x1c) + 0xc;
            int salt = ReadIntFromMemory(salt_ptr);
            int mods = ReadIntFromMemory(mods_ptr);

            return new ModsInfo()
            {
                Mod = (ModsInfo.Mods)(mods ^ salt)
            };
        }

        private int ReadIntFromMemory(IntPtr address)
        {
            uint size = 4;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(m_osu_process.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToInt32(buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        private int ReadShortFromMemory(IntPtr address)
        {
            uint size = 2;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(m_osu_process.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToUInt16(buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        private double ReadDoubleFromMemory(IntPtr address)
        {
            uint size = 8;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(m_osu_process.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToDouble(buf, 0);
            }
            return 0.0;
        }
    }

    internal class OsuProcessNoFoundException : Exception
    {
        public override string Message
        {
            get
            {
                return LANG_OSU_NOT_FOUND;
            }
        }
    }

    internal class NoFoundAddressException : Exception
    {
        public override string Message
        {
            get
            {
                return LANG_ADDRESS_NOT_FOUND;
            }
        }
    }
}