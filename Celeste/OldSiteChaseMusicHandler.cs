using Monocle;

namespace Celeste
{
    public class OldSiteChaseMusicHandler : Entity
    {
        public OldSiteChaseMusicHandler() => Tag = (int) Tags.TransitionUpdate | (int) Tags.Global;

        public override void Update()
        {
            base.Update();
            int num1 = 1150;
            int num2 = 2832;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !(Audio.CurrentMusic == "event:/music/lvl2/chase"))
                return;
            Audio.SetMusicParam("escape", (entity.X - num1) / (num2 - num1));
        }
    }
}
