﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MrOshiroDoor : Solid
    {
        private Sprite sprite;
        private Wiggler wiggler;

        public MrOshiroDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, false)
        {
            Add(sprite = GFX.SpriteBank.Create("ghost_door"));
            sprite.Position = new Vector2(Width, Height) / 2f;
            sprite.Play("idle");
            OnDashCollide = OnDashed;
            Add(wiggler = Wiggler.Create(0.6f, 3f, f => sprite.Scale = Vector2.One * (float) (1.0 - f * 0.20000000298023224)));
            SurfaceSoundIndex = 20;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Visible = Collidable = !SceneAs<Level>().Session.GetFlag("oshiro_resort_talked_1");
        }

        public void Open()
        {
            if (!Collidable)
                return;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Audio.Play("event:/game/03_resort/forcefield_vanish", Position);
            sprite.Play("open");
            Collidable = false;
        }

        public void InstantOpen() => Collidable = Visible = false;

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Audio.Play("event:/game/03_resort/forcefield_bump", Position);
            wiggler.Start();
            return DashCollisionResults.Bounce;
        }
    }
}
