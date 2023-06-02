using System;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct Union_IntBoolFloatString
    {
        [FieldOffset(0)]
        public int intvalue;
        [FieldOffset(0)]
        public bool boolvalue;
        [FieldOffset(0)]
        public float floatvalue;
        [FieldOffset(0)]
        public IntPtr stringvalue;
    }
}
