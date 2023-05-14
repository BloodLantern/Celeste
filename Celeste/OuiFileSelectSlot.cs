// Decompiled with JetBrains decompiler
// Type: Celeste.OuiFileSelectSlot
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiFileSelectSlot : Entity
    {
        public SaveData SaveData;
        public int FileSlot;
        public string Name;
        public bool AssistModeEnabled;
        public bool VariantModeEnabled;
        public bool Exists;
        public bool Corrupted;
        public string Time;
        public int FurthestArea;
        public Sprite Portrait;
        public bool HasBlackgems;
        public StrawberriesCounter Strawberries;
        public DeathsCounter Deaths;
        public List<bool> Cassettes = new();
        public List<bool[]> HeartGems = new();
        private const int height = 300;
        private const int spacing = 10;
        private const float portraitSize = 200f;
        public bool StartingGame;
        public bool Renaming;
        public bool Assisting;
        private readonly OuiFileSelect fileSelect;
        private bool deleting;
        private float highlightEase;
        private float highlightEaseDelay;
        private float selectedEase;
        private float deletingEase;
        private Tween tween;
        private int buttonIndex;
        private int deleteIndex;
        private readonly Wiggler wiggler;
        private float failedToDeleteEase;
        private float failedToDeleteTimer;
        private float screenFlash;
        private float inputDelay;
        private float newgameFade;
        private readonly float timeScale = 1f;
        private OuiFileSelectSlot.Button assistButton;
        private OuiFileSelectSlot.Button variantButton;
        private readonly Sprite normalCard;
        private readonly Sprite goldCard;
        private readonly Sprite normalTicket;
        private readonly Sprite goldTicket;
        private readonly List<OuiFileSelectSlot.Button> buttons = new();

        public Vector2 IdlePosition => new(960f, 540 + (310 * (FileSlot - 1)));

        public Vector2 SelectedPosition => new(960f, 440f);

        private bool highlighted => fileSelect.SlotIndex == FileSlot;

        private bool selected => fileSelect.SlotSelected && highlighted;

        private bool Golden => !Corrupted && Exists && SaveData.TotalStrawberries >= 202;

        private Sprite Card => !Golden ? normalCard : goldCard;

        private Sprite Ticket => !Golden ? normalTicket : goldTicket;

        private OuiFileSelectSlot(int index, OuiFileSelect fileSelect)
        {
            FileSlot = index;
            this.fileSelect = fileSelect;
            Tag |= (int)Tags.HUD | (int)Tags.PauseUpdate;
            Visible = false;
            Add(wiggler = Wiggler.Create(0.4f, 4f));
            normalTicket = new Sprite(MTN.FileSelect, "ticket");
            normalTicket.AddLoop("idle", "", 0.1f);
            normalTicket.Add("shine", "", 0.1f, "idle");
            _ = normalTicket.CenterOrigin();
            normalTicket.Play("idle");
            normalCard = new Sprite(MTN.FileSelect, "card");
            normalCard.AddLoop("idle", "", 0.1f);
            normalCard.Add("shine", "", 0.1f, "idle");
            _ = normalCard.CenterOrigin();
            normalCard.Play("idle");
            goldTicket = new Sprite(MTN.FileSelect, "ticketShine");
            goldTicket.AddLoop("idle", "", 0.1f, new int[1]);
            goldTicket.Add("shine", "", 0.05f, "idle", 0, 0, 0, 0, 0, 1, 2, 3, 4, 5);
            _ = goldTicket.CenterOrigin();
            goldTicket.Play("idle");
            goldCard = new Sprite(MTN.FileSelect, "cardShine");
            goldCard.AddLoop("idle", "", 0.1f, 5);
            goldCard.Add("shine", "", 0.05f, "idle");
            _ = goldCard.CenterOrigin();
            goldCard.Play("idle");
        }

        public OuiFileSelectSlot(int index, OuiFileSelect fileSelect, bool corrupted)
            : this(index, fileSelect)
        {
            Corrupted = corrupted;
            Exists = corrupted;
            Setup();
        }

        public OuiFileSelectSlot(int index, OuiFileSelect fileSelect, SaveData data)
            : this(index, fileSelect)
        {
            Exists = true;
            SaveData = data;
            Name = data.Name;
            if (!Dialog.Language.CanDisplay(Name))
            {
                Name = Dialog.Clean("FILE_DEFAULT");
            }

            if (!Settings.Instance.VariantsUnlocked && data.TotalHeartGems >= 24)
            {
                Settings.Instance.VariantsUnlocked = true;
            }

            AssistModeEnabled = data.AssistMode;
            VariantModeEnabled = data.VariantMode;
            Add(Deaths = new DeathsCounter(AreaMode.Normal, false, data.TotalDeaths));
            Add(Strawberries = new StrawberriesCounter(true, data.TotalStrawberries));
            Time = Dialog.FileTime(data.Time);
            if (TimeSpan.FromTicks(data.Time).TotalHours > 0.0)
            {
                timeScale = 0.725f;
            }

            FurthestArea = data.UnlockedAreas;
            foreach (AreaStats area in data.Areas)
            {
                if (area.ID <= data.UnlockedAreas)
                {
                    if (!AreaData.Areas[area.ID].Interlude && AreaData.Areas[area.ID].CanFullClear)
                    {
                        bool[] flagArray = new bool[3];
                        for (int index1 = 0; index1 < flagArray.Length; ++index1)
                        {
                            flagArray[index1] = area.Modes[index1].HeartGem;
                        }

                        Cassettes.Add(area.Cassette);
                        HeartGems.Add(flagArray);
                    }
                }
                else
                {
                    break;
                }
            }
            Setup();
        }

        private void Setup()
        {
            string str = "portrait_madeline";
            string id = "idle_normal";
            Portrait = GFX.PortraitsSpriteBank.Create(str);
            Portrait.Play(id);
            Portrait.Scale = Vector2.One * (200f / GFX.PortraitsSpriteBank.SpriteData[str].Sources[0].XML.AttrInt("size", 160));
            Add(Portrait);
        }

        public void CreateButtons()
        {
            buttons.Clear();
            if (Exists)
            {
                if (!Corrupted)
                {
                    buttons.Add(new OuiFileSelectSlot.Button()
                    {
                        Label = Dialog.Clean("file_continue"),
                        Action = new Action(OnContinueSelected)
                    });
                    if (SaveData != null)
                    {
                        List<OuiFileSelectSlot.Button> buttons1 = buttons;
                        OuiFileSelectSlot.Button button1 = new()
                        {
                            Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF")),
                            Action = new Action(OnAssistSelected),
                            Scale = 0.7f
                        };
                        OuiFileSelectSlot.Button button2 = button1;
                        assistButton = button1;
                        OuiFileSelectSlot.Button button3 = button2;
                        buttons1.Add(button3);
                        if (Settings.Instance.VariantsUnlocked || SaveData.CheatMode)
                        {
                            List<OuiFileSelectSlot.Button> buttons2 = buttons;
                            OuiFileSelectSlot.Button button4 = new()
                            {
                                Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF")),
                                Action = new Action(OnVariantSelected),
                                Scale = 0.7f
                            };
                            OuiFileSelectSlot.Button button5 = button4;
                            variantButton = button4;
                            OuiFileSelectSlot.Button button6 = button5;
                            buttons2.Add(button6);
                        }
                    }
                }
                buttons.Add(new OuiFileSelectSlot.Button()
                {
                    Label = Dialog.Clean("file_delete"),
                    Action = new Action(OnDeleteSelected),
                    Scale = 0.7f
                });
            }
            else
            {
                buttons.Add(new OuiFileSelectSlot.Button()
                {
                    Label = Dialog.Clean("file_begin"),
                    Action = new Action(OnNewGameSelected)
                });
                buttons.Add(new OuiFileSelectSlot.Button()
                {
                    Label = Dialog.Clean("file_rename"),
                    Action = new Action(OnRenameSelected),
                    Scale = 0.7f
                });
                List<OuiFileSelectSlot.Button> buttons3 = buttons;
                OuiFileSelectSlot.Button button7 = new()
                {
                    Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF")),
                    Action = new Action(OnAssistSelected),
                    Scale = 0.7f
                };
                OuiFileSelectSlot.Button button8 = button7;
                assistButton = button7;
                OuiFileSelectSlot.Button button9 = button8;
                buttons3.Add(button9);
                if (!Settings.Instance.VariantsUnlocked)
                {
                    return;
                }

                List<OuiFileSelectSlot.Button> buttons4 = buttons;
                OuiFileSelectSlot.Button button10 = new()
                {
                    Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF")),
                    Action = new Action(OnVariantSelected),
                    Scale = 0.7f
                };
                OuiFileSelectSlot.Button button11 = button10;
                variantButton = button10;
                OuiFileSelectSlot.Button button12 = button11;
                buttons4.Add(button12);
            }
        }

        private void OnContinueSelected()
        {
            StartingGame = true;
            _ = Audio.Play("event:/ui/main/savefile_begin");
            SaveData.Start(SaveData, FileSlot);
            SaveData.Instance.AssistMode = AssistModeEnabled;
            SaveData.Instance.VariantMode = VariantModeEnabled;
            SaveData.Instance.AssistModeChecks();
            if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.InArea)
            {
                _ = Audio.SetMusic(null);
                _ = Audio.SetAmbience(null);
                fileSelect.Overworld.ShowInputUI = false;
                FadeWipe fadeWipe = new(Scene, false, () => LevelEnter.Go(SaveData.Instance.CurrentSession, true));
            }
            else if (SaveData.Instance.Areas[0].Modes[0].Completed || SaveData.Instance.CheatMode)
            {
                if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.ShouldAdvance)
                {
                    SaveData.Instance.LastArea.ID = SaveData.Instance.UnlockedAreas;
                }

                SaveData.Instance.CurrentSession = null;
                _ = (Scene as Overworld).Goto<OuiChapterSelect>();
            }
            else
            {
                _ = Audio.SetMusic(null);
                _ = Audio.SetAmbience(null);
                EnterFirstArea();
            }
        }

        private void OnDeleteSelected()
        {
            deleting = true;
            wiggler.Start();
            _ = Audio.Play("event:/ui/main/message_confirm");
        }

        private void OnNewGameSelected()
        {
            _ = Audio.SetMusic(null);
            _ = Audio.SetAmbience(null);
            _ = Audio.Play("event:/ui/main/savefile_begin");
            SaveData.Start(new SaveData()
            {
                Name = Name,
                AssistMode = AssistModeEnabled,
                VariantMode = VariantModeEnabled
            }, FileSlot);
            StartingGame = true;
            EnterFirstArea();
        }

        private void EnterFirstArea()
        {
            fileSelect.Overworld.Maddy.Disabled = true;
            fileSelect.Overworld.ShowInputUI = false;
            Add(new Coroutine(EnterFirstAreaRoutine()));
        }

        private IEnumerator EnterFirstAreaRoutine()
        {
            Overworld overworld = fileSelect.Overworld;
            yield return fileSelect.Leave(null);
            yield return overworld.Mountain.EaseCamera(0, AreaData.Areas[0].MountainIdle);
            yield return 0.3f;
            double num = (double)overworld.Mountain.EaseCamera(0, AreaData.Areas[0].MountainZoom, new float?(1f));
            yield return 0.4f;
            AreaData.Areas[0].Wipe(overworld, false, null);
            overworld.RendererList.UpdateLists();
            overworld.RendererList.MoveToFront(overworld.Snow);
            yield return 0.5f;
            LevelEnter.Go(new Session(new AreaKey(0)), false);
        }

        private void OnRenameSelected()
        {
            Renaming = true;
            OuiFileNaming ouiFileNaming = fileSelect.Overworld.Goto<OuiFileNaming>();
            ouiFileNaming.FileSlot = this;
            ouiFileNaming.StartingName = Name;
            _ = Audio.Play("event:/ui/main/savefile_rename_start");
        }

        private void OnAssistSelected()
        {
            Assisting = true;
            fileSelect.Overworld.Goto<OuiAssistMode>().FileSlot = this;
            _ = Audio.Play("event:/ui/main/assist_button_info");
        }

        private void OnVariantSelected()
        {
            if (!Settings.Instance.VariantsUnlocked && (SaveData == null || !SaveData.CheatMode))
            {
                return;
            }

            VariantModeEnabled = !VariantModeEnabled;
            if (VariantModeEnabled)
            {
                AssistModeEnabled = false;
                _ = Audio.Play("event:/ui/main/button_toggle_on");
            }
            else
            {
                _ = Audio.Play("event:/ui/main/button_toggle_off");
            }

            assistButton.Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF"));
            variantButton.Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF"));
        }

        public Vector2 HiddenPosition(int x, int y)
        {
            return !selected ? new Vector2(960f, Y) + (new Vector2(x, y) * new Vector2(1920f, 1080f)) : (new Vector2(1920f, 1080f) / 2f) + (new Vector2(x, y) * new Vector2(1920f, 1080f));
        }

        public void Show()
        {
            Visible = true;
            deleting = false;
            StartingGame = false;
            Renaming = false;
            Assisting = false;
            selectedEase = 0.0f;
            highlightEase = 0.0f;
            highlightEaseDelay = 0.35f;
            Vector2 from = Position;
            StartTween(0.25f, f => Position = Vector2.Lerp(from, IdlePosition, f.Eased));
        }

        public void Select(bool resetButtonIndex)
        {
            Visible = true;
            deleting = false;
            StartingGame = false;
            Renaming = false;
            Assisting = false;
            CreateButtons();
            Card.Play("shine");
            Ticket.Play("shine");
            Vector2 from = Position;
            wiggler.Start();
            if (resetButtonIndex)
            {
                buttonIndex = 0;
            }

            deleteIndex = 1;
            inputDelay = 0.1f;
            StartTween(0.25f, f =>
            {
                Position = Vector2.Lerp(from, SelectedPosition, selectedEase = f.Eased);
                newgameFade = Math.Max(newgameFade, f.Eased);
            });
        }

        public void Unselect()
        {
            Vector2 from = Position;
            buttonIndex = 0;
            StartTween(0.25f, f =>
            {
                selectedEase = 1f - f.Eased;
                newgameFade = 1f - f.Eased;
                Position = Vector2.Lerp(from, IdlePosition, f.Eased);
            });
        }

        public void MoveTo(float x, float y)
        {
            Vector2 from = Position;
            Vector2 to = new(x, y);
            StartTween(0.25f, f => Position = Vector2.Lerp(from, to, f.Eased));
        }

        public void Hide(int x, int y)
        {
            Vector2 from = Position;
            Vector2 to = HiddenPosition(x, y);
            StartTween(0.25f, f => Position = Vector2.Lerp(from, to, f.Eased), true);
        }

        private void StartTween(float duration, Action<Tween> callback, bool hide = false)
        {
            if (tween != null && tween.Entity == this)
            {
                tween.RemoveSelf();
            }

            Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, duration));
            tween.OnUpdate = callback;
            tween.OnComplete = t =>
            {
                if (hide)
                {
                    Visible = false;
                }

                tween = null;
            };
            tween.Start();
        }

        public override void Update()
        {
            inputDelay -= Engine.DeltaTime;
            Ticket.Update();
            Card.Update();
            if (selected && fileSelect.Selected && fileSelect.Focused && !StartingGame && tween == null && inputDelay <= 0.0 && !StartingGame)
            {
                if (deleting)
                {
                    if (Input.MenuCancel.Pressed)
                    {
                        deleting = false;
                        wiggler.Start();
                        _ = Audio.Play("event:/ui/main/button_back");
                    }
                    else if (Input.MenuUp.Pressed && deleteIndex > 0)
                    {
                        deleteIndex = 0;
                        wiggler.Start();
                        _ = Audio.Play("event:/ui/main/rollover_up");
                    }
                    else if (Input.MenuDown.Pressed && deleteIndex < 1)
                    {
                        deleteIndex = 1;
                        wiggler.Start();
                        _ = Audio.Play("event:/ui/main/rollover_down");
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        if (deleteIndex == 1)
                        {
                            deleting = false;
                            wiggler.Start();
                            _ = Audio.Play("event:/ui/main/button_back");
                        }
                        else if (SaveData.TryDelete(FileSlot))
                        {
                            Exists = false;
                            Corrupted = false;
                            deleting = false;
                            deletingEase = 0.0f;
                            fileSelect.UnselectHighlighted();
                            _ = Audio.Play("event:/ui/main/savefile_delete");
                            if (!Settings.Instance.DisableFlashes)
                            {
                                screenFlash = 1f;
                            }

                            CreateButtons();
                        }
                        else
                        {
                            failedToDeleteEase = 0.0f;
                            failedToDeleteTimer = 3f;
                            _ = Audio.Play("event:/ui/main/button_invalid");
                        }
                    }
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (fileSelect.HasSlots)
                    {
                        fileSelect.UnselectHighlighted();
                        _ = Audio.Play("event:/ui/main/whoosh_savefile_in");
                        _ = Audio.Play("event:/ui/main/button_back");
                    }
                }
                else if (Input.MenuUp.Pressed && buttonIndex > 0)
                {
                    --buttonIndex;
                    wiggler.Start();
                    _ = Audio.Play("event:/ui/main/rollover_up");
                }
                else if (Input.MenuDown.Pressed && buttonIndex < buttons.Count - 1)
                {
                    ++buttonIndex;
                    wiggler.Start();
                    _ = Audio.Play("event:/ui/main/rollover_down");
                }
                else if (Input.MenuConfirm.Pressed)
                {
                    buttons[buttonIndex].Action();
                }
            }
            if (highlightEaseDelay <= 0.0)
            {
                highlightEase = Calc.Approach(highlightEase, !highlighted || (!Exists && selected) ? 0.0f : 1f, Engine.DeltaTime * 4f);
            }
            else
            {
                highlightEaseDelay -= Engine.DeltaTime;
            }

            Depth = highlighted ? -10 : 0;
            if (Renaming || Assisting)
            {
                selectedEase = Calc.Approach(selectedEase, 0.0f, Engine.DeltaTime * 4f);
            }

            deletingEase = Calc.Approach(deletingEase, deleting ? 1f : 0.0f, Engine.DeltaTime * 4f);
            failedToDeleteEase = Calc.Approach(failedToDeleteEase, failedToDeleteTimer > 0.0 ? 1f : 0.0f, Engine.DeltaTime * 4f);
            failedToDeleteTimer -= Engine.DeltaTime;
            screenFlash = Calc.Approach(screenFlash, 0.0f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render()
        {
            float num1 = Ease.CubeInOut(highlightEase);
            float num2 = wiggler.Value * 8f;
            if (selectedEase > 0.0)
            {
                Vector2 vector2_1 = Position + new Vector2(0.0f, (float)((350.0 * selectedEase) - 150.0));
                float lineHeight = ActiveFont.LineHeight;
                for (int index = 0; index < buttons.Count; ++index)
                {
                    OuiFileSelectSlot.Button button = buttons[index];
                    Vector2 vector2_2 = Vector2.UnitX * (buttonIndex != index || deleting ? 0.0f : num2);
                    Color color = SelectionColor(buttonIndex == index && !deleting);
                    ActiveFont.DrawOutline(button.Label, vector2_1 + vector2_2, new Vector2(0.5f, 0.0f), Vector2.One * button.Scale, color, 2f, Color.Black);
                    vector2_1.Y += (float)(((double)lineHeight * button.Scale) + 15.0);
                }
            }
            Vector2 position1 = Position + (Vector2.UnitX * num1 * 360f);
            Ticket.RenderPosition = position1;
            Ticket.Render();
            if (highlightEase > 0.0 && Exists && !Corrupted)
            {
                int x = -280;
                int num3 = 600;
                for (int index1 = 0; index1 < Cassettes.Count; ++index1)
                {
                    MTN.FileSelect[Cassettes[index1] ? "cassette" : "dot"].DrawCentered(position1 + new Vector2(x + (float)((index1 + 0.5) * 75.0), -75f));
                    bool[] heartGem = HeartGems[index1];
                    int num4 = 0;
                    for (int index2 = 0; index2 < heartGem.Length; ++index2)
                    {
                        if (heartGem[index2])
                        {
                            ++num4;
                        }
                    }
                    Vector2 position2 = position1 + new Vector2(x + (float)((index1 + 0.5) * 75.0), -12f);
                    if (num4 == 0)
                    {
                        MTN.FileSelect["dot"].DrawCentered(position2);
                    }
                    else
                    {
                        position2.Y -= (float)((num4 - 1) * 0.5 * 14.0);
                        int index3 = 0;
                        int num5 = 0;
                        for (; index3 < heartGem.Length; ++index3)
                        {
                            if (heartGem[index3])
                            {
                                MTN.FileSelect["heartgem" + index3].DrawCentered(position2 + new Vector2(0.0f, num5 * 14));
                                ++num5;
                            }
                        }
                    }
                }
                Deaths.Position = position1 + new Vector2(x, 68f) - Position;
                Deaths.Render();
                ActiveFont.Draw(Time, position1 + new Vector2(x + num3, 68f), new Vector2(1f, 0.5f), Vector2.One * timeScale, Color.Black * 0.6f);
            }
            else if (Corrupted)
            {
                ActiveFont.Draw(Dialog.Clean("file_corrupted"), position1, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
            }
            else if (!Exists)
            {
                ActiveFont.Draw(Dialog.Clean("file_newgame"), position1, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
            }

            Vector2 position3 = Position - (Vector2.UnitX * num1 * 360f);
            int num6 = 64;
            int num7 = 16;
            float num8 = (float)((double)Card.Width - (num6 * 2) - 200.0) - num7;
            float x1 = (float)((-(double)Card.Width / 2.0) + num6 + 200.0 + num7 + ((double)num8 / 2.0));
            float num9 = Exists ? 1f : newgameFade;
            if (!Corrupted)
            {
                if (newgameFade > 0.0 || Exists)
                {
                    if (AssistModeEnabled)
                    {
                        MTN.FileSelect["assist"].DrawCentered(position3, Color.White * num9);
                    }
                    else if (VariantModeEnabled)
                    {
                        MTN.FileSelect["variants"].DrawCentered(position3, Color.White * num9);
                    }
                }
                if (Exists && SaveData.CheatMode)
                {
                    MTN.FileSelect["cheatmode"].DrawCentered(position3, Color.White * num9);
                }
            }
            Card.RenderPosition = position3;
            Card.Render();
            if (!Corrupted)
            {
                if (Exists)
                {
                    if (SaveData.TotalStrawberries >= 175)
                    {
                        MTN.FileSelect["strawberry"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.Areas.Count > 7 && SaveData.Areas[7].Modes[0].Completed)
                    {
                        MTN.FileSelect["flag"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.TotalCassettes >= 8)
                    {
                        MTN.FileSelect["cassettes"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.TotalHeartGems >= 16)
                    {
                        MTN.FileSelect["heart"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.TotalGoldenStrawberries >= 25)
                    {
                        MTN.FileSelect["goldberry"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.TotalHeartGems >= 24)
                    {
                        MTN.FileSelect["goldheart"].DrawCentered(position3, Color.White * num9);
                    }

                    if (SaveData.Areas.Count > 10 && SaveData.Areas[10].Modes[0].Completed)
                    {
                        MTN.FileSelect["farewell"].DrawCentered(position3, Color.White * num9);
                    }
                }
                if (Exists || Renaming || newgameFade > 0.0)
                {
                    Portrait.RenderPosition = position3 + new Vector2((float)((-(double)Card.Width / 2.0) + num6 + 100.0), 0.0f);
                    Portrait.Color = Color.White * num9;
                    Portrait.Render();
                    MTN.FileSelect[!Golden ? "portraitOverlay" : "portraitOverlayGold"].DrawCentered(Portrait.RenderPosition, Color.White * num9);
                    string name = Name;
                    Vector2 position4 = position3 + new Vector2(x1, (Exists ? 0 : 64) - 32);
                    float num10 = Math.Min(1f, 440f / ActiveFont.Measure(name).X);
                    ActiveFont.Draw(name, position4, new Vector2(0.5f, 1f), Vector2.One * num10, Color.Black * 0.8f * num9);
                    if (Renaming && Scene.BetweenInterval(0.3f))
                    {
                        ActiveFont.Draw("|", new Vector2(position4.X + (float)(ActiveFont.Measure(name).X * (double)num10 * 0.5), position4.Y), new Vector2(0.0f, 1f), Vector2.One * num10, Color.Black * 0.8f * num9);
                    }
                }
                if (Exists)
                {
                    if (FurthestArea < AreaData.Areas.Count)
                    {
                        ActiveFont.Draw(Dialog.Clean(AreaData.Areas[FurthestArea].Name), position3 + new Vector2(x1, -10f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Color.Black * 0.6f);
                    }

                    Strawberries.Position = position3 + new Vector2(x1, 55f) - Position;
                    Strawberries.Render();
                }
            }
            else
            {
                ActiveFont.Draw(Dialog.Clean("file_failedtoload"), position3, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
            }

            if (deletingEase > 0.0)
            {
                float num11 = Ease.CubeOut(deletingEase);
                Vector2 vector2 = new(960f, 540f);
                float lineHeight = ActiveFont.LineHeight;
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num11 * 0.9f);
                ActiveFont.Draw(Dialog.Clean("file_delete_really"), vector2 + new Vector2(0.0f, (float)(-16.0 - (64.0 * (1.0 - (double)num11)))), new Vector2(0.5f, 1f), Vector2.One, Color.White * num11);
                ActiveFont.DrawOutline(Dialog.Clean("file_delete_yes"), vector2 + new Vector2((float)((!deleting || deleteIndex != 0 ? 0.0 : (double)num2) * 1.2000000476837158) * num11, (float)(16.0 + (64.0 * (1.0 - (double)num11)))), new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, deleting ? SelectionColor(deleteIndex == 0) : Color.Gray, 2f, Color.Black * num11);
                ActiveFont.DrawOutline(Dialog.Clean("file_delete_no"), vector2 + new Vector2((float)((!deleting || deleteIndex != 1 ? 0.0 : (double)num2) * 1.2000000476837158) * num11, (float)(16.0 + (double)lineHeight + (64.0 * (1.0 - (double)num11)))), new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, deleting ? SelectionColor(deleteIndex == 1) : Color.Gray, 2f, Color.Black * num11);
                if (failedToDeleteEase > 0.0)
                {
                    Vector2 position5 = new(960f, (float)(980.0 - (100.0 * deletingEase)));
                    Vector2 scale = Vector2.One * 0.8f;
                    if (failedToDeleteEase < 1.0 && failedToDeleteTimer > 0.0)
                    {
                        position5 += new Vector2(Calc.Random.Next(10) - 5, Calc.Random.Next(10) - 5);
                        scale = Vector2.One * (float)(0.800000011920929 + (0.20000000298023224 * (1.0 - failedToDeleteEase)));
                    }
                    ActiveFont.DrawOutline(Dialog.Clean("file_delete_failed"), position5, new Vector2(0.5f, 0.0f), scale, Color.PaleVioletRed * deletingEase, 2f, Color.Black * deletingEase);
                }
            }
            if (screenFlash <= 0.0)
            {
                return;
            }

            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.White * Ease.CubeOut(screenFlash));
        }

        public Color SelectionColor(bool selected)
        {
            return !selected
                ? Color.White
                : !Settings.Instance.DisableFlashes && !Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;
        }

        private class Button
        {
            public string Label;
            public Action Action;
            public float Scale = 1f;
        }
    }
}
