// Decompiled with JetBrains decompiler
// Type: FMOD.ERRORCALLBACK_INFO
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD
{
    public struct ERRORCALLBACK_INFO
    {
        public RESULT result;
        public ERRORCALLBACK_INSTANCETYPE instancetype;
        public IntPtr instance;
        private IntPtr functionname_internal;
        private IntPtr functionparams_internal;

        public string functionname => Marshal.PtrToStringAnsi(this.functionname_internal);

        public string functionparams => Marshal.PtrToStringAnsi(this.functionparams_internal);
    }
}
