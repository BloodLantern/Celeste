using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Pooled]
    public class CrystalDebris : Actor
    {
        public static ParticleType P_Dust;
        private Image image;
        private float percent;
        private float duration;
        private Vector2 speed;
        private Collision collideH;
        private Collision collideV;
        private Color color;
        private bool bossShatter;

        public CrystalDebris()
            : base(Vector2.Zero)
        {
            Depth = -9990;
            Collider = new Hitbox(2f, 2f, -1f, -1f);
            collideH = OnCollideH;
            collideV = OnCollideV;
            image = new Image(GFX.Game["particles/shard"]);
            image.CenterOrigin();
            Add(image);
        }

        private void Init(Vector2 position, Color color, bool boss)
        {
            Position = position;
            image.Color = this.color = color;
            image.Scale = Vector2.One;
            percent = 0.0f;
            duration = boss ? Calc.Random.Range(0.25f, 1f) : Calc.Random.Range(1f, 2f);
            speed = Calc.AngleToVector(Calc.Random.NextAngle(), boss ? Calc.Random.Range(200, 240) : (float) Calc.Random.Range(60, 160));
            bossShatter = boss;
        }

        public override void Update()
        {
            base.Update();
            if (percent > 1.0)
            {
                RemoveSelf();
            }
            else
            {
                percent += Engine.DeltaTime / duration;
                if (!bossShatter)
                {
                    speed.X = Calc.Approach(speed.X, 0.0f, Engine.DeltaTime * 20f);
                    speed.Y += 200f * Engine.DeltaTime;
                }
                else
                {
                    float num = Calc.Approach(speed.Length(), 0.0f, 300f * Engine.DeltaTime);
                    speed = speed.SafeNormalize() * num;
                }
                if (speed.Length() > 0.0)
                    image.Rotation = speed.Angle();
                image.Scale = Vector2.One * Calc.ClampedMap(percent, 0.8f, 1f, 1f, 0.0f);
                image.Scale.X *= Calc.ClampedMap(speed.Length(), 0.0f, 400f, 1f, 2f);
                image.Scale.Y *= Calc.ClampedMap(speed.Length(), 0.0f, 400f, 1f, 0.2f);
                MoveH(speed.X * Engine.DeltaTime, collideH);
                MoveV(speed.Y * Engine.DeltaTime, collideV);
                if (!Scene.OnInterval(0.05f))
                    return;
                (Scene as Level).ParticlesFG.Emit(CrystalDebris.P_Dust, Position);
            }
        }

        public override void Render()
        {
            Color color = image.Color;
            image.Color = Color.Black;
            image.Position = new Vector2(-1f, 0.0f);
            image.Render();
            image.Position = new Vector2(0.0f, -1f);
            image.Render();
            image.Position = new Vector2(1f, 0.0f);
            image.Render();
            image.Position = new Vector2(0.0f, 1f);
            image.Render();
            image.Position = Vector2.Zero;
            image.Color = color;
            base.Render();
        }

        private void OnCollideH(CollisionData hit) => speed.X *= -0.8f;

        private void OnCollideV(CollisionData hit)
        {
            if (bossShatter)
            {
                RemoveSelf();
            }
            else
            {
                if (Math.Sign(speed.X) != 0)
                    speed.X += Math.Sign(speed.X) * 5;
                else
                    speed.X += Calc.Random.Choose(-1, 1) * 5;
                speed.Y *= -1.2f;
            }
        }

        public static void Burst(Vector2 position, Color color, bool boss, int count = 1)
        {
            for (int index = 0; index < count; ++index)
            {
                CrystalDebris crystalDebris = Engine.Pooler.Create<CrystalDebris>();
                Vector2 position1 = position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                crystalDebris.Init(position1, color, boss);
                Engine.Scene.Add(crystalDebris);
            }
        }
    }
}
