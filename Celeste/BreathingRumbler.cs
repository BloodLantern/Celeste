// Decompiled with JetBrains decompiler
// Type: Celeste.BreathingRumbler
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
    public class BreathingRumbler : Entity
    {
        private const float MaxRumble = 0.25f;
        public float Strength = 0.2f;
        private float currentRumble;

        public BreathingRumbler()
        {
            currentRumble = Strength;
        }

        public override void Update()
        {
            base.Update();
            currentRumble = Calc.Approach(currentRumble, Strength, 2f * Engine.DeltaTime);
            if (currentRumble <= 0f)
            {
                return;
            }

            Input.RumbleSpecific(currentRumble * MaxRumble, 0.05f);
        }
    }
}
