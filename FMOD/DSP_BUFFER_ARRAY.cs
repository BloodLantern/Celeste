// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_BUFFER_ARRAY
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
  public struct DSP_BUFFER_ARRAY
  {
    public int numbuffers;
    public int[] buffernumchannels;
    public CHANNELMASK[] bufferchannelmask;
    public IntPtr[] buffers;
    public SPEAKERMODE speakermode;
  }
}
