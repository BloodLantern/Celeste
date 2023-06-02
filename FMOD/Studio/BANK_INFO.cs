using System;

namespace FMOD.Studio
{
    public struct BANK_INFO
    {
        public int size;
        public IntPtr userdata;
        public int userdatalength;
        public FILE_OPENCALLBACK opencallback;
        public FILE_CLOSECALLBACK closecallback;
        public FILE_READCALLBACK readcallback;
        public FILE_SEEKCALLBACK seekcallback;
    }
}
