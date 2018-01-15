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
        public string Diff { get; private set; }

        private string _path;

        public string Filename { get; private set; }
        public string FilenameFull => Path.Combine(Set?.Folder, Filename);

        public static Beatmap Empty => new Beatmap(-1,"","");

        public Beatmap(int id,string diff,string filename)
        {
            BeatmapID = id;
            Diff = diff;
            Filename = filename;
        }
    }
}