using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;

namespace OsuRTDataProvider
{
    public class PluginLanguage : I18nProvider
    {
        public static LanguageElement LANG_OSU_NOT_FOUND = "[ID:{0}]Not found osu!.exe process";
        public static LanguageElement LANG_OSU_FOUND = "[ID:{0}]Found osu!.exe process";
        public static LanguageElement LANG_TOURNEY_HINT = "Tourney Mode: {0}";

        public static LanguageElement CHECK_GOTO_RELEASE_PAGE_HINT = "Enter \"ortdp releases\" to open the releases page in your browser.";
        public static LanguageElement LANG_CHECK_ORTDP_UPDATE = "Found a new version of OsuRTDataProvider({0})";
        public static LanguageElement LANG_INIT_STATUS_FINDER_FAILED = "[ID:{0}]Init StatusFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_STATUS_FINDER_SUCCESS = "[ID:{0}]Init StatusFinder Success!";
        public static LanguageElement LANG_INIT_PLAY_FINDER_FAILED = "[ID:{0}]Init PlayFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_PLAY_FINDER_SUCCESS = "[ID:{0}]Init PlayFinder Success!";
        public static LanguageElement LANG_INIT_BEATMAP_FINDER_FAILED = "[ID:{0}]Init BeatmapFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_BEATMAP_FINDER_SUCCESS = "[ID:{0}]Init BeatmapFinder Success!";
        public static LanguageElement LANG_INIT_MODE_FINDER_FAILED = "[ID:{0}]Init ModeFinder Failed! Retry after {1} seconds";
        public static LanguageElement LANG_INIT_MODE_FINDER_SUCCESS = "[ID:{0}]Init ModeFinder Success!";
        public static LanguageElement LANG_INIT_HIT_EVENT_SUCCESS = "[ID:{0}]Init HitEventFinder Success!";
        public static LanguageElement LANG_INIT_HIT_EVENT_FAIL = "[ID:{0}]Init HitEventFinder Failed! Retry after {1} seconds";

        public static LanguageElement LANG_BEATMAP_NOT_FOUND = "Beatmap not found";

        public static GuiLanguageElement ListenInterval = "Listen interval(ms)";
        public static GuiLanguageElement EnableTourneyMode = "Tourney mode";
        public static GuiLanguageElement TeamSize = "Team size";
        public static GuiLanguageElement DebugMode = "Debug mode";
        public static GuiLanguageElement ForceOsuSongsDirectory = "Force OSU! songs directory";
        public static GuiLanguageElement GameMode = "Game Mode";
        public static GuiLanguageElement DisableProcessNotFoundInformation = "Disable OSU! process not found information";
        public static GuiLanguageElement EnableModsChangedAtListening = "Enable Mods Changed At Listening(Experimental)";

        public void SyncToCore()
        {
            DefaultLanguage.LANG_OSU_NOT_FOUND = LANG_OSU_NOT_FOUND;
            DefaultLanguage.LANG_OSU_FOUND = LANG_OSU_FOUND;
            DefaultLanguage.LANG_TOURNEY_HINT = LANG_TOURNEY_HINT;
            DefaultLanguage.CHECK_GOTO_RELEASE_PAGE_HINT = CHECK_GOTO_RELEASE_PAGE_HINT;
            DefaultLanguage.LANG_CHECK_ORTDP_UPDATE = LANG_CHECK_ORTDP_UPDATE;
            DefaultLanguage.LANG_INIT_STATUS_FINDER_FAILED = LANG_INIT_STATUS_FINDER_FAILED;
            DefaultLanguage.LANG_INIT_STATUS_FINDER_SUCCESS = LANG_INIT_STATUS_FINDER_SUCCESS;
            DefaultLanguage.LANG_INIT_PLAY_FINDER_FAILED = LANG_INIT_PLAY_FINDER_FAILED;
            DefaultLanguage.LANG_INIT_PLAY_FINDER_SUCCESS = LANG_INIT_PLAY_FINDER_SUCCESS;
            DefaultLanguage.LANG_INIT_BEATMAP_FINDER_FAILED = LANG_INIT_BEATMAP_FINDER_FAILED;
            DefaultLanguage.LANG_INIT_BEATMAP_FINDER_SUCCESS = LANG_INIT_BEATMAP_FINDER_SUCCESS;
            DefaultLanguage.LANG_INIT_MODE_FINDER_FAILED = LANG_INIT_MODE_FINDER_FAILED;
            DefaultLanguage.LANG_INIT_MODE_FINDER_SUCCESS = LANG_INIT_MODE_FINDER_SUCCESS;
            DefaultLanguage.LANG_INIT_HIT_EVENT_SUCCESS = LANG_INIT_HIT_EVENT_SUCCESS;
            DefaultLanguage.LANG_INIT_HIT_EVENT_FAIL = LANG_INIT_HIT_EVENT_FAIL;
            DefaultLanguage.LANG_BEATMAP_NOT_FOUND = LANG_BEATMAP_NOT_FOUND;
            DefaultLanguage.ListenInterval = ListenInterval;
            DefaultLanguage.EnableTourneyMode = EnableTourneyMode;
            DefaultLanguage.TeamSize = TeamSize;
            DefaultLanguage.DebugMode = DebugMode;
            DefaultLanguage.ForceOsuSongsDirectory = ForceOsuSongsDirectory;
            DefaultLanguage.GameMode = GameMode;
            DefaultLanguage.DisableProcessNotFoundInformation = DisableProcessNotFoundInformation;
            DefaultLanguage.EnableModsChangedAtListening = EnableModsChangedAtListening;
        }
    }
}
