// Decompiled with JetBrains decompiler
// Type: Celeste.WindAttackTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class WindAttackTrigger : Trigger
    {
        public WindAttackTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (this.Scene.Entities.FindFirst<Snowball>() == null)
                this.Scene.Add((Entity) new Snowball());
            this.RemoveSelf();
        }
    }
}
