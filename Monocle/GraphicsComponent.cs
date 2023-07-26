using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public abstract class GraphicsComponent : Component
    {
        public Vector2 Position;
        public Vector2 Origin;
        public Vector2 Scale = Vector2.One;
        public float Rotation;
        public Color Color = Color.White;
        public SpriteEffects Effects;

        public GraphicsComponent(bool active)
            : base(active, true)
        {
        }

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

        public bool FlipX
        {
            get => (Effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            set => Effects = value ? Effects | SpriteEffects.FlipHorizontally : Effects & ~SpriteEffects.FlipHorizontally;
        }

        public bool FlipY
        {
            get => (Effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            set => Effects = value ? Effects | SpriteEffects.FlipVertically : Effects & ~SpriteEffects.FlipVertically;
        }

        public Vector2 RenderPosition
        {
            get => (Entity == null ? Vector2.Zero : Entity.Position) + Position;
            set => Position = value - (Entity == null ? Vector2.Zero : Entity.Position);
        }

        public void DrawOutline(int offset = 1) => DrawOutline(Color.Black, offset);

        public void DrawOutline(Color color, int offset = 1)
        {
            Vector2 position = Position;
            Color color1 = Color;
            Color = color;
            for (int index1 = -1; index1 < 2; ++index1)
            {
                for (int index2 = -1; index2 < 2; ++index2)
                {
                    if (index1 != 0 || index2 != 0)
                    {
                        Position = position + new Vector2(index1 * offset, index2 * offset);
                        Render();
                    }
                }
            }
            Position = position;
            Color = color1;
        }

        public void DrawSimpleOutline()
        {
            Vector2 position = Position;
            Color color = Color;
            Color = Color.Black;
            Position = position + new Vector2(-1f, 0.0f);
            Render();
            Position = position + new Vector2(0.0f, -1f);
            Render();
            Position = position + new Vector2(1f, 0.0f);
            Render();
            Position = position + new Vector2(0.0f, 1f);
            Render();
            Position = position;
            Color = color;
        }
    }
}
