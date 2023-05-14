// Decompiled with JetBrains decompiler
// Type: Celeste.NPC05_Theo_Mirror
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC05_Theo_Mirror : NPC
    {
        private bool started;

        public NPC05_Theo_Mirror(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            IdleAnim = "idle";
            MoveAnim = "walk";
            Visible = false;
            Add(new MirrorReflection()
            {
                IgnoreEntityVisible = true
            });
            Sprite.Scale.X = 1f;
            Maxspeed = 48f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Session.GetFlag("theoInMirror"))
            {
                return;
            }

            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (started || entity == null || (double)entity.X <= (double)X - 64.0)
            {
                return;
            }

            started = true;
            Scene.Add(new CS05_TheoInMirror(this, entity));
        }
    }
}
