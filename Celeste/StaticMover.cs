using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
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
                OnDestroy();
            else
                Entity.RemoveSelf();
        }

        public void Shake(Vector2 amount)
        {
            if (OnShake == null)
                return;
            OnShake(amount);
        }

        public void Move(Vector2 amount)
        {
            if (OnMove != null)
                OnMove(amount);
            else
                Entity.Position += amount;
        }

        public bool IsRiding(Solid solid) => SolidChecker != null && SolidChecker(solid);

        public bool IsRiding(JumpThru jumpThru) => JumpThruChecker != null && JumpThruChecker(jumpThru);

        public void TriggerPlatform()
        {
            if (Platform == null)
                return;
            Platform.OnStaticMoverTrigger(this);
        }

        public void Disable()
        {
            if (OnDisable != null)
                OnDisable();
            else
                Entity.Active = Entity.Visible = Entity.Collidable = false;
        }

        public void Enable()
        {
            if (OnEnable != null)
                OnEnable();
            else
                Entity.Active = Entity.Visible = Entity.Collidable = true;
        }
    }
}
