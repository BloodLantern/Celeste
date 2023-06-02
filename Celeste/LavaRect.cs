using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class LavaRect : Component
    {
        public Vector2 Position;
        public float Fade = 16f;
        public float Spikey;
        public LavaRect.OnlyModes OnlyMode;
        public float SmallWaveAmplitude = 1f;
        public float BigWaveAmplitude = 4f;
        public float CurveAmplitude = 12f;
        public float UpdateMultiplier = 1f;
        public Color SurfaceColor = Color.White;
        public Color EdgeColor = Color.LightGray;
        public Color CenterColor = Color.DarkGray;
        private float timer = Calc.Random.NextFloat(100f);
        private VertexPositionColor[] verts;
        private bool dirty;
        private int vertCount;
        private LavaRect.Bubble[] bubbles;
        private LavaRect.SurfaceBubble[] surfaceBubbles;
        private int surfaceBubbleIndex;
        private List<List<MTexture>> surfaceBubbleAnimations;

        public int SurfaceStep { get; private set; }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public LavaRect(float width, float height, int step)
            : base(true, true)
        {
            this.Resize(width, height, step);
        }

        public void Resize(float width, float height, int step)
        {
            this.Width = width;
            this.Height = height;
            this.SurfaceStep = step;
            this.dirty = true;
            this.verts = new VertexPositionColor[(int) ((double) width / (double) this.SurfaceStep * 2.0 + (double) height / (double) this.SurfaceStep * 2.0 + 4.0) * 3 * 6 + 6];
            this.bubbles = new LavaRect.Bubble[(int) ((double) width * (double) height * 0.004999999888241291)];
            this.surfaceBubbles = new LavaRect.SurfaceBubble[(int) Math.Max(4f, (float) this.bubbles.Length * 0.25f)];
            for (int index = 0; index < this.bubbles.Length; ++index)
            {
                this.bubbles[index].Position = new Vector2(1f + Calc.Random.NextFloat(this.Width - 2f), Calc.Random.NextFloat(this.Height));
                this.bubbles[index].Speed = (float) Calc.Random.Range(4, 12);
                this.bubbles[index].Alpha = Calc.Random.Range(0.4f, 0.8f);
            }
            for (int index = 0; index < this.surfaceBubbles.Length; ++index)
                this.surfaceBubbles[index].X = -1f;
            this.surfaceBubbleAnimations = new List<List<MTexture>>();
            this.surfaceBubbleAnimations.Add(GFX.Game.GetAtlasSubtextures("danger/lava/bubble_a"));
        }

        public override void Update()
        {
            this.timer += this.UpdateMultiplier * Engine.DeltaTime;
            if ((double) this.UpdateMultiplier != 0.0)
                this.dirty = true;
            for (int index = 0; index < this.bubbles.Length; ++index)
            {
                this.bubbles[index].Position.Y -= this.UpdateMultiplier * this.bubbles[index].Speed * Engine.DeltaTime;
                if ((double) this.bubbles[index].Position.Y < 2.0 - (double) this.Wave((int) ((double) this.bubbles[index].Position.X / (double) this.SurfaceStep), this.Width))
                {
                    this.bubbles[index].Position.Y = this.Height - 1f;
                    if (Calc.Random.Chance(0.75f))
                    {
                        this.surfaceBubbles[this.surfaceBubbleIndex].X = this.bubbles[index].Position.X;
                        this.surfaceBubbles[this.surfaceBubbleIndex].Frame = 0.0f;
                        this.surfaceBubbles[this.surfaceBubbleIndex].Animation = (byte) Calc.Random.Next(this.surfaceBubbleAnimations.Count);
                        this.surfaceBubbleIndex = (this.surfaceBubbleIndex + 1) % this.surfaceBubbles.Length;
                    }
                }
            }
            for (int index = 0; index < this.surfaceBubbles.Length; ++index)
            {
                if ((double) this.surfaceBubbles[index].X >= 0.0)
                {
                    this.surfaceBubbles[index].Frame += Engine.DeltaTime * 6f;
                    if ((double) this.surfaceBubbles[index].Frame >= (double) this.surfaceBubbleAnimations[(int) this.surfaceBubbles[index].Animation].Count)
                        this.surfaceBubbles[index].X = -1f;
                }
            }
            base.Update();
        }

        private float Sin(float value) => (float) ((1.0 + Math.Sin((double) value)) / 2.0);

        private float Wave(int step, float length)
        {
            int val = step * this.SurfaceStep;
            float num1 = this.OnlyMode != LavaRect.OnlyModes.None ? 1f : Calc.ClampedMap((float) val, 0.0f, length * 0.1f) * Calc.ClampedMap((float) val, length * 0.9f, length, 1f, 0.0f);
            float num2 = this.Sin((float) ((double) val * 0.25 + (double) this.timer * 4.0)) * this.SmallWaveAmplitude + this.Sin((float) ((double) val * 0.05000000074505806 + (double) this.timer * 0.5)) * this.BigWaveAmplitude;
            if (step % 2 == 0)
                num2 += this.Spikey;
            if (this.OnlyMode != LavaRect.OnlyModes.None)
                num2 += (1f - Calc.YoYo((float) val / length)) * this.CurveAmplitude;
            return num2 * num1;
        }

        private void Quad(ref int vert, Vector2 va, Vector2 vb, Vector2 vc, Vector2 vd, Color color) => this.Quad(ref vert, va, color, vb, color, vc, color, vd, color);

        private void Quad(
            ref int vert,
            Vector2 va,
            Color ca,
            Vector2 vb,
            Color cb,
            Vector2 vc,
            Color cc,
            Vector2 vd,
            Color cd)
        {
            this.verts[vert].Position.X = va.X;
            this.verts[vert].Position.Y = va.Y;
            this.verts[vert++].Color = ca;
            this.verts[vert].Position.X = vb.X;
            this.verts[vert].Position.Y = vb.Y;
            this.verts[vert++].Color = cb;
            this.verts[vert].Position.X = vc.X;
            this.verts[vert].Position.Y = vc.Y;
            this.verts[vert++].Color = cc;
            this.verts[vert].Position.X = va.X;
            this.verts[vert].Position.Y = va.Y;
            this.verts[vert++].Color = ca;
            this.verts[vert].Position.X = vc.X;
            this.verts[vert].Position.Y = vc.Y;
            this.verts[vert++].Color = cc;
            this.verts[vert].Position.X = vd.X;
            this.verts[vert].Position.Y = vd.Y;
            this.verts[vert++].Color = cd;
        }

        private void Edge(ref int vert, Vector2 a, Vector2 b, float fade, float insetFade)
        {
            float length = (a - b).Length();
            float newMin = this.OnlyMode == LavaRect.OnlyModes.None ? insetFade / length : 0.0f;
            float num1 = length / (float) this.SurfaceStep;
            Vector2 vector2_1 = (b - a).SafeNormalize().Perpendicular();
            for (int step = 1; (double) step <= (double) num1; ++step)
            {
                Vector2 vector2_2 = Vector2.Lerp(a, b, (float) (step - 1) / num1);
                float num2 = this.Wave(step - 1, length);
                Vector2 vector2_3 = vector2_1 * num2;
                Vector2 va = vector2_2 - vector2_3;
                Vector2 vector2_4 = Vector2.Lerp(a, b, (float) step / num1);
                float num3 = this.Wave(step, length);
                Vector2 vector2_5 = vector2_1 * num3;
                Vector2 vb = vector2_4 - vector2_5;
                Vector2 vector2_6 = Vector2.Lerp(a, b, Calc.ClampedMap((float) (step - 1) / num1, 0.0f, 1f, newMin, 1f - newMin));
                Vector2 vector2_7 = Vector2.Lerp(a, b, Calc.ClampedMap((float) step / num1, 0.0f, 1f, newMin, 1f - newMin));
                this.Quad(ref vert, va + vector2_1, this.EdgeColor, vb + vector2_1, this.EdgeColor, vector2_7 + vector2_1 * (fade - num3), this.CenterColor, vector2_6 + vector2_1 * (fade - num2), this.CenterColor);
                this.Quad(ref vert, vector2_6 + vector2_1 * (fade - num2), vector2_7 + vector2_1 * (fade - num3), vector2_7 + vector2_1 * fade, vector2_6 + vector2_1 * fade, this.CenterColor);
                this.Quad(ref vert, va, vb, vb + vector2_1 * 1f, va + vector2_1 * 1f, this.SurfaceColor);
            }
        }

        public override void Render()
        {
            GameplayRenderer.End();
            if (this.dirty)
            {
                Vector2 zero = Vector2.Zero;
                Vector2 vector2_1 = zero;
                Vector2 vector2_2 = new Vector2(zero.X + this.Width, zero.Y);
                Vector2 vector2_3 = new Vector2(zero.X, zero.Y + this.Height);
                Vector2 vector2_4 = zero + new Vector2(this.Width, this.Height);
                Vector2 vector2_5 = new Vector2(Math.Min(this.Fade, this.Width / 2f), Math.Min(this.Fade, this.Height / 2f));
                this.vertCount = 0;
                if (this.OnlyMode == LavaRect.OnlyModes.None)
                {
                    this.Edge(ref this.vertCount, vector2_1, vector2_2, vector2_5.Y, vector2_5.X);
                    this.Edge(ref this.vertCount, vector2_2, vector2_4, vector2_5.X, vector2_5.Y);
                    this.Edge(ref this.vertCount, vector2_4, vector2_3, vector2_5.Y, vector2_5.X);
                    this.Edge(ref this.vertCount, vector2_3, vector2_1, vector2_5.X, vector2_5.Y);
                    this.Quad(ref this.vertCount, vector2_1 + vector2_5, vector2_2 + new Vector2(-vector2_5.X, vector2_5.Y), vector2_4 - vector2_5, vector2_3 + new Vector2(vector2_5.X, -vector2_5.Y), this.CenterColor);
                }
                else if (this.OnlyMode == LavaRect.OnlyModes.OnlyTop)
                {
                    this.Edge(ref this.vertCount, vector2_1, vector2_2, vector2_5.Y, 0.0f);
                    this.Quad(ref this.vertCount, vector2_1 + new Vector2(0.0f, vector2_5.Y), vector2_2 + new Vector2(0.0f, vector2_5.Y), vector2_4, vector2_3, this.CenterColor);
                }
                else if (this.OnlyMode == LavaRect.OnlyModes.OnlyBottom)
                {
                    this.Edge(ref this.vertCount, vector2_4, vector2_3, vector2_5.Y, 0.0f);
                    this.Quad(ref this.vertCount, vector2_1, vector2_2, vector2_4 + new Vector2(0.0f, -vector2_5.Y), vector2_3 + new Vector2(0.0f, -vector2_5.Y), this.CenterColor);
                }
                this.dirty = false;
            }
            Camera camera = (this.Scene as Level).Camera;
            GFX.DrawVertices<VertexPositionColor>(Matrix.CreateTranslation(new Vector3(this.Entity.Position + this.Position, 0.0f)) * camera.Matrix, this.verts, this.vertCount);
            GameplayRenderer.Begin();
            Vector2 vector2 = this.Entity.Position + this.Position;
            MTexture mtexture1 = GFX.Game["particles/bubble"];
            for (int index = 0; index < this.bubbles.Length; ++index)
                mtexture1.DrawCentered(vector2 + this.bubbles[index].Position, this.SurfaceColor * this.bubbles[index].Alpha);
            for (int index = 0; index < this.surfaceBubbles.Length; ++index)
            {
                if ((double) this.surfaceBubbles[index].X >= 0.0)
                {
                    MTexture mtexture2 = this.surfaceBubbleAnimations[(int) this.surfaceBubbles[index].Animation][(int) this.surfaceBubbles[index].Frame];
                    int step = (int) ((double) this.surfaceBubbles[index].X / (double) this.SurfaceStep);
                    float y = 1f - this.Wave(step, this.Width);
                    Vector2 position = vector2 + new Vector2((float) (step * this.SurfaceStep), y);
                    Vector2 justify = new Vector2(0.5f, 1f);
                    Color surfaceColor = this.SurfaceColor;
                    mtexture2.DrawJustified(position, justify, surfaceColor);
                }
            }
        }

        public enum OnlyModes
        {
            None,
            OnlyTop,
            OnlyBottom,
        }

        private struct Bubble
        {
            public Vector2 Position;
            public float Speed;
            public float Alpha;
        }

        private struct SurfaceBubble
        {
            public float X;
            public float Frame;
            public byte Animation;
        }
    }
}
