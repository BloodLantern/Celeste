using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
    public class BloomPoint : Component
    {
        public Vector2 Position = Vector2.Zero;
        public float Alpha = 1f;
        public float Radius = 8f;

        public float X
        {
            get => Position.X;
            set => Position.X = value;
        }

        public float Y
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        public BloomPoint(float alpha, float radius)
            : base(false, true)
        {
            Alpha = alpha;
            Radius = radius;
        }

        public BloomPoint(Vector2 position, float alpha, float radius)
            : base(false, true)
        {
            Position = position;
            Alpha = alpha;
            Radius = radius;
        }
    }
}
