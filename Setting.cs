using Sync.Tools;
using System;

namespace MemoryReader
{
    internal class SettingIni : IConfigurable
    {
        public ConfigurationElement ListenInterval { set; get; }
        public ConfigurationElement NoFoundOsuHintInterval { set; get; }

        public void onConfigurationLoad()
        {
            try
            {
                Setting.ListenInterval = int.Parse(ListenInterval);
            }
            catch(Exception e)
            {
                onConfigurationSave();
            }
        }

        public void onConfigurationSave()
        {
            ListenInterval = Setting.ListenInterval.ToString();
        }
    }

    internal static class Setting
    {
        public static int ListenInterval = 33;//ms

        public static string SongsPath = "";//不保存

        private static SettingIni setting_output = new SettingIni();
        private static PluginConfiuration plugin_config = null;

        public static MemoryReader PluginInstance
        {
            set
            {
                plugin_config = new PluginConfiuration(value, setting_output);
            }
        }
    }
}