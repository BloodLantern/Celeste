using System;

namespace FMOD
{
    public delegate RESULT DSP_GETLISTENERATTRIBUTES_FUNC(
        ref DSP_STATE dsp_state,
        ref int numlisteners,
        IntPtr attributes);
}
