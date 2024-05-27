using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class VertexLight : Component
    {
        public int Index = -1;
        public bool Dirty = true;
        public bool InSolid;
        public Vector2 LastNonSolidPosition;
        public Vector2 LastEntityPosition;
        public Vector2 LastPosition;
        public float InSolidAlphaMultiplier = 1f;
        public bool Started;
        public bool Spotlight;
        public float SpotlightDirection;
        public float SpotlightPush;
        public Color Color = Color.White;
        public float Alpha = 1f;
        private Vector2 position;
        private float startRadius = 16f;
        private float endRadius = 32f;

        public Vector2 Center => Entity.Position + position;

        public float X
        {
            get => position.X;
            set => Position = new Vector2(value, position.Y);
        }

        public float Y
        {
            get => position.Y;
            set => Position = new Vector2(position.X, value);
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                if (!(position != value))
                    return;
                Dirty = true;
                position = value;
            }
        }

        public float StartRadius
        {
            get => startRadius;
            set
            {
                if (startRadius == (double) value)
                    return;
                Dirty = true;
                startRadius = value;
            }
        }

        public float EndRadius
        {
            get => endRadius;
            set
            {
                if (endRadius == (double) value)
                    return;
                Dirty = true;
                endRadius = value;
            }
        }

        public VertexLight()
            : base(true, true)
        {
        }

        public VertexLight(Color color, float alpha, int startFade, int endFade)
            : this(Vector2.Zero, color, alpha, startFade, endFade)
        {
        }

        public VertexLight(Vector2 position, Color color, float alpha, int startFade, int endFade)
            : base(true, true)
        {
            Position = position;
            Color = color;
            Alpha = alpha;
            StartRadius = startFade;
            EndRadius = endFade;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            LastNonSolidPosition = Center;
            LastEntityPosition = Entity.Position;
            LastPosition = Position;
        }

        public override void Update()
        {
            InSolidAlphaMultiplier = Calc.Approach(InSolidAlphaMultiplier, InSolid ? 0.0f : 1f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void HandleGraphicsReset()
        {
            Dirty = true;
            base.HandleGraphicsReset();
        }

        public Tween CreatePulseTween()
        {
            float startA = StartRadius;
            float startB = startA + 6f;
            float endA = EndRadius;
            float endB = endA + 12f;
            Tween pulseTween = Tween.Create(Tween.TweenMode.Persist, duration: 0.5f);
            pulseTween.OnUpdate = t =>
            {
                StartRadius = (int) MathHelper.Lerp(startB, startA, t.Eased);
                EndRadius = (int) MathHelper.Lerp(endB, endA, t.Eased);
            };
            return pulseTween;
        }

        public Tween CreateFadeInTween(float time)
        {
            float from = 0.0f;
            float to = Alpha;
            Alpha = 0.0f;
            Tween fadeInTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, time);
            fadeInTween.OnUpdate = t => Alpha = MathHelper.Lerp(from, to, t.Eased);
            return fadeInTween;
        }

        public Tween CreateBurstTween(float time)
        {
            time += 0.8f;
            float delay = (time - 0.8f) / time;
            float startA = StartRadius;
            float startB = startA + 6f;
            float endA = EndRadius;
            float endB = endA + 12f;
            Tween burstTween = Tween.Create(Tween.TweenMode.Persist, duration: time);
            burstTween.OnUpdate = t =>
            {
                float amount;
                if (t.Percent >= (double) delay)
                {
                    float t1 = MathHelper.Clamp((float) ((t.Percent - (double) delay) / (1.0 - delay)), 0.0f, 1f);
                    amount = Ease.CubeIn(t1);
                }
                else
                    amount = 0.0f;
                StartRadius = (int) MathHelper.Lerp(startB, startA, amount);
                EndRadius = (int) MathHelper.Lerp(endB, endA, amount);
            };
            return burstTween;
        }
    }
}
