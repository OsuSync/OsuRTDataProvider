using Sync.Tools;
using System;

namespace MemoryReader
{
    internal class SettingIni : IConfigurable
    {
        public ConfigurationElement ListenInterval { set; get; }
        public ConfigurationElement EnableDirectoryImprecisionSearch { get; set; }

        public void onConfigurationLoad()
        {
            try
            {
                Setting.ListenInterval = int.Parse(ListenInterval);
                Setting.EnableDirectoryImprecisionSearch = bool.Parse(EnableDirectoryImprecisionSearch);
            }
            catch(Exception e)
            {
                onConfigurationSave();
            }
        }

        public void onConfigurationSave()
        {
            ListenInterval = Setting.ListenInterval.ToString();
            EnableDirectoryImprecisionSearch = Setting.EnableDirectoryImprecisionSearch.ToString();
        }
    }

    internal static class Setting
    {
        public static int ListenInterval = 33;//ms
        public static bool EnableDirectoryImprecisionSearch = true;

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