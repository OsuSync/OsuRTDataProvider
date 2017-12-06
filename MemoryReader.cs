using MemoryReader.Listen;
using Sync;
using Sync.Plugins;
using Sync.Tools;
using System;

namespace MemoryReader
{
    public class MemoryReader : Plugin
    {
        private SyncHost m_host;

        public const string PLUGIN_NAME = "MemoryReader";
        public const string PLUGIN_AUTHOR = "KedamaOvO";

        private int m_listener_managers_count = 0;
        public int TourneyListenerManagersCount { get => Setting.EnableTourneyMode ? m_listener_managers_count : 0; }

        private OSUListenerManager[] m_listener_managers = new OSUListenerManager[16];
        public OSUListenerManager ListenerManager { get => m_listener_managers[0]; }

        public OSUListenerManager[] TourneyListenerManagers { get => Setting.EnableTourneyMode ? m_listener_managers : null; }

        public MemoryReader() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());
            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnLoadComplete);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
        }

        private void OnLoadComplete(PluginEvents.LoadCompleteEvent ev)
        {
            Setting.PluginInstance = this;
            m_host = ev.Host;

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

        private void InitTourneyManager(int id)
        {
            m_listener_managers[id] = new OSUListenerManager(true, id);

#if DEBUG
            m_listener_managers[id].OnStatusChanged += (l, c) => Sync.Tools.IO.CurrentIO.Write($"[{id}]当前状态:{c}");
            m_listener_managers[id].OnCurrentMods += m => Sync.Tools.IO.CurrentIO.Write($"[{id}]Mods:{m}(0x{(uint)m.Mod:X8})");
#endif

            m_listener_managers[id].Start();
        }

        private void InitManager()
        {
            m_listener_managers[0] = new OSUListenerManager();

#if DEBUG
            m_listener_managers[0].OnStatusChanged += (l, c) => Sync.Tools.IO.CurrentIO.Write($"当前状态:{c}");
            m_listener_managers[0].OnCurrentMods += m => Sync.Tools.IO.CurrentIO.Write($"Mods:{m}(0x{(uint)m.Mod:X8})");
#endif

            m_listener_managers[0].Start();
        }
    }
}