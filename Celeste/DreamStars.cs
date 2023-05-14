// Decompiled with JetBrains decompiler
// Type: Celeste.DreamStars
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class DreamStars : Backdrop
    {
        private readonly DreamStars.Stars[] stars = new DreamStars.Stars[50];
        private Vector2 angle = Vector2.Normalize(new Vector2(-2f, -7f));
        private Vector2 lastCamera = Vector2.Zero;

        public DreamStars()
        {
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index].Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f));
                stars[index].Speed = 24f + Calc.Random.NextFloat(24f);
                stars[index].Size = 2f + Calc.Random.NextFloat(6f);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Vector2 position = (scene as Level).Camera.Position;
            Vector2 vector2 = position - lastCamera;
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index].Position += (angle * stars[index].Speed * Engine.DeltaTime) - (vector2 * 0.5f);
            }

            lastCamera = position;
        }

        public override void Render(Scene scene)
        {
            for (int index = 0; index < stars.Length; ++index)
            {
                Draw.HollowRect(new Vector2(mod(stars[index].Position.X, 320f), mod(stars[index].Position.Y, 180f)), stars[index].Size, stars[index].Size, Color.Teal);
            }
        }

        private float mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Stars
        {
            public Vector2 Position;
            public float Speed;
            public float Size;
        }
    }
}
