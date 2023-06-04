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

        public readonly int ChapterIndex
        {
            get
            {
                if (AreaData.Areas[ID].Interlude)
                    return -1;
                int chapterIndex = 0;
                for (int i = 0; i <= ID; i++)
                    if (!AreaData.Areas[i].Interlude)
                        ++chapterIndex;
                return chapterIndex;
            }
        }

        public static bool operator ==(AreaKey a, AreaKey b) => a.ID == b.ID && a.Mode == b.Mode;

        public static bool operator !=(AreaKey a, AreaKey b) => a.ID != b.ID || a.Mode != b.Mode;

        public override bool Equals(object obj) => false;

        public override int GetHashCode() => (int)(ID * 3 + Mode);

        public override string ToString()
        {
            string str = ID.ToString();
            if (Mode == AreaMode.BSide)
                str += "H";
            else if (Mode == AreaMode.CSide)
                str += "HH";
            return str;
        }
    }
}