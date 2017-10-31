using Sync.Tools;
using System;

namespace MemoryReader
{
    internal class SettingIni : IConfigurable
    {
        public ConfigurationElement ListenInterval { set; get; }
        public ConfigurationElement EnableDirectoryImprecisionSearch { get; set; }
        public ConfigurationElement EnableTourneyMode { get; set; }
        public ConfigurationElement TeamSize { get; set; }

        public void onConfigurationLoad()
        {
            try
            {
                Setting.ListenInterval = int.Parse(ListenInterval);
                Setting.EnableDirectoryImprecisionSearch = bool.Parse(EnableDirectoryImprecisionSearch);
                Setting.EnableTourneyMode = bool.Parse(EnableTourneyMode);
                Setting.TeamSize = int.Parse(TeamSize);
                if(Setting.TeamSize>8 || Setting.TeamSize<1)
                {
                    Setting.TeamSize = 1;
                    Sync.Tools.IO.CurrentIO.Write("TeameSize∈[1,8]");
                }
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
            EnableTourneyMode = Setting.EnableTourneyMode.ToString();
            TeamSize = Setting.TeamSize.ToString();
        }
    }

    internal static class Setting
    {
        public static int ListenInterval = 33;//ms
        public static bool EnableDirectoryImprecisionSearch = true;
        public static bool EnableTourneyMode = false;
        public static int TeamSize = 1;

        public static string SongsPath = string.Empty;//不保存

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