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
