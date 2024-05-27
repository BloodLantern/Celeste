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
            LeftToRight = data.Attr("direction", "leftToRight") == "leftToRight";
            FadeA = data.Float("fadeA");
            FadeB = data.Float("fadeB", 1f);
            Parameter = data.Attr("parameter");
        }

        public override void OnStay(Player player)
        {
            float num = !LeftToRight ? Calc.ClampedMap(player.Center.Y, Top, Bottom, FadeA, FadeB) : Calc.ClampedMap(player.Center.X, Left, Right, FadeA, FadeB);
            if (string.IsNullOrEmpty(Parameter))
                Audio.SetMusicParam("fade", num);
            else
                Audio.SetMusicParam(Parameter, num);
        }
    }
}
