using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MemoryReader.DefaultLanguage;

namespace MemoryReader.BeatmapInfo
{
    public class Beatmap
    {
        public int BeatmapID { get; private set; }

        public string DownloadLink {
            get
            {
                if (BeatmapID != 0) return @"http://osu.ppy.sh/b/" + BeatmapID;
                return LANG_NOT_FOUND;
            }
        }

        public BeatmapSet Set { get; set; }
        public string Diff { get; set; }

        private static string[] s_replace_list = new string[] { "*", ".", ":", "?", "\"", "<", ">" };
        private static string[] s_replace_target_list = new string[] { "", "-" };

        private List<string> _GenDiff(string diff, int startindex)
        {
            List<string> ret = new List<string>();
            StringBuilder builder = new StringBuilder(diff);
            foreach (var sc in s_replace_list)
                for (int i = startindex; i < builder.Length; ++i)
                {
                    if (builder[i] == sc[0])
                    {
                        foreach (var tc in s_replace_target_list)
                        {
                            StringBuilder tmp_builder = new StringBuilder(builder.ToString());
                            tmp_builder.Replace(sc, tc, i, 1);
                            ret.AddRange(_GenDiff(tmp_builder.ToString(), i));
                            ret.Add(tmp_builder.ToString());
                        }
                    }
                }

            return ret;
        }

        private string ObscureDiff(string path)
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

        public string LocationFile
        {
            get
            {
                if (Set == null) return string.Empty;
                if (Diff == null || Diff == string.Empty) return string.Empty;
                string path = Set.LocationPath;
                if (path == string.Empty) return string.Empty;

                if (_path != null) return _path;

                //搜索BeatmapSet文件夹找到Osu文件找到对应Diff的文件
                var dir_info = new System.IO.DirectoryInfo(path).GetFiles($"*[{ObscureDiff(Diff)}].osu");

#if DEBUG
                Sync.Tools.IO.CurrentIO.Write($"[MemoryReader]找到的{dir_info.Length}个Map文件分别为:");
                int _i = 0;
                foreach (var dir in dir_info)
                {
                    Sync.Tools.IO.CurrentIO.Write($"[MemoryReader][{_i++}]{dir.FullName}");
                }
#endif

                if (dir_info.Length>0)
                {
                    _path = dir_info[0].FullName;
                    return _path;
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
