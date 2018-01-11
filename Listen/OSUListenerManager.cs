using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Handler;
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

        #endregion Event

        private Process m_osu_process;

        private OsuPlayFinder m_play_finder = null;

        private OsuStatus m_last_osu_status = OsuStatus.Unkonwn;

        private BeatmapSet m_last_beatmapset = BeatmapSet.Empty;
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

        private string m_prev_status = string.Empty;

        private bool m_is_tourney = false;
        private int m_osu_id = 0;

        private static bool s_random_interval=false;
        private static Random s_random = new Random();

        static OsuListenerManager()
        {
            m_stop = false;

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
                    if(s_random_interval)
                        Thread.Sleep(s_random.Next(300,700));
                    else
                        Thread.Sleep(Setting.ListenInterval);
                }
            });
            ExitHandler.OnConsloeExit += () => m_stop = true;
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

        Stopwatch _sw = new Stopwatch();
        const long _retry_time = 3000;

        private void LoadMemorySearch(Process osu)
        {
            m_play_finder = new OsuPlayFinder(osu);

            if (_sw.IsRunning)
                _sw.Start();

            if(_sw.ElapsedMilliseconds%_retry_time>= 0)
            {
                if (!m_play_finder.TryInit())
                {
                    if (m_osu_process.HasExited || m_last_osu_status != OsuStatus.Playing)
                    {
                        m_play_finder = null;
                        return;
                    }
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_PLAY_FINDER_FAILED, m_osu_id, _retry_time / 1000), ConsoleColor.Red);
                    _sw.Stop();
                    _sw.Reset();
                }
                else
                {
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_INIT_PLAY_FINDER_SUCCESS, m_osu_id), ConsoleColor.Green);
                }
            }
        }

        private void ListenLoopUpdate()
        {
            OsuStatus status = GetCurrentOsuStatus();

            if (status == OsuStatus.NoFoundProcess || status == OsuStatus.Unkonwn)
            {
                m_osu_process = null;
                m_play_finder = null;
                m_modes_finder = null;

                if (status == OsuStatus.NoFoundProcess)
                    Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_OSU_NOT_FOUND,m_osu_id), ConsoleColor.Red);

                Process[] process_list;
                do
                {
                    Thread.Sleep(3000);
                    process_list = Process.GetProcessesByName("osu!");

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
                        Sync.Tools.IO.CurrentIO.WriteColor(string.Format(LANG_OSU_FOUND,m_osu_id), ConsoleColor.Green);
                }
                while (process_list.Length == 0);
            }

            if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonwn)
            {
                if (status == OsuStatus.Playing)
                {
                    if (m_play_finder == null)
                    {
                        string osu_path = Path.GetDirectoryName(m_osu_process.MainModule.FileName);
                        string osu_config_file = Path.Combine(osu_path, $"osu!.{Environment.UserName}.cfg");
                        var lines=File.ReadLines(osu_config_file);
                        string song_path;
                        foreach(var line in lines)
                        {
                            if(line.Contains("BeatmapDirectory"))
                            {
                                song_path=line.Split('=')[1].Trim();
                                if(Path.IsPathRooted(song_path))
                                    Setting.SongsPath = song_path;
                                else
                                    Setting.SongsPath = Path.Combine(osu_path, song_path);
                                break;
                            }
                        }

                        LoadMemorySearch(m_osu_process);
                    }
                }

                if (m_play_finder != null)
                {
                    BeatmapSet beatmapset = BeatmapSet.Empty;
                    Beatmap beatmap = Beatmap.Empty;
                    ModsInfo mods = ModsInfo.Empty;
                    int cb = 0;
                    int pt = 0;
                    int n300 = 0;
                    int n100 = 0;
                    int n50 = 0;
                    int nmiss = 0;
                    double hp = 0.0;
                    double acc = 0.0;

                    if (OnBeatmapSetChanged != null || OnBeatmapChanged != null) beatmapset = m_play_finder.GetCurrentBeatmapSet(m_osu_id);
                    if (OnBeatmapChanged != null) beatmap = m_play_finder.GetCurrentBeatmap();
                    if (OnPlayingTimeChanged != null) pt = m_play_finder.GetPlayingTime();

                    try
                    {
                        if (beatmapset?.BeatmapSetID != m_last_beatmapset?.BeatmapSetID)
                        {
                            OnBeatmapSetChanged?.Invoke(beatmapset);
                        }

                        if (beatmap.BeatmapID != m_last_beatmap.BeatmapID)
                        {
                            beatmap.Set = beatmapset;
                            OnBeatmapChanged?.Invoke(beatmap);
                        }

                        if (status == OsuStatus.Playing)
                        {
                            if (OnModsChanged != null) mods = m_play_finder.GetCurrentMods();
                            if (OnComboChanged != null) cb = m_play_finder.GetCurrentCombo();
                            if (On300HitChanged != null) n300 = m_play_finder.Get300Count();
                            if (On100HitChanged != null) n100 = m_play_finder.Get100Count();
                            if (On50HitChanged != null) n50 = m_play_finder.Get50Count();
                            if (OnMissHitChanged != null) nmiss = m_play_finder.GetMissCount();
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
                            On300HitChanged?.Invoke(n300);

                        if (n100 != m_last_100)
                            On100HitChanged?.Invoke(n100);

                        if (n50 != m_last_50)
                            On50HitChanged?.Invoke(n50);

                        if (nmiss != m_last_miss)
                            OnMissHitChanged?.Invoke(nmiss);

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

                    m_last_beatmapset = beatmapset;
                    m_last_beatmap = beatmap;
                    m_last_mods = mods;
                    m_last_hp = hp;
                    m_last_acc = acc;
                    m_last_combo = cb;
                    m_playing_time = pt;
                    m_last_300 = n300;
                    m_last_100 = n100;
                    m_last_50 = n50;
                    m_last_miss = nmiss;
                }
                m_last_osu_status = status;
            }
        }

        private OsuModesFinder m_modes_finder;
        private int _status_finder_timer = 3000;

        private OsuStatus GetCurrentOsuStatus()
        {
            if (m_osu_process == null) return OsuStatus.NoFoundProcess;
            if (m_osu_process.HasExited == true) return OsuStatus.NoFoundProcess;

            if (m_modes_finder == null)
            {
                m_modes_finder = new OsuModesFinder(m_osu_process);
                bool success = false;
                while (!success)
                {
                    if (_status_finder_timer >= 3000)
                    {
                        success = m_modes_finder.TryInit();
                        if (m_osu_process.HasExited)
                        {
                            m_modes_finder = null;
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

            OsuModes mode = m_modes_finder.GetCurrentOsuModes();

            if (mode == OsuModes.Unknown) return OsuStatus.Unkonwn;

            if (mode == OsuModes.Edit) return OsuStatus.Editing;

            if (mode == OsuModes.Play) return OsuStatus.Playing;

            if (mode == OsuModes.Rank) return OsuStatus.Rank;

            return OsuStatus.Listening;
        }
    }
}