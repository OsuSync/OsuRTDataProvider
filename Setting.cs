using Sync.Tools.ConfigGUI;
using Sync.Tools;
using System;

namespace OsuRTDataProvider
{
    public class SettingIni : IConfigurable
    {
        [ConfigInteger(MinValue = 1,MaxValue = 10000,NeedRestart = true)]
        public ConfigurationElement ListenInterval { set; get; }

        [ConfigBool(NeedRestart = true)]
        public ConfigurationElement EnableTourneyMode { get; set; }

        [ConfigInteger(MinValue = 1,MaxValue = 8,NeedRestart = true)]
        public ConfigurationElement TeamSize { get; set; }

        [ConfigBool(NeedRestart = true)]
        public ConfigurationElement DebugMode { get; set; }

        [ConfigPath(IsFilePath =false,NeedRestart = true)]
        public ConfigurationElement ForceOsuSongsDirectory { get; set; }
        //Auto,Osu,Taiko,Mania,CTB
        public ConfigurationElement GameMode
        {
            get => Setting.GameMode;
            set => Setting.GameMode = value;
        }

        public void onConfigurationLoad()
        {
            try
            {
                Setting.DebugMode = bool.Parse(DebugMode);
                Setting.ListenInterval = int.Parse(ListenInterval);
                Setting.EnableTourneyMode = bool.Parse(EnableTourneyMode);
                Setting.TeamSize = int.Parse(TeamSize);
                Setting.ForceOsuSongsDirectory = ForceOsuSongsDirectory;
                if (Setting.TeamSize > 8 || Setting.TeamSize < 1)
                {
                    Setting.TeamSize = 1;
                    IO.CurrentIO.Write("TeameSize∈[1,8]");
                }
            }
            catch (Exception e)
            {
                onConfigurationSave();
            }
        }

        public void onConfigurationReload()
        {
            onConfigurationLoad();
        }

        public void onConfigurationSave()
        {
            DebugMode = Setting.DebugMode.ToString();
            ListenInterval = Setting.ListenInterval.ToString();
            EnableTourneyMode = Setting.EnableTourneyMode.ToString();
            TeamSize = Setting.TeamSize.ToString();
            ForceOsuSongsDirectory = Setting.ForceOsuSongsDirectory;
        }
    }

    internal static class Setting
    {
        public static bool DebugMode = false;
        public static int ListenInterval = 100;//ms
        public static bool EnableTourneyMode = false;
        public static int TeamSize = 1;
        public static string ForceOsuSongsDirectory = "";
        public static string GameMode = "Auto";

        public static string SongsPath = string.Empty;//不保存
        public static string OsuVersion = string.Empty;
    }
}