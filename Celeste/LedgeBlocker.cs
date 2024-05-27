using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class LedgeBlocker : Component
    {
        public bool Blocking = true;
        public Func<Player, bool> BlockChecker;

        public LedgeBlocker(Func<Player, bool> blockChecker = null)
            : base(false, false)
        {
            BlockChecker = blockChecker;
        }

        public bool HopBlockCheck(Player player)
        {
            if (!Blocking || !player.CollideCheck(Entity, player.Position + Vector2.UnitX * (float) player.Facing * 8f))
                return false;
            return BlockChecker == null || BlockChecker(player);
        }

        public bool JumpThruBoostCheck(Player player)
        {
            if (!Blocking || !player.CollideCheck(Entity, player.Position - Vector2.UnitY * 2f))
                return false;
            return BlockChecker == null || BlockChecker(player);
        }

        public bool DashCorrectCheck(Player player)
        {
            if (!Blocking || !player.CollideCheck(Entity, player.Position))
                return false;
            return BlockChecker == null || BlockChecker(player);
        }
    }
}
