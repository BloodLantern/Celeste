// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_STATE_FUNCTIONS
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
    public readonly struct DSP_STATE_FUNCTIONS
    {
        private readonly DSP_ALLOC_FUNC alloc;
        private readonly DSP_REALLOC_FUNC realloc;
        private readonly DSP_FREE_FUNC free;
        private readonly DSP_GETSAMPLERATE_FUNC getsamplerate;
        private readonly DSP_GETBLOCKSIZE_FUNC getblocksize;
        private readonly IntPtr dft;
        private readonly IntPtr pan;
        private readonly DSP_GETSPEAKERMODE_FUNC getspeakermode;
        private readonly DSP_GETCLOCK_FUNC getclock;
        private readonly DSP_GETLISTENERATTRIBUTES_FUNC getlistenerattributes;
        private readonly DSP_LOG_FUNC log;
        private readonly DSP_GETUSERDATA_FUNC getuserdata;
    }
}
