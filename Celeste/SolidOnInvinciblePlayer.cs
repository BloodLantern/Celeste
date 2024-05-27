using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class SolidOnInvinciblePlayer : Component
    {
        private bool wasCollidable;
        private bool wasVisible;
        private Outline outline;

        public SolidOnInvinciblePlayer()
            : base(true, false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            Audio.Play("event:/game/general/assist_nonsolid_in", entity.Center);
            wasCollidable = entity.Collidable;
            wasVisible = entity.Visible;
            entity.Collidable = false;
            entity.Visible = false;
            if (entity.Scene == null)
                return;
            entity.Scene.Add(outline = new Outline(this));
        }

        public override void Update()
        {
            base.Update();
            Entity.Collidable = true;
            if (!Entity.CollideCheck<Player>() && !Entity.CollideCheck<TheoCrystal>())
                RemoveSelf();
            else
                Entity.Collidable = false;
        }

        public override void Removed(Entity entity)
        {
            Audio.Play("event:/game/general/assist_nonsolid_out", entity.Center);
            entity.Collidable = wasCollidable;
            entity.Visible = wasVisible;
            if (outline != null)
                outline.RemoveSelf();
            base.Removed(entity);
        }

        private class Outline : Entity
        {
            public SolidOnInvinciblePlayer Parent;

            public Outline(SolidOnInvinciblePlayer parent)
            {
                Parent = parent;
                Depth = -10;
            }

            public override void Render()
            {
                if (Parent == null || Parent.Entity == null)
                    return;
                Entity entity = Parent.Entity;
                int left = (int) entity.Left;
                int right = (int) entity.Right;
                int top = (int) entity.Top;
                int bottom = (int) entity.Bottom;
                Draw.Rect(left + 4, top + 4, entity.Width - 8f, entity.Height - 8f, Color.White * 0.25f);
                for (float x1 = left; x1 < (double) (right - 3); x1 += 3f)
                {
                    Draw.Line(x1, top, x1 + 2f, top, Color.White);
                    Draw.Line(x1, bottom - 1, x1 + 2f, bottom - 1, Color.White);
                }
                for (float y1 = top; y1 < (double) (bottom - 3); y1 += 3f)
                {
                    Draw.Line(left + 1, y1, left + 1, y1 + 2f, Color.White);
                    Draw.Line(right, y1, right, y1 + 2f, Color.White);
                }
                Draw.Rect(left + 1, top, 1f, 2f, Color.White);
                Draw.Rect(right - 2, top, 2f, 2f, Color.White);
                Draw.Rect(left, bottom - 2, 2f, 2f, Color.White);
                Draw.Rect(right - 2, bottom - 2, 2f, 2f, Color.White);
            }
        }
    }
}
