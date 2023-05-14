// Decompiled with JetBrains decompiler
// Type: Monocle.ParticleEmitter
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Monocle
{
    public class ParticleEmitter : Component
    {
        public ParticleSystem System;
        public ParticleType Type;
        public Entity Track;
        public float Interval;
        public Vector2 Position;
        public Vector2 Range;
        public int Amount;
        public float? Direction;
        private float timer;

        public ParticleEmitter(
            ParticleSystem system,
            ParticleType type,
            Vector2 position,
            Vector2 range,
            int amount,
            float interval)
            : base(true, false)
        {
            System = system;
            Type = type;
            Position = position;
            Range = range;
            Amount = amount;
            Interval = interval;
        }

        public ParticleEmitter(
            ParticleSystem system,
            ParticleType type,
            Vector2 position,
            Vector2 range,
            float direction,
            int amount,
            float interval)
            : this(system, type, position, range, amount, interval)
        {
            Direction = new float?(direction);
        }

        public ParticleEmitter(
            ParticleSystem system,
            ParticleType type,
            Entity track,
            Vector2 position,
            Vector2 range,
            float direction,
            int amount,
            float interval)
            : this(system, type, position, range, amount, interval)
        {
            Direction = new float?(direction);
            Track = track;
        }

        public void SimulateCycle()
        {
            Simulate(Type.LifeMax);
        }

        public void Simulate(float duration)
        {
            float num = duration / Interval;
            for (int index1 = 0; index1 < (double)num; ++index1)
            {
                for (int index2 = 0; index2 < Amount; ++index2)
                {
                    Particle particle = new();
                    Vector2 position = Entity.Position + Position + Calc.Random.Range(-Range, Range);
                    particle = (!Direction.HasValue ? Type.Create(ref particle, position) : Type.Create(ref particle, position, Direction.Value)) with
                    {
                        Track = Track
                    };
                    float duration1 = duration - (Interval * index1);
                    if (particle.SimulateFor(duration1))
                    {
                        System.Add(particle);
                    }
                }
            }
        }

        public void Emit()
        {
            if (Direction.HasValue)
            {
                System.Emit(Type, Amount, Entity.Position + Position, Range, Direction.Value);
            }
            else
            {
                System.Emit(Type, Amount, Entity.Position + Position, Range);
            }
        }

        public override void Update()
        {
            timer -= Engine.DeltaTime;
            if (timer > 0.0)
            {
                return;
            }

            timer = Interval;
            Emit();
        }
    }
}
