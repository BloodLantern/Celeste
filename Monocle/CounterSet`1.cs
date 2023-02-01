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
