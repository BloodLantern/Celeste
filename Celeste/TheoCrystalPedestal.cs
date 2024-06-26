﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TheoCrystalPedestal : Solid
    {
        public Image sprite;
        public bool DroppedTheo;

        public TheoCrystalPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, false)
        {
            Add(sprite = new Image(GFX.Game["characters/theoCrystal/pedestal"]));
            EnableAssistModeChecks = false;
            sprite.JustifyOrigin(0.5f, 1f);
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = false;
            OnDashCollide = (player, direction) =>
            {
                TheoCrystal entity = Scene.Tracker.GetEntity<TheoCrystal>();
                entity.OnPedestal = false;
                entity.Speed = new Vector2(0.0f, -300f);
                DroppedTheo = true;
                Collidable = false;
                (Scene as Level).Flash(Color.White);
                Celeste.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_break_free", entity.Position);
                return DashCollisionResults.Rebound;
            };
            Tag = (int) Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("foundTheoInCrystal"))
            {
                DroppedTheo = true;
            }
            else
            {
                TheoCrystal first = Scene.Entities.FindFirst<TheoCrystal>();
                if (first == null)
                    return;
                first.Depth = Depth + 1;
            }
        }

        public override void Update()
        {
            TheoCrystal entity = Scene.Tracker.GetEntity<TheoCrystal>();
            if (entity != null && !DroppedTheo)
            {
                entity.Position = Position + new Vector2(0.0f, -32f);
                entity.OnPedestal = true;
            }
            base.Update();
        }
    }
}
