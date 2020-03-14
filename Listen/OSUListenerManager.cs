using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Helper;
using OsuRTDataProvider.Memory;
using OsuRTDataProvider.Mods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static OsuRTDataProvider.DefaultLanguage;

namespace OsuRTDataProvider.Listen
{
    public class OsuListenerManager
    {
        [Flags]
        public enum OsuStatus:UInt64
        {
            NoFoundProcess = 1ul<<0,
            Unkonwn = 1ul<<1,
            SelectSong = 1ul<<2,
            Playing = 1ul<<3,
            Editing = 1ul<<4,
            Rank = 1ul<<5,
            MatchSetup = 1ul<<6,
            Lobby = 1ul<<7,
            Idle = 1ul<<8,
        }

        private static Dictionary<string, OsuPlayMode> s_game_mode_map = new Dictionary<string, OsuPlayMode>(4)
        {
            ["Osu"] = OsuPlayMode.Osu,
            ["Taiko"] = OsuPlayMode.Taiko,
            ["CatchTheBeat"] = OsuPlayMode.CatchTheBeat,
            ["Mania"] = OsuPlayMode.Mania,
        };

        static private List<Tuple<int, Action>> s_listen_update_list = new List<Tuple<int, Action>>();
        static private Task s_listen_task;
        static private bool s_stop_flag = false;

        #region Event

        public delegate void OnPlayModeChangedEvt(OsuPlayMode last, OsuPlayMode mode);

        public delegate void OnStatusChangedEvt(OsuStatus last_status, OsuStatus status);

        public delegate void OnBeatmapChangedEvt(Beatmap map);

        public delegate void OnHealthPointChangedEvt(double hp);

        public delegate void OnAccuracyChangedEvt(double acc);

        public delegate void OnComboChangedEvt(int combo);

        public delegate void OnModsChangedEvt(ModsInfo mods);

        public delegate void OnPlayingTimeChangedEvt(int ms);

        public delegate void OnHitCountChangedEvt(int hit);

        public delegate void OnErrorStatisticsChangedEvt(ErrorStatisticsResult result);

        public delegate void OnPlayerChangedEvt(string player);

        public delegate void OnHitEventsChangedEvt(PlayType playType, List<HitEvent> hitEvents);

        /// <summary>
        /// Available in Playing and Linsten.
        /// If too old beatmap, map.ID = -1.
        /// </summary>
        public event Action<int> OnScoreChanged;

        /// <summary>
        /// Available in Linsten.
        /// </summary>
        public event OnPlayModeChangedEvt OnPlayModeChanged;

        /// <summary>
        /// Available in Playing and Linsten.
        /// If too old beatmap, map.ID = -1.
        /// </summary>
        public event OnBeatmapChangedEvt OnBeatmapChanged;

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
        public event OnHitCountChangedEvt OnCount300Changed;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt OnCount100Changed;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt OnCount50Changed;

        /// <summary>
        /// Mania: 300g
        /// </summary>
        public event OnHitCountChangedEvt OnCountGekiChanged;

        /// <summary>
        /// Mania: 200
        /// </summary>
        public event OnHitCountChangedEvt OnCountKatuChanged;

        /// <summary>
        /// Available in Playing.
        /// </summary>
        public event OnHitCountChangedEvt OnCountMissChanged;

        /// <summary>
        /// Get Game Status.
        /// </summary>
        public event OnStatusChangedEvt OnStatusChanged;

        /// <summary>
        /// Get ErrorStatistics(UnstableRate and Error Hit).
        /// </summary>
        public event OnErrorStatisticsChangedEvt OnErrorStatisticsChanged;

        /// <summary>
        /// Get player name in playing.
        /// </summary>
        public event OnPlayerChangedEvt OnPlayerChanged;

        /// <summary>
        /// Get play type and hit events in playing.
        /// </summary>
        public event OnHitEventsChangedEvt OnHitEventsChanged;
        #endregion Event

        private Process m_osu_process;

        private OsuPlayFinder m_play_finder = null;
        private OsuStatusFinder m_status_finder = null;
        private OsuBeatmapFinder m_beatmap_finder = null;
        private OsuPlayModeFinder m_mode_finder = null;
        private OsuHitEventFinder m_hit_event_finder = null;

