namespace FMOD
{
    public delegate void DSP_LOG_FUNC(
        DEBUG_FLAGS level,
        StringWrapper file,
        int line,
        StringWrapper function,
        StringWrapper format);
}
