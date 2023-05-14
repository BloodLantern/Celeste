﻿// Decompiled with JetBrains decompiler
// Type: Celeste.MenuOptions
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Monocle;
using System;

namespace Celeste
{
    public static class MenuOptions
    {
        private static TextMenu menu;
        private static bool inGame;
        private static TextMenu.Item crouchDashMode;
        private static TextMenu.Item window;
        private static TextMenu.Item viewport;
        private static EventInstance snapshot;

        public static TextMenu Create(bool inGame = false, EventInstance snapshot = null)
        {
            MenuOptions.inGame = inGame;
            MenuOptions.snapshot = snapshot;
            MenuOptions.menu = new TextMenu
            {
                new TextMenu.Header(Dialog.Clean("options_title"))
            };
            MenuOptions.menu.OnClose = () => MenuOptions.crouchDashMode = null;
            if (!inGame && Dialog.Languages.Count > 1)
            {
                _ = MenuOptions.menu.Add(new TextMenu.SubHeader(""));
                TextMenu.LanguageButton languageButton = new(Dialog.Clean("options_language"), Dialog.Language);
                _ = languageButton.Pressed(new Action(MenuOptions.SelectLanguage));
                _ = MenuOptions.menu.Add(languageButton);
            }
            _ = MenuOptions.menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_controls")));
            MenuOptions.CreateRumble(MenuOptions.menu);
            MenuOptions.CreateGrabMode(MenuOptions.menu);
            MenuOptions.crouchDashMode = MenuOptions.CreateCrouchDashMode(MenuOptions.menu);
            _ = MenuOptions.menu.Add(new TextMenu.Button(Dialog.Clean("options_keyconfig")).Pressed(new Action(MenuOptions.OpenKeyboardConfig)));
            _ = MenuOptions.menu.Add(new TextMenu.Button(Dialog.Clean("options_btnconfig")).Pressed(new Action(MenuOptions.OpenButtonConfig)));
            _ = MenuOptions.menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_video")));
            _ = MenuOptions.menu.Add(new TextMenu.OnOff(Dialog.Clean("options_fullscreen"), Settings.Instance.Fullscreen).Change(new Action<bool>(MenuOptions.SetFullscreen)));
            TextMenu menu = MenuOptions.menu;
            string label = Dialog.Clean("options_window");
            int windowScale = Settings.Instance.WindowScale;
            TextMenu.Option<int> option;
            MenuOptions.window = option = new TextMenu.Slider(label, i => i.ToString() + "x", 3, 10, windowScale).Change(new Action<int>(MenuOptions.SetWindow));
            _ = menu.Add(option);
            _ = MenuOptions.menu.Add(new TextMenu.OnOff(Dialog.Clean("options_vsync"), Settings.Instance.VSync).Change(new Action<bool>(MenuOptions.SetVSync)));
            _ = MenuOptions.menu.Add(new TextMenu.OnOff(Dialog.Clean("OPTIONS_DISABLE_FLASH"), Settings.Instance.DisableFlashes).Change(b => Settings.Instance.DisableFlashes = b));
            _ = MenuOptions.menu.Add(new TextMenu.Slider(Dialog.Clean("OPTIONS_DISABLE_SHAKE"), i =>
            {
                return i == 2 ? Dialog.Clean("OPTIONS_RUMBLE_ON") : i == 1 ? Dialog.Clean("OPTIONS_RUMBLE_HALF") : Dialog.Clean("OPTIONS_RUMBLE_OFF");
            }, 0, 2, (int)Settings.Instance.ScreenShake).Change(i => Settings.Instance.ScreenShake = (ScreenshakeAmount)i));
            _ = MenuOptions.menu.Add(MenuOptions.viewport = new TextMenu.Button(Dialog.Clean("OPTIONS_VIEWPORT_PC")).Pressed(new Action(MenuOptions.OpenViewportAdjustment)));
            _ = MenuOptions.menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_audio")));
            _ = MenuOptions.menu.Add(new TextMenu.Slider(Dialog.Clean("options_music"), i => i.ToString(), 0, 10, Settings.Instance.MusicVolume).Change(new Action<int>(MenuOptions.SetMusic)).Enter(new Action(MenuOptions.EnterSound)).Leave(new Action(MenuOptions.LeaveSound)));
            _ = MenuOptions.menu.Add(new TextMenu.Slider(Dialog.Clean("options_sounds"), i => i.ToString(), 0, 10, Settings.Instance.SFXVolume).Change(new Action<int>(MenuOptions.SetSfx)).Enter(new Action(MenuOptions.EnterSound)).Leave(new Action(MenuOptions.LeaveSound)));
            _ = MenuOptions.menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_gameplay")));
            _ = MenuOptions.menu.Add(new TextMenu.Slider(Dialog.Clean("options_speedrun"), i =>
            {
                return i == 0 ? Dialog.Get("OPTIONS_OFF") : i == 1 ? Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER") : Dialog.Get("OPTIONS_SPEEDRUN_FILE");
            }, 0, 2, (int)Settings.Instance.SpeedrunClock).Change(new Action<int>(MenuOptions.SetSpeedrunClock)));
            MenuOptions.viewport.Visible = Settings.Instance.Fullscreen;
            if (MenuOptions.window != null)
            {
                MenuOptions.window.Visible = !Settings.Instance.Fullscreen;
            }

            if ((double)MenuOptions.menu.Height > (double)MenuOptions.menu.ScrollableMinSize)
            {
                MenuOptions.menu.Position.Y = MenuOptions.menu.ScrollTargetY;
            }

            return MenuOptions.menu;
        }

