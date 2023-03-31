// Decompiled with JetBrains decompiler
// Type: Celeste.ActiveFont
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public static class ActiveFont
    {
        public static PixelFont Font => Fonts.Get(Dialog.Language.FontFace);

        public static PixelFontSize FontSize => Font.Get(BaseSize);

        public static float BaseSize => Dialog.Language.FontFaceSize;

        public static float LineHeight => Font.Get(BaseSize).LineHeight;

        public static Vector2 Measure(char text)
        {
            return Font.Get(BaseSize).Measure(text);
        }

        public static Vector2 Measure(string text)
        {
            return Font.Get(BaseSize).Measure(text);
        }

        public static float WidthToNextLine(string text, int start)
        {
            return Font.Get(BaseSize).WidthToNextLine(text, start);
        }

        public static float HeightOf(string text)
        {
            return Font.Get(BaseSize).HeightOf(text);
        }

        public static void Draw(
            char character,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color)
        {
            Font.Draw(BaseSize, character, position, justify, scale, color);
        }

        private static void Draw(
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color,
            float edgeDepth,
            Color edgeColor,
            float stroke,
            Color strokeColor)
        {
            Font.Draw(BaseSize, text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
        }

        public static void Draw(string text, Vector2 position, Color color)
        {
            Draw(text, position, Vector2.Zero, Vector2.One, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);
        }

        public static void Draw(
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color)
        {
            Draw(text, position, justify, scale, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);
        }

        public static void DrawOutline(
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color,
            float stroke,
            Color strokeColor)
        {
            Draw(text, position, justify, scale, color, 0.0f, Color.Transparent, stroke, strokeColor);
        }

        public static void DrawEdgeOutline(
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color,
            float edgeDepth,
            Color edgeColor,
            float stroke = 0.0f,
            Color strokeColor = default)
        {
            Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
        }
    }
}
