// Decompiled with JetBrains decompiler
// Type: Celeste.BladeTrackSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
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
            Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
            Sprite.Play("idle");
            Depth = -50;
            Add(new MirrorReflection());
        }

        public override void Update()
        {
            base.Update();
            if (!trail || !Scene.OnInterval(0.04f))
                return;

            SceneAs<Level>().ParticlesBG.Emit(P_Trail, 2, Position, Vector2.One * 3f);
        }

        public override void OnTrackStart()
        {
            Sprite.Play("spin");
            if (hasStarted)
                _ = Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);

            hasStarted = true;
            trail = true;
        }

        public override void OnTrackEnd()
        {
            trail = false;
        }
    }
}
