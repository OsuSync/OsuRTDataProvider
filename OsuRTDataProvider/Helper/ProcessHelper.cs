using System.ComponentModel;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace OsuRTDataProvider.Helper;

public static class ProcessHelper
{
    public static unsafe string GetProcessPath(Process process)
    {
        if (process == null || process.HasExited) return null;

        // Try the standard way first, but catch the specific exception
        try
        {
            return process.MainModule.FileName;
        }
        catch (Win32Exception)
        {
            // Fallback to Win32 API if MainModule fails
        }

        HANDLE handle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION,
            false,
            (uint)process.Id);

        if (handle.IsNull)
            return null;

        try
        {
            uint capacity = 2048;
            char* buffer = stackalloc char[(int)capacity];

            if (PInvoke.QueryFullProcessImageName(handle, 0, new PWSTR(buffer), &capacity))
            {
                return new string(buffer, 0, (int)capacity);
            }
        }
        finally
        {
            PInvoke.CloseHandle(handle);
        }

        return null;
    }
}