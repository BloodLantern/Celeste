// Decompiled with JetBrains decompiler
// Type: Celeste.PropLight
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class PropLight : Entity
    {
        public PropLight(Vector2 position, Color color, float alpha)
            : base(position)
        {
            this.Add((Component) new VertexLight(color, alpha, 128, 256));
        }

        public PropLight(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.HexColor("color"), data.Float("alpha"))
        {
        }
    }
}
