// Decompiled with JetBrains decompiler
// Type: Celeste.OuiMainMenu
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiMainMenu : Oui
    {
        private static readonly Vector2 TargetPosition = new(160f, 160f);
        private static readonly Vector2 TweenFrom = new(-500f, 160f);
        private static readonly Color UnselectedColor = Color.White;
        private static readonly Color SelectedColorA = TextMenu.HighlightColorA;
        private static readonly Color SelectedColorB = TextMenu.HighlightColorB;
        private const float IconWidth = 64f;
        private const float IconSpacing = 20f;
        private float ease;
        private MainMenuClimb climbButton;
        private readonly List<MenuButton> buttons;
        private bool startOnOptions;
        private bool mountainStartFront;

        public OuiMainMenu()
        {
            buttons = new List<MenuButton>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position = OuiMainMenu.TweenFrom;
            CreateButtons();
        }

        public void CreateButtons()
        {
            foreach (Entity button in buttons)
            {
                button.RemoveSelf();
            }

            buttons.Clear();
            Vector2 targetPosition = new(320f, 160f);
            Vector2 vector2 = new(-640f, 0.0f);
            climbButton = new MainMenuClimb(this, targetPosition, targetPosition + vector2, new Action(OnBegin));
            if (!startOnOptions)
            {
                climbButton.StartSelected();
            }

            buttons.Add(climbButton);
            targetPosition += Vector2.UnitY * climbButton.ButtonHeight;
            targetPosition.X -= 140f;
            if (Celeste.PlayMode == Celeste.PlayModes.Debug)
            {
                MainMenuSmallButton mainMenuSmallButton = new("menu_debug", "menu/options", this, targetPosition, targetPosition + vector2, new Action(OnDebug));
                buttons.Add(mainMenuSmallButton);
                targetPosition += Vector2.UnitY * mainMenuSmallButton.ButtonHeight;
            }
            if (Settings.Instance.Pico8OnMainMenu || Celeste.PlayMode == Celeste.PlayModes.Debug || Celeste.PlayMode == Celeste.PlayModes.Event)
            {
                MainMenuSmallButton mainMenuSmallButton = new("menu_pico8", "menu/pico8", this, targetPosition, targetPosition + vector2, new Action(OnPico8));
                buttons.Add(mainMenuSmallButton);
                targetPosition += Vector2.UnitY * mainMenuSmallButton.ButtonHeight;
            }
            MainMenuSmallButton mainMenuSmallButton1 = new("menu_options", "menu/options", this, targetPosition, targetPosition + vector2, new Action(OnOptions));
            if (startOnOptions)
            {
                mainMenuSmallButton1.StartSelected();
            }

            buttons.Add(mainMenuSmallButton1);
            targetPosition += Vector2.UnitY * mainMenuSmallButton1.ButtonHeight;
            MainMenuSmallButton mainMenuSmallButton2 = new("menu_credits", "menu/credits", this, targetPosition, targetPosition + vector2, new Action(OnCredits));
            buttons.Add(mainMenuSmallButton2);
            targetPosition += Vector2.UnitY * mainMenuSmallButton2.ButtonHeight;
            MainMenuSmallButton mainMenuSmallButton3 = new("menu_exit", "menu/exit", this, targetPosition, targetPosition + vector2, new Action(OnExit));
            buttons.Add(mainMenuSmallButton3);
            targetPosition += Vector2.UnitY * mainMenuSmallButton3.ButtonHeight;
            for (int index = 0; index < buttons.Count; ++index)
            {
                if (index > 0)
                {
                    buttons[index].UpButton = buttons[index - 1];
                }

                if (index < buttons.Count - 1)
                {
                    buttons[index].DownButton = buttons[index + 1];
                }

                Scene.Add(buttons[index]);
            }
            if (!Visible || !Focused)
            {
                return;
            }

            foreach (MenuButton button in buttons)
            {
                button.Position = button.TargetPosition;
            }
        }

        public override void Removed(Scene scene)
        {
            foreach (MenuButton button in buttons)
            {
                scene.Remove(button);
            }

            base.Removed(scene);
        }

        public override bool IsStart(Overworld overworld, Overworld.StartMode start)
        {
            if (start == Overworld.StartMode.ReturnFromOptions)
            {
                startOnOptions = true;
                Add(new Coroutine(Enter(null)));
                return true;
            }
            if (start == Overworld.StartMode.MainMenu)
            {
                mountainStartFront = true;
                Add(new Coroutine(Enter(null)));
                return true;
            }
            return start is Overworld.StartMode.ReturnFromOptions or Overworld.StartMode.ReturnFromPico8;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiMainMenu ouiMainMenu = this;
            if (from is OuiTitleScreen or OuiFileSelect)
            {
                _ = Audio.Play("event:/ui/main/whoosh_list_in");
                yield return 0.1f;
            }
            if (from is OuiTitleScreen)
            {
                MenuButton.ClearSelection(ouiMainMenu.Scene);
                ouiMainMenu.climbButton.StartSelected();
            }
            ouiMainMenu.Visible = true;
            if (ouiMainMenu.mountainStartFront)
            {
                ouiMainMenu.Overworld.Mountain.SnapCamera(-1, new MountainCamera(new Vector3(0.0f, 6f, 12f), MountainRenderer.RotateLookAt));
            }

            ouiMainMenu.Overworld.Mountain.GotoRotationMode();
            ouiMainMenu.Overworld.Maddy.Hide();
            foreach (MenuButton button in ouiMainMenu.buttons)
            {
                button.TweenIn(0.2f);
            }

            yield return 0.2f;
            ouiMainMenu.Focused = true;
            ouiMainMenu.mountainStartFront = false;
            yield return null;
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiMainMenu ouiMainMenu = this;
            ouiMainMenu.Focused = false;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.2f, true);
            // ISSUE: reference to a compiler-generated method
            tween.OnUpdate = delegate (Tween t)
            {
                ease = 1f - t.Eased;
                Position = Vector2.Lerp(OuiMainMenu.TargetPosition, OuiMainMenu.TweenFrom, t.Eased);
            };
            ouiMainMenu.Add(tween);
            bool keepClimb = ouiMainMenu.climbButton.Selected && next is not OuiTitleScreen;
            foreach (MenuButton button in ouiMainMenu.buttons)
            {
                if (!(button == ouiMainMenu.climbButton & keepClimb))
                {
                    button.TweenOut(0.2f);
                }
            }
            yield return 0.2f;
            if (keepClimb)
            {
                ouiMainMenu.Add(new Coroutine(ouiMainMenu.SlideClimbOutLate()));
            }
            else
            {
                ouiMainMenu.Visible = false;
            }
        }

        private IEnumerator SlideClimbOutLate()
        {
            OuiMainMenu ouiMainMenu = this;
            yield return 0.2f;
            ouiMainMenu.climbButton.TweenOut(0.2f);
            yield return 0.2f;
            ouiMainMenu.Visible = false;
        }

        public Color SelectionColor => !Settings.Instance.DisableFlashes && !Scene.BetweenInterval(0.1f) ? OuiMainMenu.SelectedColorB : OuiMainMenu.SelectedColorA;

        public override void Update()
        {
            if (Selected && Focused && Input.MenuCancel.Pressed)
            {
                Focused = false;
                _ = Audio.Play("event:/ui/main/whoosh_list_out");
                _ = Audio.Play("event:/ui/main/button_back");
                _ = Overworld.Goto<OuiTitleScreen>();
            }
            base.Update();
        }

        public override void Render()
        {
            foreach (MenuButton button in buttons)
            {
                if (button.Scene == Scene)
                {
                    button.Render();
                }
            }
        }

        private void OnDebug()
        {
            _ = Audio.Play("event:/ui/main/whoosh_list_out");
            _ = Audio.Play("event:/ui/main/button_select");
            SaveData.InitializeDebugMode();
            _ = Overworld.Goto<OuiChapterSelect>();
        }

        private void OnBegin()
        {
            _ = Audio.Play("event:/ui/main/whoosh_list_out");
            _ = Audio.Play("event:/ui/main/button_climb");
            if (Celeste.PlayMode == Celeste.PlayModes.Event)
            {
                SaveData.InitializeDebugMode(false);
                _ = Overworld.Goto<OuiChapterSelect>();
            }
            else
            {
                _ = Overworld.Goto<OuiFileSelect>();
            }
        }

        private void OnPico8()
        {
            _ = Audio.Play("event:/ui/main/button_select");
            Focused = false;
            FadeWipe fadeWipe = new(Scene, false, () =>
            {
                Focused = true;
                Overworld.EnteringPico8 = true;
                SaveData.Instance = null;
                SaveData.NoFileAssistChecks();
                Engine.Scene = new Emulator(Overworld);
            });
        }

        private void OnOptions()
        {
            _ = Audio.Play("event:/ui/main/button_select");
            _ = Audio.Play("event:/ui/main/whoosh_large_in");
            _ = Overworld.Goto<OuiOptions>();
        }

        private void OnCredits()
        {
            _ = Audio.Play("event:/ui/main/button_select");
            _ = Audio.Play("event:/ui/main/whoosh_large_in");
            _ = Overworld.Goto<OuiCredits>();
        }

        private void OnExit()
        {
            _ = Audio.Play("event:/ui/main/button_select");
            Focused = false;
            FadeWipe fadeWipe = new(Scene, false, () =>
            {
                Engine.Scene = new Scene();
                Engine.Instance.Exit();
            });
        }
    }
}