        #region last status

        private OsuStatus m_last_osu_status = OsuStatus.Unkonwn;
        private string m_last_playername = string.Empty;
        private OsuPlayMode m_last_mode = OsuPlayMode.Unknown;
        private Beatmap m_last_beatmap = Beatmap.Empty;
        private ModsInfo m_last_mods = ModsInfo.Empty;

        private double m_last_hp = 0;
        private double m_last_acc = 0;
        private ErrorStatisticsResult m_last_error_statistics = ErrorStatisticsResult.Empty;
        private int m_last_combo = 0;
        private int m_playing_time = 0;
        private int m_last_300 = 0;
        private int m_last_100 = 0;
        private int m_last_50 = 0;
        private int m_last_miss = 0;
        private int m_last_geki = 0;
        private int m_last_katu = 0;
        

        private int m_last_score = 0;

        private List<HitEvent> m_hit_events = new List<HitEvent>();
        private PlayType m_play_type = PlayType.Unknown;

        #endregion last status

        private readonly bool m_is_tourney = false;
        private readonly int m_osu_id = 0;

        #region OsuRTDataProviderThread

        static OsuListenerManager()
        {
            s_stop_flag = false;

            //Listen Thread
            s_listen_task = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "OsuRTDataProviderThread";
                Thread.Sleep(2000);
                while (!s_stop_flag)
                {
                    for (int i = 0; i < s_listen_update_list.Count; i++)
                    {
                        var action = s_listen_update_list[i];
                        action.Item2();
                    }

                    Thread.Sleep(Setting.ListenInterval);
                }
            });
        }
        #endregion

        public OsuListenerManager(bool tourney = false, int osuid = 0)
        {
            m_is_tourney = tourney;
            m_osu_id = osuid;
        }

        public void Start()
        {
            s_listen_update_list.Add(new Tuple<int, Action>(m_osu_id, ListenLoopUpdate));
        }

        public void Stop()
        {
            var tuple = s_listen_update_list.Where(t => t.Item1 == m_osu_id).FirstOrDefault();
            s_listen_update_list.Remove(tuple);

            if (s_listen_update_list.Count == 0)
            {
                s_stop_flag = true;
                s_listen_task.Wait();
            }
        }

        #region Get Current Data
        
        private bool HasMask(ProvideDataMask mask, ProvideDataMask h)
        {
            return (mask & h) == h;
        }

        public ProvideData GetCurrentData(ProvideDataMask mask)
        {
            ProvideData data = new ProvideData();

            data.ClientID = m_osu_id;
            data.Status = m_last_osu_status;

            data.PlayMode = OsuPlayMode.Unknown;
            data.Beatmap = Beatmap.Empty;
            data.Mods = ModsInfo.Empty;

            data.Combo = 0;
            data.Count300 = 0;
            data.Count100 = 0;
            data.Count50 = 0;
            data.CountMiss = 0;
            data.CountGeki = 0;
            data.CountKatu = 0;
            data.HealthPoint = 0;
            data.Accuracy = 0;
            data.Time = 0;
            data.Score = 0;

            data.PlayType = PlayType.Unknown;
            data.HitEvents = new List<HitEvent>();

            if (HasMask(mask, ProvideDataMask.Beatmap))
            {
                if (OnBeatmapChanged == null) OnBeatmapChanged += (t) => { };
                data.Beatmap = m_last_beatmap;
            }

            if (HasMask(mask, ProvideDataMask.HealthPoint))
            {
                if (OnHealthPointChanged == null) OnHealthPointChanged += (t) => { };
                data.HealthPoint = m_last_hp;
            }

            if (HasMask(mask, ProvideDataMask.Accuracy))
            {
                if (OnAccuracyChanged == null) OnAccuracyChanged += (t) => { };
                data.Accuracy = m_last_acc;
            }

            if (HasMask(mask, ProvideDataMask.Combo))
            {
                if (OnComboChanged == null) OnComboChanged += (t) => { };
                data.Combo = m_last_combo;
            }

            if (HasMask(mask, ProvideDataMask.Count300))
            {
                if (OnCount300Changed == null) OnCount300Changed += (t) => { };
                data.Count300 = m_last_300;
            }

            if (HasMask(mask, ProvideDataMask.Count100))
            {
                if (OnCount100Changed == null) OnCount100Changed += (t) => { };
                data.Count100 = m_last_100;
            }

            if (HasMask(mask, ProvideDataMask.Count50))
            {
                if (OnCount50Changed == null) OnCount50Changed += (t) => { };
                data.Count50 = m_last_50;
            }

            if (HasMask(mask, ProvideDataMask.CountMiss))
            {
                if (OnCountMissChanged == null) OnCountMissChanged += (t) => { };
                data.CountMiss = m_last_miss;
            }

            if (HasMask(mask, ProvideDataMask.CountGeki))
            {
                if (OnCountGekiChanged == null) OnCountGekiChanged += (t) => { };
                data.CountGeki = m_last_geki;
            }

            if (HasMask(mask, ProvideDataMask.CountKatu))
            {
                if (OnCountKatuChanged == null) OnCountKatuChanged += (t) => { };
                data.CountKatu = m_last_katu;
            }

            if (HasMask(mask, ProvideDataMask.Time))
            {
                if (OnPlayingTimeChanged == null) OnPlayingTimeChanged += (t) => { };
                data.Time = m_playing_time;
            }

            if (HasMask(mask, ProvideDataMask.Mods))
            {
                if (OnModsChanged == null) OnModsChanged += (t) => { };
                data.Mods = m_last_mods;
            }

            if (HasMask(mask, ProvideDataMask.GameMode))
            {
                if (OnPlayModeChanged == null) OnPlayModeChanged += (t, t2) => { };
                data.PlayMode = m_last_mode;
            }

            if (HasMask(mask, ProvideDataMask.Score))
            {
                if (OnScoreChanged == null) OnScoreChanged += (s) => { };
                data.Score = m_last_score;
            }

            if (HasMask(mask, ProvideDataMask.ErrorStatistics))
            {
                if (OnErrorStatisticsChanged == null) OnErrorStatisticsChanged += (e) => { };
                data.ErrorStatistics = m_last_error_statistics;
            }

            if (HasMask(mask, ProvideDataMask.Playername))
            {
                if (OnPlayerChanged == null) OnPlayerChanged += (e) => { };
                data.Playername = m_last_playername;
            }

            if (HasMask(mask, ProvideDataMask.HitEvent))
            {
                if (OnHitEventsChanged == null) OnHitEventsChanged += (t, l) => { };
                data.PlayType = m_play_type;
                data.HitEvents = m_hit_events;
            }

            return data;
        }

        #endregion Get Current Data

        #region Init Finder
        private const long RETRY_INTERVAL = 3000;

        private Dictionary<Type, long> finder_timer_dict = new Dictionary<Type, long>();
        private T InitFinder<T>(string success_fmt, string failed_fmt) where T : OsuFinderBase
        {
            if (!finder_timer_dict.ContainsKey(typeof(T)))
                finder_timer_dict.Add(typeof(T), 0);

            T finder = null;
            long timer = finder_timer_dict[typeof(T)];

            if (timer % RETRY_INTERVAL == 0)
            {
                finder = typeof(T).GetConstructors()[0].Invoke(new object[] { m_osu_process }) as T;
                if (finder.TryInit())
                {
                    timer = 0;
                    Logger.Info(string.Format(success_fmt, m_osu_id));
                    return finder;
                }

                finder = null;
                Logger.Error(string.Format(failed_fmt, m_osu_id, RETRY_INTERVAL / 1000));
            }
            timer += Setting.ListenInterval;
            finder_timer_dict[typeof(T)] = timer;
            return finder;
        }
        #endregion

        #region Find Osu Setting
        private long find_osu_process_timer = 0;
        private const long FIND_OSU_RETRY_INTERVAL = 5000;

        private void FindOsuProcess()
        {
            if (find_osu_process_timer > FIND_OSU_RETRY_INTERVAL)
            {
                find_osu_process_timer = 0;
                Process[] process_list;

                process_list = Process.GetProcessesByName("osu!");

                if (s_stop_flag) return;
                if (process_list.Length != 0)
                {
                    if (m_is_tourney)
                    {
                        foreach (var p in process_list)
                        {
                            if (p.MainWindowTitle.Contains($"Client {m_osu_id}"))
                            {
                                m_osu_process = p;
                                break;
                            }
                        }
                    }
                    else
                    {
                        m_osu_process = process_list[0];
                    }

                    if (m_osu_process != null)
                    {
                        FindSongPathAndUsername();
                        Logger.Info(string.Format(LANG_OSU_FOUND, m_osu_id));
                        return;
                    }
                }
                find_osu_process_timer = 0;
                if (!Setting.DisableProcessNotFoundInformation)
                    Logger.Error(string.Format(LANG_OSU_NOT_FOUND, m_osu_id));
            }
            find_osu_process_timer += Setting.ListenInterval;
        }

        private void FindSongPathAndUsername()
        {
            string osu_path = "";
            find_osu_filename:
            try
            {
                osu_path = Path.GetDirectoryName(m_osu_process.MainModule.FileName);
            }
            catch (Win32Exception e)
            {
                if(Setting.DebugMode)
                    Logger.Warn($"Win32Exception: {e.ToString()}");
                Logger.Warn("Can't get osu path, Retry after 2 seconds.");
                Thread.Sleep(2000);
                goto find_osu_filename;
            }
            string osu_config_file = Path.Combine(osu_path, $"osu!.{PathHelper.WindowsPathStrip(Environment.UserName)}.cfg");
            string song_path;


            try
            {
                using (var fs = File.OpenRead(osu_config_file))
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (line.StartsWith("BeatmapDirectory"))
                        {
                            if (Directory.Exists(Setting.ForceOsuSongsDirectory))
                            {
                                Setting.SongsPath = Setting.ForceOsuSongsDirectory;
                            }
                            else
                            {
                                Logger.Info($"ForceOsuSongsDirectory: {Setting.ForceOsuSongsDirectory}");
                                Logger.Info($"The ForceOsuSongsDirectory does not exist, try searching for the songs path.");
                                song_path = line.Split('=')[1].Trim();
                                if (Path.IsPathRooted(song_path))
                                    Setting.SongsPath = song_path;
                                else
                                    Setting.SongsPath = Path.Combine(osu_path, song_path);
                            }
                        }
                        else if (line.StartsWith("Username"))
                        {
                            Setting.Username = line.Split('=')[1].Trim();
                        }
                        else if (line.StartsWith("LastVersion")&&!line.StartsWith("LastVersionPermissionsFailed"))
                        {
                            Setting.OsuVersion = line.Split('=')[1].Trim();
                            Logger.Info($"OSU Client Verison:{Setting.OsuVersion} ORTDP Version:{OsuRTDataProviderPlugin.VERSION}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Exception: {e.ToString()}");
            }

            if (string.IsNullOrWhiteSpace(Setting.SongsPath))
            {
                Logger.Warn($"Search failed, use default songs path.");
                Setting.SongsPath = Path.Combine(osu_path,"Songs");
            }
            Logger.Info($"Osu Path: {osu_path}");
            Logger.Info($"Beatmap Path: {Setting.SongsPath}");
        }
        #endregion

        private void ListenLoopUpdate()
        {
            OsuStatus status = GetCurrentOsuStatus();

            if (status == OsuStatus.NoFoundProcess)
            {
                m_osu_process = null;
                m_play_finder = null;
                m_status_finder = null;
                m_beatmap_finder = null;
                m_mode_finder = null;
                m_hit_event_finder = null;

                FindOsuProcess();
            }

            //Waiting for osu to start
            if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonwn)
            {
                //Wait for player to playing
                if (status == OsuStatus.Playing)
                {
                    if (m_play_finder == null)
                    {
                        m_play_finder = InitFinder<OsuPlayFinder>(LANG_INIT_PLAY_FINDER_SUCCESS, LANG_INIT_PLAY_FINDER_FAILED);
                    }

                    if (Setting.GameMode == "Auto" && m_mode_finder == null)
                    {
                        m_mode_finder = InitFinder<OsuPlayModeFinder>(LANG_INIT_MODE_FINDER_SUCCESS, LANG_INIT_MODE_FINDER_FAILED);
                    }

                    if (m_hit_event_finder == null)
                    {
                        m_hit_event_finder = InitFinder<OsuHitEventFinder>(LANG_INIT_HIT_EVENT_SUCCESS, LANG_INIT_HIT_EVENT_FAIL);
                    }
                }

                if (m_beatmap_finder == null)
                {
                    m_beatmap_finder = InitFinder<OsuBeatmapFinder>(LANG_INIT_BEATMAP_FINDER_SUCCESS, LANG_INIT_BEATMAP_FINDER_FAILED);
                }

                if (m_mode_finder != null)
                {
                    OsuPlayMode mode = OsuPlayMode.Osu;

                    if (OnPlayModeChanged != null) mode = m_mode_finder.GetMode();

                    if (m_last_mode != mode)
                        OnPlayModeChanged?.Invoke(m_last_mode, mode);

                    m_last_mode = mode;
                }
                else
                {
                    if (Setting.GameMode != "Auto")
                    {
                        if (s_game_mode_map.TryGetValue(Setting.GameMode, out var mode))
                            if (m_last_mode != mode)
                                OnPlayModeChanged?.Invoke(m_last_mode, mode);

                        m_last_mode = mode;
                    }
                }

                if (OnHitEventsChanged != null && m_hit_event_finder != null)
                {
                    // Hit events should work with playing time
                    if (OnPlayingTimeChanged == null) OnPlayingTimeChanged += (t) => { };

                    bool hasChanged;
                    m_hit_event_finder.GetHitEvents(status, m_playing_time, out m_play_type, out m_hit_events, out hasChanged);
                    if (hasChanged)
                    {
                        OnHitEventsChanged?.Invoke(m_play_type, m_hit_events);
                    }
                }

                if (m_play_finder != null)
                {
                    Beatmap beatmap = Beatmap.Empty;
                    ModsInfo mods = ModsInfo.Empty;
                    ErrorStatisticsResult error_statistics = ErrorStatisticsResult.Empty;
                    int cb = 0;
                    int pt = 0;
                    int n300 = 0;
                    int n100 = 0;
                    int n50 = 0;
                    int ngeki = 0;
                    int nkatu = 0;
                    int nmiss = 0;
                    int score = 0;
                    double hp = 0.0;
                    double acc = 0.0;
                    string playername = Setting.Username;

                    try
                    {
                        if (OnPlayingTimeChanged != null) pt = m_play_finder.GetPlayingTime();
                        if (OnBeatmapChanged != null) beatmap = m_beatmap_finder.GetCurrentBeatmap(m_osu_id);
                        if (Setting.EnableModsChangedAtListening && status != OsuStatus.Playing)
                            if (OnModsChanged != null) mods = m_play_finder.GetCurrentModsAtListening();

                        if (beatmap != Beatmap.Empty && beatmap != m_last_beatmap)
                        {
                            OnBeatmapChanged?.Invoke(beatmap);
                        }

                        if (status == OsuStatus.Playing)
                        {
                            if (OnErrorStatisticsChanged != null) error_statistics = m_play_finder.GetUnstableRate();
                            if (OnModsChanged != null) mods = m_play_finder.GetCurrentMods();
                            if (OnComboChanged != null) cb = m_play_finder.GetCurrentCombo();
                            if (OnCount300Changed != null) n300 = m_play_finder.Get300Count();
                            if (OnCount100Changed != null) n100 = m_play_finder.Get100Count();
                            if (OnCount50Changed != null) n50 = m_play_finder.Get50Count();
                            if (OnCountGekiChanged != null) ngeki = m_play_finder.GetGekiCount();
                            if (OnCountKatuChanged != null) nkatu = m_play_finder.GetKatuCount();
                            if (OnCountMissChanged != null) nmiss = m_play_finder.GetMissCount();
                            if (OnAccuracyChanged != null) acc = m_play_finder.GetCurrentAccuracy();
                            if (OnHealthPointChanged != null) hp = m_play_finder.GetCurrentHP();
                            if (OnScoreChanged != null) score = m_play_finder.GetCurrentScore();
                            if (OnPlayerChanged != null) playername = m_play_finder.GetCurrentPlayerName();
                        }

                        if (status != m_last_osu_status)
                            OnStatusChanged?.Invoke(m_last_osu_status, status);

                        if (mods != ModsInfo.Empty && !ModsInfo.VaildMods(mods))
                            mods = m_last_mods;

                        if (mods != m_last_mods)
                            OnModsChanged?.Invoke(mods);

                        if (hp != m_last_hp)
                            OnHealthPointChanged?.Invoke(hp);

                        if (acc != m_last_acc)
                            OnAccuracyChanged?.Invoke(acc);

                        if (!error_statistics.Equals(m_last_error_statistics))
                            OnErrorStatisticsChanged?.Invoke(error_statistics);

                        if (playername != m_last_playername)
                            OnPlayerChanged(playername);

                        if (score != m_last_score)
                            OnScoreChanged?.Invoke(score);

                        if (n300 != m_last_300)
                            OnCount300Changed?.Invoke(n300);

                        if (n100 != m_last_100)
                            OnCount100Changed?.Invoke(n100);

                        if (n50 != m_last_50)
                            OnCount50Changed?.Invoke(n50);

                        if (ngeki != m_last_geki)
                            OnCountGekiChanged?.Invoke(ngeki);

                        if (nkatu != m_last_katu)
                            OnCountKatuChanged?.Invoke(nkatu);

                        if (nmiss != m_last_miss)
                            OnCountMissChanged?.Invoke(nmiss);

                        if (cb != m_last_combo)
                            OnComboChanged?.Invoke(cb);

                        if (pt != m_playing_time)
                            OnPlayingTimeChanged?.Invoke(pt);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }

                    m_last_beatmap = beatmap;
                    m_last_mods = mods;
                    m_last_hp = hp;
                    m_last_acc = acc;
                    m_last_error_statistics = error_statistics;
                    m_last_combo = cb;
                    m_playing_time = pt;
                    m_last_300 = n300;
                    m_last_100 = n100;
                    m_last_50 = n50;
                    m_last_geki = ngeki;
                    m_last_katu = nkatu;
                    m_last_miss = nmiss;
                    m_last_score = score;
                    m_last_osu_status = status;
                    m_last_playername = playername;
                }
            }
        }

