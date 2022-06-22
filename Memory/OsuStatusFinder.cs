using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuStatusFinder : OsuFinderBase
    {
        private static readonly string s_game_modes_pattern = "\x75\x07\x8B\x45\x90\xC6\x40\x2A\x00\x83\x3D\x0\x0\x0\x0\x0F";

        private static readonly string s_game_modes_mask = "xxxxxxxxxxx????x";

        private IntPtr m_game_modes_address;
        private bool success = false;
        private static byte[] s_game_modes_pattern_bytes;

        public OsuStatusFinder(Process osu) : base(osu)
        {
        }

        public override bool TryInit()
        {
            SigScan.Reload();
            {
                //Find Game Modes
                m_game_modes_address = SigScan.FindPattern(s_game_modes_pattern_bytes = s_game_modes_pattern_bytes ?? StringToByte(s_game_modes_pattern), s_game_modes_mask, 11);
                LogHelper.LogToFile($"Game Status Address (0):0x{(int)m_game_modes_address:X8}");

                success = TryReadIntPtrFromMemory(m_game_modes_address, out m_game_modes_address);
                LogHelper.LogToFile($"Game Status Address (1):0x{(int)m_game_modes_address:X8}");
            }
            SigScan.ResetRegion();

            if (m_game_modes_address == IntPtr.Zero) success = false;



            return success;
        }

        public OsuInternalStatus GetCurrentOsuModes()
        {
            TryReadIntFromMemory(m_game_modes_address, out int value);

            return (OsuInternalStatus)value;
        }
    }
}