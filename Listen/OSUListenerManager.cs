using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Memory;
using OsuRTDataProvider.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OsuRTDataProvider.DefaultLanguage;

namespace OsuRTDataProvider.Listen
{
    public class OsuListenerManager
    {
        public enum OsuStatus
        {
            NoFoundProcess,
            Unkonwn,
            Listening,
            Playing,
            Editing,
            Rank
        }

        static private List<Tuple<int, Action>> m_action_list = new List<Tuple<int, Action>>();
        static private Task m_listen_task;
        static private bool m_stop = false;

        #region Event
        public delegate void OnPlayModeChangedEvt(OsuPlayMode last,OsuPlayMode mode);
        public delegate void OnStatusChangedEvt(OsuStatus last_status, OsuStatus status);

        public delegate void OnBeatmapChangedEvt(Beatmap map);
        public delegate void OnHealthPointChangedEvt(double hp);
        public delegate void OnAccuracyChangedEvt(double acc);
        public delegate void OnComboChangedEvt(int combo);
        public delegate void OnModsChangedEvt(ModsInfo mods);
        public delegate void OnPlayingTimeChangedEvt(int ms);
        public delegate void OnHitCountChangedEvt(int hit);

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
        /// Mania: RGB 300
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

        #endregion Event

        private Process m_osu_process;

        private OsuPlayFinder m_play_finder = null;
        private OsuStatusFinder m_status_finder=null;
        private OsuBeatmapFinder m_beatmap_finder = null;
        private OsuPlayModesFinder m_mode_finder = null;
        private int _status_finder_timer = 3000;

        #region last status
        private OsuStatus m_last_osu_status = OsuStatus.Unkonwn;

        private OsuPlayMode m_last_mode = OsuPlayMode.Unknown;
        private Beatmap m_last_beatmap = Beatmap.Empty;
        private ModsInfo m_last_mods = ModsInfo.Empty;

        private double m_last_hp = 0;
        private double m_last_acc = 0;
        private int m_last_combo = 0;
        private int m_playing_time = 0;
        private int m_last_300 = 0;
        private int m_last_100 = 0;
        private int m_last_50 = 0;
        private int m_last_miss = 0;
        private int m_last_geki = 0;
        private int m_last_katu = 0;

        #endregion

        private bool m_is_tourney = false;
        private int m_osu_id = 0;

        private static bool s_random_interval=false;
        private static Random s_random = new Random();

        static OsuListenerManager()
        {
            m_stop = false;
            int interval = Setting.ListenInterval;

            m_listen_task = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "OsuRTDataProviderThread";
                while (!m_stop)
                {
                    for (int i = 0; i < m_action_list.Count; i++)
                    {
                        var action = m_action_list[i];
                        action.Item2();
                    }

                    interval = s_random_interval?s_random.Next(300, 700): Setting.ListenInterval;

                    Thread.Sleep(interval);
                }
            });
        }

        public OsuListenerManager(bool tourney = false, int osuid = 0)
        {
            m_is_tourney = tourney;
            m_osu_id = osuid;
        }

        public void Start()
        {
            m_action_list.Add(new Tuple<int, Action>(m_osu_id, ListenLoopUpdate));
        }

        public void Stop()
        {
            var tuple = m_action_list.Where(t => t.Item1 == m_osu_id).FirstOrDefault();
            m_action_list.Remove(tuple);

            if (m_action_list.Count == 0)
            {
                m_stop = true;
                m_listen_task.Wait();
            }
        }

        private void FindOsuSongPath()
        {
            if (!string.IsNullOrWhiteSpace(Setting.ForceOsuSongsDirectory))
            {
                Setting.SongsPath = Setting.ForceOsuSongsDirectory;
                return;
            }

            string osu_path = Path.GetDirectoryName(m_osu_process.MainModule.FileName);
            string osu_config_file = Path.Combine(osu_path, $"osu!.{Environment.UserName}.cfg");
            var lines = File.ReadLines(osu_config_file);
            string song_path;
            foreach (var line in lines)
            {
                if (line.Contains("BeatmapDirectory"))
                {
                    song_path = line.Split('=')[1].Trim();
                    if (Path.IsPathRooted(song_path))
                        Setting.SongsPath = song_path;
                    else
                        Setting.SongsPath = Path.Combine(osu_path, song_path);
                }
                else if(line.Contains("LastVersion"))
                {
                    Setting.OsuVersion=line.Split('=')[1].Trim();
                    Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]OSU Client Verison:{Setting.OsuVersion}");
                    break;
                }
            }
        }

        #region Get Current Data
        private bool HasMask(ProvideDataMask mask, ProvideDataMask h)
        {
            return (mask & h) == h;
        }

        public ProvideData GetCurrentData(ProvideDataMask mask)
        {
            ProvideData data;

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
                if (OnPlayModeChanged == null) OnPlayModeChanged += (t,t2) => { };
                data.PlayMode = m_last_mode;
            }

            return data;
        }
