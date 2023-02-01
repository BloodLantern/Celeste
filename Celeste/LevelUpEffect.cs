// Decompiled with JetBrains decompiler
// Type: Celeste.LevelUpEffect
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  public class LevelUpEffect : Entity
  {
    private Sprite sprite;

    public LevelUpEffect(Vector2 position)
      : base(position)
    {
      this.Depth = -1000000;
      Audio.Play("event:/game/06_reflection/hug_levelup_text_in", this.Position);
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("player_level_up")));
      this.sprite.OnLastFrame = (Action<string>) (anim => this.RemoveSelf());
      this.sprite.OnFrameChange = (Action<string>) (anim =>
      {
        if (this.sprite.CurrentAnimationFrame != 20)
          return;
        Audio.Play("event:/game/06_reflection/hug_levelup_text_out");
      });
      this.sprite.Play("levelUp");
    }
  }
}
