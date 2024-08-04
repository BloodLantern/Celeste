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
            counters = new Dictionary<T, float>();
        }

        public float this[T index]
        {
            get
            {
                float num;
                return counters.TryGetValue(index, out num) ? Math.Max(num - timer, 0.0f) : 0.0f;
            }
            set => counters[index] = timer + value;
        }

        public bool Check(T index)
        {
            float num;
            return counters.TryGetValue(index, out num) && num - (double) timer > 0.0;
        }

        public override void Update() => timer += Engine.DeltaTime;
    }
}
