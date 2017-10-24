using MemoryReader.Listen;
using MemoryReader.Listen.Interface;
using Sync;
using Sync.Plugins;
using Sync.Tools;
using System;

namespace MemoryReader
{
    public class MemoryReader : Plugin
    {
        public const string PLUGIN_NAME = "MemoryReader";
        public const string PLUGIN_AUTHOR = "KedamaOvO";
        private OSUListenerManager m_osu_listener=new OSUListenerManager();
        public OSUListenerManager ListenerManager { get => m_osu_listener; }

        public MemoryReader() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());
            base.EventBus.BindEvent<PluginEvents.InitPluginEvent>(OnInitPlugin);
            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnLoadComplete);
        }

        [Obsolete("Please Use ListenerManager", true)]
        public void RegisterOSUListener(IOSUListener listener)
        {
            m_osu_listener.OnBeatmapChanged += listener.OnCurrentBeatmapChange;
            m_osu_listener.OnBeatmapSetChanged += listener.OnCurrentBeatmapSetChange;
            m_osu_listener.OnAccuracyChanged += listener.OnAccuracyChange;
            m_osu_listener.OnHealthPointChanged += listener.OnHPChange;
            m_osu_listener.OnComboChanged += listener.OnComboChange;
            m_osu_listener.OnCurrentMods += listener.OnCurrentModsChange;
            m_osu_listener.OnStatusChanged += listener.OnStatusChange;
        }

        [Obsolete("Please Use ListenerManager",true)]
        public void UnregisterOSUListener(IOSUListener listener)
        {
            m_osu_listener.OnBeatmapChanged -= listener.OnCurrentBeatmapChange;
            m_osu_listener.OnBeatmapSetChanged -= listener.OnCurrentBeatmapSetChange;
            m_osu_listener.OnAccuracyChanged -= listener.OnAccuracyChange;
            m_osu_listener.OnHealthPointChanged -= listener.OnHPChange;
            m_osu_listener.OnComboChanged -= listener.OnComboChange;
            m_osu_listener.OnCurrentMods -= listener.OnCurrentModsChange;
            m_osu_listener.OnStatusChanged -= listener.OnStatusChange;
        }

        private void OnInitPlugin(PluginEvents.InitPluginEvent e)
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
        }

        private void OnLoadComplete(PluginEvents.LoadCompleteEvent ev)
        {
            Setting.PluginInstance = this;

            try
            {
                m_osu_listener.Init(ev.Host);
            }
            catch (Exception e)
            {
                Sync.Tools.IO.CurrentIO.WriteColor(e.Message, ConsoleColor.Red);
                Sync.Tools.IO.CurrentIO.WriteColor(e.StackTrace, ConsoleColor.Red);
            }
#if DEBUG
            //ListenerManager.OnStatusChanged +=(l,c) => Sync.Tools.IO.CurrentIO.Write("当前状态:" + c);
            //ListenerManager.OnCurrentMods += m => Sync.Tools.IO.CurrentIO.Write("Mods:" + m);
#endif
            m_osu_listener.Start();
        }
    }
}