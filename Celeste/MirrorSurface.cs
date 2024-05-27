using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class MirrorSurface : Component
    {
        public Action OnRender;
        private Vector2 reflectionOffset;

        public Vector2 ReflectionOffset
        {
            get => reflectionOffset;
            set
            {
                reflectionOffset = value;
                ReflectionColor = new Color((float) (0.5 + Calc.Clamp(reflectionOffset.X / 32f, -1f, 1f) * 0.5), (float) (0.5 + Calc.Clamp(reflectionOffset.Y / 32f, -1f, 1f) * 0.5), 0.0f, 1f);
            }
        }

        public Color ReflectionColor { get; private set; }

        public MirrorSurface(Action onRender = null)
            : base(false, true)
        {
            OnRender = onRender;
        }
    }
}
