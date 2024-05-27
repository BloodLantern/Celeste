using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
    public class WindTrigger : Trigger
    {
        public WindController.Patterns Pattern;

        public WindTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Pattern = data.Enum<WindController.Patterns>("pattern");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            WindController first = Scene.Entities.FindFirst<WindController>();
            if (first == null)
                Scene.Add(new WindController(Pattern));
            else
                first.SetPattern(Pattern);
        }
    }
}
