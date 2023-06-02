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
