// Decompiled with JetBrains decompiler
// Type: Celeste.SeekerCollider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class SeekerCollider : Component
    {
        public Action<Seeker> OnCollide;
        public Collider Collider;

        public SeekerCollider(Action<Seeker> onCollide, Collider collider = null)
            : base(false, false)
        {
            OnCollide = onCollide;
            Collider = null;
        }

        public void Check(Seeker seeker)
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

            if (seeker.CollideCheck(Entity))
            {
                OnCollide(seeker);
            }

            Entity.Collider = collider;
        }
    }
}
