// Decompiled with JetBrains decompiler
// Type: Celeste.ChangeRespawnTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class ChangeRespawnTrigger : Trigger
    {
        public Vector2 Target;

        public ChangeRespawnTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
            this.Target = data.Nodes == null || data.Nodes.Length == 0 ? this.Center : data.Nodes[0] + offset;
            this.Visible = this.Active = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.Target = this.SceneAs<Level>().GetSpawnPoint(this.Target);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Session session = (this.Scene as Level).Session;
            if (!this.SolidCheck() || session.RespawnPoint.HasValue && !(session.RespawnPoint.Value != this.Target))
                return;
            session.HitCheckpoint = true;
            session.RespawnPoint = new Vector2?(this.Target);
            session.UpdateLevelStartDashes();
        }

        private bool SolidCheck()
        {
            Vector2 point = this.Target + Vector2.UnitY * -4f;
            return !this.Scene.CollideCheck<Solid>(point) || this.Scene.CollideCheck<FloatySpaceBlock>(point);
        }
    }
}
