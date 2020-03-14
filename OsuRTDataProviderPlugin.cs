using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Helper;
using OsuRTDataProvider.Listen;
using OsuRTDataProvider.Mods;
using Sync;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace OsuRTDataProvider
{
    [SyncPluginID("7216787b-507b-4eef-96fb-e993722acf2e", VERSION)]
    public class OsuRTDataProviderPlugin : Plugin
    {
        public const string PLUGIN_NAME = "OsuRTDataProvider";
        public const string PLUGIN_AUTHOR = "KedamaOvO";
        public const string VERSION = "1.5.15";

        private PluginConfigurationManager m_config_manager;

        private OsuListenerManager[] m_listener_managers = new OsuListenerManager[16];
        private int m_listener_managers_count = 0;

        public bool ModsChangedAtListening => Setting.EnableModsChangedAtListening;

        /// <summary>
        /// If EnableTourneyMode = false in config.ini, return 0.
        /// If EnableTourneyMode = true in config.ini, return TeamSize * 2.
        /// </summary>
        public int TourneyListenerManagersCount { get => Setting.EnableTourneyMode ? m_listener_managers_count : 0; }

        /// <summary>
        /// return a ListenerManager.
        /// </summary>
        public OsuListenerManager ListenerManager { get => m_listener_managers[0]; }

        /// <summary>
        /// If EnableTourneyMode = false in config.ini, return null.
        /// If EnableTourneyMode = true in config.ini, return all ListenerManagers.
        /// </summary>
        public OsuListenerManager[] TourneyListenerManagers { get => Setting.EnableTourneyMode ? m_listener_managers : null; }

        public OsuRTDataProviderPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());
            m_config_manager = new PluginConfigurationManager(this);
            m_config_manager.AddItem(new SettingIni());

            if (Setting.DebugMode)
            {
                base.EventBus.BindEvent<PluginEvents.ProgramReadyEvent>((e) =>
                    HardwareInformationHelper.PrintHardwareInformation());
            }

            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>(InitCommand);
            base.EventBus.BindEvent<PluginEvents.ProgramReadyEvent>((e) => {
                Logger.Info(string.Format(DefaultLanguage.LANG_TOURNEY_HINT, Setting.EnableTourneyMode));
                Task.Run(()=>UpdateChecker.CheckUpdate());
            });

            try
            {
                File.Delete("OsuRTDataProviderRelease.dll");
            }
            catch (Exception) { }
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);

            if (Setting.EnableTourneyMode)
            {
                m_listener_managers_count = Setting.TeamSize * 2;
                for (int i = 0; i < m_listener_managers_count; i++)
                    InitTourneyManager(i);
            }
            else
            {
                InitManager();
            }

            DebugOutput(Setting.DebugMode, true);
        }

        private void InitTourneyManager(int id)
        {
            m_listener_managers[id] = new OsuListenerManager(true, id);
            m_listener_managers[id].Start();
        }

        private void InitManager()
        {
            m_listener_managers[0] = new OsuListenerManager();
            m_listener_managers[0].Start();
        }

        private void InitCommand(PluginEvents.InitCommandEvent @e)
        {
            @e.Commands.Dispatch.bind("ortdp", (args) =>
             {
                 if(args.Count >= 1)
                 {
                     if(args[0] == "releases")
                     {
                         System.Diagnostics.Process.Start("https://github.com/OsuSync/OsuRTDataProvider/releases");
                     }
                 }

                 if (args.Count >= 2)
                 {
                     if (args[0] == "debug")
                     {
                         if (bool.TryParse(args[1], out bool f))
                         {
                             DebugOutput(f);
                             Logger.Info($"Debug mode = {Setting.DebugMode}");
                         }
                     }
                     return true;
                 }
                 return false;
             }, "OsuRTDataProvider control panel");
        }

        private void DebugOutput(bool enable, bool first = false)
        {
            if (!first && Setting.DebugMode == enable) return;

            if (Setting.EnableTourneyMode)
            {
                for (int i = 0; i < TourneyListenerManagersCount; i++)
                {
                    int id = i;
                    void OnTourneyStatusChanged(OsuStatus l, OsuStatus c)=>
                        Logger.Info($"[{id}]Current Game Status:{c}");
                    void OnTourneyModsChanged(ModsInfo m)=>
                        Logger.Info($"[{id}]Mods:{m}(0x{(uint)m.Mod:X8})");
                    void OnTourneyModeChanged(OsuPlayMode last, OsuPlayMode mode)=>
                        Logger.Info($"[{id}]Mode:{mode}");
                    void OnTourneyBeatmapChanged(Beatmap map) =>
                        Logger.Info($"[{id}]Beatmap: {map.Artist} - {map.Title}[{map.Difficulty}]({map.BeatmapSetID},{map.BeatmapID},{map.FilenameFull})");
                    void OnTourneyPlayerChanged(string playername) =>
                        Logger.Info($"[{id}]Current Player: {playername}");
                    void OnTourneyHitEventsChanged(PlayType playType, List<HitEvent> hitEvents)
                    {
                        string log = $"[{id}]Play Type: {playType}, end time: {(hitEvents.Count == 0 ? -1 : hitEvents[hitEvents.Count - 1].timeStamp)}, count: {hitEvents.Count}";
                        Logger.Info(log);
                    };

                    if (enable)
                    {
                        m_listener_managers[i].OnStatusChanged += OnTourneyStatusChanged;
                        m_listener_managers[i].OnModsChanged += OnTourneyModsChanged;
                        m_listener_managers[i].OnPlayModeChanged += OnTourneyModeChanged;
                        m_listener_managers[i].OnBeatmapChanged += OnTourneyBeatmapChanged;
                        m_listener_managers[i].OnPlayerChanged += OnTourneyPlayerChanged;
                        m_listener_managers[i].OnHitEventsChanged += OnTourneyHitEventsChanged;
                    }
                    else
                    {
                        m_listener_managers[i].OnStatusChanged -= OnTourneyStatusChanged;
                        m_listener_managers[i].OnModsChanged -= OnTourneyModsChanged;
                        m_listener_managers[i].OnPlayModeChanged -= OnTourneyModeChanged;
                        m_listener_managers[i].OnHitEventsChanged -= OnTourneyHitEventsChanged;
                    }
                }
            }
            else
            {
                void OnStatusChanged(OsuStatus l, OsuStatus c) =>
                    Logger.Info($"Current Game Status:{c}");
                void OnModsChanged(ModsInfo m) =>
                    Logger.Info($"Mods:{m}(0x{(uint)m.Mod:X8})");
                void OnModeChanged(OsuPlayMode last, OsuPlayMode mode) =>
                    Logger.Info($"Mode:{mode}");
                void OnBeatmapChanged(Beatmap map) =>
                    Logger.Info($"Beatmap: {map.Artist} - {map.Title}[{map.Difficulty}]({map.BeatmapSetID},{map.BeatmapID},{map.FilenameFull})");
                void OnPlayerChanged(string playername) =>
                    Logger.Info($"Current Player: {playername}");
                void OnHitEventsChanged(PlayType playType, List<HitEvent> hitEvents)
                {
                    string log = $"Play Type: {playType}, end time: {(hitEvents.Count == 0 ? -1 : hitEvents[hitEvents.Count - 1].timeStamp)}, count: {hitEvents.Count}";
                    Logger.Info(log);
                };

                if (enable)
                {
                    m_listener_managers[0].OnStatusChanged += OnStatusChanged;
                    m_listener_managers[0].OnModsChanged += OnModsChanged;
                    m_listener_managers[0].OnPlayModeChanged += OnModeChanged;
                    m_listener_managers[0].OnBeatmapChanged += OnBeatmapChanged;
                    m_listener_managers[0].OnPlayerChanged += OnPlayerChanged;
                    m_listener_managers[0].OnHitEventsChanged += OnHitEventsChanged;
                }
                else
                {
                    m_listener_managers[0].OnStatusChanged -= OnStatusChanged;
                    m_listener_managers[0].OnModsChanged -= OnModsChanged;
                    m_listener_managers[0].OnPlayModeChanged -= OnModeChanged;
                    m_listener_managers[0].OnBeatmapChanged -= OnBeatmapChanged;
                    m_listener_managers[0].OnPlayerChanged -= OnPlayerChanged;
                    m_listener_managers[0].OnHitEventsChanged -= OnHitEventsChanged;
                }
            }

            Setting.DebugMode = enable;
        }

        public override void OnDisable()
        {
            int size = Setting.EnableTourneyMode ? TourneyListenerManagersCount : 1;
            for (int i = 0; i < size; i++)
            {
                m_listener_managers[i].Stop();
            }
        }

        public override void OnExit()
        {
            OnDisable();
        }
    }
}