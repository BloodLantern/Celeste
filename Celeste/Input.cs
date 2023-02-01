// Decompiled with JetBrains decompiler
// Type: Celeste.Input
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
    public static string OverrideInputPrefix = (string) null;
    private static Dictionary<Keys, string> keyNameLookup = new Dictionary<Keys, string>();
    private static Dictionary<Buttons, string> buttonNameLookup = new Dictionary<Buttons, string>();
    private static Dictionary<string, Dictionary<string, string>> guiPathLookup = new Dictionary<string, Dictionary<string, string>>();
    private static float[] rumbleStrengths = new float[4]
    {
      0.15f,
      0.4f,
      1f,
      0.05f
    };
    private static float[] rumbleLengths = new float[5]
    {
      0.1f,
      0.25f,
      0.5f,
      1f,
      2f
    };

    public static int Gamepad
    {
      get => Celeste.Input.gamepad;
      set
      {
        int num = Calc.Clamp(value, 0, MInput.GamePads.Length - 1);
        if (Celeste.Input.gamepad == num)
          return;
        Celeste.Input.gamepad = num;
        Celeste.Input.Initialize();
      }
    }

    public static void Initialize()
    {
      bool flag = false;
      if (Celeste.Input.MoveX != null)
        flag = Celeste.Input.MoveX.Inverted;
      Celeste.Input.Deregister();
      Celeste.Input.MoveX = new VirtualIntegerAxis(Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Celeste.Input.Gamepad, 0.3f);
      Celeste.Input.MoveX.Inverted = flag;
      Celeste.Input.MoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Celeste.Input.Gamepad, 0.7f);
      Celeste.Input.GliderMoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Celeste.Input.Gamepad, 0.3f);
      Celeste.Input.Aim = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpDashOnly, Settings.Instance.Down, Settings.Instance.DownDashOnly, Settings.Instance.Left, Settings.Instance.LeftDashOnly, Settings.Instance.Right, Settings.Instance.RightDashOnly, Celeste.Input.Gamepad, 0.25f);
      Celeste.Input.Aim.InvertedX = flag;
      Celeste.Input.Feather = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Celeste.Input.Gamepad, 0.25f);
      Celeste.Input.Feather.InvertedX = flag;
      Celeste.Input.Jump = new VirtualButton(Settings.Instance.Jump, Celeste.Input.Gamepad, 0.08f, 0.2f);
      Celeste.Input.Dash = new VirtualButton(Settings.Instance.Dash, Celeste.Input.Gamepad, 0.08f, 0.2f);
      Celeste.Input.Talk = new VirtualButton(Settings.Instance.Talk, Celeste.Input.Gamepad, 0.08f, 0.2f);
      Celeste.Input.Grab = new VirtualButton(Settings.Instance.Grab, Celeste.Input.Gamepad, 0.0f, 0.2f);
      Celeste.Input.CrouchDash = new VirtualButton(Settings.Instance.DemoDash, Celeste.Input.Gamepad, 0.08f, 0.2f);
      Binding left = new Binding();
      left.Add(Keys.A);
      left.Add(Buttons.RightThumbstickLeft);
      Binding right = new Binding();
      right.Add(Keys.D);
      right.Add(Buttons.RightThumbstickRight);
      Binding up = new Binding();
      up.Add(Keys.W);
      up.Add(Buttons.RightThumbstickUp);
      Binding down = new Binding();
      down.Add(Keys.S);
      down.Add(Buttons.RightThumbstickDown);
      Celeste.Input.MountainAim = new VirtualJoystick(up, down, left, right, Celeste.Input.Gamepad, 0.1f);
      Binding binding = new Binding();
      binding.Add(Keys.Escape);
      Celeste.Input.ESC = new VirtualButton(binding, Celeste.Input.Gamepad, 0.1f, 0.2f);
      Celeste.Input.Pause = new VirtualButton(Settings.Instance.Pause, Celeste.Input.Gamepad, 0.1f, 0.2f);
      Celeste.Input.QuickRestart = new VirtualButton(Settings.Instance.QuickRestart, Celeste.Input.Gamepad, 0.1f, 0.2f);
      Celeste.Input.MenuLeft = new VirtualButton(Settings.Instance.MenuLeft, Celeste.Input.Gamepad, 0.0f, 0.4f);
      Celeste.Input.MenuLeft.SetRepeat(0.4f, 0.1f);
      Celeste.Input.MenuRight = new VirtualButton(Settings.Instance.MenuRight, Celeste.Input.Gamepad, 0.0f, 0.4f);
      Celeste.Input.MenuRight.SetRepeat(0.4f, 0.1f);
      Celeste.Input.MenuUp = new VirtualButton(Settings.Instance.MenuUp, Celeste.Input.Gamepad, 0.0f, 0.4f);
      Celeste.Input.MenuUp.SetRepeat(0.4f, 0.1f);
      Celeste.Input.MenuDown = new VirtualButton(Settings.Instance.MenuDown, Celeste.Input.Gamepad, 0.0f, 0.4f);
      Celeste.Input.MenuDown.SetRepeat(0.4f, 0.1f);
      Celeste.Input.MenuJournal = new VirtualButton(Settings.Instance.Journal, Celeste.Input.Gamepad, 0.0f, 0.2f);
      Celeste.Input.MenuConfirm = new VirtualButton(Settings.Instance.Confirm, Celeste.Input.Gamepad, 0.0f, 0.2f);
      Celeste.Input.MenuCancel = new VirtualButton(Settings.Instance.Cancel, Celeste.Input.Gamepad, 0.0f, 0.2f);
    }

    public static void Deregister()
    {
      if (Celeste.Input.ESC != null)
        Celeste.Input.ESC.Deregister();
      if (Celeste.Input.Pause != null)
        Celeste.Input.Pause.Deregister();
      if (Celeste.Input.MenuLeft != null)
        Celeste.Input.MenuLeft.Deregister();
      if (Celeste.Input.MenuRight != null)
        Celeste.Input.MenuRight.Deregister();
      if (Celeste.Input.MenuUp != null)
        Celeste.Input.MenuUp.Deregister();
      if (Celeste.Input.MenuDown != null)
        Celeste.Input.MenuDown.Deregister();
      if (Celeste.Input.MenuConfirm != null)
        Celeste.Input.MenuConfirm.Deregister();
      if (Celeste.Input.MenuCancel != null)
        Celeste.Input.MenuCancel.Deregister();
      if (Celeste.Input.MenuJournal != null)
        Celeste.Input.MenuJournal.Deregister();
      if (Celeste.Input.QuickRestart != null)
        Celeste.Input.QuickRestart.Deregister();
      if (Celeste.Input.MoveX != null)
        Celeste.Input.MoveX.Deregister();
      if (Celeste.Input.MoveY != null)
        Celeste.Input.MoveY.Deregister();
      if (Celeste.Input.GliderMoveY != null)
        Celeste.Input.GliderMoveY.Deregister();
      if (Celeste.Input.Aim != null)
        Celeste.Input.Aim.Deregister();
      if (Celeste.Input.MountainAim != null)
        Celeste.Input.MountainAim.Deregister();
      if (Celeste.Input.Jump != null)
        Celeste.Input.Jump.Deregister();
      if (Celeste.Input.Dash != null)
        Celeste.Input.Dash.Deregister();
      if (Celeste.Input.Grab != null)
        Celeste.Input.Grab.Deregister();
      if (Celeste.Input.Talk != null)
        Celeste.Input.Talk.Deregister();
      if (Celeste.Input.CrouchDash == null)
        return;
      Celeste.Input.CrouchDash.Deregister();
    }

    public static bool AnyGamepadConfirmPressed(out int gamepadIndex)
    {
      bool flag = false;
      gamepadIndex = -1;
      int gamepadIndex1 = Celeste.Input.MenuConfirm.GamepadIndex;
      for (int index = 0; index < MInput.GamePads.Length; ++index)
      {
        Celeste.Input.MenuConfirm.GamepadIndex = index;
        if (Celeste.Input.MenuConfirm.Pressed)
        {
          flag = true;
          gamepadIndex = index;
          break;
        }
      }
      Celeste.Input.MenuConfirm.GamepadIndex = gamepadIndex1;
      return flag;
    }

    public static void Rumble(RumbleStrength strength, RumbleLength length)
    {
      float num = 1f;
      if (Settings.Instance.Rumble == RumbleAmount.Half)
        num = 0.5f;
      if (Settings.Instance.Rumble == RumbleAmount.Off || MInput.GamePads.Length == 0 || MInput.Disabled)
        return;
      MInput.GamePads[Celeste.Input.Gamepad].Rumble(Celeste.Input.rumbleStrengths[(int) strength] * num, Celeste.Input.rumbleLengths[(int) length]);
    }

    public static void RumbleSpecific(float strength, float time)
    {
      float num = 1f;
      if (Settings.Instance.Rumble == RumbleAmount.Half)
        num = 0.5f;
      if (Settings.Instance.Rumble == RumbleAmount.Off || MInput.GamePads.Length == 0 || MInput.Disabled)
        return;
      MInput.GamePads[Celeste.Input.Gamepad].Rumble(strength * num, time);
    }

    public static bool GrabCheck
    {
      get
      {
        switch (Settings.Instance.GrabMode)
        {
          case GrabModes.Invert:
            return !Celeste.Input.Grab.Check;
          case GrabModes.Toggle:
            return Celeste.Input.grabToggle;
          default:
            return Celeste.Input.Grab.Check;
        }
      }
    }

    public static bool DashPressed
    {
      get
      {
        switch (Settings.Instance.CrouchDashMode)
        {
          case CrouchDashModes.Hold:
            return Celeste.Input.Dash.Pressed && !Celeste.Input.CrouchDash.Check;
          default:
            return Celeste.Input.Dash.Pressed;
        }
      }
    }

    public static bool CrouchDashPressed
    {
      get
      {
        switch (Settings.Instance.CrouchDashMode)
        {
          case CrouchDashModes.Hold:
            return Celeste.Input.Dash.Pressed && Celeste.Input.CrouchDash.Check;
          default:
            return Celeste.Input.CrouchDash.Pressed;
        }
      }
    }

    public static void UpdateGrab()
    {
      if (Settings.Instance.GrabMode != GrabModes.Toggle || !Celeste.Input.Grab.Pressed)
        return;
      Celeste.Input.grabToggle = !Celeste.Input.grabToggle;
    }

    public static void ResetGrab() => Celeste.Input.grabToggle = false;

    public static Vector2 GetAimVector(Facings defaultFacing = Facings.Right)
    {
      Vector2 vector2 = Celeste.Input.Aim.Value;
      if (vector2 == Vector2.Zero)
      {
        if (SaveData.Instance != null && SaveData.Instance.Assists.DashAssist)
          return Celeste.Input.LastAim;
        Celeste.Input.LastAim = Vector2.UnitX * (float) defaultFacing;
      }
      else if (SaveData.Instance != null && SaveData.Instance.Assists.ThreeSixtyDashing)
      {
        Celeste.Input.LastAim = vector2.SafeNormalize();
      }
      else
      {
        float radiansA = vector2.Angle();
        float num = (float) (0.39269909262657166 - ((double) radiansA < 0.0 ? 1.0 : 0.0) * 0.0872664600610733);
        Celeste.Input.LastAim = (double) Calc.AbsAngleDiff(radiansA, 0.0f) >= (double) num ? ((double) Calc.AbsAngleDiff(radiansA, 3.14159274f) >= (double) num ? ((double) Calc.AbsAngleDiff(radiansA, -1.57079637f) >= (double) num ? ((double) Calc.AbsAngleDiff(radiansA, 1.57079637f) >= (double) num ? new Vector2((float) Math.Sign(vector2.X), (float) Math.Sign(vector2.Y)).SafeNormalize() : new Vector2(0.0f, 1f)) : new Vector2(0.0f, -1f)) : new Vector2(-1f, 0.0f)) : new Vector2(1f, 0.0f);
      }
      return Celeste.Input.LastAim;
    }

    public static string GuiInputPrefix(Celeste.Input.PrefixMode mode = Celeste.Input.PrefixMode.Latest)
    {
      if (!string.IsNullOrEmpty(Celeste.Input.OverrideInputPrefix))
        return Celeste.Input.OverrideInputPrefix;
      return (mode != Celeste.Input.PrefixMode.Latest ? MInput.GamePads[Celeste.Input.Gamepad].Attached : MInput.ControllerHasFocus) ? "xb1" : "keyboard";
    }

    public static bool GuiInputController(Celeste.Input.PrefixMode mode = Celeste.Input.PrefixMode.Latest) => !Celeste.Input.GuiInputPrefix(mode).Equals("keyboard");

    public static MTexture GuiButton(
      VirtualButton button,
      Celeste.Input.PrefixMode mode = Celeste.Input.PrefixMode.Latest,
      string fallback = "controls/keyboard/oemquestion")
    {
      string prefix = Celeste.Input.GuiInputPrefix(mode);
      int num = Celeste.Input.GuiInputController(mode) ? 1 : 0;
      string input = "";
      if (num != 0)
      {
        using (List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            Buttons current = enumerator.Current;
            if (!Celeste.Input.buttonNameLookup.TryGetValue(current, out input))
              Celeste.Input.buttonNameLookup.Add(current, input = current.ToString());
          }
        }
      }
      else
      {
        Keys key = Celeste.Input.FirstKey(button);
        if (!Celeste.Input.keyNameLookup.TryGetValue(key, out input))
          Celeste.Input.keyNameLookup.Add(key, input = key.ToString());
      }
      MTexture mtexture = Celeste.Input.GuiTexture(prefix, input);
      return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
    }

    public static MTexture GuiSingleButton(
      Buttons button,
      Celeste.Input.PrefixMode mode = Celeste.Input.PrefixMode.Latest,
      string fallback = "controls/keyboard/oemquestion")
    {
      string prefix = !Celeste.Input.GuiInputController(mode) ? "xb1" : Celeste.Input.GuiInputPrefix(mode);
      string input = "";
      if (!Celeste.Input.buttonNameLookup.TryGetValue(button, out input))
        Celeste.Input.buttonNameLookup.Add(button, input = button.ToString());
      MTexture mtexture = Celeste.Input.GuiTexture(prefix, input);
      return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
    }

    public static MTexture GuiKey(Keys key, string fallback = "controls/keyboard/oemquestion")
    {
      string input;
      if (!Celeste.Input.keyNameLookup.TryGetValue(key, out input))
        Celeste.Input.keyNameLookup.Add(key, input = key.ToString());
      MTexture mtexture = Celeste.Input.GuiTexture("keyboard", input);
      return mtexture == null && fallback != null ? GFX.Gui[fallback] : mtexture;
    }

    public static Buttons FirstButton(VirtualButton button)
    {
      using (List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator())
      {
        if (enumerator.MoveNext())
          return enumerator.Current;
      }
      return Buttons.A;
    }

    public static Keys FirstKey(VirtualButton button)
    {
      foreach (Keys keys in button.Binding.Keyboard)
      {
        if (keys != Keys.None)
          return keys;
      }
      return Keys.None;
    }

    public static MTexture GuiDirection(Vector2 direction) => Celeste.Input.GuiTexture("directions", Math.Sign(direction.X).ToString() + "x" + (object) Math.Sign(direction.Y));

    private static MTexture GuiTexture(string prefix, string input)
    {
      Dictionary<string, string> dictionary;
      if (!Celeste.Input.guiPathLookup.TryGetValue(prefix, out dictionary))
        Celeste.Input.guiPathLookup.Add(prefix, dictionary = new Dictionary<string, string>());
      string id;
      if (!dictionary.TryGetValue(input, out id))
        dictionary.Add(input, id = "controls/" + prefix + "/" + input);
      if (GFX.Gui.Has(id))
        return GFX.Gui[id];
      return prefix != "fallback" ? Celeste.Input.GuiTexture("fallback", input) : (MTexture) null;
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
