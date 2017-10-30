using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryReader.Memory
{
    class OsuFinderBase
    {
        protected SigScan SigScan { get; private set; }
        protected Process OsuProcess { get; private set; }

        public OsuFinderBase(Process process)
        {
            OsuProcess = process;
            SigScan = new SigScan(OsuProcess);
        }

        protected int ReadIntFromMemory(IntPtr address)
        {
            uint size = 4;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToInt32(buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        protected int ReadShortFromMemory(IntPtr address)
        {
            uint size = 2;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToUInt16(buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        protected double ReadDoubleFromMemory(IntPtr address)
        {
            uint size = 8;
            byte[] buf = new byte[size];
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, buf, size, out ret_size_ptr))
            {
                return BitConverter.ToDouble(buf, 0);
            }
            return 0.0;
        }

        protected string ReadStringFromMemory(IntPtr address)
        {
            IntPtr str_base = (IntPtr)ReadIntFromMemory(address);
            uint len = (uint)ReadIntFromMemory(str_base + 0x4) * 2;

            byte[] buf = new byte[len];
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, str_base + 0x8, buf, len, out int ret_size_ptr))
            {
                return Encoding.Unicode.GetString(buf);
            }
            return string.Empty;
        }
    }
}
