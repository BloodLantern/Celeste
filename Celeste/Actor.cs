// Decompiled with JetBrains decompiler
// Type: Celeste.Actor
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(true)]
    public class Actor : Entity
    {
        public Collision SquishCallback;
        public bool TreatNaive;
        private Vector2 movementCounter;
        public bool IgnoreJumpThrus;
        public bool AllowPushing = true;
        public float LiftSpeedGraceTime = 0.16f;
        private Vector2 currentLiftSpeed;
        private Vector2 lastLiftSpeed;
        private float liftSpeedTimer;

        public Actor(Vector2 position)
            : base(position)
        {
            SquishCallback = new Collision(OnSquish);
        }

        protected virtual void OnSquish(CollisionData data)
        {
            if (TrySquishWiggle(data))
            {
                return;
            }

            RemoveSelf();
        }

        protected bool TrySquishWiggle(CollisionData data, int wiggleX = 3, int wiggleY = 3)
        {
            data.Pusher.Collidable = true;
            for (int index1 = 0; index1 <= wiggleX; ++index1)
            {
                for (int index2 = 0; index2 <= wiggleY; ++index2)
                {
                    if (index1 != 0 || index2 != 0)
                    {
                        for (int index3 = 1; index3 >= -1; index3 -= 2)
                        {
                            for (int index4 = 1; index4 >= -1; index4 -= 2)
                            {
                                Vector2 vector2 = new(index1 * index3, index2 * index4);
                                if (!CollideCheck<Solid>(Position + vector2))
                                {
                                    Position += vector2;
                                    data.Pusher.Collidable = false;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            for (int index5 = 0; index5 <= wiggleX; ++index5)
            {
                for (int index6 = 0; index6 <= wiggleY; ++index6)
                {
                    if (index5 != 0 || index6 != 0)
                    {
                        for (int index7 = 1; index7 >= -1; index7 -= 2)
                        {
                            for (int index8 = 1; index8 >= -1; index8 -= 2)
                            {
                                Vector2 vector2 = new(index5 * index7, index6 * index8);
                                if (!CollideCheck<Solid>(data.TargetPosition + vector2))
                                {
                                    Position = data.TargetPosition + vector2;
                                    data.Pusher.Collidable = false;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            data.Pusher.Collidable = false;
            return false;
        }

        public virtual bool IsRiding(JumpThru jumpThru)
        {
            return !IgnoreJumpThrus && CollideCheckOutside(jumpThru, Position + Vector2.UnitY);
        }

        public virtual bool IsRiding(Solid solid)
        {
            return CollideCheck(solid, Position + Vector2.UnitY);
        }

        public bool OnGround(int downCheck = 1)
        {
            return CollideCheck<Solid>(Position + (Vector2.UnitY * downCheck))
|| (!IgnoreJumpThrus && CollideCheckOutside<JumpThru>(Position + (Vector2.UnitY * downCheck)));
        }

        public bool OnGround(Vector2 at, int downCheck = 1)
        {
            Vector2 position = Position;
            Position = at;
            int num = OnGround(downCheck) ? 1 : 0;
            Position = position;
            return num != 0;
        }

        public Vector2 ExactPosition => Position + movementCounter;

        public Vector2 PositionRemainder => movementCounter;

        public void ZeroRemainderX()
        {
            movementCounter.X = 0.0f;
        }

        public void ZeroRemainderY()
        {
            movementCounter.Y = 0.0f;
        }

        public override void Update()
        {
            base.Update();
            LiftSpeed = Vector2.Zero;
            if (liftSpeedTimer <= 0.0)
            {
                return;
            }

            liftSpeedTimer -= Engine.DeltaTime;
            if (liftSpeedTimer > 0.0)
            {
                return;
            }

            lastLiftSpeed = Vector2.Zero;
        }

        public Vector2 LiftSpeed
        {
            set
            {
                currentLiftSpeed = value;
                if (!(value != Vector2.Zero) || LiftSpeedGraceTime <= 0.0)
                {
                    return;
                }

                lastLiftSpeed = value;
                liftSpeedTimer = LiftSpeedGraceTime;
            }
            get => currentLiftSpeed == Vector2.Zero ? lastLiftSpeed : currentLiftSpeed;
        }

        public void ResetLiftSpeed()
        {
            currentLiftSpeed = lastLiftSpeed = Vector2.Zero;
            liftSpeedTimer = 0.0f;
        }

        public bool MoveH(float moveH, Collision onCollide = null, Solid pusher = null)
        {
            movementCounter.X += moveH;
            int moveH1 = (int)Math.Round(movementCounter.X, MidpointRounding.ToEven);
            if (moveH1 == 0)
            {
                return false;
            }

            movementCounter.X -= moveH1;
            return MoveHExact(moveH1, onCollide, pusher);
        }

        public bool MoveV(float moveV, Collision onCollide = null, Solid pusher = null)
        {
            movementCounter.Y += moveV;
            int moveV1 = (int)Math.Round(movementCounter.Y, MidpointRounding.ToEven);
            if (moveV1 == 0)
            {
                return false;
            }

            movementCounter.Y -= moveV1;
            return MoveVExact(moveV1, onCollide, pusher);
        }

        public bool MoveHExact(int moveH, Collision onCollide = null, Solid pusher = null)
        {
            Vector2 vector2 = Position + (Vector2.UnitX * moveH);
            int direction = Math.Sign(moveH);
            int num2 = 0;
            while (moveH != 0)
            {
                Solid solid = CollideFirst<Solid>(Position + (Vector2.UnitX * direction));
                if (solid != null)
                {
                    movementCounter.X = 0.0f;
                    onCollide?.Invoke(new CollisionData()
                    {
                        Direction = Vector2.UnitX * direction,
                        Moved = Vector2.UnitX * num2,
                        TargetPosition = vector2,
                        Hit = solid,
                        Pusher = pusher
                    });
                    return true;
                }
                num2 += direction;
                moveH -= direction;
                X += direction;
            }
            return false;
        }

        public bool MoveVExact(int moveV, Collision onCollide = null, Solid pusher = null)
        {
            Vector2 vector2 = Position + (Vector2.UnitY * moveV);
            int num1 = Math.Sign(moveV);
            int num2 = 0;
            while (moveV != 0)
            {
                Platform platform1 = CollideFirst<Solid>(Position + (Vector2.UnitY * num1));
                if (platform1 != null)
                {
                    movementCounter.Y = 0.0f;
                    onCollide?.Invoke(new CollisionData()
                    {
                        Direction = Vector2.UnitY * num1,
                        Moved = Vector2.UnitY * num2,
                        TargetPosition = vector2,
                        Hit = platform1,
                        Pusher = pusher
                    });
                    return true;
                }
                if (moveV > 0 && !IgnoreJumpThrus)
                {
                    Platform platform2 = CollideFirstOutside<JumpThru>(Position + (Vector2.UnitY * num1));
                    if (platform2 != null)
                    {
                        movementCounter.Y = 0.0f;
                        onCollide?.Invoke(new CollisionData()
                        {
                            Direction = Vector2.UnitY * num1,
                            Moved = Vector2.UnitY * num2,
                            TargetPosition = vector2,
                            Hit = platform2,
                            Pusher = pusher
                        });
                        return true;
                    }
                }
                num2 += num1;
                moveV -= num1;
                Y += num1;
            }
            return false;
        }

        public void MoveTowardsX(float targetX, float maxAmount, Collision onCollide = null)
        {
            MoveToX(Calc.Approach(ExactPosition.X, targetX, maxAmount), onCollide);
        }

        public void MoveTowardsY(float targetY, float maxAmount, Collision onCollide = null)
        {
            MoveToY(Calc.Approach(ExactPosition.Y, targetY, maxAmount), onCollide);
        }

        public void MoveToX(float toX, Collision onCollide = null)
        {
            _ = MoveH(toX - ExactPosition.X, onCollide);
        }

        public void MoveToY(float toY, Collision onCollide = null)
        {
            _ = MoveV(toY - ExactPosition.Y, onCollide);
        }

        public void NaiveMove(Vector2 amount)
        {
            movementCounter += amount;
            int x = (int)Math.Round(movementCounter.X);
            int y = (int)Math.Round(movementCounter.Y);
            Position += new Vector2(x, y);
            movementCounter -= new Vector2(x, y);
        }
    }
}
