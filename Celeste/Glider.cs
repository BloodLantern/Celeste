// Decompiled with JetBrains decompiler
// Type: Celeste.Glider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Glider : Actor
    {
        public static ParticleType P_Glide;
        public static ParticleType P_GlideUp;
        public static ParticleType P_Platform;
        public static ParticleType P_Glow;
        public static ParticleType P_Expand;
        private const float HighFrictionTime = 0.5f;
        public Vector2 Speed;
        public Holdable Hold;
        private Level level;
        private readonly Collision onCollideH;
        private readonly Collision onCollideV;
        private Vector2 prevLiftSpeed;
        private Vector2 startPos;
        private float noGravityTimer;
        private float highFrictionTimer;
        private bool bubble;
        private bool tutorial;
        private bool destroyed;
        private readonly Sprite sprite;
        private readonly Wiggler wiggler;
        private readonly SineWave platformSine;
        private readonly SoundSource fallingSfx;
        private BirdTutorialGui tutorialGui;

        public Glider(Vector2 position, bool bubble, bool tutorial)
          : base(position)
        {
            this.bubble = bubble;
            this.tutorial = tutorial;
            startPos = Position;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            onCollideH = new Collision(OnCollideH);
            onCollideV = new Collision(OnCollideV);
            Add(sprite = GFX.SpriteBank.Create("glider"));
            Add(wiggler = Wiggler.Create(0.25f, 4f));
            Depth = -5;
            Add(Hold = new Holdable(0.3f));
            Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
            Hold.SlowFall = true;
            Hold.SlowRun = false;
            Hold.OnPickup = new Action(OnPickup);
            Hold.OnRelease = new Action<Vector2>(OnRelease);
            Hold.SpeedGetter = () => Speed;
            Hold.OnHitSpring = new Func<Spring, bool>(HitSpring);
            platformSine = new SineWave(0.3f);
            Add(platformSine);
            fallingSfx = new SoundSource();
            Add(fallingSfx);
            Add(new WindMover(new Action<Vector2>(WindMode)));
        }

        public Glider(EntityData e, Vector2 offset)
          : this(e.Position + offset, e.Bool(nameof(bubble)), e.Bool(nameof(tutorial)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            if (!tutorial)
            {
                return;
            }

            tutorialGui = new BirdTutorialGui(this, new Vector2(0.0f, -24f), Dialog.Clean("tutorial_carry"), new object[2]
            {
         Dialog.Clean("tutorial_hold"),
         BirdTutorialGui.ButtonPrompt.Grab
            })
            {
                Open = true
            };
            Scene.Add(tutorialGui);
        }

        public override void Update()
        {
            if (Scene.OnInterval(0.05f))
            {
                level.Particles.Emit(Glider.P_Glow, 1, Center + (Vector2.UnitY * -9f), new Vector2(10f, 4f));
            }

            sprite.Rotation = Calc.Approach(sprite.Rotation, !Hold.IsHeld ? 0.0f : (!Hold.Holder.OnGround() ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 1.04719758f, -1.04719758f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f)), 3.14159274f * Engine.DeltaTime);
            if (Hold.IsHeld && !Hold.Holder.OnGround() && (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
            {
                if (!fallingSfx.Playing)
                {
                    _ = Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                    _ = fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
                }
                Vector2 speed = Hold.Holder.Speed;
                _ = fallingSfx.Param("glider_speed", Calc.Map(new Vector2(speed.X * 0.5f, speed.Y < 0.0 ? speed.Y * 2f : speed.Y).Length(), 0.0f, 120f, newMax: 0.7f));
            }
            else
            {
                _ = fallingSfx.Stop();
            }

            base.Update();
            if (!destroyed)
            {
                foreach (SeekerBarrier entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                {
                    entity.Collidable = true;
                    int num = CollideCheck(entity) ? 1 : 0;
                    entity.Collidable = false;
                    if (num != 0)
                    {
                        destroyed = true;
                        Collidable = false;
                        if (Hold.IsHeld)
                        {
                            Vector2 speed = Hold.Holder.Speed;
                            Hold.Holder.Drop();
                            Speed = speed * 0.333f;
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                        Add(new Coroutine(DestroyAnimationRoutine()));
                        return;
                    }
                }
                if (Hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else if (!bubble)
                {
                    if (highFrictionTimer > 0.0)
                    {
                        highFrictionTimer -= Engine.DeltaTime;
                    }

                    if (OnGround())
                    {
                        Speed.X = Calc.Approach(Speed.X, OnGround(Position + (Vector2.UnitX * 3f)) ? (OnGround(Position - (Vector2.UnitX * 3f)) ? 0.0f : -20f) : 20f, 800f * Engine.DeltaTime);
                        Vector2 liftSpeed = LiftSpeed;
                        if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                        {
                            Speed = prevLiftSpeed;
                            prevLiftSpeed = Vector2.Zero;
                            Speed.Y = Math.Min(Speed.Y * 0.6f, 0.0f);
                            if (Speed.X != 0.0 && Speed.Y == 0.0)
                            {
                                Speed.Y = -60f;
                            }

                            if (Speed.Y < 0.0)
                            {
                                noGravityTimer = 0.15f;
                            }
                        }
                        else
                        {
                            prevLiftSpeed = liftSpeed;
                            if (liftSpeed.Y < 0.0 && Speed.Y < 0.0)
                            {
                                Speed.Y = 0.0f;
                            }
                        }
                    }
                    else if (Hold.ShouldHaveGravity)
                    {
                        float num = 200f;
                        if (Speed.Y >= -30.0)
                        {
                            num *= 0.5f;
                        }

                        Speed.X = Calc.Approach(Speed.X, 0.0f, (Speed.Y >= 0.0 ? (highFrictionTimer > 0.0 ? 10f : 40f) : 40f) * Engine.DeltaTime);
                        if (noGravityTimer > 0.0)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            Speed.Y = level.Wind.Y >= 0.0 ? Calc.Approach(Speed.Y, 30f, num * Engine.DeltaTime) : Calc.Approach(Speed.Y, 0.0f, num * Engine.DeltaTime);
                        }
                    }
                    _ = MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                    _ = MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                    Rectangle bounds;
                    if ((double)Left < level.Bounds.Left)
                    {
                        bounds = level.Bounds;
                        Left = bounds.Left;
                        OnCollideH(new CollisionData()
                        {
                            Direction = -Vector2.UnitX
                        });
                    }
                    else
                    {
                        double right1 = (double)Right;
                        bounds = level.Bounds;
                        double right2 = bounds.Right;
                        if (right1 > right2)
                        {
                            bounds = level.Bounds;
                            Right = bounds.Right;
                            OnCollideH(new CollisionData()
                            {
                                Direction = Vector2.UnitX
                            });
                        }
                    }
                    double top1 = (double)Top;
                    bounds = level.Bounds;
                    double top2 = bounds.Top;
                    if (top1 < top2)
                    {
                        bounds = level.Bounds;
                        Top = bounds.Top;
                        OnCollideV(new CollisionData()
                        {
                            Direction = -Vector2.UnitY
                        });
                    }
                    else
                    {
                        double top3 = (double)Top;
                        bounds = level.Bounds;
                        double num = bounds.Bottom + 16;
                        if (top3 > num)
                        {
                            RemoveSelf();
                            return;
                        }
                    }
                    Hold.CheckAgainstColliders();
                }
                else
                {
                    Position = startPos + (Vector2.UnitY * platformSine.Value * 1f);
                }

                Vector2 one = Vector2.One;
                if (!Hold.IsHeld)
                {
                    if (level.Wind.Y < 0.0)
                    {
                        PlayOpen();
                    }
                    else
                    {
                        sprite.Play("idle");
                    }
                }
                else if (Hold.Holder.Speed.Y > 20.0 || level.Wind.Y < 0.0)
                {
                    if (level.OnInterval(0.04f))
                    {
                        if (level.Wind.Y < 0.0)
                        {
                            level.ParticlesBG.Emit(Glider.P_GlideUp, 1, Position - (Vector2.UnitY * 20f), new Vector2(6f, 4f));
                        }
                        else
                        {
                            level.ParticlesBG.Emit(Glider.P_Glide, 1, Position - (Vector2.UnitY * 10f), new Vector2(6f, 4f));
                        }
                    }
                    PlayOpen();
                    if (Input.GliderMoveY.Value > 0)
                    {
                        one.X = 0.7f;
                        one.Y = 1.4f;
                    }
                    else if (Input.GliderMoveY.Value < 0)
                    {
                        one.X = 1.2f;
                        one.Y = 0.8f;
                    }
                    Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                }
                else
                {
                    sprite.Play("held");
                }

                sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, one.Y, Engine.DeltaTime * 2f);
                sprite.Scale.X = Calc.Approach(sprite.Scale.X, Math.Sign(sprite.Scale.X) * one.X, Engine.DeltaTime * 2f);
                if (tutorialGui == null)
                {
                    return;
                }

                tutorialGui.Open = tutorial && !Hold.IsHeld && (OnGround(4) || bubble);
            }
            else
            {
                Position += Speed * Engine.DeltaTime;
            }
        }

        private void PlayOpen()
        {
            if (sprite.CurrentAnimationID is "fall" or "fallLoop")
            {
                return;
            }

            sprite.Play("fall");
            sprite.Scale = new Vector2(1.5f, 0.6f);
            level.Particles.Emit(Glider.P_Expand, 16, Center + (Vector2.UnitY * -12f).Rotate(sprite.Rotation), new Vector2(8f, 3f), sprite.Rotation - 1.57079637f);
            if (!Hold.IsHeld)
            {
                return;
            }

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
        }

        public override void Render()
        {
            if (!destroyed)
            {
                sprite.DrawSimpleOutline();
            }

            base.Render();
            if (!bubble)
            {
                return;
            }

            for (int num = 0; num < 24; ++num)
            {
                Draw.Point(Position + PlatformAdd(num), PlatformColor(num));
            }
        }

        private void WindMode(Vector2 wind)
        {
            if (Hold.IsHeld)
            {
                return;
            }

            if (wind.X != 0.0)
            {
                _ = MoveH(wind.X * 0.5f);
            }

            if (wind.Y == 0.0)
            {
                return;
            }

            _ = MoveV(wind.Y);
        }

        private Vector2 PlatformAdd(int num)
        {
            return new Vector2(num - 12, (int)Math.Round(Math.Sin(Scene.TimeActive + (num * 0.20000000298023224)) * 1.7999999523162842) - 5);
        }

        private Color PlatformColor(int num)
        {
            return num is <= 1 or >= 22 ? Color.White * 0.4f : Color.White * 0.8f;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                _ = (int)(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            _ = Speed.X < 0.0
                ? Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position)
                : Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);

            Speed.X *= -1f;
            sprite.Scale = new Vector2(0.8f, 1.2f);
        }

        private void OnCollideV(CollisionData data)
        {
            if ((double)Math.Abs(Speed.Y) > 8.0)
            {
                sprite.Scale = new Vector2(1.2f, 0.8f);
                _ = Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
            }
            if (Speed.Y < 0.0)
            {
                Speed.Y *= -0.5f;
            }
            else
            {
                Speed.Y = 0.0f;
            }
        }

        private void OnPickup()
        {
            if (bubble)
            {
                for (int num = 0; num < 24; ++num)
                {
                    level.Particles.Emit(Glider.P_Platform, Position + PlatformAdd(num), PlatformColor(num));
                }
            }
            AllowPushing = false;
            Speed = Vector2.Zero;
            AddTag((int)Tags.Persistent);
            highFrictionTimer = 0.5f;
            bubble = false;
            wiggler.Start();
            tutorial = false;
        }

        private void OnRelease(Vector2 force)
        {
            if (force.X == 0.0)
            {
                _ = Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
            }

            AllowPushing = true;
            RemoveTag((int)Tags.Persistent);
            force.Y *= 0.5f;
            if (force.X != 0.0 && force.Y == 0.0)
            {
                force.Y = -0.4f;
            }

            Speed = force * 100f;
            wiggler.Start();
        }

        protected override void OnSquish(CollisionData data)
        {
            if (TrySquishWiggle(data))
            {
                return;
            }

            RemoveSelf();
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0.0)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    wiggler.Start();
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }
            }
            return false;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator DestroyAnimationRoutine()
        {
            _ = Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
            sprite.Play("death", false, false);
            yield return 1f;
            base.RemoveSelf();
            yield break;
        }
    }
}
