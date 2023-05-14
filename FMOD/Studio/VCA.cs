// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.VCA
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    public class VCA : HandleBase
    {
        public RESULT getID(out Guid id)
        {
            return VCA.FMOD_Studio_VCA_GetID(rawPtr, out id);
        }

        public RESULT getPath(out string path)
        {
            path = null;
            byte[] numArray = new byte[256];
            RESULT path1 = VCA.FMOD_Studio_VCA_GetPath(rawPtr, numArray, numArray.Length, out int retrieved);
            if (path1 == RESULT.ERR_TRUNCATED)
            {
                numArray = new byte[retrieved];
                path1 = VCA.FMOD_Studio_VCA_GetPath(rawPtr, numArray, numArray.Length, out retrieved);
            }
            if (path1 == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(numArray, 0, retrieved - 1);
            }

            return path1;
        }

        public RESULT getVolume(out float volume, out float finalvolume)
        {
            return VCA.FMOD_Studio_VCA_GetVolume(rawPtr, out volume, out finalvolume);
        }

        public RESULT setVolume(float volume)
        {
            return VCA.FMOD_Studio_VCA_SetVolume(rawPtr, volume);
        }

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

        protected override bool isValidInternal()
        {
            return VCA.FMOD_Studio_VCA_IsValid(rawPtr);
        }
    }
}
