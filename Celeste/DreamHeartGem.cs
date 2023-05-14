// Decompiled with JetBrains decompiler
// Type: Celeste.DreamHeartGem
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class DreamHeartGem : Entity
    {
        private readonly Sprite sprite;

        public DreamHeartGem(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("heartgem0"));
            sprite.Color = Color.White * 0.25f;
            sprite.Play("spin");
            Add(new BloomPoint(0.5f, 16f));
            Add(new VertexLight(Color.Aqua, 1f, 32, 64));
        }

        public override void Render()
        {
            for (int y = 0; y < (double)sprite.Height; ++y)
            {
                sprite.DrawSubrect(new Vector2((float)Math.Sin((Scene.TimeActive * 2.0) + (y * 0.40000000596046448)) * 2f, y), new Rectangle(0, y, (int)sprite.Width, 1));
            }
        }
    }
}
