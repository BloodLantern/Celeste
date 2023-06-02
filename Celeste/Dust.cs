using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public static class Dust
    {
        public static void Burst(
            Vector2 position,
            float direction,
            int count = 1,
            ParticleType particleType = null)
        {
            if (particleType == null)
                particleType = ParticleTypes.Dust;
            Vector2 vector = Calc.AngleToVector(direction - 1.57079637f, 4f);
            vector.X = Math.Abs(vector.X);
            vector.Y = Math.Abs(vector.Y);
            Level scene = Engine.Scene as Level;
            for (int index = 0; index < count; ++index)
                scene.Particles.Emit(particleType, position + Calc.Random.Range(-vector, vector), direction);
        }

        public static void BurstFG(
            Vector2 position,
            float direction,
            int count = 1,
            float range = 4f,
            ParticleType particleType = null)
        {
            if (particleType == null)
                particleType = ParticleTypes.Dust;
            Vector2 vector = Calc.AngleToVector(direction - 1.57079637f, range);
            vector.X = Math.Abs(vector.X);
            vector.Y = Math.Abs(vector.Y);
            Level scene = Engine.Scene as Level;
            for (int index = 0; index < count; ++index)
                scene.ParticlesFG.Emit(particleType, position + Calc.Random.Range(-vector, vector), direction);
        }
    }
}
