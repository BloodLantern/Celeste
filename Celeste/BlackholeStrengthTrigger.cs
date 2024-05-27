using Microsoft.Xna.Framework;

namespace Celeste
{
    public class BlackholeStrengthTrigger : Trigger
    {
        private BlackholeBG.Strengths strength;

        public BlackholeStrengthTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            strength = data.Enum<BlackholeBG.Strengths>(nameof (strength));
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            (Scene as Level).Background.Get<BlackholeBG>()?.NextStrength(Scene as Level, strength);
        }
    }
}
