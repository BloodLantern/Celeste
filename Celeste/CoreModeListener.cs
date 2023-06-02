using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class CoreModeListener : Component
    {
        public Action<Session.CoreModes> OnChange;

        public CoreModeListener(Action<Session.CoreModes> onChange)
            : base(false, false)
        {
            this.OnChange = onChange;
        }
    }
}
