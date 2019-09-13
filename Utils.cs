using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (double.TryParse(Regex.Match(osu_version_string, @"\d+(\.\d*)?").Value.ToString(),NumberStyles.Float, CultureInfo.InvariantCulture, out var ver))
            {

                return ver;
            }

            throw new Exception("Can't parse version: "+osu_version_string);
        }
    }

    public static class Logger
    {
        static Logger<OsuRTDataProviderPlugin> logger=new Logger<OsuRTDataProviderPlugin>();

        public static void Info(string message) => logger.LogInfomation(message);

        public static void Debug(string message)
        {
            if (Setting.DebugMode)
                logger.LogInfomation(message);
        }

        public static void Error(string message) => logger.LogError(message);
        public static void Warn(string message) => logger.LogWarning(message);
    }
}
