using System;
using System.Collections.Generic;
using System.Linq;
using Sync.Tools;

namespace OsuRTDataProvider.Memory
{
    public class OffsetInfo
    {
        //Version,BeatmapAddressOffset,BeatmapFolderAddressOffset,BeatmapSetAddressOffset,BeatmapFileNameAddressOffset,VersionCompareOperator
        private const string VersionOffsetString = 
            "20190806,-4,-4,-4,-4,<|" + 
            "20211014,4,0,0,0,>=|" + 
            "202204063,0,4,0,8,>=";

        public static Dictionary<double, OffsetInfo> Versions;
        public int BeatmapAddressOffset { get; set; }
        public int BeatmapSetAddressOffset { get; set; }
        public int BeatmapFolderAddressOffset { get; set; }
        public int BeatmapFileNameAddressOffset { get; set; }
        public string VersionCompareOperator { get; set; }
        public static OffsetInfo Zero { get; } = new OffsetInfo();
        public void AddOffset(OffsetInfo offsetInfo)
        {
            BeatmapAddressOffset += offsetInfo.BeatmapAddressOffset;
            BeatmapFolderAddressOffset += offsetInfo.BeatmapFolderAddressOffset;
            BeatmapSetAddressOffset += offsetInfo.BeatmapSetAddressOffset;
            BeatmapFileNameAddressOffset += offsetInfo.BeatmapFileNameAddressOffset;
        }
        
        public void SetOffset(OffsetInfo offsetInfo)
        {
            BeatmapAddressOffset = offsetInfo.BeatmapAddressOffset;
            BeatmapFolderAddressOffset =  offsetInfo.BeatmapFolderAddressOffset;
            BeatmapSetAddressOffset = offsetInfo.BeatmapSetAddressOffset;
            BeatmapFileNameAddressOffset = offsetInfo.BeatmapFileNameAddressOffset;
        }
       

        private static readonly object s_staticLock = new object();

        static void InitVersionDict()
        {
            lock (s_staticLock)
            {
                Versions = new Dictionary<double, OffsetInfo>();

                //Linq below is as same as
                //var offsetStr = VersionOffsetString.Split('|');
                //List<string[]> versions = new List<string[]>();
                //foreach(var offsetDescriptor in offsetStr)
                //{
                //     versions.Add(offsetDescriptor.Split(','));
                //}


                var versions =
                    from offsetStr
                        in VersionOffsetString.Split('|')
                    select
                        offsetStr.Split(',');
                foreach (var ver in versions)
                {
                    double[] data = (from verData in ver select double.Parse(verData)).Take(5).ToArray();
                    OffsetInfo info = new OffsetInfo
                    {
                        BeatmapAddressOffset = (int) data[1],
                        BeatmapFolderAddressOffset = (int) data[2],
                        BeatmapSetAddressOffset = (int) data[3],
                        BeatmapFileNameAddressOffset = (int) data[4]
                    };

                    info.VersionCompareOperator = ver[5];
                    Versions.Add(data[0], info);
                }

                Versions = Versions.OrderByDescending(v => v.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }


        public static OffsetInfo GetByVersion(double version)
        {
            if (Versions != null)
                return Versions.ContainsKey(version)
                    ? Versions[version]
                    : Zero;
            InitVersionDict();
            return Versions?.ContainsKey(version) ?? false
                ? Versions[version]
                : Zero;
        }

        public static OffsetInfo AutoMatch(double version)
        {
            if (Versions == null)
            {
                InitVersionDict();
            }
            foreach (var ver in Versions)
            {
                bool compareResult = false;
                switch (ver.Value.VersionCompareOperator)
                {
                    case ">": 
                        compareResult = ver.Key > version;
                        break;
                    case "<":
                        compareResult = ver.Key < version;
                        break;
                    case ">=":
                        compareResult = ver.Key >= version;
                        break;
                    case "<=":
                        compareResult = ver.Key <= version;
                        break;
                }

                if (!compareResult)
                    continue;

                return ver.Value;
            }

            return Zero;
        }
        
    }
}