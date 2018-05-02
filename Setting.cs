using Sync.Tools.ConfigGUI;
using Sync.Tools;
using System;

namespace OsuRTDataProvider
{
    public class SettingIni : IConfigurable
    {
        [Integer(MinValue = 1,MaxValue = 10000)]
        public ConfigurationElement ListenInterval
        {
            set => Setting.ListenInterval = int.Parse(value);
            get => Setting.ListenInterval.ToString();
        }

        [Bool(RequireRestart = true)]
        public ConfigurationElement EnableTourneyMode { get; set; }

        [Integer(MinValue = 1,MaxValue = 8,RequireRestart = true)]
        public ConfigurationElement TeamSize { get; set; }

        [Bool(RequireRestart = true)]
        public ConfigurationElement DebugMode { get; set; }

        [Path(IsDirectory = true,RequireRestart = true)]
        public ConfigurationElement ForceOsuSongsDirectory { get; set; }
        //Auto,Osu,Taiko,Mania,CTB
        [List(ValueList = new string[] { "Auto", "Osu", "Taiko", "CatchTheBeat", "Mania" })]
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