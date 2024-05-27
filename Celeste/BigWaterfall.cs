using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class BigWaterfall : Entity
    {
        private Layers layer;
        private float width;
        private float height;
        private float parallax;
        private List<float> lines = new List<float>();
        private Color surfaceColor;
        private Color fillColor;
        private float sine;
        private SoundSource loopingSfx;
        private float fade;

        private Vector2 RenderPosition => RenderPositionAtCamera((Scene as Level).Camera.Position + new Vector2(160f, 90f));

        public BigWaterfall(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = (int) Tags.TransitionUpdate;
            layer = data.Enum(nameof (layer), Layers.BG);
            width = data.Width;
            height = data.Height;
            if (layer == Layers.FG)
            {
                Depth = -49900;
                parallax = (float) (0.10000000149011612 + Calc.Random.NextFloat() * 0.20000000298023224);
                surfaceColor = Water.SurfaceColor;
                fillColor = Water.FillColor;
                Add(new DisplacementRenderHook(RenderDisplacement));
                lines.Add(3f);
                lines.Add(width - 4f);
                Add(loopingSfx = new SoundSource());
                loopingSfx.Play("event:/env/local/waterfall_big_main");
            }
            else
            {
                Depth = 10010;
                parallax = (float) -(0.699999988079071 + Calc.Random.NextFloat() * 0.20000000298023224);
                surfaceColor = Calc.HexToColor("89dbf0") * 0.5f;
                fillColor = Calc.HexToColor("29a7ea") * 0.3f;
                lines.Add(6f);
                lines.Add(width - 7f);
            }
            fade = 1f;
            Add(new TransitionListener
            {
                OnIn = f => fade = f,
                OnOut = f => fade = 1f - f
            });
            if (width <= 16.0)
                return;
            int num = Calc.Random.Next((int) (width / 16.0));
            for (int index = 0; index < num; ++index)
                lines.Add(8f + Calc.Random.NextFloat(width - 16f));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!(Scene as Level).Transitioning)
                return;
            fade = 0.0f;
        }

        public Vector2 RenderPositionAtCamera(Vector2 camera)
        {
            Vector2 vector2 = Position + new Vector2(width, height) / 2f - camera;
            Vector2 zero = Vector2.Zero;
            if (layer == Layers.BG)
                zero -= vector2 * 0.6f;
            else if (layer == Layers.FG)
                zero += vector2 * 0.2f;
            return Position + zero;
        }

        public void RenderDisplacement() => Draw.Rect(RenderPosition.X, Y, width, height, new Color(0.5f, 0.5f, 1f, 1f));

        public override void Update()
        {
            sine += Engine.DeltaTime;
            if (loopingSfx != null)
                loopingSfx.Position = new Vector2(RenderPosition.X - X, Calc.Clamp((Scene as Level).Camera.Position.Y + 90f, Y, height) - Y);
            base.Update();
        }

        public override void Render()
        {
            float x = RenderPosition.X;
            Color color1 = fillColor * fade;
            Color color2 = surfaceColor * fade;
            Draw.Rect(x, Y, width, this.height, color1);
            if (layer == Layers.FG)
            {
                Draw.Rect(x - 1f, Y, 3f, height, color2);
                Draw.Rect((float) (x + (double) width - 2.0), Y, 3f, height, color2);
                foreach (float line in lines)
                    Draw.Rect(x + line, Y, 1f, height, color2);
            }
            else
            {
                Vector2 position = (Scene as Level).Camera.Position;
                int height = 3;
                double num1 = Math.Max(Y, (float) Math.Floor(position.Y / (double) height) * height);
                float num2 = Math.Min(Y + this.height, position.Y + 180f);
                for (float y = (float) num1; y < (double) num2; y += height)
                {
                    int num3 = (int) (Math.Sin(y / 6.0 - sine * 8.0) * 2.0);
                    Draw.Rect(x, y, 4 + num3, height, color2);
                    Draw.Rect((float) (x + (double) width - 4.0) + num3, y, 4 - num3, height, color2);
                    foreach (float line in lines)
                        Draw.Rect(x + num3 + line, y, 1f, height, color2);
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
