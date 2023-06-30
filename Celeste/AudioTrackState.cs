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
                if (ev == value)
                    return;
                ev = value;
                Parameters.Clear();
            }
        }

        [XmlIgnore]
        public int Progress
        {
            get => (int) GetParam("progress");
            set => Param("progress", value);
        }

        public AudioTrackState()
        {
        }

        public AudioTrackState(string ev) => Event = ev;

        public AudioTrackState Layer(int index, float value) => Param(AudioState.LayerParameters[index], value);

        public AudioTrackState Layer(int index, bool value) => Param(AudioState.LayerParameters[index], value);

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

        public AudioTrackState Param(string key, bool value) => Param(key, value ? 1f : 0.0f);

        public float GetParam(string key)
        {
            foreach (MEP parameter in Parameters)
            {
                if (parameter.Key != null && parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return parameter.Value;
            }
            return 0.0f;
        }

        public AudioTrackState Clone()
        {
            AudioTrackState audioTrackState = new();
            audioTrackState.Event = Event;
            foreach (MEP parameter in Parameters)
                audioTrackState.Parameters.Add(new MEP(parameter.Key, parameter.Value));
            return audioTrackState;
        }
    }
}
