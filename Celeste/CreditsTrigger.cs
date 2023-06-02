using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CreditsTrigger : Trigger
    {
        public string Event;

        public CreditsTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Event = data.Attr("event");
        }

        public override void OnEnter(Player player)
        {
            this.Triggered = true;
            if (CS07_Credits.Instance == null)
                return;
            CS07_Credits.Instance.Event = this.Event;
        }
    }
}
