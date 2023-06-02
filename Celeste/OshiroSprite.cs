using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class OshiroSprite : Sprite
    {
        public bool AllowSpriteChanges = true;
        public bool AllowTurnInvisible = true;
        private Wiggler wiggler;

        public OshiroSprite(int facing)
        {
            this.Scale.X = (float) facing;
            GFX.SpriteBank.CreateOn((Sprite) this, "oshiro");
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add((Component) (this.wiggler = Wiggler.Create(0.3f, 2f, (Action<float>) (f =>
            {
                this.Scale.X = (float) Math.Sign(this.Scale.X) * (float) (1.0 + (double) f * 0.20000000298023224);
                this.Scale.Y = (float) (1.0 - (double) f * 0.20000000298023224);
            }))));
        }

        public override void Update()
        {
            base.Update();
            if (this.AllowSpriteChanges)
            {
                Textbox entity = this.Scene.Tracker.GetEntity<Textbox>();
                if (entity != null)
                {
                    if (entity.PortraitName.Equals("oshiro", StringComparison.OrdinalIgnoreCase) && entity.PortraitAnimation.StartsWith("side", StringComparison.OrdinalIgnoreCase))
                    {
                        if (this.CurrentAnimationID.Equals("idle"))
                            this.Pop("side", true);
                    }
                    else if (this.CurrentAnimationID.Equals("side"))
                        this.Pop("idle", true);
                }
            }
            if (!this.AllowTurnInvisible || !this.Visible)
                return;
            Level scene = this.Scene as Level;
            double x1 = (double) this.RenderPosition.X;
            Rectangle bounds = scene.Bounds;
            double num1 = (double) (bounds.Left - 8);
            int num2;
            if (x1 > num1)
            {
                double y1 = (double) this.RenderPosition.Y;
                bounds = scene.Bounds;
                double num3 = (double) (bounds.Top - 8);
                if (y1 > num3)
                {
                    double x2 = (double) this.RenderPosition.X;
                    bounds = scene.Bounds;
                    double num4 = (double) (bounds.Right + 8);
                    if (x2 < num4)
                    {
                        double y2 = (double) this.RenderPosition.Y;
                        bounds = scene.Bounds;
                        double num5 = (double) (bounds.Bottom + 16);
                        num2 = y2 < num5 ? 1 : 0;
                        goto label_13;
                    }
                }
            }
            num2 = 0;
label_13:
            this.Visible = num2 != 0;
        }

        public void Wiggle() => this.wiggler.Start();

        public void Pop(string name, bool flip)
        {
            if (this.CurrentAnimationID.Equals(name))
                return;
            this.Play(name);
            if (flip)
            {
                this.Scale.X = -this.Scale.X;
                if ((double) this.Scale.X < 0.0)
                    Audio.Play("event:/char/oshiro/chat_turn_left", this.Entity.Position);
                else
                    Audio.Play("event:/char/oshiro/chat_turn_right", this.Entity.Position);
            }
            this.wiggler.Start();
        }
    }
}
