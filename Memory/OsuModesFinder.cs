using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuModesFinder : OsuFinderBase
    {
        // 0x80,0xb8,0x0,0x0,0x0,0x0,0x00,0x75,0x19,0xa1,0x0,0x0,0x0,0x0,0x83,0xf8,0x0b,0x74,0x0b
        private static readonly string s_game_modes_pattern = "\x80\xb8\x0\x0\x0\x0\x0\x75\x19\xa1\x0\x0\x0\x0\x83\xf8\x0b\x74\x0b";

        private static readonly string s_game_modes_mask = "xx????xxxx????xxxxx";

        private IntPtr m_game_modes_address;
        private bool success = false;

        public OsuModesFinder(Process osu) : base(osu)
        {
        }

        public bool TryInit()
        {
            SigScan.Reload();

            //Find Game Modes
            m_game_modes_address = SigScan.FindPattern(StringToByte(s_game_modes_pattern), s_game_modes_mask, 10);
            success=TryReadIntPtrFromMemory(m_game_modes_address,out m_game_modes_address);
            if (m_game_modes_address == IntPtr.Zero) return false;

            SigScan.ResetRegion();

#if DEBUG
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]Game Modes Address:0x{(int)m_game_modes_address:X8}");
#endif
            return success;
        }

        public OsuModes GetCurrentOsuModes()
        {
            TryReadIntFromMemory(m_game_modes_address, out int value);

            return (OsuModes)value;
        }
    }
}