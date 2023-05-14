// Decompiled with JetBrains decompiler
// Type: FMOD.DSPConnection
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD
{
    public class DSPConnection : HandleBase
    {
        public RESULT getInput(out DSP input)
        {
            int input2 = (int)DSPConnection.FMOD_DSPConnection_GetInput(rawPtr, out IntPtr input1);
            input = new DSP(input1);
            return (RESULT)input2;
        }

        public RESULT getOutput(out DSP output)
        {
            int output2 = (int)DSPConnection.FMOD_DSPConnection_GetOutput(rawPtr, out IntPtr output1);
            output = new DSP(output1);
            return (RESULT)output2;
        }

        public RESULT setMix(float volume)
        {
            return DSPConnection.FMOD_DSPConnection_SetMix(rawPtr, volume);
        }

        public RESULT getMix(out float volume)
        {
            return DSPConnection.FMOD_DSPConnection_GetMix(rawPtr, out volume);
        }

        public RESULT setMixMatrix(
            float[] matrix,
            int outchannels,
            int inchannels,
            int inchannel_hop = 0)
        {
            return DSPConnection.FMOD_DSPConnection_SetMixMatrix(rawPtr, matrix, outchannels, inchannels, inchannel_hop);
        }

        public RESULT getMixMatrix(
            float[] matrix,
            out int outchannels,
            out int inchannels,
            int inchannel_hop = 0)
        {
            return DSPConnection.FMOD_DSPConnection_GetMixMatrix(rawPtr, matrix, out outchannels, out inchannels, inchannel_hop);
        }

        public RESULT getType(out DSPCONNECTION_TYPE type)
        {
            return DSPConnection.FMOD_DSPConnection_GetType(rawPtr, out type);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return DSPConnection.FMOD_DSPConnection_SetUserData(rawPtr, userdata);
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return DSPConnection.FMOD_DSPConnection_GetUserData(rawPtr, out userdata);
        }

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetInput(
            IntPtr dspconnection,
            out IntPtr input);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetOutput(
            IntPtr dspconnection,
            out IntPtr output);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_SetMix(IntPtr dspconnection, float volume);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetMix(
            IntPtr dspconnection,
            out float volume);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_SetMixMatrix(
            IntPtr dspconnection,
            float[] matrix,
            int outchannels,
            int inchannels,
            int inchannel_hop);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetMixMatrix(
            IntPtr dspconnection,
            float[] matrix,
            out int outchannels,
            out int inchannels,
            int inchannel_hop);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetType(
            IntPtr dspconnection,
            out DSPCONNECTION_TYPE type);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_SetUserData(
            IntPtr dspconnection,
            IntPtr userdata);

        [DllImport("fmod")]
        private static extern RESULT FMOD_DSPConnection_GetUserData(
            IntPtr dspconnection,
            out IntPtr userdata);

        public DSPConnection(IntPtr raw)
            : base(raw)
        {
        }
    }
}
