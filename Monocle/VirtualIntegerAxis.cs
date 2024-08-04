namespace Monocle
{
    public class VirtualIntegerAxis : VirtualInput
    {
        public Binding Positive;
        public Binding Negative;
        public Binding PositiveAlt;
        public Binding NegativeAlt;
        public float Threshold;
        public int GamepadIndex;
        public OverlapBehaviors OverlapBehavior;
        public bool Inverted;
        public int Value;
        public int PreviousValue;
        private bool turned;

        public VirtualIntegerAxis()
        {
        }

        public VirtualIntegerAxis(
            Binding negative,
            Binding positive,
            int gamepadIndex,
            float threshold,
            OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
        {
            Positive = positive;
            Negative = negative;
            Threshold = threshold;
            GamepadIndex = gamepadIndex;
            OverlapBehavior = overlapBehavior;
        }

        public VirtualIntegerAxis(
            Binding negative,
            Binding negativeAlt,
            Binding positive,
            Binding positiveAlt,
            int gamepadIndex,
            float threshold,
            OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
        {
            Positive = positive;
            Negative = negative;
            PositiveAlt = positiveAlt;
            NegativeAlt = negativeAlt;
            Threshold = threshold;
            GamepadIndex = gamepadIndex;
            OverlapBehavior = overlapBehavior;
        }

        public override void Update()
        {
            PreviousValue = Value;
            if (MInput.Disabled)
                return;
            bool flag1 = Positive.Axis(GamepadIndex, Threshold) > 0.0 || PositiveAlt != null && PositiveAlt.Axis(GamepadIndex, Threshold) > 0.0;
            bool flag2 = Negative.Axis(GamepadIndex, Threshold) > 0.0 || NegativeAlt != null && NegativeAlt.Axis(GamepadIndex, Threshold) > 0.0;
            if (flag1 & flag2)
            {
                switch (OverlapBehavior)
                {
                    case OverlapBehaviors.CancelOut:
                        Value = 0;
                        break;
                    case OverlapBehaviors.TakeOlder:
                        Value = PreviousValue;
                        break;
                    case OverlapBehaviors.TakeNewer:
                        if (!turned)
                        {
                            Value *= -1;
                            turned = true;
                        }
                        break;
                }
            }
            else if (flag1)
            {
                turned = false;
                Value = 1;
            }
            else if (flag2)
            {
                turned = false;
                Value = -1;
            }
            else
            {
                turned = false;
                Value = 0;
            }
            if (!Inverted)
                return;
            Value = -Value;
        }

        public static implicit operator float(VirtualIntegerAxis axis) => axis.Value;
    }
}
