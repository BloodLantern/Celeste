// Decompiled with JetBrains decompiler
// Type: Celeste.BridgeFixed
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BridgeFixed : Solid
    {
        public BridgeFixed(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, 8f, true)
        {
            MTexture texture = GFX.Game["scenery/bridge_fixed"];
            for (int x = 0; x < Width; x += texture.Width)
            {
                Rectangle rectangle = new(0, 0, texture.Width, texture.Height);
                if (x + rectangle.Width > Width)
                    rectangle.Width = (int)Width - x;

                Image image = new(texture)
                {
                    Position = new Vector2(x, -8f)
                };
                Add(image);
            }
        }
    }
}