        private static void CreateRumble(TextMenu menu)
        {
            _ = menu.Add(new TextMenu.Slider(Dialog.Clean("options_rumble_PC"), i =>
        {
            return i == 2 ? Dialog.Clean("OPTIONS_RUMBLE_ON") : i == 1 ? Dialog.Clean("OPTIONS_RUMBLE_HALF") : Dialog.Clean("OPTIONS_RUMBLE_OFF");
        }, 0, 2, (int)Settings.Instance.Rumble).Change(i =>
        {
            Settings.Instance.Rumble = (RumbleAmount)i;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }));
        }

        private static void CreateGrabMode(TextMenu menu)
        {
            _ = menu.Add(new TextMenu.Slider(Dialog.Clean("OPTIONS_GRAB_MODE"), i =>
        {
            return i == 0
                ? Dialog.Clean("OPTIONS_BUTTON_HOLD")
                : i == 1 ? Dialog.Clean("OPTIONS_BUTTON_INVERT") : Dialog.Clean("OPTIONS_BUTTON_TOGGLE");
        }, 0, 2, (int)Settings.Instance.GrabMode).Change(i =>
        {
            Settings.Instance.GrabMode = (GrabModes)i;
            Input.ResetGrab();
        }));
        }

        private static TextMenu.Item CreateCrouchDashMode(TextMenu menu)
        {
            TextMenu.Option<int> crouchDashMode = new TextMenu.Slider(Dialog.Clean("OPTIONS_CROUCH_DASH_MODE"), i => i == 0 ? Dialog.Clean("OPTIONS_BUTTON_PRESS") : Dialog.Clean("OPTIONS_BUTTON_HOLD"), 0, 1, (int)Settings.Instance.CrouchDashMode).Change(i => Settings.Instance.CrouchDashMode = (CrouchDashModes)i);
            crouchDashMode.Visible = Input.CrouchDash.Binding.HasInput;
            _ = menu.Add(crouchDashMode);
            return crouchDashMode;
        }

