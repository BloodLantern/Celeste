// Decompiled with JetBrains decompiler
// Type: Celeste.EntityID
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public struct EntityID
    {
        public static readonly EntityID None = new("null", -1);
        [XmlIgnore]
        public string Level;
        [XmlIgnore]
        public int ID;

        [XmlAttribute]
        public string Key
        {
            get => Level + ":" + ID;
            set
            {
                string[] strArray = value.Split(':');
                Level = strArray[0];
                ID = int.Parse(strArray[1]);
            }
        }

        public EntityID(string level, int entityID)
        {
            Level = level;
            ID = entityID;
        }

        public override string ToString()
        {
            return Key;
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode() ^ ID;
        }
    }
}
