using System;

namespace FMOD
{
    [Flags]
    public enum TIMEUNIT : uint
    {
        MS = 1,
        PCM = 2,
        PCMBYTES = 4,
        RAWBYTES = 8,
        PCMFRACTION = 16, // 0x00000010
        MODORDER = 256, // 0x00000100
        MODROW = 512, // 0x00000200
        MODPATTERN = 1024, // 0x00000400
    }
}
