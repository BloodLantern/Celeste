// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.COMMAND_INFO_INTERNAL
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD.Studio
{
    internal struct COMMAND_INFO_INTERNAL
    {
        public IntPtr commandname;
        public int parentcommandindex;
        public int framenumber;
        public float frametime;
        public INSTANCETYPE instancetype;
        public INSTANCETYPE outputtype;
        public uint instancehandle;
        public uint outputhandle;

        public COMMAND_INFO createPublic()
        {
            return new COMMAND_INFO()
            {
                commandname = MarshallingHelper.stringFromNativeUtf8(commandname),
                parentcommandindex = parentcommandindex,
                framenumber = framenumber,
                frametime = frametime,
                instancetype = instancetype,
                outputtype = outputtype,
                instancehandle = instancehandle,
                outputhandle = outputhandle
            };
        }
    }
}
