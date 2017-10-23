using MemoryReader.BeatmapInfo;
using MemoryReader.Listen.Interface;
using MemoryReader.Mods;
using System;
using static MemoryReader.Listen.OSUListenerManager;

namespace MemoryReader.Listen
{
    internal class OSUTestListener : IOSUListener
    {
        public void OnAccuracyChange(double acc)
        {
            Sync.Tools.IO.CurrentIO.Write(String.Format("当前Acc:{0}", acc));
        }

        public void OnComboChange(int combo)
        {
            //Sync.Tools.IO.CurrentIO.Write(String.Format("当前Combo:{0}", combo));
        }

        public void OnCurrentBeatmapChange(Beatmap beatmap)
        {
            Sync.Tools.IO.CurrentIO.Write(String.Format("当前Beatmap ID:{0} Path:{1}", beatmap.BeatmapID,beatmap.LocationFile));
        }

        public void OnCurrentBeatmapSetChange(BeatmapSet beatmap)
        {
            Sync.Tools.IO.CurrentIO.Write(String.Format("当前BeatmapSet ID:{0}", beatmap.BeatmapSetID));
        }

        public void OnCurrentModsChange(ModsInfo mod)
        {
            Sync.Tools.IO.CurrentIO.Write(String.Format("当前Mods:{0}", mod.ShortName));
        }

        public void OnHPChange(double hp)
        {
            //Sync.Tools.IO.CurrentIO.Write(String.Format("当前HP:{0}", hp));
        }

        public void OnStatusChange(OsuStatus last_status,OsuStatus status)
        {
            Sync.Tools.IO.CurrentIO.Write(String.Format("当前状态:{0}", status));
        }
    }
}