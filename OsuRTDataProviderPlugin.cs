using OsuRTDataProvider.Listen;
using Sync;
using Sync.Plugins;
using Sync.Tools;
using System;

namespace OsuRTDataProvider
{
    public class OsuRTDataProviderPlugin : Plugin
    {
        public const string PLUGIN_NAME = "OsuRTDataProvider";
        public const string PLUGIN_AUTHOR = "KedamaOvO";

        private bool m_is_init_plugin = false;

        private OsuListenerManager[] m_listener_managers = new OsuListenerManager[16];
        private int m_listener_managers_count = 0;

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
            base.EventBus.BindEvent<PluginEvents.InitPluginEvent>(OnInitPlugin);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
        }

        private void OnInitPlugin(PluginEvents.InitPluginEvent e)
        {
            if (!m_is_init_plugin)
            {
                Setting.PluginInstance = this;

                m_is_init_plugin = true;
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
            }
        }

        private void InitTourneyManager(int id)
        {
            m_listener_managers[id] = new OsuListenerManager(true, id);

#if DEBUG
            m_listener_managers[id].OnStatusChanged += (l, c) => Sync.Tools.IO.CurrentIO.Write($"[{id}]Current Game Status:{c}");
            m_listener_managers[id].OnModsChanged += m => Sync.Tools.IO.CurrentIO.Write($"[{id}]Mods:{m}(0x{(uint)m.Mod:X8})");
#endif

            m_listener_managers[id].Start();
        }

        private void InitManager()
        {
            m_listener_managers[0] = new OsuListenerManager();

#if DEBUG
            m_listener_managers[0].OnStatusChanged += (l, c) => Sync.Tools.IO.CurrentIO.Write($"Current Game Status:{c}");
            m_listener_managers[0].OnModsChanged += m => Sync.Tools.IO.CurrentIO.Write($"Mods:{m}(0x{(uint)m.Mod:X8})");
#endif

            m_listener_managers[0].Start();
        }
    }
}