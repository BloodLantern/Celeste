﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
    public class CameraTargetTrigger : Trigger
    {
        public Vector2 Target;
        public float LerpStrength;
        public PositionModes PositionMode;
        public bool XOnly;
        public bool YOnly;
        public string DeleteFlag;

        public CameraTargetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Target = data.Nodes[0] + offset - new Vector2(320f, 180f) * 0.5f;
            LerpStrength = data.Float("lerpStrength");
            PositionMode = data.Enum<PositionModes>("positionMode");
            XOnly = data.Bool("xOnly");
            YOnly = data.Bool("yOnly");
            DeleteFlag = data.Attr("deleteFlag");
        }

        public override void OnStay(Player player)
        {
            if (!string.IsNullOrEmpty(DeleteFlag) && SceneAs<Level>().Session.GetFlag(DeleteFlag))
                return;
            player.CameraAnchor = Target;
            player.CameraAnchorLerp = Vector2.One * MathHelper.Clamp(LerpStrength * GetPositionLerp(player, PositionMode), 0.0f, 1f);
            player.CameraAnchorIgnoreX = YOnly;
            player.CameraAnchorIgnoreY = XOnly;
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            bool flag = false;
            foreach (Trigger entity in Scene.Tracker.GetEntities<CameraTargetTrigger>())
            {
                if (entity.PlayerIsInside)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                foreach (Trigger entity in Scene.Tracker.GetEntities<CameraAdvanceTargetTrigger>())
                {
                    if (entity.PlayerIsInside)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
                return;
            player.CameraAnchorLerp = Vector2.Zero;
        }
    }
}
