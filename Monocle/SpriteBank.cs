// Decompiled with JetBrains decompiler
// Type: Monocle.SpriteBank
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
    public class SpriteBank
    {
        public Atlas Atlas;
        public XmlDocument XML;
        public Dictionary<string, Monocle.SpriteData> SpriteData;

        public SpriteBank(Atlas atlas, XmlDocument xml)
        {
            Atlas = atlas;
            XML = xml;
            SpriteData = new Dictionary<string, Monocle.SpriteData>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, XmlElement> dictionary = new();
            foreach (object childNode in XML["Sprites"].ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    XmlElement xml1 = childNode as XmlElement;
                    dictionary.Add(xml1.Name, xml1);
                    if (SpriteData.ContainsKey(xml1.Name))
                    {
                        throw new Exception("Duplicate sprite name in SpriteData: '" + xml1.Name + "'!");
                    }

                    Monocle.SpriteData spriteData = SpriteData[xml1.Name] = new Monocle.SpriteData(Atlas);
                    if (xml1.HasAttr("copy"))
                    {
                        spriteData.Add(dictionary[xml1.Attr("copy")], xml1.Attr("path"));
                    }

                    spriteData.Add(xml1);
                }
            }
        }

        public SpriteBank(Atlas atlas, string xmlPath)
            : this(atlas, Calc.LoadContentXML(xmlPath))
        {
        }

        public bool Has(string id)
        {
            return SpriteData.ContainsKey(id);
        }

        public Sprite Create(string id)
        {
            return SpriteData.ContainsKey(id) ? SpriteData[id].Create() : throw new Exception("Missing animation name in SpriteData: '" + id + "'!");
        }

        public Sprite CreateOn(Sprite sprite, string id)
        {
            return SpriteData.ContainsKey(id)
                ? SpriteData[id].CreateOn(sprite)
                : throw new Exception("Missing animation name in SpriteData: '" + id + "'!");
        }
    }
}
