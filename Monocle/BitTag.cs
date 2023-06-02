﻿using System;
using System.Collections.Generic;

namespace Monocle
{
    public class BitTag
    {
        internal static int TotalTags = 0;
        internal static BitTag[] byID = new BitTag[32];
        private static Dictionary<string, BitTag> byName = new Dictionary<string, BitTag>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        public int ID;
        public int Value;
        public string Name;

        public static BitTag Get(string name) => BitTag.byName[name];

        public BitTag(string name)
        {
            this.ID = BitTag.TotalTags;
            this.Value = 1 << BitTag.TotalTags;
            this.Name = name;
            BitTag.byID[this.ID] = this;
            BitTag.byName[name] = this;
            ++BitTag.TotalTags;
        }

        public static implicit operator int(BitTag tag) => tag.Value;
    }
}
