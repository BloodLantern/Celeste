// Decompiled with JetBrains decompiler
// Type: Celeste.CameraOffsetTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
