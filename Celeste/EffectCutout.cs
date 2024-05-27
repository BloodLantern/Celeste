using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class EffectCutout : Component
    {
        public float Alpha = 1f;
        private Rectangle lastSize;
        private bool lastVisible;
        private float lastAlpha;

        public int Left => (int) Entity.Collider.AbsoluteLeft;

        public int Right => (int) Entity.Collider.AbsoluteRight;

        public int Top => (int) Entity.Collider.AbsoluteTop;

        public int Bottom => (int) Entity.Collider.AbsoluteBottom;

        public Rectangle Bounds => Entity.Collider.Bounds;

        public EffectCutout()
            : base(true, true)
        {
        }

        public override void Update()
        {
            bool flag = Visible && Entity.Visible;
            Rectangle bounds = Bounds;
            if (!(lastSize != bounds) && lastAlpha == (double) Alpha && lastVisible == flag)
                return;
            MakeLightsDirty();
            lastSize = bounds;
            lastAlpha = Alpha;
            lastVisible = flag;
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
            Rectangle bounds = Bounds;
            foreach (VertexLight component in Entity.Scene.Tracker.GetComponents<VertexLight>())
            {
                if (!component.Dirty)
                {
                    Rectangle rectangle = new Rectangle((int) (component.Center.X - (double) component.EndRadius), (int) (component.Center.Y - (double) component.EndRadius), (int) component.EndRadius * 2, (int) component.EndRadius * 2);
                    if (bounds.Intersects(rectangle) || lastSize.Intersects(rectangle))
                        component.Dirty = true;
                }
            }
        }
    }
}
