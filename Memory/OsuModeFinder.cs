using OsuRTDataProvider.Listen;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayModesFinder : OsuFinderBase
    {
        //74 60 89 0d 0 0 0 0 8b 3d 0 0 0 0 85 ff
        private static readonly string s_mode_pattern = "\x74\x60\x89\x0d\x0\x0\x0\x0\x8b\x3d\x0\x0\x0\x0\x85\xff";

        private static readonly string s_mode_mask = "xxxx????xx????xx";

        private IntPtr m_mode_address;

        public OsuPlayModesFinder(Process process) : base(process)
        {
        }

        public override bool TryInit()
        {
            SigScan.Reload();

            m_mode_address = SigScan.FindPattern(StringToByte(s_mode_pattern), s_mode_mask, 4);
            bool success = TryReadIntPtrFromMemory(m_mode_address, out m_mode_address);

            EncryptLog($"Mode Address:0x{(int)m_mode_address:X8}");

            if (m_mode_address == IntPtr.Zero)
                success = false;

            SigScan.ResetRegion();

            return success;
        }

        public OsuPlayMode GetMode()
        {
            TryReadIntFromMemory(m_mode_address, out int val);
            return (OsuPlayMode)val;
        }
    }
}