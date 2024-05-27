using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class CoreModeListener : Component
    {
        public Action<Session.CoreModes> OnChange;

        public CoreModeListener(Action<Session.CoreModes> onChange)
            : base(false, false)
        {
            OnChange = onChange;
        }
    }
}
