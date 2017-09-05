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

        private static string[] s_replace_list=new string[] { "*",".",":","?","\"","<",">"};
        private static string[] s_replace_target_list = new string[] { "", "-" };

        private List<string> _GenPath(string path,int startindex)
        {
            List<string> ret = new List<string>();
            StringBuilder builder = new StringBuilder(path);
            foreach (var sc in s_replace_list)
                for(int i= startindex; i<builder.Length;++i)
                {
                    if(builder[i]==sc[0])
                    {
                        foreach(var tc in s_replace_target_list)
                        {
                            StringBuilder tmp_builder = new StringBuilder(builder.ToString());
                            tmp_builder.Replace(sc, tc, i, 1);
                            ret.AddRange(_GenPath(tmp_builder.ToString(),i));
                            ret.Add(tmp_builder.ToString());
                        }
                    }
                }

            return ret;
        }

        private List<string> GenPaths()
        {
            List<string> ret = _GenPath($"{BeatmapSetID} {Artist} - {Title}",0);
            ret.AddRange(_GenPath($"{BeatmapSetID}  - {Title}",0));

            ret.Add($"{BeatmapSetID} {Artist} - {Title}");
            ret.Add($"{BeatmapSetID} {Artist} - {Title}".Replace(" ", "+"));
            ret.Add($"{BeatmapSetID}  - {Title}");
            ret.Add($"{BeatmapSetID}  - {Title}".Replace(" ","+"));

            return ret;
        }

        public string LocationPath
        {
            get
            {
                if (Artist == null || Artist == "") return "";
                if (Title == null || Title == "") return "";

                string artist = Artist;
                string title = Title;

                List<string> paths = GenPaths();
                foreach(var  path in paths)
                {
                    if(SongPathExists(path))
                    {
                        return Path.Combine(Setting.SongsPath, path);
                    }
                }

                return "";
            }
        }

        public BeatmapSet(int id)
        {
            BeatmapSetID = id;
        }
    }
}
