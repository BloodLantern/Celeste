using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiChapterSelect : Oui
    {
        private List<OuiChapterSelectIcon> icons = new List<OuiChapterSelectIcon>();
        private int indexToSnap = -1;
        private const int scarfSegmentSize = 2;
        private MTexture scarf = GFX.Gui["areas/hover"];
        private MTexture[] scarfSegments;
        private float ease;
        private float journalEase;
        private bool journalEnabled;
        private bool disableInput;
        private bool display;
        private float inputDelay;
        private bool autoAdvancing;

        private int area
        {
            get => SaveData.Instance.LastArea.ID;
            set => SaveData.Instance.LastArea.ID = value;
        }

        public override bool IsStart(Overworld overworld, Overworld.StartMode start)
        {
            if (start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit)
                indexToSnap = area;
            return false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int count = AreaData.Areas.Count;
            for (int index = 0; index < count; ++index)
            {
                MTexture front = GFX.Gui[AreaData.Areas[index].Icon];
                MTexture back = GFX.Gui.Has(AreaData.Areas[index].Icon + "_back") ? GFX.Gui[AreaData.Areas[index].Icon + "_back"] : front;
                icons.Add(new OuiChapterSelectIcon(index, front, back));
                Scene.Add(icons[index]);
            }
            scarfSegments = new MTexture[scarf.Height / 2];
            for (int index = 0; index < scarfSegments.Length; ++index)
                scarfSegments[index] = scarf.GetSubtexture(0, index * 2, scarf.Width, 2);
            if (indexToSnap >= 0)
            {
                area = indexToSnap;
                icons[indexToSnap].SnapToSelected();
            }
            Depth = -20;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiChapterSelect ouiChapterSelect = this;
            ouiChapterSelect.Visible = true;
            ouiChapterSelect.EaseCamera();
            ouiChapterSelect.display = true;
            ouiChapterSelect.journalEnabled = Celeste.PlayMode == Celeste.PlayModes.Debug || SaveData.Instance.CheatMode;
            for (int index = 0; index <= SaveData.Instance.UnlockedAreas && !ouiChapterSelect.journalEnabled; ++index)
            {
                if (SaveData.Instance.Areas[index].Modes[0].TimePlayed > 0L && !AreaData.Get(index).Interlude)
                    ouiChapterSelect.journalEnabled = true;
            }
            OuiChapterSelectIcon unselected = null;
            if (from is OuiChapterPanel)
                (unselected = ouiChapterSelect.icons[ouiChapterSelect.area]).Unselect();
            foreach (OuiChapterSelectIcon icon in ouiChapterSelect.icons)
            {
                if (icon.Area <= SaveData.Instance.UnlockedAreas && icon != unselected)
                {
                    icon.Position = icon.HiddenPosition;
                    icon.Show();
                    icon.AssistModeUnlockable = false;
                }
                else if (SaveData.Instance.AssistMode && icon.Area == SaveData.Instance.UnlockedAreas + 1 && icon.Area <= SaveData.Instance.MaxAssistArea)
                {
                    icon.Position = icon.HiddenPosition;
                    icon.Show();
                    icon.AssistModeUnlockable = true;
                }
                yield return 0.01f;
            }
            if (!ouiChapterSelect.autoAdvancing && SaveData.Instance.UnlockedAreas == 10 && !SaveData.Instance.RevealedChapter9)
            {
                int ch = ouiChapterSelect.area;
                yield return ouiChapterSelect.SetupCh9Unlock();
                yield return ouiChapterSelect.PerformCh9Unlock(ch != 10);
            }
            if (from is OuiChapterPanel)
                yield return 0.25f;
        }

        public override IEnumerator Leave(Oui next)
        {
            display = false;
            if (next is OuiMainMenu)
            {
                while (area > SaveData.Instance.UnlockedAreas)
                    area--;
                UserIO.SaveHandler(true, false);
                yield return EaseOut(next);
                while (UserIO.Saving)
                    yield return null;
            }
            else
                yield return EaseOut(next);
        }

        private IEnumerator EaseOut(Oui next)
        {
            OuiChapterSelect ouiChapterSelect = this;
            OuiChapterSelectIcon selected = null;
            if (next is OuiChapterPanel)
                (selected = ouiChapterSelect.icons[ouiChapterSelect.area]).Select();
            foreach (OuiChapterSelectIcon icon in ouiChapterSelect.icons)
            {
                if (selected != icon)
                    icon.Hide();
                yield return 0.01f;
            }
            ouiChapterSelect.Visible = false;
        }

        public void AdvanceToNext()
        {
            autoAdvancing = true;
            Overworld.ShowInputUI = false;
            Focused = false;
            disableInput = true;
            Add(new Coroutine(AutoAdvanceRoutine()));
        }

        private IEnumerator AutoAdvanceRoutine()
        {
            OuiChapterSelect ouiChapterSelect = this;
            if (ouiChapterSelect.area < SaveData.Instance.MaxArea)
            {
                int nextArea = ouiChapterSelect.area + 1;
                if (nextArea == 9 || nextArea == 10)
                    ouiChapterSelect.icons[nextArea].HideIcon = true;
                while (!ouiChapterSelect.Selected)
                    yield return null;
                yield return 1f;
                switch (nextArea)
                {
                    case 9:
                        yield return ouiChapterSelect.PerformCh8Unlock();
                        break;
                    case 10:
                        yield return ouiChapterSelect.PerformCh9Unlock();
                        break;
                    default:
                        Audio.Play("event:/ui/postgame/unlock_newchapter");
                        Audio.Play("event:/ui/world_map/icon/roll_right");
                        ouiChapterSelect.area = nextArea;
                        ouiChapterSelect.EaseCamera();
                        ouiChapterSelect.Overworld.Maddy.Hide();
                        break;
                }
                yield return 0.25f;
            }
            ouiChapterSelect.autoAdvancing = false;
            ouiChapterSelect.disableInput = false;
            ouiChapterSelect.Focused = true;
            ouiChapterSelect.Overworld.ShowInputUI = true;
        }

        public override void Update()
        {
            if (Focused && !disableInput)
            {
                inputDelay -= Engine.DeltaTime;
                if (area >= 0 && area < AreaData.Areas.Count)
                    Input.SetLightbarColor(AreaData.Get(area).TitleBaseColor);
                if (Input.MenuCancel.Pressed)
                {
                    Audio.Play("event:/ui/main/button_back");
                    Overworld.Goto<OuiMainMenu>();
                    Overworld.Maddy.Hide();
                }
                else if (Input.MenuJournal.Pressed && journalEnabled)
                {
                    Audio.Play("event:/ui/world_map/journal/select");
                    Overworld.Goto<OuiJournal>();
                }
                else if (inputDelay <= 0.0)
                {
                    if (area > 0 && Input.MenuLeft.Pressed)
                    {
                        Audio.Play("event:/ui/world_map/icon/roll_left");
                        inputDelay = 0.15f;
                        --area;
                        icons[area].Hovered(-1);
                        EaseCamera();
                        Overworld.Maddy.Hide();
                    }
                    else if (Input.MenuRight.Pressed)
                    {
                        bool flag = SaveData.Instance.AssistMode && area == SaveData.Instance.UnlockedAreas && area < SaveData.Instance.MaxAssistArea;
                        if (area < SaveData.Instance.UnlockedAreas | flag)
                        {
                            Audio.Play("event:/ui/world_map/icon/roll_right");
                            inputDelay = 0.15f;
                            ++area;
                            icons[area].Hovered(1);
                            if (area <= SaveData.Instance.UnlockedAreas)
                                EaseCamera();
                            Overworld.Maddy.Hide();
                        }
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        if (icons[area].AssistModeUnlockable)
                        {
                            Audio.Play("event:/ui/world_map/icon/assist_skip");
                            Focused = false;
                            Overworld.ShowInputUI = false;
                            icons[area].AssistModeUnlock(() =>
                            {
                                Focused = true;
                                Overworld.ShowInputUI = true;
                                EaseCamera();
                                if (area == 10)
                                    SaveData.Instance.RevealedChapter9 = true;
                                if (area >= SaveData.Instance.MaxAssistArea)
                                    return;
                                OuiChapterSelectIcon icon = icons[area + 1];
                                icon.AssistModeUnlockable = true;
                                icon.Position = icon.HiddenPosition;
                                icon.Show();
                            });
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/icon/select");
                            SaveData.Instance.LastArea.Mode = AreaMode.Normal;
                            Overworld.Goto<OuiChapterPanel>();
                        }
                    }
                }
            }
            ease = Calc.Approach(ease, display ? 1f : 0.0f, Engine.DeltaTime * 3f);
            journalEase = Calc.Approach(journalEase, !display || disableInput || !Focused || !journalEnabled ? 0.0f : 1f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render()
        {
            Vector2 vector2 = new Vector2(960f, -scarf.Height * Ease.CubeInOut(1f - ease));
            for (int index = 0; index < scarfSegments.Length; ++index)
            {
                float num = Ease.CubeIn(index / (float) scarfSegments.Length);
                float x = (float) (num * Math.Sin(Scene.RawTimeActive * 4.0 + index * 0.05000000074505806) * 4.0 - num * 16.0);
                scarfSegments[index].DrawJustified(vector2 + new Vector2(x, index * 2), new Vector2(0.5f, 0.0f));
            }
            if (journalEase <= 0.0)
                return;
            Vector2 position = new Vector2(128f * Ease.CubeOut(journalEase), 952f);
            GFX.Gui["menu/journal"].DrawCentered(position, Color.White * Ease.CubeOut(journalEase));
            Input.GuiButton(Input.MenuJournal).Draw(position, Vector2.Zero, Color.White * Ease.CubeOut(journalEase));
        }

        private void EaseCamera()
        {
            AreaData area = AreaData.Areas[this.area];
            double num = Overworld.Mountain.EaseCamera(this.area, area.MountainIdle, targetRotate: (this.area == 10));
            Overworld.Mountain.Model.EaseState(area.MountainState);
        }

        private IEnumerator PerformCh8Unlock()
        {
            OuiChapterSelect ouiChapterSelect = this;
            Audio.Play("event:/ui/postgame/unlock_newchapter");
            Audio.Play("event:/ui/world_map/icon/roll_right");
            ouiChapterSelect.area = 9;
            ouiChapterSelect.EaseCamera();
            ouiChapterSelect.Overworld.Maddy.Hide();
            bool ready = false;
            ouiChapterSelect.icons[9].HighlightUnlock(() => ready = true);
            while (!ready)
                yield return null;
        }

        private IEnumerator SetupCh9Unlock()
        {
            icons[10].HideIcon = true;
            yield return 0.25f;
            while (area < 9)
            {
                area++;
                yield return 0.1f;
            }
        }

        private IEnumerator PerformCh9Unlock(bool easeCamera = true)
        {
            OuiChapterSelect ouiChapterSelect = this;
            Audio.Play("event:/ui/postgame/unlock_newchapter");
            Audio.Play("event:/ui/world_map/icon/roll_right");
            ouiChapterSelect.area = 10;
            yield return 0.25f;
            bool ready = false;
            ouiChapterSelect.icons[10].HighlightUnlock(() => ready = true);
            while (!ready)
                yield return null;
            if (easeCamera)
                ouiChapterSelect.EaseCamera();
            ouiChapterSelect.Overworld.Maddy.Hide();
            SaveData.Instance.RevealedChapter9 = true;
        }
    }
}
