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
        private OSUListenerManager[] m_listener_managers=new OSUListenerManager[8];
        public OSUListenerManager ListenerManager { get => m_listener_managers[0]; }

        public OSUListenerManager[] TourneyListenerManagers { get => Setting.EnableTourneyMode? m_listener_managers : null; }

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

            if(Setting.EnableTourneyMode)
            {
                for(int i=0;i<Setting.TeamSize*2;i++)
                {
                    m_listener_managers[i] = new OSUListenerManager(true, i);

#if DEBUG
                    m_listener_managers[i].OnStatusChanged +=(l,c) => Sync.Tools.IO.CurrentIO.Write($"[{i}]当前状态:" + c);
                    m_listener_managers[i].OnCurrentMods += m => Sync.Tools.IO.CurrentIO.Write($"[{i}]Mods:" + m);
#endif

                    try
                    {
                        m_listener_managers[i].Init(ev.Host);
                    }
                    catch (Exception e)
                    {
                        Sync.Tools.IO.CurrentIO.WriteColor(e.Message, ConsoleColor.Red);
                        Sync.Tools.IO.CurrentIO.WriteColor(e.StackTrace, ConsoleColor.Red);
                    }

                    m_listener_managers[i].Start();
                }
            }
            else
            {
                m_listener_managers[0] = new OSUListenerManager();

                try
                {
                    m_listener_managers[0].Init(ev.Host);
#if DEBUG
                    m_listener_managers[0].OnStatusChanged += (l, c) => Sync.Tools.IO.CurrentIO.Write("当前状态:" + c);
                    m_listener_managers[0].OnCurrentMods += m => Sync.Tools.IO.CurrentIO.Write("Mods:" + m);
#endif
                }
                catch (Exception e)
                {
                    Sync.Tools.IO.CurrentIO.WriteColor(e.Message, ConsoleColor.Red);
                    Sync.Tools.IO.CurrentIO.WriteColor(e.StackTrace, ConsoleColor.Red);
                }

                m_listener_managers[0].Start();
            }
        }
    }
}