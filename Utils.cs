using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuRTDataProvider
{
    public class Utils
    {
        public static double ConvertVersionStringToValue(string osu_version_string)
        {
            if (double.TryParse(Regex.Match(osu_version_string, @"\d+(\.\d*)?").Value.ToString(), out var ver))
            {
                return ver;
            }

#if DEBUG
            throw new Exception("无法解析屙屎版本号:"+osu_version_string);
#endif
            return 0;
        }
    }
}
