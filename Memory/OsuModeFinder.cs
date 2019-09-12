using OsuRTDataProvider.Listen;
using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuPlayModeFinder : OsuFinderBase
    {
        //85 ff 74 57? a1 ?? ?? ?? ?? 89 45 e4           
 
        private static readonly string s_mode_pattern = "\x85\xff\x74\x57\xa1\x0\x0\x0\x0\x89\x45\xe4";

        private static readonly string s_mode_mask = "xxxxx????xxx";

        private IntPtr m_mode_address;

        public OsuPlayModeFinder(Process process) : base(process)
        {
        }

        public override bool TryInit()
        {
            bool success = false;

            SigScan.Reload();
            {
                m_mode_address = SigScan.FindPattern(StringToByte(s_mode_pattern), s_mode_mask,5);
                LogHelper.LogToFile($"Mode Address (0):0x{(int)m_mode_address:X8}");

                success = TryReadIntPtrFromMemory(m_mode_address, out m_mode_address);
                LogHelper.LogToFile($"Mode Address (1):0x{(int)m_mode_address:X8}");
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