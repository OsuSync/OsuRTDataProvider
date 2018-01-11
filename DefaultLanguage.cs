using Sync.Tools;

namespace OsuRTDataProvider
{
    public class DefaultLanguage : I18nProvider
    {
        public static LanguageElement LANG_OSU_NOT_FOUND = "[OsuRTDataProvider][ID:{0}]Not found osu!.exe process";
        public static LanguageElement LANG_OSU_FOUND = "[OsuRTDataProvider][ID:{0}]Found osu!.exe process";

        public static LanguageElement LANG_INIT_STATUS_FINDER_FAILED = "[OsuRTDataProvider][ID:{0}]Init StatusFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_STATUS_FINDER_SUCCESS = "[OsuRTDataProvider][ID:{0}]Init StatusFinder Success!";
        public static LanguageElement LANG_INIT_PLAY_FINDER_FAILED = "[OsuRTDataProvider][ID:{0}]Init PlayFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_PLAY_FINDER_SUCCESS = "[OsuRTDataProvider][ID:{0}]Init PlayFinder Success!";

        public static LanguageElement LANG_BEATMAP_NOT_FOUND = "Beatmap not found";
    }
}