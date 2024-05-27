using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class BadelineDummy : Entity
    {
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        public BadelineAutoAnimator AutoAnimator;
        public SineWave Wave;
        public VertexLight Light;
        public float FloatSpeed = 120f;
        public float FloatAccel = 240f;
        public float Floatness = 2f;
        public Vector2 floatNormal = new Vector2(0.0f, 1f);

        public BadelineDummy(Vector2 position)
            : base(position)
        {
            Collider = new Hitbox(6f, 6f, -3f, -7f);
            Sprite = new PlayerSprite(PlayerSpriteMode.Badeline);
            Sprite.Play("fallSlow");
            Sprite.Scale.X = -1f;
            Hair = new PlayerHair(Sprite);
            Hair.Color = BadelineOldsite.HairColor;
            Hair.Border = Color.Black;
            Hair.Facing = Facings.Left;
            Add(Hair);
            Add(Sprite);
            Add(AutoAnimator = new BadelineAutoAnimator());
            Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = Sprite.CurrentAnimationFrame;
                if ((!(anim == "walk") || currentAnimationFrame != 0 && currentAnimationFrame != 6) && (!(anim == "runSlow") || currentAnimationFrame != 0 && currentAnimationFrame != 6) && (!(anim == "runFast") || currentAnimationFrame != 0 && currentAnimationFrame != 6))
                    return;
                Audio.Play("event:/char/badeline/footstep", Position);
            };
            Add(Wave = new SineWave(0.25f));
            Wave.OnUpdate = f => Sprite.Position = floatNormal * f * Floatness;
            Add(Light = new VertexLight(new Vector2(0.0f, -8f), Color.PaleVioletRed, 1f, 20, 60));
        }

        public void Appear(Level level, bool silent = false)
        {
            if (!silent)
            {
                Audio.Play("event:/char/badeline/appear", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
        }

        public void Vanish()
        {
            Audio.Play("event:/char/badeline/disappear", Position);
            Shockwave();
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
            RemoveSelf();
        }

        private void Shockwave() => SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);

        public IEnumerator FloatTo(
            Vector2 target,
            int? turnAtEndTo = null,
            bool faceDirection = true,
            bool fadeLight = false,
            bool quickEnd = false)
        {
            BadelineDummy badelineDummy = this;
            badelineDummy.Sprite.Play("fallSlow");
            if (faceDirection && Math.Sign(target.X - badelineDummy.X) != 0)
                badelineDummy.Sprite.Scale.X = Math.Sign(target.X - badelineDummy.X);
            Vector2 vector2 = (target - badelineDummy.Position).SafeNormalize();
            Vector2 perp = new Vector2(-vector2.Y, vector2.X);
            float speed = 0.0f;
            while (badelineDummy.Position != target)
            {
                speed = Calc.Approach(speed, badelineDummy.FloatSpeed, badelineDummy.FloatAccel * Engine.DeltaTime);
                badelineDummy.Position = Calc.Approach(badelineDummy.Position, target, speed * Engine.DeltaTime);
                badelineDummy.Floatness = Calc.Approach(badelineDummy.Floatness, 4f, 8f * Engine.DeltaTime);
                badelineDummy.floatNormal = Calc.Approach(badelineDummy.floatNormal, perp, Engine.DeltaTime * 12f);
                if (fadeLight)
                    badelineDummy.Light.Alpha = Calc.Approach(badelineDummy.Light.Alpha, 0.0f, Engine.DeltaTime * 2f);
                yield return null;
            }
            if (quickEnd)
            {
                badelineDummy.Floatness = 2f;
            }
            else
            {
                while (badelineDummy.Floatness != 2.0)
                {
                    badelineDummy.Floatness = Calc.Approach(badelineDummy.Floatness, 2f, 8f * Engine.DeltaTime);
                    yield return null;
                }
            }
            if (turnAtEndTo.HasValue)
                badelineDummy.Sprite.Scale.X = turnAtEndTo.Value;
        }

        public IEnumerator WalkTo(float x, float speed = 64f)
        {
            BadelineDummy badelineDummy = this;
            badelineDummy.Floatness = 0.0f;
            badelineDummy.Sprite.Play("walk");
            if (Math.Sign(x - badelineDummy.X) != 0)
                badelineDummy.Sprite.Scale.X = Math.Sign(x - badelineDummy.X);
            while (badelineDummy.X != (double) x)
            {
                badelineDummy.X = Calc.Approach(badelineDummy.X, x, Engine.DeltaTime * speed);
                yield return null;
            }
            badelineDummy.Sprite.Play("idle");
        }

        public IEnumerator SmashBlock(Vector2 target)
        {
            BadelineDummy badelineDummy = this;
            badelineDummy.SceneAs<Level>().Displacement.AddBurst(badelineDummy.Position, 0.5f, 24f, 96f);
            badelineDummy.Sprite.Play("dreamDashLoop");
            Vector2 from = badelineDummy.Position;
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 6f)
            {
                badelineDummy.Position = from + (target - from) * Ease.CubeOut(p);
                yield return null;
            }
            badelineDummy.Scene.Entities.FindFirst<DashBlock>().Break(badelineDummy.Position, new Vector2(0.0f, -1f), false);
            badelineDummy.Sprite.Play("idle");
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 4f)
            {
                badelineDummy.Position = target + (from - target) * Ease.CubeOut(p);
                yield return null;
            }
            badelineDummy.Sprite.Play("fallSlow");
        }

        public override void Update()
        {
            if (Sprite.Scale.X != 0.0)
                Hair.Facing = (Facings) Math.Sign(Sprite.Scale.X);
            base.Update();
        }

        public override void Render()
        {
            Vector2 renderPosition = Sprite.RenderPosition;
            Sprite.RenderPosition = Sprite.RenderPosition.Floor();
            base.Render();
            Sprite.RenderPosition = renderPosition;
        }
    }
}
