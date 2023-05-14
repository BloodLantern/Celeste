// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Oshiro_Rooftop
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC03_Oshiro_Rooftop : NPC
    {
        public NPC03_Oshiro_Rooftop(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(1));
            (Sprite as OshiroSprite).AllowTurnInvisible = false;
            MoveAnim = "move";
            IdleAnim = "idle";
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("oshiro_resort_roof"))
            {
                RemoveSelf();
            }
            else
            {
                Visible = false;
                Scene.Add(new CS03_OshiroRooftop(this));
            }
        }
    }
}
