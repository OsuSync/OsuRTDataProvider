using System;
using System.Diagnostics;

namespace OsuRTDataProvider.Memory
{
    internal class OsuStatusFinder : OsuFinderBase
    {
        private static readonly string s_game_modes_pattern = "75 07 8B 45 90 C6 40 2A 00 83 3D ?? ?? ?? ?? 0F";

        private IntPtr m_game_modes_address;
        private bool success = false;

        public OsuStatusFinder(Process osu) : base(osu)
        {
        }

        public OsuStatusFinder(ISigScan sigScan) : base(sigScan)
        {
        }

        public override bool TryInit()
        {
            SigScan.Reload();
            {
                //Find Game Modes
                m_game_modes_address = SigScan.FindPattern(s_game_modes_pattern, 11);
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