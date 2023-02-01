// Decompiled with JetBrains decompiler
// Type: Celeste.CameraTargetTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class CameraTargetTrigger : Trigger
  {
    public Vector2 Target;
    public float LerpStrength;
    public Trigger.PositionModes PositionMode;
    public bool XOnly;
    public bool YOnly;
    public string DeleteFlag;

    public CameraTargetTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.Target = data.Nodes[0] + offset - new Vector2(320f, 180f) * 0.5f;
      this.LerpStrength = data.Float("lerpStrength");
      this.PositionMode = data.Enum<Trigger.PositionModes>("positionMode");
      this.XOnly = data.Bool("xOnly");
      this.YOnly = data.Bool("yOnly");
      this.DeleteFlag = data.Attr("deleteFlag");
    }

    public override void OnStay(Player player)
    {
      if (!string.IsNullOrEmpty(this.DeleteFlag) && this.SceneAs<Level>().Session.GetFlag(this.DeleteFlag))
        return;
      player.CameraAnchor = this.Target;
      player.CameraAnchorLerp = Vector2.One * MathHelper.Clamp(this.LerpStrength * this.GetPositionLerp(player, this.PositionMode), 0.0f, 1f);
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
