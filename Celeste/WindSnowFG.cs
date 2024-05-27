using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class WindSnowFG : Backdrop
    {
        public Vector2 CameraOffset = Vector2.Zero;
        public float Alpha = 1f;
        private Vector2[] positions;
        private SineWave[] sines;
        private Vector2 scale = Vector2.One;
        private float rotation;
        private float loopWidth = 640f;
        private float loopHeight = 360f;
        private float visibleFade = 1f;

        public WindSnowFG()
        {
            Color = Color.White;
            positions = new Vector2[240];
            for (int index = 0; index < positions.Length; ++index)
                positions[index] = Calc.Random.Range(new Vector2(0.0f, 0.0f), new Vector2(loopWidth, loopHeight));
            sines = new SineWave[16];
            for (int index = 0; index < sines.Length; ++index)
            {
                sines[index] = new SineWave(Calc.Random.Range(0.8f, 1.2f));
                sines[index].Randomize();
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            visibleFade = Calc.Approach(visibleFade, IsVisible(scene as Level) ? 1f : 0.0f, Engine.DeltaTime * 2f);
            Level level = scene as Level;
            foreach (Component sine in sines)
                sine.Update();
            bool flag = level.Wind.Y == 0.0;
            if (flag)
            {
                scale.X = Math.Max(1f, Math.Abs(level.Wind.X) / 100f);
                rotation = Calc.Approach(rotation, 0.0f, Engine.DeltaTime * 8f);
            }
            else
            {
                scale.X = Math.Max(1f, Math.Abs(level.Wind.Y) / 40f);
                rotation = Calc.Approach(rotation, -1.57079637f, Engine.DeltaTime * 8f);
            }
            scale.Y = 1f / Math.Max(1f, scale.X * 0.25f);
            for (int index = 0; index < positions.Length; ++index)
            {
                float num = sines[index % sines.Length].Value;
                Vector2 vector2 = Vector2.Zero;
                vector2 = !flag ? new Vector2(0.0f, (float) (level.Wind.Y * 3.0 + num * 10.0)) : new Vector2(level.Wind.X + num * 10f, 20f);
                positions[index] += vector2 * Engine.DeltaTime;
            }
        }

        public override void Render(Scene scene)
        {
            if (Alpha <= 0.0)
                return;
            Color color = Color * visibleFade * Alpha;
            int num1 = (scene as Level).Wind.Y == 0.0 ? (int) (double) positions.Length : (int) (positions.Length * 0.60000002384185791);
            int num2 = 0;
            foreach (Vector2 position in positions)
            {
                Vector2 positionOut = position;
                positionOut.Y -= (scene as Level).Camera.Y + CameraOffset.Y;
                positionOut.Y %= loopHeight;
                if (positionOut.Y < 0.0)
                    positionOut.Y += loopHeight;
                positionOut.X -= (scene as Level).Camera.X + CameraOffset.X;
                positionOut.X %= loopWidth;
                if (positionOut.X < 0.0)
                    positionOut.X += loopWidth;
                if (num2 < num1)
                    GFX.Game["particles/snow"].DrawCentered(positionOut, color, scale, rotation);
                ++num2;
            }
        }
    }
}
