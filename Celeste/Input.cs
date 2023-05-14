// Decompiled with JetBrains decompiler
// Type: Input
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public static class Input
    {
        private static int gamepad = 0;
        public static readonly int MaxBindings = 8;
        public static VirtualButton ESC;
        public static VirtualButton Pause;
        public static VirtualButton MenuLeft;
        public static VirtualButton MenuRight;
        public static VirtualButton MenuUp;
        public static VirtualButton MenuDown;
        public static VirtualButton MenuConfirm;
        public static VirtualButton MenuCancel;
        public static VirtualButton MenuJournal;
        public static VirtualButton QuickRestart;
        public static VirtualIntegerAxis MoveX;
        public static VirtualIntegerAxis MoveY;
        public static VirtualIntegerAxis GliderMoveY;
        public static VirtualJoystick Aim;
        public static VirtualJoystick Feather;
        public static VirtualJoystick MountainAim;
        public static VirtualButton Jump;
        public static VirtualButton Dash;
        public static VirtualButton Grab;
        public static VirtualButton Talk;
        public static VirtualButton CrouchDash;
        private static bool grabToggle;
        public static Vector2 LastAim;
        public static string OverrideInputPrefix = null;
        private static readonly Dictionary<Keys, string> keyNameLookup = new();
        private static readonly Dictionary<Buttons, string> buttonNameLookup = new();
        private static readonly Dictionary<string, Dictionary<string, string>> guiPathLookup = new();
        private static readonly float[] rumbleStrengths = new float[4]
        {
            0.15f,
            0.4f,
            1f,
            0.05f
        };
        private static readonly float[] rumbleLengths = new float[5]
        {
            0.1f,
            0.25f,
            0.5f,
            1f,
            2f
        };

        public static int Gamepad
        {
            get => Input.gamepad;
            set
            {
                int num = Calc.Clamp(value, 0, MInput.GamePads.Length - 1);
                if (Input.gamepad == num)
                {
                    return;
                }

                Input.gamepad = num;
                Input.Initialize();
            }
        }

        public static void Initialize()
        {
            bool flag = false;
            if (Input.MoveX != null)
            {
                flag = Input.MoveX.Inverted;
            }

            Input.Deregister();
            Input.MoveX = new VirtualIntegerAxis(Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Input.Gamepad, 0.3f)
            {
                Inverted = flag
            };
            Input.MoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Input.Gamepad, 0.7f);
            Input.GliderMoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Input.Gamepad, 0.3f);
            Input.Aim = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpDashOnly, Settings.Instance.Down, Settings.Instance.DownDashOnly, Settings.Instance.Left, Settings.Instance.LeftDashOnly, Settings.Instance.Right, Settings.Instance.RightDashOnly, Input.Gamepad, 0.25f)
            {
                InvertedX = flag
            };
            Input.Feather = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Input.Gamepad, 0.25f)
            {
                InvertedX = flag
            };
            Input.Jump = new VirtualButton(Settings.Instance.Jump, Input.Gamepad, 0.08f, 0.2f);
            Input.Dash = new VirtualButton(Settings.Instance.Dash, Input.Gamepad, 0.08f, 0.2f);
            Input.Talk = new VirtualButton(Settings.Instance.Talk, Input.Gamepad, 0.08f, 0.2f);
            Input.Grab = new VirtualButton(Settings.Instance.Grab, Input.Gamepad, 0.0f, 0.2f);
            Input.CrouchDash = new VirtualButton(Settings.Instance.DemoDash, Input.Gamepad, 0.08f, 0.2f);
            Binding left = new();
            _ = left.Add(Keys.A);
            _ = left.Add(Buttons.RightThumbstickLeft);
            Binding right = new();
            _ = right.Add(Keys.D);
            _ = right.Add(Buttons.RightThumbstickRight);
            Binding up = new();
            _ = up.Add(Keys.W);
            _ = up.Add(Buttons.RightThumbstickUp);
            Binding down = new();
            _ = down.Add(Keys.S);
            _ = down.Add(Buttons.RightThumbstickDown);
            Input.MountainAim = new VirtualJoystick(up, down, left, right, Input.Gamepad, 0.1f);
            Binding binding = new();
            _ = binding.Add(Keys.Escape);
            Input.ESC = new VirtualButton(binding, Input.Gamepad, 0.1f, 0.2f);
            Input.Pause = new VirtualButton(Settings.Instance.Pause, Input.Gamepad, 0.1f, 0.2f);
            Input.QuickRestart = new VirtualButton(Settings.Instance.QuickRestart, Input.Gamepad, 0.1f, 0.2f);
            Input.MenuLeft = new VirtualButton(Settings.Instance.MenuLeft, Input.Gamepad, 0.0f, 0.4f);
            Input.MenuLeft.SetRepeat(0.4f, 0.1f);
            Input.MenuRight = new VirtualButton(Settings.Instance.MenuRight, Input.Gamepad, 0.0f, 0.4f);
            Input.MenuRight.SetRepeat(0.4f, 0.1f);
            Input.MenuUp = new VirtualButton(Settings.Instance.MenuUp, Input.Gamepad, 0.0f, 0.4f);
            Input.MenuUp.SetRepeat(0.4f, 0.1f);
            Input.MenuDown = new VirtualButton(Settings.Instance.MenuDown, Input.Gamepad, 0.0f, 0.4f);
            Input.MenuDown.SetRepeat(0.4f, 0.1f);
            Input.MenuJournal = new VirtualButton(Settings.Instance.Journal, Input.Gamepad, 0.0f, 0.2f);
            Input.MenuConfirm = new VirtualButton(Settings.Instance.Confirm, Input.Gamepad, 0.0f, 0.2f);
            Input.MenuCancel = new VirtualButton(Settings.Instance.Cancel, Input.Gamepad, 0.0f, 0.2f);
        }

        public static void Deregister()
        {
            Input.ESC?.Deregister();
            Input.Pause?.Deregister();
            Input.MenuLeft?.Deregister();
            Input.MenuRight?.Deregister();
            Input.MenuUp?.Deregister();
            Input.MenuDown?.Deregister();
            Input.MenuConfirm?.Deregister();
            Input.MenuCancel?.Deregister();
            Input.MenuJournal?.Deregister();
            Input.QuickRestart?.Deregister();
            Input.MoveX?.Deregister();
            Input.MoveY?.Deregister();
            Input.GliderMoveY?.Deregister();
            Input.Aim?.Deregister();
            Input.MountainAim?.Deregister();
            Input.Jump?.Deregister();
            Input.Dash?.Deregister();
            Input.Grab?.Deregister();
            Input.Talk?.Deregister();
            if (Input.CrouchDash == null)
            {
                return;
            }

            Input.CrouchDash.Deregister();
        }

        public static bool AnyGamepadConfirmPressed(out int gamepadIndex)
        {
            bool flag = false;
            gamepadIndex = -1;
            int gamepadIndex1 = Input.MenuConfirm.GamepadIndex;
            for (int index = 0; index < MInput.GamePads.Length; ++index)
            {
                Input.MenuConfirm.GamepadIndex = index;
                if (Input.MenuConfirm.Pressed)
                {
                    flag = true;
                    gamepadIndex = index;
                    break;
                }
            }
            Input.MenuConfirm.GamepadIndex = gamepadIndex1;
            return flag;
        }

        public static void Rumble(RumbleStrength strength, RumbleLength length)
        {
            float num = 1f;
            if (Settings.Instance.Rumble == RumbleAmount.Half)
            {
                num = 0.5f;
            }

            if (Settings.Instance.Rumble == RumbleAmount.Off || MInput.GamePads.Length == 0 || MInput.Disabled)
            {
                return;
            }

            MInput.GamePads[Input.Gamepad].Rumble(Input.rumbleStrengths[(int)strength] * num, Input.rumbleLengths[(int)length]);
        }

        public static void RumbleSpecific(float strength, float time)
        {
            float num = 1f;
            if (Settings.Instance.Rumble == RumbleAmount.Half)
            {
                num = 0.5f;
            }

            if (Settings.Instance.Rumble == RumbleAmount.Off || MInput.GamePads.Length == 0 || MInput.Disabled)
            {
                return;
            }

            MInput.GamePads[Input.Gamepad].Rumble(strength * num, time);
        }

        public static bool GrabCheck => Settings.Instance.GrabMode switch
        {
            GrabModes.Invert => !Input.Grab.Check,
            GrabModes.Toggle => Input.grabToggle,
            _ => Input.Grab.Check,
        };

        public static bool DashPressed => Settings.Instance.CrouchDashMode switch
        {
            CrouchDashModes.Hold => Input.Dash.Pressed && !Input.CrouchDash.Check,
            _ => Input.Dash.Pressed,
        };

        public static bool CrouchDashPressed => Settings.Instance.CrouchDashMode switch
        {
            CrouchDashModes.Hold => Input.Dash.Pressed && Input.CrouchDash.Check,
            _ => Input.CrouchDash.Pressed,
        };

        public static void UpdateGrab()
        {
            if (Settings.Instance.GrabMode != GrabModes.Toggle || !Input.Grab.Pressed)
            {
                return;
            }

            Input.grabToggle = !Input.grabToggle;
        }

        public static void ResetGrab()
        {
            Input.grabToggle = false;
        }

        public static Vector2 GetAimVector(Facings defaultFacing = Facings.Right)
        {
            Vector2 vector2 = Input.Aim.Value;
            if (vector2 == Vector2.Zero)
            {
                if (SaveData.Instance != null && SaveData.Instance.Assists.DashAssist)
                {
                    return Input.LastAim;
                }

                Input.LastAim = Vector2.UnitX * (float)defaultFacing;
            }
            else if (SaveData.Instance != null && SaveData.Instance.Assists.ThreeSixtyDashing)
            {
                Input.LastAim = vector2.SafeNormalize();
            }
            else
            {
                float radiansA = vector2.Angle();
                float num = (float)(0.39269909262657166 - (((double)radiansA < 0.0 ? 1.0 : 0.0) * 0.0872664600610733));
                Input.LastAim = (double)Calc.AbsAngleDiff(radiansA, 0.0f) >= (double)num ? ((double)Calc.AbsAngleDiff(radiansA, 3.14159274f) >= (double)num ? ((double)Calc.AbsAngleDiff(radiansA, -1.57079637f) >= (double)num ? ((double)Calc.AbsAngleDiff(radiansA, 1.57079637f) >= (double)num ? new Vector2(Math.Sign(vector2.X), Math.Sign(vector2.Y)).SafeNormalize() : new Vector2(0.0f, 1f)) : new Vector2(0.0f, -1f)) : new Vector2(-1f, 0.0f)) : new Vector2(1f, 0.0f);
            }
            return Input.LastAim;
        }

        public static string GuiInputPrefix(Input.PrefixMode mode = Input.PrefixMode.Latest)
        {
            return !string.IsNullOrEmpty(Input.OverrideInputPrefix)
                ? Input.OverrideInputPrefix
                : (mode != Input.PrefixMode.Latest ? MInput.GamePads[Input.Gamepad].Attached : MInput.ControllerHasFocus) ? "xb1" : "keyboard";
        }

        public static bool GuiInputController(Input.PrefixMode mode = Input.PrefixMode.Latest)
        {
            return !Input.GuiInputPrefix(mode).Equals("keyboard");
        }

        public static MTexture GuiButton(
            VirtualButton button,
            Input.PrefixMode mode = Input.PrefixMode.Latest,
            string fallback = "controls/keyboard/oemquestion")
        {
            string prefix = Input.GuiInputPrefix(mode);
            int num = Input.GuiInputController(mode) ? 1 : 0;
            string input = "";
            if (num != 0)
            {
                using List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    Buttons current = enumerator.Current;
                    if (!Input.buttonNameLookup.TryGetValue(current, out input))
                    {
                        Input.buttonNameLookup.Add(current, input = current.ToString());
                    }
                }
            }
            else
            {
                Keys key = Input.FirstKey(button);
                if (!Input.keyNameLookup.TryGetValue(key, out input))
                {
                    Input.keyNameLookup.Add(key, input = key.ToString());
                }
            }
            MTexture mtexture = Input.GuiTexture(prefix, input);
            return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
        }

        public static MTexture GuiSingleButton(
            Buttons button,
            Input.PrefixMode mode = Input.PrefixMode.Latest,
            string fallback = "controls/keyboard/oemquestion")
        {
            string prefix = !Input.GuiInputController(mode) ? "xb1" : Input.GuiInputPrefix(mode);
            if (!Input.buttonNameLookup.TryGetValue(button, out string input))
            {
                Input.buttonNameLookup.Add(button, input = button.ToString());
            }

            MTexture mtexture = Input.GuiTexture(prefix, input);
            return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
        }

        public static MTexture GuiKey(Keys key, string fallback = "controls/keyboard/oemquestion")
        {
            if (!Input.keyNameLookup.TryGetValue(key, out string input))
            {
                Input.keyNameLookup.Add(key, input = key.ToString());
            }

            MTexture mtexture = Input.GuiTexture("keyboard", input);
            return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
        }

        public static Buttons FirstButton(VirtualButton button)
        {
            using List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : Buttons.A;
        }

        public static Keys FirstKey(VirtualButton button)
        {
            foreach (Keys keys in button.Binding.Keyboard)
            {
                if (keys != Keys.None)
                {
                    return keys;
                }
            }
            return Keys.None;
        }

        public static MTexture GuiDirection(Vector2 direction)
        {
            return Input.GuiTexture("directions", Math.Sign(direction.X).ToString() + "x" + Math.Sign(direction.Y));
        }

        private static MTexture GuiTexture(string prefix, string input)
        {
            if (!Input.guiPathLookup.TryGetValue(prefix, out Dictionary<string, string> dictionary))
            {
                Input.guiPathLookup.Add(prefix, dictionary = new Dictionary<string, string>());
            }

            if (!dictionary.TryGetValue(input, out string id))
            {
                dictionary.Add(input, id = "controls/" + prefix + "/" + input);
            }

            return GFX.Gui.Has(id) ? GFX.Gui[id] : prefix != "fallback" ? Input.GuiTexture("fallback", input) : null;
        }

        public static void SetLightbarColor(Color color)
        {
        }

        public enum PrefixMode
        {
            Latest,
            Attached,
        }
    }
}
