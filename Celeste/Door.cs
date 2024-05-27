using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
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
            Depth = 8998;
            string str = data.Attr("type", "wood");
            if (str == "wood")
            {
                Add(sprite = GFX.SpriteBank.Create("door"));
                openSfx = "event:/game/03_resort/door_wood_open";
                closeSfx = "event:/game/03_resort/door_wood_close";
            }
            else
            {
                Add(sprite = GFX.SpriteBank.Create(str + "door"));
                openSfx = "event:/game/03_resort/door_metal_open";
                closeSfx = "event:/game/03_resort/door_metal_close";
            }
            sprite.Play("idle");
            Collider = new Hitbox(12f, 22f, -6f, -23f);
            Add(occlude = new LightOcclude(new Rectangle(-1, -24, 2, 24)));
            Add(new PlayerCollider(HitPlayer));
        }

        public override bool IsRiding(Solid solid) => Scene.CollideCheck(new Rectangle((int) X - 2, (int) Y - 2, 4, 4), solid);

        protected override void OnSquish(CollisionData data)
        {
        }

        private void HitPlayer(Player player)
        {
            if (disabled)
                return;
            Open(player.X);
        }

        public void Open(float x)
        {
            if (sprite.CurrentAnimationID == "idle")
            {
                Audio.Play(openSfx, Position);
                sprite.Play("open");
                if (X == (double) x)
                    return;
                sprite.Scale.X = Math.Sign(x - X);
            }
            else
            {
                if (!(sprite.CurrentAnimationID == "close"))
                    return;
                sprite.Play("close", true);
            }
        }

        public override void Update()
        {
            string currentAnimationId = sprite.CurrentAnimationID;
            base.Update();
            occlude.Visible = sprite.CurrentAnimationID == "idle";
            if (!disabled && CollideCheck<Solid>())
                disabled = true;
            if (!(currentAnimationId == "close") || !(sprite.CurrentAnimationID == "idle"))
                return;
            Audio.Play(closeSfx, Position);
        }
    }
}
