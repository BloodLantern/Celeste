// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.SOUND_INFO_INTERNAL
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
    public struct SOUND_INFO_INTERNAL
    {
        private readonly IntPtr name_or_data;
        private readonly MODE mode;
        private CREATESOUNDEXINFO exinfo;
        private readonly int subsoundindex;

        public void assign(out SOUND_INFO publicInfo)
        {
            publicInfo = new SOUND_INFO
            {
                mode = mode,
                exinfo = exinfo
            };
            publicInfo.exinfo.inclusionlist = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            Marshal.WriteInt32(publicInfo.exinfo.inclusionlist, subsoundindex);
            publicInfo.exinfo.inclusionlistnum = 1;
            publicInfo.subsoundindex = subsoundindex;
            if (name_or_data != IntPtr.Zero)
            {
                int num;
                int length;
                if ((mode & (MODE.OPENMEMORY | MODE.OPENMEMORY_POINT)) != MODE.DEFAULT)
                {
                    publicInfo.mode = (publicInfo.mode & ~MODE.OPENMEMORY_POINT) | MODE.OPENMEMORY;
                    num = (int)exinfo.fileoffset;
                    publicInfo.exinfo.fileoffset = 0U;
                    length = (int)exinfo.length;
                }
                else
                {
                    num = 0;
                    length = MarshallingHelper.stringLengthUtf8(name_or_data) + 1;
                }
                publicInfo.name_or_data = new byte[length];
                Marshal.Copy(new IntPtr(name_or_data.ToInt64() + num), publicInfo.name_or_data, 0, length);
            }
            else
            {
                publicInfo.name_or_data = null;
            }
        }
    }
}
