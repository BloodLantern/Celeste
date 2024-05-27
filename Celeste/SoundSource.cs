using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class SoundSource : Component
    {
        public string EventName;
        public Vector2 Position = Vector2.Zero;
        public bool DisposeOnTransition = true;
        public bool RemoveOnOneshotEnd;
        private EventInstance instance;
        private bool is3D;
        private bool isOneshot;

        public bool Playing { get; private set; }

        public bool Is3D => is3D;

        public bool IsOneshot => isOneshot;

        public bool InstancePlaying
        {
            get
            {
                if (instance != null)
                {
                    PLAYBACK_STATE state;
                    int playbackState = (int) instance.getPlaybackState(out state);
                    if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING)
                        return true;
                }
                return false;
            }
        }

        public SoundSource()
            : base(true, false)
        {
        }

        public SoundSource(string path)
            : this()
        {
            Play(path);
        }

        public SoundSource(Vector2 offset, string path)
            : this()
        {
            Position = offset;
            Play(path);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            UpdateSfxPosition();
        }

        public SoundSource Play(string path, string param = null, float value = 0.0f)
        {
            Stop();
            EventName = path;
            EventDescription eventDescription = Audio.GetEventDescription(path);
            if (eventDescription != null)
            {
                int instance = (int) eventDescription.createInstance(out this.instance);
                int num1 = (int) eventDescription.is3D(out is3D);
                int num2 = (int) eventDescription.isOneshot(out isOneshot);
            }
            if (this.instance != null)
            {
                if (is3D)
                {
                    Vector2 position = Position;
                    if (Entity != null)
                        position += Entity.Position;
                    Audio.Position(instance, position);
                }
                if (param != null)
                {
                    int num3 = (int) instance.setParameterValue(param, value);
                }
                int num4 = (int) instance.start();
                Playing = true;
            }
            return this;
        }

        public SoundSource Param(string param, float value)
        {
            if (instance != null)
            {
                int num = (int) instance.setParameterValue(param, value);
            }
            return this;
        }

        public SoundSource Pause()
        {
            if (instance != null)
            {
                int num = (int) instance.setPaused(true);
            }
            Playing = false;
            return this;
        }

        public SoundSource Resume()
        {
            if (instance != null)
            {
                bool paused1;
                int paused2 = (int) instance.getPaused(out paused1);
                if (paused1)
                {
                    int num = (int) instance.setPaused(false);
                    Playing = true;
                }
            }
            return this;
        }

        public SoundSource Stop(bool allowFadeout = true)
        {
            Audio.Stop(instance, allowFadeout);
            instance = null;
            Playing = false;
            return this;
        }

        public void UpdateSfxPosition()
        {
            if (!is3D || !(instance != null))
                return;
            Vector2 position = Position;
            if (Entity != null)
                position += Entity.Position;
            Audio.Position(instance, position);
        }

        public override void Update()
        {
            UpdateSfxPosition();
            if (!isOneshot || !(instance != null))
                return;
            PLAYBACK_STATE state;
            int playbackState = (int) instance.getPlaybackState(out state);
            if (state != PLAYBACK_STATE.STOPPED)
                return;
            int num = (int) instance.release();
            instance = null;
            Playing = false;
            if (!RemoveOnOneshotEnd)
                return;
            RemoveSelf();
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            Stop();
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Stop();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Stop(false);
        }

        public override void DebugRender(Camera camera)
        {
            Vector2 position = Position;
            if (Entity != null)
                position += Entity.Position;
            if (instance != null && Playing)
                Draw.Circle(position, (float) (4.0 + Scene.RawTimeActive * 2.0 % 1.0 * 16.0), Color.BlueViolet, 16);
            Draw.HollowRect(position.X - 2f, position.Y - 2f, 4f, 4f, Color.BlueViolet);
        }
    }
}
