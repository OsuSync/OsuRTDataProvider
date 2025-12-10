using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Mods;
using System;
using System.Collections.Generic;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace OsuRTDataProvider.Listen
{
    [Flags]
    public enum ProvideDataMask : uint
    {
        HealthPoint = 1u << 0,
        Accuracy = 1u << 1,
        Combo = 1u << 2,
        Count300 = 1u << 3,
        Count100 = 1u << 4,
        Count50 = 1u << 5,
        CountMiss = 1u << 6,
        CountGeki = 1u << 7,
        CountKatu = 1u << 8,
        Time = 1u << 9,
        Mods = 1u << 10,
        GameMode = 1u << 11,
        Beatmap = 1u << 12,
        Score = 1u << 13,
        ErrorStatistics =  1u<<14,
        Playername = 1u<<15,
        HitEvent = 1u << 16,

        HitCount = Count300 | Count100 | Count50 | CountMiss | CountGeki | CountKatu,
    }

    public class ProvideData
    {
        public int ClientID;
        public OsuStatus Status;
        public string Playername;

        public OsuPlayMode PlayMode;
        public Beatmap Beatmap;
        public ModsInfo Mods;
        public ErrorStatisticsResult ErrorStatistics;

        public double HealthPoint;
        public double Accuracy;
        public int Combo;
        public int Count300;
        public int Count100;
        public int Count50;
        public int CountMiss;
        public int CountGeki;
        public int CountKatu;
        public int Time;
        public int Score;

        public PlayType PlayType;
        public List<HitEvent> HitEvents;
    }
}