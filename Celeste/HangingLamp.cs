// Decompiled with JetBrains decompiler
// Type: Celeste.HangingLamp
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class HangingLamp : Entity
    {
        public readonly int Length;
        private readonly List<Monocle.Image> images = new();
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private float speed;
        private float rotation;
        private float soundDelay;
        private readonly SoundSource sfx;

        public HangingLamp(Vector2 position, int length)
        {
            Position = position + (Vector2.UnitX * 4f);
            Length = Math.Max(16, length);
            Depth = 2000;
            MTexture mtexture = GFX.Game["objects/hanginglamp"];
            for (int index = 0; index < Length - 8; index += 8)
            {
                Monocle.Image image;
                Add(image = new Monocle.Image(mtexture.GetSubtexture(0, 8, 8, 8)));
                image.Origin.X = 4f;
                image.Origin.Y = -index;
                images.Add(image);
            }
            Monocle.Image image1;
            Add(image1 = new Monocle.Image(mtexture.GetSubtexture(0, 0, 8, 8)));
            image1.Origin.X = 4f;
            Monocle.Image image2;
            Add(image2 = new Monocle.Image(mtexture.GetSubtexture(0, 16, 8, 8)));
            image2.Origin.X = 4f;
            image2.Origin.Y = -(Length - 8);
            images.Add(image2);
            Add(bloom = new BloomPoint(Vector2.UnitY * (Length - 4), 1f, 48f));
            Add(light = new VertexLight(Vector2.UnitY * (Length - 4), Color.White, 1f, 24, 48));
            Add(sfx = new SoundSource());
            Collider = new Hitbox(8f, Length, -4f);
        }

        public HangingLamp(EntityData e, Vector2 position)
            : this(position, Math.Max(16, e.Height))
        {
        }

        public override void Update()
        {
            base.Update();
            soundDelay -= Engine.DeltaTime;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && Collider.Collide(entity))
            {
                speed = (float)(-(double)entity.Speed.X * 0.004999999888241291 * (((double)entity.Y - (double)Y) / Length));
                if ((double)Math.Abs(speed) < 0.10000000149011612)
                {
                    speed = 0.0f;
                }
                else if (soundDelay <= 0.0)
                {
                    _ = sfx.Play("event:/game/02_old_site/lantern_hit");
                    soundDelay = 0.25f;
                }
            }
            float num = Math.Sign(this.rotation) == Math.Sign(speed) ? 8f : 6f;
            if ((double)Math.Abs(this.rotation) < 0.5)
            {
                num *= 0.5f;
            }

            if ((double)Math.Abs(this.rotation) < 0.25)
            {
                num *= 0.5f;
            }

            float rotation = this.rotation;
            speed += -Math.Sign(this.rotation) * num * Engine.DeltaTime;
            this.rotation += speed * Engine.DeltaTime;
            this.rotation = Calc.Clamp(this.rotation, -0.4f, 0.4f);
            if ((double)Math.Abs(this.rotation) < 0.019999999552965164 && (double)Math.Abs(speed) < 0.20000000298023224)
            {
                this.rotation = speed = 0.0f;
            }
            else if (Math.Sign(this.rotation) != Math.Sign(rotation) && soundDelay <= 0.0 && (double)Math.Abs(speed) > 0.5)
            {
                _ = sfx.Play("event:/game/02_old_site/lantern_hit");
                soundDelay = 0.25f;
            }
            foreach (GraphicsComponent image in images)
            {
                image.Rotation = this.rotation;
            }

            Vector2 vector = Calc.AngleToVector(this.rotation + 1.57079637f, Length - 4f);
            bloom.Position = light.Position = vector;
            sfx.Position = vector;
        }

        public override void Render()
        {
            foreach (Component component in Components)
            {
                if (component is Monocle.Image image)
                {
                    image.DrawOutline();
                }
            }
            base.Render();
        }
    }
}
