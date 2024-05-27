using Microsoft.Xna.Framework;

namespace Celeste
{
    public class OshiroTrigger : Trigger
    {
        public bool State;

        public OshiroTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            State = data.Bool("state", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (State)
            {
                Level level = SceneAs<Level>();
                Scene.Add(new AngryOshiro(new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2), false));
                RemoveSelf();
            }
            else
            {
                Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
                RemoveSelf();
            }
        }
    }
}
