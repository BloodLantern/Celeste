using System;
using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
    public class SpriteBank
    {
        public Atlas Atlas;
        public XmlDocument XML;
        public Dictionary<string, SpriteData> SpriteData;

        public SpriteBank(Atlas atlas, XmlDocument xml)
        {
            Atlas = atlas;
            XML = xml;
            SpriteData = new Dictionary<string, SpriteData>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, XmlElement> dictionary = new();
            foreach (object childNode in XML["Sprites"].ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    XmlElement xml1 = childNode as XmlElement;
                    dictionary.Add(xml1.Name, xml1);
                    if (SpriteData.ContainsKey(xml1.Name))
                        throw new Exception("Duplicate sprite name in SpriteData: '" + xml1.Name + "'!");
                    SpriteData spriteData = SpriteData[xml1.Name] = new SpriteData(Atlas);
                    if (xml1.HasAttr("copy"))
                        spriteData.Add(dictionary[xml1.Attr("copy")], xml1.Attr("path"));
                    spriteData.Add(xml1);
                }
            }
        }

        public SpriteBank(Atlas atlas, string xmlPath)
            : this(atlas, Calc.LoadContentXML(xmlPath))
        {
        }

        public bool Has(string id) => SpriteData.ContainsKey(id);

        public Sprite Create(string id) => SpriteData.ContainsKey(id) ? SpriteData[id].Create() : throw new Exception("Missing animation name in SpriteData: '" + id + "'!");

        public Sprite CreateOn(Sprite sprite, string id)
        {
            if (SpriteData.ContainsKey(id))
                return SpriteData[id].CreateOn(sprite);
            throw new Exception("Missing animation name in SpriteData: '" + id + "'!");
        }
    }
}
