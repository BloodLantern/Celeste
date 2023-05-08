// Decompiled with JetBrains decompiler
// Type: Celeste.CliffFlags
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class CliffFlags : Entity
    {
        private static readonly Color[] colors = new Color[4]
        {
            Calc.HexToColor("d85f2f"),
            Calc.HexToColor("d82f63"),
            Calc.HexToColor("2fd8a2"),
            Calc.HexToColor("d8d62f")
        };
        private static readonly Color lineColor = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);
        private static readonly Color pinColor = Color.Gray;

        public CliffFlags(Vector2 from, Vector2 to)
        {
            Depth = 8999;
            Position = from;
            Flagline flagline;
            Add(flagline = new Flagline(to, lineColor, pinColor, colors, 10, 10, 10, 10, 2, 8));
            flagline.ClothDroopAmount = 0.2f;
        }

        public CliffFlags(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Nodes[0] + offset)
        {
        }
    }
}
