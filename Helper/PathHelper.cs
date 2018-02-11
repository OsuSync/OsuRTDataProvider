using System.IO;
using System.Text;

namespace OsuRTDataProvider.Helper
{
    internal static class PathHelper
    {
        public static string WindowsPathStrip(string entry)
        {
            StringBuilder builder = new StringBuilder(entry);
            foreach (char c in Path.GetInvalidFileNameChars())
                builder.Replace(c.ToString(), string.Empty);
            builder.Replace(".", string.Empty);
            return builder.ToString();
        }
    }
}