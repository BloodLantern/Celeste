// Decompiled with JetBrains decompiler
// Type: FMOD.DEBUG_CALLBACK
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace FMOD
{
  public delegate RESULT DEBUG_CALLBACK(
    DEBUG_FLAGS flags,
    string file,
    int line,
    string func,
    string message);
}
