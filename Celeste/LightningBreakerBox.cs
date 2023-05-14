// Decompiled with JetBrains decompiler
// Type: Celeste.LightningBreakerBox
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class LightningBreakerBox : Solid
    {
        public static ParticleType P_Smash;
        public static ParticleType P_Sparks;
        private readonly Sprite sprite;
        private readonly SineWave sine;
        private Vector2 start;
        private float sink;
        private int health = 2;
        private readonly bool flag;
        private float shakeCounter;
        private readonly string music;
        private readonly int musicProgress = -1;
        private readonly bool musicStoreInSession;
        private Vector2 bounceDir;
        private readonly Wiggler bounce;
        private readonly Shaker shaker;
        private bool makeSparks;
        private bool smashParticles;
        private Coroutine pulseRoutine;
        private SoundSource firstHitSfx;
        private bool spikesLeft;
        private bool spikesRight;
        private bool spikesUp;
        private bool spikesDown;

        public LightningBreakerBox(Vector2 position, bool flipX)
            : base(position, 32f, 32f, true)
        {
            SurfaceSoundIndex = 9;
            start = Position;
            sprite = GFX.SpriteBank.Create("breakerBox");
            sprite.OnLastFrame += anim =>
            {
                if (anim == "break")
                {
                    Visible = false;
                }
                else
                {
                    if (!(anim == "open"))
                    {
                        return;
                    }

                    makeSparks = true;
                }
            };
            sprite.Position = new Vector2(Width, Height) / 2f;
            sprite.FlipX = flipX;
            Add(sprite);
            sine = new SineWave(0.5f);
            Add(sine);
            bounce = Wiggler.Create(1f, 0.5f);
            bounce.StartZero = false;
            Add(bounce);
            Add(shaker = new Shaker(false));
            OnDashCollide = new DashCollision(Dashed);
        }

        public LightningBreakerBox(EntityData e, Vector2 levelOffset)
            : this(e.Position + levelOffset, e.Bool("flipX"))
        {
            flag = e.Bool(nameof(flag));
            music = e.Attr(nameof(music), null);
            musicProgress = e.Int("music_progress", -1);
            musicStoreInSession = e.Bool("music_session");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
            spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
            spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
            spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);
        }

        public DashCollisionResults Dashed(Player player, Vector2 dir)
        {
            if (!SaveData.Instance.Assists.Invincible && ((dir == Vector2.UnitX && spikesLeft) || (dir == -Vector2.UnitX && spikesRight) || (dir == Vector2.UnitY && spikesUp) || (dir == -Vector2.UnitY && spikesDown)))
            {
                return DashCollisionResults.NormalCollision;
            } (Scene as Level).DirectionalShake(dir);
            sprite.Scale = new Vector2((float)(1.0 + ((double)Math.Abs(dir.Y) * 0.40000000596046448) - ((double)Math.Abs(dir.X) * 0.40000000596046448)), (float)(1.0 + ((double)Math.Abs(dir.X) * 0.40000000596046448) - ((double)Math.Abs(dir.Y) * 0.40000000596046448)));
            --health;
            if (health > 0)
            {
                Add(firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                Celeste.Freeze(0.1f);
                shakeCounter = 0.2f;
                shaker.On = true;
                bounceDir = dir;
                bounce.Start();
                smashParticles = true;
                Pulse();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
            {
                _ = (firstHitSfx?.Stop());
                _ = Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                Celeste.Freeze(0.2f);
                _ = player.RefillDash();
                Break();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                SmashParticles(dir.Perpendicular());
                SmashParticles(-dir.Perpendicular());
            }
            return DashCollisionResults.Rebound;
        }

        private void SmashParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (dir == Vector2.UnitX)
            {
                direction = 0.0f;
                position = CenterRight - (Vector2.UnitX * 12f);
                positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
                num = (int)((double)Height / 8.0) * 4;
            }
            else if (dir == -Vector2.UnitX)
            {
                direction = 3.14159274f;
                position = CenterLeft + (Vector2.UnitX * 12f);
                positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
                num = (int)((double)Height / 8.0) * 4;
            }
            else if (dir == Vector2.UnitY)
            {
                direction = 1.57079637f;
                position = BottomCenter - (Vector2.UnitY * 12f);
                positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
                num = (int)((double)Width / 8.0) * 4;
            }
            else
            {
                direction = -1.57079637f;
                position = TopCenter + (Vector2.UnitY * 12f);
                positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
                num = (int)((double)Width / 8.0) * 4;
            }
            int amount = num + 2;
            SceneAs<Level>().Particles.Emit(LightningBreakerBox.P_Smash, amount, position, positionRange, direction);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!flag || !(Scene as Level).Session.GetFlag("disable_lightning"))
            {
                return;
            }

            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (makeSparks && Scene.OnInterval(0.03f))
            {
                SceneAs<Level>().ParticlesFG.Emit(LightningBreakerBox.P_Sparks, 1, Center, Vector2.One * 12f);
            }

            if (shakeCounter > 0.0)
            {
                shakeCounter -= Engine.DeltaTime;
                if (shakeCounter <= 0.0)
                {
                    shaker.On = false;
                    sprite.Scale = Vector2.One * 1.2f;
                    sprite.Play("open");
                }
            }
            if (Collidable)
            {
                sink = Calc.Approach(sink, HasPlayerRider() ? 1f : 0.0f, 2f * Engine.DeltaTime);
                sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
                Vector2 start = this.start;
                start.Y += (float)((sink * 6.0) + ((double)sine.Value * (double)MathHelper.Lerp(4f, 2f, sink)));
                Vector2 vector2 = start + (bounce.Value * bounceDir * 12f);
                MoveToX(vector2.X);
                MoveToY(vector2.Y);
                if (smashParticles)
                {
                    smashParticles = false;
                    SmashParticles(bounceDir.Perpendicular());
                    SmashParticles(-bounceDir.Perpendicular());
                }
            }
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
            LiftSpeed = Vector2.Zero;
        }

        public override void Render()
        {
            Vector2 position = this.sprite.Position;
            Sprite sprite = this.sprite;
            sprite.Position += shaker.Value;
            base.Render();
            this.sprite.Position = position;
        }

        private void Pulse()
        {
            pulseRoutine = new Coroutine(Lightning.PulseRoutine(SceneAs<Level>()));
            Add(pulseRoutine);
        }

        private void Break()
        {
            Session session = (Scene as Level).Session;
            RumbleTrigger.ManuallyTrigger(Center.X, 1.2f);
            Tag = (int)Tags.Persistent;
            shakeCounter = 0.0f;
            shaker.On = false;
            sprite.Play("break");
            Collidable = false;
            DestroyStaticMovers();
            if (flag)
            {
                session.SetFlag("disable_lightning");
            }

            if (musicStoreInSession)
            {
                if (!string.IsNullOrEmpty(music))
                {
                    session.Audio.Music.Event = SFX.EventnameByHandle(music);
                }

                if (musicProgress >= 0)
                {
                    _ = session.Audio.Music.SetProgress(musicProgress);
                }

                session.Audio.Apply();
            }
            else
            {
                if (!string.IsNullOrEmpty(music))
                {
                    _ = Audio.SetMusic(SFX.EventnameByHandle(music), false);
                }

                if (musicProgress >= 0)
                {
                    Audio.SetMusicParam("progress", musicProgress);
                }

                if (!string.IsNullOrEmpty(music) && Audio.CurrentMusicEventInstance != null)
                {
                    int num = (int)Audio.CurrentMusicEventInstance.start();
                }
            }
            if (pulseRoutine != null)
            {
                pulseRoutine.Active = false;
            }

            Add(new Coroutine(Lightning.RemoveRoutine(SceneAs<Level>(), new Action(RemoveSelf))));
        }
    }
}
