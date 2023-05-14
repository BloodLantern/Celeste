// Decompiled with JetBrains decompiler
// Type: Celeste.LightningRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class LightningRenderer : Entity
    {
        private readonly List<Lightning> list = new();
        private readonly List<LightningRenderer.Edge> edges = new();
        private readonly List<LightningRenderer.Bolt> bolts = new();
        private VertexPositionColor[] edgeVerts;
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private uint edgeSeed;
        private uint leapSeed;
        private bool dirty;
        private readonly Color[] electricityColors = new Color[2]
        {
            Calc.HexToColor("fcf579"),
            Calc.HexToColor("8cf7e2")
        };
        private readonly Color[] electricityColorsLerped;
        public float Fade;
        public bool UpdateSeeds = true;
        public const int BoltBufferSize = 160;
        public bool DrawEdges = true;
        public SoundSource AmbientSfx;

        public LightningRenderer()
        {
            Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
            Depth = -1000100;
            electricityColorsLerped = new Color[electricityColors.Length];
            Add(new CustomBloom(new Action(OnRenderBloom)));
            Add(new BeforeRenderHook(new Action(OnBeforeRender)));
            Add(AmbientSfx = new SoundSource());
            AmbientSfx.DisposeOnTransition = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            for (int index = 0; index < 4; ++index)
            {
                bolts.Add(new LightningRenderer.Bolt(electricityColors[0], 1f, 160, 160));
                bolts.Add(new LightningRenderer.Bolt(electricityColors[1], 0.35f, 160, 160));
            }
        }

        public void StartAmbience()
        {
            if (AmbientSfx.Playing)
            {
                return;
            }

            _ = AmbientSfx.Play("event:/new_content/env/10_electricity");
        }

        public void StopAmbience()
        {
            _ = AmbientSfx.Stop();
        }

        public void Reset()
        {
            UpdateSeeds = true;
            Fade = 0.0f;
        }

        public void Track(Lightning block)
        {
            list.Add(block);
            if (tiles == null)
            {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height);
            }
            for (int index1 = (int)block.X / 8; index1 < ((int)block.X + block.VisualWidth) / 8; ++index1)
            {
                for (int index2 = (int)block.Y / 8; index2 < ((int)block.Y + block.VisualHeight) / 8; ++index2)
                {
                    tiles[index1 - levelTileBounds.X, index2 - levelTileBounds.Y] = true;
                }
            }
            dirty = true;
        }

        public void Untrack(Lightning block)
        {
            _ = list.Remove(block);
            if (list.Count <= 0)
            {
                tiles = null;
            }
            else
            {
                for (int index1 = (int)block.X / 8; index1 < (double)block.Right / 8.0; ++index1)
                {
                    for (int index2 = (int)block.Y / 8; index2 < (double)block.Bottom / 8.0; ++index2)
                    {
                        tiles[index1 - levelTileBounds.X, index2 - levelTileBounds.Y] = false;
                    }
                }
            }
            dirty = true;
        }

        public override void Update()
        {
            if (dirty)
            {
                RebuildEdges();
            }

            ToggleEdges();
            if (list.Count <= 0)
            {
                return;
            }

            foreach (LightningRenderer.Bolt bolt in bolts)
            {
                bolt.Update(Scene);
            }

            if (!UpdateSeeds)
            {
                return;
            }

            if (Scene.OnInterval(0.1f))
            {
                edgeSeed = (uint)Calc.Random.Next();
            }

            if (!Scene.OnInterval(0.7f))
            {
                return;
            }

            leapSeed = (uint)Calc.Random.Next();
        }

        public void ToggleEdges(bool immediate = false)
        {
            Camera camera = (Scene as Level).Camera;
            Rectangle view = new((int)camera.Left - 4, (int)camera.Top - 4, (int)((double)camera.Right - (double)camera.Left) + 8, (int)((double)camera.Bottom - (double)camera.Top) + 8);
            for (int index = 0; index < edges.Count; ++index)
            {
                if (immediate)
                {
                    edges[index].Visible = edges[index].InView(ref view);
                }
                else if (!edges[index].Visible && Scene.OnInterval(0.05f, index * 0.01f) && edges[index].InView(ref view))
                {
                    edges[index].Visible = true;
                }
                else if (edges[index].Visible && Scene.OnInterval(0.25f, index * 0.01f) && !edges[index].InView(ref view))
                {
                    edges[index].Visible = false;
                }
            }
        }

        private void RebuildEdges()
        {
            dirty = false;
            edges.Clear();
            if (list.Count <= 0)
            {
                return;
            }

            Level scene = Scene as Level;
            _ = scene.TileBounds.Left;
            Rectangle tileBounds = scene.TileBounds;
            _ = tileBounds.Top;
            tileBounds = scene.TileBounds;
            _ = tileBounds.Right;
            tileBounds = scene.TileBounds;
            _ = tileBounds.Bottom;
            Point[] pointArray = new Point[4]
            {
                new Point(0, -1),
                new Point(0, 1),
                new Point(-1, 0),
                new Point(1, 0)
            };
            foreach (Lightning parent in list)
            {
                for (int x = (int)parent.X / 8; x < (double)parent.Right / 8.0; ++x)
                {
                    for (int y = (int)parent.Y / 8; y < (double)parent.Bottom / 8.0; ++y)
                    {
                        foreach (Point point1 in pointArray)
                        {
                            Point point2 = new(-point1.Y, point1.X);
                            if (!Inside(x + point1.X, y + point1.Y) && (!Inside(x - point2.X, y - point2.Y) || Inside(x + point1.X - point2.X, y + point1.Y - point2.Y)))
                            {
                                Point point3 = new(x, y);
                                Point point4 = new(x + point2.X, y + point2.Y);
                                Vector2 vector2 = new Vector2(4f) + (new Vector2(point1.X - point2.X, point1.Y - point2.Y) * 4f);
                                int num = 1;
                                while (Inside(point4.X, point4.Y) && !Inside(point4.X + point1.X, point4.Y + point1.Y))
                                {
                                    point4.X += point2.X;
                                    point4.Y += point2.Y;
                                    ++num;
                                    if (num > 8)
                                    {
                                        Vector2 a = (new Vector2(point3.X, point3.Y) * 8f) + vector2 - parent.Position;
                                        Vector2 b = (new Vector2(point4.X, point4.Y) * 8f) + vector2 - parent.Position;
                                        edges.Add(new LightningRenderer.Edge(parent, a, b));
                                        num = 0;
                                        point3 = point4;
                                    }
                                }
                                if (num > 0)
                                {
                                    Vector2 a = (new Vector2(point3.X, point3.Y) * 8f) + vector2 - parent.Position;
                                    Vector2 b = (new Vector2(point4.X, point4.Y) * 8f) + vector2 - parent.Position;
                                    edges.Add(new LightningRenderer.Edge(parent, a, b));
                                }
                            }
                        }
                    }
                }
            }
            if (edgeVerts != null)
            {
                return;
            }

            edgeVerts = new VertexPositionColor[1024];
        }

        private bool Inside(int tx, int ty)
        {
            return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
        }

        private void OnRenderBloom()
        {
            Camera camera = (Scene as Level).Camera;
            _ = new Rectangle((int)camera.Left, (int)camera.Top, (int)((double)camera.Right - (double)camera.Left), (int)((double)camera.Bottom - (double)camera.Top));
            Color color = Color.White * (float)(0.25 + (Fade * 0.75));
            foreach (LightningRenderer.Edge edge in edges)
            {
                if (edge.Visible)
                {
                    Draw.Line(edge.Parent.Position + edge.A, edge.Parent.Position + edge.B, color, 4f);
                }
            }
            foreach (Lightning lightning in list)
            {
                if (lightning.Visible)
                {
                    Draw.Rect(lightning.X, lightning.Y, lightning.VisualWidth, lightning.VisualHeight, color);
                }
            }
            if (Fade <= 0.0)
            {
                return;
            }

            Level scene = Scene as Level;
            Draw.Rect(scene.Camera.X, scene.Camera.Y, 320f, 180f, Color.White * Fade);
        }

        private void OnBeforeRender()
        {
            if (list.Count <= 0)
            {
                return;
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)GameplayBuffers.Lightning);
            Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Calc.HexToColor("f7b262") * 0.1f, Color.White, Fade));
            Draw.SpriteBatch.Begin();
            foreach (LightningRenderer.Bolt bolt in bolts)
            {
                bolt.Render();
            }

            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            if (list.Count <= 0)
            {
                return;
            }

            Camera camera = (Scene as Level).Camera;
            _ = new Rectangle((int)camera.Left, (int)camera.Top, (int)((double)camera.Right - (double)camera.Left), (int)((double)camera.Bottom - (double)camera.Top));
            foreach (Lightning lightning in list)
            {
                if (lightning.Visible)
                {
                    Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Lightning, lightning.Position, new Rectangle?(new Rectangle((int)lightning.X, (int)lightning.Y, lightning.VisualWidth, lightning.VisualHeight)), Color.White);
                }
            }
            if (edges.Count <= 0 || !DrawEdges)
            {
                return;
            }

            for (int index = 0; index < electricityColorsLerped.Length; ++index)
            {
                electricityColorsLerped[index] = Color.Lerp(electricityColors[index], Color.White, Fade);
            }

            int index1 = 0;
            uint leapSeed = this.leapSeed;
            foreach (LightningRenderer.Edge edge in edges)
            {
                if (edge.Visible)
                {
                    LightningRenderer.DrawSimpleLightning(ref index1, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, electricityColorsLerped[0], (float)(1.0 + (Fade * 3.0)));
                    LightningRenderer.DrawSimpleLightning(ref index1, ref edgeVerts, edgeSeed + 1U, edge.Parent.Position, edge.A, edge.B, electricityColorsLerped[1], (float)(1.0 + (Fade * 3.0)));
                    if (LightningRenderer.PseudoRand(ref leapSeed) % 30U == 0U)
                    {
                        LightningRenderer.DrawBezierLightning(ref index1, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, 24f, 10, electricityColorsLerped[1]);
                    }
                }
            }
            if (index1 <= 0)
            {
                return;
            }

            GameplayRenderer.End();
            GFX.DrawVertices<VertexPositionColor>(camera.Matrix, edgeVerts, index1);
            GameplayRenderer.Begin();
        }

        private static void DrawSimpleLightning(
            ref int index,
            ref VertexPositionColor[] verts,
            uint seed,
            Vector2 pos,
            Vector2 a,
            Vector2 b,
            Color color,
            float thickness = 1f)
        {
            seed += (uint)(a.GetHashCode() + b.GetHashCode());
            a += pos;
            b += pos;
            float num1 = (b - a).Length();
            Vector2 vec = (b - a) / num1;
            Vector2 vector2_1 = vec.TurnRight();
            a += vector2_1;
            b += vector2_1;
            Vector2 vector2_2 = a;
            int num2 = LightningRenderer.PseudoRand(ref seed) % 2U == 0U ? -1 : 1;
            float num3 = LightningRenderer.PseudoRandRange(ref seed, 0.0f, 6.28318548f);
            float num4 = 0.0f;
            float num5 = index + (float)((((double)(b - a).Length() / 4.0) + 1.0) * 6.0);
            while ((double)num5 >= verts.Length)
            {
                Array.Resize<VertexPositionColor>(ref verts, verts.Length * 2);
            }

            for (int index1 = index; index1 < (double)num5; ++index1)
            {
                verts[index1].Color = color;
            }

            do
            {
                float num6 = LightningRenderer.PseudoRandRange(ref seed, 0.0f, 4f);
                num3 += 0.1f;
                num4 += 4f + num6;
                Vector2 vector2_3 = a + (vec * num4);
                Vector2 vector2_4 = (double)num4 >= (double)num1 ? b : vector2_3 + ((num2 * vector2_1 * num6) - vector2_1);
                verts[index++].Position = new Vector3(vector2_2 - (vector2_1 * thickness), 0.0f);
                verts[index++].Position = new Vector3(vector2_4 - (vector2_1 * thickness), 0.0f);
                verts[index++].Position = new Vector3(vector2_4 + (vector2_1 * thickness), 0.0f);
                verts[index++].Position = new Vector3(vector2_2 - (vector2_1 * thickness), 0.0f);
                verts[index++].Position = new Vector3(vector2_4 + (vector2_1 * thickness), 0.0f);
                verts[index++].Position = new Vector3(vector2_2, 0.0f);
                vector2_2 = vector2_4;
                num2 = -num2;
            }
            while ((double)num4 < (double)num1);
        }

        private static void DrawBezierLightning(
            ref int index,
            ref VertexPositionColor[] verts,
            uint seed,
            Vector2 pos,
            Vector2 a,
            Vector2 b,
            float anchor,
            int steps,
            Color color)
        {
            seed += (uint)(a.GetHashCode() + b.GetHashCode());
            a += pos;
            b += pos;
            Vector2 vector2_1 = (b - a).SafeNormalize().TurnRight();
            SimpleCurve simpleCurve = new(a, b, ((b + a) / 2f) + (vector2_1 * anchor));
            int num = index + ((steps + 2) * 6);
            while (num >= verts.Length)
            {
                Array.Resize<VertexPositionColor>(ref verts, verts.Length * 2);
            }

            Vector2 vector2_2 = simpleCurve.GetPoint(0.0f);
            for (int index1 = 0; index1 <= steps; ++index1)
            {
                Vector2 point = simpleCurve.GetPoint(index1 / (float)steps);
                if (index1 != steps)
                {
                    point += new Vector2(LightningRenderer.PseudoRandRange(ref seed, -2f, 2f), LightningRenderer.PseudoRandRange(ref seed, -2f, 2f));
                }

                verts[index].Position = new Vector3(vector2_2 - vector2_1, 0.0f);
                verts[index++].Color = color;
                verts[index].Position = new Vector3(point - vector2_1, 0.0f);
                verts[index++].Color = color;
                verts[index].Position = new Vector3(point, 0.0f);
                verts[index++].Color = color;
                verts[index].Position = new Vector3(vector2_2 - vector2_1, 0.0f);
                verts[index++].Color = color;
                verts[index].Position = new Vector3(point, 0.0f);
                verts[index++].Color = color;
                verts[index].Position = new Vector3(vector2_2, 0.0f);
                verts[index++].Color = color;
                vector2_2 = point;
            }
        }

        private static void DrawFatLightning(
            uint seed,
            Vector2 a,
            Vector2 b,
            float size,
            float gap,
            Color color)
        {
            seed += (uint)(a.GetHashCode() + b.GetHashCode());
            float num1 = (b - a).Length();
            Vector2 vec = (b - a) / num1;
            Vector2 vector2_1 = vec.TurnRight();
            Vector2 start = a;
            int num2 = 1;
            _ = (double)LightningRenderer.PseudoRandRange(ref seed, 0.0f, 6.28318548f);
            float num4 = 0.0f;
            do
            {
                num4 += LightningRenderer.PseudoRandRange(ref seed, 10f, 14f);
                Vector2 vector2_2 = a + (vec * num4);
                Vector2 vector2_3 = (double)num4 >= (double)num1 ? b : vector2_2 + (num2 * vector2_1 * LightningRenderer.PseudoRandRange(ref seed, 0.0f, 6f));
                Vector2 vector2_4 = vector2_3;
                if ((double)gap > 0.0)
                {
                    vector2_4 = start + ((vector2_3 - start) * (1f - gap));
                    Draw.Line(start, vector2_3 + vec, color, size * 0.5f);
                }
                Draw.Line(start, vector2_4 + vec, color, size);
                start = vector2_3;
                num2 = -num2;
            }
            while ((double)num4 < (double)num1);
        }

        private static uint PseudoRand(ref uint seed)
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            return seed;
        }

        public static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (float)((LightningRenderer.PseudoRand(ref seed) & 1023U) / 1024.0 * ((double)max - (double)min));
        }

        private class Bolt
        {
            private readonly List<Vector2> nodes = new();
            private readonly Coroutine routine;
            private bool visible;
            private float size;
            private float gap;
            private float alpha;
            private uint seed;
            private float flash;
            private readonly Color color;
            private readonly float scale;
            private readonly int width;
            private readonly int height;

            public Bolt(Color color, float scale, int width, int height)
            {
                this.color = color;
                this.width = width;
                this.height = height;
                this.scale = scale;
                routine = new Coroutine(Run());
            }

            public void Update(Scene scene)
            {
                routine.Update();
                flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 2f);
            }

            private IEnumerator Run()
            {
                yield return Calc.Random.Range(0.0f, 4f);
                while (true)
                {
                    List<Vector2> vector2List = new();
                    for (int index = 0; index < 3; ++index)
                    {
                        Vector2 vector2_1 = Calc.Random.Choose<Vector2>(new Vector2(0.0f, Calc.Random.Range(8, height - 16)), new Vector2(Calc.Random.Range(8, width - 16), 0.0f), new Vector2(width, Calc.Random.Range(8, height - 16)), new Vector2(Calc.Random.Range(8, width - 16), height));
                        Vector2 vector2_2 = vector2_1.X <= 0.0 || vector2_1.X >= (double)width ? new Vector2(width - vector2_1.X, vector2_1.Y) : new Vector2(vector2_1.X, height - vector2_1.Y);
                        vector2List.Add(vector2_1);
                        vector2List.Add(vector2_2);
                    }
                    List<Vector2> list = new();
                    for (int index = 0; index < 3; ++index)
                    {
                        list.Add(new Vector2(Calc.Random.Range(0.25f, 0.75f) * width, Calc.Random.Range(0.25f, 0.75f) * height));
                    }

                    nodes.Clear();
                    foreach (Vector2 to in vector2List)
                    {
                        nodes.Add(to);
                        nodes.Add(list.ClosestTo(to));
                    }
                    Vector2 vector2_3 = list[list.Count - 1];
                    foreach (Vector2 vector2_4 in list)
                    {
                        nodes.Add(vector2_3);
                        nodes.Add(vector2_4);
                        vector2_3 = vector2_4;
                    }
                    flash = 1f;
                    visible = true;
                    size = 5f;
                    gap = 0.0f;
                    alpha = 1f;
                    int i;
                    for (i = 0; i < 4; ++i)
                    {
                        seed = (uint)Calc.Random.Next();
                        yield return 0.1f;
                    }
                    for (i = 0; i < 5; ++i)
                    {
                        if (!Settings.Instance.DisableFlashes)
                        {
                            visible = false;
                        }

                        yield return (float)(0.05000000074505806 + (i * 0.019999999552965164));
                        float num = i / 5f;
                        visible = true;
                        size = (float)((1.0 - (double)num) * 5.0);
                        gap = num;
                        alpha = 1f - num;
                        visible = true;
                        seed = (uint)Calc.Random.Next();
                        yield return 0.025f;
                    }
                    visible = false;
                    yield return Calc.Random.Range(4f, 8f);
                }
            }

            public void Render()
            {
                if (flash > 0.0 && !Settings.Instance.DisableFlashes)
                {
                    Draw.Rect(0.0f, 0.0f, width, height, Color.White * flash * 0.15f * scale);
                }

                if (!visible)
                {
                    return;
                }

                for (int index = 0; index < nodes.Count; index += 2)
                {
                    LightningRenderer.DrawFatLightning(seed, nodes[index], nodes[index + 1], size * scale, gap, color * alpha);
                }
            }
        }

        private class Edge
        {
            public Lightning Parent;
            public bool Visible;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;

            public Edge(Lightning parent, Vector2 a, Vector2 b)
            {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            }

            public bool InView(ref Rectangle view)
            {
                return view.Left < (double)Parent.X + Max.X && view.Right > (double)Parent.X + Min.X && view.Top < (double)Parent.Y + Max.Y && view.Bottom > (double)Parent.Y + Min.Y;
            }
        }
    }
}
