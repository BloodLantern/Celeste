// Decompiled with JetBrains decompiler
// Type: Monocle.BitTag
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class BitTag
    {
        internal static int TotalTags = 0;
        internal static BitTag[] byID = new BitTag[32];
        private static readonly Dictionary<string, BitTag> byName = new(StringComparer.OrdinalIgnoreCase);
        public int ID;
        public int Value;
        public string Name;

        public static BitTag Get(string name)
        {
            return BitTag.byName[name];
        }

        public BitTag(string name)
        {
            ID = BitTag.TotalTags;
            Value = 1 << BitTag.TotalTags;
            Name = name;
            BitTag.byID[ID] = this;
            BitTag.byName[name] = this;
            ++BitTag.TotalTags;
        }

        public static implicit operator int(BitTag tag)
        {
            return tag.Value;
        }
    }
}
