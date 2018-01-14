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
                var query_folders_result = Set.AllLocationPath;
                if (query_folders_result == null) return string.Empty;

                if (_path != null) return _path;

                //搜索BeatmapSet文件夹找到Osu文件找到对应Diff的文件
                FileInfo[] dir_info;
                foreach (var path in query_folders_result)
                {
                    dir_info = new DirectoryInfo(path).GetFiles($"*[{BeatmapSet.ObscureString(Diff)}].osu");
                    if (Setting.DebugMode)
                    {
                        Sync.Tools.IO.CurrentIO.WriteColor($"[OsuRTDataProvider][{Set.OsuClientID}]Found {dir_info.Length} beatmap(s):", ConsoleColor.Blue);
                        for (int i = 0; i < dir_info.Length; i++)
                        {
                            Sync.Tools.IO.CurrentIO.WriteColor($"\t({i}){dir_info[i].FullName}", ConsoleColor.Blue);
                        }
                    }

                    if (dir_info.Length > 0)
                    {
                        _path = dir_info[0].FullName;
                        return _path;
                    }

                    //通过各个osu文件内容进行和diff对比
                    if (Setting.EnableOsuFileContentCompareSearch)
                    {
                        var result = GetOsuFilesFromContentSearch(path, Diff);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            _path = result;
                            Sync.Tools.IO.CurrentIO.WriteColor($"[OsuRTDataProvider][{Set.OsuClientID}]Found beatmap by content-compare search:{_path}", ConsoleColor.Blue);
                            return _path;
                        }
                    }

                }
                return string.Empty;
            }
        }

        public static string GetOsuFilesFromContentSearch(string folder_path, string diff)
        {
            var files = Directory.EnumerateFiles(folder_path, "*.osu");

            foreach (var file in files)
            {
                if (diff == SearchDiff(file))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        private static string SearchDiff(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                while ((!reader.EndOfStream))
                {
                    string line = reader.ReadLine();

                    if (line == "[HitObjects]")
                        break;

                    if (line.StartsWith("Version:"))
                    {
                        var result = line.Substring(8).Trim();
                        return result;
                    }
                }
            }

            return null;
        }

        public static Beatmap Empty = new Beatmap(-1);

        public Beatmap(int id)
        {
            BeatmapID = id;
        }
    }
}