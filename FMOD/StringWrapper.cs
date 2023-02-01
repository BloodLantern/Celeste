// Decompiled with JetBrains decompiler
// Type: FMOD.StringWrapper
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD
{
  public struct StringWrapper
  {
    private IntPtr nativeUtf8Ptr;

    public static implicit operator string(StringWrapper fstring)
    {
      if (fstring.nativeUtf8Ptr == IntPtr.Zero)
        return "";
      int length = 0;
      while (Marshal.ReadByte(fstring.nativeUtf8Ptr, length) != (byte) 0)
        ++length;
      if (length <= 0)
        return "";
      byte[] numArray = new byte[length];
      Marshal.Copy(fstring.nativeUtf8Ptr, numArray, 0, length);
      return Encoding.UTF8.GetString(numArray, 0, length);
    }
  }
}
