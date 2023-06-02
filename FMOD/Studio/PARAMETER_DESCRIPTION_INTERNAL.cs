using System;

namespace FMOD.Studio
{
    internal struct PARAMETER_DESCRIPTION_INTERNAL
    {
        public IntPtr name;
        public int index;
        public float minimum;
        public float maximum;
        public float defaultvalue;
        public PARAMETER_TYPE type;

        public void assign(out PARAMETER_DESCRIPTION publicDesc)
        {
            publicDesc.name = MarshallingHelper.stringFromNativeUtf8(this.name);
            publicDesc.index = this.index;
            publicDesc.minimum = this.minimum;
            publicDesc.maximum = this.maximum;
            publicDesc.defaultvalue = this.defaultvalue;
            publicDesc.type = this.type;
        }
    }
}