#if DEBUG
        private OsuInternalStatus m_last_test = OsuInternalStatus.Menu;
#endif
        private OsuStatus GetCurrentOsuStatus()
        {
            try
            {
                if (m_osu_process == null) return OsuStatus.NoFoundProcess;
                if (m_osu_process.HasExited == true) return OsuStatus.NoFoundProcess;

                if (m_status_finder == null)
                {
                    m_status_finder = InitFinder<OsuStatusFinder>(
                        LANG_INIT_STATUS_FINDER_SUCCESS,
                        LANG_INIT_STATUS_FINDER_FAILED
                        );
                    return OsuStatus.Unkonwn;
                }
            }
            catch (Win32Exception e)
            {
                Logger.Error($":{e.Message}");
            }

            OsuInternalStatus mode = m_status_finder.GetCurrentOsuModes();

#if DEBUG
            if (mode != m_last_test)
            {
                Logger.Info($"Internal Status:{mode}");
            }
            m_last_test = mode;
#endif

            switch (mode)
            {
                case OsuInternalStatus.Edit:
                    return OsuStatus.Editing;

                case OsuInternalStatus.Play:
                    return OsuStatus.Playing;

                case OsuInternalStatus.RankingTagCoop:
                case OsuInternalStatus.RankingTeam:
                case OsuInternalStatus.RankingVs:
                case OsuInternalStatus.Rank:
                    return OsuStatus.Rank;

                case OsuInternalStatus.BeatmapImport:
                case OsuInternalStatus.Menu:
                case OsuInternalStatus.OnlineSelection:
                    return OsuStatus.Idle;

                case OsuInternalStatus.MatchSetup:
                    return OsuStatus.MatchSetup;

                case OsuInternalStatus.Lobby:
                    return OsuStatus.Lobby;

                case OsuInternalStatus.SelectEdit:
                case OsuInternalStatus.SelectMulti:
                case OsuInternalStatus.SelectPlay:
                    return OsuStatus.SelectSong;

                default:
                    return OsuStatus.Unkonwn;
            }
        }
    }
}