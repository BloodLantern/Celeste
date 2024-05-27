using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class BridgeTile : JumpThru
    {
        private List<Image> images;
        private Vector2 shakeOffset;
        private float shakeTimer;
        private float speedY;
        private float colorLerp;

        public bool Fallen { get; private set; }

        public BridgeTile(Vector2 position, Rectangle tileSize)
            : base(position, tileSize.Width, false)
        {
            images = new List<Image>();
            if (tileSize.Width == 16)
            {
                int height = 24;
                int y = 0;
                while (y < tileSize.Height)
                {
                    Image image;
                    Add(image = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize.X, y, tileSize.Width, height)));
                    image.Origin = new Vector2(image.Width / 2f, 0.0f);
                    image.X = image.Width / 2f;
                    image.Y = y - 8;
                    images.Add(image);
                    y += height;
                    height = 12;
                }
            }
            else
            {
                Image image;
                Add(image = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize)));
                image.Origin = new Vector2(image.Width / 2f, 0.0f);
                image.X = image.Width / 2f;
                image.Y = -8f;
                images.Add(image);
            }
        }

        public override void Update()
        {
            base.Update();
            bool flag = images[0].Width == 16.0;
            if (!Fallen)
                return;
            if (shakeTimer > 0.0)
            {
                shakeTimer -= Engine.DeltaTime;
                if (Scene.OnInterval(0.02f))
                    shakeOffset = Calc.Random.ShakeVector();
                if (shakeTimer <= 0.0)
                {
                    Collidable = false;
                    SceneAs<Level>().Shake(0.1f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    if (flag)
                    {
                        Audio.Play("event:/game/00_prologue/bridge_support_break", Position);
                        foreach (Image image in images)
                        {
                            if (image.RenderPosition.Y > Y + 4.0)
                                Dust.Burst(image.RenderPosition, -1.57079637f, 8);
                        }
                    }
                }
                images[0].Position = new Vector2(images[0].Width / 2f, -8f) + shakeOffset;
            }
            else
            {
                colorLerp = Calc.Approach(colorLerp, 1f, 10f * Engine.DeltaTime);
                images[0].Color = Color.Lerp(Color.White, Color.Gray, colorLerp);
                shakeOffset = Vector2.Zero;
                if (flag)
                {
                    int num = 0;
                    foreach (Image image in images)
                    {
                        image.Rotation -= (float) ((num % 2 == 0 ? -1.0 : 1.0) * Engine.DeltaTime * num * 2.0);
                        image.Y += (float) (num * (double) Engine.DeltaTime * 16.0);
                        ++num;
                    }
                    speedY = Calc.Approach(speedY, 120f, 600f * Engine.DeltaTime);
                }
                else
                    speedY = Calc.Approach(speedY, 200f, 900f * Engine.DeltaTime);
                MoveV(speedY * Engine.DeltaTime);
                if (Top <= 220.0)
                    return;
                RemoveSelf();
            }
        }

        public void Fall(float timer = 0.2f)
        {
            if (Fallen)
                return;
            Fallen = true;
            shakeTimer = timer;
        }
    }
}
