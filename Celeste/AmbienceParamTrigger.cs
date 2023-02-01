// Decompiled with JetBrains decompiler
// Type: Celeste.AmbienceParamTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class AmbienceParamTrigger : Trigger
  {
    public string Parameter;
    public float From;
    public float To;
    public Trigger.PositionModes PositionMode;

    public AmbienceParamTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.Parameter = data.Attr("parameter");
      this.From = data.Float("from");
      this.To = data.Float("to");
      this.PositionMode = data.Enum<Trigger.PositionModes>("direction");
    }

    public override void OnStay(Player player)
    {
      float num = Calc.ClampedMap(this.GetPositionLerp(player, this.PositionMode), 0.0f, 1f, this.From, this.To);
      Level scene = this.Scene as Level;
      scene.Session.Audio.Ambience.Param(this.Parameter, num);
      scene.Session.Audio.Apply();
    }
  }
}
