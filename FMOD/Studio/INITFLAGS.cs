// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.INITFLAGS
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
