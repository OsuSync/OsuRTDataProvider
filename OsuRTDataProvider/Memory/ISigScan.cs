using System;

namespace OsuRTDataProvider.Memory
{
    public interface ISigScan
    {
        IntPtr FindPattern(string pattern, int offset = 0);
        void Reload();
        void ResetRegion();
        bool ReadMemory(IntPtr address, byte[] buffer, int size, out int bytesRead);
    }
}
