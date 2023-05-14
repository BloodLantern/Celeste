﻿// Decompiled with JetBrains decompiler
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
        public static readonly AreaKey None = new(-1);
        public static readonly AreaKey Default = new(0);
        [XmlAttribute]
        public int ID;
        [XmlAttribute]
        public AreaMode Mode;

        public AreaKey(int id, AreaMode mode = AreaMode.Normal)
        {
            ID = id;
            Mode = mode;
        }

        public int ChapterIndex
        {
            get
            {
                if (AreaData.Areas[ID].Interlude)
                {
                    return -1;
                }

                int chapterIndex = 0;
                for (int i = 0; i <= ID; ++i)
                {
                    if (!AreaData.Areas[i].Interlude)
                    {
                        ++chapterIndex;
                    }
                }
                return chapterIndex;
            }
        }

        public static bool operator ==(AreaKey a, AreaKey b)
        {
            return a.ID == b.ID && a.Mode == b.Mode;
        }

        public static bool operator !=(AreaKey a, AreaKey b)
        {
            return a.ID != b.ID || a.Mode != b.Mode;
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return (ID * 3) + (int)Mode;
        }

        public override string ToString()
        {
            string str = ID.ToString();
            if (Mode == AreaMode.BSide)
            {
                str += "H";
            }
            else if (Mode == AreaMode.CSide)
            {
                str += "HH";
            }

            return str;
        }
    }
}