using System;
using System.Collections.Generic;
using System.Text;

namespace SharpFast.Checksums
{
    public enum HardwareAcceleration
    {
        Undefined = 0x00,
        Software,
        Hardware32Bit,
        Hardware64Bit
    }
}
