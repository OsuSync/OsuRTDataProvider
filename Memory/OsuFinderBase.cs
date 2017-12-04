using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MemoryReader.Memory
{
    internal class OsuFinderBase
    {
        protected SigScan SigScan { get; private set; }
        protected Process OsuProcess { get; private set; }

        private int _read_max_string_length = 4096;

        public int ReadMaxStringLength
        {
            get => _read_max_string_length;
            set
            {
                _read_max_string_length = value;
                _str_buf = new byte[_read_max_string_length];
            }
        }

        public OsuFinderBase(Process process)
        {
            OsuProcess = process;
            SigScan = new SigScan(OsuProcess);

            _str_buf = new byte[ReadMaxStringLength];
        }

        private List<byte> _a = new List<byte>(64);

        protected byte[] StringToByte(string s)
        {
            _a.Clear();
            foreach (var c in s) _a.Add((byte)c);
            return _a.ToArray();
        }

        private byte[] _number_buf = new byte[8];

        protected int ReadIntFromMemory(IntPtr address)
        {
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(int), out ret_size_ptr))
            {
                return BitConverter.ToInt32(_number_buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        protected int ReadShortFromMemory(IntPtr address)
        {
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(short), out ret_size_ptr))
            {
                return BitConverter.ToUInt16(_number_buf, 0);
            }
            return 0;
            //throw new ArgumentException();
        }

        protected double ReadDoubleFromMemory(IntPtr address)
        {
            int ret_size_ptr = 0;
            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(double), out ret_size_ptr))
            {
                return BitConverter.ToDouble(_number_buf, 0);
            }
            return 0.0;
        }

        private byte[] _str_buf;

        protected bool TryReadStringFromMemory(IntPtr address, out string str)
        {
            str = null;
            IntPtr str_base = (IntPtr)ReadIntFromMemory(address);
            try
            {
                int len = ReadIntFromMemory(str_base + 0x4) * 2;

                if (len > ReadMaxStringLength || len <= 0) return false;

                if (SigScan.ReadProcessMemory(OsuProcess.Handle, str_base + 0x8, _str_buf, (uint)len, out int ret_size_ptr))
                {
                    str = Encoding.Unicode.GetString(_str_buf, 0, len);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }
    }
}