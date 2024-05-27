using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class PlayerSeeker : Actor
    {
        private Facings facing;
        private Sprite sprite;
        private Vector2 speed;
        private bool enabled;
        private float dashTimer;
        private Vector2 dashDirection;
        private float trailTimerA;
        private float trailTimerB;
        private Shaker shaker;

        public PlayerSeeker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.Play("statue");
            sprite.OnLastFrame = a =>
            {
                if (!(a == "flipMouth") && !(a == "flipEyes"))
                    return;
                facing = (Facings) (-(int) facing);
            };
            Collider = new Hitbox(10f, 10f, -5f, -5f);
            Add(new MirrorReflection());
            Add(new PlayerCollider(OnPlayer));
            Add(new VertexLight(Color.White, 1f, 32, 64));
            facing = Facings.Right;
            Add(shaker = new Shaker(false));
            Add(new Coroutine(IntroSequence()));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            level.Session.ColorGrade = "templevoid";
            level.ScreenPadding = 32f;
            level.CanRetry = false;
        }

        private IEnumerator IntroSequence()
        {
            PlayerSeeker playerSeeker = this;
            Level level = playerSeeker.Scene as Level;
            yield return null;
            Glitch.Value = 0.05f;
            level.Tracker.GetEntity<Player>()?.StartTempleMirrorVoidSleep();
            yield return 3f;
            Vector2 from = level.Camera.Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, true);
            tween.OnUpdate = f => level.Camera.Position = from + (CameraTarget - from) * f.Eased;
            playerSeeker.Add(tween);
            yield return 2f;
            playerSeeker.shaker.ShakeFor(0.5f, false);
            playerSeeker.BreakOutParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            yield return 1f;
            playerSeeker.shaker.ShakeFor(0.5f, false);
            playerSeeker.BreakOutParticles();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
            yield return 1f;
            playerSeeker.BreakOutParticles();
            Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", playerSeeker.Position);
            playerSeeker.shaker.ShakeFor(1f, false);
            playerSeeker.sprite.Play("hatch");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
            playerSeeker.enabled = true;
            yield return 0.8f;
            playerSeeker.BreakOutParticles();
            yield return 0.7f;
        }

        private void BreakOutParticles()
        {
            Level level = SceneAs<Level>();
            for (float direction = 0.0f; direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f), Calc.Random.Range(12, 20));
                level.Particles.Emit(Seeker.P_BreakOut, position, direction);
            }
        }

        private void OnPlayer(Player player)
        {
            if (player.Dead)
                return;
            Leader.StoreStrawberries(player.Leader);
            PlayerDeadBody playerDeadBody = player.Die((player.Position - Position).SafeNormalize(), true, false);
            playerDeadBody.DeathAction = End;
            playerDeadBody.ActionDelay = 0.3f;
            Engine.TimeRate = 0.25f;
        }

        private void End()
        {
            Level level = Scene as Level;
            level.OnEndOfFrame += () =>
            {
                Glitch.Value = 0.0f;
                Distort.Anxiety = 0.0f;
                Engine.TimeRate = 1f;
                level.Session.ColorGrade = null;
                level.UnloadLevel();
                level.CanRetry = true;
                level.Session.Level = "c-00";
                Session session = level.Session;
                Level level1 = level;
                Rectangle bounds = level.Bounds;
                double left = bounds.Left;
                bounds = level.Bounds;
                double top = bounds.Top;
                Vector2 from = new Vector2((float) left, (float) top);
                Vector2? nullable = level1.GetSpawnPoint(from);
                session.RespawnPoint = nullable;
                level.LoadLevel(Player.IntroTypes.WakeUp);
                Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
            };
        }

        public override void Update()
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                entity.Collidable = true;
            Level scene = Scene as Level;
            base.Update();
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            if (enabled && sprite.CurrentAnimationID != "hatch")
            {
                if (dashTimer > 0.0)
                {
                    speed = Calc.Approach(speed, Vector2.Zero, 800f * Engine.DeltaTime);
                    dashTimer -= Engine.DeltaTime;
                    if (dashTimer <= 0.0)
                        sprite.Play("spotted");
                    if (trailTimerA > 0.0)
                    {
                        trailTimerA -= Engine.DeltaTime;
                        if (trailTimerA <= 0.0)
                            CreateTrail();
                    }
                    if (trailTimerB > 0.0)
                    {
                        trailTimerB -= Engine.DeltaTime;
                        if (trailTimerB <= 0.0)
                            CreateTrail();
                    }
                    if (Scene.OnInterval(0.04f))
                    {
                        Vector2 vector = speed.SafeNormalize();
                        SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, Position + vector * 4f, Vector2.One * 4f, vector.Angle());
                    }
                }
                else
                {
                    Vector2 vector2 = Input.Aim.Value.SafeNormalize();
                    speed += vector2 * 600f * Engine.DeltaTime;
                    float val = speed.Length();
                    if (val > 120.0)
                        speed = speed.SafeNormalize(Calc.Approach(val, 120f, Engine.DeltaTime * 700f));
                    if (vector2.Y == 0.0)
                        speed.Y = Calc.Approach(speed.Y, 0.0f, 400f * Engine.DeltaTime);
                    if (vector2.X == 0.0)
                        speed.X = Calc.Approach(speed.X, 0.0f, 400f * Engine.DeltaTime);
                    if (vector2.Length() > 0.0 && sprite.CurrentAnimationID == "idle")
                    {
                        scene.Displacement.AddBurst(Position, 0.5f, 8f, 32f);
                        sprite.Play("spotted");
                        Audio.Play("event:/game/05_mirror_temple/seeker_playercontrolstart");
                    }
                    int num1 = Math.Sign((int) facing);
                    int num2 = Math.Sign(speed.X);
                    if (num2 != 0 && num1 != num2 && Math.Sign(Input.Aim.Value.X) == Math.Sign(speed.X) && Math.Abs(speed.X) > 20.0 && sprite.CurrentAnimationID != "flipMouth" && sprite.CurrentAnimationID != "flipEyes")
                        sprite.Play("flipMouth");
                    if (Input.Dash.Pressed)
                        Dash(Input.Aim.Value.EightWayNormal());
                }
                MoveH(speed.X * Engine.DeltaTime, OnCollide);
                MoveV(speed.Y * Engine.DeltaTime, OnCollide);
                Vector2 position = Position;
                double x = scene.Bounds.X;
                double y = scene.Bounds.Y;
                Rectangle bounds = scene.Bounds;
                double right = bounds.Right;
                bounds = scene.Bounds;
                double bottom = bounds.Bottom;
                Position = position.Clamp((float) x, (float) y, (float) right, (float) bottom);
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    float val = (Position - entity.Position).Length();
                    if (val < 200.0 && entity.Sprite.CurrentAnimationID == "asleep")
                    {
                        entity.Sprite.Rate = 2f;
                        entity.Sprite.Play("wakeUp");
                    }
                    else if (val < 100.0 && entity.Sprite.CurrentAnimationID != "wakeUp")
                    {
                        entity.Sprite.Rate = 1f;
                        entity.Sprite.Play("runFast");
                        entity.Facing = X > (double) entity.X ? Facings.Left : Facings.Right;
                    }
                    if (val < 50.0 && dashTimer <= 0.0)
                        Dash((entity.Center - Center).SafeNormalize());
                    Engine.TimeRate = Calc.ClampedMap(val, 60f, 220f, 0.5f);
                    Camera camera = scene.Camera;
                    Vector2 cameraTarget = CameraTarget;
                    camera.Position += (cameraTarget - camera.Position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                    Distort.Anxiety = Calc.ClampedMap(val, 0.0f, 200f, 0.25f, 0.0f) + Calc.Random.NextFloat(0.05f);
                    Distort.AnxietyOrigin = (new Vector2(entity.X, scene.Camera.Top) - scene.Camera.Position) / new Vector2(320f, 180f);
                }
                else
                    Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 1f * Engine.DeltaTime);
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<SeekerBarrier>())
                entity.Collidable = false;
        }

        private void CreateTrail()
        {
            Vector2 scale = sprite.Scale;
            sprite.Scale.X *= (float) facing;
            TrailManager.Add(this, Seeker.TrailColor);
            sprite.Scale = scale;
        }

        private void OnCollide(CollisionData data)
        {
            if (dashTimer <= 0.0)
            {
                if (data.Direction.X != 0.0)
                    speed.X = 0.0f;
                if (data.Direction.Y == 0.0)
                    return;
                speed.Y = 0.0f;
            }
            else
            {
                float direction;
                Vector2 position;
                Vector2 positionRange;
                if (data.Direction.X > 0.0)
                {
                    direction = 3.14159274f;
                    position = new Vector2(Right, Y);
                    positionRange = Vector2.UnitY * 4f;
                }
                else if (data.Direction.X < 0.0)
                {
                    direction = 0.0f;
                    position = new Vector2(Left, Y);
                    positionRange = Vector2.UnitY * 4f;
                }
                else if (data.Direction.Y > 0.0)
                {
                    direction = -1.57079637f;
                    position = new Vector2(X, Bottom);
                    positionRange = Vector2.UnitX * 4f;
                }
                else
                {
                    direction = 1.57079637f;
                    position = new Vector2(X, Top);
                    positionRange = Vector2.UnitX * 4f;
                }
                SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, position, positionRange, direction);
                if (data.Hit is SeekerBarrier)
                {
                    (data.Hit as SeekerBarrier).OnReflectSeeker();
                    Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
                }
                else
                    Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
                if (data.Direction.X != 0.0)
                {
                    speed.X *= -0.8f;
                    sprite.Scale = new Vector2(0.6f, 1.4f);
                }
                else if (data.Direction.Y != 0.0)
                {
                    speed.Y *= -0.8f;
                    sprite.Scale = new Vector2(1.4f, 0.6f);
                }
                if (!(data.Hit is TempleCrackedBlock))
                    return;
                Celeste.Freeze(0.15f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                (data.Hit as TempleCrackedBlock).Break(Position);
            }
        }

        private void Dash(Vector2 dir)
        {
            if (dashTimer <= 0.0)
            {
                CreateTrail();
                trailTimerA = 0.1f;
                trailTimerB = 0.25f;
            }
            dashTimer = 0.3f;
            dashDirection = dir;
            if (dashDirection == Vector2.Zero)
                dashDirection.X = Math.Sign((int) facing);
            if (dashDirection.X != 0.0)
                facing = (Facings) Math.Sign(dashDirection.X);
            speed = dashDirection * 400f;
            sprite.Play("attacking");
            SceneAs<Level>().DirectionalShake(dashDirection);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
            if (dashDirection.X == 0.0)
                sprite.Scale = new Vector2(0.6f, 1.4f);
            else
                sprite.Scale = new Vector2(1.4f, 0.6f);
        }

        public Vector2 CameraTarget
        {
            get
            {
                Rectangle bounds = (Scene as Level).Bounds;
                return (Position + new Vector2(-160f, -90f)).Clamp(bounds.Left, bounds.Top, bounds.Right - 320, bounds.Bottom - 180);
            }
        }

        public override void Render()
        {
            if (SaveData.Instance.Assists.InvisibleMotion && enabled && speed.LengthSquared() > 100.0)
                return;
            Vector2 position = Position;
            Position += shaker.Value;
            Vector2 scale = sprite.Scale;
            sprite.Scale.X *= (float) facing;
            base.Render();
            Position = position;
            sprite.Scale = scale;
        }
    }
}