        private static void SetFullscreen(bool on)
        {
            Settings.Instance.Fullscreen = on;
            Settings.Instance.ApplyScreen();
            if (MenuOptions.window != null)
            {
                MenuOptions.window.Visible = !on;
            }

            if (MenuOptions.viewport == null)
            {
                return;
            }

            MenuOptions.viewport.Visible = on;
        }

        private static void SetVSync(bool on)
        {
            Settings.Instance.VSync = on;
            Engine.Graphics.SynchronizeWithVerticalRetrace = Settings.Instance.VSync;
            Engine.Graphics.ApplyChanges();
        }

        private static void SetWindow(int scale)
        {
            Settings.Instance.WindowScale = scale;
            Settings.Instance.ApplyScreen();
        }

        private static void SetMusic(int volume)
        {
            Settings.Instance.MusicVolume = volume;
            Settings.Instance.ApplyMusicVolume();
        }

        private static void SetSfx(int volume)
        {
            Settings.Instance.SFXVolume = volume;
            Settings.Instance.ApplySFXVolume();
        }

        private static void SetSpeedrunClock(int val)
        {
            Settings.Instance.SpeedrunClock = (SpeedrunType)val;
        }

        private static void OpenViewportAdjustment()
        {
            if (Engine.Scene is Overworld)
            {
                (Engine.Scene as Overworld).ShowInputUI = false;
            }

            MenuOptions.menu.Visible = false;
            MenuOptions.menu.Focused = false;
            Engine.Scene.Add(new ViewportAdjustmentUI()
            {
                OnClose = () =>
                {
                    MenuOptions.menu.Visible = true;
                    MenuOptions.menu.Focused = true;
                    if (Engine.Scene is not Overworld)
                    {
                        return;
                    } (Engine.Scene as Overworld).ShowInputUI = true;
                }
            });
            Engine.Scene.OnEndOfFrame += Engine.Scene.Entities.UpdateLists;
        }

        private static void SelectLanguage()
        {
            MenuOptions.menu.Focused = false;
            LanguageSelectUI languageSelectUi = new()
            {
                OnClose = () => MenuOptions.menu.Focused = true
            };
            Engine.Scene.Add(languageSelectUi);
            Engine.Scene.OnEndOfFrame += Engine.Scene.Entities.UpdateLists;
        }

        private static void OpenKeyboardConfig()
        {
            MenuOptions.menu.Focused = false;
            KeyboardConfigUI keyboardConfigUi = new()
            {
                OnClose = () => MenuOptions.menu.Focused = true
            };
            Engine.Scene.Add(keyboardConfigUi);
            Engine.Scene.OnEndOfFrame += Engine.Scene.Entities.UpdateLists;
        }

        private static void OpenButtonConfig()
        {
            MenuOptions.menu.Focused = false;
            if (Engine.Scene is Overworld)
            {
                (Engine.Scene as Overworld).ShowConfirmUI = false;
            }

            ButtonConfigUI buttonConfigUi = new()
            {
                OnClose = () =>
            {
                MenuOptions.menu.Focused = true;
                if (Engine.Scene is not Overworld)
                {
                    return;
                } (Engine.Scene as Overworld).ShowConfirmUI = true;
            }
            };
            Engine.Scene.Add(buttonConfigUi);
            Engine.Scene.OnEndOfFrame += Engine.Scene.Entities.UpdateLists;
        }

        private static void EnterSound()
        {
            if (!MenuOptions.inGame || !(snapshot != null))
            {
                return;
            }

            Audio.EndSnapshot(MenuOptions.snapshot);
        }

        private static void LeaveSound()
        {
            if (!MenuOptions.inGame || !(snapshot != null))
            {
                return;
            }

            Audio.ResumeSnapshot(MenuOptions.snapshot);
        }

        public static void UpdateCrouchDashModeVisibility()
        {
            if (MenuOptions.crouchDashMode == null)
            {
                return;
            }

            MenuOptions.crouchDashMode.Visible = Input.CrouchDash.Binding.HasInput;
        }
    }
}
