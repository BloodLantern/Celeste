// Decompiled with JetBrains decompiler
// Type: Celeste.MEP
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class MEP
    {
        [XmlAttribute]
        public string Key;
        [XmlAttribute]
        public float Value;

        public MEP()
        {
        }

        public MEP(string key, float value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
