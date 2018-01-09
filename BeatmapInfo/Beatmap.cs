using System;
using System.IO;
using static OsuRTDataProvider.DefaultLanguage;

namespace OsuRTDataProvider.BeatmapInfo
{
    public class Beatmap
    {
        public int BeatmapID { get; private set; }

        public string DownloadLink
        {
            get
            {
                if (BeatmapID != 0) return @"http://osu.ppy.sh/b/" + BeatmapID;
                return LANG_BEATMAP_NOT_FOUND;
            }
        }

        /// <summary>
        /// Return the beatmap's set;
        /// </summary>
        public BeatmapSet Set { get; set; }

        /// <summary>
        /// Return the beatmap's difficulty.
        /// </summary>
        public string Diff { get; set; }

        private string _path;

        /// <summary>
        /// Return the possible beatmap paths.
        /// If not found.return string.Empty.
        /// </summary>
        public string LocationFile
        {
            get
            {
                if (Set == null) return string.Empty;
                if (Diff == null || Diff == string.Empty) return string.Empty;
                if (Set.AllLocationPath == null) return string.Empty;

                if (_path != null) return _path;

                //搜索BeatmapSet文件夹找到Osu文件找到对应Diff的文件
                FileInfo[] dir_info;
                foreach (var path in Set.AllLocationPath)
                {
                    dir_info = new DirectoryInfo(path).GetFiles($"*[{BeatmapSet.ObscureString(Diff)}].osu");
                    if (Setting.DebugMode)
                    {
                        Sync.Tools.IO.CurrentIO.WriteColor($"[OsuRTDataProvider][{Set.OsuClientID}]Found {dir_info.Length} beatmap(s):",ConsoleColor.Blue);
                        for (int i=0;i<dir_info.Length;i++)
                        {
                            Sync.Tools.IO.CurrentIO.WriteColor($"\t({i}){dir_info[i].FullName}", ConsoleColor.Blue);
                        }
                    }

                        if (dir_info.Length > 0)
                    {
                        _path = dir_info[0].FullName;
                        return _path;
                    }
                }
                return string.Empty;
            }
        }

        public static Beatmap Empty = new Beatmap(-1);

        public Beatmap(int id)
        {
            BeatmapID = id;
        }
    }
}