using Monocle;

namespace Celeste
{
    [Tracked()]
    public class MirrorReflection : Component
    {
        public bool IgnoreEntityVisible;
        public bool IsRendering;

        public MirrorReflection()
            : base(false, true)
        {
        }
    }
}
