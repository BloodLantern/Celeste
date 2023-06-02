using System;

namespace FMOD
{
    public delegate RESULT DSP_DFT_FFTREAL_FUNC(
        ref DSP_STATE dsp_state,
        int size,
        IntPtr signal,
        IntPtr dft,
        IntPtr window,
        int signalhop);
}
