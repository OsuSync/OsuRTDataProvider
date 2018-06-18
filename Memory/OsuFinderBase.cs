using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace OsuRTDataProvider.Memory
{
    internal abstract class OsuFinderBase
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

        protected void EncryptLog(string plainText)
        {
            string msg = plainText;
            ISyncOutput output = IO.CurrentIO;

#if !DEBUG
            string _public_key = @"<RSAKeyValue><Modulus>yAs66SUY9SqPiZcoriVGzbLkpHGzJcyhLustyfA6fNQjE8COalr6rnjgyI44hFSYkhpz6ThMjsnINLDPv23k6ZkPzQSXA7HyBDHUj6L8xf9YoypWjGlRbou6usynWfK525bzOomGaLFSmz8WN0KZgzfsP42oHBHcwv6DeWurwH2KZogYv8NDAACslizbApJET3oPFPdiO/PnwMOoPpXnJYSE00S23ZsEFkqj1eGOWnB7Xije/NDL1ijxSFn27YhT66dI64mluz1818LaaPDYvCHivkCKqhKdpJeDrYfOZiY2v2Hpn3hr/DUEM14vJTpBTDxBfG498X5j5J0gZDQ8gQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        
            output = IO.FileLogger;
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(_public_key);
                byte[] cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), false);
                msg = Convert.ToBase64String(cipherbytes);
            }
#endif
            output.Write($"[OsuRTDataProvider]{msg}");
        }

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

        private byte[] _str_buf;

        protected bool TryReadStringFromMemory(IntPtr address, out string str)
        {
            str = null;
            TryReadIntPtrFromMemory(address, out IntPtr str_base);

            try
            {
                TryReadIntFromMemory(str_base + 0x4, out int len);
                len *= 2;

                if (len > ReadMaxStringLength || len <= 0) return false;

                if (SigScan.ReadProcessMemory(OsuProcess.Handle, str_base + 0x8, _str_buf, (uint)len, out int ret_size_ptr))
                {
                    str = Encoding.Unicode.GetString(_str_buf, 0, len);
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