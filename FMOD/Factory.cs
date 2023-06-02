using System;
using System.Runtime.InteropServices;

namespace FMOD
{
    public class Factory
    {
        public static RESULT System_Create(out FMOD.System system)
        {
            system = (FMOD.System) null;
            IntPtr system1 = new IntPtr();
            RESULT result = Factory.FMOD_System_Create(out system1);
            if (result != RESULT.OK)
                return result;
            system = new FMOD.System(system1);
            return result;
        }

        [DllImport("fmod")]
        private static extern RESULT FMOD_System_Create(out IntPtr system);
    }
}
