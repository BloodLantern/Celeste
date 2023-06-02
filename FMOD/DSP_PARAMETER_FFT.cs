using System;
using System.Runtime.InteropServices;

namespace FMOD
{
    public struct DSP_PARAMETER_FFT
    {
        public int length;
        public int numchannels;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        private IntPtr[] spectrum_internal;

        public float[][] spectrum
        {
            get
            {
                float[][] spectrum = new float[this.numchannels][];
                for (int index = 0; index < this.numchannels; ++index)
                {
                    spectrum[index] = new float[this.length];
                    Marshal.Copy(this.spectrum_internal[index], spectrum[index], 0, this.length);
                }
                return spectrum;
            }
        }
    }
}
