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
        private Vector2 burstScale;
        private float alpha = 1f;
        private Image sprite;
        private BloomPoint bloom;

        public AbsorbOrb(Vector2 position, Entity into = null, Vector2? absorbTarget = null)
        {
            AbsorbInto = into;
            AbsorbTarget = absorbTarget;
            Position = position;
            Tag = (int) Tags.FrozenUpdate;
            Depth = -2000000;
            consumeDelay = 0.7f + Calc.Random.NextFloat() * 0.3f;
            burstSpeed = 80f + Calc.Random.NextFloat() * 40f;
            burstDirection = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, 1f);
            Add(sprite = new Image(GFX.Game["collectables/heartGem/orb"]));
            sprite.CenterOrigin();
            Add(bloom = new BloomPoint(1f, 16f));
        }

        public override void Update()
        {
            base.Update();
            
            Vector2 vector2_1 = Vector2.Zero;
            bool entityDead = false;
            
            if (AbsorbInto != null)
            {
                vector2_1 = AbsorbInto.Center;
                entityDead = AbsorbInto.Scene == null || AbsorbInto is Player { Dead: true };
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
                
                entityDead = entity?.Scene == null || entity.Dead;
            }
            
            if (entityDead)
            {
                Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                burstSpeed = Calc.Approach(burstSpeed, 800f, Engine.RawDeltaTime * 200f);
                
                sprite.Rotation = burstDirection.Angle();
                sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                sprite.Color = Color.White * (alpha = Calc.Approach(alpha, 0.0f, Engine.DeltaTime));
            }
            else if (consumeDelay > 0f)
            {
                Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                burstSpeed = Calc.Approach(burstSpeed, 0f, Engine.RawDeltaTime * 120f);
                sprite.Rotation = burstDirection.Angle();
                sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                consumeDelay -= Engine.RawDeltaTime;
                
                if (consumeDelay > 0f)
                    return;
                
                Vector2 end = vector2_1;
                Vector2 vector2_2 = (Position + end) / 2f;
                Vector2 vector2_3 = (end - Position).SafeNormalize().Perpendicular() * (Position - end).Length() * (0.05f + Calc.Random.NextFloat() * 0.45f);
                float num1 = end.X - Position.X;
                float num2 = end.Y - Position.Y;
                
                if (Math.Abs(num1) > (double) Math.Abs(num2) && Math.Sign(vector2_3.X) != Math.Sign(num1) || Math.Abs(num2) > (double) Math.Abs(num2) && Math.Sign(vector2_3.Y) != Math.Sign(num2))
                    vector2_3 *= -1f;
                
                curve = new SimpleCurve(Position, end, vector2_2 + vector2_3);
                duration = 0.3f + Calc.Random.NextFloat(0.25f);
                burstScale = sprite.Scale;
            }
            else
            {
                curve.End = vector2_1;
                
                if (percent >= 1f)
                    RemoveSelf();
                
                percent = Calc.Approach(percent, 1f, Engine.RawDeltaTime / duration);
                float percentEased = Ease.CubeIn(percent);
                Position = curve.GetPoint(percentEased);
                float num = Calc.YoYo(percentEased) * curve.GetLengthParametric(10);
                
                sprite.Scale = new Vector2(Math.Min(2f, 0.5f + num * 0.02f), Math.Max(0.05f, 0.5f - num * 0.004f));
                sprite.Color = Color.White * (1f - percentEased);
                sprite.Rotation = Calc.Angle(Position, curve.GetPoint(Ease.CubeIn(percent + 0.01f)));
            }
        }
    }
}
