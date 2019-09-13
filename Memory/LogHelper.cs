using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Memory
{
    static class LogHelper
    {
        public static void LogToFile(string plainText)
        {
            /*
            string msg = plainText;
#if DEBUG
            ISyncOutput output = IO.CurrentIO;
#else
            ISyncOutput output = IO.FileLogger;
#endif

            output.Write($"{msg}");
            */

            //now is directly output to default in DebugMode
            Logger.Debug(plainText);
        }
    }
}
