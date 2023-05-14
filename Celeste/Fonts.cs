// Decompiled with JetBrains decompiler
// Type: Celeste.Fonts
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Celeste
{
    public static class Fonts
    {
        private static readonly Dictionary<string, List<string>> paths = new();
        private static readonly Dictionary<string, PixelFont> loadedFonts = new();

        public static PixelFont Load(string face)
        {
            if (!Fonts.loadedFonts.TryGetValue(face, out PixelFont pixelFont) && Fonts.paths.TryGetValue(face, out List<string> stringList))
            {
                Fonts.loadedFonts.Add(face, pixelFont = new PixelFont(face));
                foreach (string path in stringList)
                {
                    _ = pixelFont.AddFontSize(path, GFX.Gui);
                }
            }
            return pixelFont;
        }

        public static PixelFont Get(string face)
        {
            return Fonts.loadedFonts.TryGetValue(face, out PixelFont pixelFont) ? pixelFont : null;
        }

        public static void Unload(string face)
        {
            if (!Fonts.loadedFonts.TryGetValue(face, out PixelFont pixelFont))
            {
                return;
            }

            pixelFont.Dispose();
            _ = Fonts.loadedFonts.Remove(face);
        }

        public static void Reload()
        {
            List<string> stringList = new();
            foreach (string key in Fonts.loadedFonts.Keys)
            {
                stringList.Add(key);
            }

            foreach (string str in stringList)
            {
                Fonts.loadedFonts[str].Dispose();
                _ = Fonts.Load(str);
            }
        }

        public static void Prepare()
        {
            XmlReaderSettings settings = new()
            {
                CloseInput = true
            };
            foreach (string file in Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Dialog"), "*.fnt", SearchOption.AllDirectories))
            {
                string key = null;
                using (XmlReader xmlReader = XmlReader.Create(File.OpenRead(file), settings))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "info")
                        {
                            key = xmlReader.GetAttribute("face");
                        }
                    }
                }
                if (key != null)
                {
                    if (!Fonts.paths.TryGetValue(key, out List<string> stringList))
                    {
                        Fonts.paths.Add(key, stringList = new List<string>());
                    }

                    stringList.Add(file);
                }
            }
        }

        public static void Log()
        {
            Engine.Commands.Log("EXISTING FONTS:");
            foreach (KeyValuePair<string, List<string>> path in Fonts.paths)
            {
                Engine.Commands.Log(" - " + path.Key);
                foreach (string str in path.Value)
                {
                    Engine.Commands.Log(" - > " + str);
                }
            }
            Engine.Commands.Log("LOADED:");
            foreach (KeyValuePair<string, PixelFont> loadedFont in Fonts.loadedFonts)
            {
                Engine.Commands.Log(" - " + loadedFont.Key);
            }
        }
    }
}
