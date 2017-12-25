using System.Collections.Generic;
using System.IO;
using System.Text;
using static OsuListenEssential.DefaultLanguage;

namespace OsuListenEssential.BeatmapInfo
{
    public class BeatmapSet
    {
        private int m_beatmap_id=-1;
        /// <summary>
        /// Return set id.
        /// If no found return -1;
        /// </summary>
        public int BeatmapSetID {
            get
            {
                if (m_beatmap_id > 0) return m_beatmap_id;
                if (Setting.EnableDirectoryImprecisionSearch)
                {
                    var dir_list=ImprecisionSearchSongs(new DirectoryInfo(Setting.SongsPath));
                    string name = dir_list[0].Name;
                    int len=name.IndexOf(' ');
                    string id = name.Substring(0,len);

                    if (int.TryParse(id, out m_beatmap_id))
                        return m_beatmap_id;
                }
                return -1;
            }
            private set=>m_beatmap_id=value;
        }

        /// <summary>
        /// If BeatmapSetID > 0. Return beatmap's download link.
        /// </summary>
        public string DownloadLink
        {
            get
            {
                if (BeatmapSetID > 0) return $"http://osu.ppy.sh/s/{BeatmapSetID}";
                return LANG_BEATMAP_NOT_FOUND;
            }
        }

        /// <summary>
        /// Return the beatmap's artist.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Return the beatmap's title.
        /// </summary>
        public string Title { get; set; }

        private static string[] s_replace_list = new string[] { "*", ".", ":", "?", "\"", "<", ">", "/","~"};

        public static string ObscureString(string path)
        {
            StringBuilder builder = new StringBuilder(path);

            foreach (var c in s_replace_list)
                builder.Replace(c, "*");
            
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] > 127)
                    builder[i] = '*';
            }
            return builder.ToString();
        }

        private LinkedList<string> _paths;


        private DirectoryInfo[] ImprecisionSearchSongs(DirectoryInfo dir_info)
        {
            DirectoryInfo[] dir_list; 
            dir_list = dir_info.GetDirectories(ObscureString($"*{Artist} - {Title}*"));
            if (dir_list.Length == 0)
            {
                dir_list = dir_info.GetDirectories(ObscureString($"* - {Title}*"));//inso mirror bug
            }
            return dir_list;
        }

        private DirectoryInfo[] SearchSongs()
        {
            var dir_info = new DirectoryInfo(Setting.SongsPath);
            DirectoryInfo[] dir_list;

            dir_list = dir_info.GetDirectories(ObscureString($"{BeatmapSetID} {Artist} - {Title}*"));
            if (dir_list.Length == 0)
            {
                dir_list = dir_info.GetDirectories(ObscureString($"{BeatmapSetID}  - {Title}*"));//inso mirror bug
            }

            if (dir_list.Length == 0 && Setting.EnableDirectoryImprecisionSearch)
            {
                dir_list = ImprecisionSearchSongs(dir_info);
            }

            return dir_list;
        }

        /// <summary>
        /// Return all possible beatmap set paths.
        /// /// If not found.return null;
        /// </summary>
        public LinkedList<string> AllLocationPath
        {
            get
            {
                if (Artist == null || Artist == string.Empty) return null;
                if (Title == null || Title == string.Empty) return null;

                if (_paths != null) return _paths;

                var dir_list=SearchSongs();

#if DEBUG
                Sync.Tools.IO.CurrentIO.Write($"[OsuListenEssential]找到的{dir_list.Length}个文件夹路径分别为:");
                int _i = 0;
                foreach (var dir in dir_list)
                {
                    Sync.Tools.IO.CurrentIO.Write($"[OsuListenEssential][{_i++}]{dir.FullName}");
                }
#endif

                if (dir_list.Length != 0)
                {
                    _paths = new LinkedList<string>();
                    foreach (var d in dir_list)
                    {
                        _paths.AddLast(d.FullName);
                    }
                    return _paths;
                }
                return null;
            }
        }

        /// <summary>
        /// Return the first of all possible beatmap set paths.
        /// If not found.return string.Empty.
        /// </summary>
        public string LocationPath
        {
            get
            {
                var list = AllLocationPath;
                if (list == null) return string.Empty;
                return list.First.Value;
            }
        }

        public static BeatmapSet Empty = new BeatmapSet(-1);

        public BeatmapSet(int id)
        {
            BeatmapSetID = id;
        }
    }
}