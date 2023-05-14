// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Oshiro_Hallway2
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC03_Oshiro_Hallway2 : NPC
    {
        private bool talked;

        public NPC03_Oshiro_Hallway2(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(-1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("oshiro_resort_talked_3"))
            {
                RemoveSelf();
            }
            else
            {
                Session.LightingAlphaAdd = 0.15f;
            }
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (talked || entity == null || (double)entity.X <= (double)X - 60.0)
            {
                return;
            }

            Scene.Add(new CS03_OshiroHallway2(entity, this));
            talked = true;
        }
    }
}
