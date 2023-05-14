// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualJoystick
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Monocle
{
    public class VirtualJoystick : VirtualInput
    {
        public Binding Up;
        public Binding Down;
        public Binding Left;
        public Binding Right;
        public Binding UpAlt;
        public Binding DownAlt;
        public Binding LeftAlt;
        public Binding RightAlt;
        public float Threshold;
        public int GamepadIndex;
        public VirtualInput.OverlapBehaviors OverlapBehavior;
        public bool InvertedX;
        public bool InvertedY;
        private Vector2 value;
        private Vector2 previousValue;
        private bool hTurned;
        private bool vTurned;

        public Vector2 Value { get; private set; }

        public Vector2 PreviousValue { get; private set; }

        public VirtualJoystick(
            Binding up,
            Binding down,
            Binding left,
            Binding right,
            int gamepadIndex,
            float threshold,
            VirtualInput.OverlapBehaviors overlapBehavior = VirtualInput.OverlapBehaviors.TakeNewer)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
            GamepadIndex = gamepadIndex;
            Threshold = threshold;
            OverlapBehavior = overlapBehavior;
        }

        public VirtualJoystick(
            Binding up,
            Binding upAlt,
            Binding down,
            Binding downAlt,
            Binding left,
            Binding leftAlt,
            Binding right,
            Binding rightAlt,
            int gamepadIndex,
            float threshold,
            VirtualInput.OverlapBehaviors overlapBehavior = VirtualInput.OverlapBehaviors.TakeNewer)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
            UpAlt = upAlt;
            DownAlt = downAlt;
            LeftAlt = leftAlt;
            RightAlt = rightAlt;
            GamepadIndex = gamepadIndex;
            Threshold = threshold;
            OverlapBehavior = overlapBehavior;
        }

        public override void Update()
        {
            previousValue = value;
            if (!MInput.Disabled)
            {
                Vector2 zero = value;
                float num1 = Right.Axis(GamepadIndex, 0.0f);
                float num2 = Left.Axis(GamepadIndex, 0.0f);
                float num3 = Down.Axis(GamepadIndex, 0.0f);
                float num4 = Up.Axis(GamepadIndex, 0.0f);
                if ((double)num1 == 0.0 && RightAlt != null)
                {
                    num1 = RightAlt.Axis(GamepadIndex, 0.0f);
                }

                if ((double)num2 == 0.0 && LeftAlt != null)
                {
                    num2 = LeftAlt.Axis(GamepadIndex, 0.0f);
                }

                if ((double)num3 == 0.0 && DownAlt != null)
                {
                    num3 = DownAlt.Axis(GamepadIndex, 0.0f);
                }

                if ((double)num4 == 0.0 && UpAlt != null)
                {
                    num4 = UpAlt.Axis(GamepadIndex, 0.0f);
                }

                if ((double)num1 > (double)num2)
                {
                    num2 = 0.0f;
                }
                else if ((double)num2 > (double)num1)
                {
                    num1 = 0.0f;
                }

                if ((double)num3 > (double)num4)
                {
                    num4 = 0.0f;
                }
                else if ((double)num4 > (double)num3)
                {
                    num3 = 0.0f;
                }

                if ((double)num1 != 0.0 && (double)num2 != 0.0)
                {
                    switch (OverlapBehavior)
                    {
                        case VirtualInput.OverlapBehaviors.CancelOut:
                            zero.X = 0.0f;
                            break;
                        case VirtualInput.OverlapBehaviors.TakeOlder:
                            if (zero.X > 0.0)
                            {
                                zero.X = num1;
                                break;
                            }
                            if (zero.X < 0.0)
                            {
                                zero.X = num2;
                                break;
                            }
                            break;
                        case VirtualInput.OverlapBehaviors.TakeNewer:
                            if (!hTurned)
                            {
                                if (zero.X > 0.0)
                                {
                                    zero.X = -num2;
                                }
                                else if (zero.X < 0.0)
                                {
                                    zero.X = num1;
                                }

                                hTurned = true;
                                break;
                            }
                            if (zero.X > 0.0)
                            {
                                zero.X = num1;
                                break;
                            }
                            if (zero.X < 0.0)
                            {
                                zero.X = -num2;
                                break;
                            }
                            break;
                    }
                }
                else if ((double)num1 != 0.0)
                {
                    hTurned = false;
                    zero.X = num1;
                }
                else if ((double)num2 != 0.0)
                {
                    hTurned = false;
                    zero.X = -num2;
                }
                else
                {
                    hTurned = false;
                    zero.X = 0.0f;
                }
                if ((double)num3 != 0.0 && (double)num4 != 0.0)
                {
                    switch (OverlapBehavior)
                    {
                        case VirtualInput.OverlapBehaviors.CancelOut:
                            zero.Y = 0.0f;
                            break;
                        case VirtualInput.OverlapBehaviors.TakeOlder:
                            if (zero.Y > 0.0)
                            {
                                zero.Y = num3;
                                break;
                            }
                            if (zero.Y < 0.0)
                            {
                                zero.Y = -num4;
                                break;
                            }
                            break;
                        case VirtualInput.OverlapBehaviors.TakeNewer:
                            if (!vTurned)
                            {
                                if (zero.Y > 0.0)
                                {
                                    zero.Y = -num4;
                                }
                                else if (zero.Y < 0.0)
                                {
                                    zero.Y = num3;
                                }

                                vTurned = true;
                                break;
                            }
                            if (zero.Y > 0.0)
                            {
                                zero.Y = num3;
                                break;
                            }
                            if (zero.Y < 0.0)
                            {
                                zero.Y = -num4;
                                break;
                            }
                            break;
                    }
                }
                else if ((double)num3 != 0.0)
                {
                    vTurned = false;
                    zero.Y = num3;
                }
                else if ((double)num4 != 0.0)
                {
                    vTurned = false;
                    zero.Y = -num4;
                }
                else
                {
                    vTurned = false;
                    zero.Y = 0.0f;
                }
                if ((double)zero.Length() < Threshold)
                {
                    zero = Vector2.Zero;
                }

                value = zero;
            }
            Value = new Vector2(InvertedX ? value.X * -1f : value.X, InvertedY ? value.Y * -1f : value.Y);
            PreviousValue = new Vector2(InvertedX ? previousValue.X * -1f : previousValue.X, InvertedY ? previousValue.Y * -1f : previousValue.Y);
        }

        public static implicit operator Vector2(VirtualJoystick joystick)
        {
            return joystick.Value;
        }
    }
}
