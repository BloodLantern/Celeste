// Decompiled with JetBrains decompiler
// Type: Monocle.Tracked
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Monocle
{
    public class Tracked : Attribute
    {
        public bool Inherited;

        public Tracked(bool inherited = false) => this.Inherited = inherited;
    }
}
