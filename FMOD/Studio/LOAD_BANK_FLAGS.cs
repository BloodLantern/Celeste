using System;

namespace FMOD.Studio
{
    [Flags]
    public enum LOAD_BANK_FLAGS : uint
    {
        NORMAL = 0,
        NONBLOCKING = 1,
        DECOMPRESS_SAMPLES = 2,
    }
}
