using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class NPC03_Theo_Escaping : NPC
    {
        private bool talked;
        private VertexLight light;
        public Grate grate;

        public NPC03_Theo_Escaping(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Play("idle");
            Sprite.X = -4f;
            SetupTheoSpriteSounds();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(light = new VertexLight(Center - Position, Color.White, 1f, 32, 64));
            while (!CollideCheck<Solid>(Position + new Vector2(1f, 0.0f)))
                ++X;
            grate = new Grate(Position + new Vector2(Width / 2f, -8f));
            Scene.Add(grate);
            Sprite.Play("goToVent");
        }

        public override void Update()
        {
            base.Update();
            Player first = Scene.Entities.FindFirst<Player>();
            if (first != null && !talked && first.X > X - 100.0)
                Talk(first);
            if (Sprite.CurrentAnimationID == "pullVent" && Sprite.CurrentAnimationFrame > 0)
                grate.Sprite.X = 0.0f;
            else
                grate.Sprite.X = 1f;
        }

        private void Talk(Player player)
        {
            talked = true;
            Scene.Add(new CS03_TheoEscape(this, player));
        }

        public void CrawlUntilOut()
        {
            Sprite.Scale.X = 1f;
            Sprite.Play("crawl");
            Add(new Coroutine(CrawlUntilOutRoutine()));
        }

        private IEnumerator CrawlUntilOutRoutine()
        {
            NPC03_Theo_Escaping npC03TheoEscaping = this;
            npC03TheoEscaping.AddTag((int) Tags.Global);
            int target = (npC03TheoEscaping.Scene as Level).Bounds.Right + 280;
            while (npC03TheoEscaping.X != (double) target)
            {
                npC03TheoEscaping.X = Calc.Approach(npC03TheoEscaping.X, target, 20f * Engine.DeltaTime);
                yield return null;
            }
            npC03TheoEscaping.Scene.Remove(npC03TheoEscaping);
        }

        public class Grate : Entity
        {
            public Image Sprite;
            private Vector2 speed;
            private bool falling;
            private float alpha = 1f;

            public Grate(Vector2 position)
                : base(position)
            {
                Add(Sprite = new Image(GFX.Game["scenery/grate"]));
                Sprite.JustifyOrigin(0.5f, 0.0f);
                Sprite.Rotation = 1.57079637f;
            }

            public void Fall()
            {
                Audio.Play("event:/char/theo/resort_vent_tumble", Position);
                falling = true;
                speed = new Vector2(-120f, -120f);
                Collider = new Hitbox(2f, 2f, -2f, -1f);
            }

            public override void Update()
            {
                if (falling)
                {
                    speed.X = Calc.Approach(speed.X, 0.0f, Engine.DeltaTime * 120f);
                    speed.Y += 400f * Engine.DeltaTime;
                    Position += speed * Engine.DeltaTime;
                    if (CollideCheck<Solid>(Position + new Vector2(0.0f, 2f)) && speed.Y > 0.0)
                        speed.Y = (float) (-(double) speed.Y * 0.25);
                    alpha -= Engine.DeltaTime;
                    Sprite.Rotation += (float) (Engine.DeltaTime * (double) speed.Length() * 0.05000000074505806);
                    Sprite.Color = Color.White * alpha;
                    if (alpha <= 0.0)
                        RemoveSelf();
                }
                base.Update();
            }
        }
    }
}
