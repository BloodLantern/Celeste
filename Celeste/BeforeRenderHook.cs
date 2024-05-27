using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class BeforeRenderHook : Component
    {
        public Action Callback;

        public BeforeRenderHook(Action callback)
            : base(false, true)
        {
            Callback = callback;
        }
    }
}
