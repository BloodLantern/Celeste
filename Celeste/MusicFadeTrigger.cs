using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MusicFadeTrigger : Trigger
    {
        public bool LeftToRight;
        public float FadeA;
        public float FadeB;
        public string Parameter;

        public MusicFadeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.LeftToRight = data.Attr("direction", "leftToRight") == "leftToRight";
            this.FadeA = data.Float("fadeA");
            this.FadeB = data.Float("fadeB", 1f);
            this.Parameter = data.Attr("parameter");
        }

        public override void OnStay(Player player)
        {
            float num = !this.LeftToRight ? Calc.ClampedMap(player.Center.Y, this.Top, this.Bottom, this.FadeA, this.FadeB) : Calc.ClampedMap(player.Center.X, this.Left, this.Right, this.FadeA, this.FadeB);
            if (string.IsNullOrEmpty(this.Parameter))
                Audio.SetMusicParam("fade", num);
            else
                Audio.SetMusicParam(this.Parameter, num);
        }
    }
}
