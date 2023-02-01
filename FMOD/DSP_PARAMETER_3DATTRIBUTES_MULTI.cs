// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_PARAMETER_3DATTRIBUTES_MULTI
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Runtime.InteropServices;

namespace FMOD
{
  public struct DSP_PARAMETER_3DATTRIBUTES_MULTI
  {
    public int numlisteners;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public _3D_ATTRIBUTES[] relative;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public float[] weight;
    public _3D_ATTRIBUTES absolute;
  }
}
