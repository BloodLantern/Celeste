using Microsoft.Xna.Framework;

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
            if (Scene.Entities.FindFirst<Snowball>() == null)
                Scene.Add(new Snowball());
            RemoveSelf();
        }
    }
}
