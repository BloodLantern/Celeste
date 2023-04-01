// Decompiled with JetBrains decompiler
// Type: Celeste.AudioTrackState
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class AudioTrackState
    {
        [XmlIgnore]
        private string ev;
        public List<MEP> Parameters = new();

        [XmlAttribute]
        public string Event
        {
            get => ev;
            set
            {
                if (!(ev != value))
                {
                    return;
                }

                ev = value;
                Parameters.Clear();
            }
        }

        [XmlIgnore]
        public int Progress
        {
            get => (int)GetParam("progress");
            set => Param("progress", value);
        }

        public AudioTrackState()
        {
        }

        public AudioTrackState(string ev)
        {
            Event = ev;
        }

        public AudioTrackState Layer(int index, float value)
        {
            return Param(AudioState.LayerParameters[index], value);
        }

        public AudioTrackState Layer(int index, bool value)
        {
            return Param(AudioState.LayerParameters[index], value);
        }

        public AudioTrackState SetProgress(int value)
        {
            Progress = value;
            return this;
        }

        public AudioTrackState Param(string key, float value)
        {
            foreach (MEP parameter in Parameters)
            {
                if (parameter.Key != null && parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    parameter.Value = value;
                    return this;
                }
            }
            Parameters.Add(new MEP(key, value));
            return this;
        }

        public AudioTrackState Param(string key, bool value)
        {
            return Param(key, value ? 1 : 0);
        }

        public float GetParam(string key)
        {
            foreach (MEP parameter in Parameters)
            {
                if (parameter.Key != null && parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    return parameter.Value;
                }
            }
            return 0;
        }

        public AudioTrackState Clone()
        {
            AudioTrackState audioTrackState = new()
            {
                Event = Event
            };
            foreach (MEP parameter in Parameters)
            {
                audioTrackState.Parameters.Add(new MEP(parameter.Key, parameter.Value));
            }

            return audioTrackState;
        }
    }
}
