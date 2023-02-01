// Decompiled with JetBrains decompiler
// Type: Celeste.BlackholeStrengthTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
  public class BlackholeStrengthTrigger : Trigger
  {
    private BlackholeBG.Strengths strength;

    public BlackholeStrengthTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.strength = data.Enum<BlackholeBG.Strengths>(nameof (strength));
    }

    public override void OnEnter(Player player)
    {
      base.OnEnter(player);
      (this.Scene as Level).Background.Get<BlackholeBG>()?.NextStrength(this.Scene as Level, this.strength);
    }
  }
}
