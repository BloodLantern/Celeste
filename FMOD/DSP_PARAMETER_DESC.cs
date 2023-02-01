// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_PARAMETER_DESC
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Runtime.InteropServices;

namespace FMOD
{
  public struct DSP_PARAMETER_DESC
  {
    public DSP_PARAMETER_TYPE type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] name;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] label;
    public string description;
    public DSP_PARAMETER_DESC_UNION desc;
  }
}
