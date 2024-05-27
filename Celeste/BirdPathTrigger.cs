using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BirdPathTrigger : Trigger
    {
        private BirdPath bird;
        private bool triggered;

        public BirdPathTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            BirdPath first = Scene.Entities.FindFirst<BirdPath>();
            if (first != null)
            {
                bird = first;
                bird.WaitForTrigger();
            }
            else
                RemoveSelf();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered)
                return;
            bird.Trigger();
            triggered = true;
        }
    }
}
