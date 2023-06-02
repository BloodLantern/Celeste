using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class MirrorSurface : Component
    {
        public Action OnRender;
        private Vector2 reflectionOffset;

        public Vector2 ReflectionOffset
        {
            get => this.reflectionOffset;
            set
            {
                this.reflectionOffset = value;
                this.ReflectionColor = new Color((float) (0.5 + (double) Calc.Clamp(this.reflectionOffset.X / 32f, -1f, 1f) * 0.5), (float) (0.5 + (double) Calc.Clamp(this.reflectionOffset.Y / 32f, -1f, 1f) * 0.5), 0.0f, 1f);
            }
        }

        public Color ReflectionColor { get; private set; }

        public MirrorSurface(Action onRender = null)
            : base(false, true)
        {
            this.OnRender = onRender;
        }
    }
}
