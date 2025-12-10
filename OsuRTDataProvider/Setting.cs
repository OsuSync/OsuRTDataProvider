using System;

namespace OsuRTDataProvider
{
    public static class Setting
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
