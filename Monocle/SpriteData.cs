﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
    public class SpriteData
    {
        public List<SpriteDataSource> Sources = new List<SpriteDataSource>();
        public Sprite Sprite;
        public Atlas Atlas;

        public SpriteData(Atlas atlas)
        {
            Sprite = new Sprite(atlas, "");
            Atlas = atlas;
        }

        public void Add(XmlElement xml, string overridePath = null)
        {
            SpriteDataSource spriteDataSource = new SpriteDataSource
            {
                XML = xml
            };
            spriteDataSource.Path = spriteDataSource.XML.Attr("path");
            spriteDataSource.OverridePath = overridePath;
            string prefix = "Sprite '" + spriteDataSource.XML.Name + "': ";
            if (!spriteDataSource.XML.HasAttr("path") && string.IsNullOrEmpty(overridePath))
                throw new Exception(prefix + "'path' is missing!");
            HashSet<string> ids = new HashSet<string>();
            foreach (XmlElement xml1 in spriteDataSource.XML.GetElementsByTagName("Anim"))
                CheckAnimXML(xml1, prefix, ids);
            foreach (XmlElement xml2 in spriteDataSource.XML.GetElementsByTagName("Loop"))
                CheckAnimXML(xml2, prefix, ids);
            if (spriteDataSource.XML.HasAttr("start") && !ids.Contains(spriteDataSource.XML.Attr("start")))
                throw new Exception(prefix + "starting animation '" + spriteDataSource.XML.Attr("start") + "' is missing!");
            string str1 = !spriteDataSource.XML.HasChild("Justify") || !spriteDataSource.XML.HasChild("Origin") ? spriteDataSource.XML.Attr("path", "") : throw new Exception(prefix + "has both Origin and Justify tags!");
            float defaultValue = spriteDataSource.XML.AttrFloat("delay", 0.0f);
            foreach (XmlElement xml3 in spriteDataSource.XML.GetElementsByTagName("Anim"))
            {
                Chooser<string> into = !xml3.HasAttr("goto") ? null : Chooser<string>.FromString<string>(xml3.Attr("goto"));
                string id = xml3.Attr("id");
                string str2 = xml3.Attr("path", "");
                int[] frames = Calc.ReadCSVIntWithTricks(xml3.Attr("frames", ""));
                string path = string.IsNullOrEmpty(overridePath) || !HasFrames(Atlas, overridePath + str2, frames) ? str1 + str2 : overridePath + str2;
                Sprite.Add(id, path, xml3.AttrFloat("delay", defaultValue), into, frames);
            }
            foreach (XmlElement xml4 in spriteDataSource.XML.GetElementsByTagName("Loop"))
            {
                string id = xml4.Attr("id");
                string str3 = xml4.Attr("path", "");
                int[] frames = Calc.ReadCSVIntWithTricks(xml4.Attr("frames", ""));
                string path = string.IsNullOrEmpty(overridePath) || !HasFrames(Atlas, overridePath + str3, frames) ? str1 + str3 : overridePath + str3;
                Sprite.AddLoop(id, path, xml4.AttrFloat("delay", defaultValue), frames);
            }
            if (spriteDataSource.XML.HasChild("Center"))
            {
                Sprite.CenterOrigin();
                Sprite.Justify = new Vector2(0.5f, 0.5f);
            }
            else if (spriteDataSource.XML.HasChild("Justify"))
            {
                Sprite.JustifyOrigin(spriteDataSource.XML.ChildPosition("Justify"));
                Sprite.Justify = spriteDataSource.XML.ChildPosition("Justify");
            }
            else if (spriteDataSource.XML.HasChild("Origin"))
                Sprite.Origin = spriteDataSource.XML.ChildPosition("Origin");
            if (spriteDataSource.XML.HasChild("Position"))
                Sprite.Position = spriteDataSource.XML.ChildPosition("Position");
            if (spriteDataSource.XML.HasAttr("start"))
                Sprite.Play(spriteDataSource.XML.Attr("start"));
            Sources.Add(spriteDataSource);
        }

        private bool HasFrames(Atlas atlas, string path, int[] frames = null)
        {
            if (frames == null || frames.Length == 0)
                return atlas.GetAtlasSubtexturesAt(path, 0) != null;
            for (int index = 0; index < frames.Length; ++index)
            {
                if (atlas.GetAtlasSubtexturesAt(path, frames[index]) == null)
                    return false;
            }
            return true;
        }

        private void CheckAnimXML(XmlElement xml, string prefix, HashSet<string> ids)
        {
            if (!xml.HasAttr("id"))
                throw new Exception(prefix + "'id' is missing on " + xml.Name + "!");
            if (ids.Contains(xml.Attr("id")))
                throw new Exception(prefix + "multiple animations with id '" + xml.Attr("id") + "'!");
            ids.Add(xml.Attr("id"));
        }

        public Sprite Create() => Sprite.CreateClone();

        public Sprite CreateOn(Sprite sprite) => Sprite.CloneInto(sprite);
    }
}
