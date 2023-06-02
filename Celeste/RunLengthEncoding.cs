using System.Collections.Generic;
using System.Text;

namespace Celeste
{
    public static class RunLengthEncoding
    {
        public static byte[] Encode(string str)
        {
            List<byte> byteList = new();
            for (int index = 0; index < str.Length; ++index)
            {
                byte num = 1;
                char ch;
                for (ch = str[index]; index + 1 < str.Length && str[index + 1] == ch && num < byte.MaxValue; ++index)
                    ++num;
                byteList.Add(num);
                byteList.Add((byte) ch);
            }
            return byteList.ToArray();
        }

        public static string Decode(byte[] bytes)
        {
            StringBuilder stringBuilder = new();
            for (int index = 0; index < bytes.Length; index += 2)
                stringBuilder.Append((char) bytes[index + 1], bytes[index]);
            return stringBuilder.ToString();
        }
    }
}