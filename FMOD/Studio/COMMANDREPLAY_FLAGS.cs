using System;

namespace FMOD.Studio
{
    [Flags]
    public enum COMMANDREPLAY_FLAGS : uint
    {
        NORMAL = 0,
        SKIP_CLEANUP = 1,
        FAST_FORWARD = 2,
        SKIP_BANK_LOAD = 4,
    }
}
