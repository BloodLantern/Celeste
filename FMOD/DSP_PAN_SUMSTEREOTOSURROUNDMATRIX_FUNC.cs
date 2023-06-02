using System;

namespace FMOD
{
    public delegate RESULT DSP_PAN_SUMSTEREOTOSURROUNDMATRIX_FUNC(
        ref DSP_STATE dsp_state,
        int targetSpeakerMode,
        float direction,
        float extent,
        float rotation,
        float lowFrequencyGain,
        float overallGain,
        int matrixHop,
        IntPtr matrix);
}
