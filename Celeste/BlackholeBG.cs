using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class BlackholeBG : Backdrop
    {
        private const string STRENGTH_FLAG = "blackhole_strength";
        private const int BG_STEPS = 20;
        private const int STREAM_MIN_COUNT = 30;
        private const int STREAM_MAX_COUNT = 50;
        private const int PARTICLE_MIN_COUNT = 150;
        private const int PARTICLE_MAX_COUNT = 220;
        private const int SPIRAL_MIN_COUNT = 0;
        private const int SPIRAL_MAX_COUNT = 10;
        private const int SPIRAL_SEGMENTS = 12;
        private Color[] colorsMild = new Color[3]
        {
            Calc.HexToColor("6e3199") * 0.8f,
            Calc.HexToColor("851f91") * 0.8f,
            Calc.HexToColor("3026b0") * 0.8f
        };
        private Color[] colorsWild = new Color[3]
        {
            Calc.HexToColor("ca4ca7"),
            Calc.HexToColor("b14cca"),
            Calc.HexToColor("ca4ca7")
        };
        private Color[] colorsLerp;
        private Color[,] colorsLerpBlack;
        private Color[,] colorsLerpTransparent;
        private const int colorSteps = 20;
        public float Alpha = 1f;
        public float Scale = 1f;
        public float Direction = 1f;
        public float StrengthMultiplier = 1f;
        public Vector2 CenterOffset;
        public Vector2 OffsetOffset;
        private Strengths strength;
        private readonly Color bgColorInner = Calc.HexToColor("000000");
        private readonly Color bgColorOuterMild = Calc.HexToColor("512a8b");
        private readonly Color bgColorOuterWild = Calc.HexToColor("bd2192");
        private readonly MTexture bgTexture;
        private StreamParticle[] streams = new StreamParticle[50];
        private VertexPositionColorTexture[] streamVerts = new VertexPositionColorTexture[300];
        private Particle[] particles = new Particle[220];
        private SpiralDebris[] spirals = new SpiralDebris[10];
        private VertexPositionColorTexture[] spiralVerts = new VertexPositionColorTexture[720];
        private VirtualRenderTarget buffer;
        private Vector2 center;
        private Vector2 offset;
        private Vector2 shake;
        private float spinTime;
        private bool checkedFlag;

        public BlackholeBG()
        {
            bgTexture = GFX.Game["objects/temple/portal/portal"];
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/10/blackhole/particle");
            int index1 = 0;
            for (int index2 = 0; index2 < 50; ++index2)
            {
                MTexture mtexture = streams[index2].Texture = Calc.Random.Choose(atlasSubtextures);
                streams[index2].Percent = Calc.Random.NextFloat();
                streams[index2].Speed = Calc.Random.Range(0.2f, 0.4f);
                streams[index2].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                streams[index2].Color = Calc.Random.Next(colorsMild.Length);
                streamVerts[index1].TextureCoordinate = new Vector2(mtexture.LeftUV, mtexture.TopUV);
                streamVerts[index1 + 1].TextureCoordinate = new Vector2(mtexture.RightUV, mtexture.TopUV);
                streamVerts[index1 + 2].TextureCoordinate = new Vector2(mtexture.RightUV, mtexture.BottomUV);
                streamVerts[index1 + 3].TextureCoordinate = new Vector2(mtexture.LeftUV, mtexture.TopUV);
                streamVerts[index1 + 4].TextureCoordinate = new Vector2(mtexture.RightUV, mtexture.BottomUV);
                streamVerts[index1 + 5].TextureCoordinate = new Vector2(mtexture.LeftUV, mtexture.BottomUV);
                index1 += 6;
            }
            int index3 = 0;
            for (int index4 = 0; index4 < 10; ++index4)
            {
                MTexture mtexture = streams[index4].Texture = Calc.Random.Choose(atlasSubtextures);
                spirals[index4].Percent = Calc.Random.NextFloat();
                spirals[index4].Offset = Calc.Random.NextFloat(6.28318548f);
                spirals[index4].Color = Calc.Random.Next(colorsMild.Length);
                for (int index5 = 0; index5 < 12; ++index5)
                {
                    float x1 = MathHelper.Lerp(mtexture.LeftUV, mtexture.RightUV, index5 / 12f);
                    float x2 = MathHelper.Lerp(mtexture.LeftUV, mtexture.RightUV, (index5 + 1) / 12f);
                    spiralVerts[index3].TextureCoordinate = new Vector2(x1, mtexture.TopUV);
                    spiralVerts[index3 + 1].TextureCoordinate = new Vector2(x2, mtexture.TopUV);
                    spiralVerts[index3 + 2].TextureCoordinate = new Vector2(x2, mtexture.BottomUV);
                    spiralVerts[index3 + 3].TextureCoordinate = new Vector2(x1, mtexture.TopUV);
                    spiralVerts[index3 + 4].TextureCoordinate = new Vector2(x2, mtexture.BottomUV);
                    spiralVerts[index3 + 5].TextureCoordinate = new Vector2(x1, mtexture.BottomUV);
                    index3 += 6;
                }
            }
            for (int index6 = 0; index6 < 220; ++index6)
            {
                particles[index6].Percent = Calc.Random.NextFloat();
                particles[index6].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                particles[index6].Color = Calc.Random.Next(colorsMild.Length);
            }
            center = new Vector2(320f, 180f) / 2f;
            offset = Vector2.Zero;
            colorsLerp = new Color[colorsMild.Length];
            colorsLerpBlack = new Color[colorsMild.Length, 20];
            colorsLerpTransparent = new Color[colorsMild.Length, 20];
        }

        public void SnapStrength(Level level, Strengths strength)
        {
            this.strength = strength;
            StrengthMultiplier = 1f + (float) strength;
            level.Session.SetCounter("blackhole_strength", (int) strength);
        }

        public void NextStrength(Level level, Strengths strength)
        {
            this.strength = strength;
            level.Session.SetCounter("blackhole_strength", (int) strength);
        }

        public int StreamCount => (int) MathHelper.Lerp(30f, 50f, (float) ((StrengthMultiplier - 1.0) / 3.0));

        public int ParticleCount => (int) MathHelper.Lerp(150f, 220f, (float) ((StrengthMultiplier - 1.0) / 3.0));

        public int SpiralCount => (int) MathHelper.Lerp(0.0f, 10f, (float) ((StrengthMultiplier - 1.0) / 3.0));

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!checkedFlag)
            {
                int counter = (scene as Level).Session.GetCounter("blackhole_strength");
                if (counter >= 0)
                    SnapStrength(scene as Level, (Strengths) counter);
                checkedFlag = true;
            }
            if (!Visible)
                return;
            StrengthMultiplier = Calc.Approach(StrengthMultiplier, 1f + (float) strength, Engine.DeltaTime * 0.1f);
            if (scene.OnInterval(0.05f))
            {
                for (int index1 = 0; index1 < colorsMild.Length; ++index1)
                {
                    colorsLerp[index1] = Color.Lerp(colorsMild[index1], colorsWild[index1], (float) ((StrengthMultiplier - 1.0) / 3.0));
                    for (int index2 = 0; index2 < 20; ++index2)
                    {
                        colorsLerpBlack[index1, index2] = Color.Lerp(colorsLerp[index1], Color.Black, index2 / 19f) * FadeAlphaMultiplier;
                        colorsLerpTransparent[index1, index2] = Color.Lerp(colorsLerp[index1], Color.Transparent, index2 / 19f) * FadeAlphaMultiplier;
                    }
                }
            }
            float num1 = (float) (1.0 + (StrengthMultiplier - 1.0) * 0.699999988079071);
            int streamCount = StreamCount;
            int v1 = 0;
            for (int index = 0; index < streamCount; ++index)
            {
                streams[index].Percent += streams[index].Speed * Engine.DeltaTime * num1 * Direction;
                if (streams[index].Percent >= 1.0 && Direction > 0.0)
                {
                    streams[index].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                    --streams[index].Percent;
                }
                else if (streams[index].Percent < 0.0 && Direction < 0.0)
                {
                    streams[index].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                    ++streams[index].Percent;
                }
                float percent = streams[index].Percent;
                float num2 = Ease.CubeIn(Calc.ClampedMap(percent, 0.0f, 0.8f));
                float num3 = Ease.CubeIn(Calc.ClampedMap(percent, 0.2f, 1f));
                Vector2 normal = streams[index].Normal;
                Vector2 vector2_1 = normal.Perpendicular();
                Vector2 vector2_2 = normal * 16f + normal * (1f - num2) * 200f;
                float num4 = (float) ((1.0 - num2) * 8.0);
                Color color1 = colorsLerpBlack[streams[index].Color, (int) (num2 * 0.60000002384185791 * 19.0)];
                Vector2 vector2_3 = normal * 16f + normal * (1f - num3) * 280f;
                float num5 = (float) ((1.0 - num3) * 8.0);
                Color color2 = colorsLerpBlack[streams[index].Color, (int) (num3 * 0.60000002384185791 * 19.0)];
                Vector2 a = vector2_2 - vector2_1 * num4;
                Vector2 b = vector2_2 + vector2_1 * num4;
                Vector2 c = vector2_3 + vector2_1 * num5;
                Vector2 d = vector2_3 - vector2_1 * num5;
                AssignVertColors(streamVerts, v1, ref color1, ref color1, ref color2, ref color2);
                AssignVertPosition(streamVerts, v1, ref a, ref b, ref c, ref d);
                v1 += 6;
            }
            float num6 = StrengthMultiplier * 0.25f;
            int particleCount = ParticleCount;
            for (int index = 0; index < particleCount; ++index)
            {
                particles[index].Percent += Engine.DeltaTime * num6 * Direction;
                if (particles[index].Percent >= 1.0 && Direction > 0.0)
                {
                    particles[index].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                    --particles[index].Percent;
                }
                else if (particles[index].Percent < 0.0 && Direction < 0.0)
                {
                    particles[index].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
                    ++particles[index].Percent;
                }
            }
            float num7 = (float) (0.20000000298023224 + (StrengthMultiplier - 1.0) * 0.10000000149011612);
            int spiralCount = SpiralCount;
            Color color3 = Color.Lerp(Color.Lerp(bgColorOuterMild, bgColorOuterWild, (float) ((StrengthMultiplier - 1.0) / 3.0)), Color.White, 0.1f) * 0.8f;
            int v2 = 0;
            for (int index3 = 0; index3 < spiralCount; ++index3)
            {
                spirals[index3].Percent += streams[index3].Speed * Engine.DeltaTime * num7 * Direction;
                if (spirals[index3].Percent >= 1.0 && Direction > 0.0)
                {
                    spirals[index3].Offset = Calc.Random.NextFloat(6.28318548f);
                    --spirals[index3].Percent;
                }
                else if (spirals[index3].Percent < 0.0 && Direction < 0.0)
                {
                    spirals[index3].Offset = Calc.Random.NextFloat(6.28318548f);
                    ++spirals[index3].Percent;
                }
                double percent = spirals[index3].Percent;
                float offset = spirals[index3].Offset;
                float num8 = Calc.ClampedMap((float) percent, 0.0f, 0.8f);
                float num9 = Calc.ClampedMap((float) percent, 0.0f, 1f);
                for (int index4 = 0; index4 < 12; ++index4)
                {
                    float num10 = 1f - MathHelper.Lerp(num8, num9, index4 / 12f);
                    float num11 = 1f - MathHelper.Lerp(num8, num9, (index4 + 1) / 12f);
                    Vector2 vector1 = Calc.AngleToVector(num10 * (float) (20.0 + index4 * 0.20000000298023224) + offset, 1f);
                    Vector2 vector2_4 = vector1 * num10 * 200f;
                    float num12 = num10 * (float) (4.0 + StrengthMultiplier * 4.0);
                    Vector2 vector2 = Calc.AngleToVector(num11 * (float) (20.0 + (index4 + 1) * 0.20000000298023224) + offset, 1f);
                    Vector2 vector2_5 = vector2 * num11 * 200f;
                    float num13 = num11 * (float) (4.0 + StrengthMultiplier * 4.0);
                    Color color4 = Color.Lerp(color3, Color.Black, (float) ((1.0 - num10) * 0.5));
                    Color color5 = Color.Lerp(color3, Color.Black, (float) ((1.0 - num11) * 0.5));
                    Vector2 a = vector2_4 + vector1 * num12;
                    Vector2 b = vector2_5 + vector2 * num13;
                    Vector2 c = vector2_5 - vector2 * num13;
                    Vector2 d = vector2_4 - vector1 * num12;
                    AssignVertColors(spiralVerts, v2, ref color4, ref color5, ref color5, ref color4);
                    AssignVertPosition(spiralVerts, v2, ref a, ref b, ref c, ref d);
                    v2 += 6;
                }
            }
            Vector2 wind = (scene as Level).Wind;
            center += (new Vector2(320f, 180f) / 2f + wind * 0.15f + CenterOffset - center) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
            this.offset += (-wind * 0.25f + OffsetOffset - this.offset) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
            if (scene.OnInterval(0.025f))
                shake = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), (float) (2.0 * (StrengthMultiplier - 1.0)));
            spinTime += (2f + StrengthMultiplier) * Engine.DeltaTime;
        }

        private void AssignVertColors(
            VertexPositionColorTexture[] verts,
            int v,
            ref Color a,
            ref Color b,
            ref Color c,
            ref Color d)
        {
            verts[v].Color = a;
            verts[v + 1].Color = b;
            verts[v + 2].Color = c;
            verts[v + 3].Color = a;
            verts[v + 4].Color = c;
            verts[v + 5].Color = d;
        }

        private void AssignVertPosition(
            VertexPositionColorTexture[] verts,
            int v,
            ref Vector2 a,
            ref Vector2 b,
            ref Vector2 c,
            ref Vector2 d)
        {
            verts[v].Position = new Vector3(a, 0.0f);
            verts[v + 1].Position = new Vector3(b, 0.0f);
            verts[v + 2].Position = new Vector3(c, 0.0f);
            verts[v + 3].Position = new Vector3(a, 0.0f);
            verts[v + 4].Position = new Vector3(c, 0.0f);
            verts[v + 5].Position = new Vector3(d, 0.0f);
        }

        public override void BeforeRender(Scene scene)
        {
            if (buffer == null || buffer.IsDisposed)
                buffer = VirtualContent.CreateRenderTarget("Black Hole", 320, 180);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(bgColorInner);
            Draw.SpriteBatch.Begin();
            Color color1 = Color.Lerp(bgColorOuterMild, bgColorOuterWild, (float) ((StrengthMultiplier - 1.0) / 3.0));
            for (int index = 0; index < 20; ++index)
            {
                float num = (float) ((1.0 - spinTime % 1.0) * 0.05000000074505806 + index / 20.0);
                Color color2 = Color.Lerp(bgColorInner, color1, Ease.SineOut(num));
                float scale = Calc.ClampedMap(num, 0.0f, 1f, 0.1f, 4f);
                float rotation = 6.28318548f * num;
                bgTexture.DrawCentered(center + offset * num + shake * (1f - num), color2, scale, rotation);
            }
            Draw.SpriteBatch.End();
            if (SpiralCount > 0)
            {
                Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture;
                GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0.0f), spiralVerts, SpiralCount * 12 * 6, GFX.FxTexture);
            }
            if (StreamCount > 0)
            {
                Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture;
                GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0.0f), streamVerts, StreamCount * 6, GFX.FxTexture);
            }
            Draw.SpriteBatch.Begin();
            int particleCount = ParticleCount;
            for (int index = 0; index < particleCount; ++index)
            {
                float val = Ease.CubeIn(Calc.Clamp(particles[index].Percent, 0.0f, 1f));
                Vector2 vector2_1 = center + particles[index].Normal * Calc.ClampedMap(val, 1f, 0.0f, 8f, 220f);
                Color color3 = colorsLerpTransparent[particles[index].Color, (int) (val * 19.0)];
                float num = (float) (1.0 + (1.0 - val) * 1.5);
                Vector2 vector2_2 = new Vector2(num, num) / 2f;
                Draw.Rect(vector2_1 - vector2_2, num, num, color3);
            }
            Draw.SpriteBatch.End();
        }

        public override void Ended(Scene scene)
        {
            if (buffer == null)
                return;
            buffer.Dispose();
            buffer = null;
        }

        public override void Render(Scene scene)
        {
            if (buffer == null || buffer.IsDisposed)
                return;
            Vector2 vector2 = new Vector2(buffer.Width, buffer.Height) / 2f;
            Draw.SpriteBatch.Draw((RenderTarget2D) buffer, vector2, buffer.Bounds, Color.White * FadeAlphaMultiplier * Alpha, 0.0f, vector2, Scale, SpriteEffects.None, 0.0f);
        }

        public enum Strengths
        {
            Mild,
            Medium,
            High,
            Wild,
        }

        private struct StreamParticle
        {
            public int Color;
            public MTexture Texture;
            public float Percent;
            public float Speed;
            public Vector2 Normal;
        }

        private struct Particle
        {
            public int Color;
            public Vector2 Normal;
            public float Percent;
        }

        private struct SpiralDebris
        {
            public int Color;
            public float Percent;
            public float Offset;
        }
    }
}
