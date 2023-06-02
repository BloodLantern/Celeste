using System;

namespace FMOD.Studio
{
    [Flags]
    public enum COMMANDCAPTURE_FLAGS : uint
    {
        NORMAL = 0,
        FILEFLUSH = 1,
        SKIP_INITIAL_STATE = 2,
    }
}
