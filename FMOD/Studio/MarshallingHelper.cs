// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.MarshallingHelper
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    internal class MarshallingHelper
    {
        public static int stringLengthUtf8(IntPtr nativeUtf8)
        {
            int ofs = 0;
            while (Marshal.ReadByte(nativeUtf8, ofs) != 0)
            {
                ++ofs;
            }

            return ofs;
        }

        public static string stringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int count = MarshallingHelper.stringLengthUtf8(nativeUtf8);
            if (count == 0)
            {
                return string.Empty;
            }

            byte[] numArray = new byte[count];
            Marshal.Copy(nativeUtf8, numArray, 0, numArray.Length);
            return Encoding.UTF8.GetString(numArray, 0, count);
        }
    }
}
