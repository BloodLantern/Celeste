// Decompiled with JetBrains decompiler
// Type: Monocle.PixelFontCharacter
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
    public class PixelFontCharacter
    {
        public int Character;
        public MTexture Texture;
        public int XOffset;
        public int YOffset;
        public int XAdvance;
        public Dictionary<int, int> Kerning = new();

        public PixelFontCharacter(int character, MTexture texture, XmlElement xml)
        {
            Character = character;
            Texture = texture.GetSubtexture(xml.AttrInt("x"), xml.AttrInt("y"), xml.AttrInt("width"), xml.AttrInt("height"));
            XOffset = xml.AttrInt("xoffset");
            YOffset = xml.AttrInt("yoffset");
            XAdvance = xml.AttrInt("xadvance");
        }
    }
}
