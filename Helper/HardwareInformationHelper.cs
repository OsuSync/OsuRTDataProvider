using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Helper
{
    static class HardwareInformationHelper
    {
        public static string GetPhysicalMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
        }

        public static String GetProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            String info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                string name = (string)mo["Name"];

                info = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];
                
                break;
            }
            return info;
        }

        public static string GetOSInformation()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
                }
                catch { }
            }
            return "BIOS Maker: Unknown";
        }

        private static void Print(string str)
        {
#if !DEBUG
            Sync.Tools.IO.FileLogger.Write($"{str}");
#else   
            Sync.Tools.IO.CurrentIO.Write($"{str}");
#endif
        }

        public static void PrintHardwareInformation()
        {
            Print($"CLI: {Environment.Version}");
            Print($"OS: {GetOSInformation()}");
            Print($"CPU: {GetProcessorInformation()}");
            Print($"Memory:  {GetPhysicalMemory()} Total");
        }
    }
}
