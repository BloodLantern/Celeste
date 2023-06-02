using System;

namespace FMOD.Studio
{
    [Flags]
    public enum SYSTEM_CALLBACK_TYPE : uint
    {
        PREUPDATE = 1,
        POSTUPDATE = 2,
        BANK_UNLOAD = 4,
        ALL = 4294967295, // 0xFFFFFFFF
    }
}
