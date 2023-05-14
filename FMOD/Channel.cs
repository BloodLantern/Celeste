// Decompiled with JetBrains decompiler
// Type: FMOD.Channel
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD
{
    public class Channel : ChannelControl
    {
        public RESULT setFrequency(float frequency)
        {
            return Channel.FMOD_Channel_SetFrequency(getRaw(), frequency);
        }

        public RESULT getFrequency(out float frequency)
        {
            return Channel.FMOD_Channel_GetFrequency(getRaw(), out frequency);
        }

        public RESULT setPriority(int priority)
        {
            return Channel.FMOD_Channel_SetPriority(getRaw(), priority);
        }

        public RESULT getPriority(out int priority)
        {
            return Channel.FMOD_Channel_GetPriority(getRaw(), out priority);
        }

        public RESULT setPosition(uint position, TIMEUNIT postype)
        {
            return Channel.FMOD_Channel_SetPosition(getRaw(), position, postype);
        }

        public RESULT getPosition(out uint position, TIMEUNIT postype)
        {
            return Channel.FMOD_Channel_GetPosition(getRaw(), out position, postype);
        }

        public RESULT setChannelGroup(ChannelGroup channelgroup)
        {
            return Channel.FMOD_Channel_SetChannelGroup(getRaw(), channelgroup.getRaw());
        }

        public RESULT getChannelGroup(out ChannelGroup channelgroup)
        {
            int channelGroup = (int)Channel.FMOD_Channel_GetChannelGroup(getRaw(), out IntPtr channelgroup1);
            channelgroup = new ChannelGroup(channelgroup1);
            return (RESULT)channelGroup;
        }

        public RESULT setLoopCount(int loopcount)
        {
            return Channel.FMOD_Channel_SetLoopCount(getRaw(), loopcount);
        }

        public RESULT getLoopCount(out int loopcount)
        {
            return Channel.FMOD_Channel_GetLoopCount(getRaw(), out loopcount);
        }

        public RESULT setLoopPoints(
            uint loopstart,
            TIMEUNIT loopstarttype,
            uint loopend,
            TIMEUNIT loopendtype)
        {
            return Channel.FMOD_Channel_SetLoopPoints(getRaw(), loopstart, loopstarttype, loopend, loopendtype);
        }

        public RESULT getLoopPoints(
            out uint loopstart,
            TIMEUNIT loopstarttype,
            out uint loopend,
            TIMEUNIT loopendtype)
        {
            return Channel.FMOD_Channel_GetLoopPoints(getRaw(), out loopstart, loopstarttype, out loopend, loopendtype);
        }

        public RESULT isVirtual(out bool isvirtual)
        {
            return Channel.FMOD_Channel_IsVirtual(getRaw(), out isvirtual);
        }

        public RESULT getCurrentSound(out Sound sound)
        {
            int currentSound = (int)Channel.FMOD_Channel_GetCurrentSound(getRaw(), out IntPtr sound1);
            sound = new Sound(sound1);
            return (RESULT)currentSound;
        }

        public RESULT getIndex(out int index)
        {
            return Channel.FMOD_Channel_GetIndex(getRaw(), out index);
        }

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetFrequency(IntPtr channel, float frequency);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetFrequency(
            IntPtr channel,
            out float frequency);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetPriority(IntPtr channel, int priority);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetPriority(IntPtr channel, out int priority);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetChannelGroup(
            IntPtr channel,
            IntPtr channelgroup);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetChannelGroup(
            IntPtr channel,
            out IntPtr channelgroup);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_IsVirtual(IntPtr channel, out bool isvirtual);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetCurrentSound(
            IntPtr channel,
            out IntPtr sound);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetIndex(IntPtr channel, out int index);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetPosition(
            IntPtr channel,
            uint position,
            TIMEUNIT postype);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetPosition(
            IntPtr channel,
            out uint position,
            TIMEUNIT postype);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetMode(IntPtr channel, MODE mode);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetMode(IntPtr channel, out MODE mode);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetLoopCount(IntPtr channel, int loopcount);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetLoopCount(IntPtr channel, out int loopcount);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetLoopPoints(
            IntPtr channel,
            uint loopstart,
            TIMEUNIT loopstarttype,
            uint loopend,
            TIMEUNIT loopendtype);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetLoopPoints(
            IntPtr channel,
            out uint loopstart,
            TIMEUNIT loopstarttype,
            out uint loopend,
            TIMEUNIT loopendtype);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_SetUserData(IntPtr channel, IntPtr userdata);

        [DllImport("fmod")]
        private static extern RESULT FMOD_Channel_GetUserData(IntPtr channel, out IntPtr userdata);

        public Channel(IntPtr raw)
            : base(raw)
        {
        }
    }
}
