// Decompiled with JetBrains decompiler
// Type: Celeste.DisplacementRenderer
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
    public class DisplacementRenderer : Monocle.Renderer
    {
        public bool Enabled = true;
        private float timer;
        private readonly List<DisplacementRenderer.Burst> points = new();

        public bool HasDisplacement(Scene scene)
        {
            return points.Count > 0 || scene.Tracker.GetComponent<DisplacementRenderHook>() != null || (scene as Level).Foreground.Get<HeatWave>() != null;
        }

        public DisplacementRenderer.Burst Add(DisplacementRenderer.Burst point)
        {
            points.Add(point);
            return point;
        }

        public DisplacementRenderer.Burst Remove(DisplacementRenderer.Burst point)
        {
            _ = points.Remove(point);
            return point;
        }

        public DisplacementRenderer.Burst AddBurst(
            Vector2 position,
            float duration,
            float radiusFrom,
            float radiusTo,
            float alpha = 1f,
            Ease.Easer alphaEaser = null,
            Ease.Easer radiusEaser = null)
        {
            MTexture texture = GFX.Game["util/displacementcircle"];
            return Add(new DisplacementRenderer.Burst(texture, position, texture.Center, duration)
            {
                ScaleFrom = radiusFrom / (texture.Width / 2),
                ScaleTo = radiusTo / (texture.Width / 2),
                AlphaFrom = alpha,
                AlphaTo = 0.0f,
                AlphaEaser = alphaEaser
            });
        }

        public override void Update(Scene scene)
        {
            timer += Engine.DeltaTime;
            for (int index = points.Count - 1; index >= 0; --index)
            {
                if (points[index].Percent >= 1.0)
                {
                    points.RemoveAt(index);
                }
                else
                {
                    points[index].Update();
                }
            }
        }

        public void Clear()
        {
            points.Clear();
        }

        public override void BeforeRender(Scene scene)
        {
            Distort.WaterSine = timer * 16f;
            Distort.WaterCameraY = (int)Math.Floor((double)(scene as Level).Camera.Y);
            Camera camera = (scene as Level).Camera;
            Color color = new(0.5f, 0.5f, 0.0f, 1f);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Displacement.Target);
            Engine.Graphics.GraphicsDevice.Clear(color);
            if (!Enabled)
            {
                return;
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
            (scene as Level).Foreground.Get<HeatWave>()?.RenderDisplacement(scene as Level);
            foreach (DisplacementRenderHook component in scene.Tracker.GetComponents<DisplacementRenderHook>())
            {
                if (component.Visible && component.RenderDisplacement != null)
                {
                    component.RenderDisplacement();
                }
            }
            foreach (DisplacementRenderer.Burst point in points)
            {
                point.Render();
            }

            foreach (Entity entity in scene.Tracker.GetEntities<FakeWall>())
            {
                Draw.Rect(entity.X, entity.Y, entity.Width, entity.Height, color);
            }

            Draw.SpriteBatch.End();
        }

        public class Burst
        {
            public MTexture Texture;
            public Entity Follow;
            public Vector2 Position;
            public Vector2 Origin;
            public float Duration;
            public float Percent;
            public float ScaleFrom;
            public float ScaleTo = 1f;
            public Ease.Easer ScaleEaser;
            public float AlphaFrom = 1f;
            public float AlphaTo;
            public Ease.Easer AlphaEaser;
            public Rectangle? WorldClipRect;
            public Collider WorldClipCollider;
            public int WorldClipPadding;

            public Burst(MTexture texture, Vector2 position, Vector2 origin, float duration)
            {
                Texture = texture;
                Position = position;
                Origin = origin;
                Duration = duration;
            }

            public void Update()
            {
                Percent += Engine.DeltaTime / Duration;
            }

            public void Render()
            {
                Vector2 position = Position;
                if (Follow != null)
                {
                    position += Follow.Position;
                }

                float num1 = AlphaEaser == null ? AlphaFrom + ((AlphaTo - AlphaFrom) * Percent) : AlphaFrom + ((AlphaTo - AlphaFrom) * AlphaEaser(Percent));
                float num2 = ScaleEaser == null ? ScaleFrom + ((ScaleTo - ScaleFrom) * Percent) : ScaleFrom + ((ScaleTo - ScaleFrom) * ScaleEaser(Percent));
                Vector2 origin = Origin;
                Rectangle clip = new(0, 0, Texture.Width, Texture.Height);
                if (WorldClipCollider != null)
                {
                    WorldClipRect = new Rectangle?(WorldClipCollider.Bounds);
                }

                if (WorldClipRect.HasValue)
                {
                    Rectangle rectangle = WorldClipRect.Value;
                    rectangle.X -= 1 + WorldClipPadding;
                    rectangle.Y -= 1 + WorldClipPadding;
                    rectangle.Width += 1 + (WorldClipPadding * 2);
                    rectangle.Height += 1 + (WorldClipPadding * 2);
                    float num3 = position.X - (origin.X * num2);
                    if ((double)num3 < rectangle.Left)
                    {
                        int num4 = (int)((rectangle.Left - (double)num3) / (double)num2);
                        origin.X -= num4;
                        clip.X = num4;
                        clip.Width -= num4;
                    }
                    float num5 = position.Y - (origin.Y * num2);
                    if ((double)num5 < rectangle.Top)
                    {
                        int num6 = (int)((rectangle.Top - (double)num5) / (double)num2);
                        origin.Y -= num6;
                        clip.Y = num6;
                        clip.Height -= num6;
                    }
                    float num7 = position.X + ((Texture.Width - origin.X) * num2);
                    if ((double)num7 > rectangle.Right)
                    {
                        int num8 = (int)(((double)num7 - rectangle.Right) / (double)num2);
                        clip.Width -= num8;
                    }
                    float num9 = position.Y + ((Texture.Height - origin.Y) * num2);
                    if ((double)num9 > rectangle.Bottom)
                    {
                        int num10 = (int)(((double)num9 - rectangle.Bottom) / (double)num2);
                        clip.Height -= num10;
                    }
                }
                Texture.Draw(position, origin, Color.White * num1, Vector2.One * num2, 0.0f, clip);
            }
        }
    }
}
