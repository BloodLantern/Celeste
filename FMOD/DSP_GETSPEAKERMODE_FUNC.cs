namespace FMOD
{
    public delegate RESULT DSP_GETSPEAKERMODE_FUNC(
        ref DSP_STATE dsp_state,
        ref int speakermode_mixer,
        ref int speakermode_output);
}
