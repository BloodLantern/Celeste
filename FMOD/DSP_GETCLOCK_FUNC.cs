namespace FMOD
{
    public delegate RESULT DSP_GETCLOCK_FUNC(
        ref DSP_STATE dsp_state,
        out ulong clock,
        out uint offset,
        out uint length);
}
