using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace OsuRTDataProvider.Memory
{

    public class BeatmapOffsetInfo
    {
        public enum CompareCondition
        {
            None,
            Older,
            OlderOrEquals,
            Equals,
            NewerOrEquals,
            Newer
        }

        public double Version { get; set; }
        public int BeatmapAddressOffset { get; set; }
        public int BeatmapSetAddressOffset { get; set; }
        public int BeatmapFolderAddressOffset { get; set; }
        public int BeatmapFileNameAddressOffset { get; set; }
        public CompareCondition VersionCompareCondition { get; set; }
        public void AddOffset(BeatmapOffsetInfo beatmapOffsetInfo)
        {
            BeatmapAddressOffset += beatmapOffsetInfo.BeatmapAddressOffset;
            BeatmapFolderAddressOffset += beatmapOffsetInfo.BeatmapFolderAddressOffset;
            BeatmapSetAddressOffset += beatmapOffsetInfo.BeatmapSetAddressOffset;
            BeatmapFileNameAddressOffset += beatmapOffsetInfo.BeatmapFileNameAddressOffset;
        }
        
        public void SetOffset(BeatmapOffsetInfo beatmapOffsetInfo)
        {
            BeatmapAddressOffset = beatmapOffsetInfo.BeatmapAddressOffset;
            BeatmapFolderAddressOffset =  beatmapOffsetInfo.BeatmapFolderAddressOffset;
            BeatmapSetAddressOffset = beatmapOffsetInfo.BeatmapSetAddressOffset;
            BeatmapFileNameAddressOffset = beatmapOffsetInfo.BeatmapFileNameAddressOffset;
        }


        public override string ToString() => $"{VersionCompareCondition} " +
                                             $"{Version} " +
                                             $"BeatmapAddress: {BeatmapAddressOffset} " +
                                             $"BeatmapFolder: {BeatmapAddressOffset} " +
                                             $"BeatmapSet: {BeatmapSetAddressOffset} " +
                                             $"BeatmapFileName: {BeatmapFileNameAddressOffset}";

        public static BeatmapOffsetInfo MatchVersion(double version)
        {
            Dictionary<OsuVersionCompareInfoAttribute, PropertyInfo> propertyForVersions = new Dictionary<OsuVersionCompareInfoAttribute, PropertyInfo>();
            Type t = typeof(BeatmapOffsetInfo);
            var methods = t.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (!method.IsDefined(typeof(OsuVersionCompareInfoAttribute), false))
                {
                    continue;
                }

                var attr = method.GetCustomAttribute<OsuVersionCompareInfoAttribute>();

                propertyForVersions.Add(attr, method);
            }
            propertyForVersions = propertyForVersions.OrderByDescending(item => item.Key.OsuVersion).
                ToDictionary(item => item.Key,item => item.Value);

            bool versionMatched = false;
            foreach (var compareInfo in propertyForVersions)
            {
                double comparedVersion = compareInfo.Key.OsuVersion;
                switch (compareInfo.Key.CompareCondition)
                {
                    case CompareCondition.Older: 
                        versionMatched = comparedVersion < version;
                        break;
                    case CompareCondition.OlderOrEquals:
                        versionMatched = comparedVersion <= version;
                        break;
                    case CompareCondition.Newer:
                        versionMatched = comparedVersion > version;
                        break;
                    case CompareCondition.NewerOrEquals:
                        versionMatched = comparedVersion <= version;
                        break;
                    case CompareCondition.Equals:
                        //https://www.jetbrains.com/help/resharper/2022.1/CompareOfFloatsByEqualityOperator.html
                        versionMatched = Math.Abs(comparedVersion - version) < 0.00001;
                        break;
                }

                if (versionMatched)
                {
                    return (BeatmapOffsetInfo)compareInfo.Value.GetValue(null);
                }
            }
            

            return new BeatmapOffsetInfo {Version = version};
        }

        [OsuVersionCompareInfo(20190816, CompareCondition.Older)]
        public static BeatmapOffsetInfo Version20190816 { get; } = new BeatmapOffsetInfo
        {
            Version = 20190816,
            BeatmapFileNameAddressOffset = -4,
            BeatmapSetAddressOffset = -4,
            BeatmapAddressOffset = -4,
            BeatmapFolderAddressOffset = -4,
            VersionCompareCondition = CompareCondition.Older
        };


        [OsuVersionCompareInfo(20211014, CompareCondition.NewerOrEquals)]
        public static BeatmapOffsetInfo Version20211014 { get; } = new BeatmapOffsetInfo
        {
            Version = 20211014,
            BeatmapFileNameAddressOffset = 0,
            BeatmapSetAddressOffset = 0,
            BeatmapAddressOffset = 4,
            BeatmapFolderAddressOffset = 0,
            VersionCompareCondition = CompareCondition.NewerOrEquals
        };

        [OsuVersionCompareInfo(20220406.3, CompareCondition.NewerOrEquals)]
        public static BeatmapOffsetInfo Version202204063 { get; } = new BeatmapOffsetInfo
        {
            Version = 20220406.3,
            BeatmapFileNameAddressOffset = 8,
            BeatmapSetAddressOffset = 0,
            BeatmapAddressOffset = 0,
            BeatmapFolderAddressOffset = 4,
            VersionCompareCondition = CompareCondition.NewerOrEquals
        };



    }
}