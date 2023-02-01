// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_METERING_INFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Runtime.InteropServices;

namespace FMOD
{
  [StructLayout(LayoutKind.Sequential)]
  public class DSP_METERING_INFO
  {
    public int numsamples;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public float[] peaklevel;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public float[] rmslevel;
    public short numchannels;
  }
}
