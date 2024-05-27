using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class SeekerBarrierRenderer : Entity
    {
        private List<SeekerBarrier> list = new List<SeekerBarrier>();
        private List<Edge> edges = new List<Edge>();
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private bool dirty;

        public SeekerBarrierRenderer()
        {
            Tag = (int) Tags.Global | (int) Tags.TransitionUpdate;
            Depth = 0;
            Add(new CustomBloom(OnRenderBloom));
        }

        public void Track(SeekerBarrier block)
        {
            list.Add(block);
            if (tiles == null)
            {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height);
            }
            for (int index1 = (int) block.X / 8; index1 < block.Right / 8.0; ++index1)
            {
                for (int index2 = (int) block.Y / 8; index2 < block.Bottom / 8.0; ++index2)
                    tiles[index1 - levelTileBounds.X, index2 - levelTileBounds.Y] = true;
            }
            dirty = true;
        }

        public void Untrack(SeekerBarrier block)
        {
            list.Remove(block);
            if (list.Count <= 0)
            {
                tiles = null;
            }
            else
            {
                for (int index1 = (int) block.X / 8; index1 < block.Right / 8.0; ++index1)
                {
                    for (int index2 = (int) block.Y / 8; index2 < block.Bottom / 8.0; ++index2)
                        tiles[index1 - levelTileBounds.X, index2 - levelTileBounds.Y] = false;
                }
            }
            dirty = true;
        }

        public override void Update()
        {
            if (dirty)
                RebuildEdges();
            UpdateEdges();
        }

        public void UpdateEdges()
        {
            Camera camera = (Scene as Level).Camera;
            Rectangle view = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4, (int) (camera.Right - (double) camera.Left) + 8, (int) (camera.Bottom - (double) camera.Top) + 8);
            for (int index = 0; index < edges.Count; ++index)
            {
                if (edges[index].Visible)
                {
                    if (Scene.OnInterval(0.25f, index * 0.01f) && !edges[index].InView(ref view))
                        edges[index].Visible = false;
                }
                else if (Scene.OnInterval(0.05f, index * 0.01f) && edges[index].InView(ref view))
                    edges[index].Visible = true;
                if (edges[index].Visible && (Scene.OnInterval(0.05f, index * 0.01f) || edges[index].Wave == null))
                    edges[index].UpdateWave(Scene.TimeActive * 3f);
            }
        }

        private void RebuildEdges()
        {
            dirty = false;
            edges.Clear();
            if (list.Count <= 0)
                return;
            Level scene = Scene as Level;
            int left = scene.TileBounds.Left;
            Rectangle tileBounds = scene.TileBounds;
            int top = tileBounds.Top;
            tileBounds = scene.TileBounds;
            int right = tileBounds.Right;
            tileBounds = scene.TileBounds;
            int bottom = tileBounds.Bottom;
            Point[] pointArray = new Point[4]
            {
                new Point(0, -1),
                new Point(0, 1),
                new Point(-1, 0),
                new Point(1, 0)
            };
            foreach (SeekerBarrier parent in list)
            {
                for (int x = (int) parent.X / 8; x < parent.Right / 8.0; ++x)
                {
                    for (int y = (int) parent.Y / 8; y < parent.Bottom / 8.0; ++y)
                    {
                        foreach (Point point1 in pointArray)
                        {
                            Point point2 = new Point(-point1.Y, point1.X);
                            if (!Inside(x + point1.X, y + point1.Y) && (!Inside(x - point2.X, y - point2.Y) || Inside(x + point1.X - point2.X, y + point1.Y - point2.Y)))
                            {
                                Point point3 = new Point(x, y);
                                Point point4 = new Point(x + point2.X, y + point2.Y);
                                Vector2 vector2 = new Vector2(4f) + new Vector2(point1.X - point2.X, point1.Y - point2.Y) * 4f;
                                for (; Inside(point4.X, point4.Y) && !Inside(point4.X + point1.X, point4.Y + point1.Y); point4.Y += point2.Y)
                                    point4.X += point2.X;
                                Vector2 a = new Vector2(point3.X, point3.Y) * 8f + vector2 - parent.Position;
                                Vector2 b = new Vector2(point4.X, point4.Y) * 8f + vector2 - parent.Position;
                                edges.Add(new Edge(parent, a, b));
                            }
                        }
                    }
                }
            }
        }

        private bool Inside(int tx, int ty) => tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];

        private void OnRenderBloom()
        {
            Camera camera = (Scene as Level).Camera;
            Rectangle rectangle = new Rectangle((int) camera.Left, (int) camera.Top, (int) (camera.Right - (double) camera.Left), (int) (camera.Bottom - (double) camera.Top));
            foreach (SeekerBarrier seekerBarrier in list)
            {
                if (seekerBarrier.Visible)
                    Draw.Rect(seekerBarrier.X, seekerBarrier.Y, seekerBarrier.Width, seekerBarrier.Height, Color.White);
            }
            foreach (Edge edge in edges)
            {
                if (edge.Visible)
                {
                    Vector2 vector2_1 = edge.Parent.Position + edge.A;
                    Vector2 vector2_2 = edge.Parent.Position + edge.B;
                    for (int index = 0; index <= (double) edge.Length; ++index)
                    {
                        Vector2 start = vector2_1 + edge.Normal * index;
                        Draw.Line(start, start + edge.Perpendicular * edge.Wave[index], Color.White);
                    }
                }
            }
        }

        public override void Render()
        {
            if (list.Count <= 0)
                return;
            Color color1 = Color.White * 0.15f;
            Color color2 = Color.White * 0.25f;
            foreach (SeekerBarrier seekerBarrier in list)
            {
                if (seekerBarrier.Visible)
                    Draw.Rect(seekerBarrier.Collider, color1);
            }
            if (edges.Count <= 0)
                return;
            foreach (Edge edge in edges)
            {
                if (edge.Visible)
                {
                    Vector2 vector2_1 = edge.Parent.Position + edge.A;
                    Vector2 vector2_2 = edge.Parent.Position + edge.B;
                    Color.Lerp(color2, Color.White, edge.Parent.Flash);
                    for (int index = 0; index <= (double) edge.Length; ++index)
                    {
                        Vector2 start = vector2_1 + edge.Normal * index;
                        Draw.Line(start, start + edge.Perpendicular * edge.Wave[index], color1);
                    }
                }
            }
        }

        private class Edge
        {
            public SeekerBarrier Parent;
            public bool Visible;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;

            public Edge(SeekerBarrier parent, Vector2 a, Vector2 b)
            {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                Normal = (b - a).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (a - b).Length();
            }

            public void UpdateWave(float time)
            {
                if (Wave == null || Wave.Length <= (double) Length)
                    Wave = new float[(int) Length + 2];
                for (int along = 0; along <= (double) Length; ++along)
                    Wave[along] = GetWaveAt(time, along, Length);
            }

            private float GetWaveAt(float offset, float along, float length)
            {
                if (along <= 1.0 || along >= length - 1.0 || Parent.Solidify >= 1.0)
                    return 0.0f;
                float a = offset + along * 0.25f;
                return (float) ((1.0 + (Math.Sin(a) * 2.0 + Math.Sin(a * 0.25)) * Ease.SineInOut(Calc.YoYo(along / length))) * (1.0 - Parent.Solidify));
            }

            public bool InView(ref Rectangle view) => view.Left < Parent.X + (double) Max.X && view.Right > Parent.X + (double) Min.X && view.Top < Parent.Y + (double) Max.Y && view.Bottom > Parent.Y + (double) Min.Y;
        }
    }
}
