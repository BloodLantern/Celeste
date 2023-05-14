// Decompiled with JetBrains decompiler
// Type: Celeste.LightOcclude
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class LightOcclude : Component
    {
        public float Alpha = 1f;
        private Rectangle? bounds;
        public Rectangle RenderBounds;
        private Rectangle lastSize;
        private bool lastVisible;
        private float lastAlpha;

        public int Left => bounds.HasValue ? (int)Entity.X + bounds.Value.Left : (int)Entity.Collider.AbsoluteLeft;

        public int Width => bounds.HasValue ? bounds.Value.Width : (int)Entity.Collider.Width;

        public int Top => bounds.HasValue ? (int)Entity.Y + bounds.Value.Top : (int)Entity.Collider.AbsoluteTop;

        public int Height => bounds.HasValue ? bounds.Value.Height : (int)Entity.Collider.Height;

        public int Right => Left + Width;

        public int Bottom => Top + Height;

        public LightOcclude(float alpha = 1f)
            : base(true, true)
        {
            Alpha = alpha;
        }

        public LightOcclude(Rectangle bounds, float alpha = 1f)
            : this(alpha)
        {
            this.bounds = new Rectangle?(bounds);
        }

        public override void Update()
        {
            base.Update();
            bool flag = Visible && Entity.Visible;
            Rectangle rectangle = new(Left, Top, Width, Height);
            if (!(lastSize != rectangle) && lastVisible == flag && lastAlpha == (double)Alpha)
            {
                return;
            }

            MakeLightsDirty();
            lastVisible = flag;
            lastSize = rectangle;
            lastAlpha = Alpha;
        }

        public override void Removed(Entity entity)
        {
            MakeLightsDirty();
            base.Removed(entity);
        }

        public override void EntityRemoved(Scene scene)
        {
            MakeLightsDirty();
            base.EntityRemoved(scene);
        }

        private void MakeLightsDirty()
        {
            Rectangle rectangle1 = new(Left, Top, Width, Height);
            foreach (VertexLight component in Entity.Scene.Tracker.GetComponents<VertexLight>())
            {
                if (!component.Dirty)
                {
                    Rectangle rectangle2 = new((int)(component.Center.X - (double)component.EndRadius), (int)(component.Center.Y - (double)component.EndRadius), (int)component.EndRadius * 2, (int)component.EndRadius * 2);
                    if (rectangle1.Intersects(rectangle2) || lastSize.Intersects(rectangle2))
                    {
                        component.Dirty = true;
                    }
                }
            }
        }
    }
}
