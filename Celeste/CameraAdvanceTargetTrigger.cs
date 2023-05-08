// Decompiled with JetBrains decompiler
// Type: Celeste.CameraAdvanceTargetTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CameraAdvanceTargetTrigger : Trigger
    {
        public Vector2 Target;
        public Vector2 LerpStrength;
        public PositionModes PositionModeX;
        public PositionModes PositionModeY;
        public bool XOnly;
        public bool YOnly;

        public CameraAdvanceTargetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Target = data.Nodes[0] + offset - (new Vector2(320f, 180f) * 0.5f);
            LerpStrength.X = data.Float("lerpStrengthX");
            LerpStrength.Y = data.Float("lerpStrengthY");
            PositionModeX = data.Enum<PositionModes>("positionModeX");
            PositionModeY = data.Enum<PositionModes>("positionModeY");
            XOnly = data.Bool("xOnly");
            YOnly = data.Bool("yOnly");
        }

        public override void OnStay(Player player)
        {
            player.CameraAnchor = Target;
            player.CameraAnchorLerp.X = MathHelper.Clamp(LerpStrength.X * GetPositionLerp(player, PositionModeX), 0f, 1f);
            player.CameraAnchorLerp.Y = MathHelper.Clamp(LerpStrength.Y * GetPositionLerp(player, PositionModeY), 0f, 1f);
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
