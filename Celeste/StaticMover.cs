// Decompiled with JetBrains decompiler
// Type: Celeste.StaticMover
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class StaticMover : Component
    {
        public Func<Solid, bool> SolidChecker;
        public Func<JumpThru, bool> JumpThruChecker;
        public Action<Vector2> OnMove;
        public Action<Vector2> OnShake;
        public Action<Platform> OnAttach;
        public Action OnDestroy;
        public Action OnDisable;
        public Action OnEnable;
        public Platform Platform;

        public StaticMover()
            : base(false, false)
        {
        }

        public void Destroy()
        {
            if (OnDestroy != null)
            {
                OnDestroy();
            }
            else
            {
                Entity.RemoveSelf();
            }
        }

        public void Shake(Vector2 amount)
        {
            if (OnShake == null)
            {
                return;
            }

            OnShake(amount);
        }

        public void Move(Vector2 amount)
        {
            if (OnMove != null)
            {
                OnMove(amount);
            }
            else
            {
                Entity.Position += amount;
            }
        }

        public bool IsRiding(Solid solid)
        {
            return SolidChecker != null && SolidChecker(solid);
        }

        public bool IsRiding(JumpThru jumpThru)
        {
            return JumpThruChecker != null && JumpThruChecker(jumpThru);
        }

        public void TriggerPlatform()
        {
            if (Platform == null)
            {
                return;
            }

            Platform.OnStaticMoverTrigger(this);
        }

        public void Disable()
        {
            if (OnDisable != null)
            {
                OnDisable();
            }
            else
            {
                Entity.Active = Entity.Visible = Entity.Collidable = false;
            }
        }

        public void Enable()
        {
            if (OnEnable != null)
            {
                OnEnable();
            }
            else
            {
                Entity.Active = Entity.Visible = Entity.Collidable = true;
            }
        }
    }
}
