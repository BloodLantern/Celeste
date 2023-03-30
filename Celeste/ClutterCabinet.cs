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
        private Sprite sprite;

        public bool Opened { get; private set; }

        public ClutterCabinet(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.sprite = GFX.SpriteBank.Create("clutterCabinet")));
            this.sprite.Position = new Vector2(8f);
            this.sprite.Play("idle");
            this.Depth = -10001;
        }

        public ClutterCabinet(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public void Open()
        {
            this.sprite.Play("open");
            this.Opened = true;
        }

        public void Close()
        {
            this.sprite.Play("close");
            this.Opened = false;
        }
    }
}
