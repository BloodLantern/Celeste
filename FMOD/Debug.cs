// Decompiled with JetBrains decompiler
// Type: FMOD.Debug
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Runtime.InteropServices;

namespace FMOD
{
    public class Debug
    {
        public static RESULT Initialize(
            DEBUG_FLAGS flags,
            DEBUG_MODE mode = DEBUG_MODE.TTY,
            DEBUG_CALLBACK callback = null,
            string filename = null)
        {
            return Debug.FMOD_Debug_Initialize(flags, mode, callback, filename);
        }

        [DllImport("fmod")]
        private static extern RESULT FMOD_Debug_Initialize(
            DEBUG_FLAGS flags,
            DEBUG_MODE mode,
            DEBUG_CALLBACK callback,
            string filename);
    }
}
