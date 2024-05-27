using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(true)]
    public abstract class Platform : Entity
    {
        private Vector2 movementCounter;
        private Vector2 shakeAmount;
        private bool shaking;
        private float shakeTimer;
        protected List<StaticMover> staticMovers = new List<StaticMover>();
        public Vector2 LiftSpeed;
        public bool Safe;
        public bool BlockWaterfalls = true;
        public int SurfaceSoundIndex = 8;
        public int SurfaceSoundPriority;
        public DashCollision OnDashCollide;
        public Action<Vector2> OnCollide;

        public Vector2 Shake => shakeAmount;

        public Hitbox Hitbox => Collider as Hitbox;

        public Vector2 ExactPosition => Position + movementCounter;

        public Platform(Vector2 position, bool safe)
            : base(position)
        {
            Safe = safe;
            Depth = -9000;
        }

        public void ClearRemainder() => movementCounter = Vector2.Zero;

        public override void Update()
        {
            base.Update();
            LiftSpeed = Vector2.Zero;
            if (!shaking)
                return;
            if (Scene.OnInterval(0.04f))
            {
                Vector2 shakeAmount = this.shakeAmount;
                this.shakeAmount = Calc.Random.ShakeVector();
                OnShake(this.shakeAmount - shakeAmount);
            }
            if (shakeTimer <= 0.0)
                return;
            shakeTimer -= Engine.DeltaTime;
            if (shakeTimer > 0.0)
                return;
            shaking = false;
            StopShaking();
        }

        public void StartShaking(float time = 0.0f)
        {
            shaking = true;
            shakeTimer = time;
        }

        public void StopShaking()
        {
            shaking = false;
            if (!(shakeAmount != Vector2.Zero))
                return;
            OnShake(-shakeAmount);
            shakeAmount = Vector2.Zero;
        }

        public virtual void OnShake(Vector2 amount) => ShakeStaticMovers(amount);

        public void ShakeStaticMovers(Vector2 amount)
        {
            foreach (StaticMover staticMover in staticMovers)
                staticMover.Shake(amount);
        }

        public void MoveStaticMovers(Vector2 amount)
        {
            foreach (StaticMover staticMover in staticMovers)
                staticMover.Move(amount);
        }

        public void DestroyStaticMovers()
        {
            foreach (StaticMover staticMover in staticMovers)
                staticMover.Destroy();
            staticMovers.Clear();
        }

        public void DisableStaticMovers()
        {
            foreach (StaticMover staticMover in staticMovers)
                staticMover.Disable();
        }

        public void EnableStaticMovers()
        {
            foreach (StaticMover staticMover in staticMovers)
                staticMover.Enable();
        }

        public virtual void OnStaticMoverTrigger(StaticMover sm)
        {
        }

        public virtual int GetLandSoundIndex(Entity entity) => SurfaceSoundIndex;

        public virtual int GetWallSoundIndex(Player player, int side) => SurfaceSoundIndex;

        public virtual int GetStepSoundIndex(Entity entity) => SurfaceSoundIndex;

        public void MoveH(float moveH)
        {
            LiftSpeed.X = Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
            movementCounter.X += moveH;
            int move = (int) Math.Round(movementCounter.X);
            if (move == 0)
                return;
            movementCounter.X -= move;
            MoveHExact(move);
        }

        public void MoveH(float moveH, float liftSpeedH)
        {
            LiftSpeed.X = liftSpeedH;
            movementCounter.X += moveH;
            int move = (int) Math.Round(movementCounter.X);
            if (move == 0)
                return;
            movementCounter.X -= move;
            MoveHExact(move);
        }

        public void MoveV(float moveV)
        {
            LiftSpeed.Y = Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
            movementCounter.Y += moveV;
            int move = (int) Math.Round(movementCounter.Y);
            if (move == 0)
                return;
            movementCounter.Y -= move;
            MoveVExact(move);
        }

        public void MoveV(float moveV, float liftSpeedV)
        {
            LiftSpeed.Y = liftSpeedV;
            movementCounter.Y += moveV;
            int move = (int) Math.Round(movementCounter.Y);
            if (move == 0)
                return;
            movementCounter.Y -= move;
            MoveVExact(move);
        }

        public void MoveToX(float x) => MoveH(x - ExactPosition.X);

        public void MoveToX(float x, float liftSpeedX) => MoveH(x - ExactPosition.X, liftSpeedX);

        public void MoveToY(float y) => MoveV(y - ExactPosition.Y);

        public void MoveToY(float y, float liftSpeedY) => MoveV(y - ExactPosition.Y, liftSpeedY);

        public void MoveTo(Vector2 position)
        {
            MoveToX(position.X);
            MoveToY(position.Y);
        }

        public void MoveTo(Vector2 position, Vector2 liftSpeed)
        {
            MoveToX(position.X, liftSpeed.X);
            MoveToY(position.Y, liftSpeed.Y);
        }

        public void MoveTowardsX(float x, float amount) => MoveToX(Calc.Approach(ExactPosition.X, x, amount));

        public void MoveTowardsY(float y, float amount) => MoveToY(Calc.Approach(ExactPosition.Y, y, amount));

        public abstract void MoveHExact(int move);

        public abstract void MoveVExact(int move);

        public void MoveToNaive(Vector2 position)
        {
            MoveToXNaive(position.X);
            MoveToYNaive(position.Y);
        }

        public void MoveToXNaive(float x) => MoveHNaive(x - ExactPosition.X);

        public void MoveToYNaive(float y) => MoveVNaive(y - ExactPosition.Y);

        public void MoveHNaive(float moveH)
        {
            LiftSpeed.X = Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
            movementCounter.X += moveH;
            int num = (int) Math.Round(movementCounter.X);
            if (num == 0)
                return;
            movementCounter.X -= num;
            X += num;
            MoveStaticMovers(Vector2.UnitX * num);
        }

        public void MoveVNaive(float moveV)
        {
            LiftSpeed.Y = Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
            movementCounter.Y += moveV;
            int num = (int) Math.Round(movementCounter.Y);
            if (num == 0)
                return;
            movementCounter.Y -= num;
            Y += num;
            MoveStaticMovers(Vector2.UnitY * num);
        }

        public bool MoveHCollideSolids(
            float moveH,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null)
        {
            LiftSpeed.X = Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
            movementCounter.X += moveH;
            int moveH1 = (int) Math.Round(movementCounter.X);
            if (moveH1 == 0)
                return false;
            movementCounter.X -= moveH1;
            return MoveHExactCollideSolids(moveH1, thruDashBlocks, onCollide);
        }

        public bool MoveVCollideSolids(
            float moveV,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null)
        {
            LiftSpeed.Y = Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
            movementCounter.Y += moveV;
            int moveV1 = (int) Math.Round(movementCounter.Y);
            if (moveV1 == 0)
                return false;
            movementCounter.Y -= moveV1;
            return MoveVExactCollideSolids(moveV1, thruDashBlocks, onCollide);
        }

        public bool MoveHCollideSolidsAndBounds(
            Level level,
            float moveH,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null)
        {
            LiftSpeed.X = Engine.DeltaTime != 0.0 ? moveH / Engine.DeltaTime : 0.0f;
            movementCounter.X += moveH;
            int moveH1 = (int) Math.Round(movementCounter.X);
            if (moveH1 == 0)
                return false;
            movementCounter.X -= moveH1;
            double num1 = Left + (double) moveH1;
            Rectangle bounds = level.Bounds;
            double left = bounds.Left;
            bool flag;
            if (num1 < left)
            {
                flag = true;
                bounds = level.Bounds;
                moveH1 = bounds.Left - (int) Left;
            }
            else
            {
                double num2 = Right + (double) moveH1;
                bounds = level.Bounds;
                double right = bounds.Right;
                if (num2 > right)
                {
                    flag = true;
                    bounds = level.Bounds;
                    moveH1 = bounds.Right - (int) Right;
                }
                else
                    flag = false;
            }
            return MoveHExactCollideSolids(moveH1, thruDashBlocks, onCollide) | flag;
        }

        public bool MoveVCollideSolidsAndBounds(
            Level level,
            float moveV,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null,
            bool checkBottom = true)
        {
            LiftSpeed.Y = Engine.DeltaTime != 0.0 ? moveV / Engine.DeltaTime : 0.0f;
            movementCounter.Y += moveV;
            int moveV1 = (int) Math.Round(movementCounter.Y);
            if (moveV1 == 0)
                return false;
            movementCounter.Y -= moveV1;
            int num = level.Bounds.Bottom + 32;
            bool flag;
            if (Top + (double) moveV1 < level.Bounds.Top)
            {
                flag = true;
                moveV1 = level.Bounds.Top - (int) Top;
            }
            else if (checkBottom && Bottom + (double) moveV1 > num)
            {
                flag = true;
                moveV1 = num - (int) Bottom;
            }
            else
                flag = false;
            return MoveVExactCollideSolids(moveV1, thruDashBlocks, onCollide) | flag;
        }

        public bool MoveHExactCollideSolids(
            int moveH,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null)
        {
            float x = X;
            int num = Math.Sign(moveH);
            int move = 0;
            Solid solid = null;
            while (moveH != 0)
            {
                if (thruDashBlocks)
                {
                    foreach (DashBlock entity in Scene.Tracker.GetEntities<DashBlock>())
                    {
                        if (CollideCheck(entity, Position + Vector2.UnitX * num))
                        {
                            entity.Break(Center, Vector2.UnitX * num);
                            SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }
                solid = CollideFirst<Solid>(Position + Vector2.UnitX * num);
                if (solid == null)
                {
                    move += num;
                    moveH -= num;
                    X += num;
                }
                else
                    break;
            }
            X = x;
            MoveHExact(move);
            if (solid != null && onCollide != null)
                onCollide(Vector2.UnitX * num, Vector2.UnitX * move, solid);
            return solid != null;
        }

        public bool MoveVExactCollideSolids(
            int moveV,
            bool thruDashBlocks,
            Action<Vector2, Vector2, Platform> onCollide = null)
        {
            float y = Y;
            int num = Math.Sign(moveV);
            int move = 0;
            Platform platform = null;
            while (moveV != 0)
            {
                if (thruDashBlocks)
                {
                    foreach (DashBlock entity in Scene.Tracker.GetEntities<DashBlock>())
                    {
                        if (CollideCheck(entity, Position + Vector2.UnitY * num))
                        {
                            entity.Break(Center, Vector2.UnitY * num);
                            SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }
                platform = CollideFirst<Solid>(Position + Vector2.UnitY * num);
                if (platform == null)
                {
                    if (moveV > 0)
                    {
                        platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * num);
                        if (platform != null)
                            break;
                    }
                    move += num;
                    moveV -= num;
                    Y += num;
                }
                else
                    break;
            }
            Y = y;
            MoveVExact(move);
            if (platform != null && onCollide != null)
                onCollide(Vector2.UnitY * num, Vector2.UnitY * move, platform);
            return platform != null;
        }
    }
}
