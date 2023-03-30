// Decompiled with JetBrains decompiler
// Type: Celeste.LedgeBlocker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class LedgeBlocker : Component
    {
        public bool Blocking = true;
        public Func<Player, bool> BlockChecker;

        public LedgeBlocker(Func<Player, bool> blockChecker = null)
            : base(false, false)
        {
            this.BlockChecker = blockChecker;
        }

        public bool HopBlockCheck(Player player)
        {
            if (!this.Blocking || !player.CollideCheck(this.Entity, player.Position + Vector2.UnitX * (float) player.Facing * 8f))
                return false;
            return this.BlockChecker == null || this.BlockChecker(player);
        }

        public bool JumpThruBoostCheck(Player player)
        {
            if (!this.Blocking || !player.CollideCheck(this.Entity, player.Position - Vector2.UnitY * 2f))
                return false;
            return this.BlockChecker == null || this.BlockChecker(player);
        }

        public bool DashCorrectCheck(Player player)
        {
            if (!this.Blocking || !player.CollideCheck(this.Entity, player.Position))
                return false;
            return this.BlockChecker == null || this.BlockChecker(player);
        }
    }
}
