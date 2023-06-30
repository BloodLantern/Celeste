using System.Collections.Generic;
using System.Text;

namespace Celeste
{
    public static class RunLengthEncoding
    {
        public static byte[] Encode(string str)
        {
            List<byte> result = new();
            for (int i = 0; i < str.Length; i++)
            {
                byte valueCount = 1;
                char value;
                for (value = str[i]; i + 1 < str.Length && str[i + 1] == value && valueCount < byte.MaxValue; ++i)
                    ++valueCount;
                result.Add(valueCount);
                result.Add((byte)value);
            }
            return result.ToArray();
        }

        public static string Decode(byte[] bytes)
        {
            StringBuilder result = new();
            for (int i = 0; i < bytes.Length; i += 2)
                result.Append((char)bytes[i + 1], bytes[i]);
            return result.ToString();
        }
    }
}