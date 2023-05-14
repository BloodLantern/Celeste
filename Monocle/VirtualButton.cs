﻿// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualButton
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework.Input;

namespace Monocle
{
    public class VirtualButton : VirtualInput
    {
        public Binding Binding;
        public float Threshold;
        public float BufferTime;
        public int GamepadIndex;
        public Keys? DebugOverridePressed;
        private float firstRepeatTime;
        private float multiRepeatTime;
        private float bufferCounter;
        private float repeatCounter;
        private bool canRepeat;
        private bool consumed;

        public bool Repeating { get; private set; }

        public VirtualButton(
            Binding binding,
            int gamepadIndex,
            float bufferTime,
            float triggerThreshold)
        {
            Binding = binding;
            GamepadIndex = gamepadIndex;
            BufferTime = bufferTime;
            Threshold = triggerThreshold;
        }

        public VirtualButton()
        {
        }

        public void SetRepeat(float repeatTime)
        {
            SetRepeat(repeatTime, repeatTime);
        }

        public void SetRepeat(float firstRepeatTime, float multiRepeatTime)
        {
            this.firstRepeatTime = firstRepeatTime;
            this.multiRepeatTime = multiRepeatTime;
            canRepeat = this.firstRepeatTime > 0.0;
            if (canRepeat)
            {
                return;
            }

            Repeating = false;
        }

        public override void Update()
        {
            consumed = false;
            bufferCounter -= Engine.DeltaTime;
            bool flag = false;
            if (Binding.Pressed(GamepadIndex, Threshold))
            {
                bufferCounter = BufferTime;
                flag = true;
            }
            else if (Binding.Check(GamepadIndex, Threshold))
            {
                flag = true;
            }

            if (!flag)
            {
                Repeating = false;
                repeatCounter = 0.0f;
                bufferCounter = 0.0f;
            }
            else
            {
                if (!canRepeat)
                {
                    return;
                }

                Repeating = false;
                if (repeatCounter == 0.0)
                {
                    repeatCounter = firstRepeatTime;
                }
                else
                {
                    repeatCounter -= Engine.DeltaTime;
                    if (repeatCounter > 0.0)
                    {
                        return;
                    }

                    Repeating = true;
                    repeatCounter = multiRepeatTime;
                }
            }
        }

        public bool Check => !MInput.Disabled && Binding.Check(GamepadIndex, Threshold);

        public bool Pressed => (DebugOverridePressed.HasValue && MInput.Keyboard.Check(DebugOverridePressed.Value))
|| (!MInput.Disabled && !consumed && (bufferCounter > 0.0 || Repeating || Binding.Pressed(GamepadIndex, Threshold)));

        public bool Released => !MInput.Disabled && Binding.Released(GamepadIndex, Threshold);

        public void ConsumeBuffer()
        {
            bufferCounter = 0.0f;
        }

        public void ConsumePress()
        {
            bufferCounter = 0.0f;
            consumed = true;
        }

        public static implicit operator bool(VirtualButton button)
        {
            return button.Check;
        }
    }
}
