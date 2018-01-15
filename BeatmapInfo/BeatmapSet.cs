using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static OsuRTDataProvider.DefaultLanguage;

namespace OsuRTDataProvider.BeatmapInfo
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

                if (Folder.Length > 0)
                {
                    string name = Folder;
                    int len = name.IndexOf(' ');
                    if (len != -1)
                    {
                        string id = name.Substring(0, len);

                        if (int.TryParse(id, out m_beatmap_id))
                            return m_beatmap_id;
                    }
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
        public string Artist { get; private set; }

        /// <summary>
        /// Return the beatmap's title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Return the first of all possible beatmap set paths.
        /// If not found.return string.Empty.
        /// </summary>
        public string Folder { get; private set; }

        public static BeatmapSet Empty => new BeatmapSet(-1,0,"","","");
        public int OsuClientID { get;private set;}

        public BeatmapSet(int id,int osu_id,string artist,string title,string folder)
        {
            BeatmapSetID = id;
            OsuClientID = osu_id;
            Folder = Path.Combine(Setting.SongsPath,folder);
            Artist = artist;
            Title = title;
        }
    }
}