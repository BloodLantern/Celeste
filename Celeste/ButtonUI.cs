using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public static class ButtonUI
    {
        public static float Width(string label, VirtualButton button)
        {
            MTexture mtexture = Input.GuiButton(button);
            return ActiveFont.Measure(label).X + 8f + mtexture.Width;
        }

        public static void Render(
            Vector2 position,
            string label,
            VirtualButton button,
            float scale,
            float justifyX = 0.5f,
            float wiggle = 0.0f,
            float alpha = 1f)
        {
            MTexture mtexture = Input.GuiButton(button);
            float num = ButtonUI.Width(label, button);
            position.X -= (float) (scale * (double) num * (justifyX - 0.5));
            mtexture.Draw(position, new Vector2(mtexture.Width - num / 2f, mtexture.Height / 2f), Color.White * alpha, scale + wiggle);
            ButtonUI.DrawText(label, position, num / 2f, scale + wiggle, alpha);
        }

        private static void DrawText(
            string text,
            Vector2 position,
            float justify,
            float scale,
            float alpha)
        {
            float x = ActiveFont.Measure(text).X;
            ActiveFont.DrawOutline(text, position, new Vector2(justify / x, 0.5f), Vector2.One * scale, Color.White * alpha, 2f, Color.Black * alpha);
        }
    }
}
