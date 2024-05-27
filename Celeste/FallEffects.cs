using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class FallEffects : Entity
    {
        private static readonly Color[] colors = new Color[2]
        {
            Color.White,
            Color.LightGray
        };
        private static readonly Color[] faded = new Color[2];
        private Particle[] particles = new Particle[50];
        private float fade;
        private bool enabled;
        public static float SpeedMultiplier = 1f;

        public FallEffects()
        {
            Tag = (int) Tags.Global;
            Depth = -1000000;
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
                particles[index].Speed = Calc.Random.Range(120, 240);
                particles[index].Color = Calc.Random.Next(FallEffects.colors.Length);
            }
        }

        public static void Show(bool visible)
        {
            FallEffects fallEffects = Engine.Scene.Tracker.GetEntity<FallEffects>();
            if (fallEffects == null & visible)
                Engine.Scene.Add(fallEffects = new FallEffects());
            if (fallEffects != null)
                fallEffects.enabled = visible;
            FallEffects.SpeedMultiplier = 1f;
        }

        public override void Update()
        {
            base.Update();
            for (int index = 0; index < particles.Length; ++index)
                particles[index].Position -= Vector2.UnitY * particles[index].Speed * FallEffects.SpeedMultiplier * Engine.DeltaTime;
            fade = Calc.Approach(fade, enabled ? 1f : 0.0f, (enabled ? 1f : 4f) * Engine.DeltaTime);
        }

        public override void Render()
        {
            if (fade <= 0.0)
                return;
            Camera camera = (Scene as Level).Camera;
            for (int index = 0; index < FallEffects.faded.Length; ++index)
                FallEffects.faded[index] = FallEffects.colors[index] * fade;
            for (int index = 0; index < particles.Length; ++index)
            {
                float height = 8f * FallEffects.SpeedMultiplier;
                Draw.Rect(new Vector2
                {
                    X = mod(particles[index].Position.X - camera.X, 320f),
                    Y = mod((float) (particles[index].Position.Y - (double) camera.Y - 16.0), 212f)
                } + camera.Position - new Vector2(0.0f, height / 2f), 1f, height, FallEffects.faded[particles[index].Color]);
            }
        }

        private float mod(float x, float m) => (x % m + m) % m;

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public int Color;
        }
    }
}
