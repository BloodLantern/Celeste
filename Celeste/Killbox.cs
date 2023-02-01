// Decompiled with JetBrains decompiler
// Type: Celeste.Killbox
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  [Tracked(false)]
  public class Killbox : Entity
  {
    public Killbox(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Collider = (Collider) new Hitbox((float) data.Width, 32f);
      this.Collidable = false;
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
    }

    private void OnPlayer(Player player)
    {
      if (SaveData.Instance.Assists.Invincible)
      {
        player.Play("event:/game/general/assist_screenbottom");
        player.Bounce(this.Top);
      }
      else
        player.Die(Vector2.Zero);
    }

    public override void Update()
    {
      if (!this.Collidable)
      {
        Player entity = this.Scene.Tracker.GetEntity<Player>();
        if (entity != null && (double) entity.Bottom < (double) this.Top - 32.0)
          this.Collidable = true;
      }
      else
      {
        Player entity = this.Scene.Tracker.GetEntity<Player>();
        if (entity != null && (double) entity.Top > (double) this.Bottom + 32.0)
          this.Collidable = false;
      }
      base.Update();
    }
  }
}
