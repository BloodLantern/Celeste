﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class ChangeRespawnTrigger : Trigger
    {
        public Vector2 Target;

        public ChangeRespawnTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Target = data.Nodes == null || data.Nodes.Length == 0 ? Center : data.Nodes[0] + offset;
            Visible = Active = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Target = SceneAs<Level>().GetSpawnPoint(Target);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Session session = (Scene as Level).Session;
            if (!SolidCheck() || session.RespawnPoint.HasValue && !(session.RespawnPoint.Value != Target))
                return;
            session.HitCheckpoint = true;
            session.RespawnPoint = Target;
            session.UpdateLevelStartDashes();
        }

        private bool SolidCheck()
        {
            Vector2 point = Target + Vector2.UnitY * -4f;
            return !Scene.CollideCheck<Solid>(point) || Scene.CollideCheck<FloatySpaceBlock>(point);
        }
    }
}
