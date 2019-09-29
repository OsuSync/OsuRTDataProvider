using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

//Clone Form https://github.com/Deathmax/dxgw2/blob/master/Trainer_Rewrite/SigScan.cs
//Modified by KedamaOvO

//
// sigScan C# Implementation - Written by atom0s [aka Wiccaan]
// Class Version: 2.0.0
//
// [ CHANGE LOG ] -------------------------------------------------------------------------
//
//      2.0.0
//          - Updated to no longer require unsafe or fixed code.
//          - Removed unneeded methods and code.
//
//      1.0.0
//          - First version written and release.
//
// [ CREDITS ] ----------------------------------------------------------------------------
//
// sigScan is based on the FindPattern code written by
// dom1n1k and Patrick at GameDeception.net
//
// Full credit to them for the purpose of this code. I, atom0s, simply
// take credit for converting it to C#.
//
// [ USAGE ] ------------------------------------------------------------------------------
//
// Examples:
//
//      SigScan _sigScan = new SigScan();
//      _sigScan.Process = someProc;
//      _sigScan.Address = new IntPtr(0x123456);
//      _sigScan.Size = 0x1000;
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
//      SigScan _sigScan = new SigScan(someProc, new IntPtr(0x123456), 0x1000);
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
// ----------------------------------------------------------------------------------------
namespace OsuRTDataProvider.Memory
{
    internal class SigScan
    {
        private class MemoryRegion
        {
            public IntPtr AllocationBase { get; set; }
            public IntPtr BaseAddress { get; set; }
            public uint RegionSize { get; set; }
            public byte[] DumpedRegion { get; set; }
        }

        /// <summary>
        /// m_vProcess
        ///
        ///     The process we want to read the memory of.
        /// </summary>
        private Process m_vProcess;

        /// <summary>
        /// m_vAddress
        ///
        ///     The starting address we want to begin reading at.
        /// </summary>

        /// <summary>
        /// m_vSize
        ///
        ///     The number of bytes we wish to read from the process.
        /// </summary>

        #region "sigScan Class Construction"

        /// <summary>
        /// SigScan
        ///
        ///     Overloaded class constructor that sets the class
        ///     properties during construction.
        /// </summary>
        /// <param name="proc">The process to dump the memory from.</param>
        /// <param name="addr">The started address to begin the dump.</param>
        /// <param name="size">The size of the dump.</param>
        public SigScan(Process proc)
        {
            this.m_vProcess = proc;
            //InitMemoryRegionInfo();
        }

        #endregion "sigScan Class Construction"

        public void Reload()
        {
            ResetRegion();
            InitMemoryRegionInfo();
        }

        private List<MemoryRegion> m_memoryRegionList = new List<MemoryRegion>();
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int MEM_COMMIT = 0x00001000;
        private const int PAGE_READWRITE = 0x04;
        private const int PROCESS_WM_READ = 0x0010;

        private void InitMemoryRegionInfo()
        {
            SYSTEM_INFO sys_info;
            //Get the maximum and minimum addresses of the process. 
            GetSystemInfo(out sys_info);
            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            long current_address = (long)proc_min_address;
            long lproc_max_address = (long)proc_max_address;

            IntPtr handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, m_vProcess.Id);

            if (handle == IntPtr.Zero)
            {
                Logger.Error($"Error Code:0x{Marshal.GetLastWin32Error():X8}");
                return;
            }

            MEMORY_BASIC_INFORMATION mem_basic_info = new MEMORY_BASIC_INFORMATION();

            int mem_info_size = Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();

            while (current_address < lproc_max_address)
            {
                //Query the current memory page information.
                int size = VirtualQueryEx(handle, new IntPtr(current_address), out mem_basic_info, (uint)mem_info_size);

                if (size != mem_info_size)
                {
                    Logger.Error($"Error Code:0x{Marshal.GetLastWin32Error():X8}");
                    break;
                }

                //Dump JIT code
                if ((mem_basic_info.Protect & AllocationProtect.PAGE_EXECUTE_READWRITE)>0 && mem_basic_info.State == MEM_COMMIT)
                {
                    var region = new MemoryRegion()
                    {
                        BaseAddress = mem_basic_info.BaseAddress,
                        AllocationBase = mem_basic_info.AllocationBase,
                        RegionSize = mem_basic_info.RegionSize
                    };
                    m_memoryRegionList.Add(region);
                }

                //if (Setting.DebugMode)
                //{
                //    LogHelper.EncryptLog($"BaseAddress: 0x{mem_basic_info.BaseAddress:X8} RegionSize: 0x{mem_basic_info.RegionSize:X8} AllocationBase: 0x{mem_basic_info.AllocationBase:X8} Protect: {mem_basic_info.Protect} Commit: {mem_basic_info.State==MEM_COMMIT}(0x{mem_basic_info.State:X8})");
                //}

                current_address += mem_basic_info.RegionSize;
            }

