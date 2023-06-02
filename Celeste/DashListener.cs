using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class DashListener : Component
    {
        public Action<Vector2> OnDash;
        public Action OnSet;

        public DashListener()
            : base(false, false)
        {
        }
    }
}
