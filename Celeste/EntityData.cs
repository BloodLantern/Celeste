using Microsoft.Xna.Framework;
using Monocle;
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
            if (Nodes == null)
                return null;
            Vector2[] vector2Array = new Vector2[Nodes.Length];
            for (int index = 0; index < Nodes.Length; ++index)
                vector2Array[index] = Nodes[index] + offset;
            return vector2Array;
        }

        public Vector2[] NodesWithPosition(Vector2 offset)
        {
            if (Nodes == null)
                return new Vector2[1]{ Position + offset };
            Vector2[] vector2Array = new Vector2[Nodes.Length + 1];
            vector2Array[0] = Position + offset;
            for (int index = 0; index < Nodes.Length; ++index)
                vector2Array[index + 1] = Nodes[index] + offset;
            return vector2Array;
        }

        public bool Has(string key) => Values.ContainsKey(key);

        public string Attr(string key, string defaultValue = "")
        {
            return Values != null && Values.TryGetValue(key, out object obj) ? obj.ToString() : defaultValue;
        }

        public float Float(string key, float defaultValue = 0.0f)
        {
            if (Values != null && Values.TryGetValue(key, out object obj))
            {
                if (obj is float num)
                    return num;
                if (float.TryParse(obj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    return result;
            }
            return defaultValue;
        }

        public bool Bool(string key, bool defaultValue = false)
        {
            if (Values != null && Values.TryGetValue(key, out object obj))
            {
                if (obj is bool flag)
                    return flag;
                if (bool.TryParse(obj.ToString(), out bool result))
                    return result;
            }
            return defaultValue;
        }

        public int Int(string key, int defaultValue = 0)
        {
            if (Values != null && Values.TryGetValue(key, out object obj))
            {
                if (obj is int num)
                    return num;
                if (int.TryParse(obj.ToString(), out int result))
                    return result;
            }
            return defaultValue;
        }

        public char Char(string key, char defaultValue = '\0')
        {
            return Values != null && Values.TryGetValue(key, out object obj) && char.TryParse(obj.ToString(), out char result) ? result : defaultValue;
        }

        public Vector2? FirstNodeNullable(Vector2? offset = null)
        {
            if (Nodes == null || Nodes.Length == 0)
                return new Vector2?();
            return offset.HasValue ? new Vector2?(Nodes[0] + offset.Value) : new Vector2?(Nodes[0]);
        }

        public T Enum<T>(string key, T defaultValue = default (T)) where T : struct
        {
            return Values != null && Values.TryGetValue(key, out object obj) && System.Enum.TryParse<T>(obj.ToString(), true, out T result) ? result : defaultValue;
        }

        public Color HexColor(string key, Color defaultValue = default (Color))
        {
            if (Values.TryGetValue(key, out object obj))
            {
                string hex = obj.ToString();
                if (hex.Length == 6)
                    return Calc.HexToColor(hex);
            }
            return defaultValue;
        }
    }
}
