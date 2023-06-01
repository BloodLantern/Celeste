// Decompiled with JetBrains decompiler
// Type: Celeste.BadelineAutoAnimator
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class BadelineAutoAnimator : Component
    {
        public bool Enabled = true;
        private string lastAnimation;
        private bool wasSyncingSprite;
        private Wiggler pop;

        public BadelineAutoAnimator()
            : base(true, false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add((Component) (this.pop = Wiggler.Create(0.5f, 4f, (Action<float>) (f =>
            {
                Sprite sprite = this.Entity.Get<Sprite>();
                if (sprite == null)
                    return;
                sprite.Scale = new Vector2((float) Math.Sign(sprite.Scale.X), 1f) * (float) (1.0 + 0.25 * (double) f);
            }))));
        }

        public override void Removed(Entity entity)
        {
            entity.Remove((Component) this.pop);
            base.Removed(entity);
        }

        public void SetReturnToAnimation(string anim) => this.lastAnimation = anim;

        public override void Update()
        {
            Sprite sprite = this.Entity.Get<Sprite>();
            if (this.Scene == null || sprite == null)
                return;
            bool flag = false;
            Textbox entity = this.Scene.Tracker.GetEntity<Textbox>();
            if (this.Enabled && entity != null)
            {
                if (entity.PortraitName.IsIgnoreCase("badeline"))
                {
                    if (entity.PortraitAnimation.IsIgnoreCase("scoff"))
                    {
                        if (!this.wasSyncingSprite)
                            this.lastAnimation = sprite.CurrentAnimationID;
                        sprite.Play("laugh");
                        this.wasSyncingSprite = flag = true;
                    }
                    else if (entity.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
                    {
                        if (!this.wasSyncingSprite)
                        {
                            this.pop.Start();
                            this.lastAnimation = sprite.CurrentAnimationID;
                        }
                        sprite.Play("angry");
                        this.wasSyncingSprite = flag = true;
                    }
                }
            }
            if (!this.wasSyncingSprite || flag)
                return;
            this.wasSyncingSprite = false;
            if (string.IsNullOrEmpty(this.lastAnimation) || this.lastAnimation == "spin")
                this.lastAnimation = "fallSlow";
            if (sprite.CurrentAnimationID == "angry")
                this.pop.Start();
            sprite.Play(this.lastAnimation);
        }
    }
}
