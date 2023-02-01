// Decompiled with JetBrains decompiler
// Type: Celeste.DreamHeartGem
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  public class DreamHeartGem : Entity
  {
    private Sprite sprite;

    public DreamHeartGem(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("heartgem0")));
      this.sprite.Color = Color.White * 0.25f;
      this.sprite.Play("spin");
      this.Add((Component) new BloomPoint(0.5f, 16f));
      this.Add((Component) new VertexLight(Color.Aqua, 1f, 32, 64));
    }

    public override void Render()
    {
      for (int y = 0; (double) y < (double) this.sprite.Height; ++y)
        this.sprite.DrawSubrect(new Vector2((float) Math.Sin((double) this.Scene.TimeActive * 2.0 + (double) y * 0.40000000596046448) * 2f, (float) y), new Rectangle(0, y, (int) this.sprite.Width, 1));
    }
  }
}
