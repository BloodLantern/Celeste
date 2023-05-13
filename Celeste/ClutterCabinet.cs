// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterCabinet
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class ClutterCabinet : Entity
    {
        private readonly Sprite sprite;

        public bool Opened { get; private set; }

        public ClutterCabinet(Vector2 position)
            : base(position)
        {
            Add(sprite = GFX.SpriteBank.Create("clutterCabinet"));
            sprite.Position = new Vector2(8f);
            sprite.Play("idle");
            Depth = -10001;
        }

        public ClutterCabinet(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public void Open()
        {
            sprite.Play("open");
            Opened = true;
        }

        public void Close()
        {
            sprite.Play("close");
            Opened = false;
        }
    }
}
