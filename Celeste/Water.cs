// Decompiled with JetBrains decompiler
// Type: Celeste.Water
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
    [Tracked(false)]
    public class Water : Entity
    {
        public static ParticleType P_Splash;
        public static readonly Color FillColor = Color.LightSkyBlue * 0.3f;
        public static readonly Color SurfaceColor = Color.LightSkyBlue * 0.8f;
        public static readonly Color RayTopColor = Color.LightSkyBlue * 0.6f;
        public static readonly Vector2 RayAngle = new Vector2(-4f, 8f).SafeNormalize();
        public Water.Surface TopSurface;
        public Water.Surface BottomSurface;
        public List<Water.Surface> Surfaces = new();
        private Rectangle fill;
        private readonly bool[,] grid;
        private Water.Tension playerBottomTension;
        private readonly HashSet<WaterInteraction> contains = new();

        public Water(EntityData data, Vector2 offset)
            : this(data.Position + offset, true, data.Bool("hasBottom"), data.Width, data.Height)
        {
        }

        public Water(
            Vector2 position,
            bool topSurface,
            bool bottomSurface,
            float width,
            float height)
        {
            Position = position;
            Tag = (int)Tags.TransitionUpdate;
            Depth = -9999;
            Collider = new Hitbox(width, height);
            grid = new bool[(int)((double)width / 8.0), (int)((double)height / 8.0)];
            fill = new Rectangle(0, 0, (int)width, (int)height);
            int y = 8;
            if (topSurface)
            {
                TopSurface = new Water.Surface(Position + new Vector2(width / 2f, y), new Vector2(0.0f, -1f), width, height);
                Surfaces.Add(TopSurface);
                fill.Y += y;
                fill.Height -= y;
            }
            if (bottomSurface)
            {
                BottomSurface = new Water.Surface(Position + new Vector2(width / 2f, height - y), new Vector2(0.0f, 1f), width, height);
                Surfaces.Add(BottomSurface);
                fill.Height -= y;
            }
            Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int index1 = 0;
            for (int length1 = grid.GetLength(0); index1 < length1; ++index1)
            {
                int index2 = 0;
                for (int length2 = grid.GetLength(1); index2 < length2; ++index2)
                {
                    grid[index1, index2] = !Scene.CollideCheck<Solid>(new Rectangle((int)X + (index1 * 8), (int)Y + (index2 * 8), 8, 8));
                }
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (Water.Surface surface in Surfaces)
            {
                surface.Update();
            }

            foreach (WaterInteraction component in Scene.Tracker.GetComponents<WaterInteraction>())
            {
                Entity entity = component.Entity;
                bool flag1 = contains.Contains(component);
                bool flag2 = CollideCheck(entity);
                if (flag1 != flag2)
                {
                    if (entity.Center.Y <= (double)Center.Y && TopSurface != null)
                    {
                        TopSurface.DoRipple(entity.Center, 1f);
                    }
                    else if (entity.Center.Y > (double)Center.Y && BottomSurface != null)
                    {
                        BottomSurface.DoRipple(entity.Center, 1f);
                    }

                    bool flag3 = component.IsDashing();
                    int num = entity.Center.Y >= (double)Center.Y || Scene.CollideCheck<Solid>(new Rectangle((int)entity.Center.X - 4, (int)entity.Center.Y, 8, 16)) ? 0 : 1;
                    if (flag1)
                    {
                        _ = flag3
                            ? Audio.Play("event:/char/madeline/water_dash_out", entity.Center, "deep", num)
                            : Audio.Play("event:/char/madeline/water_out", entity.Center, "deep", num);

                        component.DrippingTimer = 2f;
                    }
                    else
                    {
                        _ = flag3 && num == 1
                            ? Audio.Play("event:/char/madeline/water_dash_in", entity.Center, "deep", num)
                            : Audio.Play("event:/char/madeline/water_in", entity.Center, "deep", num);

                        component.DrippingTimer = 0.0f;
                    }
                    _ = flag1 ? contains.Remove(component) : contains.Add(component);
                }
                if (BottomSurface != null && entity is Player)
                {
                    if (flag2 && (double)entity.Y > (double)Bottom - 8.0)
                    {
                        playerBottomTension ??= BottomSurface.SetTension(entity.Position, 0.0f);
                        playerBottomTension.Position = BottomSurface.GetPointAlong(entity.Position);
                        playerBottomTension.Strength = Calc.ClampedMap(entity.Y, Bottom - 8f, Bottom + 4f);
                    }
                    else if (playerBottomTension != null)
                    {
                        BottomSurface.RemoveTension(playerBottomTension);
                        playerBottomTension = null;
                    }
                }
            }
        }

        public void RenderDisplacement()
        {
            Color color = new(0.5f, 0.5f, 0.25f, 1f);
            int index1 = 0;
            int length1 = grid.GetLength(0);
            int length2 = grid.GetLength(1);
            for (; index1 < length1; ++index1)
            {
                if (length2 > 0 && grid[index1, 0])
                {
                    Draw.Rect(X + (index1 * 8), Y + 3f, 8f, 5f, color);
                }

                for (int index2 = 1; index2 < length2; ++index2)
                {
                    if (grid[index1, index2])
                    {
                        int num = 1;
                        while (index2 + num < length2 && grid[index1, index2 + num])
                        {
                            ++num;
                        }

                        Draw.Rect(X + (index1 * 8), Y + (index2 * 8), 8f, num * 8, color);
                        index2 += num - 1;
                    }
                }
            }
        }

        public override void Render()
        {
            Draw.Rect(X + fill.X, Y + fill.Y, fill.Width, fill.Height, Water.FillColor);
            GameplayRenderer.End();
            foreach (Water.Surface surface in Surfaces)
            {
                surface.Render((Scene as Level).Camera);
            }

            GameplayRenderer.Begin();
        }

        public class Ripple
        {
            public float Position;
            public float Speed;
            public float Height;
            public float Percent;
            public float Duration;
        }

        public class Tension
        {
            public float Position;
            public float Strength;
        }

        public class Ray
        {
            public float Position;
            public float Percent;
            public float Duration;
            public float Width;
            public float Length;
            private readonly float MaxWidth;

            public Ray(float maxWidth)
            {
                MaxWidth = maxWidth;
                Reset(Calc.Random.NextFloat());
            }

            public void Reset(float percent)
            {
                Position = Calc.Random.NextFloat() * MaxWidth;
                Percent = percent;
                Duration = Calc.Random.Range(2f, 8f);
                Width = Calc.Random.Range(2, 16);
                Length = Calc.Random.Range(8f, 128f);
            }
        }

        public class Surface
        {
            public const int Resolution = 4;
            public const float RaysPerPixel = 0.2f;
            public const float BaseHeight = 6f;
            public readonly Vector2 Outwards;
            public readonly int Width;
            public readonly int BodyHeight;
            public Vector2 Position;
            public List<Water.Ripple> Ripples = new();
            public List<Water.Ray> Rays = new();
            public List<Water.Tension> Tensions = new();
            private float timer;
            private readonly VertexPositionColor[] mesh;
            private readonly int fillStartIndex;
            private readonly int rayStartIndex;
            private readonly int surfaceStartIndex;

            public Surface(Vector2 position, Vector2 outwards, float width, float bodyHeight)
            {
                Position = position;
                Outwards = outwards;
                Width = (int)width;
                BodyHeight = (int)bodyHeight;
                int num1 = (int)((double)width / 4.0);
                int num2 = (int)((double)width * 0.20000000298023224);
                Rays = new List<Water.Ray>();
                for (int index = 0; index < num2; ++index)
                {
                    Rays.Add(new Water.Ray(width));
                }

                fillStartIndex = 0;
                rayStartIndex = num1 * 6;
                surfaceStartIndex = (num1 + num2) * 6;
                mesh = new VertexPositionColor[((num1 * 2) + num2) * 6];
                for (int fillStartIndex = this.fillStartIndex; fillStartIndex < this.fillStartIndex + (num1 * 6); ++fillStartIndex)
                {
                    mesh[fillStartIndex].Color = Water.FillColor;
                }

                for (int rayStartIndex = this.rayStartIndex; rayStartIndex < this.rayStartIndex + (num2 * 6); ++rayStartIndex)
                {
                    mesh[rayStartIndex].Color = Color.Transparent;
                }

                for (int surfaceStartIndex = this.surfaceStartIndex; surfaceStartIndex < this.surfaceStartIndex + (num1 * 6); ++surfaceStartIndex)
                {
                    mesh[surfaceStartIndex].Color = Water.SurfaceColor;
                }
            }

            public float GetPointAlong(Vector2 position)
            {
                Vector2 vector2 = Outwards.Perpendicular();
                Vector2 lineA = Position + (vector2 * (-Width / 2));
                return (lineA - Calc.ClosestPointOnLine(lineA, Position + (vector2 * (Width / 2)), position)).Length();
            }

            public Water.Tension SetTension(Vector2 position, float strength)
            {
                Water.Tension tension = new()
                {
                    Position = GetPointAlong(position),
                    Strength = strength
                };
                Tensions.Add(tension);
                return tension;
            }

            public void RemoveTension(Water.Tension tension)
            {
                _ = Tensions.Remove(tension);
            }

            public void DoRipple(Vector2 position, float multiplier)
            {
                float num1 = 80f;
                float num2 = 3f;
                float pointAlong = GetPointAlong(position);
                int num3 = 2;
                if (Width < 200)
                {
                    num2 *= Calc.ClampedMap(Width, 0.0f, 200f, 0.25f);
                    multiplier *= Calc.ClampedMap(Width, 0.0f, 200f, 0.5f);
                }
                Ripples.Add(new Water.Ripple()
                {
                    Position = pointAlong,
                    Speed = -num1,
                    Height = num3 * multiplier,
                    Percent = 0.0f,
                    Duration = num2
                });
                Ripples.Add(new Water.Ripple()
                {
                    Position = pointAlong,
                    Speed = num1,
                    Height = num3 * multiplier,
                    Percent = 0.0f,
                    Duration = num2
                });
            }

            public void Update()
            {
                timer += Engine.DeltaTime;
                Vector2 vector2_1 = Outwards.Perpendicular();
                for (int index = Ripples.Count - 1; index >= 0; --index)
                {
                    Water.Ripple ripple = Ripples[index];
                    if (ripple.Percent > 1.0)
                    {
                        Ripples.RemoveAt(index);
                    }
                    else
                    {
                        ripple.Position += ripple.Speed * Engine.DeltaTime;
                        if (ripple.Position < 0.0 || ripple.Position > (double)Width)
                        {
                            ripple.Speed = -ripple.Speed;
                            ripple.Position = Calc.Clamp(ripple.Position, 0.0f, Width);
                        }
                        ripple.Percent += Engine.DeltaTime / ripple.Duration;
                    }
                }
                int num1 = 0;
                int fillStartIndex = this.fillStartIndex;
                int surfaceStartIndex = this.surfaceStartIndex;
                while (num1 < Width)
                {
                    int position1 = num1;
                    float surfaceHeight1 = GetSurfaceHeight(position1);
                    int position2 = Math.Min(num1 + 4, Width);
                    float surfaceHeight2 = GetSurfaceHeight(position2);
                    mesh[fillStartIndex].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)) + (Outwards * surfaceHeight1), 0.0f);
                    mesh[fillStartIndex + 1].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)) + (Outwards * surfaceHeight2), 0.0f);
                    mesh[fillStartIndex + 2].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)), 0.0f);
                    mesh[fillStartIndex + 3].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)) + (Outwards * surfaceHeight2), 0.0f);
                    mesh[fillStartIndex + 4].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)), 0.0f);
                    mesh[fillStartIndex + 5].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)), 0.0f);
                    mesh[surfaceStartIndex].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)) + (Outwards * (surfaceHeight1 + 1f)), 0.0f);
                    mesh[surfaceStartIndex + 1].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)) + (Outwards * (surfaceHeight2 + 1f)), 0.0f);
                    mesh[surfaceStartIndex + 2].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)) + (Outwards * surfaceHeight1), 0.0f);
                    mesh[surfaceStartIndex + 3].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)) + (Outwards * (surfaceHeight2 + 1f)), 0.0f);
                    mesh[surfaceStartIndex + 4].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position2)) + (Outwards * surfaceHeight2), 0.0f);
                    mesh[surfaceStartIndex + 5].Position = new Vector3(Position + (vector2_1 * ((-Width / 2) + position1)) + (Outwards * surfaceHeight1), 0.0f);
                    num1 += 4;
                    fillStartIndex += 6;
                    surfaceStartIndex += 6;
                }
                Vector2 vector2_2 = Position + (vector2_1 * (-Width / 2f));
                int rayStartIndex = this.rayStartIndex;
                foreach (Water.Ray ray in Rays)
                {
                    if (ray.Percent > 1.0)
                    {
                        ray.Reset(0.0f);
                    }

                    ray.Percent += Engine.DeltaTime / ray.Duration;
                    float num2 = 1f;
                    if (ray.Percent < 0.10000000149011612)
                    {
                        num2 = Calc.ClampedMap(ray.Percent, 0.0f, 0.1f);
                    }
                    else if (ray.Percent > 0.89999997615814209)
                    {
                        num2 = Calc.ClampedMap(ray.Percent, 0.9f, 1f, 1f, 0.0f);
                    }

                    float position3 = Math.Max(0.0f, ray.Position - (ray.Width / 2f));
                    float position4 = Math.Min(Width, ray.Position + (ray.Width / 2f));
                    float num3 = Math.Min(BodyHeight, 0.7f * ray.Length);
                    float num4 = 0.3f * ray.Length;
                    Vector2 vector2_3 = vector2_2 + (vector2_1 * position3) + (Outwards * GetSurfaceHeight(position3));
                    Vector2 vector2_4 = vector2_2 + (vector2_1 * position4) + (Outwards * GetSurfaceHeight(position4));
                    Vector2 vector2_5 = vector2_2 + (vector2_1 * (position4 - num4)) - (Outwards * num3);
                    Vector2 vector2_6 = vector2_2 + (vector2_1 * (position3 - num4)) - (Outwards * num3);
                    mesh[rayStartIndex].Position = new Vector3(vector2_3, 0.0f);
                    mesh[rayStartIndex].Color = Water.RayTopColor * num2;
                    mesh[rayStartIndex + 1].Position = new Vector3(vector2_4, 0.0f);
                    mesh[rayStartIndex + 1].Color = Water.RayTopColor * num2;
                    mesh[rayStartIndex + 2].Position = new Vector3(vector2_6, 0.0f);
                    mesh[rayStartIndex + 3].Position = new Vector3(vector2_4, 0.0f);
                    mesh[rayStartIndex + 3].Color = Water.RayTopColor * num2;
                    mesh[rayStartIndex + 4].Position = new Vector3(vector2_5, 0.0f);
                    mesh[rayStartIndex + 5].Position = new Vector3(vector2_6, 0.0f);
                    rayStartIndex += 6;
                }
            }

            public float GetSurfaceHeight(Vector2 position)
            {
                return GetSurfaceHeight(GetPointAlong(position));
            }

            public float GetSurfaceHeight(float position)
            {
                if ((double)position < 0.0 || (double)position > Width)
                {
                    return 0.0f;
                }

                float num1 = 0.0f;
                foreach (Water.Ripple ripple in Ripples)
                {
                    float val = Math.Abs(ripple.Position - position);
                    float num2 = (double)val >= 12.0 ? Calc.ClampedMap(val, 16f, 32f, -0.75f, 0.0f) : Calc.ClampedMap(val, 0.0f, 16f, 1f, -0.75f);
                    num1 += num2 * ripple.Height * Ease.CubeIn(1f - ripple.Percent);
                }
                float num3 = Calc.Clamp(num1, -4f, 4f);
                foreach (Water.Tension tension in Tensions)
                {
                    float t = Calc.ClampedMap(Math.Abs(tension.Position - position), 0.0f, 24f, 1f, 0.0f);
                    num3 += (float)((double)Ease.CubeOut(t) * tension.Strength * 12.0);
                }
                float val1 = position / Width;
                return (num3 * Calc.ClampedMap(val1, 0.0f, 0.1f, 0.5f) * Calc.ClampedMap(val1, 0.9f, 1f, 1f, 0.5f)) + (float)Math.Sin(timer + ((double)position * 0.10000000149011612)) + 6f;
            }

            public void Render(Camera camera)
            {
                GFX.DrawVertices<VertexPositionColor>(camera.Matrix, mesh, mesh.Length);
            }
        }
    }
}
