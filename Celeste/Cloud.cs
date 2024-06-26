﻿using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Cloud : JumpThru
    {
        public static ParticleType P_Cloud;
        public static ParticleType P_FragileCloud;
        private Sprite sprite;
        private Wiggler wiggler;
        private ParticleType particleType;
        private SoundSource sfx;
        private bool waiting = true;
        private float speed;
        private float startY;
        private float respawnTimer;
        private bool returning;
        private bool fragile;
        private float timer;
        private Vector2 scale;
        private bool canRumble;

        public Cloud(Vector2 position, bool fragile)
            : base(position, 32, false)
        {
            this.fragile = fragile;
            startY = Y;
            Collider.Position.X = -16f;
            timer = Calc.Random.NextFloat() * 4f;
            Add(wiggler = Wiggler.Create(0.3f, 4f));
            particleType = fragile ? Cloud.P_FragileCloud : Cloud.P_Cloud;
            SurfaceSoundIndex = 4;
            Add(new LightOcclude(0.2f));
            scale = Vector2.One;
            Add(sfx = new SoundSource());
        }

        public Cloud(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool(nameof (fragile)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string id = fragile ? "cloudFragile" : "cloud";
            if (SceneAs<Level>().Session.Area.Mode != AreaMode.Normal)
            {
                Collider.Position.X += 2f;
                Collider.Width -= 6f;
                id += "Remix";
            }
            Add(sprite = GFX.SpriteBank.Create(id));
            sprite.Origin = new Vector2(sprite.Width / 2f, 8f);
            sprite.OnFrameChange = s =>
            {
                if (!(s == "spawn") || sprite.CurrentAnimationFrame != 6)
                    return;
                wiggler.Start();
            };
        }

        public override void Update()
        {
            base.Update();
            scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
            scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
            timer += Engine.DeltaTime;
            if (GetPlayerRider() != null)
                sprite.Position = Vector2.Zero;
            else
                sprite.Position = Calc.Approach(sprite.Position, new Vector2(0.0f, (float) Math.Sin(timer * 2.0)), Engine.DeltaTime * 4f);
            if (respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer > 0.0)
                    return;
                waiting = true;
                Y = startY;
                speed = 0.0f;
                scale = Vector2.One;
                Collidable = true;
                sprite.Play("spawn");
                sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
            }
            else if (waiting)
            {
                Player playerRider = GetPlayerRider();
                if (playerRider == null || playerRider.Speed.Y < 0.0)
                    return;
                canRumble = true;
                speed = 180f;
                scale = new Vector2(1.3f, 0.7f);
                waiting = false;
                if (fragile)
                    Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                else
                    Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);
            }
            else if (returning)
            {
                speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
                MoveTowardsY(startY, speed * Engine.DeltaTime);
                if (ExactPosition.Y != (double) startY)
                    return;
                returning = false;
                waiting = true;
                speed = 0.0f;
            }
            else
            {
                if (fragile && Collidable && !HasPlayerRider())
                {
                    Collidable = false;
                    sprite.Play("fade");
                }
                if (speed < 0.0 && canRumble)
                {
                    canRumble = false;
                    if (HasPlayerRider())
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                if (speed < 0.0 && Scene.OnInterval(0.02f))
                    (Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(0.0f, 2f), new Vector2(Collider.Width / 2f, 1f), 1.57079637f);
                if (fragile && speed < 0.0)
                    sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0.0f, Engine.DeltaTime * 4f);
                if (Y >= (double) startY)
                {
                    speed -= 1200f * Engine.DeltaTime;
                }
                else
                {
                    speed += 1200f * Engine.DeltaTime;
                    if (speed >= -100.0)
                    {
                        Player playerRider = GetPlayerRider();
                        if (playerRider != null && playerRider.Speed.Y >= 0.0)
                            playerRider.Speed.Y = -200f;
                        if (fragile)
                        {
                            Collidable = false;
                            sprite.Play("fade");
                            respawnTimer = 2.5f;
                        }
                        else
                        {
                            scale = new Vector2(0.7f, 1.3f);
                            returning = true;
                        }
                    }
                }
                float liftSpeedV = speed;
                if (liftSpeedV < 0.0)
                    liftSpeedV = -220f;
                MoveV(speed * Engine.DeltaTime, liftSpeedV);
            }
        }

        public override void Render()
        {
            sprite.Scale = scale * (float) (1.0 + 0.10000000149011612 * wiggler.Value);
            base.Render();
        }
    }
}
