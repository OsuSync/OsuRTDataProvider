using MemoryReader.BeatmapInfo;
using MemoryReader.Memory;
using MemoryReader.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MemoryReader.DefaultLanguage;

namespace MemoryReader.Listen
{
    public class OSUListenerManager
    {
        [Flags]
        public enum OsuStatus
        {
            NoFoundProcess,
            Unkonwn,
            Listening,
            Playing,
            Editing,
            Rank
        }

        static private LinkedList<Tuple<int, Action>> m_action_list = new LinkedList<Tuple<int, Action>>();
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
        /// if OsuStatus turns Listen , mods = Mods.Unknown
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

        private OsuPlayFinder m_memory_finder = null;

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

        static OSUListenerManager()
        {
            m_stop = false;
            m_listen_task = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "MemoryReaderListenThread";
                while (!m_stop)
                {
                    foreach (var action in m_action_list)
                        action.Item2();
                    Thread.Sleep(Setting.ListenInterval);
                }
            });
        }

        public OSUListenerManager(bool tourney = false, int osuid = 0)
        {
            m_is_tourney = tourney;
            m_osu_id = osuid;
        }

        public void Start()
        {
            m_action_list.AddLast(new Tuple<int, Action>(m_osu_id, ListenLoop));
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

        private void LoadMemorySearch(Process osu)
        {
            m_memory_finder = new OsuPlayFinder(osu);

            while (!m_memory_finder.TryInit())
            {
                if (m_osu_process.HasExited || m_last_osu_status != OsuStatus.Playing)
                {
                    m_memory_finder = null;
                    return;
                }
                Thread.Sleep(500);
            }
        }

        private void ListenLoop()
        {
            OsuStatus status = GetCurrentOsuStatus();

            if (status == OsuStatus.NoFoundProcess || status == OsuStatus.Unkonwn)
            {
                m_osu_process = null;
                m_memory_finder = null;
                m_modes_finder = null;

                if (status == OsuStatus.NoFoundProcess)
                    Sync.Tools.IO.CurrentIO.WriteColor(LANG_OSU_NOT_FOUND, ConsoleColor.Red);

                Process[] process_list;
                do
                {
                    Thread.Sleep(500);
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
                        Sync.Tools.IO.CurrentIO.WriteColor(LANG_OSU_FOUND, ConsoleColor.Green);
                }
                while (process_list.Length == 0);
            }

            if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonwn)
            {
                if (status == OsuStatus.Playing)
                {
                    if (m_memory_finder == null)
                    {
                        Setting.SongsPath = Path.Combine(Path.GetDirectoryName(m_osu_process.MainModule.FileName), "Songs");
                        LoadMemorySearch(m_osu_process);
                    }
                }

                if (m_memory_finder != null)
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

                    if (OnBeatmapSetChanged != null || OnBeatmapChanged != null) beatmapset = m_memory_finder.GetCurrentBeatmapSet();
                    if (OnBeatmapChanged != null) beatmap = m_memory_finder.GetCurrentBeatmap();
                    if (OnPlayingTimeChanged != null) pt = m_memory_finder.GetPlayingTime();

                    try
                    {
                        if (beatmapset.BeatmapSetID != m_last_beatmapset.BeatmapSetID)
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
                            if (OnModsChanged != null) mods = m_memory_finder.GetCurrentMods();
                            if (OnComboChanged != null) cb = m_memory_finder.GetCurrentCombo();
                            if (On300HitChanged != null) n300 = m_memory_finder.Get300Count();
                            if (On100HitChanged != null) n100 = m_memory_finder.Get100Count();
                            if (On50HitChanged != null) n50 = m_memory_finder.Get50Count();
                            if (OnMissHitChanged != null) nmiss = m_memory_finder.GetMissCount();
                            if (OnAccuracyChanged != null) acc = m_memory_finder.GetCurrentAccuracy();
                            if (OnHealthPointChanged != null) hp = m_memory_finder.GetCurrentHP();
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

        private OsuStatus GetCurrentOsuStatus()
        {
            if (m_osu_process == null) return OsuStatus.NoFoundProcess;
            if (m_osu_process.HasExited == true) return OsuStatus.NoFoundProcess;

            if (m_modes_finder == null)
            {
                m_modes_finder = new OsuModesFinder(m_osu_process);
                while (!m_modes_finder.TryInit())
                {
                    if (m_osu_process.HasExited)
                    {
                        m_modes_finder = null;
                        return OsuStatus.Unkonwn;
                    }
                    Thread.Sleep(500);
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