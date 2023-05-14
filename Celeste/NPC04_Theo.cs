// Decompiled with JetBrains decompiler
// Type: Celeste.NPC04_Theo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC04_Theo : NPC
    {
        private bool started;

        public NPC04_Theo(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            IdleAnim = "idle";
            MoveAnim = "walk";
            Visible = false;
            Maxspeed = 48f;
            SetupTheoSpriteSounds();
        }

        public override void Update()
        {
            base.Update();
            if (started)
            {
                return;
            }

            Gondola first = Scene.Entities.FindFirst<Gondola>();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (first == null || entity == null || (double)entity.X <= (double)first.Left - 16.0)
            {
                return;
            }

            started = true;
            Scene.Add(new CS04_Gondola(this, first, entity));
        }
    }
}
