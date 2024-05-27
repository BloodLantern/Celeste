using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class ChimneySmokeFx
    {
        public static void Burst(Vector2 position, float direction, int count, ParticleSystem system = null)
        {
            Vector2 vector = Calc.AngleToVector(direction - 1.57079637f, 2f);
            vector.X = Math.Abs(vector.X);
            vector.Y = Math.Abs(vector.Y);
            if (system == null)
                system = (Engine.Scene as Level).ParticlesFG;
            for (int index = 0; index < count; ++index)
                system.Emit(Calc.Random.Choose(ParticleTypes.Chimney), position + Calc.Random.Range(-vector, vector), direction);
        }
    }
}
