using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class Tween : Component
    {
        public Ease.Easer Easer;
        public Action<Tween> OnUpdate;
        public Action<Tween> OnComplete;
        public Action<Tween> OnStart;
        public bool UseRawDeltaTime;
        private bool startedReversed;
        private ulong cachedFrame;
        private static readonly List<Tween> cached = new();

        public Tween.TweenMode Mode { get; private set; }

        public float Duration { get; private set; }

        public float TimeLeft { get; private set; }

        public float Percent { get; private set; }

        public float Eased { get; private set; }

        public bool Reverse { get; private set; }

        public static Tween Create(
            Tween.TweenMode mode,
            Ease.Easer easer = null,
            float duration = 1f,
            bool start = false)
        {
            Tween tween1 = null;
            foreach (Tween tween2 in Tween.cached)
            {
                if (Engine.FrameCounter > tween2.cachedFrame + 3UL)
                {
                    tween1 = tween2;
                    Tween.cached.Remove(tween2);
                    break;
                }
            }
            if (tween1 == null)
                tween1 = new Tween();
            tween1.OnUpdate = tween1.OnComplete = tween1.OnStart = null;
            tween1.Init(mode, easer, duration, start);
            return tween1;
        }

        public static Tween Set(
            Entity entity,
            Tween.TweenMode tweenMode,
            float duration,
            Ease.Easer easer,
            Action<Tween> onUpdate,
            Action<Tween> onComplete = null)
        {
            Tween tween = Tween.Create(tweenMode, easer, duration, true);
            tween.OnUpdate += onUpdate;
            tween.OnComplete += onComplete;
            entity.Add(tween);
            return tween;
        }

        public static Tween Position(
            Entity entity,
            Vector2 targetPosition,
            float duration,
            Ease.Easer easer,
            Tween.TweenMode tweenMode = Tween.TweenMode.Oneshot)
        {
            Vector2 startPosition = entity.Position;
            Tween tween = Tween.Create(tweenMode, easer, duration, true);
            tween.OnUpdate = t => entity.Position = Vector2.Lerp(startPosition, targetPosition, t.Eased);
            entity.Add(tween);
            return tween;
        }

        private Tween()
            : base(false, false)
        {
        }

        private void Init(Tween.TweenMode mode, Ease.Easer easer, float duration, bool start)
        {
            if ((double) duration <= 0.0)
                duration = 1E-06f;
            UseRawDeltaTime = false;
            Mode = mode;
            Easer = easer;
            Duration = duration;
            TimeLeft = 0.0f;
            Percent = 0.0f;
            Active = false;
            if (!start)
                return;
            Start();
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Tween.cached.Add(this);
            cachedFrame = Engine.FrameCounter;
        }

        public override void Update()
        {
            TimeLeft -= UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime;
            Percent = Math.Max(0.0f, TimeLeft) / Duration;
            if (!Reverse)
                Percent = 1f - Percent;
            Eased = Easer == null ? Percent : Easer(Percent);
            if (OnUpdate != null)
                OnUpdate(this);
            if ((double) TimeLeft > 0.0)
                return;
            TimeLeft = 0.0f;
            if (OnComplete != null)
                OnComplete(this);
            switch (Mode)
            {
                case Tween.TweenMode.Persist:
                    Active = false;
                    break;
                case Tween.TweenMode.Oneshot:
                    Active = false;
                    RemoveSelf();
                    break;
                case Tween.TweenMode.Looping:
                    Start(Reverse);
                    break;
                case Tween.TweenMode.YoyoOneshot:
                    if (Reverse == startedReversed)
                    {
                        Start(!Reverse);
                        startedReversed = !Reverse;
                        break;
                    }
                    Active = false;
                    RemoveSelf();
                    break;
                case Tween.TweenMode.YoyoLooping:
                    Start(!Reverse);
                    break;
            }
        }

        public void Start() => Start(false);

        public void Start(bool reverse)
        {
            startedReversed = Reverse = reverse;
            TimeLeft = Duration;
            Eased = Percent = Reverse ? 1f : 0.0f;
            Active = true;
            if (OnStart == null)
                return;
            OnStart(this);
        }

        public void Start(float duration, bool reverse = false)
        {
            Duration = duration;
            Start(reverse);
        }

        public void Stop() => Active = false;

        public void Reset()
        {
            TimeLeft = Duration;
            Eased = Percent = Reverse ? 1f : 0.0f;
        }

        public IEnumerator Wait()
        {
            Tween tween = this;
            while (tween.Active)
                yield return null;
        }

        public float Inverted => 1f - Eased;

        public enum TweenMode
        {
            Persist,
            Oneshot,
            Looping,
            YoyoOneshot,
            YoyoLooping,
        }
    }
}
