﻿using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Pooled]
    public class Debris : Actor
    {
        private Image image;
        private float lifeTimer;
        private float alpha;
        private Vector2 speed;
        private Collision collideH;
        private Collision collideV;
        private int rotateSign;
        private float fadeLerp;
        private bool playSound = true;
        private bool dreaming;
        private SineWave dreamSine;
        private bool hasHitGround;
        private char tileset;

        public Debris()
            : base(Vector2.Zero)
        {
            Collider = new Hitbox(4f, 4f, -2f, -2f);
            Tag = (int) Tags.Persistent;
            Depth = 2000;
            Add(image = new Image(null));
            collideH = OnCollideH;
            collideV = OnCollideV;
            Add(dreamSine = new SineWave(0.6f));
            dreamSine.Randomize();
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
            rotateSign = Calc.Random.Choose(1, -1);
            image.Texture = !GFX.Game.Has("debris/" + tileset) ? GFX.Game["debris/1"] : GFX.Game["debris/" + tileset];
            image.CenterOrigin();
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

        private void OnCollideH(CollisionData data) => speed.X *= -0.8f;

        private void OnCollideV(CollisionData data)
        {
            if (speed.Y > 0.0)
                hasHitGround = true;
            speed.Y *= -0.6f;
            if (speed.Y < 0.0 && speed.Y > -50.0)
                speed.Y = 0.0f;
            if (speed.Y == 0.0 && hasHitGround)
                return;
            ImpactSfx(Math.Abs(speed.Y));
        }

        private void ImpactSfx(float spd)
        {
            if (!playSound)
                return;
            string path = "event:/game/general/debris_dirt";
            if (tileset == '4' || tileset == '5' || tileset == '6' || tileset == '7' || tileset == 'a' || tileset == 'c' || tileset == 'd' || tileset == 'e' || tileset == 'f' || tileset == 'd' || tileset == 'g')
                path = "event:/game/general/debris_stone";
            else if (tileset == '9')
                path = "event:/game/general/debris_wood";
            Audio.Play(path, Position, "debris_velocity", Calc.ClampedMap(spd, 0.0f, 150f));
        }

        public override void Update()
        {
            base.Update();
            image.Rotation += Math.Abs(speed.X) * rotateSign * Engine.DeltaTime;
            if (fadeLerp < 1.0)
                fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
            MoveH(speed.X * Engine.DeltaTime, collideH);
            MoveV(speed.Y * Engine.DeltaTime, collideV);
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
                    speed.Y = Calc.Approach(speed.Y, 100f, 400f * Engine.DeltaTime);
            }
            if (lifeTimer > 0.0)
                lifeTimer -= Engine.DeltaTime;
            else if (alpha > 0.0)
            {
                alpha -= 4f * Engine.DeltaTime;
                if (alpha <= 0.0)
                    RemoveSelf();
            }
            image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
        }
    }
}
