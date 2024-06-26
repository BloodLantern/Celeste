﻿using Microsoft.Xna.Framework;

namespace Celeste
{
    public class NPC04_Theo : NPC
    {
        private bool started;

        public NPC04_Theo(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            IdleAnim = "idle";
            MoveAnim = "walk";
            Visible = false;
            Maxspeed = 48f;
            SetupTheoSpriteSounds();
        }

        public override void Update()
        {
            base.Update();
            if (started)
                return;
            Gondola first = Scene.Entities.FindFirst<Gondola>();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (first == null || entity == null || entity.X <= first.Left - 16.0)
                return;
            started = true;
            Scene.Add(new CS04_Gondola(this, first, entity));
        }
    }
}
