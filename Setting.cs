using OsuRTDataProvider.Handler;
using Sync.Tools;
using System;
using System.IO;

namespace OsuRTDataProvider
{
    public class SettingIni : IConfigurable
    {
        public ConfigurationElement ListenInterval { set; get; }
        public ConfigurationElement EnableDirectoryImprecisionSearch { get; set; }
        public ConfigurationElement EnableTourneyMode { get; set; }
        public ConfigurationElement TeamSize { get; set; }
        public ConfigurationElement DebugMode { get; set; }
        public ConfigurationElement EnableOsuFileContentCompareSearch { get; set; }

        public void onConfigurationLoad()
        {
            try
            {
                Setting.DebugMode = bool.Parse(DebugMode);
                Setting.ListenInterval = int.Parse(ListenInterval);
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
            DebugMode = Setting.DebugMode.ToString();
            ListenInterval = Setting.ListenInterval.ToString();
            EnableTourneyMode = Setting.EnableTourneyMode.ToString();
            TeamSize = Setting.TeamSize.ToString();
        }
    }

    internal static class Setting
    {
        public static bool DebugMode = false;
        public static int ListenInterval = 100;//ms
        public static bool EnableTourneyMode = false;
        public static int TeamSize = 1;
 
        public static string SongsPath = string.Empty;//不保存

        private static SettingIni setting_output = new SettingIni();
        private static PluginConfiuration plugin_config = null;

        public static OsuRTDataProviderPlugin PluginInstance
        {
            set
            {
                plugin_config = new PluginConfiuration(value, setting_output);
            }
        }

        static Setting()
        {
            ExitHandler.OnConsloeExit += ()=>plugin_config?.ForceSave();
        }
    }
}