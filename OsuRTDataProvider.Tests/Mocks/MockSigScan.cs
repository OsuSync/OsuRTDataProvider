using System;
using System.Collections.Generic;
using OsuRTDataProvider.Memory;

namespace OsuRTDataProvider.Tests.Mocks
{
    public class MockSigScan : ISigScan
    {
        private readonly byte[] _memory;
        private readonly int _baseAddress;

        public MockSigScan(byte[] memory, int baseAddress = 0x1000)
        {
            _memory = memory;
            _baseAddress = baseAddress;
        }

        public IntPtr FindPattern(string pattern, int offset = 0)
        {
            // Parse pattern
            var parts = pattern.Split(' ');
            var patternBytes = new byte?[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                    patternBytes[i] = null;
                else
                    patternBytes[i] = Convert.ToByte(parts[i], 16);
            }

            // Search
            for (int i = 0; i <= _memory.Length - patternBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < patternBytes.Length; j++)
                {
                    if (patternBytes[j].HasValue && _memory[i + j] != patternBytes[j].Value)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return new IntPtr(_baseAddress + i + offset);
                }
            }

            return IntPtr.Zero;
        }

        public void Reload()
        {
            // Do nothing
        }

        public void ResetRegion()
        {
            // Do nothing
        }

        public bool ReadMemory(IntPtr address, byte[] buffer, int size, out int bytesRead)
        {
            bytesRead = 0;
            long offset = address.ToInt64() - _baseAddress;

            if (offset < 0 || offset >= _memory.Length)
                return false;

            int toRead = Math.Min(size, _memory.Length - (int)offset);
            Array.Copy(_memory, (int)offset, buffer, 0, toRead);
            bytesRead = toRead;
            return toRead == size;
        }
    }
}
