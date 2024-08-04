using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Monocle
{
    public class PixelFontSize
    {
        public List<MTexture> Textures;
        public Dictionary<int, PixelFontCharacter> Characters;
        public int LineHeight;
        public float Size;
        public bool Outline;
        private StringBuilder temp = new StringBuilder();

        public string AutoNewline(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            temp.Clear();
            string[] strArray = Regex.Split(text, "(\\s)");
            float num1 = 0.0f;
            foreach (string text1 in strArray)
            {
                float x = Measure(text1).X;
                if (x + (double) num1 > width)
                {
                    temp.Append('\n');
                    num1 = 0.0f;
                    if (text1.Equals(" "))
                        continue;
                }
                if (x > (double) width)
                {
                    int num2 = 1;
                    int startIndex = 0;
                    for (; num2 < text1.Length; ++num2)
                    {
                        if (num2 - startIndex > 1 && Measure(text1.Substring(startIndex, num2 - startIndex - 1)).X > (double) width)
                        {
                            temp.Append(text1.Substring(startIndex, num2 - startIndex - 1));
                            temp.Append('\n');
                            startIndex = num2 - 1;
                        }
                    }
                    string text2 = text1.Substring(startIndex, text1.Length - startIndex);
                    temp.Append(text2);
                    num1 += Measure(text2).X;
                }
                else
                {
                    num1 += x;
                    temp.Append(text1);
                }
            }
            return temp.ToString();
        }

        public PixelFontCharacter Get(int id)
        {
            PixelFontCharacter pixelFontCharacter = null;
            return Characters.TryGetValue(id, out pixelFontCharacter) ? pixelFontCharacter : null;
        }

        public Vector2 Measure(char text)
        {
            PixelFontCharacter pixelFontCharacter = null;
            return Characters.TryGetValue(text, out pixelFontCharacter) ? new Vector2(pixelFontCharacter.XAdvance, LineHeight) : Vector2.Zero;
        }

        public Vector2 Measure(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;
            Vector2 vector2 = new Vector2(0.0f, LineHeight);
            float num1 = 0.0f;
            for (int index = 0; index < text.Length; ++index)
            {
                if (text[index] == '\n')
                {
                    vector2.Y += LineHeight;
                    if (num1 > (double) vector2.X)
                        vector2.X = num1;
                    num1 = 0.0f;
                }
                else
                {
                    PixelFontCharacter pixelFontCharacter = null;
                    if (Characters.TryGetValue(text[index], out pixelFontCharacter))
                    {
                        num1 += pixelFontCharacter.XAdvance;
                        int num2;
                        if (index < text.Length - 1 && pixelFontCharacter.Kerning.TryGetValue(text[index + 1], out num2))
                            num1 += num2;
                    }
                }
            }
            if (num1 > (double) vector2.X)
                vector2.X = num1;
            return vector2;
        }

        public float WidthToNextLine(string text, int start)
        {
            if (string.IsNullOrEmpty(text))
                return 0.0f;
            float nextLine = 0.0f;
            int index = start;
            for (int length = text.Length; index < length && text[index] != '\n'; ++index)
            {
                PixelFontCharacter pixelFontCharacter = null;
                if (Characters.TryGetValue(text[index], out pixelFontCharacter))
                {
                    nextLine += pixelFontCharacter.XAdvance;
                    int num;
                    if (index < length - 1 && pixelFontCharacter.Kerning.TryGetValue(text[index + 1], out num))
                        nextLine += num;
                }
            }
            return nextLine;
        }

        public float HeightOf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0.0f;
            int num = 1;
            if (text.IndexOf('\n') >= 0)
            {
                for (int index = 0; index < text.Length; ++index)
                {
                    if (text[index] == '\n')
                        ++num;
                }
            }
            return num * LineHeight;
        }

        public void Draw(
            char character,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color)
        {
            if (char.IsWhiteSpace(character))
                return;
            PixelFontCharacter pixelFontCharacter = null;
            if (!Characters.TryGetValue(character, out pixelFontCharacter))
                return;
            Vector2 vector2_1 = Measure(character);
            Vector2 vector2_2 = new Vector2(vector2_1.X * justify.X, vector2_1.Y * justify.Y);
            Vector2 val = position + (new Vector2(pixelFontCharacter.XOffset, pixelFontCharacter.YOffset) - vector2_2) * scale;
            pixelFontCharacter.Texture.Draw(val.Floor(), Vector2.Zero, color, scale);
        }

        public void Draw(
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
            if (string.IsNullOrEmpty(text))
                return;
            Vector2 zero = Vector2.Zero;
            Vector2 vector2 = new Vector2((justify.X != 0.0 ? WidthToNextLine(text, 0) : 0.0f) * justify.X, HeightOf(text) * justify.Y);
            for (int index = 0; index < text.Length; ++index)
            {
                if (text[index] == '\n')
                {
                    zero.X = 0.0f;
                    zero.Y += LineHeight;
                    if (justify.X != 0.0)
                        vector2.X = WidthToNextLine(text, index + 1) * justify.X;
                }
                else
                {
                    PixelFontCharacter pixelFontCharacter = null;
                    if (Characters.TryGetValue(text[index], out pixelFontCharacter))
                    {
                        Vector2 position1 = position + (zero + new Vector2(pixelFontCharacter.XOffset, pixelFontCharacter.YOffset) - vector2) * scale;
                        if (stroke > 0.0 && !Outline)
                        {
                            if (edgeDepth > 0.0)
                            {
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(0.0f, -stroke), Vector2.Zero, strokeColor, scale);
                                for (float y = -stroke; y < edgeDepth + (double) stroke; y += stroke)
                                {
                                    pixelFontCharacter.Texture.Draw(position1 + new Vector2(-stroke, y), Vector2.Zero, strokeColor, scale);
                                    pixelFontCharacter.Texture.Draw(position1 + new Vector2(stroke, y), Vector2.Zero, strokeColor, scale);
                                }
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(-stroke, edgeDepth + stroke), Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(0.0f, edgeDepth + stroke), Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(stroke, edgeDepth + stroke), Vector2.Zero, strokeColor, scale);
                            }
                            else
                            {
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(-1f, -1f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(0.0f, -1f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(1f, -1f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(-1f, 0.0f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(1f, 0.0f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(-1f, 1f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(0.0f, 1f) * stroke, Vector2.Zero, strokeColor, scale);
                                pixelFontCharacter.Texture.Draw(position1 + new Vector2(1f, 1f) * stroke, Vector2.Zero, strokeColor, scale);
                            }
                        }
                        if (edgeDepth > 0.0)
                            pixelFontCharacter.Texture.Draw(position1 + Vector2.UnitY * edgeDepth, Vector2.Zero, edgeColor, scale);
                        pixelFontCharacter.Texture.Draw(position1, Vector2.Zero, color, scale);
                        zero.X += pixelFontCharacter.XAdvance;
                        int num;
                        if (index < text.Length - 1 && pixelFontCharacter.Kerning.TryGetValue(text[index + 1], out num))
                            zero.X += num;
                    }
                }
            }
        }

        public void Draw(string text, Vector2 position, Color color) => Draw(text, position, Vector2.Zero, Vector2.One, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);

        public void Draw(string text, Vector2 position, Vector2 justify, Vector2 scale, Color color) => Draw(text, position, justify, scale, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);

        public void DrawOutline(
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

        public void DrawEdgeOutline(
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color,
            float edgeDepth,
            Color edgeColor,
            float stroke = 0.0f,
            Color strokeColor = default (Color))
        {
            Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
        }
    }
}
