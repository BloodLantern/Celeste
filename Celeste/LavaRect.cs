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
        public OnlyModes OnlyMode;
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
        private Bubble[] bubbles;
        private SurfaceBubble[] surfaceBubbles;
        private int surfaceBubbleIndex;
        private List<List<MTexture>> surfaceBubbleAnimations;

        public int SurfaceStep { get; private set; }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public LavaRect(float width, float height, int step)
            : base(true, true)
        {
            Resize(width, height, step);
        }

        public void Resize(float width, float height, int step)
        {
            Width = width;
            Height = height;
            SurfaceStep = step;
            dirty = true;
            verts = new VertexPositionColor[(int) ((double) width / SurfaceStep * 2.0 + (double) height / SurfaceStep * 2.0 + 4.0) * 3 * 6 + 6];
            bubbles = new Bubble[(int) (width * (double) height * 0.004999999888241291)];
            surfaceBubbles = new SurfaceBubble[(int) Math.Max(4f, bubbles.Length * 0.25f)];
            for (int index = 0; index < bubbles.Length; ++index)
            {
                bubbles[index].Position = new Vector2(1f + Calc.Random.NextFloat(Width - 2f), Calc.Random.NextFloat(Height));
                bubbles[index].Speed = Calc.Random.Range(4, 12);
                bubbles[index].Alpha = Calc.Random.Range(0.4f, 0.8f);
            }
            for (int index = 0; index < surfaceBubbles.Length; ++index)
                surfaceBubbles[index].X = -1f;
            surfaceBubbleAnimations = new List<List<MTexture>>();
            surfaceBubbleAnimations.Add(GFX.Game.GetAtlasSubtextures("danger/lava/bubble_a"));
        }

        public override void Update()
        {
            timer += UpdateMultiplier * Engine.DeltaTime;
            if (UpdateMultiplier != 0.0)
                dirty = true;
            for (int index = 0; index < bubbles.Length; ++index)
            {
                bubbles[index].Position.Y -= UpdateMultiplier * bubbles[index].Speed * Engine.DeltaTime;
                if (bubbles[index].Position.Y < 2.0 - Wave((int) (bubbles[index].Position.X / (double) SurfaceStep), Width))
                {
                    bubbles[index].Position.Y = Height - 1f;
                    if (Calc.Random.Chance(0.75f))
                    {
                        surfaceBubbles[surfaceBubbleIndex].X = bubbles[index].Position.X;
                        surfaceBubbles[surfaceBubbleIndex].Frame = 0.0f;
                        surfaceBubbles[surfaceBubbleIndex].Animation = (byte) Calc.Random.Next(surfaceBubbleAnimations.Count);
                        surfaceBubbleIndex = (surfaceBubbleIndex + 1) % surfaceBubbles.Length;
                    }
                }
            }
            for (int index = 0; index < surfaceBubbles.Length; ++index)
            {
                if (surfaceBubbles[index].X >= 0.0)
                {
                    surfaceBubbles[index].Frame += Engine.DeltaTime * 6f;
                    if (surfaceBubbles[index].Frame >= (double) surfaceBubbleAnimations[surfaceBubbles[index].Animation].Count)
                        surfaceBubbles[index].X = -1f;
                }
            }
            base.Update();
        }

        private float Sin(float value) => (float) ((1.0 + Math.Sin(value)) / 2.0);

        private float Wave(int step, float length)
        {
            int val = step * SurfaceStep;
            float num1 = OnlyMode != OnlyModes.None ? 1f : Calc.ClampedMap(val, 0.0f, length * 0.1f) * Calc.ClampedMap(val, length * 0.9f, length, 1f, 0.0f);
            float num2 = Sin((float) (val * 0.25 + timer * 4.0)) * SmallWaveAmplitude + Sin((float) (val * 0.05000000074505806 + timer * 0.5)) * BigWaveAmplitude;
            if (step % 2 == 0)
                num2 += Spikey;
            if (OnlyMode != OnlyModes.None)
                num2 += (1f - Calc.YoYo(val / length)) * CurveAmplitude;
            return num2 * num1;
        }

        private void Quad(ref int vert, Vector2 va, Vector2 vb, Vector2 vc, Vector2 vd, Color color) => Quad(ref vert, va, color, vb, color, vc, color, vd, color);

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
            verts[vert].Position.X = va.X;
            verts[vert].Position.Y = va.Y;
            verts[vert++].Color = ca;
            verts[vert].Position.X = vb.X;
            verts[vert].Position.Y = vb.Y;
            verts[vert++].Color = cb;
            verts[vert].Position.X = vc.X;
            verts[vert].Position.Y = vc.Y;
            verts[vert++].Color = cc;
            verts[vert].Position.X = va.X;
            verts[vert].Position.Y = va.Y;
            verts[vert++].Color = ca;
            verts[vert].Position.X = vc.X;
            verts[vert].Position.Y = vc.Y;
            verts[vert++].Color = cc;
            verts[vert].Position.X = vd.X;
            verts[vert].Position.Y = vd.Y;
            verts[vert++].Color = cd;
        }

        private void Edge(ref int vert, Vector2 a, Vector2 b, float fade, float insetFade)
        {
            float length = (a - b).Length();
            float newMin = OnlyMode == OnlyModes.None ? insetFade / length : 0.0f;
            float num1 = length / SurfaceStep;
            Vector2 vector2_1 = (b - a).SafeNormalize().Perpendicular();
            for (int step = 1; step <= (double) num1; ++step)
            {
                Vector2 vector2_2 = Vector2.Lerp(a, b, (step - 1) / num1);
                float num2 = Wave(step - 1, length);
                Vector2 vector2_3 = vector2_1 * num2;
                Vector2 va = vector2_2 - vector2_3;
                Vector2 vector2_4 = Vector2.Lerp(a, b, step / num1);
                float num3 = Wave(step, length);
                Vector2 vector2_5 = vector2_1 * num3;
                Vector2 vb = vector2_4 - vector2_5;
                Vector2 vector2_6 = Vector2.Lerp(a, b, Calc.ClampedMap((step - 1) / num1, 0.0f, 1f, newMin, 1f - newMin));
                Vector2 vector2_7 = Vector2.Lerp(a, b, Calc.ClampedMap(step / num1, 0.0f, 1f, newMin, 1f - newMin));
                Quad(ref vert, va + vector2_1, EdgeColor, vb + vector2_1, EdgeColor, vector2_7 + vector2_1 * (fade - num3), CenterColor, vector2_6 + vector2_1 * (fade - num2), CenterColor);
                Quad(ref vert, vector2_6 + vector2_1 * (fade - num2), vector2_7 + vector2_1 * (fade - num3), vector2_7 + vector2_1 * fade, vector2_6 + vector2_1 * fade, CenterColor);
                Quad(ref vert, va, vb, vb + vector2_1 * 1f, va + vector2_1 * 1f, SurfaceColor);
            }
        }

        public override void Render()
        {
            GameplayRenderer.End();
            if (dirty)
            {
                Vector2 zero = Vector2.Zero;
                Vector2 vector2_1 = zero;
                Vector2 vector2_2 = new(zero.X + Width, zero.Y);
                Vector2 vector2_3 = new(zero.X, zero.Y + Height);
                Vector2 vector2_4 = zero + new Vector2(Width, Height);
                Vector2 vector2_5 = new(Math.Min(Fade, Width / 2f), Math.Min(Fade, Height / 2f));
                vertCount = 0;
                if (OnlyMode == OnlyModes.None)
                {
                    Edge(ref vertCount, vector2_1, vector2_2, vector2_5.Y, vector2_5.X);
                    Edge(ref vertCount, vector2_2, vector2_4, vector2_5.X, vector2_5.Y);
                    Edge(ref vertCount, vector2_4, vector2_3, vector2_5.Y, vector2_5.X);
                    Edge(ref vertCount, vector2_3, vector2_1, vector2_5.X, vector2_5.Y);
                    Quad(ref vertCount, vector2_1 + vector2_5, vector2_2 + new Vector2(-vector2_5.X, vector2_5.Y), vector2_4 - vector2_5, vector2_3 + new Vector2(vector2_5.X, -vector2_5.Y), CenterColor);
                }
                else if (OnlyMode == OnlyModes.OnlyTop)
                {
                    Edge(ref vertCount, vector2_1, vector2_2, vector2_5.Y, 0.0f);
                    Quad(ref vertCount, vector2_1 + new Vector2(0.0f, vector2_5.Y), vector2_2 + new Vector2(0.0f, vector2_5.Y), vector2_4, vector2_3, CenterColor);
                }
                else if (OnlyMode == OnlyModes.OnlyBottom)
                {
                    Edge(ref vertCount, vector2_4, vector2_3, vector2_5.Y, 0.0f);
                    Quad(ref vertCount, vector2_1, vector2_2, vector2_4 + new Vector2(0.0f, -vector2_5.Y), vector2_3 + new Vector2(0.0f, -vector2_5.Y), CenterColor);
                }
                dirty = false;
            }
            Camera camera = (Scene as Level).Camera;
            GFX.DrawVertices(Matrix.CreateTranslation(new Vector3(Entity.Position + Position, 0.0f)) * camera.Matrix, verts, vertCount);
            GameplayRenderer.Begin();
            Vector2 vector2 = Entity.Position + Position;
            MTexture mtexture1 = GFX.Game["particles/bubble"];
            for (int index = 0; index < bubbles.Length; ++index)
                mtexture1.DrawCentered(vector2 + bubbles[index].Position, SurfaceColor * bubbles[index].Alpha);
            for (int index = 0; index < surfaceBubbles.Length; ++index)
            {
                if (surfaceBubbles[index].X >= 0.0)
                {
                    MTexture mtexture2 = surfaceBubbleAnimations[surfaceBubbles[index].Animation][(int) surfaceBubbles[index].Frame];
                    int step = (int) (surfaceBubbles[index].X / (double) SurfaceStep);
                    float y = 1f - Wave(step, Width);
                    Vector2 position = vector2 + new Vector2(step * SurfaceStep, y);
                    Vector2 justify = new(0.5f, 1f);
                    Color surfaceColor = SurfaceColor;
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
