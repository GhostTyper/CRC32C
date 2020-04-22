using System;
using System.Runtime.Intrinsics.X86;

namespace SharpFast.Checksums
{
    public class CRC32C
    {
        private static uint[]? table;
        public static HardwareAcceleration Acceleration;

        private uint crc;

        public CRC32C()
        {
            crc = 0xFFFFFFFF;

            if (Acceleration == HardwareAcceleration.Undefined)
                if (Sse42.X64.IsSupported)
                    Acceleration = HardwareAcceleration.Hardware64Bit;
                else if (Sse42.IsSupported)
                    Acceleration = HardwareAcceleration.Hardware32Bit;
                else
                {
                    InitializeSoftwareTable();

                    Acceleration = HardwareAcceleration.Software;
                }
        }

        public static void InitializeSoftwareTable()
        {
            uint[] table = new uint[256];

            uint temp;

            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;

                for (int j = 8; j > 0; --j)
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ 0x82F63B78;
                    else
                        temp >>= 1;

                table[i] = temp;
            }

            CRC32C.table = table;
        }

        public unsafe void Calculate(byte[] data, int offset, int count)
        {
            switch (Acceleration)
            {
                case HardwareAcceleration.Software: // Softwareimplementation.
                    for (int i = 0; i < count; ++i)
                        crc = (crc >> 8) ^ table![((crc) & 0xff) ^ data[offset + i]];
                    return;
                case HardwareAcceleration.Hardware32Bit: // 32 Bit Hardwarebeschleunigt.
                    fixed (byte* bpBuffer = data)
                    {
                        byte* pBuffer = bpBuffer + offset;
                        byte* pEnd = bpBuffer + offset + count - 3;

                        for (; pBuffer < pEnd; pBuffer += 4)
                            crc = Sse42.Crc32(crc, *(uint*)pBuffer);

                        pEnd += 3;

                        for (; pBuffer < pEnd; pBuffer++)
                            crc = Sse42.Crc32(crc, *pBuffer);
                    }
                    return;
                case HardwareAcceleration.Hardware64Bit: // 64 Bit Hardwarebeschleunigt.
                    fixed (byte* bpBuffer = data)
                    {
                        byte* pBuffer = bpBuffer + offset;
                        byte* pEnd = bpBuffer + offset + count - 7;

                        for (; pBuffer < pEnd; pBuffer += 8)
                            crc = (uint)Sse42.X64.Crc32(crc, *(ulong*)pBuffer);

                        pEnd += 7;

                        for (; pBuffer < pEnd; pBuffer++)
                            crc = Sse42.Crc32(crc, *pBuffer);
                    }
                    return;
            }
        }

        public unsafe void Calculate(byte data)
        {
            switch (Acceleration)
            {
                case HardwareAcceleration.Software: // Softwareimplementation.
                    crc = (crc >> 8) ^ table![((crc) & 0xff) ^ data];
                    return;
                case HardwareAcceleration.Hardware32Bit: // 32 Bit Hardwarebeschleunigt.
                case HardwareAcceleration.Hardware64Bit: // 64 Bit Hardwarebeschleunigt.
                    crc = Sse42.Crc32(crc, data);
                    return;
            }
        }

        public unsafe void Calculate(byte* pBuffer, int count)
        {
            switch (Acceleration)
            {
                case HardwareAcceleration.Software: // Softwareimplementation.
                    for (int i = 0; i < count; ++i)
                        crc = (crc >> 8) ^ table![((crc) & 0xff) ^ *(pBuffer + i)];
                    return;
                case HardwareAcceleration.Hardware32Bit: // 32 Bit Hardwarebeschleunigt.
                    {
                        byte* pEnd = pBuffer + count - 3;

                        for (; pBuffer < pEnd; pBuffer += 4)
                            crc = Sse42.Crc32(crc, *(uint*)pBuffer);

                        pEnd += 3;

                        for (; pBuffer < pEnd; pBuffer++)
                            crc = Sse42.Crc32(crc, *pBuffer);
                    }
                    return;
                case HardwareAcceleration.Hardware64Bit: // 64 Bit Hardwarebeschleunigt.
                    {
                        byte* pEnd = pBuffer + count - 7;

                        for (; pBuffer < pEnd; pBuffer += 8)
                            crc = (uint)Sse42.X64.Crc32(crc, *(ulong*)pBuffer);

                        pEnd += 7;

                        for (; pBuffer < pEnd; pBuffer++)
                            crc = Sse42.Crc32(crc, *pBuffer);
                    }
                    return;
            }
        }

        public uint CRC => ~crc;
    }
}
