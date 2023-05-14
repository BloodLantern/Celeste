// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualIntegerAxis
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
        public VirtualInput.OverlapBehaviors OverlapBehavior;
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
            VirtualInput.OverlapBehaviors overlapBehavior = VirtualInput.OverlapBehaviors.TakeNewer)
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
            VirtualInput.OverlapBehaviors overlapBehavior = VirtualInput.OverlapBehaviors.TakeNewer)
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
            {
                return;
            }

            bool flag1 = (double)Positive.Axis(GamepadIndex, Threshold) > 0.0 || (PositiveAlt != null && (double)PositiveAlt.Axis(GamepadIndex, Threshold) > 0.0);
            bool flag2 = (double)Negative.Axis(GamepadIndex, Threshold) > 0.0 || (NegativeAlt != null && (double)NegativeAlt.Axis(GamepadIndex, Threshold) > 0.0);
            if (flag1 & flag2)
            {
                switch (OverlapBehavior)
                {
                    case VirtualInput.OverlapBehaviors.CancelOut:
                        Value = 0;
                        break;
                    case VirtualInput.OverlapBehaviors.TakeOlder:
                        Value = PreviousValue;
                        break;
                    case VirtualInput.OverlapBehaviors.TakeNewer:
                        if (!turned)
                        {
                            Value *= -1;
                            turned = true;
                            break;
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
            {
                return;
            }

            Value = -Value;
        }

        public static implicit operator float(VirtualIntegerAxis axis)
        {
            return axis.Value;
        }
    }
}
