// Decompiled with JetBrains decompiler
// Type: Celeste.VertexLight
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
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

        public Vector2 Center => this.Entity.Position + this.position;

        public float X
        {
            get => this.position.X;
            set => this.Position = new Vector2(value, this.position.Y);
        }

        public float Y
        {
            get => this.position.Y;
            set => this.Position = new Vector2(this.position.X, value);
        }

        public Vector2 Position
        {
            get => this.position;
            set
            {
                if (!(this.position != value))
                    return;
                this.Dirty = true;
                this.position = value;
            }
        }

        public float StartRadius
        {
            get => this.startRadius;
            set
            {
                if ((double) this.startRadius == (double) value)
                    return;
                this.Dirty = true;
                this.startRadius = value;
            }
        }

        public float EndRadius
        {
            get => this.endRadius;
            set
            {
                if ((double) this.endRadius == (double) value)
                    return;
                this.Dirty = true;
                this.endRadius = value;
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
            this.Position = position;
            this.Color = color;
            this.Alpha = alpha;
            this.StartRadius = (float) startFade;
            this.EndRadius = (float) endFade;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            this.LastNonSolidPosition = this.Center;
            this.LastEntityPosition = this.Entity.Position;
            this.LastPosition = this.Position;
        }

        public override void Update()
        {
            this.InSolidAlphaMultiplier = Calc.Approach(this.InSolidAlphaMultiplier, this.InSolid ? 0.0f : 1f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void HandleGraphicsReset()
        {
            this.Dirty = true;
            base.HandleGraphicsReset();
        }

        public Tween CreatePulseTween()
        {
            float startA = this.StartRadius;
            float startB = startA + 6f;
            float endA = this.EndRadius;
            float endB = endA + 12f;
            Tween pulseTween = Tween.Create(Tween.TweenMode.Persist, duration: 0.5f);
            pulseTween.OnUpdate = (Action<Tween>) (t =>
            {
                this.StartRadius = (float) (int) MathHelper.Lerp(startB, startA, t.Eased);
                this.EndRadius = (float) (int) MathHelper.Lerp(endB, endA, t.Eased);
            });
            return pulseTween;
        }

        public Tween CreateFadeInTween(float time)
        {
            float from = 0.0f;
            float to = this.Alpha;
            this.Alpha = 0.0f;
            Tween fadeInTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, time);
            fadeInTween.OnUpdate = (Action<Tween>) (t => this.Alpha = MathHelper.Lerp(from, to, t.Eased));
            return fadeInTween;
        }

        public Tween CreateBurstTween(float time)
        {
            time += 0.8f;
            float delay = (time - 0.8f) / time;
            float startA = this.StartRadius;
            float startB = startA + 6f;
            float endA = this.EndRadius;
            float endB = endA + 12f;
            Tween burstTween = Tween.Create(Tween.TweenMode.Persist, duration: time);
            burstTween.OnUpdate = (Action<Tween>) (t =>
            {
                float amount;
                if ((double) t.Percent >= (double) delay)
                {
                    float t1 = MathHelper.Clamp((float) (((double) t.Percent - (double) delay) / (1.0 - (double) delay)), 0.0f, 1f);
                    amount = Ease.CubeIn(t1);
                }
                else
                    amount = 0.0f;
                this.StartRadius = (float) (int) MathHelper.Lerp(startB, startA, amount);
                this.EndRadius = (float) (int) MathHelper.Lerp(endB, endA, amount);
            });
            return burstTween;
        }
    }
}
