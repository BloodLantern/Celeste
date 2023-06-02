using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CameraLocker : Component
    {
        public const float UpwardMaxYOffset = 180f;
        public Level.CameraLockModes LockMode;
        public float MaxXOffset;
        public float MaxYOffset;

        public CameraLocker(Level.CameraLockModes lockMode, float maxXOffset, float maxYOffset)
            : base(lockMode == Level.CameraLockModes.BoostSequence, false)
        {
            this.LockMode = lockMode;
            this.MaxXOffset = maxXOffset;
            this.MaxYOffset = maxYOffset;
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            this.SceneAs<Level>().CameraLockMode = this.LockMode;
        }
    }
}
