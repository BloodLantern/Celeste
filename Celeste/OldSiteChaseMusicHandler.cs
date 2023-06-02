using Monocle;

namespace Celeste
{
    public class OldSiteChaseMusicHandler : Entity
    {
        public OldSiteChaseMusicHandler() => this.Tag = (int) Tags.TransitionUpdate | (int) Tags.Global;

        public override void Update()
        {
            base.Update();
            int num1 = 1150;
            int num2 = 2832;
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null || !(Audio.CurrentMusic == "event:/music/lvl2/chase"))
                return;
            Audio.SetMusicParam("escape", (entity.X - (float) num1) / (float) (num2 - num1));
        }
    }
}
