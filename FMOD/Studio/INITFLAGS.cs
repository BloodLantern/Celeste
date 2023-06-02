using System;

namespace FMOD.Studio
{
    [Flags]
    public enum INITFLAGS : uint
    {
        NORMAL = 0,
        LIVEUPDATE = 1,
        ALLOW_MISSING_PLUGINS = 2,
        SYNCHRONOUS_UPDATE = 4,
        DEFERRED_CALLBACKS = 8,
        LOAD_FROM_UPDATE = 16, // 0x00000010
    }
}
