using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class Parallax : Backdrop
    {
        public Vector2 CameraOffset = Vector2.Zero;
        public BlendState BlendState = BlendState.AlphaBlend;
        public MTexture Texture;
        public bool DoFadeIn;
        public float Alpha = 1f;
        private float fadeIn = 1f;

        public Parallax(MTexture texture)
        {
            Name = texture.AtlasPath;
            Texture = texture;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Position += Speed * Engine.DeltaTime;
            Position += WindMultiplier * (scene as Level).Wind * Engine.DeltaTime;
            if (DoFadeIn)
                fadeIn = Calc.Approach(fadeIn, Visible ? 1f : 0.0f, Engine.DeltaTime);
            else
                fadeIn = Visible ? 1f : 0.0f;
        }

        public override void Render(Scene scene)
        {
            Vector2 vector2_1 = ((scene as Level).Camera.Position + CameraOffset).Floor();
            Vector2 vector2_2 = (Position - vector2_1 * Scroll).Floor();
            float num = fadeIn * Alpha * FadeAlphaMultiplier;
            if (FadeX != null)
                num *= FadeX.Value(vector2_1.X + 160f);
            if (FadeY != null)
                num *= FadeY.Value(vector2_1.Y + 90f);
            Color color = Color;
            if (num < 1.0)
                color *= num;
            if (color.A <= 1)
                return;
            if (LoopX)
            {
                while (vector2_2.X < 0.0)
                    vector2_2.X += Texture.Width;
                while (vector2_2.X > 0.0)
                    vector2_2.X -= Texture.Width;
            }
            if (LoopY)
            {
                while (vector2_2.Y < 0.0)
                    vector2_2.Y += Texture.Height;
                while (vector2_2.Y > 0.0)
                    vector2_2.Y -= Texture.Height;
            }
            SpriteEffects flip = SpriteEffects.None;
            if (FlipX && FlipY)
                flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            else if (FlipX)
                flip = SpriteEffects.FlipHorizontally;
            else if (FlipY)
                flip = SpriteEffects.FlipVertically;
            for (float x = vector2_2.X; x < 320.0; x += Texture.Width)
            {
                for (float y = vector2_2.Y; y < 180.0; y += Texture.Height)
                {
                    Texture.Draw(new Vector2(x, y), Vector2.Zero, color, 1f, 0.0f, flip);
                    if (!LoopY)
                        break;
                }
                if (!LoopX)
                    break;
            }
        }
    }
}
