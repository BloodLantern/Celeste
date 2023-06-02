using System;

namespace FMOD
{
    public delegate RESULT DSP_PAN_SUMMONOMATRIX_FUNC(
        ref DSP_STATE dsp_state,
        int sourceSpeakerMode,
        float lowFrequencyGain,
        float overallGain,
        IntPtr matrix);
}
