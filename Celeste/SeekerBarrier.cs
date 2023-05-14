// Decompiled with JetBrains decompiler
// Type: Celeste.SeekerBarrier
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class SeekerBarrier : Solid
    {
        public float Flash;
        public float Solidify;
        public bool Flashing;
        private float solidifyDelay;
        private readonly List<Vector2> particles = new();
        private readonly List<SeekerBarrier> adjacent = new();
        private readonly float[] speeds = new float[3] { 12f, 20f, 40f };

        public SeekerBarrier(Vector2 position, float width, float height)
            : base(position, width, height, false)
        {
            Collidable = false;
            for (int index = 0; index < (double)Width * (double)Height / 16.0; ++index)
            {
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            }
        }

        public SeekerBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Tracker.GetEntity<SeekerBarrierRenderer>().Track(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Tracker.GetEntity<SeekerBarrierRenderer>().Untrack(this);
        }

        public override void Update()
        {
            if (Flashing)
            {
                Flash = Calc.Approach(Flash, 0.0f, Engine.DeltaTime * 4f);
                if (Flash <= 0.0)
                {
                    Flashing = false;
                }
            }
            else if (solidifyDelay > 0.0)
            {
                solidifyDelay -= Engine.DeltaTime;
            }
            else if (Solidify > 0.0)
            {
                Solidify = Calc.Approach(Solidify, 0.0f, Engine.DeltaTime);
            }

            int length = speeds.Length;
            float height = Height;
            int index = 0;
            for (int count = particles.Count; index < count; ++index)
            {
                Vector2 vector2 = particles[index] + (Vector2.UnitY * speeds[index % length] * Engine.DeltaTime);
                vector2.Y %= height - 1f;
                particles[index] = vector2;
            }
            base.Update();
        }

        public void OnReflectSeeker()
        {
            Flash = 1f;
            Solidify = 1f;
            solidifyDelay = 1f;
            Flashing = true;
            Scene.CollideInto<SeekerBarrier>(new Rectangle((int)X, (int)Y - 2, (int)Width, (int)Height + 4), adjacent);
            Scene.CollideInto<SeekerBarrier>(new Rectangle((int)X - 2, (int)Y, (int)Width + 4, (int)Height), adjacent);
            foreach (SeekerBarrier seekerBarrier in adjacent)
            {
                if (!seekerBarrier.Flashing)
                {
                    seekerBarrier.OnReflectSeeker();
                }
            }
            adjacent.Clear();
        }

        public override void Render()
        {
            Color color = Color.White * 0.5f;
            foreach (Vector2 particle in particles)
            {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
            }

            if (!Flashing)
            {
                return;
            }

            Draw.Rect(Collider, Color.White * Flash * 0.5f);
        }
    }
}
