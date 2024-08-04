using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class OutlineText : Text
    {
        public Color OutlineColor = Color.Black;
        public int OutlineOffset = 1;

        public OutlineText(
            SpriteFont font,
            string text,
            Vector2 position,
            Color color,
            HorizontalAlign horizontalAlign = HorizontalAlign.Center,
            VerticalAlign verticalAlign = VerticalAlign.Center)
            : base(font, text, position, color, horizontalAlign, verticalAlign)
        {
        }

        public OutlineText(
            SpriteFont font,
            string text,
            Vector2 position,
            HorizontalAlign horizontalAlign = HorizontalAlign.Center,
            VerticalAlign verticalAlign = VerticalAlign.Center)
            : this(font, text, position, Color.White, horizontalAlign, verticalAlign)
        {
        }

        public OutlineText(SpriteFont font, string text)
            : this(font, text, Vector2.Zero, Color.White)
        {
        }

        public override void Render()
        {
            for (int index1 = -1; index1 < 2; ++index1)
            {
                for (int index2 = -1; index2 < 2; ++index2)
                {
                    if (index1 != 0 || index2 != 0)
                        Draw.SpriteBatch.DrawString(Font, DrawText, RenderPosition + new Vector2(index1 * OutlineOffset, index2 * OutlineOffset), OutlineColor, Rotation, Origin, Scale, Effects, 0.0f);
                }
            }
            base.Render();
        }
    }
}
