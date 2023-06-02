namespace FMOD
{
    public delegate RESULT DSP_PAN_GETROLLOFFGAIN_FUNC(
        ref DSP_STATE dsp_state,
        DSP_PAN_3D_ROLLOFF_TYPE rolloff,
        float distance,
        float mindistance,
        float maxdistance,
        out float gain);
}
