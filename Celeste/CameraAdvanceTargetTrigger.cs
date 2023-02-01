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
    public Trigger.PositionModes PositionModeX;
    public Trigger.PositionModes PositionModeY;
    public bool XOnly;
    public bool YOnly;

    public CameraAdvanceTargetTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.Target = data.Nodes[0] + offset - new Vector2(320f, 180f) * 0.5f;
      this.LerpStrength.X = data.Float("lerpStrengthX");
      this.LerpStrength.Y = data.Float("lerpStrengthY");
      this.PositionModeX = data.Enum<Trigger.PositionModes>("positionModeX");
      this.PositionModeY = data.Enum<Trigger.PositionModes>("positionModeY");
      this.XOnly = data.Bool("xOnly");
      this.YOnly = data.Bool("yOnly");
    }

    public override void OnStay(Player player)
    {
      player.CameraAnchor = this.Target;
      player.CameraAnchorLerp.X = MathHelper.Clamp(this.LerpStrength.X * this.GetPositionLerp(player, this.PositionModeX), 0.0f, 1f);
      player.CameraAnchorLerp.Y = MathHelper.Clamp(this.LerpStrength.Y * this.GetPositionLerp(player, this.PositionModeY), 0.0f, 1f);
      player.CameraAnchorIgnoreX = this.YOnly;
      player.CameraAnchorIgnoreY = this.XOnly;
    }

    public override void OnLeave(Player player)
    {
      base.OnLeave(player);
      bool flag = false;
      foreach (Trigger entity in this.Scene.Tracker.GetEntities<CameraTargetTrigger>())
      {
        if (entity.PlayerIsInside)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
      {
        foreach (Trigger entity in this.Scene.Tracker.GetEntities<CameraAdvanceTargetTrigger>())
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
