using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD
{
    internal class StringMarshalHelper
    {
        internal static void NativeToBuilder(StringBuilder builder, IntPtr nativeMem)
        {
            byte[] numArray = new byte[builder.Capacity];
            Marshal.Copy(nativeMem, numArray, 0, builder.Capacity);
            int count = Array.IndexOf<byte>(numArray, (byte) 0);
            if (count <= 0)
                return;
            string str = Encoding.UTF8.GetString(numArray, 0, count);
            builder.Append(str);
        }
    }
}
