using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class LevelEndingHook : Component
    {
        public Action OnEnd;

        public LevelEndingHook(Action onEnd)
            : base(false, false)
        {
            this.OnEnd = onEnd;
        }
    }
}
