// Decompiled with JetBrains decompiler
// Type: Celeste.Oui
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
    public abstract class Oui : Entity
    {
        public bool Focused;

        public Overworld Overworld => this.SceneAs<Overworld>();

        public bool Selected => this.Overworld != null && this.Overworld.Current == this;

        public Oui() => this.AddTag((int) Tags.HUD);

        public virtual bool IsStart(Overworld overworld, Overworld.StartMode start) => false;

        public abstract IEnumerator Enter(Oui from);

        public abstract IEnumerator Leave(Oui next);
    }
}
