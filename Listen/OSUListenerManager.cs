using MemoryReader.BeatmapInfo;
using MemoryReader.Memory;
using MemoryReader.Mods;
using NowPlaying;
using Sync;
using System;
using System.Diagnostics;
using System.IO;
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
            Unkonw,
            Listening,
            Playing,
            Editing
        }

        #region Event

        public delegate void OnBeatmapChangedEvt(Beatmap map);

        public delegate void OnBeatmapSetChangedEvt(BeatmapSet set);

        public delegate void OnHealthPointChangedEvt(double hp);

        public delegate void OnAccuracyChangedEvt(double acc);

        public delegate void OnComboChangedEvt(int combo);

        public delegate void OnCurrentModsEvt(ModsInfo mods);

        public delegate void OnPlayingTimeChangedEvt(int ms);

        public delegate void OnHitCountChangedEvt(int hit);

        public delegate void OnStatusChangedEvt(OsuStatus last_status, OsuStatus status);

        public event OnBeatmapChangedEvt OnBeatmapChanged;

        public event OnBeatmapSetChangedEvt OnBeatmapSetChanged;

        public event OnHealthPointChangedEvt OnHealthPointChanged;

        public event OnAccuracyChangedEvt OnAccuracyChanged;

        public event OnComboChangedEvt OnComboChanged;

        public event OnCurrentModsEvt OnCurrentMods;

        public event OnPlayingTimeChangedEvt OnPlayingTimeChanged;

        public event OnHitCountChangedEvt On300HitChanged;

        public event OnHitCountChangedEvt On100HitChanged;

        public event OnHitCountChangedEvt On50HitChanged;

        public event OnHitCountChangedEvt OnMissHitChanged;

        public event OnStatusChangedEvt OnStatusChanged;

        #endregion Event

        private Process m_osu_process;

        private MemoryFinder m_memory_finder = null;

        private OsuStatus m_last_osu_status = OsuStatus.Unkonw;
        private OSUStatus m_now_player_status = new OSUStatus();
        private bool m_stop = false;
        private Task m_listen_task;

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

        public void Init(SyncHost host)
        {
            foreach (var t in host.EnumPluings())
            {
                if (t.getName() == "Now Playing")
                {
                    NowPlayingEvents.Instance.BindEvent<StatusChangeEvent>(p =>
                    {
                        m_now_player_status = p.CurrentStatus;
                    });
                    break;
                }
            }
        }

        public void Start()
        {
            m_stop = false;
            m_listen_task = Task.Run(new Action(ListenLoop));
        }

        public void Stop()
        {
            m_stop = true;
        }

        private void LoadMemorySearch(Process osu)
        {
            m_memory_finder = new MemoryFinder(osu);
        }

        private void ListenLoop()
        {
            Thread.CurrentThread.Name = "MemoryReaderListenThread";

            while (!m_stop)
            {
                OsuStatus status = GetCurrentOsuStatus();

                if (status == OsuStatus.NoFoundProcess || status == OsuStatus.Unkonw)
                {
                    m_osu_process = null;
                    m_memory_finder = null;

                    if (status == OsuStatus.NoFoundProcess)
                        Sync.Tools.IO.CurrentIO.WriteColor(LANG_OSU_NOT_FOUND, ConsoleColor.Red);

                    Process[] process_list;
                    do
                    {
                        process_list = Process.GetProcessesByName("osu!");
                        Thread.Sleep(500);
                    }
                    while (process_list.Length == 0);

                    m_osu_process = process_list[0];
                }

                if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonw)
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
                        double acc = 0.0f;

                        #region if listen

                        if (OnCurrentMods != null) mods = m_memory_finder.GetCurrentMods();
                        if (OnBeatmapSetChanged != null || OnBeatmapChanged != null) beatmapset = m_memory_finder.GetCurrentBeatmapSet();
                        if (OnBeatmapChanged != null) beatmap = m_memory_finder.GetCurrentBeatmap();
                        if (OnPlayingTimeChanged != null) pt = m_memory_finder.GetPlayingTime();
                        if (OnComboChanged != null) cb = m_memory_finder.GetCurrentCombo();
                        if (On300HitChanged != null) n300 = m_memory_finder.Get300Count();
                        if (On100HitChanged != null) n100 = m_memory_finder.Get100Count();
                        if (On50HitChanged != null) n50 = m_memory_finder.Get50Count();
                        if (OnMissHitChanged != null) nmiss = m_memory_finder.GetMissCount();
                        if (OnAccuracyChanged != null) acc = m_memory_finder.GetCurrentAccuracy();
                        if (OnHealthPointChanged != null) hp = m_memory_finder.GetCurrentHP();

                        #endregion if listen

                        try
                        {
                            beatmapset.Artist = m_now_player_status.Artist;
                            beatmapset.Title = m_now_player_status.Title;
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
                                if (m_last_osu_status == OsuStatus.Listening || m_last_osu_status == OsuStatus.Unkonw ||
                                m_last_osu_status == OsuStatus.NoFoundProcess)
                                {
                                    if (!string.IsNullOrWhiteSpace(m_now_player_status.Diff))
                                        beatmap.Diff = m_now_player_status.Diff;
                                    beatmap.Set = beatmapset;
                                    OnBeatmapChanged?.Invoke(beatmap);
                                }

                                if (mods.Mod != m_last_mods.Mod)
                                    OnCurrentMods?.Invoke(mods);

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
                            }
                            else
                            {
                                acc = 0;
                                hp = 0;
                                nmiss = 0;
                                n300 = 0;
                                n100 = 0;
                                n50 = 0;
                                mods.Reset();
                                cb = 0;
                            }

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

                Thread.Sleep(Setting.ListenInterval);
            }
        }

        private OsuStatus GetCurrentOsuStatus()
        {
            if (m_osu_process == null) return OsuStatus.NoFoundProcess;
            if (m_osu_process.HasExited == true) return OsuStatus.NoFoundProcess;

            //string osu_title = m_osu_process != null ? m_osu_process.MainWindowTitle : "unknown title";

            if (m_now_player_status.Status == null) return OsuStatus.Unkonw;

            if (m_now_player_status.Status == "Editing") return OsuStatus.Editing;

            if (m_now_player_status.Status == "Playing") return OsuStatus.Playing;

            if (m_now_player_status.Status == "Watching") return OsuStatus.Playing;
            return OsuStatus.Listening;
        }
    }
}