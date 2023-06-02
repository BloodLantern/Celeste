using Microsoft.Xna.Framework;

namespace Celeste
{
    public class BlackholeStrengthTrigger : Trigger
    {
        private BlackholeBG.Strengths strength;

        public BlackholeStrengthTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.strength = data.Enum<BlackholeBG.Strengths>(nameof (strength));
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            (this.Scene as Level).Background.Get<BlackholeBG>()?.NextStrength(this.Scene as Level, this.strength);
        }
    }
}
