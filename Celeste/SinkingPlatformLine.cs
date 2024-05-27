using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class SinkingPlatformLine : Entity
    {
        private Color lineEdgeColor = Calc.HexToColor("2a1923");
        private Color lineInnerColor = Calc.HexToColor("160b12");
        private float height;

        public SinkingPlatformLine(Vector2 position)
        {
            Position = position;
            Depth = 9001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            height = SceneAs<Level>().Bounds.Height - (Y - SceneAs<Level>().Bounds.Y);
        }

        public override void Render()
        {
            Draw.Rect(X - 1f, Y, 3f, height, lineEdgeColor);
            Draw.Rect(X, Y + 1f, 1f, height, lineInnerColor);
        }
    }
}
