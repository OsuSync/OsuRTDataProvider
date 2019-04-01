[English](https://github.com/KedamaOvO/OsuRTDataProvider/blob/master/README.md)  
# 这是什么
OsuRTDataProvider是一个 [OsuSync](https://github.com/Deliay/osuSync) 插件.  
实验性的支持Osu!Tourney。  
  
OsuRTDataProvider能实时的从[OSU!](https://osu.ppy.sh)中获取以下内容(只支持正式版):
* BeatmapID
* 游戏状态
* Accuracy
* 生命值
* 当前连击
* 300数量
* 100数量
* 50数量
* Miss数量
* Mods
* 播放时间
* 分数
* 游戏模式

OSU!客户端版本要求: **b20171225.2 之后**

# 怎么使用
1. 下载 [OsuSync](https://github.com/Deliay/osuSync)。
2. 下载 [OsuRTDataProvider](https://github.com/KedamaOvO/OsuRTDataProvider-Release/releases)。
3. 复制OsuRTDataProvider 到 {OsuSync Path}/Plugins 目录下。
4. 运行 OsuSync。

# Config.ini
[OsuRTDataProvider.SettingIni]

|Setting Name|Default Value|Description|
| ----- | ----- | ----- |
| ListenInterval | 100 | 监听数据的间隔(单位毫秒)。PS:如果太小可能会卡 |  
|EnableTourneyMode | False | 启用Tourney模式?(实验性) |
| TeamSize | 1 | Tourney client的队伍大小|
| ForceOsuSongsDirectory |  | 强制在指定路径中搜索Beatmap|
| GameMode | Auto |如果ModeFinder初始化失败. 请手动设置游戏模式|
| DisableProcessNotFoundInformation | False | 隐藏"没有发现osu.exe进程"|
| EnableModsChangedAtListening | False | 尝试在非Play状态监听Mods变化|

# API
#### OsuRTDataProviderPlugin ***class***
##### Property
```csharp
        public OsuListenerManager ListenerManager;

        //如果config.ini中的EnableTourneyMode = false,该属性为null.
        public OsuListenerManager[] TourneyListenerManagers;
        public int TourneyListenerManagersCount;
```
#### OsuListenerManager ***class***
##### Event
```csharp
        public delegate void OnBeatmapChangedEvt(Beatmap map);
        public delegate void OnHealthPointChangedEvt(double hp);
        public delegate void OnAccuracyChangedEvt(double acc);
        public delegate void OnComboChangedEvt(int combo);
        public delegate void OnModsChangedEvt(ModsInfo mods);
        public delegate void OnPlayingTimeChangedEvt(int ms);
        public delegate void OnHitCountChangedEvt(int hit);
        public delegate void OnStatusChangedEvt(OsuStatus last_status, OsuStatus status);

        /// <summary>
        /// Playing和Linsten时可用.
        /// 如果Beatmap太老, map.ID = -1.
        /// </summary>
        public event OnBeatmapChangedEvt OnBeatmapChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnHealthPointChangedEvt OnHealthPointChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnAccuracyChangedEvt OnAccuracyChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnComboChangedEvt OnComboChanged;

        /// <summary>
        /// Playing时可用.
        /// Playing->Listen时 , mods = ModsInfo.Empty
        /// </summary>
        public event OnModsChangedEvt OnModsChanged;

        /// <summary>
        /// Playing和Linsten时可用.
        /// </summary>
        public event OnPlayingTimeChangedEvt OnPlayingTimeChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnHitCountChangedEvt On300HitChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnHitCountChangedEvt On100HitChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnHitCountChangedEvt On50HitChanged;

        /// <summary>
        /// Playing时可用.
        /// </summary>
        public event OnHitCountChangedEvt OnMissHitChanged;

        /// <summary>
        /// 任何时候可用.
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
