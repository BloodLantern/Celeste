using System;
using System.Collections.Generic;

namespace Monocle
{
    public class CounterSet<T> : Component
    {
        private Dictionary<T, float> counters;
        private float timer;

        public CounterSet()
            : base(true, false)
        {
            this.counters = new Dictionary<T, float>();
        }

        public float this[T index]
        {
            get
            {
                float num;
                return this.counters.TryGetValue(index, out num) ? Math.Max(num - this.timer, 0.0f) : 0.0f;
            }
            set => this.counters[index] = this.timer + value;
        }

        public bool Check(T index)
        {
            float num;
            return this.counters.TryGetValue(index, out num) && (double) num - (double) this.timer > 0.0;
        }

        public override void Update() => this.timer += Engine.DeltaTime;
    }
}
