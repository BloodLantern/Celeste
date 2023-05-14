// Decompiled with JetBrains decompiler
// Type: Celeste.Puffer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class Puffer : Actor
    {
        private const float RespawnTime = 2.5f;
        private const float RespawnMoveTime = 0.5f;
        private const float BounceSpeed = 200f;
        private const float ExplodeRadius = 40f;
        private const float DetectRadius = 32f;
        private const float StunnedAccel = 320f;
        private const float AlertedRadius = 60f;
        private const float CantExplodeTime = 0.5f;
        private readonly Sprite sprite;
        private Puffer.States state;
        private Vector2 startPosition;
        private Vector2 anchorPosition;
        private Vector2 lastSpeedPosition;
        private Vector2 lastSinePosition;
        private readonly Monocle.Circle pushRadius;
        private readonly Monocle.Circle breakWallsRadius;
        private readonly Monocle.Circle detectRadius;
        private readonly SineWave idleSine;
        private Vector2 hitSpeed;
        private float goneTimer;
        private float cannotHitTimer;
        private readonly Collision onCollideV;
        private readonly Collision onCollideH;
        private float alertTimer;
        private readonly Wiggler bounceWiggler;
        private readonly Wiggler inflateWiggler;
        private Vector2 scale;
        private SimpleCurve returnCurve;
        private float cantExplodeTimer;
        private Vector2 lastPlayerPos;
        private float playerAliveFade;
        private Vector2 facing = Vector2.One;
        private float eyeSpin;

        public Puffer(Vector2 position, bool faceRight)
            : base(position)
        {
            Collider = new Hitbox(12f, 10f, -6f, -5f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer), new Hitbox(14f, 12f, -7f, -7f)));
            Add(sprite = GFX.SpriteBank.Create("pufferFish"));
            sprite.Play("idle");
            if (!faceRight)
            {
                facing.X = -1f;
            }

            idleSine = new SineWave(0.5f);
            _ = idleSine.Randomize();
            Add(idleSine);
            anchorPosition = Position;
            Position += new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
            state = Puffer.States.Idle;
            startPosition = lastSinePosition = lastSpeedPosition = Position;
            pushRadius = new Monocle.Circle(40f);
            detectRadius = new Monocle.Circle(32f);
            breakWallsRadius = new Monocle.Circle(16f);
            onCollideV = new Collision(OnCollideV);
            onCollideH = new Collision(OnCollideH);
            scale = Vector2.One;
            bounceWiggler = Wiggler.Create(0.6f, 2.5f, v => sprite.Rotation = (float)((double)v * 20.0 * (Math.PI / 180.0)));
            Add(bounceWiggler);
            inflateWiggler = Wiggler.Create(0.6f, 2f);
            Add(inflateWiggler);
        }

        public Puffer(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("right"))
        {
        }

        public override bool IsRiding(JumpThru jumpThru)
        {
            return false;
        }

        public override bool IsRiding(Solid solid)
        {
            return false;
        }

        protected override void OnSquish(CollisionData data)
        {
            Explode();
            GotoGone();
        }

        private void OnCollideH(CollisionData data)
        {
            hitSpeed.X *= -0.8f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Direction.Y <= 0.0)
            {
                return;
            }

            for (int index1 = -1; index1 <= 1; index1 += 2)
            {
                for (int index2 = 1; index2 <= 2; ++index2)
                {
                    Vector2 at = Position + (Vector2.UnitX * index2 * index1);
                    if (!CollideCheck<Solid>(at) && !OnGround(at))
                    {
                        Position = at;
                        return;
                    }
                }
            }
            hitSpeed.Y *= -0.2f;
        }

        private void GotoIdle()
        {
            if (state == Puffer.States.Gone)
            {
                Position = startPosition;
                cantExplodeTimer = 0.5f;
                sprite.Play("recover");
                _ = Audio.Play("event:/new_content/game/10_farewell/puffer_reform", Position);
            }
            lastSinePosition = lastSpeedPosition = anchorPosition = Position;
            hitSpeed = Vector2.Zero;
            idleSine.Reset();
            state = Puffer.States.Idle;
        }

        private void GotoHit(Vector2 from)
        {
            scale = new Vector2(1.2f, 0.8f);
            hitSpeed = Vector2.UnitY * 200f;
            state = Puffer.States.Hit;
            bounceWiggler.Start();
            Alert(true, false);
            _ = Audio.Play("event:/new_content/game/10_farewell/puffer_boop", Position);
        }

        private void GotoHitSpeed(Vector2 speed)
        {
            hitSpeed = speed;
            state = Puffer.States.Hit;
        }

        private void GotoGone()
        {
            Vector2 control = Position + ((startPosition - Position) * 0.5f);
            if ((double)(startPosition - Position).LengthSquared() > 100.0)
            {
                if ((double)Math.Abs(Position.Y - startPosition.Y) > (double)Math.Abs(Position.X - startPosition.X))
                {
                    if (Position.X > (double)startPosition.X)
                    {
                        control += Vector2.UnitX * -24f;
                    }
                    else
                    {
                        control += Vector2.UnitX * 24f;
                    }
                }
                else if (Position.Y > (double)startPosition.Y)
                {
                    control += Vector2.UnitY * -24f;
                }
                else
                {
                    control += Vector2.UnitY * 24f;
                }
            }
            returnCurve = new SimpleCurve(Position, startPosition, control);
            Collidable = false;
            goneTimer = 2.5f;
            state = Puffer.States.Gone;
        }

        private void Explode()
        {
            Collider collider = Collider;
            Collider = pushRadius;
            _ = Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
            sprite.Play("explode");
            Player player = CollideFirst<Player>();
            if (player != null && !Scene.CollideCheck<Solid>(Position, player.Center))
            {
                _ = player.ExplodeLaunch(Position, false, true);
            }

            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !Scene.CollideCheck<Solid>(Position, theoCrystal.Center))
            {
                theoCrystal.ExplodeLaunch(Position);
            }

            foreach (TempleCrackedBlock entity in Scene.Tracker.GetEntities<TempleCrackedBlock>())
            {
                if (CollideCheck(entity))
                {
                    entity.Break(Position);
                }
            }
            foreach (TouchSwitch entity in Scene.Tracker.GetEntities<TouchSwitch>())
            {
                if (CollideCheck(entity))
                {
                    entity.TurnOn();
                }
            }
            foreach (FloatingDebris entity in Scene.Tracker.GetEntities<FloatingDebris>())
            {
                if (CollideCheck(entity))
                {
                    entity.OnExplode(Position);
                }
            }
            Collider = collider;
            Level level = SceneAs<Level>();
            level.Shake();
            _ = level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            _ = level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            _ = level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
            for (float direction = 0.0f; (double)direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
                level.Particles.Emit(Seeker.P_Regen, position, direction);
            }
        }

        public override void Render()
        {
            sprite.Scale = scale * (float)(1.0 + ((double)inflateWiggler.Value * 0.40000000596046448));
            Sprite sprite1 = sprite;
            sprite1.Scale *= facing;
            bool flag1 = false;
            if (sprite.CurrentAnimationID is not "hidden" and not "explode" and not "recover")
            {
                flag1 = true;
            }
            else if (sprite.CurrentAnimationID == "explode" && sprite.CurrentAnimationFrame <= 1)
            {
                flag1 = true;
            }
            else if (sprite.CurrentAnimationID == "recover" && sprite.CurrentAnimationFrame >= 4)
            {
                flag1 = true;
            }

            if (flag1)
            {
                sprite.DrawSimpleOutline();
            }

            float num1 = playerAliveFade * Calc.ClampedMap((Position - lastPlayerPos).Length(), 128f, 96f);
            if ((double)num1 > 0.0 && state != Puffer.States.Gone)
            {
                bool flag2 = false;
                Vector2 lastPlayerPos = this.lastPlayerPos;
                if (lastPlayerPos.Y < (double)Y)
                {
                    lastPlayerPos.Y = Y - (float)((lastPlayerPos.Y - (double)Y) * 0.5);
                    lastPlayerPos.X += lastPlayerPos.X - X;
                    flag2 = true;
                }
                float radiansB = (lastPlayerPos - Position).Angle();
                for (int index = 0; index < 28; ++index)
                {
                    float num2 = (float)Math.Sin(Scene.TimeActive * 0.5) * 0.02f;
                    float num3 = Calc.Map((index / 28f) + num2, 0.0f, 1f, -1f * (float)Math.PI / 30f, 3.24631262f) + (float)((double)bounceWiggler.Value * 20.0 * (Math.PI / 180.0));
                    Vector2 vector = Calc.AngleToVector(num3, 1f);
                    Vector2 start = Position + (vector * 32f);
                    float t = Calc.ClampedMap(Calc.AbsAngleDiff(num3, radiansB), 1.57079637f, 0.17453292f);
                    float num4 = Ease.CubeOut(t) * 0.8f * num1;
                    if ((double)num4 > 0.0)
                    {
                        if (index is 0 or 27)
                        {
                            Draw.Line(start, start - (vector * 10f), Color.White * num4);
                        }
                        else
                        {
                            Vector2 vector2_1 = vector * (float)Math.Sin((Scene.TimeActive * 2.0) + (index * 0.60000002384185791));
                            if (index % 2 == 0)
                            {
                                vector2_1 *= -1f;
                            }

                            Vector2 vector2_2 = start + vector2_1;
                            if (!flag2 && (double)Calc.AbsAngleDiff(num3, radiansB) <= 0.17453292012214661)
                            {
                                Draw.Line(vector2_2, vector2_2 - (vector * 3f), Color.White * num4);
                            }
                            else
                            {
                                Draw.Point(vector2_2, Color.White * num4);
                            }
                        }
                    }
                }
            }
            base.Render();
            if (sprite.CurrentAnimationID == "alerted")
            {
                Vector2 from = Position + (new Vector2(3f, facing.X < 0.0 ? -5f : -4f) * sprite.Scale);
                Vector2 vector = Calc.AngleToVector(Calc.Angle(from, lastPlayerPos + new Vector2(0.0f, -4f)) + (float)(eyeSpin * 6.2831854820251465 * 2.0), 1f);
                Vector2 vector2 = from + new Vector2((float)Math.Round(vector.X), (float)Math.Round((double)Calc.ClampedMap(vector.Y, -1f, 1f, -1f, 2f)));
                Draw.Rect(vector2.X, vector2.Y, 1f, 1f, Color.Black);
            }
            Sprite sprite2 = sprite;
            sprite2.Scale /= facing;
        }

        public override void Update()
        {
            base.Update();
            eyeSpin = Calc.Approach(eyeSpin, 0.0f, Engine.DeltaTime * 1.5f);
            scale = Calc.Approach(scale, Vector2.One, 1f * Engine.DeltaTime);
            if (cannotHitTimer > 0.0)
            {
                cannotHitTimer -= Engine.DeltaTime;
            }

            if (state != Puffer.States.Gone && cantExplodeTimer > 0.0)
            {
                cantExplodeTimer -= Engine.DeltaTime;
            }

            if (alertTimer > 0.0)
            {
                alertTimer -= Engine.DeltaTime;
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                playerAliveFade = Calc.Approach(playerAliveFade, 0.0f, 1f * Engine.DeltaTime);
            }
            else
            {
                playerAliveFade = Calc.Approach(playerAliveFade, 1f, 1f * Engine.DeltaTime);
                lastPlayerPos = entity.Center;
            }
            switch (state)
            {
                case Puffer.States.Idle:
                    if (Position != lastSinePosition)
                    {
                        anchorPosition += Position - lastSinePosition;
                    }

                    Vector2 vector2 = anchorPosition + new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
                    MoveToX(vector2.X);
                    MoveToY(vector2.Y);
                    lastSinePosition = Position;
                    if (ProximityExplodeCheck())
                    {
                        Explode();
                        GotoGone();
                        break;
                    }
                    if (AlertedCheck())
                    {
                        Alert(false, true);
                    }
                    else if (sprite.CurrentAnimationID == "alerted" && alertTimer <= 0.0)
                    {
                        _ = Audio.Play("event:/new_content/game/10_farewell/puffer_shrink", Position);
                        sprite.Play("unalert");
                    }
                    using (List<Component>.Enumerator enumerator = Scene.Tracker.GetComponents<PufferCollider>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ((PufferCollider)enumerator.Current).Check(this);
                        }

                        break;
                    }
                case Puffer.States.Hit:
                    lastSpeedPosition = Position;
                    _ = MoveH(hitSpeed.X * Engine.DeltaTime, onCollideH);
                    _ = MoveV(hitSpeed.Y * Engine.DeltaTime, new Collision(OnCollideV));
                    anchorPosition = Position;
                    hitSpeed.X = Calc.Approach(hitSpeed.X, 0.0f, 150f * Engine.DeltaTime);
                    hitSpeed = Calc.Approach(hitSpeed, Vector2.Zero, 320f * Engine.DeltaTime);
                    if (ProximityExplodeCheck())
                    {
                        Explode();
                        GotoGone();
                        break;
                    }
                    if ((double)Top >= SceneAs<Level>().Bounds.Bottom + 5)
                    {
                        sprite.Play("hidden");
                        GotoGone();
                        break;
                    }
                    foreach (PufferCollider component in Scene.Tracker.GetComponents<PufferCollider>())
                    {
                        component.Check(this);
                    }

                    if (!(hitSpeed == Vector2.Zero))
                    {
                        break;
                    }

                    ZeroRemainderX();
                    ZeroRemainderY();
                    GotoIdle();
                    break;
                case Puffer.States.Gone:
                    float goneTimer = this.goneTimer;
                    this.goneTimer -= Engine.DeltaTime;
                    if (this.goneTimer <= 0.5)
                    {
                        if ((double)goneTimer > 0.5 && (double)returnCurve.GetLengthParametric(8) > 8.0)
                        {
                            _ = Audio.Play("event:/new_content/game/10_farewell/puffer_return", Position);
                        }

                        Position = returnCurve.GetPoint(Ease.CubeInOut(Calc.ClampedMap(this.goneTimer, 0.5f, 0.0f)));
                    }
                    if (this.goneTimer > 0.0)
                    {
                        break;
                    }

                    Visible = Collidable = true;
                    GotoIdle();
                    break;
            }
        }

        public bool HitSpring(Spring spring)
        {
            switch (spring.Orientation)
            {
                case Spring.Orientations.WallLeft:
                    if (hitSpeed.X > 60.0)
                    {
                        return false;
                    }

                    facing.X = 1f;
                    GotoHitSpeed(280f * Vector2.UnitX);
                    MoveTowardsY(spring.CenterY, 4f);
                    bounceWiggler.Start();
                    Alert(true, false);
                    return true;
                case Spring.Orientations.WallRight:
                    if (hitSpeed.X < -60.0)
                    {
                        return false;
                    }

                    facing.X = -1f;
                    GotoHitSpeed(280f * -Vector2.UnitX);
                    MoveTowardsY(spring.CenterY, 4f);
                    bounceWiggler.Start();
                    Alert(true, false);
                    return true;
                default:
                    if (hitSpeed.Y < 0.0)
                    {
                        return false;
                    }

                    GotoHitSpeed(224f * -Vector2.UnitY);
                    MoveTowardsX(spring.CenterX, 4f);
                    bounceWiggler.Start();
                    Alert(true, false);
                    return true;
            }
        }

        private bool ProximityExplodeCheck()
        {
            if (cantExplodeTimer > 0.0)
            {
                return false;
            }

            bool flag = false;
            Collider collider = Collider;
            Collider = detectRadius;
            Player player;
            if ((player = CollideFirst<Player>()) != null && (double)player.CenterY >= (double)Y + (double)collider.Bottom - 4.0 && !Scene.CollideCheck<Solid>(Position, player.Center))
            {
                flag = true;
            }

            Collider = collider;
            return flag;
        }

        private bool AlertedCheck()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            return entity != null && (double)(entity.Center - Center).Length() < 60.0;
        }

        private void Alert(bool restart, bool playSfx)
        {
            if (sprite.CurrentAnimationID == "idle")
            {
                if (playSfx)
                {
                    _ = Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
                }

                sprite.Play("alert");
                inflateWiggler.Start();
            }
            else if (restart && playSfx)
            {
                _ = Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
            }

            alertTimer = 2f;
        }

        private void OnPlayer(Player player)
        {
            if (state == Puffer.States.Gone || cantExplodeTimer > 0.0)
            {
                return;
            }

            if (cannotHitTimer <= 0.0)
            {
                if ((double)player.Bottom > lastSpeedPosition.Y + 3.0)
                {
                    Explode();
                    GotoGone();
                }
                else
                {
                    player.Bounce(Top);
                    GotoHit(player.Center);
                    MoveToX(anchorPosition.X);
                    idleSine.Reset();
                    anchorPosition = lastSinePosition = Position;
                    eyeSpin = 1f;
                }
            }
            cannotHitTimer = 0.1f;
        }

        private enum States
        {
            Idle,
            Hit,
            Gone,
        }
    }
}
