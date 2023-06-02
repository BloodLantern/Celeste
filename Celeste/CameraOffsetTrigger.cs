using Microsoft.Xna.Framework;

namespace Celeste
{
    public class CameraOffsetTrigger : Trigger
    {
        public Vector2 CameraOffset;

        public CameraOffsetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.CameraOffset = new Vector2(data.Float("cameraX"), data.Float("cameraY"));
            this.CameraOffset.X *= 48f;
            this.CameraOffset.Y *= 32f;
        }

        public override void OnEnter(Player player) => this.SceneAs<Level>().CameraOffset = this.CameraOffset;
    }
}
