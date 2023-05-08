// Decompiled with JetBrains decompiler
// Type: Celeste.ChimneySmokeFx
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class ChimneySmokeFx
    {
        public static void Burst(Vector2 position, float direction, int count, ParticleSystem system = null)
        {
            Vector2 vector = Calc.AngleToVector(direction - (float) Math.PI, 2f);
            vector.X = Math.Abs(vector.X);
            vector.Y = Math.Abs(vector.Y);
            system ??= (Engine.Scene as Level).ParticlesFG;
            for (int index = 0; index < count; ++index)
                system.Emit(Calc.Random.Choose(ParticleTypes.Chimney), position + Calc.Random.Range(-vector, vector), direction);
        }
    }
}
