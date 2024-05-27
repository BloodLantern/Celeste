using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class LevelEndingHook : Component
    {
        public Action OnEnd;

        public LevelEndingHook(Action onEnd)
            : base(false, false)
        {
            OnEnd = onEnd;
        }
    }
}
