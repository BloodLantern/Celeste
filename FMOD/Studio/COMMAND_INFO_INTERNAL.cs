using System;

namespace FMOD.Studio
{
    internal struct COMMAND_INFO_INTERNAL
    {
        public IntPtr commandname;
        public int parentcommandindex;
        public int framenumber;
        public float frametime;
        public INSTANCETYPE instancetype;
        public INSTANCETYPE outputtype;
        public uint instancehandle;
        public uint outputhandle;

        public COMMAND_INFO createPublic() => new COMMAND_INFO()
        {
            commandname = MarshallingHelper.stringFromNativeUtf8(this.commandname),
            parentcommandindex = this.parentcommandindex,
            framenumber = this.framenumber,
            frametime = this.frametime,
            instancetype = this.instancetype,
            outputtype = this.outputtype,
            instancehandle = this.instancehandle,
            outputhandle = this.outputhandle
        };
    }
}
