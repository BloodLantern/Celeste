// Decompiled with JetBrains decompiler
// Type: Celeste.AreaKey
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Xml.Serialization;

namespace Celeste
{
  [Serializable]
  public struct AreaKey
  {
    public static readonly AreaKey None = new AreaKey(-1);
    public static readonly AreaKey Default = new AreaKey(0);
    [XmlAttribute]
    public int ID;
    [XmlAttribute]
    public AreaMode Mode;

    public AreaKey(int id, AreaMode mode = AreaMode.Normal)
    {
      this.ID = id;
      this.Mode = mode;
    }

    public int ChapterIndex
    {
      get
      {
        if (AreaData.Areas[this.ID].Interlude)
          return -1;
        int chapterIndex = 0;
        for (int index = 0; index <= this.ID; ++index)
        {
          if (!AreaData.Areas[index].Interlude)
            ++chapterIndex;
        }
        return chapterIndex;
      }
    }

    public static bool operator ==(AreaKey a, AreaKey b) => a.ID == b.ID && a.Mode == b.Mode;

    public static bool operator !=(AreaKey a, AreaKey b) => a.ID != b.ID || a.Mode != b.Mode;

    public override bool Equals(object obj) => false;

    public override int GetHashCode() => (int) (this.ID * 3 + this.Mode);

    public override string ToString()
    {
      string str = this.ID.ToString();
      if (this.Mode == AreaMode.BSide)
        str += "H";
      else if (this.Mode == AreaMode.CSide)
        str += "HH";
      return str;
    }
  }
}
