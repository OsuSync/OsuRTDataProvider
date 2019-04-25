using OsuRTDataProvider.Listen;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayModeFinder : OsuFinderBase
    {
        //83 7D D4 00 0F 84 XX XX XX XX A1 ?? ?? ?? ?? 83 F8 01 74 0E                
 
        private static readonly string s_mode_pattern = "\x83\x7d\xd4\x00\x0f\x84\xdd\x01\x00\x00\xa1\x0\x0\x0\x0\x83\xf8\x01\x74\x0e";

        private static readonly string s_mode_mask = "xxxxxxxxxxx????xxxxx";

        private IntPtr m_mode_address;

        public OsuPlayModeFinder(Process process) : base(process)
        {
        }

        public override bool TryInit()
        {
            bool success = false;

            SigScan.Reload();
            {
                m_mode_address = SigScan.FindPattern(StringToByte(s_mode_pattern), s_mode_mask,11);
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