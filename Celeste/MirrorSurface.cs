// Decompiled with JetBrains decompiler
// Type: Celeste.MirrorSurface
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
