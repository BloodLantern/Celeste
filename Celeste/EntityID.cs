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

        public override string ToString() => Key;

        public override int GetHashCode() => Level.GetHashCode() ^ ID;
    }
}
