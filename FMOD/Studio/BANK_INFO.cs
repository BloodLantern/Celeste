// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.BANK_INFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD.Studio
{
    public struct BANK_INFO
    {
        public int size;
        public IntPtr userdata;
        public int userdatalength;
        public FILE_OPENCALLBACK opencallback;
        public FILE_CLOSECALLBACK closecallback;
        public FILE_READCALLBACK readcallback;
        public FILE_SEEKCALLBACK seekcallback;
    }
}
