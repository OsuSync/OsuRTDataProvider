using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Memory
{
    public class OsuVersionCompareInfoAttribute : Attribute
    {
        public OsuVersionCompareInfoAttribute(double osuVersion, BeatmapOffsetInfo.CompareCondition compareCondition)
        {
            OsuVersion = osuVersion;
            CompareCondition = compareCondition;
        }

        public double OsuVersion { get; }
        public BeatmapOffsetInfo.CompareCondition CompareCondition { get; }

    }
}
