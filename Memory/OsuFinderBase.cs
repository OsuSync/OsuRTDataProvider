using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OsuRTDataProvider.Memory
{
    internal abstract class OsuFinderBase
    {
        protected SigScan SigScan { get; private set; }
        protected Process OsuProcess { get; private set; }

        private int max_bytes_length = 40960;

        public int ReadMaxStringLength
        {
            get => max_bytes_length;
            set
            {
                max_bytes_length = value;
                _bytes_buf = new byte[max_bytes_length];
            }
        }

        public OsuFinderBase(Process process)
        {
            OsuProcess = process;
            SigScan = new SigScan(OsuProcess);

            _bytes_buf = new byte[ReadMaxStringLength];
        }

        private List<byte> _a = new List<byte>(64);

        protected byte[] StringToByte(string s)
        {
            _a.Clear();
            foreach (var c in s) _a.Add((byte)c);
            return _a.ToArray();
        }

        private byte[] _number_buf = new byte[8];

        protected bool TryReadIntPtrFromMemory(IntPtr address, out IntPtr value)
        {
            int ret_size_ptr = 0;
            value = IntPtr.Zero;

            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(int), out ret_size_ptr))
            {
                value = (IntPtr)BitConverter.ToInt32(_number_buf, 0);
                return true;
            }
            return false;
        }

        protected bool TryReadIntFromMemory(IntPtr address, out int value)
        {
            int ret_size_ptr = 0;
            value = 0;

            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(int), out ret_size_ptr))
            {
                value = BitConverter.ToInt32(_number_buf, 0);
                return true;
            }
            return false;
        }

        protected bool TryReadShortFromMemory(IntPtr address, out ushort value)
        {
            int ret_size_ptr = 0;
            value = 0;

            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(ushort), out ret_size_ptr))
            {
                value = BitConverter.ToUInt16(_number_buf, 0);
                return true;
            }
            return false;
        }

        protected bool TryReadDoubleFromMemory(IntPtr address, out double value)
        {
            int ret_size_ptr = 0;
            value = double.NaN;

            if (SigScan.ReadProcessMemory(OsuProcess.Handle, address, _number_buf, sizeof(double), out ret_size_ptr))
            {
                value = BitConverter.ToDouble(_number_buf, 0);
                return true;
            }
            return false;
        }

        private byte[] _bytes_buf;

        protected bool TryReadStringFromMemory(IntPtr address, out string str)
        {
            str = null;
            TryReadIntPtrFromMemory(address, out IntPtr str_base);

            try
            {
                TryReadIntFromMemory(str_base + 0x4, out int len);
                len *= 2;

                if (len > ReadMaxStringLength || len <= 0) return false;

                if (SigScan.ReadProcessMemory(OsuProcess.Handle, str_base + 0x8, _bytes_buf, (uint)len, out int ret_size_ptr))
                {
                    str = Encoding.Unicode.GetString(_bytes_buf, 0, len);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        protected bool TryReadListFromMemory(IntPtr address, out List<int> list)
        {
            list = null;
            int type_size = Marshal.SizeOf<int>();
            TryReadIntPtrFromMemory(address, out IntPtr list_ptr);

            try
            {
                TryReadIntFromMemory(list_ptr + 0xc, out int len);

                if (len > ReadMaxStringLength || len <= 0) return false;

                TryReadIntPtrFromMemory(list_ptr + 0x4, out var array_ptr);

                if (SigScan.ReadProcessMemory(OsuProcess.Handle, array_ptr + 0x8, _bytes_buf, (uint)(len * type_size), out int ret_size_ptr))
                {
                    int[] data = new int[len];
                    Buffer.BlockCopy(_bytes_buf, 0, data, 0, len * type_size);
                    list = new List<int>(data);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public abstract bool TryInit();
    }
}