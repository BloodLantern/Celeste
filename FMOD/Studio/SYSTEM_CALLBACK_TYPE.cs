// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.SYSTEM_CALLBACK_TYPE
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
