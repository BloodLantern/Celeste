// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.Util
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
  public class Util
  {
    public static RESULT ParseID(string idString, out Guid id) => Util.FMOD_Studio_ParseID(Encoding.UTF8.GetBytes(idString + "\0"), out id);

    [DllImport("fmodstudio")]
    private static extern RESULT FMOD_Studio_ParseID(byte[] idString, out Guid id);
  }
}
