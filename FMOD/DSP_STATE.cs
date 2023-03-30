// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_STATE
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
    public struct DSP_STATE
    {
        public IntPtr instance;
        public IntPtr plugindata;
        public uint channelmask;
        public int source_speakermode;
        public IntPtr sidechaindata;
        public int sidechainchannels;
        public IntPtr functions;
        public int systemobject;
    }
}
