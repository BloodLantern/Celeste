using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class DisplacementRenderHook : Component
    {
        public Action RenderDisplacement;

        public DisplacementRenderHook(Action render)
            : base(false, true)
        {
            RenderDisplacement = render;
        }
    }
}
