using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace OsuRTDataProvider.Listen
{
    public struct ProvideData
    {
        public OsuStatus current_status;

        public int client_id;
        public double hp;
        public double acc;
        public int last_combo;
        public int playing_time;
        public int count_300;
        public int count_100;
        public int count_50;
        public int count_miss;

        public Beatmap beatmap;
        public ModsInfo mods;
    }
}
