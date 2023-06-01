// Decompiled with JetBrains decompiler
// Type: Celeste.EntityData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Celeste
{
    public class EntityData
    {
        public int ID;
        public string Name;
        public LevelData Level;
        public Vector2 Position;
        public Vector2 Origin;
        public int Width;
        public int Height;
        public Vector2[] Nodes;
        public Dictionary<string, object> Values;

        public Vector2[] NodesOffset(Vector2 offset)
        {
            if (this.Nodes == null)
                return (Vector2[]) null;
            Vector2[] vector2Array = new Vector2[this.Nodes.Length];
            for (int index = 0; index < this.Nodes.Length; ++index)
                vector2Array[index] = this.Nodes[index] + offset;
            return vector2Array;
        }

        public Vector2[] NodesWithPosition(Vector2 offset)
        {
            if (this.Nodes == null)
                return new Vector2[1]{ this.Position + offset };
            Vector2[] vector2Array = new Vector2[this.Nodes.Length + 1];
            vector2Array[0] = this.Position + offset;
            for (int index = 0; index < this.Nodes.Length; ++index)
                vector2Array[index + 1] = this.Nodes[index] + offset;
            return vector2Array;
        }

        public bool Has(string key) => this.Values.ContainsKey(key);

        public string Attr(string key, string defaultValue = "")
        {
            object obj;
            return this.Values != null && this.Values.TryGetValue(key, out obj) ? obj.ToString() : defaultValue;
        }

        public float Float(string key, float defaultValue = 0.0f)
        {
            object obj;
            if (this.Values != null && this.Values.TryGetValue(key, out obj))
            {
                if (obj is float num)
                    return num;
                float result;
                if (float.TryParse(obj.ToString(), NumberStyles.Float, (IFormatProvider) CultureInfo.InvariantCulture, out result))
                    return result;
            }
            return defaultValue;
        }

        public bool Bool(string key, bool defaultValue = false)
        {
            object obj;
            if (this.Values != null && this.Values.TryGetValue(key, out obj))
            {
                if (obj is bool flag)
                    return flag;
                bool result;
                if (bool.TryParse(obj.ToString(), out result))
                    return result;
            }
            return defaultValue;
        }

        public int Int(string key, int defaultValue = 0)
        {
            object obj;
            if (this.Values != null && this.Values.TryGetValue(key, out obj))
            {
                if (obj is int num)
                    return num;
                int result;
                if (int.TryParse(obj.ToString(), out result))
                    return result;
            }
            return defaultValue;
        }

        public char Char(string key, char defaultValue = '\0')
        {
            object obj;
            char result;
            return this.Values != null && this.Values.TryGetValue(key, out obj) && char.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        public Vector2? FirstNodeNullable(Vector2? offset = null)
        {
            if (this.Nodes == null || this.Nodes.Length == 0)
                return new Vector2?();
            return offset.HasValue ? new Vector2?(this.Nodes[0] + offset.Value) : new Vector2?(this.Nodes[0]);
        }

        public T Enum<T>(string key, T defaultValue = default (T)) where T : struct
        {
            object obj;
            T result;
            return this.Values != null && this.Values.TryGetValue(key, out obj) && System.Enum.TryParse<T>(obj.ToString(), true, out result) ? result : defaultValue;
        }

        public Color HexColor(string key, Color defaultValue = default (Color))
        {
            object obj;
            if (this.Values.TryGetValue(key, out obj))
            {
                string hex = obj.ToString();
                if (hex.Length == 6)
                    return Calc.HexToColor(hex);
            }
            return defaultValue;
        }
    }
}
