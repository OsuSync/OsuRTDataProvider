[中文](https://github.com/KedamaOvO/OsuRTDataProvider-Release/blob/master/README-CN.md)  
# What is this?
OsuRTDataProvider is an [OsuSync](https://github.com/Deliay/osuSync) plugin.  
Experimental Support OSU!Tourney.  
  
OsuRTDataProvider can be obtained from [OSU!](https://osu.ppy.sh)(Std Only):
* BeatmapID
* Game Status
* Accuracy
* Health Point
* Combo
* Count 300
* Count 100
* Count 50
* Count Miss
* Song Title
* Mods
* Playing Time

OSU! Clinet Version Requirements: **b20171225.2 After**  

# How to use?
1. Download [OsuSync](https://github.com/Deliay/osuSync)
2. Download [OsuRTDataProvider](https://github.com/KedamaOvO/OsuRTDataProvider-Release/releases).
3. Copy OsuRTDataProvider to {OsuSync Path}/Plugins.
4. Run OsuSync.

# Config.ini
[OsuRTDataProvider.SettingIni]  
ListenInterval=33 #ms  
EnableDirectoryImprecisionSearch=True #Increase search range.  
EnableTourneyMode=False #Is tourney client?(Experimental)  
TeamSize=1 #Tourney client team size

# API
#### OsuRTDataProviderPlugin ***class***
##### Property
```csharp
        public OsuListenerManager ListenerManager;

        //If EnableTourneyMode = false in config.ini, return null.
        public OsuListenerManager[] TourneyListenerManagers;
        public int TourneyListenerManagersCount;
```
#### OsuListenerManager ***class***
##### Event
```csharp
        public delegate void OnBeatmapChangedEvt(Beatmap map);
        public delegate void OnBeatmapSetChangedEvt(BeatmapSet set);
        public delegate void OnHealthPointChangedEvt(double hp);
        public delegate void OnAccuracyChangedEvt(double acc);
        public delegate void OnComboChangedEvt(int combo);
        public delegate void OnModsChangedEvt(ModsInfo mods);
        public delegate void OnPlayingTimeChangedEvt(int ms);
        public delegate void OnHitCountChangedEvt(int hit);
        public delegate void OnStatusChangedEvt(OsuStatus last_status, OsuStatus status);

        /// <summary>
        /// Available in Playing and Linsten.
        /// If too old beatmap, map.ID = -1.
        /// </summary>
        public event OnBeatmapChangedEvt OnBeatmapChanged;

        /// <summary>
        /// Available in Playing and Linsten.
        /// If too old beatmap, set.ID = -1.
        /// </summary>
        public event OnBeatmapSetChangedEvt OnBeatmapSetChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHealthPointChangedEvt OnHealthPointChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnAccuracyChangedEvt OnAccuracyChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnComboChangedEvt OnComboChanged;

        /// <summary>
        /// Available in Playing.
        /// if OsuStatus turns Listen , mods = ModsInfo.Empty
        /// </summary>
        public event OnModsChangedEvt OnModsChanged;

        /// <summary>
        /// Available in Playing and Listen.
        /// </summary>
        public event OnPlayingTimeChangedEvt OnPlayingTimeChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt On300HitChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt On100HitChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt On50HitChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt OnMissHitChanged;

        /// <summary>
        /// Get Game Status.
        /// </summary>
        public event OnStatusChangedEvt OnStatusChanged;
```

##### OsuStatus ***enum***
```csharp
        public enum OsuStatus
        {
            NoFoundProcess,
            Unkonwn,
            Listening,
            Playing,
            Editing,
            Rank
        }
```