// Decompiled with JetBrains decompiler
// Type: Celeste.ButtonConfigUI
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class ButtonConfigUI : TextMenu
    {
        private bool remapping;
        private float remappingEase;
        private Binding remappingBinding;
        private string remappingText;
        private float inputDelay;
        private float timeout;
        private bool closing;
        private float closingDelay;
        private bool waitingForController;
        private bool resetHeld;
        private float resetTime;
        private float resetDelay;
        private readonly List<Buttons> all = new()
        {
            Buttons.A,
            Buttons.B,
            Buttons.X,
            Buttons.Y,
            Buttons.LeftShoulder,
            Buttons.RightShoulder,
            Buttons.LeftTrigger,
            Buttons.RightTrigger
        };
        public static readonly string StadiaControllerDisclaimer = "No endorsement or affiliation is intended between Stadia and the manufacturers\nof non-Stadia controllers or consoles. STADIA, the Stadia beacon, Google, and related\nmarks and logos are trademarks of Google LLC. All other trademarks are the\nproperty of their respective owners.";

        public ButtonConfigUI()
        {
            _ = Add(new Header(Dialog.Clean("BTN_CONFIG_TITLE")));
            _ = Add(new InputMappingInfo(true));
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_GAMEPLAY")));
            AddMap("LEFT", Settings.Instance.Left);
            AddMap("RIGHT", Settings.Instance.Right);
            AddMap("UP", Settings.Instance.Up);
            AddMap("DOWN", Settings.Instance.Down);
            AddMap("JUMP", Settings.Instance.Jump);
            AddMap("DASH", Settings.Instance.Dash);
            AddMap("GRAB", Settings.Instance.Grab);
            AddMap("TALK", Settings.Instance.Talk);
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MENUS")));
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MENU_NOTICE"), false));
            AddMap("LEFT", Settings.Instance.MenuLeft);
            AddMap("RIGHT", Settings.Instance.MenuRight);
            AddMap("UP", Settings.Instance.MenuUp);
            AddMap("DOWN", Settings.Instance.MenuDown);
            AddMap("CONFIRM", Settings.Instance.Confirm);
            AddMap("CANCEL", Settings.Instance.Cancel);
            AddMap("JOURNAL", Settings.Instance.Journal);
            AddMap("PAUSE", Settings.Instance.Pause);
            _ = Add(new SubHeader(""));
            Button button = new(Dialog.Clean("KEY_CONFIG_RESET"))
            {
                IncludeWidthInMeasurement = false,
                AlwaysCenter = true,
                OnPressed = () =>
                {
                    resetHeld = true;
                    resetTime = 0f;
                    resetDelay = 0f;
                },
                ConfirmSfx = "event:/ui/main/button_lowkey"
            };
            _ = Add(button);
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_ADVANCED")));
            AddMap("QUICKRESTART", Settings.Instance.QuickRestart);
            AddMap("DEMO", Settings.Instance.DemoDash);
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MOVE_ONLY")));
            AddMap("LEFT", Settings.Instance.LeftMoveOnly);
            AddMap("RIGHT", Settings.Instance.RightMoveOnly);
            AddMap("UP", Settings.Instance.UpMoveOnly);
            AddMap("DOWN", Settings.Instance.DownMoveOnly);
            _ = Add(new SubHeader(Dialog.Clean("KEY_CONFIG_DASH_ONLY")));
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
            Alpha = 0f;
        }

        private void AddMap(string label, Binding binding)
        {
            string txt = Dialog.Clean("KEY_CONFIG_" + label);
            _ = Add(new Setting(txt, binding, true).Pressed(() =>
            {
                remappingText = txt;
                Remap(binding);
            }).AltPressed(() => Clear(binding)));
        }

        private void Remap(Binding binding)
        {
            if (!Input.GuiInputController())
                return;

            remapping = true;
            remappingBinding = binding;
            timeout = 5f;
            Focused = false;
        }

        private void AddRemap(Buttons btn)
        {
            while (remappingBinding.Controller.Count >= Input.MaxBindings)
                remappingBinding.Controller.RemoveAt(0);

            remapping = false;
            inputDelay = 0.25f;
            if (!remappingBinding.Add(btn))
                _ = Audio.Play("event:/ui/main/button_invalid");

            Input.Initialize();
        }

        private void Clear(Binding binding)
        {
            if (binding.ClearGamepad())
                return;

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
                    Settings.Instance.SetDefaultButtonControls(true);
                    Input.Initialize();
                    _ = Audio.Play("event:/ui/main/button_select");
                }
                if (!Input.MenuConfirm.Check && resetDelay > 0.3f)
                {
                    _ = Audio.Play("event:/ui/main/button_invalid");
                    resetHeld = false;
                }
                if (resetHeld)
                    return;
            }
            base.Update();
            Focused = !closing && inputDelay <= 0 && !waitingForController && !remapping;
            if (!closing)
            {
                if (!MInput.GamePads[Input.Gamepad].Attached)
                    waitingForController = true;
                else if (waitingForController)
                    waitingForController = false;

                if (Input.MenuCancel.Pressed && !remapping)
                    OnCancel();
            }
            if (inputDelay > 0 && !remapping)
                inputDelay -= Engine.RawDeltaTime;

            remappingEase = Calc.Approach(remappingEase, remapping ? 1f : 0f, Engine.RawDeltaTime * 4f);
            if (remappingEase >= 0.25 && remapping)
            {
                if (Input.ESC.Pressed || timeout <= 0 || !Input.GuiInputController())
                {
                    remapping = false;
                    Focused = true;
                }
                else
                {
                    MInput.GamePadData gamePad = MInput.GamePads[Input.Gamepad];
                    float num = 0.25f;
                    if (gamePad.LeftStickLeftPressed(num))
                        AddRemap(Buttons.LeftThumbstickLeft);
                    else if (gamePad.LeftStickRightPressed(num))
                        AddRemap(Buttons.LeftThumbstickRight);
                    else if (gamePad.LeftStickUpPressed(num))
                        AddRemap(Buttons.LeftThumbstickUp);
                    else if (gamePad.LeftStickDownPressed(num))
                        AddRemap(Buttons.LeftThumbstickDown);
                    else if (gamePad.RightStickLeftPressed(num))
                        AddRemap(Buttons.RightThumbstickLeft);
                    else if (gamePad.RightStickRightPressed(num))
                        AddRemap(Buttons.RightThumbstickRight);
                    else if (gamePad.RightStickDownPressed(num))
                        AddRemap(Buttons.RightThumbstickDown);
                    else if (gamePad.RightStickUpPressed(num))
                        AddRemap(Buttons.RightThumbstickUp);
                    else if (gamePad.LeftTriggerPressed(num))
                        AddRemap(Buttons.LeftTrigger);
                    else if (gamePad.RightTriggerPressed(num))
                        AddRemap(Buttons.RightTrigger);
                    else if (gamePad.Pressed(Buttons.DPadLeft))
                        AddRemap(Buttons.DPadLeft);
                    else if (gamePad.Pressed(Buttons.DPadRight))
                        AddRemap(Buttons.DPadRight);
                    else if (gamePad.Pressed(Buttons.DPadUp))
                        AddRemap(Buttons.DPadUp);
                    else if (gamePad.Pressed(Buttons.DPadDown))
                        AddRemap(Buttons.DPadDown);
                    else if (gamePad.Pressed(Buttons.A))
                        AddRemap(Buttons.A);
                    else if (gamePad.Pressed(Buttons.B))
                        AddRemap(Buttons.B);
                    else if (gamePad.Pressed(Buttons.X))
                        AddRemap(Buttons.X);
                    else if (gamePad.Pressed(Buttons.Y))
                        AddRemap(Buttons.Y);
                    else if (gamePad.Pressed(Buttons.Start))
                        AddRemap(Buttons.Start);
                    else if (gamePad.Pressed(Buttons.Back))
                        AddRemap(Buttons.Back);
                    else if (gamePad.Pressed(Buttons.LeftShoulder))
                        AddRemap(Buttons.LeftShoulder);
                    else if (gamePad.Pressed(Buttons.RightShoulder))
                        AddRemap(Buttons.RightShoulder);
                    else if (gamePad.Pressed(Buttons.LeftStick))
                        AddRemap(Buttons.LeftStick);
                    else if (gamePad.Pressed(Buttons.RightStick))
                        AddRemap(Buttons.RightStick);
                }
                timeout -= Engine.RawDeltaTime;
            }
            closingDelay -= Engine.RawDeltaTime;
            Alpha = Calc.Approach(Alpha, !closing || closingDelay > 0 ? 1f : 0f, Engine.RawDeltaTime * 8f);
            if (!closing || Alpha > 0)
                return;

            Close();
        }

        public override void Render()
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(Alpha));
            Vector2 position = new Vector2(1920f, 1080f) * 0.5f;
            if (MInput.GamePads[Input.Gamepad].Attached)
            {
                base.Render();
                if (remappingEase > 0)
                {
                    Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * Ease.CubeInOut(remappingEase));
                    ActiveFont.Draw(Dialog.Get("BTN_CONFIG_CHANGING"), position + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Vector2.One * 0.7f, Color.LightGray * Ease.CubeIn(remappingEase));
                    ActiveFont.Draw(remappingText, position + new Vector2(0f, 8f), new Vector2(0.5f, 0f), Vector2.One * 2f, Color.White * Ease.CubeIn(remappingEase));
                }
            }
            else
                ActiveFont.Draw(Dialog.Clean("BTN_CONFIG_NOCONTROLLER"), position, new Vector2(0.5f, 0.5f), Vector2.One, Color.White * Ease.CubeOut(Alpha));

            if (!resetHeld)
                return;

            float num1 = Ease.CubeInOut(Calc.Min(1f, resetDelay / 0.2f));
            float num2 = Ease.SineOut(Calc.Min(1f, resetTime / 1.5f));
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * num1);
            float width = 480f;
            double x = (1920 - width) / 2;
            Draw.Rect(x, 530f, width, 20f, Color.White * 0.25f * num1);
            Draw.Rect(x, 530f, width * num2, 20f, Color.White * num1);
        }
    }
}
