using OsuRTDataProvider.Listen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Memory
{
    class OsuPlayModesFinder : OsuFinderBase
    {
        //0x39,0xa6,0,0,0,0,0x01,0x00,0x00,0x85,0,0,0,0,0xa1,0,0,0,0,0xc3
        private static readonly string s_mode_pattern = "\x39\xa6\x0\x0\x0\x0\x01\x00\x00\x85\x0\x0\x0\x0\xa1\x0\x0\x0\x0\xc3";
        private static readonly string s_mode_mask = "xx????xxxx????x????x";

        private IntPtr m_mode_address;

        public OsuPlayModesFinder(Process process) : base(process)
        {
        }
        public bool TryInit()
        {
            SigScan.Reload();

            m_mode_address=SigScan.FindPattern(StringToByte(s_mode_pattern), s_mode_mask, 15);
            bool success=TryReadIntPtrFromMemory(m_mode_address, out m_mode_address);
#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Mode Address:0x{(int)m_mode_address:X8}");
#endif
            if (m_mode_address == IntPtr.Zero)
                success = false;

            return success;
        }

        public PlayMode GetMode()
        {
            TryReadIntFromMemory(m_mode_address, out int val);
            return (PlayMode)val;
        }
    }
}