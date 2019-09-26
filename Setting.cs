using Sync.Tools.ConfigurationAttribute;
using Sync.Tools;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
        public ConfigurationElement EnableTourneyMode
        {
            get => Setting.EnableTourneyMode.ToString();
            set => Setting.EnableTourneyMode = bool.Parse(value);
        }

        [Integer(MinValue = 1,MaxValue = 8,RequireRestart = true)]
        public ConfigurationElement TeamSize {
            get => Setting.TeamSize.ToString();
            set
            {
                Setting.TeamSize = int.Parse(value);
                if (Setting.TeamSize < 1 || Setting.TeamSize > 8)
                {
                    Setting.TeamSize = 0;
                    Logger.Info("TeamSize must be between 1 and 8.");
                }
            }
        }

        [Bool(RequireRestart = true)]
        public ConfigurationElement DebugMode
        {
            get => Setting.DebugMode.ToString();
            set => Setting.DebugMode = bool.Parse(value);
        }

        [Path(IsDirectory = true, RequireRestart = true)]
        public ConfigurationElement ForceOsuSongsDirectory
        {
            get => Setting.ForceOsuSongsDirectory;
            set => Setting.ForceOsuSongsDirectory = value;
        }
        //Auto,Osu,Taiko,Mania,CTB
        [List(ValueList = new string[] { "Auto", "Osu", "Taiko", "CatchTheBeat", "Mania" })]
        public ConfigurationElement GameMode
        {
            get => Setting.GameMode;
            set => Setting.GameMode = value;
        }

        [Bool]
        public ConfigurationElement DisableProcessNotFoundInformation
        {
            set => Setting.DisableProcessNotFoundInformation = bool.Parse(value);
            get => Setting.DisableProcessNotFoundInformation.ToString();
        }

        [Bool]
        public ConfigurationElement EnableModsChangedAtListening
        {
            set => Setting.EnableModsChangedAtListening = bool.Parse(value);
            get => Setting.EnableModsChangedAtListening.ToString();
        }

        public void onConfigurationLoad()
        {
        }

        public void onConfigurationReload()
        {
        }

        public void onConfigurationSave()
        {
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
        public static bool DisableProcessNotFoundInformation = false;
        public static bool EnableModsChangedAtListening = false;

        #region NoSave
        public static string SongsPath = string.Empty;
        public static string OsuVersion = string.Empty;
        public static string Username = string.Empty;
        #endregion

        public static double CurrentOsuVersionValue => Utils.ConvertVersionStringToValue(OsuVersion);
    }
}