// Decompiled with JetBrains decompiler
// Type: FMOD.MEMORY_TYPE
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
    [Flags]
    public enum MEMORY_TYPE : uint
    {
        NORMAL = 0,
        STREAM_FILE = 1,
        STREAM_DECODE = 2,
        SAMPLEDATA = 4,
        DSP_BUFFER = 8,
        PLUGIN = 16, // 0x00000010
        XBOX360_PHYSICAL = 1048576, // 0x00100000
        PERSISTENT = 2097152, // 0x00200000
        SECONDARY = 4194304, // 0x00400000
        ALL = 4294967295, // 0xFFFFFFFF
    }
}
