// Decompiled with JetBrains decompiler
// Type: Celeste.Debris
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Pooled]
    public class Debris : Actor
    {
        private readonly Monocle.Image image;
        private float lifeTimer;
        private float alpha;
        private Vector2 speed;
        private readonly Collision collideH;
        private readonly Collision collideV;
        private int rotateSign;
        private float fadeLerp;
        private bool playSound = true;
        private bool dreaming;
        private readonly SineWave dreamSine;
        private bool hasHitGround;
        private char tileset;

        public Debris()
            : base(Vector2.Zero)
        {
            Collider = new Hitbox(4f, 4f, -2f, -2f);
            Tag = (int)Tags.Persistent;
            Depth = 2000;
            Add(image = new Monocle.Image(null));
            collideH = new Collision(OnCollideH);
            collideV = new Collision(OnCollideV);
            Add(dreamSine = new SineWave(0.6f));
            _ = dreamSine.Randomize();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            dreaming = SceneAs<Level>().Session.Dreaming;
        }

        public Debris Init(Vector2 pos, char tileset, bool playSound = true)
        {
            Position = pos;
            this.tileset = tileset;
            this.playSound = playSound;
            lifeTimer = Calc.Random.Range(0.6f, 2.6f);
            alpha = 1f;
            hasHitGround = false;
            speed = Vector2.Zero;
            fadeLerp = 0.0f;
            rotateSign = Calc.Random.Choose<int>(1, -1);
            image.Texture = !GFX.Game.Has("debris/" + tileset.ToString()) ? GFX.Game["debris/1"] : GFX.Game["debris/" + tileset.ToString()];
            _ = image.CenterOrigin();
            image.Color = Color.White * alpha;
            image.Rotation = Calc.Random.NextAngle();
            image.Scale.X = Calc.Random.Range(0.5f, 1f);
            image.Scale.Y = Calc.Random.Range(0.5f, 1f);
            image.FlipX = Calc.Random.Chance(0.5f);
            image.FlipY = Calc.Random.Chance(0.5f);
            return this;
        }

        public Debris BlastFrom(Vector2 from)
        {
            float length = Calc.Random.Range(30, 40);
            speed = (Position - from).SafeNormalize(length);
            speed = speed.Rotate(Calc.Random.Range(-0.2617994f, 0.2617994f));
            return this;
        }

        private void OnCollideH(CollisionData data)
        {
            speed.X *= -0.8f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (speed.Y > 0.0)
            {
                hasHitGround = true;
            }

            speed.Y *= -0.6f;
            if ((double)speed.Y is < 0.0 and > (-50.0))
            {
                speed.Y = 0.0f;
            }

            if (speed.Y == 0.0 && hasHitGround)
            {
                return;
            }

            ImpactSfx(Math.Abs(speed.Y));
        }

        private void ImpactSfx(float spd)
        {
            if (!playSound)
            {
                return;
            }

            string path = "event:/game/general/debris_dirt";
            if (tileset is '4' or '5' or '6' or '7' or 'a' or 'c' or 'd' or 'e' or 'f' or 'd' or 'g')
            {
                path = "event:/game/general/debris_stone";
            }
            else if (tileset == '9')
            {
                path = "event:/game/general/debris_wood";
            }

            _ = Audio.Play(path, Position, "debris_velocity", Calc.ClampedMap(spd, 0.0f, 150f));
        }

        public override void Update()
        {
            base.Update();
            image.Rotation += Math.Abs(speed.X) * rotateSign * Engine.DeltaTime;
            if (fadeLerp < 1.0)
            {
                fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
            }

            _ = MoveH(speed.X * Engine.DeltaTime, collideH);
            _ = MoveV(speed.Y * Engine.DeltaTime, collideV);
            if (dreaming)
            {
                speed.X = Calc.Approach(speed.X, 0.0f, 50f * Engine.DeltaTime);
                speed.Y = Calc.Approach(speed.Y, 6f * dreamSine.Value, 100f * Engine.DeltaTime);
            }
            else
            {
                bool flag = OnGround();
                speed.X = Calc.Approach(speed.X, 0.0f, (flag ? 50f : 20f) * Engine.DeltaTime);
                if (!flag)
                {
                    speed.Y = Calc.Approach(speed.Y, 100f, 400f * Engine.DeltaTime);
                }
            }
            if (lifeTimer > 0.0)
            {
                lifeTimer -= Engine.DeltaTime;
            }
            else if (alpha > 0.0)
            {
                alpha -= 4f * Engine.DeltaTime;
                if (alpha <= 0.0)
                {
                    RemoveSelf();
                }
            }
            image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
        }
    }
}