#endregion

        const long _retry_time = 3000;
        private long _play_finder_timer = 0;
        private long _beatmap_finder_timer = 0;
        private long _mode_finer_timer = 0;


        private void LoadBeatmapFinder()
        {
            if (_beatmap_finder_timer % _retry_time == 0)
            {
                m_beatmap_finder = new OsuBeatmapFinder(m_osu_process);
                if (m_beatmap_finder.TryInit())
                {
                    _beatmap_finder_timer = 0;
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_BEATMAP_FINDER_SUCCESS, m_osu_id), ConsoleColor.Green);
                    return;
                }
                m_beatmap_finder = null;
                Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_BEATMAP_FINDER_FAILED, m_osu_id, _retry_time / 1000), ConsoleColor.Red);
            }
            _beatmap_finder_timer += Setting.ListenInterval;
        }

        private void LoadPlayFinder()
        {
            if (_play_finder_timer % _retry_time == 0 && _play_finder_timer != 0)
            {
                m_play_finder = new OsuPlayFinder(m_osu_process);
                if (m_play_finder.TryInit())
                {
                    _play_finder_timer = 0;
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_PLAY_FINDER_SUCCESS, m_osu_id), ConsoleColor.Green);
                    return;
                }

                m_play_finder = null;
                Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_PLAY_FINDER_FAILED, m_osu_id, _retry_time / 1000), ConsoleColor.Red);
            }
            _play_finder_timer += Setting.ListenInterval;
        }

        private void LoadModeFinder()
        {
            if (_mode_finer_timer % _retry_time == 0 && _mode_finer_timer != 0)
            {
                m_mode_finder = new OsuPlayModesFinder(m_osu_process);
                if (m_mode_finder.TryInit())
                {
                    _mode_finer_timer = 0;
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_MODE_FINDER_SUCCESS, m_osu_id), ConsoleColor.Green);
                    return;
                }

                m_mode_finder = null;
                Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_MODE_FINDER_FAILED, m_osu_id, _retry_time / 1000), ConsoleColor.Red);
            }
            _mode_finer_timer += Setting.ListenInterval;
        }

        private void FindOsuProcess()
        {
            Process[] process_list;
            do
            {
                Thread.Sleep(3000);
                process_list = Process.GetProcessesByName("osu!");

                if (m_stop) return;
                if (process_list.Length == 0) continue;


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
                    FindOsuSongPath();
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_OSU_FOUND, m_osu_id), ConsoleColor.Green);
                }
            }
            while (process_list.Length == 0);
        }

        private void ListenLoopUpdate()
        {
            OsuStatus status = GetCurrentOsuStatus();

            if (status == OsuStatus.NoFoundProcess || status == OsuStatus.Unkonwn)
            {
                m_osu_process = null;
                m_play_finder = null;
                m_status_finder = null;
                m_beatmap_finder = null;
                m_mode_finder = null;

                if (status == OsuStatus.NoFoundProcess)
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_OSU_NOT_FOUND,m_osu_id), ConsoleColor.Red);

                FindOsuProcess();
            }

            if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonwn)
            {
                if (m_beatmap_finder == null)
                {
                    LoadBeatmapFinder();
                }

                if(m_mode_finder == null)
                {
                    LoadModeFinder();
                }

                if (status == OsuStatus.Playing)
                {
                    if (m_play_finder == null)
                    {
                        LoadPlayFinder();
                    }
                }

                if(m_mode_finder!=null)
                {
                    OsuPlayMode mode = OsuPlayMode.Osu;

                    if (OnPlayModeChanged != null) mode = m_mode_finder.GetMode();

                    if (m_last_mode != mode)
                        OnPlayModeChanged(m_last_mode, mode);

                    m_last_mode=mode;
                }

                if (m_play_finder != null)
                {
                    Beatmap beatmap = Beatmap.Empty;
                    ModsInfo mods = ModsInfo.Empty;
                    int cb = 0;
                    int pt = 0;
                    int n300 = 0;
                    int n100 = 0;
                    int n50 = 0;
                    int ngeki = 0;
                    int nkatu = 0;
                    int nmiss = 0;
                    double hp = 0.0;
                    double acc = 0.0;

                    if (OnBeatmapChanged != null) beatmap = m_beatmap_finder.GetCurrentBeatmap(m_osu_id);
                    if (OnPlayingTimeChanged != null) pt = m_play_finder.GetPlayingTime();

                    try
                    {
                        if (beatmap!=Beatmap.Empty&&beatmap != m_last_beatmap)
                        {
                            OnBeatmapChanged?.Invoke(beatmap);
                        }

                        if (status == OsuStatus.Playing)
                        {
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
                        }

                        if (mods != m_last_mods)
                            OnModsChanged?.Invoke(mods);

                        if (hp != m_last_hp)
                            OnHealthPointChanged?.Invoke(hp);

                        if (acc != m_last_acc)
                            OnAccuracyChanged?.Invoke(acc);

                        if (n300 != m_last_300)
                            OnCount300Changed?.Invoke(n300);

                        if (n100 != m_last_100)
                            OnCount100Changed?.Invoke(n100);

                        if (n50 != m_last_50)
                            OnCount50Changed?.Invoke(n50);

                        if (ngeki != m_last_geki)
                            OnCountGekiChanged(ngeki);

                        if (nkatu != m_last_katu)
                            OnCountKatuChanged(nkatu);

                        if (nmiss != m_last_miss)
                            OnCountMissChanged?.Invoke(nmiss);

                        if (cb != m_last_combo)
                            OnComboChanged?.Invoke(cb);

                        if (pt != m_playing_time)
                            OnPlayingTimeChanged?.Invoke(pt);

                        if (status != m_last_osu_status)
                            OnStatusChanged?.Invoke(m_last_osu_status, status);

                        if(!m_is_tourney)
                        {
                            if ((mods.Mod & ModsInfo.Mods.Hidden) == ModsInfo.Mods.Hidden ||
                                (mods.Mod & ModsInfo.Mods.Flashlight) == ModsInfo.Mods.Flashlight)
                                s_random_interval = true;
                            else
                                s_random_interval = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Sync.Tools.IO.CurrentIO.WriteColor(e.ToString(), ConsoleColor.Red);
                    }

                    m_last_beatmap = beatmap;
                    m_last_mods = mods;
                    m_last_hp = hp;
                    m_last_acc = acc;
                    m_last_combo = cb;
                    m_playing_time = pt;
                    m_last_300 = n300;
                    m_last_100 = n100;
                    m_last_50 = n50;
                    m_last_geki = ngeki;
                    m_last_katu = nkatu;
                    m_last_miss = nmiss;
                    m_last_osu_status = status;
                }
            }
        }

        private OsuStatus GetCurrentOsuStatus()
        {
            if (m_osu_process == null) return OsuStatus.NoFoundProcess;
            if (m_osu_process.HasExited == true) return OsuStatus.NoFoundProcess;

            if (m_status_finder == null)
            {
                m_status_finder = new OsuStatusFinder(m_osu_process);
                bool success = false;
                while (!success)
                {
                    if (_status_finder_timer >= 3000)
                    {
                        success = m_status_finder.TryInit();

                        if (m_stop) return OsuStatus.NoFoundProcess;

                        if (m_osu_process.HasExited)
                        {
                            m_status_finder = null;
                            return OsuStatus.Unkonwn;
                        }

                        if (success)
                        {
                            Sync.Tools.IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_INIT_STATUS_FINDER_SUCCESS, m_osu_id, 3), ConsoleColor.Green);
                            break;
                        }
                        Sync.Tools.IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_INIT_STATUS_FINDER_FAILED, m_osu_id, 3), ConsoleColor.Red);
                        _status_finder_timer = 0;
                    }
                    Thread.Sleep(500);
                    _status_finder_timer += 500;
                }
               
            }

            OsuInternalStatus mode = m_status_finder.GetCurrentOsuModes();

            if (mode == OsuInternalStatus.Unknown) return OsuStatus.Unkonwn;

            if (mode == OsuInternalStatus.Edit) return OsuStatus.Editing;

            if (mode == OsuInternalStatus.Play) return OsuStatus.Playing;

            if (mode == OsuInternalStatus.Rank) return OsuStatus.Rank;

            return OsuStatus.Listening;
        }
    }
}