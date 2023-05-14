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
            entity.Add(pop = Wiggler.Create(0.5f, 4f, f =>
            {
                Sprite sprite = Entity.Get<Sprite>();
                if (sprite == null)
                {
                    return;
                }

                sprite.Scale = new Vector2(Math.Sign(sprite.Scale.X), 1f) * (1 + (0.25f * f));
            }));
        }

        public override void Removed(Entity entity)
        {
            entity.Remove(pop);
            base.Removed(entity);
        }

        public void SetReturnToAnimation(string anim)
        {
            lastAnimation = anim;
        }

        public override void Update()
        {
            Sprite sprite = Entity.Get<Sprite>();
            if (Scene == null || sprite == null)
            {
                return;
            }

            bool flag = false;
            Textbox entity = Scene.Tracker.GetEntity<Textbox>();
            if (Enabled && entity != null)
            {
                if (entity.PortraitName.IsIgnoreCase("badeline"))
                {
                    if (entity.PortraitAnimation.IsIgnoreCase("scoff"))
                    {
                        if (!wasSyncingSprite)
                        {
                            lastAnimation = sprite.CurrentAnimationID;
                        }

                        sprite.Play("laugh");
                        wasSyncingSprite = flag = true;
                    }
                    else if (entity.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
                    {
                        if (!wasSyncingSprite)
                        {
                            pop.Start();
                            lastAnimation = sprite.CurrentAnimationID;
                        }
                        sprite.Play("angry");
                        wasSyncingSprite = flag = true;
                    }
                }
            }

            if (!wasSyncingSprite || flag)
            {
                return;
            }

            wasSyncingSprite = false;
            if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
            {
                lastAnimation = "fallSlow";
            }

            if (sprite.CurrentAnimationID == "angry")
            {
                pop.Start();
            }

            sprite.Play(lastAnimation);
        }
    }
}
