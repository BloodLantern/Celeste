// Decompiled with JetBrains decompiler
// Type: Celeste.AudioState
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Celeste
{
    [Serializable]
    public class AudioState
    {
        public static string[] LayerParameters = new string[10]
        {
            "layer0",
            "layer1",
            "layer2",
            "layer3",
            "layer4",
            "layer5",
            "layer6",
            "layer7",
            "layer8",
            "layer9"
        };
        public AudioTrackState Music = new();
        public AudioTrackState Ambience = new();

        public AudioState()
        {
        }

        public AudioState(AudioTrackState music, AudioTrackState ambience)
        {
            if (music != null)
            {
                Music = music.Clone();
            }

            if (ambience == null)
            {
                return;
            }

            Ambience = ambience.Clone();
        }

        public AudioState(string music, string ambience)
        {
            Music.Event = music;
            Ambience.Event = ambience;
        }

        public void Apply(bool forceSixteenthNoteHack = false)
        {
            bool flag1 = Audio.SetMusic(Music.Event, false);
            if (Audio.CurrentMusicEventInstance != null)
            {
                foreach (MEP parameter in Music.Parameters)
                {
                    if (!(parameter.Key == "sixteenth_note") || forceSixteenthNoteHack)
                    {
                        Audio.SetParameter(Audio.CurrentMusicEventInstance, parameter.Key, parameter.Value);
                    }
                }
                if (flag1)
                {
                    _ = (int)Audio.CurrentMusicEventInstance.start();
                }
            }
            bool flag2 = Audio.SetAmbience(Ambience.Event, false);
            if (!(Audio.CurrentAmbienceEventInstance != null))
            {
                return;
            }

            foreach (MEP parameter in Ambience.Parameters)
            {
                Audio.SetParameter(Audio.CurrentAmbienceEventInstance, parameter.Key, parameter.Value);
            }

            if (!flag2)
            {
                return;
            }

            _ = (int)Audio.CurrentAmbienceEventInstance.start();
        }

        public void Stop(bool allowFadeOut = true)
        {
            _ = Audio.SetMusic(null, false, allowFadeOut);
            _ = Audio.SetAmbience(null);
        }

        public AudioState Clone()
        {
            return new AudioState()
            {
                Music = Music.Clone(),
                Ambience = Ambience.Clone()
            };
        }
    }
}
