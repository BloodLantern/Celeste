// Decompiled with JetBrains decompiler
// Type: Celeste.PufferCollider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class PufferCollider : Component
    {
        public Action<Puffer> OnCollide;
        public Collider Collider;

        public PufferCollider(Action<Puffer> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.OnCollide = onCollide;
            this.Collider = (Collider) null;
        }

        public void Check(Puffer puffer)
        {
            if (this.OnCollide == null)
                return;
            Collider collider = this.Entity.Collider;
            if (this.Collider != null)
                this.Entity.Collider = this.Collider;
            if (puffer.CollideCheck(this.Entity))
                this.OnCollide(puffer);
            this.Entity.Collider = collider;
        }
    }
}
