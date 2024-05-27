using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class DustEdge : Component
    {
        public Action RenderDust;

        public DustEdge(Action onRenderDust)
            : base(false, true)
        {
            RenderDust = onRenderDust;
        }
    }
}
