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
    public static class Utils
    {
        public static double ConvertVersionStringToValue(string osu_version_string)
        {
            if (double.TryParse(Regex.Match(osu_version_string, @"\d+(\.\d*)?").Value.ToString(),NumberStyles.Float, CultureInfo.InvariantCulture, out var ver))
            {

                return ver;
            }

            throw new Exception("Can't parse version: "+osu_version_string);
        }

        //https://gist.github.com/peppy/3a11cb58c856b6af7c1916422f668899
        public static List<double> GetErrorStatisticsArray(List<int> list)
        {
            if (list == null || list.Count == 0)
                return null;
            List<double> result = new List<double>(4);
            double total = 0, _total = 0, totalAll = 0;
            int count = 0, _count = 0;
            int max = 0, min = int.MaxValue;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > max)
                    max = list[i];
                if (list[i] < min)
                    min = list[i];
                totalAll += list[i];
                if (list[i] >= 0)
                {
                    total += list[i];
                    count++;
                }
                else
                {
                    _total += list[i];
                    _count++;
                }
            }
            double avarage = totalAll / list.Count;
            double variance = 0;
            for (int i = 0; i < list.Count; i++)
            {
                variance += Math.Pow(list[i] - avarage, 2);
            }
            variance = variance / list.Count;
            result.Add(_count == 0 ? 0 : _total / _count); //0
            result.Add(count == 0 ? 0 : total / count); //1
            result.Add(avarage); //2
            result.Add(variance); //3
            result.Add(Math.Sqrt(variance)); //4
            result.Add(max); //5
            result.Add(min); //6
            return result;
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
