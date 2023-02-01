// Decompiled with JetBrains decompiler
// Type: Celeste.LightFadeTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
  public class LightFadeTrigger : Trigger
  {
    public float LightAddFrom;
    public float LightAddTo;
    public Trigger.PositionModes PositionMode;

    public LightFadeTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.AddTag((int) Tags.TransitionUpdate);
      this.LightAddFrom = data.Float("lightAddFrom");
      this.LightAddTo = data.Float("lightAddTo");
      this.PositionMode = data.Enum<Trigger.PositionModes>("positionMode");
    }

    public override void OnStay(Player player)
    {
      Level scene = this.Scene as Level;
      Session session = scene.Session;
      float num1 = this.LightAddFrom + (this.LightAddTo - this.LightAddFrom) * MathHelper.Clamp(this.GetPositionLerp(player, this.PositionMode), 0.0f, 1f);
      double num2 = (double) num1;
      session.LightingAlphaAdd = (float) num2;
      scene.Lighting.Alpha = scene.BaseLightingAlpha + num1;
    }
  }
}
