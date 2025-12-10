using System.Reflection;

namespace OsuRTDataProvider
{
    internal static class VersionInfo
    {
        public static string GetVersion() => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
