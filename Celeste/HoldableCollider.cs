// Decompiled with JetBrains decompiler
// Type: Celeste.HoldableCollider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class HoldableCollider : Component
    {
        private readonly Collider collider;
        public Action<Holdable> OnCollide;

        public HoldableCollider(Action<Holdable> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.collider = collider;
            OnCollide = onCollide;
        }

        public bool Check(Holdable holdable)
        {
            Collider collider = Entity.Collider;
            if (this.collider != null)
            {
                Entity.Collider = this.collider;
            }

            int num = holdable.Entity.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider;
            return num != 0;
        }
    }
}
