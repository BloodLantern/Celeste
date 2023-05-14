// Decompiled with JetBrains decompiler
// Type: Celeste.BigWaterfall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class BigWaterfall : Entity
    {
        private readonly Layers layer;
        private readonly float width;
        private readonly float height;
        private readonly float parallax;
        private readonly List<float> lines = new();
        private Color surfaceColor;
        private Color fillColor;
        private float sine;
        private readonly SoundSource loopingSfx;
        private float fade;

        private Vector2 RenderPosition => RenderPositionAtCamera((Scene as Level).Camera.Position + new Vector2(160f, 90f));

        public BigWaterfall(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            layer = data.Enum(nameof(layer), Layers.BG);
            width = data.Width;
            height = data.Height;
            if (layer == Layers.FG)
            {
                Depth = -49900;
                parallax = 0.1f + (Calc.Random.NextFloat() * 0.2f);
                surfaceColor = Water.SurfaceColor;
                fillColor = Water.FillColor;
                Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
                lines.Add(3f);
                lines.Add(width - 4f);
                Add(loopingSfx = new SoundSource());
                _ = loopingSfx.Play("event:/env/local/waterfall_big_main");
            }
            else
            {
                Depth = 10010;
                parallax = -(0.7f + (Calc.Random.NextFloat() * 0.2f));
                surfaceColor = Calc.HexToColor("89dbf0") * 0.5f;
                fillColor = Calc.HexToColor("29a7ea") * 0.3f;
                lines.Add(6f);
                lines.Add(width - 7f);
            }
            fade = 1f;
            Add(new TransitionListener()
            {
                OnIn = f => fade = f,
                OnOut = f => fade = 1f - f
            });
            if (width <= 16)
            {
                return;
            }

            int num = Calc.Random.Next((int)(width / 16.0));
            for (int i = 0; i < num; ++i)
            {
                lines.Add(8f + Calc.Random.NextFloat(width - 16f));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!(Scene as Level).Transitioning)
            {
                return;
            }

            fade = 0;
        }

        public Vector2 RenderPositionAtCamera(Vector2 camera)
        {
            Vector2 vector2 = Position + (new Vector2(width, height) / 2f) - camera;
            Vector2 zero = Vector2.Zero;
            if (layer == Layers.BG)
            {
                zero -= vector2 * 0.6f;
            }
            else if (layer == Layers.FG)
            {
                zero += vector2 * 0.2f;
            }

            return Position + zero;
        }

        public void RenderDisplacement()
        {
            Draw.Rect(RenderPosition.X, Y, width, height, new Color(0.5f, 0.5f, 1f, 1f));
        }

        public override void Update()
        {
            sine += Engine.DeltaTime;
            if (loopingSfx != null)
            {
                loopingSfx.Position = new Vector2(RenderPosition.X - X, Calc.Clamp((Scene as Level).Camera.Position.Y + 90f, Y, height) - Y);
            }

            base.Update();
        }

        public override void Render()
        {
            float x = RenderPosition.X;
            Color color1 = fillColor * fade;
            Color color2 = surfaceColor * fade;
            Draw.Rect(x, Y, width, height, color1);
            if (layer == Layers.FG)
            {
                Draw.Rect(x - 1f, Y, 3f, height, color2);
                Draw.Rect(x + width - 2, Y, 3f, height, color2);
                foreach (float line in lines)
                {
                    Draw.Rect(x + line, Y, 1f, height, color2);
                }
            }
            else
            {
                Vector2 position = (Scene as Level).Camera.Position;
                int height = 3;
                float num1 = (float)Math.Max(Y, Math.Floor(position.Y / height) * height);
                float num2 = Math.Min(Y + this.height, position.Y + 180f);
                for (float y = num1; y < num2; y += height)
                {
                    int num3 = (int)Math.Sin((y / 6) - (sine * 8)) * 2;
                    Draw.Rect(x, y, 4 + num3, height, color2);
                    Draw.Rect(x + width - 4 + num3, y, 4 - num3, height, color2);
                    foreach (float line in lines)
                    {
                        Draw.Rect(x + num3 + line, y, 1f, height, color2);
                    }
                }
            }
        }

        private enum Layers
        {
            FG,
            BG,
        }
    }
}
