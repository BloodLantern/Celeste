// Decompiled with JetBrains decompiler
// Type: Celeste.Holdable
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class Holdable : Component
    {
        public Collider PickupCollider;
        public Action OnPickup;
        public Action<Vector2> OnCarry;
        public Action<Vector2> OnRelease;
        public Func<HoldableCollider, bool> DangerousCheck;
        public Action<Seeker> OnHitSeeker;
        public Action<HoldableCollider, int> OnSwat;
        public Func<Spring, bool> OnHitSpring;
        public Action<Entity> OnHitSpinner;
        public Func<Vector2> SpeedGetter;
        public bool SlowRun = true;
        public bool SlowFall;
        private readonly float cannotHoldDelay;
        private Vector2 startPos;
        private float gravityTimer;
        private float cannotHoldTimer;
        private int idleDepth;

        public Player Holder { get; private set; }

        public Holdable(float cannotHoldDelay = 0.1f)
            : base(true, false)
        {
            this.cannotHoldDelay = cannotHoldDelay;
        }

        public bool Check(Player player)
        {
            Collider collider = Entity.Collider;
            if (PickupCollider != null)
            {
                Entity.Collider = PickupCollider;
            }

            int num = player.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider;
            return num != 0;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            startPos = Entity.Position;
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            if (Holder is null or null)
            {
                return;
            }

            Holder.Holding = null;
        }

        public bool Pickup(Player player)
        {
            if (cannotHoldTimer > 0.0 || Scene == null || Entity.Scene == null)
            {
                return false;
            }

            idleDepth = Entity.Depth;
            Entity.Depth = player.Depth - 1;
            Entity.Visible = true;
            Holder = player;
            OnPickup?.Invoke();
            return true;
        }

        public void Carry(Vector2 position)
        {
            if (OnCarry != null)
            {
                OnCarry(position);
            }
            else
            {
                Entity.Position = position;
            }
        }

        public void Release(Vector2 force)
        {
            if (Entity.CollideCheck<Solid>())
            {
                if (force.X != 0.0)
                {
                    bool flag = false;
                    int num1 = Math.Sign(force.X);
                    int num2 = 0;
                    while (!flag && num2++ < 10)
                    {
                        if (!Entity.CollideCheck<Solid>(Entity.Position + (num1 * num2 * Vector2.UnitX)))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        Entity.X += num1 * num2;
                    }
                }
                while (Entity.CollideCheck<Solid>())
                {
                    Entity.Position += Vector2.UnitY;
                }
            }
            Entity.Depth = idleDepth;
            Holder = null;
            gravityTimer = 0.1f;
            cannotHoldTimer = cannotHoldDelay;
            if (OnRelease == null)
            {
                return;
            }

            OnRelease(force);
        }

        public bool IsHeld => Holder != null;

        public bool ShouldHaveGravity => gravityTimer <= 0.0;

        public override void Update()
        {
            base.Update();
            if (cannotHoldTimer > 0.0)
            {
                cannotHoldTimer -= Engine.DeltaTime;
            }

            if (gravityTimer <= 0.0)
            {
                return;
            }

            gravityTimer -= Engine.DeltaTime;
        }

        public void CheckAgainstColliders()
        {
            foreach (HoldableCollider component in Scene.Tracker.GetComponents<HoldableCollider>())
            {
                if (component.Check(this))
                {
                    component.OnCollide(this);
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (PickupCollider == null)
            {
                return;
            }

            Collider collider = Entity.Collider;
            Entity.Collider = PickupCollider;
            Entity.Collider.Render(camera, Color.Pink);
            Entity.Collider = collider;
        }

        public bool Dangerous(HoldableCollider hc)
        {
            return DangerousCheck != null && DangerousCheck(hc);
        }

        public void HitSeeker(Seeker seeker)
        {
            if (OnHitSeeker == null)
            {
                return;
            }

            OnHitSeeker(seeker);
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (OnSwat == null)
            {
                return;
            }

            OnSwat(hc, dir);
        }

        public bool HitSpring(Spring spring)
        {
            return OnHitSpring != null && OnHitSpring(spring);
        }

        public void HitSpinner(Entity spinner)
        {
            if (OnHitSpinner == null)
            {
                return;
            }

            OnHitSpinner(spinner);
        }

        public Vector2 GetSpeed()
        {
            return SpeedGetter != null ? SpeedGetter() : Vector2.Zero;
        }
    }
}
