using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryReader.Memory
{
    class OsuModesFinder:OsuFinderBase
    {

        private static readonly byte[] s_game_modes_patterm = new byte[]
{
           0x80,0xb8,0x0,0x0,0x0,0x0,0x00,0x75,0x19,0xa1,0x0,0x0,0x0,0x0,0x83,0xf8,0x0b,0x74,0x0b
};

        private static readonly string s_game_modes_mask = "xx????xxxx????xxxxx";

        private IntPtr m_game_modes_address;

        public OsuModesFinder(Process osu):base(osu)
        {
        }

        public bool TryInit()
        {
            SigScan.Reload();

            //Find Game Modes
            m_game_modes_address = SigScan.FindPattern(s_game_modes_patterm, s_game_modes_mask, 10);
            if (m_game_modes_address == IntPtr.Zero)return false;

            m_game_modes_address = (IntPtr)ReadIntFromMemory(m_game_modes_address);

            SigScan.ResetRegion();

#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]Game Modes Address:0x{(int)m_game_modes_address:X8}");
#endif
            return true;
        }

        public OsuModes GetCurrentOsuModes()
        {
            return (OsuModes)ReadIntFromMemory(m_game_modes_address);
        }
    }
}
