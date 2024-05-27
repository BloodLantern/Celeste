using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class Seeker : Actor
    {
        public static ParticleType P_Attack;
        public static ParticleType P_HitWall;
        public static ParticleType P_Stomp;
        public static ParticleType P_Regen;
        public static ParticleType P_BreakOut;
        public static readonly Color TrailColor = Calc.HexToColor("99e550");
        private const int StIdle = 0;
        private const int StPatrol = 1;
        private const int StSpotted = 2;
        private const int StAttack = 3;
        private const int StStunned = 4;
        private const int StSkidding = 5;
        private const int StRegenerate = 6;
        private const int StReturned = 7;
        private const int size = 12;
        private const int bounceWidth = 16;
        private const int bounceHeight = 4;
        private const float Accel = 600f;
        private const float WallCollideStunThreshold = 100f;
        private const float StunXSpeed = 100f;
        private const float BounceSpeed = 200f;
        private const float SightDistSq = 25600f;
        private const float ExplodeRadius = 40f;
        private Hitbox physicsHitbox;
        private Hitbox breakWallsHitbox;
        private Hitbox attackHitbox;
        private Hitbox bounceHitbox;
        private Circle pushRadius;
        private Circle breakWallsRadius;
        private StateMachine State;
        private Vector2 lastSpottedAt;
        private Vector2 lastPathTo;
        private bool spotted;
        private bool canSeePlayer;
        private Collision onCollideH;
        private Collision onCollideV;
        private Random random;
        private Vector2 lastPosition;
        private Shaker shaker;
        private Wiggler scaleWiggler;
        private bool lastPathFound;
        private List<Vector2> path;
        private int pathIndex;
        private Vector2[] patrolPoints;
        private SineWave idleSineX;
        private SineWave idleSineY;
        public VertexLight Light;
        private bool dead;
        private SoundSource boopedSfx;
        private SoundSource aggroSfx;
        private SoundSource reviveSfx;
        private Sprite sprite;
        private int facing = 1;
        private int spriteFacing = 1;
        private string nextSprite;
        private HoldableCollider theo;
        private HashSet<string> flipAnimations = new HashSet<string>
        {
            "flipMouth",
            "flipEyes",
            "skid"
        };
        public Vector2 Speed;
        private const float FarDistSq = 12544f;
        private const float IdleAccel = 200f;
        private const float IdleSpeed = 50f;
        private const float PatrolSpeed = 25f;
        private const int PatrolChoices = 3;
        private const float PatrolWaitTime = 0.4f;
        private static PatrolPoint[] patrolChoices = new PatrolPoint[3];
        private float patrolWaitTimer;
        private const float SpottedTargetSpeed = 60f;
        private const float SpottedFarSpeed = 90f;
        private const float SpottedMaxYDist = 24f;
        private const float AttackMinXDist = 16f;
        private const float SpottedLosePlayerTime = 0.6f;
        private const float SpottedMinAttackTime = 0.2f;
        private float spottedLosePlayerTimer;
        private float spottedTurnDelay;
        private const float AttackWindUpSpeed = -60f;
        private const float AttackWindUpTime = 0.3f;
        private const float AttackStartSpeed = 180f;
        private const float AttackTargetSpeed = 260f;
        private const float AttackAccel = 300f;
        private const float DirectionDotThreshold = 0.4f;
        private const int AttackTargetUpShift = 2;
        private const float AttackMaxRotateRadians = 0.610865235f;
        private float attackSpeed;
        private bool attackWindUp;
        private const float StunnedAccel = 150f;
        private const float StunTime = 0.8f;
        private const float SkiddingAccel = 200f;
        private const float StrongSkiddingAccel = 400f;
        private const float StrongSkiddingTime = 0.08f;
        private bool strongSkid;

        public Seeker(Vector2 position, Vector2[] patrolPoints)
            : base(position)
        {
            Depth = -200;
            this.patrolPoints = patrolPoints;
            lastPosition = position;
            Collider = physicsHitbox = new Hitbox(6f, 6f, -3f, -3f);
            breakWallsHitbox = new Hitbox(6f, 14f, -3f, -7f);
            attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
            bounceHitbox = new Hitbox(16f, 6f, -8f, -8f);
            pushRadius = new Circle(40f);
            breakWallsRadius = new Circle(16f);
            Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
            Add(new PlayerCollider(OnBouncePlayer, bounceHitbox));
            Add(shaker = new Shaker(false));
            Add(State = new StateMachine());
            State.SetCallbacks(0, IdleUpdate, IdleCoroutine);
            State.SetCallbacks(1, PatrolUpdate, begin: PatrolBegin);
            State.SetCallbacks(2, SpottedUpdate, SpottedCoroutine, SpottedBegin);
            State.SetCallbacks(3, AttackUpdate, AttackCoroutine, AttackBegin);
            State.SetCallbacks(4, StunnedUpdate, StunnedCoroutine);
            State.SetCallbacks(5, SkiddingUpdate, SkiddingCoroutine, SkiddingBegin, SkiddingEnd);
            State.SetCallbacks(6, RegenerateUpdate, RegenerateCoroutine, RegenerateBegin, RegenerateEnd);
            State.SetCallbacks(7, null, ReturnedCoroutine);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Add(idleSineX = new SineWave(0.5f));
            Add(idleSineY = new SineWave(0.7f));
            Add(Light = new VertexLight(Color.White, 1f, 32, 64));
            Add(theo = new HoldableCollider(OnHoldable, attackHitbox));
            Add(new MirrorReflection());
            path = new List<Vector2>();
            IgnoreJumpThrus = true;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.OnLastFrame = f =>
            {
                if (!flipAnimations.Contains(f) || spriteFacing == facing)
                    return;
                spriteFacing = facing;
                if (nextSprite == null)
                    return;
                sprite.Play(nextSprite);
                nextSprite = null;
            };
            sprite.OnChange = (last, next) =>
            {
                nextSprite = null;
                sprite.OnLastFrame(last);
            };
            SquishCallback = d =>
            {
                if (dead || TrySquishWiggle(d))
                    return;
                Entity entity = new Entity(Position);
                entity.Add(new DeathEffect(Color.HotPink, Center - Position)
                {
                    OnEnd = () => entity.RemoveSelf()
                });
                entity.Depth = -1000000;
                Scene.Add(entity);
                Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);
                RemoveSelf();
                dead = true;
            };
            scaleWiggler = Wiggler.Create(0.8f, 2f);
            Add(scaleWiggler);
            Add(boopedSfx = new SoundSource());
            Add(aggroSfx = new SoundSource());
            Add(reviveSfx = new SoundSource());
        }

        public Seeker(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.NodesOffset(offset))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            random = new Random(SceneAs<Level>().Session.LevelData.LoadSeed);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || X == (double) entity.X)
                SnapFacing(1f);
            else
                SnapFacing(Math.Sign(entity.X - X));
        }

        public override bool IsRiding(JumpThru jumpThru) => false;

        public override bool IsRiding(Solid solid) => false;

        public bool Attacking => State.State == 3 && !attackWindUp;

        public bool Spotted => State.State == 3 || State.State == 2;

        public bool Regenerating => State.State == 6;

        private void OnAttackPlayer(Player player)
        {
            if (State.State != 4)
            {
                player.Die((player.Center - Position).SafeNormalize());
            }
            else
            {
                Collider collider = Collider;
                Collider = bounceHitbox;
                player.PointBounce(Center);
                Speed = (Center - player.Center).SafeNormalize(100f);
                scaleWiggler.Start();
                Collider = collider;
            }
        }

        private void OnBouncePlayer(Player player)
        {
            Collider collider = Collider;
            Collider = attackHitbox;
            if (CollideCheck(player))
            {
                OnAttackPlayer(player);
            }
            else
            {
                player.Bounce(Top);
                GotBouncedOn(player);
            }
            Collider = collider;
        }

        private void GotBouncedOn(Entity entity)
        {
            Celeste.Freeze(0.15f);
            Speed = (Center - entity.Center).SafeNormalize(200f);
            State.State = 6;
            sprite.Scale = new Vector2(1.4f, 0.6f);
            SceneAs<Level>().Particles.Emit(Seeker.P_Stomp, 8, Center - Vector2.UnitY * 5f, new Vector2(6f, 3f));
        }

        public void HitSpring() => Speed.Y = -150f;

        private bool CanSeePlayer(Player player)
        {
            if (player == null || State.State != 2 && !SceneAs<Level>().InsideCamera(Center) && Vector2.DistanceSquared(Center, player.Center) > 25600.0)
                return false;
            Vector2 vector2 = (player.Center - Center).Perpendicular().SafeNormalize(2f);
            return !Scene.CollideCheck<Solid>(Center + vector2, player.Center + vector2) && !Scene.CollideCheck<Solid>(Center - vector2, player.Center - vector2);
        }

        public override void Update()
        {
            Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
            foreach (Entity entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                entity.Collidable = true;
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            if (State.State == 6)
            {
                canSeePlayer = false;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                canSeePlayer = CanSeePlayer(entity);
                if (canSeePlayer)
                {
                    spotted = true;
                    lastSpottedAt = entity.Center;
                }
            }
            if (lastPathTo != lastSpottedAt)
            {
                lastPathTo = lastSpottedAt;
                pathIndex = 0;
                lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, Center, FollowTarget);
            }
            base.Update();
            lastPosition = Position;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            Level level = SceneAs<Level>();
            double left1 = Left;
            Rectangle bounds = level.Bounds;
            double left2 = bounds.Left;
            if (left1 < left2 && Speed.X < 0.0)
            {
                bounds = level.Bounds;
                Left = bounds.Left;
                onCollideH(CollisionData.Empty);
            }
            else
            {
                double right1 = Right;
                bounds = level.Bounds;
                double right2 = bounds.Right;
                if (right1 > right2 && Speed.X > 0.0)
                {
                    bounds = level.Bounds;
                    Right = bounds.Right;
                    onCollideH(CollisionData.Empty);
                }
            }
            double top = Top;
            bounds = level.Bounds;
            double num = bounds.Top - 8;
            if (top < num && Speed.Y < 0.0)
            {
                bounds = level.Bounds;
                Top = bounds.Top - 8;
                onCollideV(CollisionData.Empty);
            }
            else
            {
                double bottom1 = Bottom;
                bounds = level.Bounds;
                double bottom2 = bounds.Bottom;
                if (bottom1 > bottom2 && Speed.Y > 0.0)
                {
                    bounds = level.Bounds;
                    Bottom = bounds.Bottom;
                    onCollideV(CollisionData.Empty);
                }
            }
            foreach (SeekerCollider component in Scene.Tracker.GetComponents<SeekerCollider>())
                component.Check(this);
            if (State.State == 3 && Speed.X > 0.0)
            {
                bounceHitbox.Width = 16f;
                bounceHitbox.Position.X = -10f;
            }
            else if (State.State == 3 && Speed.Y < 0.0)
            {
                bounceHitbox.Width = 16f;
                bounceHitbox.Position.X = -6f;
            }
            else
            {
                bounceHitbox.Width = 12f;
                bounceHitbox.Position.X = -6f;
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                entity.Collidable = false;
        }

        private void TurnFacing(float dir, string gotoSprite = null)
        {
            if (dir != 0.0)
                facing = Math.Sign(dir);
            if (spriteFacing != facing)
            {
                if (State.State == 5)
                    sprite.Play("skid");
                else if (State.State == 3 || State.State == 2)
                    sprite.Play("flipMouth");
                else
                    sprite.Play("flipEyes");
                nextSprite = gotoSprite;
            }
            else
            {
                if (gotoSprite == null)
                    return;
                sprite.Play(gotoSprite);
            }
        }

        private void SnapFacing(float dir)
        {
            if (dir == 0.0)
                return;
            spriteFacing = facing = Math.Sign(dir);
        }

        private void OnHoldable(Holdable holdable)
        {
            if (State.State != 6 && holdable.Dangerous(theo))
            {
                holdable.HitSeeker(this);
                State.State = 4;
                Speed = (Center - holdable.Entity.Center).SafeNormalize(120f);
                scaleWiggler.Start();
            }
            else
            {
                if (State.State != 3 && State.State != 5 || !holdable.IsHeld)
                    return;
                holdable.Swat(theo, Math.Sign(Speed.X));
                State.State = 4;
                Speed = (Center - holdable.Entity.Center).SafeNormalize(120f);
                scaleWiggler.Start();
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += shaker.Value;
            Vector2 scale = this.sprite.Scale;
            Sprite sprite = this.sprite;
            sprite.Scale *= (float) (1.0 - 0.30000001192092896 * scaleWiggler.Value);
            this.sprite.Scale.X *= spriteFacing;
            base.Render();
            Position = position;
            this.sprite.Scale = scale;
        }

        public override void DebugRender(Camera camera)
        {
            Collider collider = Collider;
            Collider = attackHitbox;
            attackHitbox.Render(camera, Color.Red);
            Collider = bounceHitbox;
            bounceHitbox.Render(camera, Color.Aqua);
            Collider = collider;
        }

        private void SlammedIntoWall(CollisionData data)
        {
            float direction;
            float x;
            if (data.Direction.X > 0.0)
            {
                direction = 3.14159274f;
                x = Right;
            }
            else
            {
                direction = 0.0f;
                x = Left;
            }
            SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, new Vector2(x, Y), Vector2.UnitY * 4f, direction);
            if (data.Hit is DashSwitch)
            {
                int num = (int) (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Collider = breakWallsHitbox;
            foreach (TempleCrackedBlock entity in Scene.Tracker.GetEntities<TempleCrackedBlock>())
            {
                if (CollideCheck(entity, Position + Vector2.UnitX * Math.Sign(Speed.X)))
                    entity.Break(Center);
            }
            Collider = physicsHitbox;
            SceneAs<Level>().DirectionalShake(Vector2.UnitX * Math.Sign(Speed.X));
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Speed.X = Math.Sign(Speed.X) * -100f;
            Speed.Y *= 0.4f;
            sprite.Scale.X = 0.6f;
            sprite.Scale.Y = 1.4f;
            shaker.ShakeFor(0.5f, false);
            scaleWiggler.Start();
            State.State = 4;
            if (data.Hit is SeekerBarrier)
            {
                (data.Hit as SeekerBarrier).OnReflectSeeker();
                Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
            }
            else
                Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
        }

        private void OnCollideH(CollisionData data)
        {
            if (State.State == 3 && data.Hit != null)
            {
                int x = Math.Sign(Speed.X);
                if (!CollideCheck<Solid>(Position + new Vector2(x, 4f)) && !MoveVExact(4) || !CollideCheck<Solid>(Position + new Vector2(x, -4f)) && !MoveVExact(-4))
                    return;
            }
            if ((State.State == 3 || State.State == 5) && Math.Abs(Speed.X) >= 100.0)
                SlammedIntoWall(data);
            else
                Speed.X *= -0.2f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (State.State == 3)
                Speed.Y *= -0.6f;
            else
                Speed.Y *= -0.2f;
        }

        private Vector2 FollowTarget => lastSpottedAt - Vector2.UnitY * 2f;

        private void CreateTrail()
        {
            Vector2 scale = this.sprite.Scale;
            Sprite sprite = this.sprite;
            sprite.Scale *= (float) (1.0 - 0.30000001192092896 * scaleWiggler.Value);
            this.sprite.Scale.X *= spriteFacing;
            TrailManager.Add(this, Seeker.TrailColor, 0.5f);
            this.sprite.Scale = scale;
        }

        private int IdleUpdate()
        {
            if (canSeePlayer)
                return 2;
            Vector2 target = Vector2.Zero;
            if (spotted && Vector2.DistanceSquared(Center, FollowTarget) > 64.0)
            {
                float speedMagnitude = GetSpeedMagnitude(50f);
                target = !lastPathFound ? (FollowTarget - Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude);
            }
            if (target == Vector2.Zero)
            {
                target.X = idleSineX.Value * 6f;
                target.Y = idleSineY.Value * 6f;
            }
            Speed = Calc.Approach(Speed, target, 200f * Engine.DeltaTime);
            if (Speed.LengthSquared() > 400.0)
                TurnFacing(Speed.X);
            if (spriteFacing == facing)
                sprite.Play("idle");
            return 0;
        }

        private IEnumerator IdleCoroutine()
        {
            Seeker seeker = this;
            if (seeker.patrolPoints != null && seeker.patrolPoints.Length != 0 && seeker.spotted)
            {
                while (Vector2.DistanceSquared(seeker.Center, seeker.FollowTarget) > 64.0)
                    yield return null;
                yield return 0.3f;
                seeker.State.State = 1;
            }
        }

        private Vector2 GetPathSpeed(float magnitude)
        {
            if (pathIndex >= path.Count)
                return Vector2.Zero;
            if (Vector2.DistanceSquared(Center, path[pathIndex]) >= 36.0)
                return (path[pathIndex] - Center).SafeNormalize(magnitude);
            ++pathIndex;
            return GetPathSpeed(magnitude);
        }

        private float GetSpeedMagnitude(float baseMagnitude)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return baseMagnitude;
            return Vector2.DistanceSquared(Center, entity.Center) > 12544.0 ? baseMagnitude * 3f : baseMagnitude * 1.5f;
        }

        private void PatrolBegin()
        {
            State.State = ChoosePatrolTarget();
            patrolWaitTimer = 0.0f;
        }

        private int PatrolUpdate()
        {
            if (canSeePlayer)
                return 2;
            if (patrolWaitTimer > 0.0)
            {
                patrolWaitTimer -= Engine.DeltaTime;
                if (patrolWaitTimer <= 0.0)
                    return ChoosePatrolTarget();
            }
            else if (Vector2.DistanceSquared(Center, lastSpottedAt) < 144.0)
                patrolWaitTimer = 0.4f;
            float speedMagnitude = GetSpeedMagnitude(25f);
            Speed = Calc.Approach(Speed, !lastPathFound ? (FollowTarget - Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude), 600f * Engine.DeltaTime);
            if (Speed.LengthSquared() > 100.0)
                TurnFacing(Speed.X);
            if (spriteFacing == facing)
                sprite.Play("search");
            return 1;
        }

        private int ChoosePatrolTarget()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return 0;
            for (int index = 0; index < 3; ++index)
                Seeker.patrolChoices[index].Distance = 0.0f;
            int val2 = 0;
            foreach (Vector2 patrolPoint in patrolPoints)
            {
                if (Vector2.DistanceSquared(Center, patrolPoint) >= 576.0)
                {
                    float num = Vector2.DistanceSquared(patrolPoint, entity.Center);
                    for (int index1 = 0; index1 < 3; ++index1)
                    {
                        if (num < (double) Seeker.patrolChoices[index1].Distance || Seeker.patrolChoices[index1].Distance <= 0.0)
                        {
                            ++val2;
                            for (int index2 = 2; index2 > index1; --index2)
                            {
                                Seeker.patrolChoices[index2].Distance = Seeker.patrolChoices[index2 - 1].Distance;
                                Seeker.patrolChoices[index2].Point = Seeker.patrolChoices[index2 - 1].Point;
                            }
                            Seeker.patrolChoices[index1].Distance = num;
                            Seeker.patrolChoices[index1].Point = patrolPoint;
                            break;
                        }
                    }
                }
            }
            if (val2 <= 0)
                return 0;
            lastSpottedAt = Seeker.patrolChoices[random.Next(Math.Min(3, val2))].Point;
            lastPathTo = lastSpottedAt;
            pathIndex = 0;
            lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, Center, FollowTarget);
            return 1;
        }

        private void SpottedBegin()
        {
            aggroSfx.Play("event:/game/05_mirror_temple/seeker_aggro");
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
                TurnFacing(entity.X - X, "spot");
            spottedLosePlayerTimer = 0.6f;
            spottedTurnDelay = 1f;
        }

        private int SpottedUpdate()
        {
            if (!canSeePlayer)
            {
                spottedLosePlayerTimer -= Engine.DeltaTime;
                if (spottedLosePlayerTimer < 0.0)
                    return 0;
            }
            else
                spottedLosePlayerTimer = 0.6f;
            float speedMagnitude = GetSpeedMagnitude(60f);
            Vector2 vector2_1 = !lastPathFound ? (FollowTarget - Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude);
            if (Vector2.DistanceSquared(Center, FollowTarget) < 2500.0 && Y < (double) FollowTarget.Y)
            {
                float num = vector2_1.Angle();
                if (Y < FollowTarget.Y - 2.0)
                    num = Calc.AngleLerp(num, 1.57079637f, 0.5f);
                else if (Y > FollowTarget.Y + 2.0)
                    num = Calc.AngleLerp(num, -1.57079637f, 0.5f);
                vector2_1 = Calc.AngleToVector(num, 60f);
                Vector2 vector2_2 = Vector2.UnitX * Math.Sign(X - lastSpottedAt.X) * 48f;
                if (Math.Abs(X - lastSpottedAt.X) < 36.0 && !CollideCheck<Solid>(Position + vector2_2) && !CollideCheck<Solid>(lastSpottedAt + vector2_2))
                    vector2_1.X = Math.Sign(X - lastSpottedAt.X) * 60;
            }
            Speed = Calc.Approach(Speed, vector2_1, 600f * Engine.DeltaTime);
            spottedTurnDelay -= Engine.DeltaTime;
            if (spottedTurnDelay <= 0.0)
                TurnFacing(Speed.X, "spotted");
            return 2;
        }

        private IEnumerator SpottedCoroutine()
        {
            yield return 0.2f;
            while (!CanAttack())
                yield return null;
            State.State = 3;
        }

        private bool CanAttack()
        {
            if (Math.Abs(Y - lastSpottedAt.Y) > 24.0 || Math.Abs(X - lastSpottedAt.X) < 16.0)
                return false;
            Vector2 vector2 = (FollowTarget - Center).SafeNormalize();
            return Vector2.Dot(-Vector2.UnitY, vector2) <= 0.5 && Vector2.Dot(Vector2.UnitY, vector2) <= 0.5 && !CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(lastSpottedAt.X - X) * 24f);
        }

        private void AttackBegin()
        {
            Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
            attackWindUp = true;
            attackSpeed = -60f;
            Speed = (FollowTarget - Center).SafeNormalize(-60f);
        }

        private int AttackUpdate()
        {
            if (!attackWindUp)
            {
                Vector2 vector1 = (FollowTarget - Center).SafeNormalize();
                if (Vector2.Dot(Speed.SafeNormalize(), vector1) < 0.40000000596046448)
                    return 5;
                attackSpeed = Calc.Approach(attackSpeed, 260f, 300f * Engine.DeltaTime);
                Speed = Speed.RotateTowards(vector1.Angle(), 7f * (float) Math.PI / 36f * Engine.DeltaTime).SafeNormalize(attackSpeed);
                if (Scene.OnInterval(0.04f))
                {
                    Vector2 vector2 = (-Speed).SafeNormalize();
                    SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, Position + vector2 * 4f, Vector2.One * 4f, vector2.Angle());
                }
                if (Scene.OnInterval(0.06f))
                    CreateTrail();
            }
            return 3;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator AttackCoroutine()
        {
                TurnFacing(lastSpottedAt.X - X, "windUp");
                yield return 0.3f;
                attackWindUp = false;
                attackSpeed = 180f;
                Speed = (lastSpottedAt - Vector2.UnitY * 2f - Center).SafeNormalize(180f);
                SnapFacing(Speed.X);
        }

        private int StunnedUpdate()
        {
            Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 4;
        }

        private IEnumerator StunnedCoroutine()
        {
            yield return 0.8f;
            State.State = 0;
        }

        private void SkiddingBegin()
        {
            Audio.Play("event:/game/05_mirror_temple/seeker_dash_turn", Position);
            strongSkid = false;
            TurnFacing(-facing);
        }

        private int SkiddingUpdate()
        {
            Speed = Calc.Approach(Speed, Vector2.Zero, (strongSkid ? 400f : 200f) * Engine.DeltaTime);
            if (Speed.LengthSquared() >= 400.0)
                return 5;
            return canSeePlayer ? 2 : 0;
        }

        private IEnumerator SkiddingCoroutine()
        {
            yield return 0.08f;
            strongSkid = true;
        }

        private void SkiddingEnd() => spriteFacing = facing;

        private void RegenerateBegin()
        {
            Audio.Play("event:/game/general/thing_booped", Position);
            boopedSfx.Play("event:/game/05_mirror_temple/seeker_booped");
            sprite.Play("takeHit");
            Collidable = false;
            State.Locked = true;
            Light.StartRadius = 16f;
            Light.EndRadius = 32f;
        }

        private void RegenerateEnd()
        {
            reviveSfx.Play("event:/game/05_mirror_temple/seeker_revive");
            Collidable = true;
            Light.StartRadius = 32f;
            Light.EndRadius = 64f;
        }

        private int RegenerateUpdate()
        {
            Speed.X = Calc.Approach(Speed.X, 0.0f, 150f * Engine.DeltaTime);
            Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 6;
        }

        private IEnumerator RegenerateCoroutine()
        {
            Seeker seeker = this;
            yield return 1f;
            seeker.shaker.On = true;
            yield return 0.2f;
            seeker.sprite.Play("pulse");
            yield return 0.5f;
            seeker.sprite.Play("recover");
            RecoverBlast.Spawn(seeker.Position);
            yield return 0.15f;
            seeker.Collider = seeker.pushRadius;
            Player player = seeker.CollideFirst<Player>();
            if (player != null && !seeker.Scene.CollideCheck<Solid>(seeker.Position, player.Center))
                player.ExplodeLaunch(seeker.Position);
            TheoCrystal theoCrystal = seeker.CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !seeker.Scene.CollideCheck<Solid>(seeker.Position, theoCrystal.Center))
                theoCrystal.ExplodeLaunch(seeker.Position);
            foreach (TempleCrackedBlock entity in seeker.Scene.Tracker.GetEntities<TempleCrackedBlock>())
            {
                if (seeker.CollideCheck(entity))
                    entity.Break(seeker.Position);
            }
            foreach (TouchSwitch entity in seeker.Scene.Tracker.GetEntities<TouchSwitch>())
            {
                if (seeker.CollideCheck(entity))
                    entity.TurnOn();
            }
            seeker.Collider = seeker.physicsHitbox;
            Level level = seeker.SceneAs<Level>();
            level.Displacement.AddBurst(seeker.Position, 0.4f, 12f, 36f, 0.5f);
            level.Displacement.AddBurst(seeker.Position, 0.4f, 24f, 48f, 0.5f);
            level.Displacement.AddBurst(seeker.Position, 0.4f, 36f, 60f, 0.5f);
            for (float direction = 0.0f; direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = seeker.Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f), Calc.Random.Range(12, 18));
                level.Particles.Emit(Seeker.P_Regen, position, direction);
            }
            seeker.shaker.On = false;
            seeker.State.Locked = false;
            seeker.State.State = 7;
        }

        private IEnumerator ReturnedCoroutine()
        {
            yield return 0.3f;
            State.State = 0;
        }

        private struct PatrolPoint
        {
            public Vector2 Point;
            public float Distance;
        }

        [Pooled]
        private class RecoverBlast : Entity
        {
            private Sprite sprite;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Depth = -199;
                if (sprite == null)
                {
                    Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    sprite.OnLastFrame = a => RemoveSelf();
                }
                sprite.Play("shockwave", true);
            }

            public static void Spawn(Vector2 position)
            {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }
    }
}
