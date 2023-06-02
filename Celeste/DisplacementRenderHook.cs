using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class DisplacementRenderHook : Component
    {
        public Action RenderDisplacement;

        public DisplacementRenderHook(Action render)
            : base(false, true)
        {
            this.RenderDisplacement = render;
        }
    }
}
