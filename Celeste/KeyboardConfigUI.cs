// Decompiled with JetBrains decompiler
// Type: Celeste.KeyboardConfigUI
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class KeyboardConfigUI : TextMenu
    {
        private bool remapping;
        private float remappingEase;
        private Binding remappingBinding;
        private string remappingText;
        private float inputDelay;
        private float timeout;
        private bool closing;
        private float closingDelay;
        private bool resetHeld;
        private float resetTime;
        private float resetDelay;

        public KeyboardConfigUI()
        {
            _ = Add(new TextMenu.Header(Dialog.Clean("KEY_CONFIG_TITLE")));
            _ = Add(new InputMappingInfo(false));
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_GAMEPLAY")));
            AddMap("LEFT", Settings.Instance.Left);
            AddMap("RIGHT", Settings.Instance.Right);
            AddMap("UP", Settings.Instance.Up);
            AddMap("DOWN", Settings.Instance.Down);
            AddMap("JUMP", Settings.Instance.Jump);
            AddMap("DASH", Settings.Instance.Dash);
            AddMap("GRAB", Settings.Instance.Grab);
            AddMap("TALK", Settings.Instance.Talk);
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_MENUS")));
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_MENU_NOTICE"), false));
            AddMap("LEFT", Settings.Instance.MenuLeft);
            AddMap("RIGHT", Settings.Instance.MenuRight);
            AddMap("UP", Settings.Instance.MenuUp);
            AddMap("DOWN", Settings.Instance.MenuDown);
            AddMap("CONFIRM", Settings.Instance.Confirm);
            AddMap("CANCEL", Settings.Instance.Cancel);
            AddMap("JOURNAL", Settings.Instance.Journal);
            AddMap("PAUSE", Settings.Instance.Pause);
            _ = Add(new TextMenu.SubHeader(""));
            TextMenu.Button button = new(Dialog.Clean("KEY_CONFIG_RESET"))
            {
                IncludeWidthInMeasurement = false,
                AlwaysCenter = true,
                ConfirmSfx = "event:/ui/main/button_lowkey",
                OnPressed = () =>
                {
                    resetHeld = true;
                    resetTime = 0.0f;
                    resetDelay = 0.0f;
                }
            };
            _ = Add(button);
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_ADVANCED")));
            AddMap("QUICKRESTART", Settings.Instance.QuickRestart);
            AddMap("DEMO", Settings.Instance.DemoDash);
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_MOVE_ONLY")));
            AddMap("LEFT", Settings.Instance.LeftMoveOnly);
            AddMap("RIGHT", Settings.Instance.RightMoveOnly);
            AddMap("UP", Settings.Instance.UpMoveOnly);
            AddMap("DOWN", Settings.Instance.DownMoveOnly);
            _ = Add(new TextMenu.SubHeader(Dialog.Clean("KEY_CONFIG_DASH_ONLY")));
            AddMap("LEFT", Settings.Instance.LeftDashOnly);
            AddMap("RIGHT", Settings.Instance.RightDashOnly);
            AddMap("UP", Settings.Instance.UpDashOnly);
            AddMap("DOWN", Settings.Instance.DownDashOnly);
            OnESC = OnCancel = () =>
            {
                MenuOptions.UpdateCrouchDashModeVisibility();
                Focused = false;
                closing = true;
            };
            MinWidth = 600f;
            Position.Y = ScrollTargetY;
            Alpha = 0.0f;
        }

        private void AddMap(string label, Binding binding)
        {
            string txt = Dialog.Clean("KEY_CONFIG_" + label);
            _ = Add(new TextMenu.Setting(txt, binding, false).Pressed(() =>
            {
                remappingText = txt;
                Remap(binding);
            }).AltPressed(() => Clear(binding)));
        }

        private void Remap(Binding binding)
        {
            remapping = true;
            remappingBinding = binding;
            timeout = 5f;
            Focused = false;
        }

        private void AddRemap(Keys key)
        {
            while (remappingBinding.Keyboard.Count >= Input.MaxBindings)
            {
                remappingBinding.Keyboard.RemoveAt(0);
            }

            remapping = false;
            inputDelay = 0.25f;
            if (!remappingBinding.Add(key))
            {
                _ = Audio.Play("event:/ui/main/button_invalid");
            }

            Input.Initialize();
        }

        private void Clear(Binding binding)
        {
            if (binding.ClearKeyboard())
            {
                return;
            }

            _ = Audio.Play("event:/ui/main/button_invalid");
        }

        public override void Update()
        {
            if (resetHeld)
            {
                resetDelay += Engine.DeltaTime;
                resetTime += Engine.DeltaTime;
                if (resetTime > 1.5)
                {
                    resetDelay = 0.0f;
                    resetHeld = false;
                    Settings.Instance.SetDefaultKeyboardControls(true);
                    Input.Initialize();
                    _ = Audio.Play("event:/ui/main/button_select");
                }
                if (!Input.MenuConfirm.Check && resetDelay > 0.30000001192092896)
                {
                    _ = Audio.Play("event:/ui/main/button_invalid");
                    resetHeld = false;
                }
                if (resetHeld)
                {
                    return;
                }
            }
            base.Update();
            Focused = !closing && inputDelay <= 0.0 && !remapping;
            if (!closing && Input.MenuCancel.Pressed && !remapping)
            {
                OnCancel();
            }

            if (inputDelay > 0.0 && !remapping)
            {
                inputDelay -= Engine.RawDeltaTime;
            }

            remappingEase = Calc.Approach(remappingEase, remapping ? 1f : 0.0f, Engine.RawDeltaTime * 4f);
            if (remappingEase >= 0.25 && remapping)
            {
                if (Input.ESC.Pressed || timeout <= 0.0)
                {
                    remapping = false;
                    Focused = true;
                }
                else
                {
                    Keys[] pressedKeys = MInput.Keyboard.CurrentState.GetPressedKeys();
                    if (pressedKeys != null && pressedKeys.Length != 0 && MInput.Keyboard.Pressed(pressedKeys[pressedKeys.Length - 1]))
                    {
                        AddRemap(pressedKeys[pressedKeys.Length - 1]);
                    }
                }
                timeout -= Engine.RawDeltaTime;
            }
            closingDelay -= Engine.RawDeltaTime;
            Alpha = Calc.Approach(Alpha, !closing || closingDelay > 0.0 ? 1f : 0.0f, Engine.RawDeltaTime * 8f);
            if (!closing || Alpha > 0.0)
            {
                return;
            }

            Close();
        }

        public override void Render()
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(Alpha));
            Vector2 vector2 = new Vector2(1920f, 1080f) * 0.5f;
            base.Render();
            if (remappingEase > 0.0)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * Ease.CubeInOut(remappingEase));
                ActiveFont.Draw(Dialog.Get("KEY_CONFIG_CHANGING"), vector2 + new Vector2(0.0f, -8f), new Vector2(0.5f, 1f), Vector2.One * 0.7f, Color.LightGray * Ease.CubeIn(remappingEase));
                ActiveFont.Draw(remappingText, vector2 + new Vector2(0.0f, 8f), new Vector2(0.5f, 0.0f), Vector2.One * 2f, Color.White * Ease.CubeIn(remappingEase));
            }
            if (!resetHeld)
            {
                return;
            }

            float num1 = Ease.CubeInOut(Calc.Min(1f, resetDelay / 0.2f));
            float num2 = Ease.SineOut(Calc.Min(1f, resetTime / 1.5f));
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * num1);
            float width = 480f;
            double x = (1920.0 - (double)width) / 2.0;
            Draw.Rect((float)x, 530f, width, 20f, Color.White * 0.25f * num1);
            Draw.Rect((float)x, 530f, width * num2, 20f, Color.White * num1);
        }
    }
}
