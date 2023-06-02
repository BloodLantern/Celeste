using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class Door : Actor
    {
        private Sprite sprite;
        private string openSfx;
        private string closeSfx;
        private LightOcclude occlude;
        private bool disabled;

        public Door(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Depth = 8998;
            string str = data.Attr("type", "wood");
            if (str == "wood")
            {
                this.Add((Component) (this.sprite = GFX.SpriteBank.Create("door")));
                this.openSfx = "event:/game/03_resort/door_wood_open";
                this.closeSfx = "event:/game/03_resort/door_wood_close";
            }
            else
            {
                this.Add((Component) (this.sprite = GFX.SpriteBank.Create(str + "door")));
                this.openSfx = "event:/game/03_resort/door_metal_open";
                this.closeSfx = "event:/game/03_resort/door_metal_close";
            }
            this.sprite.Play("idle");
            this.Collider = (Collider) new Hitbox(12f, 22f, -6f, -23f);
            this.Add((Component) (this.occlude = new LightOcclude(new Rectangle(-1, -24, 2, 24))));
            this.Add((Component) new PlayerCollider(new Action<Player>(this.HitPlayer)));
        }

        public override bool IsRiding(Solid solid) => this.Scene.CollideCheck(new Rectangle((int) this.X - 2, (int) this.Y - 2, 4, 4), (Entity) solid);

        protected override void OnSquish(CollisionData data)
        {
        }

        private void HitPlayer(Player player)
        {
            if (this.disabled)
                return;
            this.Open(player.X);
        }

        public void Open(float x)
        {
            if (this.sprite.CurrentAnimationID == "idle")
            {
                Audio.Play(this.openSfx, this.Position);
                this.sprite.Play("open");
                if ((double) this.X == (double) x)
                    return;
                this.sprite.Scale.X = (float) Math.Sign(x - this.X);
            }
            else
            {
                if (!(this.sprite.CurrentAnimationID == "close"))
                    return;
                this.sprite.Play("close", true);
            }
        }

        public override void Update()
        {
            string currentAnimationId = this.sprite.CurrentAnimationID;
            base.Update();
            this.occlude.Visible = this.sprite.CurrentAnimationID == "idle";
            if (!this.disabled && this.CollideCheck<Solid>())
                this.disabled = true;
            if (!(currentAnimationId == "close") || !(this.sprite.CurrentAnimationID == "idle"))
                return;
            Audio.Play(this.closeSfx, this.Position);
        }
    }
}
