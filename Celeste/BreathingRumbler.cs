using Monocle;

namespace Celeste
{
    public class BreathingRumbler : Entity
    {
        private const float MaxRumble = 0.25f;
        public float Strength = 0.2f;
        private float currentRumble;

        public BreathingRumbler() => this.currentRumble = this.Strength;

        public override void Update()
        {
            base.Update();
            this.currentRumble = Calc.Approach(this.currentRumble, this.Strength, 2f * Engine.DeltaTime);
            if ((double) this.currentRumble <= 0.0)
                return;
            Input.RumbleSpecific(this.currentRumble * 0.25f, 0.05f);
        }
    }
}
