using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class PostUpdateHook : Component
    {
        public Action OnPostUpdate;

        public PostUpdateHook(Action onPostUpdate)
            : base(false, false)
        {
            this.OnPostUpdate = onPostUpdate;
        }
    }
}
