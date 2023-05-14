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
            Collider = new Hitbox(data.Width, 32f);
            Collidable = false;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        private void OnPlayer(Player player)
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                _ = player.Play("event:/game/general/assist_screenbottom");
                player.Bounce(Top);
            }
            else
            {
                _ = player.Die(Vector2.Zero);
            }
        }

        public override void Update()
        {
            if (!Collidable)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && (double)entity.Bottom < (double)Top - 32.0)
                {
                    Collidable = true;
                }
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && (double)entity.Top > (double)Bottom + 32.0)
                {
                    Collidable = false;
                }
            }
            base.Update();
        }
    }
}
