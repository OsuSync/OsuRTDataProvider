using Sync.Tools;

namespace OsuListenEssential
{
    public class DefaultLanguage : I18nProvider
    {
        public static LanguageElement LANG_OSU_NOT_FOUND = "[OsuListenEssential:{0}]Not found osu!.exe process";
        public static LanguageElement LANG_OSU_FOUND = "[OsuListenEssential:{0}]Found osu!.exe process";

        public static LanguageElement LANG_INIT_PLAY_FINDER_FAILED = "[OsuListenEssential:{0}]Init Play Finder Failed! Retry after {1} seconds";

        public static LanguageElement LANG_BEATMAP_NOT_FOUND = "Beatmap not found";
    }
}