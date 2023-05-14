// Decompiled with JetBrains decompiler
// Type: Celeste.Lamp
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Lamp : Entity
    {
        private readonly Monocle.Image sprite;

        public Lamp(Vector2 position, bool broken)
        {
            Position = position;
            Depth = 5;
            Add(sprite = new Monocle.Image(GFX.Game["scenery/lamp"].GetSubtexture(broken ? 16 : 0, 0, 16, 80)));
            sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
            if (broken)
            {
                return;
            }

            Add(new BloomPoint(new Vector2(0.0f, -66f), 1f, 16f));
        }
    }
}
