// Decompiled with JetBrains decompiler
// Type: Celeste.TheoCrystal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked(false)]
    public class TheoCrystal : Actor
    {
        public static ParticleType P_Impact;
        public Vector2 Speed;
        public bool OnPedestal;
        public Holdable Hold;
        private readonly Sprite sprite;
        private bool dead;
        private Level Level;
        private readonly Collision onCollideH;
        private readonly Collision onCollideV;
        private float noGravityTimer;
        private Vector2 prevLiftSpeed;
        private Vector2 previousPosition;
        private HoldableCollider hitSeeker;
        private float swatTimer;
        private bool shattering;
        private float hardVerticalHitSoundCooldown;
        private BirdTutorialGui tutorialGui;
        private float tutorialTimer;

        public TheoCrystal(Vector2 position)
            : base(position)
        {
            previousPosition = position;
            Depth = 100;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = GFX.SpriteBank.Create("theo_crystal"));
            sprite.Scale.X = -1f;
            Add(Hold = new Holdable());
            Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = new Action(OnPickup);
            Hold.OnRelease = new Action<Vector2>(OnRelease);
            Hold.DangerousCheck = new Func<HoldableCollider, bool>(Dangerous);
            Hold.OnHitSeeker = new Action<Seeker>(HitSeeker);
            Hold.OnSwat = new Action<HoldableCollider, int>(Swat);
            Hold.OnHitSpring = new Func<Spring, bool>(HitSpring);
            Hold.OnHitSpinner = new Action<Entity>(HitSpinner);
            Hold.SpeedGetter = () => Speed;
            onCollideH = new Collision(OnCollideH);
            onCollideV = new Collision(OnCollideV);
            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
            Tag = (int)Tags.TransitionUpdate;
            Add(new MirrorReflection());
        }

        public TheoCrystal(EntityData e, Vector2 offset)
            : this(e.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            foreach (TheoCrystal entity in Level.Tracker.GetEntities<TheoCrystal>())
            {
                if (entity != this && entity.Hold.IsHeld)
                {
                    RemoveSelf();
                }
            }
            if (!(Level.Session.Level == "e-00"))
            {
                return;
            }

            tutorialGui = new BirdTutorialGui(this, new Vector2(0.0f, -24f), Dialog.Clean("tutorial_carry"), new object[2]
            {
                 Dialog.Clean("tutorial_hold"),
                 BirdTutorialGui.ButtonPrompt.Grab
            })
            {
                Open = false
            };
            Scene.Add(tutorialGui);
        }

        public override void Update()
        {
            base.Update();
            if (shattering || dead)
            {
                return;
            }

            if (swatTimer > 0.0)
            {
                swatTimer -= Engine.DeltaTime;
            }

            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (OnPedestal)
            {
                Depth = 8999;
            }
            else
            {
                Depth = 100;
                if (Hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else
                {
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
                        float num1 = 800f;
                        if ((double)Math.Abs(Speed.Y) <= 30.0)
                        {
                            num1 *= 0.5f;
                        }

                        float num2 = 350f;
                        if (Speed.Y < 0.0)
                        {
                            num2 *= 0.5f;
                        }

                        Speed.X = Calc.Approach(Speed.X, 0.0f, num2 * Engine.DeltaTime);
                        if (noGravityTimer > 0.0)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 200f, num1 * Engine.DeltaTime);
                        }
                    }
                    previousPosition = ExactPosition;
                    _ = MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                    _ = MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                    if (Center.X > (double)Level.Bounds.Right)
                    {
                        _ = MoveH(32f * Engine.DeltaTime);
                        if ((double)Left - 8.0 > Level.Bounds.Right)
                        {
                            RemoveSelf();
                        }
                    }
                    else if ((double)Left < Level.Bounds.Left)
                    {
                        Left = Level.Bounds.Left;
                        Speed.X *= -0.4f;
                    }
                    else if ((double)Top < Level.Bounds.Top - 4)
                    {
                        Top = Level.Bounds.Top + 4;
                        Speed.Y = 0.0f;
                    }
                    else if ((double)Bottom > Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                    {
                        Bottom = Level.Bounds.Bottom;
                        Speed.Y = -300f;
                        _ = Audio.Play("event:/game/general/assist_screenbottom", Position);
                    }
                    else if ((double)Top > Level.Bounds.Bottom)
                    {
                        Die();
                    }

                    if ((double)X < Level.Bounds.Left + 10)
                    {
                        _ = MoveH(32f * Engine.DeltaTime);
                    }

                    Player entity = Scene.Tracker.GetEntity<Player>();
                    TempleGate templeGate = CollideFirst<TempleGate>();
                    if (templeGate != null && entity != null)
                    {
                        templeGate.Collidable = false;
                        _ = MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                        templeGate.Collidable = true;
                    }
                }
                if (!dead)
                {
                    Hold.CheckAgainstColliders();
                }

                if (hitSeeker != null && swatTimer <= 0.0 && !hitSeeker.Check(Hold))
                {
                    hitSeeker = null;
                }

                if (tutorialGui == null)
                {
                    return;
                }

                if (!OnPedestal && !Hold.IsHeld && OnGround() && Level.Session.GetFlag("foundTheoInCrystal"))
                {
                    tutorialTimer += Engine.DeltaTime;
                }
                else
                {
                    tutorialTimer = 0.0f;
                }

                tutorialGui.Open = tutorialTimer > 0.25;
            }
        }

        public IEnumerator Shatter()
        {
            TheoCrystal theoCrystal = this;
            theoCrystal.shattering = true;
            BloomPoint bloom = new(0.0f, 32f);
            VertexLight light = new(Color.AliceBlue, 0.0f, 64, 200);
            theoCrystal.Add(bloom);
            theoCrystal.Add(light);
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime)
            {
                theoCrystal.Position += theoCrystal.Speed * (1f - p) * Engine.DeltaTime;
                theoCrystal.Level.ZoomFocusPoint = theoCrystal.TopCenter - theoCrystal.Level.Camera.Position;
                light.Alpha = p;
                bloom.Alpha = p;
                yield return null;
            }
            yield return 0.5f;
            theoCrystal.Level.Shake();
            theoCrystal.sprite.Play("shatter");
            yield return 1f;
            theoCrystal.Level.Shake();
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (Hold.IsHeld)
            {
                return;
            }

            Speed = (Center - from).SafeNormalize(120f);
            _ = SlashFx.Burst(Center, Speed.Angle());
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (!Hold.IsHeld || hitSeeker != null)
            {
                return;
            }

            swatTimer = 0.1f;
            hitSeeker = hc;
            Hold.Holder.Swat(dir);
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }

            _ = Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        }

        public void HitSpinner(Entity spinner)
        {
            if (Hold.IsHeld || (double)Speed.Length() >= 0.0099999997764825821)
            {
                return;
            }

            Vector2 vector2 = LiftSpeed;
            if ((double)vector2.Length() >= 0.0099999997764825821)
            {
                return;
            }

            vector2 = previousPosition - ExactPosition;
            if ((double)vector2.Length() >= 0.0099999997764825821 || !OnGround())
            {
                return;
            }

            int num = Math.Sign(X - spinner.X);
            if (num == 0)
            {
                num = 1;
            }

            Speed.X = num * 120f;
            Speed.Y = -30f;
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
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0.0)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                _ = (int)(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            _ = Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if ((double)Math.Abs(Speed.X) > 100.0)
            {
                ImpactParticles(data.Direction);
            }

            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                _ = (int)(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0.0)
            {
                if (hardVerticalHitSoundCooldown <= 0.0)
                {
                    _ = Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0.0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    _ = Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0.0f);
                }
            }
            if (Speed.Y > 160.0)
            {
                ImpactParticles(data.Direction);
            }

            if (Speed.Y > 140.0 && data.Hit is not SwapBlock && data.Hit is not DashSwitch)
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0.0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0.0)
            {
                direction = 3.14159274f;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0.0)
            {
                direction = 0.0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0.0)
            {
                direction = -1.57079637f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = 1.57079637f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0.0 && base.IsRiding(solid);
        }

        protected override void OnSquish(CollisionData data)
        {
            if (TrySquishWiggle(data) || SaveData.Instance.Assists.Invincible)
            {
                return;
            }

            Die();
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag((int)Tags.Persistent);
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag((int)Tags.Persistent);
            if (force.X != 0.0 && force.Y == 0.0)
            {
                force.Y = -0.4f;
            }

            Speed = force * 200f;
            if (!(Speed != Vector2.Zero))
            {
                return;
            }

            noGravityTimer = 0.1f;
        }

        public void Die()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            Player entity = Level.Tracker.GetEntity<Player>();
            _ = (entity?.Die(-Vector2.UnitX * (float)entity.Facing));
            _ = Audio.Play("event:/char/madeline/death", Position);
            Add(new DeathEffect(Color.ForestGreen, new Vector2?(Center - Position)));
            sprite.Visible = false;
            Depth = -1000000;
            AllowPushing = false;
        }
    }
}
