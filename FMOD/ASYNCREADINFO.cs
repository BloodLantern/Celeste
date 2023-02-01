// Decompiled with JetBrains decompiler
// Type: FMOD.ASYNCREADINFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
  public struct ASYNCREADINFO
  {
    public IntPtr handle;
    public uint offset;
    public uint sizebytes;
    public int priority;
    public IntPtr userdata;
    public IntPtr buffer;
    public uint bytesread;
    public ASYNCREADINFO_DONE_CALLBACK done;
  }
}
