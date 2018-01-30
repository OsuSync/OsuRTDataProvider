using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OsuRTDataProvider.Memory
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

        private string _key1 = "(A_Ud0ahsof;askf";
        private string strKey = "aifjle;fbn vksig";

        protected void EncryptLog(string plainText)
        {
            string msg = plainText;

#if !DEBUG
            if (Setting.DebugMode)
            {
                //分组加密算法
                SymmetricAlgorithm des = Rijndael.Create();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
                                                                          //设置密钥及密钥向量
                des.Key = Encoding.UTF8.GetBytes(strKey);
                des.IV = StringToByte(_key1);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组
                        msg = $"{Convert.ToBase64String(cipherBytes)}";
                    }
                }
                Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]{msg}");
            }
#else
            Sync.Tools.IO.CurrentIO.Write($"[OsuRTDataProvider]{msg}");
#endif
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

        protected bool TryReadIntFromMemory(IntPtr address,out int value)
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

        protected bool TryReadDoubleFromMemory(IntPtr address,out double value)
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
            TryReadIntPtrFromMemory(address,out IntPtr str_base);

            try
            {
                TryReadIntFromMemory(str_base + 0x4, out int len);
                len*= 2;

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