            CloseHandle(handle);

            if(m_memoryRegionList.Count==0)
            {
                Logger.Error($"Error:List is Empty");
            }
        }

        #region "sigScan Class Private Methods"

        /// <summary>
        /// DumpMemory
        ///
        ///     Internal memory dump function that uses the set class
        ///     properties to dump a memory region.
        /// </summary>
        /// <returns>Boolean based on RPM results and valid properties.</returns>
        private bool DumpMemory()
        {
            try
            {
                // Checks to ensure we have valid data.
                if (this.m_vProcess == null)
                    return false;
                if (this.m_vProcess.HasExited == true)
                    return false;

                // Create the region space to dump into.
                foreach (var region in m_memoryRegionList)
                {
                    if (region.DumpedRegion != null) continue;

                    region.DumpedRegion = new byte[region.RegionSize];

                    bool bReturn = false;
                    int nBytesRead = 0;

                    // Dump the memory.
                    bReturn = ReadProcessMemory(
                        this.m_vProcess.Handle, region.BaseAddress, region.DumpedRegion, region.RegionSize, out nBytesRead
                        );

                    // Validation checks.
                    if (bReturn == false || nBytesRead != region.RegionSize)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($":{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// MaskCheck
        ///
        ///     Compares the current pattern byte to the current memory dump
        ///     byte to check for a match. Uses wildcards to skip bytes that
        ///     are deemed unneeded in the compares.
        /// </summary>
        /// <param name="nOffset">Offset in the dump to start at.</param>
        /// <param name="btPattern">Pattern to scan for.</param>
        /// <param name="strMask">Mask to compare against.</param>
        /// <returns>Boolean depending on if the pattern was found.</returns>
        private bool MaskCheck(MemoryRegion region, int nOffset, byte[] btPattern, string strMask)
        {
            // Loop the pattern and compare to the mask and dump.
            for (int x = 0; x < btPattern.Length && (nOffset + x) < region.RegionSize; x++)
            {
                // If the mask char is a wildcard, just continue.
                if (strMask[x] == '?')
                    continue;

                // If the mask char is not a wildcard, ensure a match is made in the pattern.
                if ((strMask[x] == 'x') && (btPattern[x] != region.DumpedRegion[nOffset + x]))
                    return false;
            }

            // The loop was successful so we found the pattern.
            return true;
        }

        #endregion "sigScan Class Private Methods"

        #region "sigScan Class Public Methods"

        /// <summary>
        /// FindPattern
        ///
        ///     Attempts to locate the given pattern inside the dumped memory region
        ///     compared against the given mask. If the pattern is found, the offset
        ///     is added to the located address and returned to the user.
        /// </summary>
        /// <param name="btPattern">Byte pattern to look for in the dumped region.</param>
        /// <param name="strMask">The mask string to compare against.</param>
        /// <param name="nOffset">The offset added to the result address.</param>
        /// <returns>IntPtr - zero if not found, address if found.</returns>
        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.

                if (!this.DumpMemory())
                    return IntPtr.Zero;

                // Ensure the mask and pattern lengths match.
                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;

                // Loop the region and look for the pattern.
                foreach (var region in m_memoryRegionList)
                {
                    for (int x = 0; x < region.DumpedRegion.Length; x++)
                    {
                        if (this.MaskCheck(region, x, btPattern, strMask))
                        {
                            // The pattern was found, return it.
                            return new IntPtr((int)region.BaseAddress + (x + nOffset));
                        }
                    }
                }
                // Pattern was not found.
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Logger.Error($":{ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ResetRegion
        ///
        ///     Resets the memory dump array to nothing to allow
        ///     the class to redump the memory.
        /// </summary>
        public void ResetRegion()
        {
            m_memoryRegionList.Clear();
        }

        #endregion "sigScan Class Public Methods"

        #region "sigScan Class Properties"

        public Process Process
        {
            get { return this.m_vProcess; }
            set { this.m_vProcess = value; }
        }

        #endregion "sigScan Class Properties"

        #region PInvoke

#if !X64
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public AllocationProtect Protect;
            public uint Type;
        }
#else
        [StructLayout(LayoutKind.Sequential,Pack = 16)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint __alignment1;
            public ulong RegionSize;
            public uint State;
            public AllocationProtect Protect;
            public uint Type;
            public uint __alignment2;
        }
#endif
        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            private ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        /// <summary>
        /// ReadProcessMemory
        ///
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

#endregion PInvoke
    }
}