using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TempleMirror : Entity
    {
        private readonly Color color = Calc.HexToColor("05070e");
        private readonly Vector2 size;
        private MTexture[,] frame = new MTexture[3, 3];
        private MirrorSurface surface;

        public TempleMirror(EntityData e, Vector2 offset)
            : base(e.Position + offset)
        {
            size = new Vector2(e.Width, e.Height);
            Depth = 9500;
            Collider = new Hitbox(e.Width, e.Height);
            Add(surface = new MirrorSurface());
            surface.ReflectionOffset = new Vector2(e.Float("reflectX"), e.Float("reflectY"));
            surface.OnRender = () => Draw.Rect(X + 2f, Y + 2f, size.X - 4f, size.Y - 4f, surface.ReflectionColor);
            MTexture mtexture = GFX.Game["scenery/templemirror"];
            for (int index1 = 0; index1 < mtexture.Width / 8; ++index1)
            {
                for (int index2 = 0; index2 < mtexture.Height / 8; ++index2)
                    frame[index1, index2] = mtexture.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new Frame(this));
        }

        public override void Render() => Draw.Rect(X + 3f, Y + 3f, size.X - 6f, size.Y - 6f, color);

        private class Frame : Entity
        {
            private TempleMirror mirror;

            public Frame(TempleMirror mirror)
            {
                this.mirror = mirror;
                Depth = 8995;
            }

            public override void Render()
            {
                Position = mirror.Position;
                MTexture[,] frame = mirror.frame;
                Vector2 size = mirror.size;
                frame[0, 0].Draw(Position + new Vector2(0.0f, 0.0f));
                frame[2, 0].Draw(Position + new Vector2(size.X - 8f, 0.0f));
                frame[0, 2].Draw(Position + new Vector2(0.0f, size.Y - 8f));
                frame[2, 2].Draw(Position + new Vector2(size.X - 8f, size.Y - 8f));
                for (int index = 1; index < size.X / 8.0 - 1.0; ++index)
                {
                    frame[1, 0].Draw(Position + new Vector2(index * 8, 0.0f));
                    frame[1, 2].Draw(Position + new Vector2(index * 8, size.Y - 8f));
                }
                for (int index = 1; index < size.Y / 8.0 - 1.0; ++index)
                {
                    frame[0, 1].Draw(Position + new Vector2(0.0f, index * 8));
                    frame[2, 1].Draw(Position + new Vector2(size.X - 8f, index * 8));
                }
            }
        }
    }
}
