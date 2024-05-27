using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class DeathEffect : Component
    {
        public Vector2 Position;
        public Color Color;
        public float Percent;
        public float Duration = 0.834f;
        public Action<float> OnUpdate;
        public Action OnEnd;

        public DeathEffect(Color color, Vector2? offset = null)
            : base(true, true)
        {
            Color = color;
            Position = offset.HasValue ? offset.Value : Vector2.Zero;
            Percent = 0.0f;
        }

        public override void Update()
        {
            base.Update();
            if (Percent > 1.0)
            {
                RemoveSelf();
                if (OnEnd != null)
                    OnEnd();
            }
            Percent = Calc.Approach(Percent, 1f, Engine.DeltaTime / Duration);
            if (OnUpdate == null)
                return;
            OnUpdate(Percent);
        }

        public override void Render() => DeathEffect.Draw(Entity.Position + Position, Color, Percent);

        public static void Draw(Vector2 position, Color color, float ease)
        {
            Color color1 = Math.Floor(ease * 10.0) % 2.0 == 0.0 ? color : Color.White;
            MTexture mtexture = GFX.Game["characters/player/hair00"];
            float num = ease < 0.5 ? 0.5f + ease : Ease.CubeOut((float) (1.0 - (ease - 0.5) * 2.0));
            for (int index = 0; index < 8; ++index)
            {
                Vector2 vector = Calc.AngleToVector((float) ((index / 8.0 + ease * 0.25) * 6.2831854820251465), Ease.CubeOut(ease) * 24f);
                mtexture.DrawCentered(position + vector + new Vector2(-1f, 0.0f), Color.Black, new Vector2(num, num));
                mtexture.DrawCentered(position + vector + new Vector2(1f, 0.0f), Color.Black, new Vector2(num, num));
                mtexture.DrawCentered(position + vector + new Vector2(0.0f, -1f), Color.Black, new Vector2(num, num));
                mtexture.DrawCentered(position + vector + new Vector2(0.0f, 1f), Color.Black, new Vector2(num, num));
            }
            for (int index = 0; index < 8; ++index)
            {
                Vector2 vector = Calc.AngleToVector((float) ((index / 8.0 + ease * 0.25) * 6.2831854820251465), Ease.CubeOut(ease) * 24f);
                mtexture.DrawCentered(position + vector, color1, new Vector2(num, num));
            }
        }
    }
}
