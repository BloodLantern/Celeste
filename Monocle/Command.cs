// Decompiled with JetBrains decompiler
// Type: Monocle.Command
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Monocle
{
  public class Command : Attribute
  {
    public string Name;
    public string Help;

    public Command(string name, string help)
    {
      this.Name = name;
      this.Help = help;
    }
  }
}
