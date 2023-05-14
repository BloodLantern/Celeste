// Decompiled with JetBrains decompiler
// Type: Celeste.SoundSource
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
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
                    _ = (int)instance.getPlaybackState(out PLAYBACK_STATE state);
                    if (state is PLAYBACK_STATE.PLAYING or PLAYBACK_STATE.STARTING or PLAYBACK_STATE.SUSTAINING)
                    {
                        return true;
                    }
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
            _ = Play(path);
        }

        public SoundSource(Vector2 offset, string path)
            : this()
        {
            Position = offset;
            _ = Play(path);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            UpdateSfxPosition();
        }

        public SoundSource Play(string path, string param = null, float value = 0.0f)
        {
            _ = Stop();
            EventName = path;
            EventDescription eventDescription = Audio.GetEventDescription(path);
            if (eventDescription != null)
            {
                _ = (int)eventDescription.createInstance(out instance);
                _ = (int)eventDescription.is3D(out is3D);
                _ = (int)eventDescription.isOneshot(out isOneshot);
            }
            if (instance != null)
            {
                if (is3D)
                {
                    Vector2 position = Position;
                    if (Entity != null)
                    {
                        position += Entity.Position;
                    }

                    Audio.Position(instance, position);
                }
                if (param != null)
                {
                    _ = (int)instance.setParameterValue(param, value);
                }

                _ = (int)instance.start();
                Playing = true;
            }
            return this;
        }

        public SoundSource Param(string param, float value)
        {
            if (instance != null)
            {
                _ = (int)instance.setParameterValue(param, value);
            }
            return this;
        }

        public SoundSource Pause()
        {
            if (instance != null)
            {
                _ = (int)instance.setPaused(true);
            }
            Playing = false;
            return this;
        }

        public SoundSource Resume()
        {
            if (instance != null)
            {
                _ = (int)instance.getPaused(out bool paused1);
                if (paused1)
                {
                    _ = (int)instance.setPaused(false);
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
            {
                return;
            }

            Vector2 position = Position;
            if (Entity != null)
            {
                position += Entity.Position;
            }

            Audio.Position(instance, position);
        }

        public override void Update()
        {
            UpdateSfxPosition();
            if (!isOneshot || !(instance != null))
            {
                return;
            }

            _ = (int)instance.getPlaybackState(out PLAYBACK_STATE state);
            if (state != PLAYBACK_STATE.STOPPED)
            {
                return;
            }

            _ = (int)instance.release();
            instance = null;
            Playing = false;
            if (!RemoveOnOneshotEnd)
            {
                return;
            }

            RemoveSelf();
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            _ = Stop();
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            _ = Stop();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            _ = Stop(false);
        }

        public override void DebugRender(Camera camera)
        {
            Vector2 position = Position;
            if (Entity != null)
            {
                position += Entity.Position;
            }

            if (instance != null && Playing)
            {
                Draw.Circle(position, (float)(4.0 + (Scene.RawTimeActive * 2.0 % 1.0 * 16.0)), Color.BlueViolet, 16);
            }

            Draw.HollowRect(position.X - 2f, position.Y - 2f, 4f, 4f, Color.BlueViolet);
        }
    }
}
