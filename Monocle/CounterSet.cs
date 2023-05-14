// Decompiled with JetBrains decompiler
// Type: Monocle.CounterSet`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class CounterSet<T> : Component
    {
        private readonly Dictionary<T, float> counters;
        private float timer;

        public CounterSet()
            : base(true, false)
        {
            counters = new Dictionary<T, float>();
        }

        public float this[T index]
        {
            get => counters.TryGetValue(index, out float num) ? Math.Max(num - timer, 0.0f) : 0.0f;
            set => counters[index] = timer + value;
        }

        public bool Check(T index)
        {
            return counters.TryGetValue(index, out float num) && (double)num - timer > 0.0;
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
        }
    }
}
