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
