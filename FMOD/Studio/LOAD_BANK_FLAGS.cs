﻿// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.LOAD_BANK_FLAGS
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
