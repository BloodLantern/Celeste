using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public class ParticleSystem : Entity
    {
        private Particle[] particles;
        private int nextSlot;

        public ParticleSystem(int depth, int maxParticles)
        {
            particles = new Particle[maxParticles];
            Depth = depth;
        }

        public void Clear()
        {
            for (int index = 0; index < particles.Length; ++index)
                particles[index].Active = false;
        }

        public void ClearRect(Rectangle rect, bool inside)
        {
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 position = particles[index].Position;
                if ((position.X <= (double) rect.Left || position.Y <= (double) rect.Top || position.X >= (double) rect.Right ? 0 : (position.Y < (double) rect.Bottom ? 1 : 0)) == (inside ? 1 : 0))
                    particles[index].Active = false;
            }
        }

        public override void Update()
        {
            for (int index = 0; index < particles.Length; ++index)
            {
                if (particles[index].Active)
                    particles[index].Update();
            }
        }

        public override void Render()
        {
            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    particle.Render();
            }
        }

        public void Render(float alpha)
        {
            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    particle.Render(alpha);
            }
        }

        public void Simulate(float duration, float interval, Action<ParticleSystem> emitter)
        {
            float num1 = 0.016f;
            for (float num2 = 0.0f; num2 < (double) duration; num2 += num1)
            {
                if ((int) ((num2 - (double) num1) / interval) < (int) (num2 / (double) interval))
                    emitter(this);
                for (int index = 0; index < particles.Length; ++index)
                {
                    if (particles[index].Active)
                        particles[index].Update(num1);
                }
            }
        }

        public void Add(Particle particle)
        {
            particles[nextSlot] = particle;
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position)
        {
            type.Create(ref particles[nextSlot], position);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, float direction)
        {
            type.Create(ref particles[nextSlot], position, direction);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, Color color)
        {
            type.Create(ref particles[nextSlot], position, color);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, Color color, float direction)
        {
            type.Create(ref particles[nextSlot], position, color, direction);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange)
        {
            for (int index = 0; index < amount; ++index)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange));
        }

        public void Emit(
            ParticleType type,
            int amount,
            Vector2 position,
            Vector2 positionRange,
            float direction)
        {
            for (int index = 0; index < amount; ++index)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), direction);
        }

        public void Emit(
            ParticleType type,
            int amount,
            Vector2 position,
            Vector2 positionRange,
            Color color)
        {
            for (int index = 0; index < amount; ++index)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color);
        }

        public void Emit(
            ParticleType type,
            int amount,
            Vector2 position,
            Vector2 positionRange,
            Color color,
            float direction)
        {
            for (int index = 0; index < amount; ++index)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color, direction);
        }

        public void Emit(
            ParticleType type,
            Entity track,
            int amount,
            Vector2 position,
            Vector2 positionRange,
            float direction)
        {
            for (int index = 0; index < amount; ++index)
            {
                type.Create(ref particles[nextSlot], track, Calc.Random.Range(position - positionRange, position + positionRange), direction, type.Color);
                nextSlot = (nextSlot + 1) % particles.Length;
            }
        }
    }
}
