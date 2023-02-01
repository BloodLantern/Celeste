// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.Union_IntBoolFloatString
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct Union_IntBoolFloatString
  {
    [FieldOffset(0)]
    public int intvalue;
    [FieldOffset(0)]
    public bool boolvalue;
    [FieldOffset(0)]
    public float floatvalue;
    [FieldOffset(0)]
    public IntPtr stringvalue;
  }
}
