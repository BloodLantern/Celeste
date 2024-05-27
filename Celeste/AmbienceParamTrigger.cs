using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class AmbienceParamTrigger : Trigger
    {
        public string Parameter;
        public float From;
        public float To;
        public PositionModes PositionMode;

        public AmbienceParamTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Parameter = data.Attr("parameter");
            From = data.Float("from");
            To = data.Float("to");
            PositionMode = data.Enum<PositionModes>("direction");
        }

        public override void OnStay(Player player)
        {
            float num = Calc.ClampedMap(GetPositionLerp(player, PositionMode), 0.0f, 1f, From, To);
            Level scene = Scene as Level;
            scene.Session.Audio.Ambience.Param(Parameter, num);
            scene.Session.Audio.Apply();
        }
    }
}
