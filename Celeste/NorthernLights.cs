// Decompiled with JetBrains decompiler
// Type: Celeste.NorthernLights
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class NorthernLights : Backdrop
    {
        private static readonly Color[] colors = new Color[4]
        {
            Calc.HexToColor("2de079"),
            Calc.HexToColor("62f4f6"),
            Calc.HexToColor("45bc2e"),
            Calc.HexToColor("3856f0")
        };
        private readonly List<NorthernLights.Strand> strands = new();
        private readonly NorthernLights.Particle[] particles = new NorthernLights.Particle[50];
        private readonly VertexPositionColorTexture[] verts = new VertexPositionColorTexture[1024];
        private readonly VertexPositionColor[] gradient = new VertexPositionColor[6];
        private VirtualRenderTarget buffer;
        private float timer;
        public float OffsetY;
        public float NorthernLightsAlpha = 1f;

        public NorthernLights()
        {
            for (int index = 0; index < 3; ++index)
            {
                strands.Add(new NorthernLights.Strand());
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
                particles[index].Speed = Calc.Random.Range(4, 14);
                particles[index].Color = Calc.Random.Choose<Color>(NorthernLights.colors);
            }
            Color color1 = Calc.HexToColor("020825");
            Color color2 = Calc.HexToColor("170c2f");
            gradient[0] = new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), color1);
            gradient[1] = new VertexPositionColor(new Vector3(320f, 0.0f, 0.0f), color1);
            gradient[2] = new VertexPositionColor(new Vector3(320f, 180f, 0.0f), color2);
            gradient[3] = new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), color1);
            gradient[4] = new VertexPositionColor(new Vector3(320f, 180f, 0.0f), color2);
            gradient[5] = new VertexPositionColor(new Vector3(0.0f, 180f, 0.0f), color2);
        }

        public override void Update(Scene scene)
        {
            if (Visible)
            {
                timer += Engine.DeltaTime;
                foreach (NorthernLights.Strand strand in strands)
                {
                    strand.Percent += Engine.DeltaTime / strand.Duration;
                    strand.Alpha = Calc.Approach(strand.Alpha, strand.Percent < 1.0 ? 1f : 0.0f, Engine.DeltaTime);
                    if (strand.Alpha <= 0.0 && strand.Percent >= 1.0)
                    {
                        strand.Reset(0.0f);
                    }

                    foreach (NorthernLights.Node node in strand.Nodes)
                    {
                        node.SineOffset += Engine.DeltaTime;
                    }
                }
                for (int index = 0; index < particles.Length; ++index)
                {
                    particles[index].Position.Y += particles[index].Speed * Engine.DeltaTime;
                }
            }
            base.Update(scene);
        }

        public override void BeforeRender(Scene scene)
        {
            buffer ??= VirtualContent.CreateRenderTarget("northern-lights", 320, 180);
            int vert = 0;
            foreach (NorthernLights.Strand strand in strands)
            {
                NorthernLights.Node node1 = strand.Nodes[0];
                for (int index = 1; index < strand.Nodes.Count; ++index)
                {
                    NorthernLights.Node node2 = strand.Nodes[index];
                    float num1 = Math.Min(1f, index / 4f) * NorthernLightsAlpha;
                    float num2 = Math.Min(1f, (strand.Nodes.Count - index) / 4f) * NorthernLightsAlpha;
                    float num3 = OffsetY + ((float)Math.Sin(node1.SineOffset) * 3f);
                    float num4 = OffsetY + ((float)Math.Sin(node2.SineOffset) * 3f);
                    Set(ref vert, node1.Position.X, node1.Position.Y + num3, node1.TextureOffset, 1f, node1.Color * (node1.BottomAlpha * strand.Alpha * num1));
                    Set(ref vert, node1.Position.X, node1.Position.Y - node1.Height + num3, node1.TextureOffset, 0.05f, node1.Color * (node1.TopAlpha * strand.Alpha * num1));
                    Set(ref vert, node2.Position.X, node2.Position.Y - node2.Height + num4, node2.TextureOffset, 0.05f, node2.Color * (node2.TopAlpha * strand.Alpha * num2));
                    Set(ref vert, node1.Position.X, node1.Position.Y + num3, node1.TextureOffset, 1f, node1.Color * (node1.BottomAlpha * strand.Alpha * num1));
                    Set(ref vert, node2.Position.X, node2.Position.Y - node2.Height + num4, node2.TextureOffset, 0.05f, node2.Color * (node2.TopAlpha * strand.Alpha * num2));
                    Set(ref vert, node2.Position.X, node2.Position.Y + num4, node2.TextureOffset, 1f, node2.Color * (node2.BottomAlpha * strand.Alpha * num2));
                    node1 = node2;
                }
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)buffer);
            GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, gradient, gradient.Length);
            Engine.Graphics.GraphicsDevice.Textures[0] = GFX.Misc["northernlights"].Texture.Texture;
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GFX.DrawVertices<VertexPositionColorTexture>(Matrix.Identity, verts, vert, GFX.FxTexture);
            bool clear = false;
            _ = GaussianBlur.Blur((RenderTarget2D)buffer, GameplayBuffers.TempA, buffer, clear: clear, samples: GaussianBlur.Samples.Five, sampleScale: 0.25f, direction: GaussianBlur.Direction.Vertical);
            Draw.SpriteBatch.Begin();
            Camera camera = (scene as Level).Camera;
            for (int index = 0; index < particles.Length; ++index)
            {
                Draw.Rect(new Vector2()
                {
                    X = mod(particles[index].Position.X - (camera.X * 0.2f), 320f),
                    Y = mod(particles[index].Position.Y - (camera.Y * 0.2f), 180f)
                }, 1f, 1f, particles[index].Color);
            }

            Draw.SpriteBatch.End();
        }

        public override void Ended(Scene scene)
        {
            buffer?.Dispose();
            buffer = null;
            base.Ended(scene);
        }

        private void Set(ref int vert, float px, float py, float tx, float ty, Color color)
        {
            verts[vert].Color = color;
            verts[vert].Position.X = px;
            verts[vert].Position.Y = py;
            verts[vert].TextureCoordinate.X = tx;
            verts[vert].TextureCoordinate.Y = ty;
            ++vert;
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, Color.White);
        }

        private float mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private class Strand
        {
            public List<NorthernLights.Node> Nodes = new();
            public float Duration;
            public float Percent;
            public float Alpha;

            public Strand()
            {
                Reset(Calc.Random.NextFloat());
            }

            public void Reset(float startPercent)
            {
                Percent = startPercent;
                Duration = Calc.Random.Range(12f, 32f);
                Alpha = 0.0f;
                Nodes.Clear();
                Vector2 vector2 = new(Calc.Random.Range(-40, 60), Calc.Random.Range(40, 90));
                float num = Calc.Random.NextFloat();
                Color color = Calc.Random.Choose<Color>(NorthernLights.colors);
                for (int index = 0; index < 40; ++index)
                {
                    NorthernLights.Node node = new()
                    {
                        Position = vector2,
                        TextureOffset = num,
                        Height = Calc.Random.Range(10, 80),
                        TopAlpha = Calc.Random.Range(0.3f, 0.8f),
                        BottomAlpha = Calc.Random.Range(0.5f, 1f),
                        SineOffset = Calc.Random.NextFloat() * 6.28318548f,
                        Color = Color.Lerp(color, Calc.Random.Choose<Color>(NorthernLights.colors), Calc.Random.Range(0.0f, 0.3f))
                    };
                    num += Calc.Random.Range(0.02f, 0.2f);
                    vector2 += new Vector2(Calc.Random.Range(4, 20), Calc.Random.Range(-15, 15));
                    Nodes.Add(node);
                }
            }
        }

        private class Node
        {
            public Vector2 Position;
            public float TextureOffset;
            public float Height;
            public float TopAlpha;
            public float BottomAlpha;
            public float SineOffset;
            public Color Color;
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public Color Color;
        }
    }
}
