﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class SinkingPlatform : JumpThru
    {
        private float speed;
        private float startY;
        private float riseTimer;
        private MTexture[] textures;
        private Shaker shaker;
        private SoundSource downSfx;
        private SoundSource upSfx;

        public SinkingPlatform(Vector2 position, int width)
            : base(position, width, false)
        {
            startY = Y;
            Depth = 1;
            SurfaceSoundIndex = 15;
            Add(shaker = new Shaker(false));
            Add(new LightOcclude(0.2f));
            Add(downSfx = new SoundSource());
            Add(upSfx = new SoundSource());
        }

        public SinkingPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture mtexture = GFX.Game["objects/woodPlatform/" + AreaData.Get(scene).WoodPlatform];
            textures = new MTexture[mtexture.Width / 8];
            for (int index = 0; index < textures.Length; ++index)
                textures[index] = mtexture.GetSubtexture(index * 8, 0, 8, 8);
            scene.Add(new SinkingPlatformLine(Position + new Vector2(Width / 2f, Height / 2f)));
        }

        public override void Render()
        {
            Vector2 vector2 = shaker.Value;
            textures[0].Draw(Position + vector2);
            for (int x = 8; x < Width - 8.0; x += 8)
                textures[1].Draw(Position + vector2 + new Vector2(x, 0.0f));
            textures[3].Draw(Position + vector2 + new Vector2(Width - 8f, 0.0f));
            textures[2].Draw(Position + vector2 + new Vector2((float) (Width / 2.0 - 4.0), 0.0f));
        }

        public override void Update()
        {
            base.Update();
            Player playerRider = GetPlayerRider();
            if (playerRider != null)
            {
                if (riseTimer <= 0.0)
                {
                    if (ExactPosition.Y <= (double) startY)
                        Audio.Play("event:/game/03_resort/platform_vert_start", Position);
                    shaker.ShakeFor(0.15f, false);
                }
                riseTimer = 0.1f;
                speed = Calc.Approach(speed, playerRider.Ducking ? 60f : 30f, 400f * Engine.DeltaTime);
            }
            else if (riseTimer > 0.0)
            {
                riseTimer -= Engine.DeltaTime;
                speed = Calc.Approach(speed, 45f, 600f * Engine.DeltaTime);
            }
            else
                speed = Calc.Approach(speed, -50f, 400f * Engine.DeltaTime);
            if (speed > 0.0)
            {
                if (!downSfx.Playing)
                    downSfx.Play("event:/game/03_resort/platform_vert_down_loop");
                downSfx.Param("ducking", playerRider == null || !playerRider.Ducking ? 0.0f : 1f);
                if (upSfx.Playing)
                    upSfx.Stop();
                MoveV(speed * Engine.DeltaTime);
            }
            else
            {
                if (speed >= 0.0 || ExactPosition.Y <= (double) startY)
                    return;
                if (!upSfx.Playing)
                    upSfx.Play("event:/game/03_resort/platform_vert_up_loop");
                if (downSfx.Playing)
                    downSfx.Stop();
                MoveTowardsY(startY, -speed * Engine.DeltaTime);
                if (ExactPosition.Y > (double) startY)
                    return;
                upSfx.Stop();
                Audio.Play("event:/game/03_resort/platform_vert_end", Position);
                shaker.ShakeFor(0.1f, false);
            }
        }
    }
}
