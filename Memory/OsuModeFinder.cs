using OsuRTDataProvider.Listen;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayModesFinder : OsuFinderBase
    {
        //8B 40 30 FF 50 14 83 3D 00000000 03    

        private static readonly string s_mode_pattern = "\x8B\x40\x30\xFF\x50\x14\x83\x3D\x00\x00\x00\x00\x03";

        private static readonly string s_mode_mask = "xxxxxxxx????x";

        private IntPtr m_mode_address;

        public OsuPlayModesFinder(Process process) : base(process)
        {
        }

        public override bool TryInit()
        {
            bool success = false;

            SigScan.Reload();
            {
                m_mode_address = SigScan.FindPattern(StringToByte(s_mode_pattern), s_mode_mask,8);
                EncryptLog($"Mode Address (0):0x{(int)m_mode_address:X8}");

                success = TryReadIntPtrFromMemory(m_mode_address, out m_mode_address);
                EncryptLog($"Mode Address (1):0x{(int)m_mode_address:X8}");
            }
            SigScan.ResetRegion();

            if (m_mode_address == IntPtr.Zero)
                success = false;

            return success;
        }

        public OsuPlayMode GetMode()
        {
            TryReadIntFromMemory(m_mode_address, out int val);
            return (OsuPlayMode)val;
        }
    }
}