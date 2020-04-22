using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpFast.Checksums;
using System;

namespace Tests
{
    [TestClass]
    public class CRCTests
    {
        [TestMethod]
        public void KnownValueSoftware()
        {
            byte[] source = System.Text.Encoding.ASCII.GetBytes("The world is a wonderful place to be. At least until climate change gets us.");

            CRC32C crc = new CRC32C();

            CRC32C.InitializeSoftwareTable();
            CRC32C.Acceleration = HardwareAcceleration.Software;

            crc.Calculate(source, 0, source.Length);

            Assert.AreEqual(crc.CRC, 0xC22C2C84U, "CRC doesn't match.");
        }

        [TestMethod]
        public void KnownValueHardwareX86()
        {
            byte[] source = System.Text.Encoding.ASCII.GetBytes("The world is a wonderful place to be. At least until climate change gets us.");

            CRC32C crc = new CRC32C();

            CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;

            crc.Calculate(source, 0, source.Length);

            Assert.AreEqual(crc.CRC, 0xC22C2C84U, "CRC doesn't match.");
        }

        [TestMethod]
        public void KnownValueHardwareX64()
        {
            byte[] source = System.Text.Encoding.ASCII.GetBytes("The world is a wonderful place to be. At least until climate change gets us.");

            CRC32C crc = new CRC32C();

            CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;

            crc.Calculate(source, 0, source.Length);

            Assert.AreEqual(crc.CRC, 0xC22C2C84U, "CRC doesn't match.");
        }


        [TestMethod]
        public void RunUpTestArray()
        {
            Random rng = new Random();

            byte[] source = new byte[65536];

            rng.NextBytes(source);

            uint masterCRC;

            CRC32C crcSoftware = new CRC32C();
            CRC32C crcHardwareX86;
            CRC32C crcHardwareX64;
            CRC32C crcMixed;

            CRC32C.InitializeSoftwareTable();

            CRC32C.Acceleration = HardwareAcceleration.Software;

            crcSoftware.Calculate(source, 0, 65536);

            masterCRC = crcSoftware.CRC;

            for (int stepSize = 1; stepSize < 25; stepSize++)
            {
                crcSoftware = new CRC32C();
                crcHardwareX86 = new CRC32C();
                crcHardwareX64 = new CRC32C();
                crcMixed = new CRC32C();

                int position;

                for (position = 0; position < 65536 - stepSize; position += stepSize)
                {
                    CRC32C.Acceleration = HardwareAcceleration.Software;
                    crcSoftware.Calculate(source, position, stepSize);

                    CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;
                    crcHardwareX86.Calculate(source, position, stepSize);

                    CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;
                    crcHardwareX64.Calculate(source, position, stepSize);

                    CRC32C.Acceleration = (HardwareAcceleration)rng.Next(1, 4);
                    crcMixed.Calculate(source, position, stepSize);

                    Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
                    Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
                    Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
                }

                if (position < 65536)
                {
                    CRC32C.Acceleration = HardwareAcceleration.Software;
                    crcSoftware.Calculate(source, position, 65536 - position);

                    CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;
                    crcHardwareX86.Calculate(source, position, 65536 - position);

                    CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;
                    crcHardwareX64.Calculate(source, position, 65536 - position);

                    CRC32C.Acceleration = (HardwareAcceleration)rng.Next(1, 4);
                    crcMixed.Calculate(source, position, 65536 - position);
                }

                Assert.AreEqual(crcSoftware.CRC, masterCRC, "Software doesn't match.");
                Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
                Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
                Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
            }
        }

        [TestMethod]
        public unsafe void RunUpTestPointer()
        {
            Random rng = new Random();

            byte[] source = new byte[65536];

            rng.NextBytes(source);

            uint masterCRC;

            CRC32C crcSoftware = new CRC32C();
            CRC32C crcHardwareX86;
            CRC32C crcHardwareX64;
            CRC32C crcMixed;

            CRC32C.InitializeSoftwareTable();

            CRC32C.Acceleration = HardwareAcceleration.Software;

            crcSoftware.Calculate(source, 0, 65536);

            masterCRC = crcSoftware.CRC;

            fixed (byte* bpSource = source)
                for (int stepSize = 1; stepSize < 25; stepSize++)
                {
                    crcSoftware = new CRC32C();
                    crcHardwareX86 = new CRC32C();
                    crcHardwareX64 = new CRC32C();
                    crcMixed = new CRC32C();

                    int position;

                    for (position = 0; position < 65536 - stepSize; position += stepSize)
                    {
                        CRC32C.Acceleration = HardwareAcceleration.Software;
                        crcSoftware.Calculate(bpSource + position, stepSize);

                        CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;
                        crcHardwareX86.Calculate(bpSource + position, stepSize);

                        CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;
                        crcHardwareX64.Calculate(bpSource + position, stepSize);

                        CRC32C.Acceleration = (HardwareAcceleration)rng.Next(1, 4);
                        crcMixed.Calculate(bpSource + position, stepSize);

                        Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
                        Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
                        Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
                    }

                    if (position < 65536)
                    {
                        CRC32C.Acceleration = HardwareAcceleration.Software;
                        crcSoftware.Calculate(bpSource + position, 65536 - position);

                        CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;
                        crcHardwareX86.Calculate(bpSource + position, 65536 - position);

                        CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;
                        crcHardwareX64.Calculate(bpSource + position, 65536 - position);

                        CRC32C.Acceleration = (HardwareAcceleration)rng.Next(1, 4);
                        crcMixed.Calculate(bpSource + position, 65536 - position);
                    }

                    Assert.AreEqual(crcSoftware.CRC, masterCRC, "Software doesn't match.");
                    Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
                    Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
                    Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
                }
        }

        [TestMethod]
        public unsafe void RunUpTestBytes()
        {
            Random rng = new Random();

            byte[] source = new byte[65536];

            rng.NextBytes(source);

            uint masterCRC;

            CRC32C crcSoftware = new CRC32C();
            CRC32C crcHardwareX86 = new CRC32C();
            CRC32C crcHardwareX64 = new CRC32C();
            CRC32C crcMixed = new CRC32C();

            CRC32C.InitializeSoftwareTable();

            CRC32C.Acceleration = HardwareAcceleration.Software;

            crcSoftware.Calculate(source, 0, 65536);

            masterCRC = crcSoftware.CRC;

            crcSoftware = new CRC32C();

            for (int position = 0; position < 65536; position++)
            {
                CRC32C.Acceleration = HardwareAcceleration.Software;
                crcSoftware.Calculate(source[position]);

                CRC32C.Acceleration = HardwareAcceleration.Hardware32Bit;
                crcHardwareX86.Calculate(source[position]);

                CRC32C.Acceleration = HardwareAcceleration.Hardware64Bit;
                crcHardwareX64.Calculate(source[position]);

                CRC32C.Acceleration = (HardwareAcceleration)rng.Next(1, 4);
                crcMixed.Calculate(source[position]);

                Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
                Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
                Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
            }

            Assert.AreEqual(crcSoftware.CRC, masterCRC, "Software doesn't match.");
            Assert.AreEqual(crcSoftware.CRC, crcHardwareX86.CRC, "HardwareX86 doesn't match.");
            Assert.AreEqual(crcSoftware.CRC, crcHardwareX64.CRC, "HardwareX64 doesn't match.");
            Assert.AreEqual(crcSoftware.CRC, crcMixed.CRC, "Mixed doesn't match.");
        }
    }
}
