﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public abstract class CutsceneEntity : Entity
    {
        public bool WasSkipped;
        public bool RemoveOnSkipped = true;
        public bool EndingChapterAfter;
        public Level Level;

        public bool Running { get; private set; }

        public bool FadeInOnSkip { get; private set; }

        public CutsceneEntity(bool fadeInOnSkip = true, bool endingChapterAfter = false)
        {
            FadeInOnSkip = fadeInOnSkip;
            EndingChapterAfter = endingChapterAfter;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = scene as Level;
            Start();
        }

        public void Start()
        {
            Running = true;
            Level.StartCutscene(SkipCutscene, FadeInOnSkip, EndingChapterAfter);
            OnBegin(Level);
        }

        public override void Update()
        {
            if (Level.RetryPlayerCorpse != null)
                Active = false;
            else
                base.Update();
        }

        private void SkipCutscene(Level level)
        {
            WasSkipped = true;
            EndCutscene(level, RemoveOnSkipped);
        }

        public void EndCutscene(Level level, bool removeSelf = true)
        {
            Running = false;
            OnEnd(level);
            level.EndCutscene();
            if (!removeSelf)
                return;
            RemoveSelf();
        }

        public abstract void OnBegin(Level level);

        public abstract void OnEnd(Level level);

        public static IEnumerator CameraTo(
            Vector2 target,
            float duration,
            Ease.Easer ease = null,
            float delay = 0.0f)
        {
            if (ease == null)
                ease = Ease.CubeInOut;
            if (delay > 0.0)
                yield return delay;
            Level level = Engine.Scene as Level;
            Vector2 from = level.Camera.Position;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / duration)
            {
                level.Camera.Position = from + (target - from) * ease(p);
                yield return null;
            }
            level.Camera.Position = target;
        }
    }
}
