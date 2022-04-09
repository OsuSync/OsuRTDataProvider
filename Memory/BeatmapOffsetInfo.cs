using System;
using System.Collections.Generic;
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
            Dictionary<double, MethodInfo> methodsForVersions = new Dictionary<double, MethodInfo>();
            Type t = typeof(BeatmapOffsetInfo);
            var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (!method.IsDefined(typeof(OsuVersionCompareInfoAttribute), false))
                {
                    continue;
                }

                var attr = method.GetCustomAttribute<OsuVersionCompareInfoAttribute>();

                methodsForVersions.Add(attr.OsuVersion, method);
            }

            methodsForVersions = methodsForVersions.OrderByDescending(item => item.Key).
                ToDictionary(item => item.Key,item => item.Value);

            foreach (var method in methodsForVersions)
            {
                object invokeRet = method.Value.Invoke(null, new object[] {version});
                if (invokeRet != null)
                {
                    return (BeatmapOffsetInfo)invokeRet;
                }
            }

            return new BeatmapOffsetInfo {Version = version};
        }

        [OsuVersionCompareInfo(20190816, CompareCondition.Older)]
        public static BeatmapOffsetInfo Version20190816(double version)
        {
            if (version < 20190816)
            {
                return new BeatmapOffsetInfo
                {
                    Version = 20190816,
                    BeatmapFileNameAddressOffset = -4,
                    BeatmapSetAddressOffset = -4,
                    BeatmapAddressOffset = -4,
                    BeatmapFolderAddressOffset = -4,
                    VersionCompareCondition = CompareCondition.Older
                };
            }

            return null;
        }

        [OsuVersionCompareInfo(20211014, CompareCondition.NewerOrEquals)]
        public static BeatmapOffsetInfo Version20211014(double version)
        {
            if (version >= 20211014)
            {
                return new BeatmapOffsetInfo
                {
                    Version = 20211014,
                    BeatmapFileNameAddressOffset = 0,
                    BeatmapSetAddressOffset = 0,
                    BeatmapAddressOffset = 4,
                    BeatmapFolderAddressOffset = 0,
                    VersionCompareCondition = CompareCondition.NewerOrEquals
                };
            }

            return null;
        }

        [OsuVersionCompareInfo(20220406.3, CompareCondition.NewerOrEquals)]
        public static BeatmapOffsetInfo Version20220406_3(double version)
        {
            if (version >= 20220406.3)
            {
                return new BeatmapOffsetInfo
                {
                    Version = 20220406.3,
                    BeatmapFileNameAddressOffset = 8,
                    BeatmapSetAddressOffset = 0,
                    BeatmapAddressOffset = 0,
                    BeatmapFolderAddressOffset = 4,
                    VersionCompareCondition = CompareCondition.NewerOrEquals
                };
            }

            return null;
        }


    }
}