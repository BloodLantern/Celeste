// Decompiled with JetBrains decompiler
// Type: Celeste.NoRefillTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
    public class NoRefillTrigger : Trigger
    {
        public bool State;

        public NoRefillTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.State = data.Bool("state");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            this.SceneAs<Level>().Session.Inventory.NoRefills = this.State;
        }
    }
}
