// Decompiled with JetBrains decompiler
// Type: Celeste.CameraLocker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            LockMode = lockMode;
            MaxXOffset = maxXOffset;
            MaxYOffset = maxYOffset;
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            SceneAs<Level>().CameraLockMode = LockMode;
        }
    }
}
