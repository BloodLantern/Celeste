using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class CustomBloom : Component
    {
        public Action OnRenderBloom;

        public CustomBloom(Action onRenderBloom)
            : base(false, true)
        {
            this.OnRenderBloom = onRenderBloom;
        }
    }
}
