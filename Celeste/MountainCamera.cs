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
            this.Position = pos;
            this.Target = target;
            this.Rotation = new Quaternion().LookAt(this.Position, this.Target, Vector3.Up);
        }

        public void LookAt(Vector3 pos)
        {
            this.Target = pos;
            this.Rotation = new Quaternion().LookAt(this.Position, this.Target, Vector3.Up);
        }
    }
}
