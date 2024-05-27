using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class CustomBloom : Component
    {
        public Action OnRenderBloom;

        public CustomBloom(Action onRenderBloom)
            : base(false, true)
        {
            OnRenderBloom = onRenderBloom;
        }
    }
}
