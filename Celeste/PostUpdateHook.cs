using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class PostUpdateHook : Component
    {
        public Action OnPostUpdate;

        public PostUpdateHook(Action onPostUpdate)
            : base(false, false)
        {
            OnPostUpdate = onPostUpdate;
        }
    }
}
