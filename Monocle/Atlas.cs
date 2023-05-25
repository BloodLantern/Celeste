// Decompiled with JetBrains decompiler
// Type: Monocle.Atlas
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Monocle
{
    public class Atlas
    {
        public List<VirtualTexture> Sources;
        private readonly Dictionary<string, MTexture> textures = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<MTexture>> orderedTexturesCache = new();
        private readonly Dictionary<string, string> links = new();

        public static Atlas FromAtlas(string path, AtlasDataFormat format)
        {
            Atlas atlas = new()
            {
                Sources = new List<VirtualTexture>()
            };
            ReadAtlasData(atlas, path, format);
            return atlas;
        }

        private static void ReadAtlasData(Atlas atlas, string path, AtlasDataFormat format)
        {
            switch (format)
            {
                case AtlasDataFormat.TexturePacker_Sparrow:
                    XmlElement xml1 = Calc.LoadContentXML(path)["TextureAtlas"];
                    string path2_1 = xml1.Attr("imagePath", "");
                    VirtualTexture texture1 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), path2_1));
                    MTexture parent1 = new(texture1);
                    atlas.Sources.Add(texture1);
                    IEnumerator enumerator1 = xml1.GetElementsByTagName("SubTexture").GetEnumerator();
                    try
                    {
                        while (enumerator1.MoveNext())
                        {
                            XmlElement current = (XmlElement) enumerator1.Current;
                            string str = current.Attr("name");
                            Rectangle clipRect = current.Rect();
                            atlas.textures[str] =
                                !current.HasAttr("frameX")
                                ? new MTexture(parent1, str, clipRect)
                                : new MTexture(parent1, str, clipRect,
                                    new Vector2(-current.AttrInt("frameX"), -current.AttrInt("frameY")),
                                        current.AttrInt("frameWidth"), current.AttrInt("frameHeight"));
                        }
                        break;
                    }
                    finally
                    {
                        if (enumerator1 is IDisposable disposable)
                            disposable.Dispose();
                    }
                case AtlasDataFormat.CrunchXml:
                    IEnumerator enumerator2 = Calc.LoadContentXML(path)[nameof(atlas)].GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            XmlElement current = (XmlElement) enumerator2.Current;
                            string str1 = current.Attr("n", "");
                            VirtualTexture texture2 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), str1 + ".png"));
                            MTexture parent2 = new(texture2);
                            atlas.Sources.Add(texture2);
                            foreach (XmlElement xml2 in (XmlNode) current)
                            {
                                string str2 = xml2.Attr("n");
                                Rectangle clipRect = new(xml2.AttrInt("x"), xml2.AttrInt("y"), xml2.AttrInt("w"), xml2.AttrInt("h"));
                                atlas.textures[str2] =
                                    !xml2.HasAttr("fx")
                                    ? new MTexture(parent2, str2, clipRect)
                                    : new MTexture(parent2, str2, clipRect,
                                        new Vector2(-xml2.AttrInt("fx"), -xml2.AttrInt("fy")),
                                            xml2.AttrInt("fw"), xml2.AttrInt("fh"));
                            }
                        }
                        break;
                    }
                    finally
                    {
                        if (enumerator2 is IDisposable disposable)
                            disposable.Dispose();
                    }
                case AtlasDataFormat.CrunchBinary:
                    using (FileStream input = File.OpenRead(Path.Combine(Engine.ContentDirectory, path)))
                    {
                        BinaryReader stream = new(input);
                        short num1 = stream.ReadInt16();
                        for (int index1 = 0; index1 < num1; ++index1)
                        {
                            string str3 = stream.ReadNullTerminatedString();
                            VirtualTexture texture3 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), str3 + ".png"));
                            atlas.Sources.Add(texture3);
                            MTexture parent3 = new(texture3);
                            short num2 = stream.ReadInt16();
                            for (int index2 = 0; index2 < num2; ++index2)
                            {
                                string str4 = stream.ReadNullTerminatedString();
                                short x = stream.ReadInt16();
                                short y = stream.ReadInt16();
                                short width1 = stream.ReadInt16();
                                short height1 = stream.ReadInt16();
                                short num3 = stream.ReadInt16();
                                short num4 = stream.ReadInt16();
                                short width2 = stream.ReadInt16();
                                short height2 = stream.ReadInt16();
                                atlas.textures[str4] = new MTexture(parent3, str4, new Rectangle(x, y, width1, height1), new Vector2(-num3, -num4), width2, height2);
                            }
                        }
                        break;
                    }
                case AtlasDataFormat.CrunchXmlOrBinary:
                    if (File.Exists(Path.Combine(Engine.ContentDirectory, path + ".bin")))
                    {
                        ReadAtlasData(atlas, path + ".bin", AtlasDataFormat.CrunchBinary);
                        break;
                    }
                    ReadAtlasData(atlas, path + ".xml", AtlasDataFormat.CrunchXml);
                    break;
                case AtlasDataFormat.CrunchBinaryNoAtlas:
                    using (FileStream input = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".bin")))
                    {
                        BinaryReader stream = new(input);
                        short num5 = stream.ReadInt16();
                        for (int index3 = 0; index3 < num5; ++index3)
                        {
                            string path2_2 = stream.ReadNullTerminatedString();
                            string path1 = Path.Combine(Path.GetDirectoryName(path), path2_2);
                            short num6 = stream.ReadInt16();
                            for (int index4 = 0; index4 < num6; ++index4)
                            {
                                string key = stream.ReadNullTerminatedString();
                                /*_ = (int)stream.ReadInt16();
                                _ = (int)stream.ReadInt16();
                                _ = (int)stream.ReadInt16();
                                _ = (int)stream.ReadInt16();*/
                                _ = stream.ReadBytes(8);
                                short num11 = stream.ReadInt16();
                                short num12 = stream.ReadInt16();
                                short frameWidth = stream.ReadInt16();
                                short frameHeight = stream.ReadInt16();
                                VirtualTexture texture4 = VirtualContent.CreateTexture(Path.Combine(path1, key + ".png"));
                                atlas.Sources.Add(texture4);
                                atlas.textures[key] = new MTexture(texture4, new Vector2(-num11, -num12), frameWidth, frameHeight);
                            }
                        }
                        break;
                    }
                case AtlasDataFormat.Packer:
                    using (FileStream input = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".meta")))
                    {
                        BinaryReader binaryReader = new(input);
                        _ = binaryReader.ReadInt32();
                        _ = binaryReader.ReadString();
                        _ = binaryReader.ReadInt32();
                        short num13 = binaryReader.ReadInt16();
                        for (int index5 = 0; index5 < num13; ++index5)
                        {
                            string str5 = binaryReader.ReadString();
                            VirtualTexture texture5 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), str5 + ".data"));
                            atlas.Sources.Add(texture5);
                            MTexture parent4 = new(texture5);
                            short num14 = binaryReader.ReadInt16();
                            for (int index6 = 0; index6 < num14; ++index6)
                            {
                                string str6 = binaryReader.ReadString().Replace('\\', '/');
                                short x = binaryReader.ReadInt16();
                                short y = binaryReader.ReadInt16();
                                short width3 = binaryReader.ReadInt16();
                                short height3 = binaryReader.ReadInt16();
                                short num15 = binaryReader.ReadInt16();
                                short num16 = binaryReader.ReadInt16();
                                short width4 = binaryReader.ReadInt16();
                                short height4 = binaryReader.ReadInt16();
                                atlas.textures[str6] = new MTexture(parent4, str6, new Rectangle(x, y, width3, height3), new Vector2(-num15, -num16), width4, height4);
                            }
                        }
                        if (input.Position >= input.Length || !(binaryReader.ReadString() == "LINKS"))
                            break;

                        short num17 = binaryReader.ReadInt16();
                        for (int index = 0; index < num17; ++index)
                        {
                            string key = binaryReader.ReadString();
                            string str = binaryReader.ReadString();
                            atlas.links.Add(key, str);
                        }
                        break;
                    }
                case AtlasDataFormat.PackerNoAtlas:
                    using (FileStream input = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".meta")))
                    {
                        BinaryReader binaryReader = new(input);
                        _ = binaryReader.ReadInt32();
                        _ = binaryReader.ReadString();
                        _ = binaryReader.ReadInt32();
                        short num18 = binaryReader.ReadInt16();
                        for (int index7 = 0; index7 < num18; ++index7)
                        {
                            string path2_3 = binaryReader.ReadString();
                            string path1 = Path.Combine(Path.GetDirectoryName(path), path2_3);
                            short num19 = binaryReader.ReadInt16();
                            for (int index8 = 0; index8 < num19; ++index8)
                            {
                                string key = binaryReader.ReadString().Replace('\\', '/');
                                _ = (int)binaryReader.ReadInt16();
                                _ = (int)binaryReader.ReadInt16();
                                _ = (int)binaryReader.ReadInt16();
                                _ = (int)binaryReader.ReadInt16();
                                short num24 = binaryReader.ReadInt16();
                                short num25 = binaryReader.ReadInt16();
                                short frameWidth = binaryReader.ReadInt16();
                                short frameHeight = binaryReader.ReadInt16();
                                VirtualTexture texture6 = VirtualContent.CreateTexture(Path.Combine(path1, key + ".data"));
                                atlas.Sources.Add(texture6);
                                atlas.textures[key] = new MTexture(texture6, new Vector2(-num24, -num25), frameWidth, frameHeight)
                                {
                                    AtlasPath = key
                                };
                            }
                        }
                        if (input.Position >= input.Length || !(binaryReader.ReadString() == "LINKS"))
                            break;

                        short num26 = binaryReader.ReadInt16();
                        for (int index = 0; index < num26; ++index)
                        {
                            string key = binaryReader.ReadString();
                            string str = binaryReader.ReadString();
                            atlas.links.Add(key, str);
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static Atlas FromMultiAtlas(
            string rootPath,
            string[] dataPath,
            AtlasDataFormat format)
        {
            Atlas atlas = new()
            {
                Sources = new List<VirtualTexture>()
            };
            for (int index = 0; index < dataPath.Length; ++index)
                ReadAtlasData(atlas, Path.Combine(rootPath, dataPath[index]), format);

            return atlas;
        }

        public static Atlas FromMultiAtlas(
            string rootPath,
            string filename,
            AtlasDataFormat format)
        {
            Atlas atlas = new()
            {
                Sources = new List<VirtualTexture>()
            };
            int num = 0;
            while (true)
            {
                string str = Path.Combine(rootPath, filename + num.ToString() + ".xml");
                if (File.Exists(Path.Combine(Engine.ContentDirectory, str)))
                {
                    ReadAtlasData(atlas, str, format);
                    ++num;
                }
                else
                    break;
            }
            return atlas;
        }

        public static Atlas FromDirectory(string path)
        {
            Atlas atlas = new()
            {
                Sources = new List<VirtualTexture>()
            };
            string contentDirectory = Engine.ContentDirectory;
            int contentPathLength = contentDirectory.Length;
            string fullPath = Path.Combine(contentDirectory, path);
            int fullPathLength = fullPath.Length;
            foreach (string file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(file);
                if (extension is ".png" or ".xnb")
                {
                    VirtualTexture texture = VirtualContent.CreateTexture(file.Substring(contentPathLength + 1));
                    atlas.Sources.Add(texture);
                    string str = file.Substring(fullPathLength + 1);
                    string key = str.Substring(0, str.Length - 4).Replace('\\', '/');
                    atlas.textures.Add(key, new MTexture(texture));
                }
             }
            return atlas;
        }

        public MTexture this[string id]
        {
            get => textures[id];
            set => textures[id] = value;
        }

        public bool Has(string id)
        {
            return textures.ContainsKey(id);
        }

        public MTexture GetOrDefault(string id, MTexture defaultTexture)
        {
            return string.IsNullOrEmpty(id) || !Has(id) ? defaultTexture : textures[id];
        }

        public List<MTexture> GetAtlasSubtextures(string key)
        {
            if (!orderedTexturesCache.TryGetValue(key, out List<MTexture> atlasSubtextures))
            {
                atlasSubtextures = new List<MTexture>();
                int index = 0;
                while (true)
                {
                    MTexture subtextureFromAtlasAt = GetAtlasSubtextureFromAtlasAt(key, index);
                    if (subtextureFromAtlasAt != null)
                    {
                        atlasSubtextures.Add(subtextureFromAtlasAt);
                        ++index;
                    }
                    else
                        break;
                }
                orderedTexturesCache.Add(key, atlasSubtextures);
            }
            return atlasSubtextures;
        }

        private MTexture GetAtlasSubtextureFromCacheAt(string key, int index)
        {
            return orderedTexturesCache[key][index];
        }

        private MTexture GetAtlasSubtextureFromAtlasAt(string key, int index)
        {
            if (index == 0 && textures.ContainsKey(key))
                return textures[key];

            string str = index.ToString();
            for (int length = str.Length; str.Length < length + 6; str = "0" + str)
                if (textures.TryGetValue(key + str, out MTexture subtextureFromAtlasAt))
                    return subtextureFromAtlasAt;
            return null;
        }

        public MTexture GetAtlasSubtexturesAt(string key, int index)
        {
            return orderedTexturesCache.TryGetValue(key, out List<MTexture> mtextureList) ? mtextureList[index] : GetAtlasSubtextureFromAtlasAt(key, index);
        }

        public MTexture GetLinkedTexture(string key)
        {
            return key != null && links.TryGetValue(key, out string key1) && textures.TryGetValue(key1, out MTexture mtexture) ? mtexture : null;
        }

        public void Dispose()
        {
            foreach (VirtualAsset source in Sources)
                source.Dispose();

            Sources.Clear();
            textures.Clear();
        }

        public enum AtlasDataFormat
        {
            TexturePacker_Sparrow,
            CrunchXml,
            CrunchBinary,
            CrunchXmlOrBinary,
            CrunchBinaryNoAtlas,
            Packer,
            PackerNoAtlas,
        }
    }
}
