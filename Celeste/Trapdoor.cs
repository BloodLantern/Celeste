// Decompiled with JetBrains decompiler
// Type: Celeste.Trapdoor
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Trapdoor : Entity
    {
        private readonly Sprite sprite;
        private readonly PlayerCollider playerCollider;
        private readonly LightOcclude occluder;

        public Trapdoor(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            Depth = 8999;
            Add(sprite = GFX.SpriteBank.Create("trapdoor"));
            sprite.Play("idle");
            sprite.Y = 6f;
            Collider = new Hitbox(24f, 4f, y: 6f);
            Add(playerCollider = new PlayerCollider(new Action<Player>(Open)));
            Add(occluder = new LightOcclude(new Rectangle(0, 6, 24, 2)));
        }

        private void Open(Player player)
        {
            Collidable = false;
            occluder.Visible = false;
            if (player.Speed.Y >= 0.0)
            {
                _ = Audio.Play("event:/game/03_resort/trapdoor_fromtop", Position);
                sprite.Play("open");
            }
            else
            {
                _ = Audio.Play("event:/game/03_resort/trapdoor_frombottom", Position);
                Add(new Coroutine(OpenFromBottom()));
            }
        }

        private IEnumerator OpenFromBottom()
        {
            sprite.Scale.Y = -1f;
            yield return sprite.PlayRoutine("open_partial");
            yield return 0.1f;
            sprite.Rate = -1f;
            yield return sprite.PlayRoutine("open_partial", true);
            sprite.Scale.Y = 1f;
            sprite.Rate = 1f;
            sprite.Play("open", true);
        }
    }
}
