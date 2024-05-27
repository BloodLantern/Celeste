using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class NPC00_Granny : NPC
    {
        public Hahaha Hahaha;
        public GrannyLaughSfx LaughSfx;
        private bool talking;

        public NPC00_Granny(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Play("idle");
            Add(LaughSfx = new GrannyLaughSfx(Sprite));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((scene as Level).Session.GetFlag("granny"))
                Sprite.Play("laugh");
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
        }

        public override void Update()
        {
            Player entity = Level.Tracker.GetEntity<Player>();
            if (entity != null && !Session.GetFlag("granny") && !talking)
            {
                int num = Level.Bounds.Left + 96;
                if (entity.OnGround() && entity.X >= (double) num && entity.X <= X + 16.0 && Math.Abs(entity.Y - Y) < 4.0 && entity.Facing == (Facings) Math.Sign(X - entity.X))
                {
                    talking = true;
                    Scene.Add(new CS00_Granny(this, entity));
                }
            }
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }
    }
}
