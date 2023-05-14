// Decompiled with JetBrains decompiler
// Type: Celeste.MountainCamera
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public struct MountainCamera
    {
        public Vector3 Position;
        public Vector3 Target;
        public Quaternion Rotation;

        public MountainCamera(Vector3 pos, Vector3 target)
        {
            Position = pos;
            Target = target;
            Rotation = new Quaternion().LookAt(Position, Target, Vector3.Up);
        }

        public void LookAt(Vector3 pos)
        {
            Target = pos;
            Rotation = new Quaternion().LookAt(Position, Target, Vector3.Up);
        }
    }
}
