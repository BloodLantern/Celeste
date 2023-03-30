// Decompiled with JetBrains decompiler
// Type: Celeste.BinaryPacker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Celeste
{
    public static class BinaryPacker
    {
        public static readonly HashSet<string> IgnoreAttributes = new HashSet<string>() {
            "_eid"
        };
        public static string InnerTextAttributeName = "innerText";
        public static string OutputFileExtension = ".bin";
        private static Dictionary<string, short> stringValue = new Dictionary<string, short>();
        private static string[] stringLookup;
        private static short stringCounter;

        public static void ToBinary(string filename, string outdir = null)
        {
            string extension = Path.GetExtension(filename);
            if (outdir != null)
                Path.Combine(outdir + Path.GetFileName(filename));
            filename.Replace(extension, OutputFileExtension);
            XmlDocument xmlDocument = new();
            xmlDocument.Load(filename);
            XmlElement rootElement = (XmlElement)null;
            foreach (object childNode in xmlDocument.ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    rootElement = childNode as XmlElement;
                    break;
                }
            }
            ToBinary(rootElement, outdir);
        }

        public static void ToBinary(XmlElement rootElement, string outfilename)
        {
            stringValue.Clear();
            stringCounter = 0;
            CreateLookupTable(rootElement);
            AddLookupValue(InnerTextAttributeName);
            using (FileStream output = new FileStream(outfilename, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(output);
                writer.Write("CELESTE MAP");
                writer.Write(Path.GetFileNameWithoutExtension(outfilename));
                writer.Write(stringValue.Count);
                foreach (KeyValuePair<string, short> keyValuePair in stringValue)
                    writer.Write(keyValuePair.Key);
                WriteElement(writer, rootElement);
                writer.Flush();
            }
        }

        private static void CreateLookupTable(XmlElement element)
        {
            AddLookupValue(element.Name);
            foreach (XmlAttribute attribute in (XmlNamedNodeMap)element.Attributes)
            {
                if (!IgnoreAttributes.Contains(attribute.Name))
                {
                    AddLookupValue(attribute.Name);
                    if (ParseValue(attribute.Value, out byte type, out object _) && type == 5)
                        AddLookupValue(attribute.Value);
                }
            }
            foreach (object childNode in element.ChildNodes)
            {
                if (childNode is XmlElement)
                    CreateLookupTable(childNode as XmlElement);
            }
        }

        private static void AddLookupValue(string name)
        {
            if (stringValue.ContainsKey(name))
                return;
            stringValue.Add(name, stringCounter);
            ++stringCounter;
        }

        private static void WriteElement(BinaryWriter writer, XmlElement element)
        {
            int num1 = 0;
            foreach (object childNode in element.ChildNodes)
            {
                if (childNode is XmlElement)
                    ++num1;
            }
            int num2 = 0;
            foreach (XmlAttribute attribute in (XmlNamedNodeMap)element.Attributes)
            {
                if (!IgnoreAttributes.Contains(attribute.Name))
                    ++num2;
            }
            if (element.InnerText.Length > 0 && num1 == 0)
                ++num2;
            writer.Write(stringValue[element.Name]);
            writer.Write(num2);
            foreach (XmlAttribute attribute in (XmlNamedNodeMap)element.Attributes)
            {
                if (!IgnoreAttributes.Contains(attribute.Name))
                {
                    ParseValue(attribute.Value, out byte type, out object result);
                    writer.Write(stringValue[attribute.Name]);
                    writer.Write(type);
                    switch (type)
                    {
                        case 0:
                            writer.Write((bool)result);
                            continue;
                        case 1:
                            writer.Write((byte)result);
                            continue;
                        case 2:
                            writer.Write((short)result);
                            continue;
                        case 3:
                            writer.Write((int)result);
                            continue;
                        case 4:
                            writer.Write((float)result);
                            continue;
                        case 5:
                            writer.Write(stringValue[(string)result]);
                            continue;
                        default:
                            continue;
                    }
                }
            }
            if (element.InnerText.Length > 0 && num1 == 0)
            {
                writer.Write(stringValue[InnerTextAttributeName]);
                if (element.Name == "solids" || element.Name == "bg")
                {
                    byte[] buffer = RunLengthEncoding.Encode(element.InnerText);
                    writer.Write(7);
                    writer.Write(buffer.Length);
                    writer.Write(buffer);
                }
                else
                {
                    writer.Write(6);
                    writer.Write(element.InnerText);
                }
            }
            writer.Write(num1);
            foreach (object childNode in element.ChildNodes)
            {
                if (childNode is XmlElement)
                    WriteElement(writer, childNode as XmlElement);
            }
        }

        private static bool ParseValue(string value, out byte type, out object result)
        {
            if (bool.TryParse(value, out bool boolResult))
            {
                type = 0;
                result = boolResult;
            }
            else
            {
                if (byte.TryParse(value, out byte byteResult))
                {
                    type = 1;
                    result = byteResult;
                }
                else
                {
                    if (short.TryParse(value, out short shortResult))
                    {
                        type = 2;
                        result = shortResult;
                    }
                    else
                    {
                        if (int.TryParse(value, out int intResult))
                        {
                            type = 3;
                            result = intResult;
                        }
                        else
                        {
                            if (float.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, (IFormatProvider)CultureInfo.InvariantCulture, out float floatResult))
                            {
                                type = 4;
                                result = floatResult;
                            }
                            else
                            {
                                type = 5;
                                result = value;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static Element FromBinary(string filename)
        {
            Element element;
            using (FileStream input = File.OpenRead(filename))
            {
                BinaryReader reader = new(input);
                reader.ReadString();
                string mapName = reader.ReadString();
                short length = reader.ReadInt16();
                stringLookup = new string[length];
                for (int i = 0; i < length; ++i)
                    stringLookup[i] = reader.ReadString();
                element = ReadElement(reader);
                element.Package = mapName;
            }
            return element;
        }

        private static Element ReadElement(BinaryReader reader)
        {
            Element element = new()
            {
                Name = stringLookup[reader.ReadInt16()]
            };

            byte valueCount = reader.ReadByte();
            if (valueCount > 0)
                element.Attributes = new Dictionary<string, object>();

            for (int i = 0; i < valueCount; ++i)
            {
                string key = stringLookup[reader.ReadInt16()];
                byte value = reader.ReadByte();
                object obj = null;
                switch (value)
                {
                    case 0:
                        obj = reader.ReadBoolean();
                        break;
                    case 1:
                        obj = Convert.ToInt32(reader.ReadByte());
                        break;
                    case 2:
                        obj = Convert.ToInt32(reader.ReadInt16());
                        break;
                    case 3:
                        obj = reader.ReadInt32();
                        break;
                    case 4:
                        obj = reader.ReadSingle();
                        break;
                    case 5:
                        obj = stringLookup[reader.ReadInt16()];
                        break;
                    case 6:
                        obj = reader.ReadString();
                        break;
                    case 7:
                        short count = reader.ReadInt16();
                        obj = RunLengthEncoding.Decode(reader.ReadBytes(count));
                        break;
                }
                element.Attributes.Add(key, obj);
            }

            short childCount = reader.ReadInt16();
            if (childCount > 0)
                element.Children = new List<Element>();

            for (int i = 0; i < childCount; ++i)
                element.Children.Add(ReadElement(reader));

            return element;
        }

        public class Element
        {
            public string Package;
            public string Name;
            public Dictionary<string, object> Attributes;
            public List<Element> Children;

            public bool HasAttr(string name) => Attributes != null && Attributes.ContainsKey(name);

            public string Attr(string name, string defaultValue = "")
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj.ToString();
            }

            public bool AttrBool(string name, bool defaultValue = false)
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj is bool flag ? flag : bool.Parse(obj.ToString());
            }

            public float AttrFloat(string name, float defaultValue = 0.0f)
            {
                if (Attributes == null || !Attributes.TryGetValue(name, out object obj))
                    obj = defaultValue;
                return obj is float num ? num : float.Parse(obj.ToString(), CultureInfo.InvariantCulture);
            }
        }
    }
}