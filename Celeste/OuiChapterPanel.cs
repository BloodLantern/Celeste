using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiChapterPanel : Oui
    {
        public AreaKey Area;
        public AreaStats RealStats;
        public AreaStats DisplayedStats;
        public AreaData Data;
        public Overworld.StartMode OverworldStartMode;
        public bool EnteringChapter;
        public const int ContentOffsetX = 440;
        public const int NoStatsHeight = 300;
        public const int StatsHeight = 540;
        public const int CheckpointsHeight = 730;
        private bool initialized;
        private string chapter = "";
        private bool selectingMode = true;
        private float height;
        private bool resizing;
        private readonly Wiggler wiggler;
        private readonly Wiggler modeAppearWiggler;
        private MTexture card = new();
        private Vector2 contentOffset;
        private float spotlightRadius;
        private float spotlightAlpha;
        private Vector2 spotlightPosition;
        private AreaCompleteTitle remixUnlockText;
        private readonly StrawberriesCounter strawberries = new(true, 0, showOutOf: true);
        private Vector2 strawberriesOffset;
        private readonly DeathsCounter deaths = new(AreaMode.Normal, true, 0);
        private Vector2 deathsOffset;
        private readonly HeartGemDisplay heart = new(0, false);
        private Vector2 heartOffset;
        private int checkpoint;
        private readonly List<Option> modes = new();
        private readonly List<Option> checkpoints = new();
        private EventInstance bSideUnlockSfx;

        public Vector2 OpenPosition => new(1070f, 100f);

        public Vector2 ClosePosition => new(2220f, 100f);

        public Vector2 IconOffset => new(690f, 86f);

        private Vector2 OptionsRenderPosition => Position + new Vector2(contentOffset.X, 128f + height);

        private int option
        {
            get => !selectingMode ? checkpoint : (int) Area.Mode;
            set
            {
                if (selectingMode)
                    Area.Mode = (AreaMode) value;
                else
                    checkpoint = value;
            }
        }

        private List<Option> options => !selectingMode ? checkpoints : modes;

        public OuiChapterPanel()
        {
            Add(strawberries);
            Add(deaths);
            Add(heart);
            deaths.CanWiggle = false;
            strawberries.CanWiggle = false;
            strawberries.OverworldSfx = true;
            Add(wiggler = Wiggler.Create(0.4f, 4f));
            Add(modeAppearWiggler = Wiggler.Create(0.4f, 4f));
        }

        public override bool IsStart(Overworld overworld, Overworld.StartMode start)
        {
            if (start is Overworld.StartMode.AreaComplete or Overworld.StartMode.AreaQuit)
            {
                bool shouldAdvance = start == Overworld.StartMode.AreaComplete && (Celeste.PlayMode == Celeste.PlayModes.Event || SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.ShouldAdvance);
                Position = OpenPosition;
                Reset();
                Add(new Coroutine(IncrementStats(shouldAdvance)));
                overworld.ShowInputUI = false;
                overworld.Mountain.SnapState(Data.MountainState);
                overworld.Mountain.SnapCamera(Area.ID, Data.MountainZoom);
                double num = (double) overworld.Mountain.EaseCamera(Area.ID, Data.MountainSelect, new float?(1f));
                OverworldStartMode = start;
                return true;
            }
            Position = ClosePosition;
            return false;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiChapterPanel ouiChapterPanel = this;
            ouiChapterPanel.Visible = true;
            ouiChapterPanel.Area.Mode = AreaMode.Normal;
            ouiChapterPanel.Reset();
            double num = (double) ouiChapterPanel.Overworld.Mountain.EaseCamera(ouiChapterPanel.Area.ID, ouiChapterPanel.Data.MountainSelect);
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
            {
                yield return null;
                ouiChapterPanel.Position = ouiChapterPanel.ClosePosition + (ouiChapterPanel.OpenPosition - ouiChapterPanel.ClosePosition) * Ease.CubeOut(p);
            }
            ouiChapterPanel.Position = ouiChapterPanel.OpenPosition;
        }

        private void Reset()
        {
            Area = SaveData.Instance.LastArea;
            Data = AreaData.Areas[Area.ID];
            RealStats = SaveData.Instance.Areas[Area.ID];
            if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.OldStats != null && SaveData.Instance.CurrentSession.Area.ID == Area.ID)
            {
                DisplayedStats = SaveData.Instance.CurrentSession.OldStats;
                SaveData.Instance.CurrentSession = null;
            }
            else
                DisplayedStats = RealStats;
            height = GetModeHeight();
            modes.Clear();
            bool flag = false;
            if (!Data.Interlude && Data.HasMode(AreaMode.BSide) && (DisplayedStats.Cassette || (SaveData.Instance.DebugMode || SaveData.Instance.CheatMode) && DisplayedStats.Cassette == RealStats.Cassette))
                flag = true;
            int num = Data.Interlude || !Data.HasMode(AreaMode.CSide) || SaveData.Instance.UnlockedModes < 3 ? 0 : (Celeste.PlayMode != Celeste.PlayModes.Event ? 1 : 0);
            modes.Add(new Option()
            {
                Label = Dialog.Clean(Data.Interlude ? "FILE_BEGIN" : "overworld_normal").ToUpper(),
                Icon = GFX.Gui["menu/play"],
                ID = "A"
            });
            if (flag)
                AddRemixButton();
            if (num != 0)
                modes.Add(new Option()
                {
                    Label = Dialog.Clean("overworld_remix2"),
                    Icon = GFX.Gui["menu/rmx2"],
                    ID = "C"
                });
            selectingMode = true;
            UpdateStats(false);
            SetStatsPosition(false);
            for (int index = 0; index < options.Count; ++index)
                options[index].SlideTowards(index, options.Count, true);
            chapter = Dialog.Get("area_chapter").Replace("{x}", Area.ChapterIndex.ToString().PadLeft(2));
            contentOffset = new Vector2(440f, 120f);
            initialized = true;
        }

        private int GetModeHeight()
        {
            AreaModeStats mode = RealStats.Modes[(int) Area.Mode];
            bool flag = mode.Strawberries.Count <= 0;
            if (!Data.Interlude && (mode.Deaths > 0 && Area.Mode != AreaMode.Normal || mode.Completed || mode.HeartGem))
                flag = false;
            return !flag ? 540 : 300;
        }

        private Option AddRemixButton()
        {
            Option option = new()
            {
                Label = Dialog.Clean("overworld_remix"),
                Icon = GFX.Gui["menu/remix"],
                ID = "B"
            };
            modes.Insert(1, option);
            return option;
        }

        // ISSUE: reference to a compiler-generated field
        public override IEnumerator Leave(Oui next)
        {
                Overworld.Mountain.EaseCamera(Area.ID, Data.MountainIdle, null, true, false);
                Add(new Coroutine(EaseOut(true), true));
                yield break;
        }

        public IEnumerator EaseOut(bool removeChildren = true)
        {
            OuiChapterPanel ouiChapterPanel = this;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
            {
                ouiChapterPanel.Position = ouiChapterPanel.OpenPosition + (ouiChapterPanel.ClosePosition - ouiChapterPanel.OpenPosition) * Ease.CubeIn(p);
                yield return null;
            }
            if (!ouiChapterPanel.Selected)
                ouiChapterPanel.Visible = false;
        }

        public void Start(string checkpoint = null)
        {
            Focused = false;
            Audio.Play("event:/ui/world_map/chapter/checkpoint_start");
            Add(new Coroutine(StartRoutine(checkpoint)));
        }

        private IEnumerator StartRoutine(string checkpoint = null)
        {
            OuiChapterPanel ouiChapterPanel = this;
            ouiChapterPanel.EnteringChapter = true;
            ouiChapterPanel.Overworld.Maddy.Hide(false);
            float num = ouiChapterPanel.Overworld.Mountain.EaseCamera(ouiChapterPanel.Area.ID, ouiChapterPanel.Data.MountainZoom, new float?(1f));
            ouiChapterPanel.Add(new Coroutine(ouiChapterPanel.EaseOut(false)));
            yield return 0.2f;

            ScreenWipe.WipeColor = Color.Black;
            AreaData.Get(ouiChapterPanel.Area).Wipe(ouiChapterPanel.Overworld, false, null);
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            if ((ouiChapterPanel.Area.ID == 0 || ouiChapterPanel.Area.ID == 9) && checkpoint == null && ouiChapterPanel.Area.Mode == AreaMode.Normal)
            {
                ouiChapterPanel.Overworld.RendererList.UpdateLists();
                ouiChapterPanel.Overworld.RendererList.MoveToFront(ouiChapterPanel.Overworld.Snow);
            }
            yield return 0.5f;

            LevelEnter.Go(new Session(ouiChapterPanel.Area, checkpoint), false);
        }

        private void Swap()
        {
            Focused = false;
            Overworld.ShowInputUI = !selectingMode;
            Add(new Coroutine(SwapRoutine()));
        }

        private IEnumerator SwapRoutine()
        {
            OuiChapterPanel ouiChapterPanel = this;
            float fromHeight = ouiChapterPanel.height;
            int toHeight = ouiChapterPanel.selectingMode ? 730 : ouiChapterPanel.GetModeHeight();
            ouiChapterPanel.resizing = true;
            ouiChapterPanel.PlayExpandSfx(fromHeight, toHeight);
            float offset = 800f;
            float p;
            for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
            {
                yield return null;
                ouiChapterPanel.contentOffset.X = (float) (440.0 + (double) offset * (double) Ease.CubeIn(p));
                ouiChapterPanel.height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(p * 0.5f));
            }
            ouiChapterPanel.selectingMode = !ouiChapterPanel.selectingMode;
            if (!ouiChapterPanel.selectingMode)
            {
                HashSet<string> checkpoints = SaveData.Instance.GetCheckpoints(ouiChapterPanel.Area);
                int num = checkpoints.Count + 1;
                ouiChapterPanel.checkpoints.Clear();
                ouiChapterPanel.checkpoints.Add(new Option()
                {
                    Label = Dialog.Clean("overworld_start"),
                    BgColor = Calc.HexToColor("eabe26"),
                    Icon = GFX.Gui["areaselect/startpoint"],
                    CheckpointLevelName = null,
                    CheckpointRotation = Calc.Random.Choose<int>(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
                    CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
                    Large = false,
                    Siblings = num
                });
                foreach (string level in checkpoints)
                    ouiChapterPanel.checkpoints.Add(new Option()
                    {
                        Label = AreaData.GetCheckpointName(ouiChapterPanel.Area, level),
                        Icon = GFX.Gui["areaselect/checkpoint"],
                        CheckpointLevelName = level,
                        CheckpointRotation = Calc.Random.Choose<int>(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
                        CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
                        Large = false,
                        Siblings = num
                    });
                if (!ouiChapterPanel.RealStats.Modes[(int) ouiChapterPanel.Area.Mode].Completed && !SaveData.Instance.DebugMode && !SaveData.Instance.CheatMode)
                {
                    ouiChapterPanel.option = ouiChapterPanel.checkpoints.Count - 1;
                    for (int index = 0; index < ouiChapterPanel.checkpoints.Count - 1; ++index)
                        ouiChapterPanel.options[index].CheckpointSlideOut = 1f;
                }
                else
                    ouiChapterPanel.option = 0;
                for (int index = 0; index < ouiChapterPanel.options.Count; ++index)
                    ouiChapterPanel.options[index].SlideTowards(index, ouiChapterPanel.options.Count, true);
            }
            ouiChapterPanel.options[ouiChapterPanel.option].Pop = 1f;
            for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
            {
                yield return null;
                ouiChapterPanel.height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(Math.Min(1f, (float) (0.5 + (double) p * 0.5))));
                ouiChapterPanel.contentOffset.X = (float) (440.0 + (double) offset * (1.0 - (double) Ease.CubeOut(p)));
            }
            ouiChapterPanel.contentOffset.X = 440f;
            ouiChapterPanel.height = toHeight;
            ouiChapterPanel.Focused = true;
            ouiChapterPanel.resizing = false;
        }

        public override void Update()
        {
            if (!initialized)
                return;
            base.Update();
            for (int index = 0; index < options.Count; ++index)
            {
                Option option = options[index];
                option.Pop = Calc.Approach(option.Pop, this.option == index ? 1f : 0.0f, Engine.DeltaTime * 4f);
                option.Appear = Calc.Approach(option.Appear, 1f, Engine.DeltaTime * 3f);
                option.CheckpointSlideOut = Calc.Approach(option.CheckpointSlideOut, this.option > index ? 1f : 0.0f, Engine.DeltaTime * 4f);
                option.Faded = Calc.Approach(option.Faded, this.option == index || option.Appeared ? 0.0f : 1f, Engine.DeltaTime * 4f);
                option.SlideTowards(index, options.Count, false);
            }
            if (selectingMode && !resizing)
                height = Calc.Approach(height, GetModeHeight(), Engine.DeltaTime * 1600f);
            if (Selected && Focused)
            {
                if (Input.MenuLeft.Pressed && option > 0)
                {
                    Audio.Play("event:/ui/world_map/chapter/tab_roll_left");
                    --option;
                    wiggler.Start();
                    if (selectingMode)
                    {
                        UpdateStats();
                        PlayExpandSfx(height, GetModeHeight());
                    }
                    else
                        Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_add");
                }
                else if (Input.MenuRight.Pressed && option + 1 < options.Count)
                {
                    Audio.Play("event:/ui/world_map/chapter/tab_roll_right");
                    ++option;
                    wiggler.Start();
                    if (selectingMode)
                    {
                        UpdateStats();
                        PlayExpandSfx(height, GetModeHeight());
                    }
                    else
                        Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_remove");
                }
                else if (Input.MenuConfirm.Pressed)
                {
                    if (selectingMode)
                    {
                        if (!SaveData.Instance.FoundAnyCheckpoints(Area))
                        {
                            Start();
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/chapter/level_select");
                            Swap();
                        }
                    }
                    else
                        Start(options[option].CheckpointLevelName);
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (selectingMode)
                    {
                        Audio.Play("event:/ui/world_map/chapter/back");
                        Overworld.Goto<OuiChapterSelect>();
                    }
                    else
                    {
                        Audio.Play("event:/ui/world_map/chapter/checkpoint_back");
                        Swap();
                    }
                }
            }
            SetStatsPosition(true);
        }

        public override void Render()
        {
            if (!initialized)
                return;
            Vector2 optionsRenderPosition = OptionsRenderPosition;
            for (int index = 0; index < options.Count; ++index)
            {
                if (!options[index].OnTopOfUI)
                    options[index].Render(optionsRenderPosition, option == index, wiggler, modeAppearWiggler);
            }
            bool flag = false;
            if (RealStats.Modes[(int) Area.Mode].Completed)
            {
                int mode = (int) Area.Mode;
                foreach (EntityData goldenberry in AreaData.Areas[Area.ID].Mode[mode].MapData.Goldenberries)
                {
                    EntityID entityId = new(goldenberry.Level.Name, goldenberry.ID);
                    if (RealStats.Modes[mode].Strawberries.Contains(entityId))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            MTexture mtexture1 = GFX.Gui[!flag ? "areaselect/cardtop" : "areaselect/cardtop_golden"];
            mtexture1.Draw(Position + new Vector2(0.0f, -32f));
            MTexture mtexture2 = GFX.Gui[!flag ? "areaselect/card" : "areaselect/card_golden"];
            card = mtexture2.GetSubtexture(0, mtexture2.Height - (int) height, mtexture2.Width, (int) height, card);
            card.Draw(Position + new Vector2(0.0f, mtexture1.Height - 32));
            for (int index = 0; index < options.Count; ++index)
            {
                if (options[index].OnTopOfUI)
                    options[index].Render(optionsRenderPosition, option == index, wiggler, modeAppearWiggler);
            }
            ActiveFont.Draw(options[option].Label, optionsRenderPosition + new Vector2(0.0f, -140f), Vector2.One * 0.5f, Vector2.One * (float) (1.0 + (double) wiggler.Value * 0.10000000149011612), Color.Black * 0.8f);
            if (selectingMode)
            {
                strawberries.Position = contentOffset + new Vector2(0.0f, 170f) + strawberriesOffset;
                deaths.Position = contentOffset + new Vector2(0.0f, 170f) + deathsOffset;
                heart.Position = contentOffset + new Vector2(0.0f, 170f) + heartOffset;
                base.Render();
            }
            if (!selectingMode)
            {
                Vector2 center = Position + new Vector2(contentOffset.X, 340f);
                for (int index = options.Count - 1; index >= 0; --index)
                    DrawCheckpoint(center, options[index], index);
            }
            GFX.Gui["areaselect/title"].Draw(Position + new Vector2(-60f, 0.0f), Vector2.Zero, Data.TitleBaseColor);
            GFX.Gui["areaselect/accent"].Draw(Position + new Vector2(-60f, 0.0f), Vector2.Zero, Data.TitleAccentColor);
            string text = Dialog.Clean(AreaData.Get(Area).Name);
            if (Data.Interlude)
            {
                ActiveFont.Draw(text, Position + IconOffset + new Vector2(-100f, 0.0f), new Vector2(1f, 0.5f), Vector2.One * 1f, Data.TitleTextColor * 0.8f);
            }
            else
            {
                ActiveFont.Draw(chapter, Position + IconOffset + new Vector2(-100f, -2f), new Vector2(1f, 1f), Vector2.One * 0.6f, Data.TitleAccentColor * 0.8f);
                ActiveFont.Draw(text, Position + IconOffset + new Vector2(-100f, -18f), new Vector2(1f, 0.0f), Vector2.One * 1f, Data.TitleTextColor * 0.8f);
            }
            if (spotlightAlpha <= 0.0)
                return;
            HiresRenderer.EndRender();
            SpotlightWipe.DrawSpotlight(spotlightPosition, spotlightRadius, Color.Black * spotlightAlpha);
            HiresRenderer.BeginRender();
        }

        private void DrawCheckpoint(Vector2 center, Option option, int checkpointIndex)
        {
            MTexture checkpointPreview = GetCheckpointPreview(Area, option.CheckpointLevelName);
            MTexture checkpoint = MTN.Checkpoints["polaroid"];
            float checkpointRotation = option.CheckpointRotation;
            Vector2 position1 = center + option.CheckpointOffset + Vector2.UnitX * 800f * Ease.CubeIn(option.CheckpointSlideOut);
            Vector2 position2 = position1;
            Color white = Color.White;
            double rotation = (double) checkpointRotation;
            checkpoint.DrawCentered(position2, white, 0.75f, (float) rotation);
            MTexture mtexture = GFX.Gui["collectables/strawberry"];
            if (checkpointPreview != null)
            {
                Vector2 scale = Vector2.One * 0.75f;
                if (SaveData.Instance.Assists.MirrorMode)
                    scale.X = -scale.X;
                scale *= 720f / checkpointPreview.Width;
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.PointClamp);
                checkpointPreview.DrawCentered(position1, Color.White, scale, checkpointRotation);
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender();
            }
            int mode = (int) Area.Mode;
            if (!RealStats.Modes[mode].Completed && !SaveData.Instance.CheatMode && !SaveData.Instance.DebugMode)
                return;
            Vector2 vec = new(300f, 220f);
            Vector2 vector2 = position1 + vec.Rotate(checkpointRotation);
            int length = checkpointIndex != 0 ? Data.Mode[mode].Checkpoints[checkpointIndex - 1].Strawberries : Data.Mode[mode].StartStrawberries;
            bool[] flagArray = new bool[length];
            foreach (EntityID strawberry in RealStats.Modes[mode].Strawberries)
            {
                for (int index = 0; index < length; ++index)
                {
                    EntityData entityData = Data.Mode[mode].StrawberriesByCheckpoint[checkpointIndex, index];
                    if (entityData != null && entityData.Level.Name == strawberry.Level && entityData.ID == strawberry.ID)
                        flagArray[index] = true;
                }
            }
            Vector2 vector = Calc.AngleToVector(checkpointRotation, 1f);
            Vector2 position3 = vector2 - vector * length * 44f;
            if (Area.Mode == AreaMode.Normal && Data.CassetteCheckpointIndex == checkpointIndex)
            {
                Vector2 position4 = position3 - vector * 60f;
                if (RealStats.Cassette)
                    MTN.Journal["cassette"].DrawCentered(position4, Color.White, 1f, checkpointRotation);
                else
                    MTN.Journal["cassette_outline"].DrawCentered(position4, Color.DarkGray, 1f, checkpointRotation);
            }
            for (int index = 0; index < length; ++index)
            {
                mtexture.DrawCentered(position3, flagArray[index] ? Color.White : Color.Black * 0.3f, 0.5f, checkpointRotation);
                position3 += vector * 44f;
            }
        }

        private void UpdateStats(
            bool wiggle = true,
            bool? overrideStrawberryWiggle = null,
            bool? overrideDeathWiggle = null,
            bool? overrideHeartWiggle = null)
        {
            AreaModeStats mode = DisplayedStats.Modes[(int) Area.Mode];
            AreaData areaData = AreaData.Get(Area);
            deaths.Visible = mode.Deaths > 0 && (Area.Mode != AreaMode.Normal || RealStats.Modes[(int) Area.Mode].Completed) && !AreaData.Get(Area).Interlude;
            deaths.Amount = mode.Deaths;
            deaths.SetMode(areaData.IsFinal ? AreaMode.CSide : Area.Mode);
            heart.Visible = mode.HeartGem && !areaData.Interlude && areaData.CanFullClear;
            heart.SetCurrentMode(Area.Mode, mode.HeartGem);
            strawberries.Visible = (mode.TotalStrawberries > 0 || mode.Completed && Area.Mode == AreaMode.Normal && AreaData.Get(Area).Mode[0].TotalStrawberries > 0) && !AreaData.Get(Area).Interlude;
            strawberries.Amount = mode.TotalStrawberries;
            strawberries.OutOf = Data.Mode[0].TotalStrawberries;
            strawberries.ShowOutOf = mode.Completed && Area.Mode == AreaMode.Normal;
            strawberries.Golden = Area.Mode != 0;
            if (!wiggle)
                return;
            if (strawberries.Visible && (!overrideStrawberryWiggle.HasValue || overrideStrawberryWiggle.Value))
                strawberries.Wiggle();
            if (heart.Visible && (!overrideHeartWiggle.HasValue || overrideHeartWiggle.Value))
                heart.Wiggle();
            if (!deaths.Visible || overrideDeathWiggle.HasValue && !overrideDeathWiggle.Value)
                return;
            deaths.Wiggle();
        }

        private void SetStatsPosition(bool approach)
        {
            if (heart.Visible && (strawberries.Visible || deaths.Visible))
            {
                heartOffset = Approach(heartOffset, new Vector2(-120f, 0.0f), !approach);
                strawberriesOffset = Approach(strawberriesOffset, new Vector2(120f, deaths.Visible ? -40f : 0.0f), !approach);
                deathsOffset = Approach(deathsOffset, new Vector2(120f, strawberries.Visible ? 40f : 0.0f), !approach);
            }
            else if (heart.Visible)
            {
                heartOffset = Approach(heartOffset, Vector2.Zero, !approach);
            }
            else
            {
                strawberriesOffset = Approach(strawberriesOffset, new Vector2(0.0f, deaths.Visible ? -40f : 0.0f), !approach);
                deathsOffset = Approach(deathsOffset, new Vector2(0.0f, strawberries.Visible ? 40f : 0.0f), !approach);
            }
        }

        private Vector2 Approach(Vector2 from, Vector2 to, bool snap) => snap ? to : (from += (to - from) * (1f - (float) Math.Pow(1.0 / 1000.0, (double) Engine.DeltaTime)));

        private IEnumerator IncrementStatsDisplay(
            AreaModeStats modeStats,
            AreaModeStats newModeStats,
            bool doHeartGem,
            bool doStrawberries,
            bool doDeaths,
            bool doRemixUnlock)
        {
            OuiChapterPanel ouiChapterPanel = this;
            if (doHeartGem)
            {
                Audio.Play("event:/ui/postgame/crystal_heart");
                ouiChapterPanel.heart.Visible = true;
                ouiChapterPanel.heart.SetCurrentMode(ouiChapterPanel.Area.Mode, true);
                ouiChapterPanel.heart.Appear(ouiChapterPanel.Area.Mode);
                yield return 1.8f;
            }
            if (doStrawberries)
            {
                ouiChapterPanel.strawberries.CanWiggle = true;
                ouiChapterPanel.strawberries.Visible = true;
                while (newModeStats.TotalStrawberries > modeStats.TotalStrawberries)
                {
                    int num = newModeStats.TotalStrawberries - modeStats.TotalStrawberries;
                    if (num < 3)
                        yield return 0.3f;
                    else if (num < 8)
                    {
                        yield return 0.2f;
                    }
                    else
                    {
                        yield return 0.1f;
                        ++modeStats.TotalStrawberries;
                    }
                    ++modeStats.TotalStrawberries;
                    ouiChapterPanel.strawberries.Amount = modeStats.TotalStrawberries;
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                }
                ouiChapterPanel.strawberries.CanWiggle = false;
                yield return 0.5f;
                if (newModeStats.Completed && !modeStats.Completed && ouiChapterPanel.Area.Mode == AreaMode.Normal)
                {
                    yield return 0.25f;
                    Audio.Play(ouiChapterPanel.strawberries.Amount >= ouiChapterPanel.Data.Mode[0].TotalStrawberries ? "event:/ui/postgame/strawberry_total_all" : "event:/ui/postgame/strawberry_total");
                    ouiChapterPanel.strawberries.OutOf = ouiChapterPanel.Data.Mode[0].TotalStrawberries;
                    ouiChapterPanel.strawberries.ShowOutOf = true;
                    ouiChapterPanel.strawberries.Wiggle();
                    modeStats.Completed = true;
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    yield return 0.6f;
                }
            }
            if (doDeaths)
            {
                Audio.Play("event:/ui/postgame/death_appear");
                ouiChapterPanel.deaths.CanWiggle = true;
                ouiChapterPanel.deaths.Visible = true;
                while (newModeStats.Deaths > modeStats.Deaths)
                {
                    yield return ouiChapterPanel.HandleDeathTick(modeStats.Deaths, newModeStats.Deaths, out int add);
                    modeStats.Deaths += add;
                    ouiChapterPanel.deaths.Amount = modeStats.Deaths;
                    if (modeStats.Deaths >= newModeStats.Deaths)
                        Audio.Play("event:/ui/postgame/death_final");
                    else
                        Audio.Play("event:/ui/postgame/death_count");
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                }
                yield return 0.8f;
                ouiChapterPanel.deaths.CanWiggle = false;
            }
            if (doRemixUnlock)
            {
                ouiChapterPanel.bSideUnlockSfx = Audio.Play("event:/ui/postgame/unlock_bside");
                Option o = ouiChapterPanel.AddRemixButton();
                o.Appear = 0.0f;
                o.IconEase = 0.0f;
                o.Appeared = true;
                yield return 0.5f;
                ouiChapterPanel.spotlightPosition = o.GetRenderPosition(ouiChapterPanel.OptionsRenderPosition);
                float t;
                for (t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 0.5f)
                {
                    ouiChapterPanel.spotlightAlpha = t * 0.9f;
                    ouiChapterPanel.spotlightRadius = 128f * Ease.CubeOut(t);
                    yield return null;
                }
                yield return 0.3f;
                while ((double) (o.IconEase += Engine.DeltaTime * 2f) < 1.0)
                    yield return null;
                o.IconEase = 1f;
                ouiChapterPanel.modeAppearWiggler.Start();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                ouiChapterPanel.remixUnlockText = new AreaCompleteTitle(ouiChapterPanel.spotlightPosition + new Vector2(0.0f, 80f), Dialog.Clean("OVERWORLD_REMIX_UNLOCKED"), 1f);
                ouiChapterPanel.remixUnlockText.Tag = (int) Tags.HUD;
                ouiChapterPanel.Overworld.Add(ouiChapterPanel.remixUnlockText);
                yield return 1.5f;
                for (t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / 0.5f)
                {
                    ouiChapterPanel.spotlightAlpha = (float) ((1.0 - (double) t) * 0.5);
                    ouiChapterPanel.spotlightRadius = (float) (128.0 + 128.0 * (double) Ease.CubeOut(t));
                    ouiChapterPanel.remixUnlockText.Alpha = 1f - Ease.CubeOut(t);
                    yield return null;
                }
                ouiChapterPanel.remixUnlockText.RemoveSelf();
                ouiChapterPanel.remixUnlockText = null;
                o.Appeared = false;
                o = null;
            }
        }

        public IEnumerator IncrementStats(bool shouldAdvance)
        {
            OuiChapterPanel ouiChapterPanel = this;
            ouiChapterPanel.Focused = false;
            ouiChapterPanel.Overworld.ShowInputUI = false;
            if (ouiChapterPanel.Data.Interlude)
            {
                if (shouldAdvance && ouiChapterPanel.OverworldStartMode == Overworld.StartMode.AreaComplete)
                {
                    yield return 1.2f;
                    ouiChapterPanel.Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
                }
                else
                    ouiChapterPanel.Focused = true;
                yield return null;
            }
            else
            {
                AreaData data = ouiChapterPanel.Data;
                AreaStats displayedStats = ouiChapterPanel.DisplayedStats;
                AreaStats area = SaveData.Instance.Areas[data.ID];
                AreaModeStats modeStats = displayedStats.Modes[(int) ouiChapterPanel.Area.Mode];
                AreaModeStats newModeStats = area.Modes[(int) ouiChapterPanel.Area.Mode];
                bool doStrawberries = newModeStats.TotalStrawberries > modeStats.TotalStrawberries;
                bool doHeartGem = newModeStats.HeartGem && !modeStats.HeartGem;
                bool doDeaths = newModeStats.Deaths > modeStats.Deaths && (ouiChapterPanel.Area.Mode != AreaMode.Normal || newModeStats.Completed);
                bool doRemixUnlock = ouiChapterPanel.Area.Mode == AreaMode.Normal && data.HasMode(AreaMode.BSide) && area.Cassette && !displayedStats.Cassette;
                if (doStrawberries | doHeartGem | doDeaths | doRemixUnlock)
                    yield return 0.8f;
                bool skipped = false;
                Coroutine routine = new(ouiChapterPanel.IncrementStatsDisplay(modeStats, newModeStats, doHeartGem, doStrawberries, doDeaths, doRemixUnlock));
                ouiChapterPanel.Add(routine);
                yield return null;
                while (!routine.Finished)
                {
                    if (MInput.GamePads[0].Pressed(Buttons.Start) || MInput.Keyboard.Pressed(Keys.Enter))
                    {
                        routine.Active = false;
                        routine.RemoveSelf();
                        skipped = true;
                        Audio.Stop(ouiChapterPanel.bSideUnlockSfx);
                        Audio.Play("event:/new_content/ui/skip_all");
                        break;
                    }
                    yield return null;
                }
                if (skipped & doRemixUnlock)
                {
                    ouiChapterPanel.spotlightAlpha = 0.0f;
                    ouiChapterPanel.spotlightRadius = 0.0f;
                    if (ouiChapterPanel.remixUnlockText != null)
                    {
                        ouiChapterPanel.remixUnlockText.RemoveSelf();
                        ouiChapterPanel.remixUnlockText = null;
                    }
                    if (ouiChapterPanel.modes.Count <= 1 || ouiChapterPanel.modes[1].ID != "B")
                    {
                        ouiChapterPanel.AddRemixButton();
                    }
                    else
                    {
                        Option mode = ouiChapterPanel.modes[1];
                        mode.IconEase = 1f;
                        mode.Appear = 1f;
                        mode.Appeared = false;
                    }
                }
                ouiChapterPanel.DisplayedStats = ouiChapterPanel.RealStats;
                if (skipped)
                {
                    doStrawberries = doStrawberries && modeStats.TotalStrawberries != newModeStats.TotalStrawberries;
                    doDeaths &= modeStats.Deaths != newModeStats.Deaths;
                    doHeartGem = doHeartGem && !ouiChapterPanel.heart.Visible;
                    ouiChapterPanel.UpdateStats(overrideStrawberryWiggle: new bool?(doStrawberries), overrideDeathWiggle: new bool?(doDeaths), overrideHeartWiggle: new bool?(doHeartGem));
                }
                yield return null;
                routine = null;
                if (shouldAdvance && ouiChapterPanel.OverworldStartMode == Overworld.StartMode.AreaComplete)
                {
                    if (!doDeaths && !doStrawberries && !doHeartGem || Settings.Instance.SpeedrunClock != SpeedrunType.Off)
                        yield return 1.2f;
                    ouiChapterPanel.Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
                }
                else
                {
                    ouiChapterPanel.Focused = true;
                    ouiChapterPanel.Overworld.ShowInputUI = true;
                }
            }
        }

        private float HandleDeathTick(int oldDeaths, int newDeaths, out int add)
        {
            int num = newDeaths - oldDeaths;
            if (num < 3)
            {
                add = 1;
                return 0.3f;
            }
            if (num < 8)
            {
                add = 2;
                return 0.2f;
            }
            if (num < 30)
            {
                add = 5;
                return 0.1f;
            }
            if (num < 100)
            {
                add = 10;
                return 0.1f;
            }
            if (num < 1000)
            {
                add = 25;
                return 0.1f;
            }
            add = 100;
            return 0.1f;
        }

        private void PlayExpandSfx(float currentHeight, float nextHeight)
        {
            if ((double) nextHeight > (double) currentHeight)
            {
                Audio.Play("event:/ui/world_map/chapter/pane_expand");
            }
            else
            {
                if ((double) nextHeight >= (double) currentHeight)
                    return;
                Audio.Play("event:/ui/world_map/chapter/pane_contract");
            }
        }

        public static string GetCheckpointPreviewName(AreaKey area, string level) => level == null ? area.ToString() : area.ToString() + "_" + level;

        private MTexture GetCheckpointPreview(AreaKey area, string level)
        {
            string checkpointPreviewName = GetCheckpointPreviewName(area, level);
            return MTN.Checkpoints.Has(checkpointPreviewName) ? MTN.Checkpoints[checkpointPreviewName] : null;
        }

        private class Option
        {
            public string Label;
            public string ID;
            public MTexture Icon;
            public MTexture Bg = GFX.Gui["areaselect/tab"];
            public Color BgColor = Calc.HexToColor("3c6180");
            public float Pop;
            public bool Large = true;
            public int Siblings;
            public float Slide;
            public float Appear = 1f;
            public float IconEase = 1f;
            public bool Appeared;
            public float Faded;
            public float CheckpointSlideOut;
            public string CheckpointLevelName;
            public float CheckpointRotation;
            public Vector2 CheckpointOffset;

            public float Scale => Siblings < 5 ? 1f : 0.8f;

            public bool OnTopOfUI => Pop > 0.5;

            public void SlideTowards(int i, int count, bool snap)
            {
                float num = (float) (count / 2.0 - 0.5);
                float target = i - num;
                if (snap)
                    Slide = target;
                else
                    Slide = Calc.Approach(Slide, target, Engine.DeltaTime * 4f);
            }

            public Vector2 GetRenderPosition(Vector2 center)
            {
                float num = (Large ? 170f : 130f) * Scale;
                if (Siblings > 0 && (double) num * Siblings > 750.0)
                    num = 750 / Siblings;
                Vector2 renderPosition = center + new Vector2(Slide * num, (float) (Math.Sin(Pop * 3.1415927410125732) * 70.0 - Pop * 12.0));
                renderPosition.Y += (float) ((1.0 - (double) Ease.CubeOut(Appear)) * -200.0);
                renderPosition.Y -= (float) ((1.0 - (double) Scale) * 80.0);
                return renderPosition;
            }

            public void Render(Vector2 center, bool selected, Wiggler wiggler, Wiggler appearWiggler)
            {
                float num1 = (float) ((double) Scale + (selected ? (double) wiggler.Value * 0.25 : 0.0) + (Appeared ? (double) appearWiggler.Value * 0.25 : 0.0));
                Vector2 renderPosition = GetRenderPosition(center);
                Color color1 = Color.Lerp(BgColor, Color.Black, (float) ((1.0 - Pop) * 0.60000002384185791));
                Bg.DrawCentered(renderPosition + new Vector2(0.0f, 10f), color1, (Appeared ? Scale : num1) * new Vector2(Large ? 1f : 0.9f, 1f));
                if (IconEase <= 0.0)
                    return;
                float num2 = Ease.CubeIn(IconEase);
                Color color2 = Color.Lerp(Color.White, Color.Black, Faded * 0.6f) * num2;
                Icon.DrawCentered(renderPosition, color2, (float) ((Bg.Width - 50) / (double) Icon.Width * (double) num1 * (2.5 - (double) num2 * 1.5)));
            }
        }
    }
}
