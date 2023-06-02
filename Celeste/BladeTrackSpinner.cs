﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BladeTrackSpinner : TrackSpinner
    {
        public static ParticleType P_Trail;
        public Sprite Sprite;
        private bool hasStarted;
        private bool trail;

        public BladeTrackSpinner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("templeBlade")));
            this.Sprite.Play("idle");
            this.Depth = -50;
            this.Add((Component) new MirrorReflection());
        }

        public override void Update()
        {
            base.Update();
            if (!this.trail || !this.Scene.OnInterval(0.04f))
                return;
            this.SceneAs<Level>().ParticlesBG.Emit(BladeTrackSpinner.P_Trail, 2, this.Position, Vector2.One * 3f);
        }

        public override void OnTrackStart()
        {
            this.Sprite.Play("spin");
            if (this.hasStarted)
                Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", this.Position);
            this.hasStarted = true;
            this.trail = true;
        }

        public override void OnTrackEnd() => this.trail = false;
    }
}
