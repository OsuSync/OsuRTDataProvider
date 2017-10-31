using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MemoryReader.DefaultLanguage;

namespace MemoryReader.BeatmapInfo
{
    public class BeatmapSet
    {
        public int BeatmapSetID { get; private set; }
        public string DownloadLink
        {
            get
            {
                if (BeatmapSetID != 0) return @"http://osu.ppy.sh/s/" + BeatmapSetID;
                return LANG_NOT_FOUND;
            }
        }

        public string Artist { get; set; }
        public string Title { get; set; }

        private string Encode(string str)
        {
            return str.Replace("*", "-").Replace(".","");
        }

        private bool SongPathExists(string songs)
        {
            if (songs.Contains("\"")|| songs.Contains("<")|| songs.Contains(">")) return false;
            string path = Path.Combine(Setting.SongsPath, songs);
            return Directory.Exists(path);
        }

        private static string[] s_replace_list=new string[] { "*",".",":","?","\"","<",">","/"};

        private string ObscurePath(string path)
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

        private string _path;

        public string LocationPath
        {
            get
            {
                if (Artist == null || Artist == string.Empty) return string.Empty;
                if (Title == null || Title == string.Empty) return string.Empty;

                if (_path != null) return _path;

                var dir_info = new System.IO.DirectoryInfo(Setting.SongsPath);
                DirectoryInfo[] dir_list;

                if (BeatmapSetID == -1)
                {
                    if (Setting.EnableDirectoryImprecisionSearch)
                    {
                        dir_list = dir_info.GetDirectories(ObscurePath($"{Artist} - {Title}"));
                        if(dir_list.Length==0)
                        {
                            dir_list = dir_info.GetDirectories(ObscurePath($" - {Title}"));//inso mirror bug
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    dir_list = dir_info.GetDirectories(ObscurePath($"{BeatmapSetID} {Artist} - {Title}"));
                    if (dir_list.Length == 0)
                    {
                        dir_list = dir_info.GetDirectories(ObscurePath($"{BeatmapSetID}  - {Title}"));//inso mirror bug
                    }
                }
                
#if DEBUG
                Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]找到的{dir_list.Length}个文件夹路径分别为:");
                int _i = 0;
                foreach (var dir in dir_list)
                {
                    Sync.Tools.IO.CurrentIO.Write($"[MemoryReader][{_i++}]{dir.FullName}");
                }
#endif

                if(dir_list.Length!=0)
                {
                    _path = dir_list[0].FullName;
                    return _path;
                }

                return string.Empty;
            }
        }

        public static BeatmapSet Empty = new BeatmapSet(-1);

        public BeatmapSet(int id)
        {
            BeatmapSetID = id;
        }
    }
}
