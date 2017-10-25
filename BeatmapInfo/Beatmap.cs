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

        public string LocationFile
        {
            get
            {
                if (Set == null) return "";
                if (Diff == null || Diff == "") return "";
                string path = Set.LocationPath;
                if (path == "") return "";

                List<string> diffs = _GenDiff(Diff, 0);
                diffs.Add(Diff);

                //搜索BeatmapSet文件夹找到Osu文件找到对应Diff的文件
                var dir_info = new System.IO.DirectoryInfo(path).GetFiles($"*.osu");

                foreach(var info in dir_info)
                {
                    int pos = info.Name.IndexOf('[');
                    if (pos == -1) return "";
                    string file_diff = info.Name.Substring(pos);

                    foreach (var diff in diffs)
                    {
                        if (file_diff.Contains(diff))
                            return System.IO.Path.Combine(path, info.Name);
                    }
                }

                return "";
            }
        }

        public static Beatmap Empty = new Beatmap(-1);

        public Beatmap(int id)
        {
            BeatmapID = id;
        }
    }
}
