using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class SoundEmitter : Entity
    {
        public static SoundEmitter Play(string sfx)
        {
            SoundEmitter soundEmitter = new SoundEmitter(sfx);
            Engine.Scene.Add(soundEmitter);
            return soundEmitter;
        }

        public static SoundEmitter Play(string sfx, Entity follow, Vector2? offset = null)
        {
            SoundEmitter soundEmitter = new SoundEmitter(sfx, follow, offset.HasValue ? offset.Value : Vector2.Zero);
            Engine.Scene.Add(soundEmitter);
            return soundEmitter;
        }

        public SoundSource Source { get; private set; }

        private SoundEmitter(string sfx)
        {
            Add(Source = new SoundSource());
            Source.Play(sfx);
            Source.DisposeOnTransition = false;
            Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
            Add(new LevelEndingHook(OnLevelEnding));
        }

        private SoundEmitter(string sfx, Entity follow, Vector2 offset)
        {
            Add(Source = new SoundSource());
            Position = follow.Position + offset;
            Source.Play(sfx);
            Source.DisposeOnTransition = false;
            Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
            Add(new LevelEndingHook(OnLevelEnding));
        }

        public override void Update()
        {
            base.Update();
            if (Source.Playing)
                return;
            RemoveSelf();
        }

        private void OnLevelEnding() => Source.Stop();
    }
}
