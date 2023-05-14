// Decompiled with JetBrains decompiler
// Type: Monocle.SineWave
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Monocle
{
    public class SineWave : Component
    {
        public float Frequency = 1f;
        public float Rate = 1f;
        public Action<float> OnUpdate;
        public bool UseRawDeltaTime;
        private float counter;

        public float Value { get; private set; }

        public float ValueOverTwo { get; private set; }

        public float TwoValue { get; private set; }

        public SineWave()
            : base(true, false)
        {
        }

        public SineWave(float frequency, float offset = 0.0f)
            : this()
        {
            Frequency = frequency;
            Counter = offset;
        }

        public override void Update()
        {
            Counter += (float)(6.2831854820251465 * Frequency * Rate * (UseRawDeltaTime ? (double)Engine.RawDeltaTime : (double)Engine.DeltaTime));
            if (OnUpdate == null)
            {
                return;
            }

            OnUpdate(Value);
        }

        public float ValueOffset(float offset)
        {
            return (float)Math.Sin(counter + (double)offset);
        }

        public SineWave Randomize()
        {
            Counter = (float)((double)Calc.Random.NextFloat() * 6.2831854820251465 * 2.0);
            return this;
        }

        public void Reset()
        {
            Counter = 0.0f;
        }

        public void StartUp()
        {
            Counter = 1.57079637f;
        }

        public void StartDown()
        {
            Counter = 4.712389f;
        }

        public float Counter
        {
            get => counter;
            set
            {
                counter = (float)(((double)value + 25.132741928100586) % 25.132741928100586);
                Value = (float)Math.Sin(counter);
                ValueOverTwo = (float)Math.Sin(counter / 2.0);
                TwoValue = (float)Math.Sin(counter * 2.0);
            }
        }
    }
}
