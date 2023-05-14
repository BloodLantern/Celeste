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
            OnCollide = onCollide;
            Collider = null;
        }

        public void Check(Puffer puffer)
        {
            if (OnCollide == null)
            {
                return;
            }

            Collider collider = Entity.Collider;
            if (Collider != null)
            {
                Entity.Collider = Collider;
            }

            if (puffer.CollideCheck(Entity))
            {
                OnCollide(puffer);
            }

            Entity.Collider = collider;
        }
    }
}
