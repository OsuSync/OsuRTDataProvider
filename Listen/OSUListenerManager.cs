using MemoryReader.BeatmapInfo;
using MemoryReader.Memory;
using MemoryReader.Mods;
using NowPlaying;
using Sync;
using Sync.Tools;
using System;
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

        private MemoryFinder m_memory_finder;

        private OsuStatus m_last_osu_status = OsuStatus.Unkonw;
        private OSUStatus m_now_player_status = new OSUStatus();
        private bool m_stop = false;
        private Task m_listen_task;

        private BeatmapSet m_last_beatmapset = new BeatmapSet(0);
        private Beatmap m_last_beatmap = new Beatmap(0);
        private ModsInfo m_last_mods = new ModsInfo();

        private double m_last_hp = 0;
        private double m_last_acc = 0;
        private int m_last_combo = 0;
        private int m_playing_time = 0;
        private int m_last_300=0;
        private int m_last_100 = 0;
        private int m_last_50 = 0;
        private int m_last_miss = 0;

        private string m_prev_status = string.Empty;

        public OSUListenerManager()
        {
            
        }

        public void Init(SyncHost host)
        {
            foreach (var t in host.EnumPluings())
            {
                if (t.getName() == "Now Playing")
                {
                    ((NowPlaying.NowPlaying)t).EventBus.BindEvent<StatusChangeEvent>(p =>
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
            UInt32 count = 0;

            while (!m_stop)
            {
                OsuStatus status = GetCurrentOsuStatus();

                if (status != OsuStatus.NoFoundProcess && status != OsuStatus.Unkonw)
                {
                    if (status == OsuStatus.Playing)
                    {
                        if (m_memory_finder==null)
                        {
                            Process[] process_list;
                            do
                            {
                                process_list = Process.GetProcessesByName("osu!");
                                Thread.Sleep(100);
                            }
                            while (process_list.Length == 0);
                            Setting.SongsPath = Path.Combine(Path.GetDirectoryName(process_list[0].MainModule.FileName), "Songs");
                            LoadMemorySearch(process_list[0]);
                        }
                    }

                    if (m_memory_finder != null)
                    {
                        BeatmapSet beatmapset = m_memory_finder.GetCurrentBeatmapSet();
                        Beatmap beatmap = m_memory_finder.GetCurrentBeatmap();
                        ModsInfo mods = m_memory_finder.GetCurrentMods();
                        double hp = m_memory_finder.GetCurrentHP();
                        double acc = m_memory_finder.GetCurrentAccuracy();
                        int cb = m_memory_finder.GetCurrentCombo();
                        int pt = m_memory_finder.GetPlayingTime();
                        int n300 = m_memory_finder.Get300Count();
                        int n100 = m_memory_finder.Get100Count();
                        int n50 = m_memory_finder.Get50Count();
                        int nmiss = m_memory_finder.GetMissCount();
                        
                        beatmapset.Artist = m_now_player_status.artist;
                        beatmapset.Title = m_now_player_status.title;
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
                            if (m_last_osu_status == OsuStatus.Listening)
                            {
                                IO.CurrentIO.Write($"[MemoryReader]m_now_player_status.diff = {m_now_player_status.diff}");
                                if (!string.IsNullOrWhiteSpace(m_now_player_status.diff))
                                    beatmap.Diff = m_now_player_status.diff;
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
                            mods = new ModsInfo();
                            cb = 0;
                        }

                        if (status != m_last_osu_status)
                            OnStatusChanged?.Invoke(m_last_osu_status, status);

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
                else
                {
                    if (m_last_osu_status == OsuStatus.NoFoundProcess)
                    {
                        m_memory_finder = null;
                        if (count % (Setting.NoFoundOSUHintInterval * Setting.ListenInterval) == 0)
                        {
                            Sync.Tools.IO.CurrentIO.WriteColor(LANG_OSU_NOT_FOUND, ConsoleColor.Red);
                            count = 0;
                        }
                        count++;
                    }
                }

                Thread.Sleep(Setting.ListenInterval);
            }
        }

        private OsuStatus GetCurrentOsuStatus()
        {
            var result = Process.GetProcessesByName("osu!");
            if (result.Length == 0) return OsuStatus.NoFoundProcess;
            string osu_title = result.Length != 0 ? result[0].MainWindowTitle : "unknown title";

            if (m_now_player_status.status == null) return OsuStatus.Unkonw;

            if (m_now_player_status.status == "Editing" || (osu_title != "osu!" && osu_title.Contains(".osu"))) return OsuStatus.Editing;

            if (m_now_player_status.status == "Playing" || (osu_title != "osu!" && osu_title != "")) return OsuStatus.Playing;

            if (m_now_player_status.status == "Watching") return OsuStatus.Playing;
            return OsuStatus.Listening;
        }
    }
}