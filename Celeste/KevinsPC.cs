﻿// Decompiled with JetBrains decompiler
// Type: Celeste.KevinsPC
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class KevinsPC : Actor
    {
        private readonly Monocle.Image image;
        private readonly MTexture spectogram;
        private MTexture subtex;
        private readonly SoundSource sfx;
        private float timer;

        public KevinsPC(Vector2 position)
            : base(position)
        {
            Add(image = new Monocle.Image(GFX.Game["objects/kevinspc/pc"]));
            _ = image.JustifyOrigin(0.5f, 1f);
            Depth = 8999;
            spectogram = GFX.Game["objects/kevinspc/spectogram"];
            subtex = spectogram.GetSubtexture(0, 0, 32, 18, subtex);
            Add(sfx = new SoundSource("event:/new_content/env/local/kevinpc"));
            sfx.Position = new Vector2(0.0f, -16f);
            timer = 0.0f;
        }

        public KevinsPC(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override bool IsRiding(Solid solid)
        {
            return Scene.CollideCheck(new Rectangle((int)X - 4, (int)Y, 8, 2), solid);
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            int num = spectogram.Width - 32;
            subtex = spectogram.GetSubtexture((int)(timer * (num / 22.0) % num), 0, 32, 18, subtex);
        }

        public override void Render()
        {
            base.Render();
            if (subtex == null)
            {
                return;
            }

            subtex.Draw(Position + new Vector2(-16f, -39f));
            Draw.Rect(X - 16f, Y - 39f, 32f, 18f, Color.Black * 0.25f);
        }
    }
}
