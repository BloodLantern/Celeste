﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class VirtualRenderTarget : VirtualAsset
    {
        public RenderTarget2D Target;
        public int MultiSampleCount;

        public bool Depth { get; private set; }

        public bool Preserve { get; private set; }

        public bool IsDisposed => Target == null || Target.IsDisposed || Target.GraphicsDevice.IsDisposed;

        public Rectangle Bounds => Target.Bounds;

        internal VirtualRenderTarget(
            string name,
            int width,
            int height,
            int multiSampleCount,
            bool depth,
            bool preserve)
        {
            Name = name;
            Width = width;
            Height = height;
            MultiSampleCount = multiSampleCount;
            Depth = depth;
            Preserve = preserve;
            Reload();
        }

        internal override void Unload()
        {
            if (Target == null || Target.IsDisposed)
                return;
            Target.Dispose();
        }

        internal override void Reload()
        {
            Unload();
            Target = new RenderTarget2D(Engine.Instance.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, Depth ? DepthFormat.Depth24Stencil8 : DepthFormat.None, MultiSampleCount, Preserve ? RenderTargetUsage.PreserveContents : RenderTargetUsage.DiscardContents);
        }

        public override void Dispose()
        {
            Unload();
            Target = null;
            VirtualContent.Remove(this);
        }

        public static implicit operator RenderTarget2D(VirtualRenderTarget target) => target.Target;
    }
}
