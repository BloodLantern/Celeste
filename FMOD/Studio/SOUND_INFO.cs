// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.SOUND_INFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    public class SOUND_INFO
    {
        public byte[] name_or_data;
        public MODE mode;
        public CREATESOUNDEXINFO exinfo;
        public int subsoundindex;

        public string name
        {
            get
            {
                if ((mode & (MODE.OPENMEMORY | MODE.OPENMEMORY_POINT)) != MODE.DEFAULT || name_or_data == null)
                {
                    return null;
                }

                int count = Array.IndexOf<byte>(name_or_data, 0);
                return count > 0 ? Encoding.UTF8.GetString(name_or_data, 0, count) : null;
            }
        }

        ~SOUND_INFO()
        {
            if (!(exinfo.inclusionlist != IntPtr.Zero))
            {
                return;
            }

            Marshal.FreeHGlobal(exinfo.inclusionlist);
        }
    }
}
