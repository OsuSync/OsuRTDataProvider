using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OsuRTDataProvider;
using OsuRTDataProvider.BeatmapInfo;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OsuRTDataProvider.Listen.OsuListenerManager;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Player;
using OppaiWNet.Wrap;

namespace OnlinePPOutput
{
    public class OnlinePPOutputPlugin : Plugin,IConfigurable
    {
        public ConfigurationElement OsuApi { get; set; }
        public ConfigurationElement OsuScoreDBFilePath { get; set; }
        public ConfigurationElement OsuID { get; set; }

        Logger<OnlinePPOutputPlugin> logger = new Logger<OnlinePPOutputPlugin>();

        MemoryMappedFile file;
        public const int MMF_CAPACITY = 4096;

        bool is_selecting = false;
        PluginConfigurationManager config;
        private Task current_task;

        public OnlinePPOutputPlugin() : base("OnlinePPOutputPlugin", "MikiraSora")
        {
            file = MemoryMappedFile.CreateOrOpen("online_pp", MMF_CAPACITY, MemoryMappedFileAccess.ReadWrite);
            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnLoaded);

            config = new PluginConfigurationManager(this);
            config.AddItem(this);
        }

        private void OnLoaded(PluginEvents.LoadCompleteEvent @event)
        {
            if (string.IsNullOrWhiteSpace(OsuApi)|| string.IsNullOrWhiteSpace(OsuID))
            {
                logger.LogError("Init failed! please input your osu!api(https://osu.ppy.sh/p/api) and your osu! id in config.ini");
                return;
            }

            var ortdp_plugin = @event.Host.EnumPluings().OfType<OsuRTDataProviderPlugin>().FirstOrDefault();

            if (ortdp_plugin!=null)
            {
                ortdp_plugin.ListenerManager.OnStatusChanged += ListenerManager_OnStatusChanged;
                ortdp_plugin.ListenerManager.OnBeatmapChanged += ListenerManager_OnBeatmapChanged;
            }

            Clear();
        }

        private void ListenerManager_OnBeatmapChanged(Beatmap map)
        {
            if (map==Beatmap.Empty)
            {
                Clear();
                return;
            }

            ChangeBeatmap(map);
        }

        private void ListenerManager_OnStatusChanged(OsuStatus last_status, OsuStatus status)
        {
            is_selecting = status == OsuStatus.SelectSong;
            logger.LogInfomation(is_selecting?"start listen":"stop listen");

            if (!is_selecting)
                Clear();
        }

        private float? GetOnlineScorePP(int id)
        {
            var url = $"https://osu.ppy.sh/api/get_scores?k={OsuApi}&b={id}&u={OsuID}&type=string";

            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

                var response = request.GetResponse();

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var data = reader.ReadToEnd();
                    var arr=JsonConvert.DeserializeObject(data) as JArray;
                    return arr.FirstOrDefault()?["pp"].ToObject<float>();
                }
            }
            catch (Exception e)
            {
                logger.LogError("Try get online pp failed! :" + e.Message);
                return null;
            }

        }

        private float? GetLocalBestScorePP(string file_path)
        {
            try
            {
                var data = File.ReadAllBytes(file_path);
                var replay = GetBestLocalRecord(data);

                if (replay == null)
                    return null;

                Ezpp pp_info = new Ezpp(data);
                pp_info.Count100=replay.Count100;
                pp_info.Count50=replay.Count50;
                pp_info.Mode=(int)replay.GameMode;
                pp_info.Combo=replay.Combo;
                pp_info.Mods=(Mods)(int)replay.Mods;

                pp_info.ApplyChange();

                return pp_info.PP;
            }
            catch (Exception e)
            {
                logger.LogError("Try get online pp failed! :" + e.Message);
                return null;
            }

        }

        CancellationTokenSource cancel_token;

        private void ChangeBeatmap(Beatmap map)
        {
            if (!is_selecting)
                return;

            if (cancel_token != null)
                cancel_token.Cancel();

            cancel_token = new CancellationTokenSource();

            current_task = Task.Run(() =>
            {
                var online_pp = GetOnlineScorePP(map.BeatmapID);
                var local_pp = GetLocalBestScorePP(map.FilenameFull);

                Output($"Online: {(online_pp!=null?online_pp+"pp":"no pp")}\n  Local: {(local_pp != null ? local_pp + "pp" : "no record")}");

            }, cancel_token.Token);
        }

        private void Clear()
        {
            Output(string.Empty);
        }

        private void Output(string content)
        {
            logger.LogInfomation("output:"+content);

            lock (this)
            {
                using (StreamWriter stream = new StreamWriter(file.CreateViewStream()))
                {
                    stream.Write(content);
                    stream.Write('\0');
                }
            }
        }

        public void onConfigurationLoad()
        {

        }

        public void onConfigurationSave()
        {

        }

        public void onConfigurationReload()
        {

        }

        private Replay GetBestLocalRecord(byte[] data)
        {
            var hash = BeatmapHashHelper.GetHashFromOsuFile(data);
            ScoresDb db = ScoresDb.Read(OsuScoreDBFilePath);
            var result = db.Beatmaps.AsParallel().Where((pair) => pair.Key == hash);
            if (result.Count() != 0)
            {
                var list = result.First().Value;
                list.Sort((a, b) => b.Score - a.Score);
                return list.First();
            }
            return null;
        }

        ~OnlinePPOutputPlugin()
        {
            Clear();
            file.Dispose();
        }
    }
}
