using System;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public struct EntityID
    {
        public static readonly EntityID None = new EntityID("null", -1);
        [XmlIgnore]
        public string Level;
        [XmlIgnore]
        public int ID;

        [XmlAttribute]
        public string Key
        {
            get => this.Level + ":" + (object) this.ID;
            set
            {
                string[] strArray = value.Split(':');
                this.Level = strArray[0];
                this.ID = int.Parse(strArray[1]);
            }
        }

        public EntityID(string level, int entityID)
        {
            this.Level = level;
            this.ID = entityID;
        }

        public override string ToString() => this.Key;

        public override int GetHashCode() => this.Level.GetHashCode() ^ this.ID;
    }
}
