// Decompiled with JetBrains decompiler
// Type: Celeste.MovingPlatformLine
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MovingPlatformLine : Entity
    {
        private Color lineEdgeColor;
        private Color lineInnerColor;
        private Vector2 end;

        public MovingPlatformLine(Vector2 position, Vector2 end)
        {
            this.Position = position;
            this.Depth = 9001;
            this.end = end;
        }

        public override void Added(Scene scene)
        {
            if ((scene as Level).Session.Area.ID == 4)
            {
                this.lineEdgeColor = Calc.HexToColor("a4464a");
                this.lineInnerColor = Calc.HexToColor("86354e");
            }
            else
            {
                this.lineEdgeColor = Calc.HexToColor("2a1923");
                this.lineInnerColor = Calc.HexToColor("160b12");
            }
            base.Added(scene);
        }

        public override void Render()
        {
            Vector2 vector2_1 = (this.end - this.Position).SafeNormalize();
            Vector2 vector2_2 = new Vector2(-vector2_1.Y, vector2_1.X);
            Draw.Line(this.Position - vector2_1 - vector2_2, this.end + vector2_1 - vector2_2, this.lineEdgeColor);
            Draw.Line(this.Position - vector2_1, this.end + vector2_1, this.lineEdgeColor);
            Draw.Line(this.Position - vector2_1 + vector2_2, this.end + vector2_1 + vector2_2, this.lineEdgeColor);
            Draw.Line(this.Position, this.end, this.lineInnerColor);
        }
    }
}
