using System.Runtime.InteropServices;

namespace FMOD
{
    public struct DSP_PARAMETER_3DATTRIBUTES_MULTI
    {
        public int numlisteners;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public _3D_ATTRIBUTES[] relative;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public float[] weight;
        public _3D_ATTRIBUTES absolute;
    }
}
