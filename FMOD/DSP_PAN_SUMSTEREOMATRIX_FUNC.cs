using System;

namespace FMOD
{
    public delegate RESULT DSP_PAN_SUMSTEREOMATRIX_FUNC(
        ref DSP_STATE dsp_state,
        int sourceSpeakerMode,
        float pan,
        float lowFrequencyGain,
        float overallGain,
        int matrixHop,
        IntPtr matrix);
}
