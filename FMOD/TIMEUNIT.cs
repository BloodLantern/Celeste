// Decompiled with JetBrains decompiler
// Type: FMOD.TIMEUNIT
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
  [Flags]
  public enum TIMEUNIT : uint
  {
    MS = 1,
    PCM = 2,
    PCMBYTES = 4,
    RAWBYTES = 8,
    PCMFRACTION = 16, // 0x00000010
    MODORDER = 256, // 0x00000100
    MODROW = 512, // 0x00000200
    MODPATTERN = 1024, // 0x00000400
  }
}
