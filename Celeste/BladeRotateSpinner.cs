﻿// Decompiled with JetBrains decompiler
// Type: Celeste.BladeRotateSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BladeRotateSpinner : RotateSpinner
    {
        public Sprite Sprite;

        public BladeRotateSpinner(EntityData data, Vector2 offset)
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
            if (Scene.OnInterval(0.04f))
                SceneAs<Level>().ParticlesBG.Emit(BladeTrackSpinner.P_Trail, 2, Position, Vector2.One * 3f);

            if (!Scene.OnInterval(1f))
                return;

            Sprite.Play("spin");
        }
    }
}
