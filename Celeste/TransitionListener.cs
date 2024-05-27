using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class TransitionListener : Component
    {
        public Action OnInBegin;
        public Action OnInEnd;
        public Action<float> OnIn;
        public Action OnOutBegin;
        public Action<float> OnOut;

        public TransitionListener()
            : base(false, false)
        {
        }
    }
}
