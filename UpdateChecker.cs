using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuRTDataProvider
{
    static class UpdateChecker
    {
        private static readonly Regex NAME_REGEX = new Regex(@"""name"":\s*""v(\d+\.\d+\.\d+)""");

        private const string LATEST_RELEASE_URL = "https://api.github.com/repos/OsuSync/OsuRTDataProvider/releases/latest";

        public static void CheckUpdate()
        {
            try
            {
                string data = GetHttpData(LATEST_RELEASE_URL);
                var groups = NAME_REGEX.Match(data).Groups;
                string ortdp_version = groups[1].Value;
                bool has_update = CheckRtppUpdate(ortdp_version);

                if(has_update)
                {
                    Logger.Warn(DefaultLanguage.CHECK_GOTO_RELEASE_PAGE_HINT);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private static bool CheckRtppUpdate(string tag)
        {
            Version ver = Version.Parse(tag);
            Version selfVer = Version.Parse(OsuRTDataProviderPlugin.VERSION);
            if (ver > selfVer)
            {
                Logger.Warn(string.Format(DefaultLanguage.LANG_CHECK_ORTDP_UPDATE, ver));
                return true;
            }
            return false;
        }

        private static string GetHttpData(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create(url);
            wReq.UserAgent = "OsuSync";
            WebResponse wResp = wReq.GetResponse();
            Stream respStream = wResp.GetResponseStream();

            using (StreamReader reader = new StreamReader(respStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
