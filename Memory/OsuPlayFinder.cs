using OsuRTDataProvider.Listen;
using OsuRTDataProvider.Mods;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayFinder : OsuFinderBase
    {
        #region Address Arguments

        //0xA1,0,0,0,0,0x8D,0x56,0x0C,0xE8,0x00,0x00,0x00,0x00,0x8B,0x47,0x04
        private static readonly string s_acc_pattern = "\xA1\x0\x0\x0\x0\x8D\x56\x0C\xE8\x00\x00\x00\x00\x8B\x47\x04";
        private static readonly string s_acc_mask = "x????xxxx????xxx";

        //0x73,0x7a,0x8b,0x0d,0x0,0x0,0x0,0x0,0x85,0xc9,0x74,0x1f
        private static readonly string s_acc_pattern_fallback = "\x73\x7a\x8b\x0d\x0\x0\x0\x0\x85\xc9\x74\x1f\x8d\x55\xf0";
        private static readonly string s_acc_mask_fallback = "xxxx????xxxxxxx";

        //0x5e,0x5f,0x5d,0xc3,0xa1,0x0,0x0,0x0,0x0,0x89,0x0,0x04
        private static readonly string s_time_pattern = "\x5e\x5f\x5d\xc3\xa1\x0\x0\x0\x0\x89\x0\x04";
        private static readonly string s_time_mask = "xxxxx????x?x";

        private static readonly string s_global_mods_pattern = "\x8B\xF1\xA1\x00\x00\x00\x00\x25\x00\x00\x40\x00\x85\xC0";
        private static readonly string s_global_mods_mask = "xxx????xxxxxxx";

        #endregion Address Arguments

        private IntPtr m_acc_address;//acc,combo,hp,mods,300hit,100hit,50hit,miss Base Address
        private IntPtr m_time_address;
        private IntPtr m_mods_address;

        private static byte[] s_global_mods_pattern_bytes;
        private static byte[] s_acc_pattern_bytes;
        private static byte[] s_acc_pattern_fallback_bytes;
        private static byte[] s_time_pattern_bytes;

        public OsuPlayFinder(Process osu) : base(osu)
        {
        }

        public override bool TryInit()
        {
            bool success = false;
            bool m_accuracy_address_success = false;
            bool m_time_address_success = false;
            bool m_mods_address_success = false;

            SigScan.Reload();
            {
                if (Setting.EnableModsChangedAtListening)
                {
                    //Find mods address
                    m_mods_address = SigScan.FindPattern(s_global_mods_pattern_bytes = s_global_mods_pattern_bytes ?? StringToByte(s_global_mods_pattern), s_global_mods_mask, 3);
                    LogHelper.LogToFile($"Mods Base Address (0):0x{(int)m_mods_address:X8}");

                    m_mods_address_success = TryReadIntPtrFromMemory(m_mods_address, out m_mods_address);
                    LogHelper.LogToFile($"Mods Base Address (1):0x{(int)m_mods_address:X8}");
                }
                
                //Find acc Address
                m_acc_address = SigScan.FindPattern(s_acc_pattern_bytes = s_acc_pattern_bytes ?? StringToByte(s_acc_pattern), s_acc_mask, 1);
                LogHelper.LogToFile($"Playing Accuracy Base Address (0):0x{(int)m_acc_address:X8}");

                m_accuracy_address_success = TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
                LogHelper.LogToFile($"Playing Accuracy Base Address (1):0x{(int)m_acc_address:X8}");

                if (!m_accuracy_address_success)//use s_acc_pattern_fallback
                {
                    LogHelper.LogToFile("Use Fallback Accuracy Pattern");
                    m_acc_address = SigScan.FindPattern(s_acc_pattern_fallback_bytes = s_acc_pattern_fallback_bytes ?? StringToByte(s_acc_pattern_fallback), s_acc_mask_fallback, 4);
                    LogHelper.LogToFile($"Playing Accuracy Base Address (0):0x{(int)m_acc_address:X8}");

                    m_accuracy_address_success = TryReadIntPtrFromMemory(m_acc_address, out m_acc_address);
                    LogHelper.LogToFile($"Playing Accuracy Base Address (1):0x{(int)m_acc_address:X8}");
                }

                //Find Time Address
                m_time_address = SigScan.FindPattern(s_time_pattern_bytes = s_time_pattern_bytes ?? StringToByte(s_time_pattern), s_time_mask, 5);
                LogHelper.LogToFile($"Time Base Address (0):0x{(int)m_time_address:X8}");

                m_time_address_success = TryReadIntPtrFromMemory(m_time_address, out m_time_address);
                LogHelper.LogToFile($"Time Base Address (1):0x{(int)m_time_address:X8}");
            }
            SigScan.ResetRegion();

            success = m_time_address_success && m_accuracy_address_success;
            if(Setting.EnableModsChangedAtListening)
                 success = success && m_mods_address_success;

            if (m_acc_address == IntPtr.Zero || m_time_address == IntPtr.Zero)
                success = false;

            return success;
        }

        public double GetCurrentAccuracy()
        {
            TryReadIntPtrFromMemory(RulesetBaseAddress + 0x48, out var tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr + 0x14, out double value);
            return value;
        }

        public double GetCurrentHP()
        {
            TryReadIntPtrFromMemory(RulesetBaseAddress + 0x40, out var tmp_ptr);

            TryReadDoubleFromMemory(tmp_ptr + 0x1c, out double value);
            return value;
        }

        #region Score Address

        public int GetCurrentScore()
        {
            TryReadIntPtrFromMemory(m_acc_address, out var tmpPtr);

            TryReadIntPtrFromMemory(tmpPtr + 0x44, out tmpPtr);
            TryReadIntFromMemory(tmpPtr + 0xf8, out var value);
            return value;
        }

        public int GetCurrentCombo()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x94, out var value);
            return value;
        }

        public int GetMissCount()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x92, out var value);
            return value;
        }

        public int Get300Count()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x8a, out ushort value);
            return value;
        }

        public int Get100Count()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x88, out var value);
            return value;
        }

        public int Get50Count()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x8c, out var value);
            return value;
        }

        /// <summary>
        /// Osu Geki
        /// Mania 300g
        /// </summary>
        /// <returns></returns>
        public int GetGekiCount()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x8e, out var value);
            return value;
        }

        /// <summary>
        /// Osu Katu
        /// Mania 200
        /// </summary>
        public int GetKatuCount()
        {
            TryReadShortFromMemory(ScoreBaseAddress + 0x90, out var value);
            return value;
        }

        public ErrorStatisticsResult GetUnstableRate()
        {
            TryReadListFromMemory<int>(ScoreBaseAddress + 0x38, out var list);
            if (list == null)
                return ErrorStatisticsResult.Empty;
            var result = Utils.GetErrorStatisticsArray(list);
            return new ErrorStatisticsResult
            {
                ErrorMin = result[0],
                ErrorMax = result[1],
                UnstableRate = result[4] * 10,
            };
        }

        public string GetCurrentPlayerName()
        {
            TryReadStringFromMemory(ScoreBaseAddress + 0x28, out var str);
            return str;
        }

        public ModsInfo GetCurrentModsAtListening()
        {
            if (TryReadIntFromMemory(m_mods_address, out var mods))
            {
                //if (TryReadIntFromMemory(mods_ptr + 0x8, out int salt) &&
                //    TryReadIntFromMemory(mods_ptr + 0xc, out int mods))
                //{
                return new ModsInfo()
                {
                    Mod = (ModsInfo.Mods)(mods)
                };
                //}
            }
            return ModsInfo.Empty;
        }

        public ModsInfo GetCurrentMods()
        {
            IntPtr mods_ptr;

            var tmp_ptr = ScoreBaseAddress;
            TryReadIntPtrFromMemory(tmp_ptr + 0x1c, out mods_ptr);

            if (TryReadIntFromMemory(mods_ptr + 0x8, out int salt) &&
                TryReadIntFromMemory(mods_ptr + 0xc, out int mods))
            {
                return new ModsInfo()
                {
                    Mod = (ModsInfo.Mods)(mods ^ salt)
                };
            }
            return ModsInfo.Empty;
        }
        #endregion

        #region Time Address
        public int GetPlayingTime()
        {
            TryReadIntFromMemory(m_time_address, out int value);
            return value;
        }
        #endregion



        private IntPtr RulesetBaseAddress
        {
            get
            {
                TryReadIntPtrFromMemory(m_acc_address, out var tmp_ptr);
                return tmp_ptr;
            }
        }

        private IntPtr ScoreBaseAddress
        {
            get
            {
                TryReadIntPtrFromMemory(RulesetBaseAddress + 0x38, out var tmp_ptr);
                return tmp_ptr;
            }
        }
    }
}