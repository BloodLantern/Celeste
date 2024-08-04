using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monocle
{
    public struct Particle
    {
        public Entity Track;
        public ParticleType Type;
        public MTexture Source;
        public bool Active;
        public Color Color;
        public Color StartColor;
        public Vector2 Position;
        public Vector2 Speed;
        public float Size;
        public float StartSize;
        public float Life;
        public float StartLife;
        public float ColorSwitch;
        public float Rotation;
        public float Spin;

        public bool SimulateFor(float duration)
        {
            if (duration > (double) Life)
            {
                Life = 0.0f;
                Active = false;
                return false;
            }
            float num1 = Engine.TimeRate * (Engine.Instance.TargetElapsedTime.Milliseconds / 1000f);
            if (num1 > 0.0)
            {
                for (float num2 = 0.0f; num2 < (double) duration; num2 += num1)
                    Update(num1);
            }
            return true;
        }

        public void Update(float? delta = null)
        {
            float y = !delta.HasValue ? (Type.UseActualDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) : delta.Value;
            float num1 = Life / StartLife;
            Life -= y;
            if (Life <= 0.0)
            {
                Active = false;
            }
            else
            {
                if (Type.RotationMode == ParticleType.RotationModes.SameAsDirection)
                {
                    if (Speed != Vector2.Zero)
                        Rotation = Speed.Angle();
                }
                else
                    Rotation += Spin * y;
                float num2 = Type.FadeMode != ParticleType.FadeModes.Linear ? (Type.FadeMode != ParticleType.FadeModes.Late ? (Type.FadeMode != ParticleType.FadeModes.InAndOut ? 1f : (num1 <= 0.75 ? (num1 >= 0.25 ? 1f : num1 / 0.25f) : (float) (1.0 - (num1 - 0.75) / 0.25))) : Math.Min(1f, num1 / 0.25f)) : num1;
                if (num2 == 0.0)
                {
                    Color = Color.Transparent;
                }
                else
                {
                    if (Type.ColorMode == ParticleType.ColorModes.Static)
                        Color = StartColor;
                    else if (Type.ColorMode == ParticleType.ColorModes.Fade)
                        Color = Color.Lerp(Type.Color2, StartColor, num1);
                    else if (Type.ColorMode == ParticleType.ColorModes.Blink)
                        Color = Calc.BetweenInterval(Life, 0.1f) ? StartColor : Type.Color2;
                    else if (Type.ColorMode == ParticleType.ColorModes.Choose)
                        Color = StartColor;
                    if (num2 < 1.0)
                        Color *= num2;
                }
                Position += Speed * y;
                Speed += Type.Acceleration * y;
                Speed = Calc.Approach(Speed, Vector2.Zero, Type.Friction * y);
                if (Type.SpeedMultiplier != 1.0)
                    Speed *= (float) Math.Pow(Type.SpeedMultiplier, y);
                if (!Type.ScaleOut)
                    return;
                Size = StartSize * Ease.CubeOut(num1);
            }
        }

        public void Render()
        {
            Vector2 position = new Vector2((int) Position.X, (int) Position.Y);
            if (Track != null)
                position += Track.Position;
            Draw.SpriteBatch.Draw(Source.Texture.Texture, position, Source.ClipRect, Color, Rotation, Source.Center, Size, SpriteEffects.None, 0.0f);
        }

        public void Render(float alpha)
        {
            Vector2 position = new Vector2((int) Position.X, (int) Position.Y);
            if (Track != null)
                position += Track.Position;
            Draw.SpriteBatch.Draw(Source.Texture.Texture, position, Source.ClipRect, Color * alpha, Rotation, Source.Center, Size, SpriteEffects.None, 0.0f);
        }
    }
}
