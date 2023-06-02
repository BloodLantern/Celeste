using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class NPC03_Theo_Escaping : NPC
    {
        private bool talked;
        private VertexLight light;
        public NPC03_Theo_Escaping.Grate grate;

        public NPC03_Theo_Escaping(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("theo")));
            this.Sprite.Play("idle");
            this.Sprite.X = -4f;
            this.SetupTheoSpriteSounds();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.Add((Component) (this.light = new VertexLight(this.Center - this.Position, Color.White, 1f, 32, 64)));
            while (!this.CollideCheck<Solid>(this.Position + new Vector2(1f, 0.0f)))
                ++this.X;
            this.grate = new NPC03_Theo_Escaping.Grate(this.Position + new Vector2(this.Width / 2f, -8f));
            this.Scene.Add((Entity) this.grate);
            this.Sprite.Play("goToVent");
        }

        public override void Update()
        {
            base.Update();
            Player first = this.Scene.Entities.FindFirst<Player>();
            if (first != null && !this.talked && (double) first.X > (double) this.X - 100.0)
                this.Talk(first);
            if (this.Sprite.CurrentAnimationID == "pullVent" && this.Sprite.CurrentAnimationFrame > 0)
                this.grate.Sprite.X = 0.0f;
            else
                this.grate.Sprite.X = 1f;
        }

        private void Talk(Player player)
        {
            this.talked = true;
            this.Scene.Add((Entity) new CS03_TheoEscape(this, player));
        }

        public void CrawlUntilOut()
        {
            this.Sprite.Scale.X = 1f;
            this.Sprite.Play("crawl");
            this.Add((Component) new Coroutine(this.CrawlUntilOutRoutine()));
        }

        private IEnumerator CrawlUntilOutRoutine()
        {
            NPC03_Theo_Escaping npC03TheoEscaping = this;
            npC03TheoEscaping.AddTag((int) Tags.Global);
            int target = (npC03TheoEscaping.Scene as Level).Bounds.Right + 280;
            while ((double) npC03TheoEscaping.X != (double) target)
            {
                npC03TheoEscaping.X = Calc.Approach(npC03TheoEscaping.X, (float) target, 20f * Engine.DeltaTime);
                yield return (object) null;
            }
            npC03TheoEscaping.Scene.Remove((Entity) npC03TheoEscaping);
        }

        public class Grate : Entity
        {
            public Monocle.Image Sprite;
            private Vector2 speed;
            private bool falling;
            private float alpha = 1f;

            public Grate(Vector2 position)
                : base(position)
            {
                this.Add((Component) (this.Sprite = new Monocle.Image(GFX.Game["scenery/grate"])));
                this.Sprite.JustifyOrigin(0.5f, 0.0f);
                this.Sprite.Rotation = 1.57079637f;
            }

            public void Fall()
            {
                Audio.Play("event:/char/theo/resort_vent_tumble", this.Position);
                this.falling = true;
                this.speed = new Vector2(-120f, -120f);
                this.Collider = (Collider) new Hitbox(2f, 2f, -2f, -1f);
            }

            public override void Update()
            {
                if (this.falling)
                {
                    this.speed.X = Calc.Approach(this.speed.X, 0.0f, Engine.DeltaTime * 120f);
                    this.speed.Y += 400f * Engine.DeltaTime;
                    this.Position = this.Position + this.speed * Engine.DeltaTime;
                    if (this.CollideCheck<Solid>(this.Position + new Vector2(0.0f, 2f)) && (double) this.speed.Y > 0.0)
                        this.speed.Y = (float) (-(double) this.speed.Y * 0.25);
                    this.alpha -= Engine.DeltaTime;
                    this.Sprite.Rotation += (float) ((double) Engine.DeltaTime * (double) this.speed.Length() * 0.05000000074505806);
                    this.Sprite.Color = Color.White * this.alpha;
                    if ((double) this.alpha <= 0.0)
                        this.RemoveSelf();
                }
                base.Update();
            }
        }
    }
}
