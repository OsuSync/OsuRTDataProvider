using OsuRTDataProvider.Mods;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayFinder : OsuFinderBase
    {
        #region Address Arguments

        //0xbf,0x01,0x00,0x00,0x00,0xeb,0x03,0x83,0xcf,0xff,0xa1,0,0,0,0,0x83,0x3d,0,0,0,0,0x02,0x0f,0x85
        private static readonly string s_acc_pattern = "\xbf\x01\x00\x00\x00\xeb\x03\x83\xcf\xff\xa1\x0\x0\x0\x0\x83\x3d\x0\x0\x0\x0\x02\x0f\x85";

        //0x73,0x7a,0x8b,0x0d,0x0,0x0,0x0,0x0,0x85,0xc9,0x74,0x1f
        private static readonly string s_acc_pattern2 = "\x73\x7a\x8b\x0d\x0\x0\x0\x0\x85\xc9\x74\x1f";

        private static readonly string s_acc_mask = "xxxxxxxxxxx????xx????xxx";
        private static readonly string s_acc_mask2 = "xxxx????xxxx";

        private bool m_use_acc_address2 = false;

        //0x5e,0x5f,0x5d,0xc3,0xa1,0x0,0x0,0x0,0x0,0x89,0x0,0x04
        private static readonly string s_time_pattern = "\x5e\x5f\x5d\xc3\xa1\x0\x0\x0\x0\x89\x0\x04";

        private static readonly string s_time_mask = "xxxxx????x?x";

        #endregion Address Arguments

        private IntPtr m_acc_address;//acc,combo,hp,mods,300hit,100hit,50hit,miss Base Address
        private IntPtr m_time_address;

        public OsuPlayFinder(Process osu) : base(osu)
        {
        }

        public override bool TryInit()
        {
            bool success = false;
            bool m_accuracy_address_success = false;
            bool m_time_address_success = false;

            SigScan.Reload();
            {
                //Find acc Address
                m_acc_address = SigScan.FindPattern(StringToByte(s_acc_pattern), s_acc_mask, 11);
                m_accuracy_address_success = TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
                if (!m_accuracy_address_success)
                {
                    m_acc_address = SigScan.FindPattern(StringToByte(s_acc_pattern2), s_acc_mask2, 4);
                    m_accuracy_address_success = TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
                    m_use_acc_address2 = true;
                }

                //Find Time Address
                m_time_address = SigScan.FindPattern(StringToByte(s_time_pattern), s_time_mask, 5);
                m_time_address_success = TryReadIntPtrFromMemory(m_time_address, out m_time_address);
            }
            SigScan.ResetRegion();

            success=m_time_address_success && m_accuracy_address_success;

            if (m_acc_address == IntPtr.Zero || m_time_address == IntPtr.Zero)
                success = false;

            EncryptLog($"Use Accuracy Address2={m_use_acc_address2}");
            EncryptLog($"Playing Accuracy Base Address:0x{(int)m_acc_address:X8}");
            EncryptLog($"Playing Time Base Address:0x{(int)m_time_address:X8}");

            return success;
        }

        public double GetCurrentAccuracy()
        {
            TryReadIntPtrFromMemory(m_acc_address, out IntPtr tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x48, out tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr + 0x14, out double value);
            return value;
        }

        public int GetCurrentCombo()
        {
            TryReadIntPtrFromMemory(m_acc_address, out IntPtr tmp_ptr);
            if (!m_use_acc_address2) TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x90, out var value);
            return value;
        }

        public int GetMissCount()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x8e, out var value);
            return value;
        }

        public int Get300Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x86, out ushort value);
            return value;
        }

        public int Get100Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x84, out var value);
            return value;
        }

        public int Get50Count()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x88, out var value);
            return value;
        }

        /// <summary>
        /// Osu Geki
        /// Mania RGB 300
        /// </summary>
        /// <returns></returns>
        public int GetGekiCount()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + 0x8a, out var value);
            return value;
        }

        /// <summary>
        /// Osu Katu
        /// Mania 200
        /// </summary>
        public int GetKatuCount()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadShortFromMemory(tmp_ptr + +0x8c, out var value);
            return value;
        }

        public int GetPlayingTime()
        {
            TryReadIntFromMemory(m_time_address, out int value);
            return value;
        }

        public double GetCurrentHP()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x40, out tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr + 0x1c, out double value);
            return value;
        }

        public ModsInfo GetCurrentMods()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
            if (!m_use_acc_address2)
                TryReadIntPtrFromMemory(tmp_ptr + 0x60, out tmp_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x38, out tmp_ptr);

            TryReadIntPtrFromMemory(tmp_ptr + 0x1c, out var salt_ptr);
            TryReadIntPtrFromMemory(tmp_ptr + 0x1c, out var mods_ptr);

            if (TryReadIntFromMemory(salt_ptr + 0x8, out int salt) &&
                TryReadIntFromMemory(mods_ptr + 0xc, out int mods))
            {
                return new ModsInfo()
                {
                    Mod = (ModsInfo.Mods)(mods ^ salt)
                };
            }
            return ModsInfo.Empty;
        }
    }
}