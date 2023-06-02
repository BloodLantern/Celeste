using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class BeforeRenderHook : Component
    {
        public Action Callback;

        public BeforeRenderHook(Action callback)
            : base(false, true)
        {
            this.Callback = callback;
        }
    }
}
