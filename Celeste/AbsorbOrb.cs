// Decompiled with JetBrains decompiler
// Type: Celeste.AbsorbOrb
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class AbsorbOrb : Entity
    {
        public Entity AbsorbInto;
        public Vector2? AbsorbTarget;
        private SimpleCurve curve;
        private float duration;
        private float percent;
        private float consumeDelay;
        private float burstSpeed;
        private Vector2 burstDirection;
        //private Vector2 burstScale;
        private float alpha = 1f;
        private readonly Monocle.Image sprite;
        //private readonly BloomPoint bloom;

        public AbsorbOrb(Vector2 position, Entity into = null, Vector2? absorbTarget = null)
        {
            AbsorbInto = into;
            AbsorbTarget = absorbTarget;
            Position = position;
            Tag = (int) Tags.FrozenUpdate;
            Depth = -2000000;
            consumeDelay = (float) (0.699999988079071 + Calc.Random.NextFloat() * 0.30000001192092896);
            burstSpeed = (float) (80.0 + Calc.Random.NextFloat() * 40.0);
            burstDirection = Calc.AngleToVector(Calc.Random.NextFloat() * 6.28318548f, 1f);
            Add(sprite = new Monocle.Image(GFX.Game["collectables/heartGem/orb"]));
            sprite.CenterOrigin();
            Add(/*bloom = */new BloomPoint(1f, 16f));
        }

        public override void Update()
        {
            base.Update();
            Vector2 vector2_1 = Vector2.Zero;
            bool flag = false;
            if (AbsorbInto != null)
            {
                vector2_1 = AbsorbInto.Center;
                flag = AbsorbInto.Scene == null || AbsorbInto is Player && (AbsorbInto as Player).Dead;
            }
            else if (AbsorbTarget.HasValue)
            {
                vector2_1 = AbsorbTarget.Value;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                    vector2_1 = entity.Center;
                flag = entity == null || entity.Scene == null || entity.Dead;
            }
            if (flag)
            {
                Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                burstSpeed = Calc.Approach(burstSpeed, 800f, Engine.RawDeltaTime * 200f);
                sprite.Rotation = burstDirection.Angle();
                sprite.Scale = new Vector2(Math.Min(2f, (float) (0.5 + burstSpeed * 0.019999999552965164)), Math.Max(0.05f, (float) (0.5 - burstSpeed * 0.0040000001899898052)));
                sprite.Color = Color.White * (alpha = Calc.Approach(alpha, 0.0f, Engine.DeltaTime));
            }
            else if (consumeDelay > 0.0)
            {
                Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                burstSpeed = Calc.Approach(burstSpeed, 0.0f, Engine.RawDeltaTime * 120f);
                sprite.Rotation = burstDirection.Angle();
                sprite.Scale = new Vector2(Math.Min(2f, (float) (0.5 + burstSpeed * 0.019999999552965164)), Math.Max(0.05f, (float) (0.5 - burstSpeed * 0.0040000001899898052)));
                consumeDelay -= Engine.RawDeltaTime;
                if (consumeDelay > 0.0)
                    return;
                Vector2 position = Position;
                Vector2 end = vector2_1;
                Vector2 vector2_2 = (position + end) / 2f;
                Vector2 vector2_3 = (end - position).SafeNormalize().Perpendicular() * (position - end).Length() * (float) (0.05000000074505806 + Calc.Random.NextFloat() * 0.44999998807907104);
                float num1 = end.X - position.X;
                float num2 = end.Y - position.Y;
                if (Math.Abs(num1) > Math.Abs(num2) && Math.Sign(vector2_3.X) != Math.Sign(num1) || Math.Abs(num2) > Math.Abs(num2) && Math.Sign(vector2_3.Y) != Math.Sign(num2))
                    vector2_3 *= -1f;
                curve = new SimpleCurve(position, end, vector2_2 + vector2_3);
                duration = 0.3f + Calc.Random.NextFloat(0.25f);
                //burstScale = sprite.Scale;
            }
            else
            {
                curve.End = vector2_1;
                if (this.percent >= 1.0)
                    RemoveSelf();
                this.percent = Calc.Approach(this.percent, 1f, Engine.RawDeltaTime / duration);
                float percent = Ease.CubeIn(this.percent);
                Position = curve.GetPoint(this.percent);
                float num = Calc.YoYo(this.percent) * curve.GetLengthParametric(10);
                sprite.Scale = new Vector2(Math.Min(2f, (float) (0.5 + num * 0.019999999552965164)), Math.Max(0.05f, (float) (0.5 - num * 0.0040000001899898052)));
                sprite.Color = Color.White * (1f - percent);
                sprite.Rotation = Calc.Angle(Position, curve.GetPoint(Ease.CubeIn(percent + 0.01f)));
            }
        }
    }
}
