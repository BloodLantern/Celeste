using System;

namespace FMOD
{
    public delegate IntPtr DSP_REALLOC_FUNC(
        IntPtr ptr,
        uint size,
        MEMORY_TYPE type,
        StringWrapper sourcestr);
}
