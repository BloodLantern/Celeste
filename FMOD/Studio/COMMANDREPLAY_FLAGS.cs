// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.COMMANDREPLAY_FLAGS
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
