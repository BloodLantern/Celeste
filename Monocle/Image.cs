// Decompiled with JetBrains decompiler
// Type: Monocle.Image
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Monocle
{
    public class Image : GraphicsComponent
    {
        public MTexture Texture;
        public bool TEST;

        public Image(MTexture texture)
            : base(false)
        {
            Texture = texture;
        }

        internal Image(MTexture texture, bool active)
            : base(active)
        {
            Texture = texture;
        }

        public override void Render()
        {
            if (Texture == null)
            {
                return;
            }

            Texture.Draw(RenderPosition, Origin, Color, Scale, Rotation, Effects);
        }

        public virtual float Width => Texture.Width;

        public virtual float Height => Texture.Height;

        public Image SetOrigin(float x, float y)
        {
            Origin.X = x;
            Origin.Y = y;
            return this;
        }

        public Image CenterOrigin()
        {
            Origin.X = Width / 2f;
            Origin.Y = Height / 2f;
            return this;
        }

        public Image JustifyOrigin(Vector2 at)
        {
            Origin.X = Width * at.X;
            Origin.Y = Height * at.Y;
            return this;
        }

        public Image JustifyOrigin(float x, float y)
        {
            Origin.X = Width * x;
            Origin.Y = Height * y;
            return this;
        }

        public Image SetColor(Color color)
        {
            Color = color;
            return this;
        }
    }
}
