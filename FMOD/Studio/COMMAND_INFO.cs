// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.COMMAND_INFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace FMOD.Studio
{
    public struct COMMAND_INFO
    {
        public string commandname;
        public int parentcommandindex;
        public int framenumber;
        public float frametime;
        public INSTANCETYPE instancetype;
        public INSTANCETYPE outputtype;
        public uint instancehandle;
        public uint outputhandle;
    }
}
