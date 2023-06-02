namespace FMOD.Studio
{
    public struct COMMAND_INFO
    {
        public string commandname;
        public int parentcommandindex;
        public int framenumber;
        public float frametime;
        public INSTANCETYPE instancetype;
        public INSTANCETYPE outputtype;
        public uint instancehandle;
        public uint outputhandle;
    }
}
