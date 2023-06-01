// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerCollider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class PlayerCollider : Component
    {
        public Action<Player> OnCollide;
        public Collider Collider;
        public Collider FeatherCollider;

        public PlayerCollider(Action<Player> onCollide, Collider collider = null, Collider featherCollider = null)
            : base(false, false)
        {
            this.OnCollide = onCollide;
            this.Collider = collider;
            this.FeatherCollider = featherCollider;
        }

        public bool Check(Player player)
        {
            Collider collider1 = this.Collider;
            if (this.FeatherCollider != null && player.StateMachine.State == 19)
                collider1 = this.FeatherCollider;
            if (collider1 == null)
            {
                if (!player.CollideCheck(this.Entity))
                    return false;
                this.OnCollide(player);
                return true;
            }
            Collider collider2 = this.Entity.Collider;
            this.Entity.Collider = collider1;
            int num = player.CollideCheck(this.Entity) ? 1 : 0;
            this.Entity.Collider = collider2;
            if (num == 0)
                return false;
            this.OnCollide(player);
            return true;
        }

        public override void DebugRender(Camera camera)
        {
            if (this.Collider == null)
                return;
            Collider collider = this.Entity.Collider;
            this.Entity.Collider = this.Collider;
            this.Collider.Render(camera, Color.HotPink);
            this.Entity.Collider = collider;
        }
    }
}
