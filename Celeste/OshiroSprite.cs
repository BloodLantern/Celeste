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
            Scale.X = facing;
            GFX.SpriteBank.CreateOn(this, "oshiro");
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(wiggler = Wiggler.Create(0.3f, 2f, f =>
            {
                Scale.X = Math.Sign(Scale.X) * (float) (1.0 + f * 0.20000000298023224);
                Scale.Y = (float) (1.0 - f * 0.20000000298023224);
            }));
        }

        public override void Update()
        {
            base.Update();
            if (AllowSpriteChanges)
            {
                Textbox entity = Scene.Tracker.GetEntity<Textbox>();
                if (entity != null)
                {
                    if (entity.PortraitName.Equals("oshiro", StringComparison.OrdinalIgnoreCase) && entity.PortraitAnimation.StartsWith("side", StringComparison.OrdinalIgnoreCase))
                    {
                        if (CurrentAnimationID.Equals("idle"))
                            Pop("side", true);
                    }
                    else if (CurrentAnimationID.Equals("side"))
                        Pop("idle", true);
                }
            }
            if (!AllowTurnInvisible || !Visible)
                return;
            Level scene = Scene as Level;
            double x1 = RenderPosition.X;
            Rectangle bounds = scene.Bounds;
            double num1 = bounds.Left - 8;
            int num2;
            if (x1 > num1)
            {
                double y1 = RenderPosition.Y;
                bounds = scene.Bounds;
                double num3 = bounds.Top - 8;
                if (y1 > num3)
                {
                    double x2 = RenderPosition.X;
                    bounds = scene.Bounds;
                    double num4 = bounds.Right + 8;
                    if (x2 < num4)
                    {
                        double y2 = RenderPosition.Y;
                        bounds = scene.Bounds;
                        double num5 = bounds.Bottom + 16;
                        num2 = y2 < num5 ? 1 : 0;
                        goto label_13;
                    }
                }
            }
            num2 = 0;
label_13:
            Visible = num2 != 0;
        }

        public void Wiggle() => wiggler.Start();

        public void Pop(string name, bool flip)
        {
            if (CurrentAnimationID.Equals(name))
                return;
            Play(name);
            if (flip)
            {
                Scale.X = -Scale.X;
                if (Scale.X < 0.0)
                    Audio.Play("event:/char/oshiro/chat_turn_left", Entity.Position);
                else
                    Audio.Play("event:/char/oshiro/chat_turn_right", Entity.Position);
            }
            wiggler.Start();
        }
    }
}
