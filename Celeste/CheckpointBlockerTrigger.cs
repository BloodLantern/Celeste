using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
    public class CheckpointBlockerTrigger : Trigger
    {
        public CheckpointBlockerTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }
    }
}
