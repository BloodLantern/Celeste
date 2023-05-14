// Decompiled with JetBrains decompiler
// Type: Celeste.TheoPhone
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TheoPhone : Entity
    {
        private readonly VertexLight light;

        public TheoPhone(Vector2 position)
            : base(position)
        {
            Add(light = new VertexLight(Color.LawnGreen, 1f, 8, 16));
            Add(new Monocle.Image(GFX.Game["characters/theo/phone"]).JustifyOrigin(0.5f, 1f));
        }

        public override void Update()
        {
            if (Scene.OnInterval(0.5f))
            {
                light.Visible = !light.Visible;
            }

            base.Update();
        }
    }
}
