using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    public class VCA : HandleBase
    {
        public RESULT getID(out Guid id) => VCA.FMOD_Studio_VCA_GetID(this.rawPtr, out id);

        public RESULT getPath(out string path)
        {
            path = (string) null;
            byte[] numArray = new byte[256];
            int retrieved = 0;
            RESULT path1 = VCA.FMOD_Studio_VCA_GetPath(this.rawPtr, numArray, numArray.Length, out retrieved);
            if (path1 == RESULT.ERR_TRUNCATED)
            {
                numArray = new byte[retrieved];
                path1 = VCA.FMOD_Studio_VCA_GetPath(this.rawPtr, numArray, numArray.Length, out retrieved);
            }
            if (path1 == RESULT.OK)
                path = Encoding.UTF8.GetString(numArray, 0, retrieved - 1);
            return path1;
        }

        public RESULT getVolume(out float volume, out float finalvolume) => VCA.FMOD_Studio_VCA_GetVolume(this.rawPtr, out volume, out finalvolume);

        public RESULT setVolume(float volume) => VCA.FMOD_Studio_VCA_SetVolume(this.rawPtr, volume);

        [DllImport("fmodstudio")]
        private static extern bool FMOD_Studio_VCA_IsValid(IntPtr vca);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_VCA_GetID(IntPtr vca, out Guid id);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_VCA_GetPath(
            IntPtr vca,
            [Out] byte[] path,
            int size,
            out int retrieved);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_VCA_GetVolume(
            IntPtr vca,
            out float volume,
            out float finalvolume);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_VCA_SetVolume(IntPtr vca, float value);

        public VCA(IntPtr raw)
            : base(raw)
        {
        }

        protected override bool isValidInternal() => VCA.FMOD_Studio_VCA_IsValid(this.rawPtr);
    }
}
