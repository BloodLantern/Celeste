// Decompiled with JetBrains decompiler
// Type: Monocle.PixelFont
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Monocle
{
    public class PixelFont
    {
        public string Face;
        public List<PixelFontSize> Sizes = new();
        private readonly List<VirtualTexture> managedTextures = new();

        public PixelFont(string face)
        {
            Face = face;
        }

        public PixelFontSize AddFontSize(string path, Atlas atlas = null, bool outline = false)
        {
            XmlElement data = Calc.LoadXML(path)["font"];
            return AddFontSize(path, data, atlas, outline);
        }

        public PixelFontSize AddFontSize(
            string path,
            XmlElement data,
            Atlas atlas = null,
            bool outline = false)
        {
            float num1 = data["info"].AttrFloat("size");
            foreach (PixelFontSize siz in Sizes)
            {
                if (siz.Size == (double)num1)
                {
                    return siz;
                }
            }
            List<MTexture> mtextureList = new();
            foreach (XmlElement xml in (XmlNode)data["pages"])
            {
                string str = xml.Attr("file");
                string withoutExtension = Path.GetFileNameWithoutExtension(str);
                if (atlas != null && atlas.Has(withoutExtension))
                {
                    mtextureList.Add(atlas[withoutExtension]);
                }
                else
                {
                    VirtualTexture texture = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path).Substring(Engine.ContentDirectory.Length + 1), str));
                    mtextureList.Add(new MTexture(texture));
                    managedTextures.Add(texture);
                }
            }
            PixelFontSize pixelFontSize = new()
            {
                Textures = mtextureList,
                Characters = new Dictionary<int, PixelFontCharacter>(),
                LineHeight = data["common"].AttrInt("lineHeight"),
                Size = num1,
                Outline = outline
            };
            foreach (XmlElement xml in (XmlNode)data["chars"])
            {
                int num2 = xml.AttrInt("id");
                int index = xml.AttrInt("page", 0);
                pixelFontSize.Characters.Add(num2, new PixelFontCharacter(num2, mtextureList[index], xml));
            }
            if (data["kernings"] != null)
            {
                foreach (XmlElement xml in (XmlNode)data["kernings"])
                {
                    int key1 = xml.AttrInt("first");
                    int key2 = xml.AttrInt("second");
                    int num3 = xml.AttrInt("amount");
                    if (pixelFontSize.Characters.TryGetValue(key1, out PixelFontCharacter pixelFontCharacter))
                    {
                        pixelFontCharacter.Kerning.Add(key2, num3);
                    }
                }
            }
            Sizes.Add(pixelFontSize);
            Sizes.Sort((a, b) => Math.Sign(a.Size - b.Size));
            return pixelFontSize;
        }

        public PixelFontSize Get(float size)
        {
            int index1 = 0;
            for (int index2 = Sizes.Count - 1; index1 < index2; ++index1)
            {
                if (Sizes[index1].Size >= (double)size)
                {
                    return Sizes[index1];
                }
            }
            return Sizes[Sizes.Count - 1];
        }

        public bool Has(float size)
        {
            int index1 = 0;
            for (int index2 = Sizes.Count - 1; index1 < index2; ++index1)
            {
                if (Sizes[index1].Size == (double)size)
                {
                    return true;
                }
            }
            return false;
        }

        public void Draw(
            float baseSize,
            char character,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color)
        {
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
            scale *= baseSize / pixelFontSize.Size;
            pixelFontSize.Draw(character, position, justify, scale, color);
        }

        public void Draw(
            float baseSize,
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
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
            scale *= baseSize / pixelFontSize.Size;
            pixelFontSize.Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
        }

        public void Draw(float baseSize, string text, Vector2 position, Color color)
        {
            Vector2 one = Vector2.One;
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(one.X, one.Y));
            _ = one * (baseSize / pixelFontSize.Size);
            pixelFontSize.Draw(text, position, Vector2.Zero, Vector2.One, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);
        }

        public void Draw(
            float baseSize,
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color)
        {
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
            scale *= baseSize / pixelFontSize.Size;
            pixelFontSize.Draw(text, position, justify, scale, color, 0.0f, Color.Transparent, 0.0f, Color.Transparent);
        }

        public void DrawOutline(
            float baseSize,
            string text,
            Vector2 position,
            Vector2 justify,
            Vector2 scale,
            Color color,
            float stroke,
            Color strokeColor)
        {
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
            scale *= baseSize / pixelFontSize.Size;
            pixelFontSize.Draw(text, position, justify, scale, color, 0.0f, Color.Transparent, stroke, strokeColor);
        }

        public void DrawEdgeOutline(
            float baseSize,
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
            PixelFontSize pixelFontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
            scale *= baseSize / pixelFontSize.Size;
            pixelFontSize.Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
        }

        public void Dispose()
        {
            foreach (VirtualAsset managedTexture in managedTextures)
            {
                managedTexture.Dispose();
            }

            Sizes.Clear();
        }
    }
}
