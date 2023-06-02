using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class WindMover : Component
    {
        public Action<Vector2> Move;

        public WindMover(Action<Vector2> move)
            : base(false, false)
        {
            this.Move = move;
        }
    }
}
