using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class DustEdge : Component
    {
        public Action RenderDust;

        public DustEdge(Action onRenderDust)
            : base(false, true)
        {
            this.RenderDust = onRenderDust;
        }
    }
}
