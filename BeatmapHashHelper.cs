using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OnlinePPOutput
{
    public static class BeatmapHashHelper
    {
        static MD5 md5 = new MD5CryptoServiceProvider();

        public static string GetHashFromOsuFile(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                data = md5.ComputeHash(data);
            }
            catch { return string.Empty; }

            foreach (byte b in data)
                sb.Append(b.ToString("x2"));

            var result = sb.ToString();

            return result;
        }
    }
}