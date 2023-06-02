using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Trapdoor : Entity
    {
        private Sprite sprite;
        private PlayerCollider playerCollider;
        private LightOcclude occluder;

        public Trapdoor(EntityData data, Vector2 offset)
        {
            this.Position = data.Position + offset;
            this.Depth = 8999;
            this.Add((Component) (this.sprite = GFX.SpriteBank.Create("trapdoor")));
            this.sprite.Play("idle");
            this.sprite.Y = 6f;
            this.Collider = (Collider) new Hitbox(24f, 4f, y: 6f);
            this.Add((Component) (this.playerCollider = new PlayerCollider(new Action<Player>(this.Open))));
            this.Add((Component) (this.occluder = new LightOcclude(new Rectangle(0, 6, 24, 2))));
        }

        private void Open(Player player)
        {
            this.Collidable = false;
            this.occluder.Visible = false;
            if ((double) player.Speed.Y >= 0.0)
            {
                Audio.Play("event:/game/03_resort/trapdoor_fromtop", this.Position);
                this.sprite.Play("open");
            }
            else
            {
                Audio.Play("event:/game/03_resort/trapdoor_frombottom", this.Position);
                this.Add((Component) new Coroutine(this.OpenFromBottom()));
            }
        }

        private IEnumerator OpenFromBottom()
        {
            this.sprite.Scale.Y = -1f;
            yield return (object) this.sprite.PlayRoutine("open_partial");
            yield return (object) 0.1f;
            this.sprite.Rate = -1f;
            yield return (object) this.sprite.PlayRoutine("open_partial", true);
            this.sprite.Scale.Y = 1f;
            this.sprite.Rate = 1f;
            this.sprite.Play("open", true);
        }
    }
}
