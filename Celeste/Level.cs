using Celeste.Editor;
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
    public class Level : Scene, IOverlayHandler
    {
        public bool Completed;
        public bool NewLevel;
        public bool TimerStarted;
        public bool TimerStopped;
        public bool TimerHidden;
        public Session Session;
        public Vector2? StartPosition;
        public bool DarkRoom;
        public Player.IntroTypes LastIntroType;
        public bool InCredits;
        public bool AllowHudHide = true;
        public VirtualMap<char> SolidsData;
        public VirtualMap<char> BgData;
        public float NextTransitionDuration = 0.65f;
        public const float DefaultTransitionDuration = 0.65f;
        public ScreenWipe Wipe;
        private Coroutine transition;
        private Coroutine saving;
        public FormationBackdrop FormationBackdrop;
        public SolidTiles SolidTiles;
        public BackgroundTiles BgTiles;
        public Color BackgroundColor = Color.Black;
        public BackdropRenderer Background;
        public BackdropRenderer Foreground;
        public GameplayRenderer GameplayRenderer;
        public HudRenderer HudRenderer;
        public LightingRenderer Lighting;
        public DisplacementRenderer Displacement;
        public BloomRenderer Bloom;
        public TileGrid FgTilesLightMask;
        public ParticleSystem Particles;
        public ParticleSystem ParticlesBG;
        public ParticleSystem ParticlesFG;
        public HiresSnow HiresSnow;
        public TotalStrawberriesDisplay strawberriesDisplay;
        private WindController windController;
        public const float CameraOffsetXInterval = 48f;
        public const float CameraOffsetYInterval = 32f;
        public Camera Camera;
        public CameraLockModes CameraLockMode;
        public Vector2 CameraOffset;
        public float CameraUpwardMaxY;
        private Vector2 shakeDirection;
        private int lastDirectionalShake;
        private float shakeTimer;
        private Vector2 cameraPreShake;
        public float ScreenPadding;
        private float flash;
        private Color flashColor = Color.White;
        private bool doFlash;
        private bool flashDrawPlayer;
        private float glitchTimer;
        private float glitchSeed;
        public float Zoom = 1f;
        public float ZoomTarget = 1f;
        public Vector2 ZoomFocusPoint;
        private string lastColorGrade;
        private float colorGradeEase;
        private float colorGradeEaseSpeed = 1f;
        public Vector2 Wind;
        public float WindSine;
        public float WindSineTimer;
        public bool Frozen;
        public bool PauseLock;
        public bool CanRetry = true;
        public bool PauseMainMenuOpen;
        private bool wasPaused;
        private float wasPausedTimer;
        private float unpauseTimer;
        public bool SaveQuitDisabled;
        public bool InCutscene;
        public bool SkippingCutscene;
        private Coroutine skipCoroutine;
        private Action<Level> onCutsceneSkip;
        private bool onCutsceneSkipFadeIn;
        private bool onCutsceneSkipResetZoom;
        private bool endingChapterAfterCutscene;
        public static EventInstance DialogSnapshot;
        private static EventInstance PauseSnapshot;
        private static EventInstance AssistSpeedSnapshot;
        private static int AssistSpeedSnapshotValue = -1;
        public Pathfinder Pathfinder;
        public PlayerDeadBody RetryPlayerCorpse;
        public float BaseLightingAlpha;
        private bool updateHair = true;
        public bool InSpace;
        public bool HasCassetteBlocks;
        public float CassetteBlockTempo;
        public int CassetteBlockBeats;
        public Random HiccupRandom;
        public bool Raining;
        private Session.CoreModes coreMode;

        public Vector2 LevelOffset
        {
            get
            {
                Rectangle bounds = Bounds;
                double left = bounds.Left;
                bounds = Bounds;
                double top = bounds.Top;
                return new Vector2((float) left, (float) top);
            }
        }

        public Point LevelSolidOffset
        {
            get
            {
                Rectangle bounds = Bounds;
                int x = bounds.Left / 8 - TileBounds.X;
                bounds = Bounds;
                int y = bounds.Top / 8 - TileBounds.Y;
                return new Point(x, y);
            }
        }

        public Rectangle TileBounds => Session.MapData.TileBounds;

        public bool Transitioning => transition != null;

        public Vector2 ShakeVector { get; private set; }

        public float VisualWind => Wind.X + WindSine;

        public bool FrozenOrPaused => Frozen || Paused;

        public bool CanPause
        {
            get
            {
                Player entity = Tracker.GetEntity<Player>();
                if (entity == null || entity.Dead || wasPaused || Paused || PauseLock || SkippingCutscene || Transitioning || Wipe != null || UserIO.Saving)
                    return false;
                return entity.LastBooster == null || !entity.LastBooster.Ch9HubTransition || !entity.LastBooster.BoostingPlayer;
            }
        }

        public Overlay Overlay { get; set; }

        public bool ShowHud
        {
            get
            {
                if (Completed)
                    return false;
                if (Paused)
                    return true;
                return Tracker.GetEntity<Textbox>() == null && Tracker.GetEntity<MiniTextbox>() == null && !Frozen && !InCutscene;
            }
        }

        public override void Begin()
        {
            ScreenWipe.WipeColor = Color.Black;
            GameplayBuffers.Create();
            Distort.WaterAlpha = 1f;
            Distort.WaterSineDirection = 1f;
            Audio.MusicUnderwater = false;
            Audio.EndSnapshot(Level.DialogSnapshot);
            base.Begin();
        }

        public override void End()
        {
            base.End();
            Foreground.Ended(this);
            Background.Ended(this);
            EndPauseEffects();
            Audio.BusStopAll("bus:/gameplay_sfx");
            Audio.MusicUnderwater = false;
            Audio.SetAmbience(null);
            Audio.SetAltMusic(null);
            Audio.EndSnapshot(Level.DialogSnapshot);
            Audio.ReleaseSnapshot(Level.AssistSpeedSnapshot);
            Level.AssistSpeedSnapshot = null;
            Level.AssistSpeedSnapshotValue = -1;
            GameplayBuffers.Unload();
            ClutterBlockGenerator.Dispose();
            Engine.TimeRateB = 1f;
        }

        public void LoadLevel(Player.IntroTypes playerIntro, bool isFromLoader = false)
        {
            TimerHidden = false;
            TimerStopped = false;
            LastIntroType = playerIntro;
            Background.Fade = 0.0f;
            CanRetry = true;
            ScreenPadding = 0.0f;
            Displacement.Enabled = true;
            PauseLock = false;
            Frozen = false;
            CameraLockMode = CameraLockModes.None;
            RetryPlayerCorpse = null;
            FormationBackdrop.Display = false;
            SaveQuitDisabled = false;
            lastColorGrade = Session.ColorGrade;
            colorGradeEase = 0.0f;
            colorGradeEaseSpeed = 1f;
            HasCassetteBlocks = false;
            CassetteBlockTempo = 1f;
            CassetteBlockBeats = 2;
            Raining = false;
            bool flag1 = false;
            bool flag2 = false;
            HiccupRandom ??= new Random(Session.Area.ID * 77 + (int) Session.Area.Mode * 999);
            Entities.FindFirst<LightningRenderer>()?.Reset();
            Calc.PushRandom(Session.LevelData.LoadSeed);
            MapData mapData = Session.MapData;
            LevelData levelData = Session.LevelData;
            Vector2 entityOffset = new(levelData.Bounds.Left, levelData.Bounds.Top);
            bool flag3 = playerIntro != Player.IntroTypes.Fall || levelData.Name == "0";
            DarkRoom = levelData.Dark && !Session.GetFlag("ignore_darkness_" + levelData.Name);
            Zoom = 1f;
            Session.Audio ??= AreaData.Get(Session).Mode[(int) Session.Area.Mode].AudioState.Clone();
            if (!levelData.DelayAltMusic)
                Audio.SetAltMusic(SFX.EventnameByHandle(levelData.AltMusic));
            if (levelData.Music.Length > 0)
                Session.Audio.Music.Event = SFX.EventnameByHandle(levelData.Music);
            if (!AreaData.GetMode(Session.Area).IgnoreLevelAudioLayerData)
            {
                for (int index = 0; index < 4; ++index)
                    Session.Audio.Music.Layer(index + 1, levelData.MusicLayers[index]);
            }
            if (levelData.MusicProgress >= 0)
                Session.Audio.Music.Progress = levelData.MusicProgress;
            Session.Audio.Music.Layer(6, levelData.MusicWhispers);
            if (levelData.Ambience.Length > 0)
                Session.Audio.Ambience.Event = SFX.EventnameByHandle(levelData.Ambience);
            if (levelData.AmbienceProgress >= 0)
                Session.Audio.Ambience.Progress = levelData.AmbienceProgress;
            Session.Audio.Apply(isFromLoader);
            CoreMode = Session.CoreMode;
            NewLevel = !Session.LevelFlags.Contains(levelData.Name);
            if (flag3)
            {
                if (!Session.LevelFlags.Contains(levelData.Name))
                    Session.FurthestSeenLevel = levelData.Name;
                Session.LevelFlags.Add(levelData.Name);
                Session.UpdateLevelStartDashes();
            }
            Vector2? nullable = new Vector2?();
            CameraOffset = new Vector2(48f, 32f) * levelData.CameraOffset;
            Entities.FindFirst<WindController>()?.RemoveSelf();
            Add(windController = new WindController(levelData.WindPattern));
            if (playerIntro != Player.IntroTypes.Transition)
                windController.SetStartPattern();
            if (levelData.Underwater)
                Add(new Water(entityOffset, false, false, levelData.Bounds.Width, levelData.Bounds.Height));
            InSpace = levelData.Space;
            if (InSpace)
                Add(new SpaceController());
            if (levelData.Name == "-1" && Session.Area.ID == 0 && !SaveData.Instance.CheatMode)
                Add(new UnlockEverythingThingy());
            int index1 = 0;
            List<EntityID> entityIdList = new();
            Player entity1 = Tracker.GetEntity<Player>();
            if (entity1 != null)
            {
                foreach (Follower follower in entity1.Leader.Followers)
                    entityIdList.Add(follower.ParentEntityID);
            }
            foreach (EntityData entity in levelData.Entities)
            {
                int id = entity.ID;
                EntityID entityId = new(levelData.Name, id);
                if (!Session.DoNotLoad.Contains(entityId) && !entityIdList.Contains(entityId))
                {
                    switch (entity.Name)
                    {
                        case "SoundTest3d":
                            Add(new _3dSoundTest(entity, entityOffset));
                            continue;
                        case "SummitBackgroundManager":
                            Add(new AscendManager(entity, entityOffset));
                            continue;
                        case "badelineBoost":
                            Add(new BadelineBoost(entity, entityOffset));
                            continue;
                        case "bigSpinner":
                            Add(new Bumper(entity, entityOffset));
                            continue;
                        case "bigWaterfall":
                            Add(new BigWaterfall(entity, entityOffset));
                            continue;
                        case "bird":
                            Add(new BirdNPC(entity, entityOffset));
                            continue;
                        case "birdForsakenCityGem":
                            Add(new ForsakenCitySatellite(entity, entityOffset));
                            continue;
                        case "birdPath":
                            Add(new BirdPath(entityId, entity, entityOffset));
                            continue;
                        case "blackGem":
                            if (!Session.HeartGem || Session.Area.Mode != AreaMode.Normal)
                            {
                                Add(new HeartGem(entity, entityOffset));
                                continue;
                            }
                            continue;
                        case "blockField":
                            Add(new BlockField(entity, entityOffset));
                            continue;
                        case "bonfire":
                            Add(new Bonfire(entity, entityOffset));
                            continue;
                        case "booster":
                            Add(new Booster(entity, entityOffset));
                            continue;
                        case "bounceBlock":
                            Add(new BounceBlock(entity, entityOffset));
                            continue;
                        case "bridge":
                            Add(new Bridge(entity, entityOffset));
                            continue;
                        case "bridgeFixed":
                            Add(new BridgeFixed(entity, entityOffset));
                            continue;
                        case "cassette":
                            if (!Session.Cassette)
                            {
                                Add(new Cassette(entity, entityOffset));
                                continue;
                            }
                            continue;
                        case "cassetteBlock":
                            CassetteBlock cassetteBlock = new(entity, entityOffset, entityId);
                            Add(cassetteBlock);
                            HasCassetteBlocks = true;
                            if (CassetteBlockTempo == 1f)
                                CassetteBlockTempo = cassetteBlock.Tempo;
                            CassetteBlockBeats = Math.Max(cassetteBlock.Index + 1, CassetteBlockBeats);
                            if (!flag1)
                            {
                                flag1 = true;
                                if (Tracker.GetEntity<CassetteBlockManager>() == null && ShouldCreateCassetteManager)
                                {
                                    Add(new CassetteBlockManager());
                                    continue;
                                }
                                continue;
                            }
                            continue;
                        case "chaserBarrier":
                            Add(new ChaserBarrier(entity, entityOffset));
                            continue;
                        case "checkpoint":
                            if (flag3)
                            {
                                Checkpoint checkpoint = new(entity, entityOffset);
                                Add(checkpoint);
                                nullable = new Vector2?(entity.Position + entityOffset + checkpoint.SpawnOffset);
                                continue;
                            }
                            continue;
                        case "cliffflag":
                            Add(new CliffFlags(entity, entityOffset));
                            continue;
                        case "cliffside_flag":
                            Add(new CliffsideWindFlag(entity, entityOffset));
                            continue;
                        case "clothesline":
                            Add(new Clothesline(entity, entityOffset));
                            continue;
                        case "cloud":
                            Add(new Cloud(entity, entityOffset));
                            continue;
                        case "clutterCabinet":
                            Add(new ClutterCabinet(entity, entityOffset));
                            continue;
                        case "clutterDoor":
                            Add(new ClutterDoor(entity, entityOffset, Session));
                            continue;
                        case "cobweb":
                            Add(new Cobweb(entity, entityOffset));
                            continue;
                        case "colorSwitch":
                            Add(new ClutterSwitch(entity, entityOffset));
                            continue;
                        case "conditionBlock":
                            ConditionBlockModes conditionBlockModes = entity.Enum<ConditionBlockModes>("condition");
                            EntityID none = EntityID.None;
                            string[] strArray = entity.Attr("conditionID").Split(':');
                            none.Level = strArray[0];
                            none.ID = Convert.ToInt32(strArray[1]);
                            bool hasExitBlock = conditionBlockModes switch
                            {
                                ConditionBlockModes.Key => Session.DoNotLoad.Contains(none),
                                ConditionBlockModes.Button => Session.GetFlag(DashSwitch.GetFlagName(none)),
                                ConditionBlockModes.Strawberry => Session.Strawberries.Contains(none),
                                _ => throw new Exception("Condition type not supported!"),
                            };
                            if (hasExitBlock)
                            {
                                Add(new ExitBlock(entity, entityOffset));
                                continue;
                            }
                            continue;
                        case "coreMessage":
                            Add(new CoreMessage(entity, entityOffset));
                            continue;
                        case "coreModeToggle":
                            Add(new CoreModeToggle(entity, entityOffset));
                            continue;
                        case "coverupWall":
                            Add(new CoverupWall(entity, entityOffset));
                            continue;
                        case "crumbleBlock":
                            Add(new CrumblePlatform(entity, entityOffset));
                            continue;
                        case "crumbleWallOnRumble":
                            Add(new CrumbleWallOnRumble(entity, entityOffset, entityId));
                            continue;
                        case "crushBlock":
                            Add(new CrushBlock(entity, entityOffset));
                            continue;
                        case "cutsceneNode":
                            Add(new CutsceneNode(entity, entityOffset));
                            continue;
                        case "darkChaser":
                            Add(new BadelineOldsite(entity, entityOffset, index1));
                            ++index1;
                            continue;
                        case "dashBlock":
                            Add(new DashBlock(entity, entityOffset, entityId));
                            continue;
                        case "dashSwitchH":
                        case "dashSwitchV":
                            Add(DashSwitch.Create(entity, entityOffset, entityId));
                            continue;
                        case "door":
                            Add(new Door(entity, entityOffset));
                            continue;
                        case "dreamBlock":
                            Add(new DreamBlock(entity, entityOffset));
                            continue;
                        case "dreamHeartGem":
                            if (!Session.HeartGem)
                            {
                                Add(new DreamHeartGem(entity, entityOffset));
                                continue;
                            }
                            continue;
                        case "dreammirror":
                            Add(new DreamMirror(entityOffset + entity.Position));
                            continue;
                        case "exitBlock":
                            Add(new ExitBlock(entity, entityOffset));
                            continue;
                        case "eyebomb":
                            Add(new Puffer(entity, entityOffset));
                            continue;
                        case "fakeBlock":
                            Add(new FakeWall(entityId, entity, entityOffset, FakeWall.Modes.Block));
                            continue;
                        case "fakeHeart":
                            Add(new FakeHeart(entity, entityOffset));
                            continue;
                        case "fakeWall":
                            Add(new FakeWall(entityId, entity, entityOffset, FakeWall.Modes.Wall));
                            continue;
                        case "fallingBlock":
                            Add(new FallingBlock(entity, entityOffset));
                            continue;
                        case "finalBoss":
                            Add(new FinalBoss(entity, entityOffset));
                            continue;
                        case "finalBossFallingBlock":
                            Add(FallingBlock.CreateFinalBossBlock(entity, entityOffset));
                            continue;
                        case "finalBossMovingBlock":
                            Add(new FinalBossMovingBlock(entity, entityOffset));
                            continue;
                        case "fireBall":
                            Add(new FireBall(entity, entityOffset));
                            continue;
                        case "fireBarrier":
                            Add(new FireBarrier(entity, entityOffset));
                            continue;
                        case "flingBird":
                            Add(new FlingBird(entity, entityOffset));
                            continue;
                        case "flingBirdIntro":
                            Add(new FlingBirdIntro(entity, entityOffset));
                            continue;
                        case "floatingDebris":
                            Add(new FloatingDebris(entity, entityOffset));
                            continue;
                        case "floatySpaceBlock":
                            Add(new FloatySpaceBlock(entity, entityOffset));
                            continue;
                        case "flutterbird":
                            Add(new FlutterBird(entity, entityOffset));
                            continue;
                        case "foregroundDebris":
                            Add(new ForegroundDebris(entity, entityOffset));
                            continue;
                        case "friendlyGhost":
                            Add(new AngryOshiro(entity, entityOffset));
                            continue;
                        case "glassBlock":
                            Add(new GlassBlock(entity, entityOffset));
                            continue;
                        case "glider":
                            Add(new Glider(entity, entityOffset));
                            continue;
                        case "goldenBerry":
                            int num1 = SaveData.Instance.CheatMode ? 1 : 0;
                            bool flag5 = Session.FurthestSeenLevel == Session.Level || Session.Deaths == 0;
                            bool flag6 = SaveData.Instance.UnlockedModes >= 3 || SaveData.Instance.DebugMode;
                            bool completed = SaveData.Instance.Areas[Session.Area.ID].Modes[(int) Session.Area.Mode].Completed;
                            if (((num1 != 0 ? 1 : (flag6 & completed ? 1 : 0)) & (flag5 ? 1 : 0)) != 0)
                            {
                                Add(new Strawberry(entity, entityOffset, entityId));
                                continue;
                            }
                            continue;
                        case "goldenBlock":
                            Add(new GoldenBlock(entity, entityOffset));
                            continue;
                        case "gondola":
                            Add(new Gondola(entity, entityOffset));
                            continue;
                        case "greenBlocks":
                            ClutterBlockGenerator.Init(this);
                            ClutterBlockGenerator.Add((int) (entity.Position.X / 8.0), (int) (entity.Position.Y / 8.0), entity.Width / 8, entity.Height / 8, ClutterBlock.Colors.Green);
                            continue;
                        case "hahaha":
                            Add(new Hahaha(entity, entityOffset));
                            continue;
                        case "hanginglamp":
                            Add(new HangingLamp(entity, entityOffset + entity.Position));
                            continue;
                        case "heartGemDoor":
                            Add(new HeartGemDoor(entity, entityOffset));
                            continue;
                        case "iceBlock":
                            Add(new IceBlock(entity, entityOffset));
                            continue;
                        case "infiniteStar":
                            Add(new FlyFeather(entity, entityOffset));
                            continue;
                        case "introCar":
                            Add(new IntroCar(entity, entityOffset));
                            continue;
                        case "introCrusher":
                            Add(new IntroCrusher(entity, entityOffset));
                            continue;
                        case "invisibleBarrier":
                            Add(new InvisibleBarrier(entity, entityOffset));
                            continue;
                        case "jumpThru":
                            Add(new JumpthruPlatform(entity, entityOffset));
                            continue;
                        case "kevins_pc":
                            Add(new KevinsPC(entity, entityOffset));
                            continue;
                        case "key":
                            Add(new Key(entity, entityOffset, entityId));
                            continue;
                        case "killbox":
                            Add(new Killbox(entity, entityOffset));
                            continue;
                        case "lamp":
                            Add(new Lamp(entityOffset + entity.Position, entity.Bool("broken")));
                            continue;
                        case "light":
                            Add(new PropLight(entity, entityOffset));
                            continue;
                        case "lightbeam":
                            Add(new LightBeam(entity, entityOffset));
                            continue;
                        case "lightning":
                            if (entity.Bool("perLevel") || !Session.GetFlag("disable_lightning"))
                            {
                                Add(new Lightning(entity, entityOffset));
                                flag2 = true;
                                continue;
                            }
                            continue;
                        case "lightningBlock":
                            Add(new LightningBreakerBox(entity, entityOffset));
                            continue;
                        case "lockBlock":
                            Add(new LockBlock(entity, entityOffset, entityId));
                            continue;
                        case "memorial":
                            Add(new Memorial(entity, entityOffset));
                            continue;
                        case "memorialTextController":
                            if (Session.Dashes == 0 && Session.StartedFromBeginning)
                            {
                                Add(new Strawberry(entity, entityOffset, entityId));
                                continue;
                            }
                            continue;
                        case "moonCreature":
                            Add(new MoonCreature(entity, entityOffset));
                            continue;
                        case "moveBlock":
                            Add(new MoveBlock(entity, entityOffset));
                            continue;
                        case "movingPlatform":
                            Add(new MovingPlatform(entity, entityOffset));
                            continue;
                        case "negaBlock":
                            Add(new NegaBlock(entity, entityOffset));
                            continue;
                        case "npc":
                            string npcName = entity.Attr("npc").ToLower();
                            Vector2 position = entity.Position + entityOffset;
                            if (npcName == "granny_00_house")
                            {
                                Add(new NPC00_Granny(position));
                                continue;
                            }
                            if (npcName == "theo_01_campfire")
                            {
                                Add(new NPC01_Theo(position));
                                continue;
                            }
                            if (npcName == "theo_02_campfire")
                            {
                                Add(new NPC02_Theo(position));
                                continue;
                            }
                            if (npcName == "theo_03_escaping")
                            {
                                if (!Session.GetFlag("resort_theo"))
                                {
                                    Add(new NPC03_Theo_Escaping(position));
                                    continue;
                                }
                                continue;
                            }
                            if (npcName == "theo_03_vents")
                            {
                                Add(new NPC03_Theo_Vents(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_lobby")
                            {
                                Add(new NPC03_Oshiro_Lobby(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_hallway")
                            {
                                Add(new NPC03_Oshiro_Hallway1(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_hallway2")
                            {
                                Add(new NPC03_Oshiro_Hallway2(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_bigroom")
                            {
                                Add(new NPC03_Oshiro_Cluttter(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "oshiro_03_breakdown")
                            {
                                Add(new NPC03_Oshiro_Breakdown(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_suite")
                            {
                                Add(new NPC03_Oshiro_Suite(position));
                                continue;
                            }
                            if (npcName == "oshiro_03_rooftop")
                            {
                                Add(new NPC03_Oshiro_Rooftop(position));
                                continue;
                            }
                            if (npcName == "granny_04_cliffside")
                            {
                                Add(new NPC04_Granny(position));
                                continue;
                            }
                            if (npcName == "theo_04_cliffside")
                            {
                                Add(new NPC04_Theo(position));
                                continue;
                            }
                            if (npcName == "theo_05_entrance")
                            {
                                Add(new NPC05_Theo_Entrance(position));
                                continue;
                            }
                            if (npcName == "theo_05_inmirror")
                            {
                                Add(new NPC05_Theo_Mirror(position));
                                continue;
                            }
                            if (npcName == "evil_05")
                            {
                                Add(new NPC05_Badeline(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "theo_06_plateau")
                            {
                                Add(new NPC06_Theo_Plateau(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_06_intro")
                            {
                                Add(new NPC06_Granny(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "badeline_06_crying")
                            {
                                Add(new NPC06_Badeline_Crying(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_06_ending")
                            {
                                Add(new NPC06_Granny_Ending(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "theo_06_ending")
                            {
                                Add(new NPC06_Theo_Ending(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_07x")
                            {
                                Add(new NPC07X_Granny_Ending(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "theo_08_inside")
                            {
                                Add(new NPC08_Theo(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_08_inside")
                            {
                                Add(new NPC08_Granny(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_09_outside")
                            {
                                Add(new NPC09_Granny_Outside(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_09_inside")
                            {
                                Add(new NPC09_Granny_Inside(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "gravestone_10")
                            {
                                Add(new NPC10_Gravestone(entity, entityOffset));
                                continue;
                            }
                            if (npcName == "granny_10_never")
                            {
                                Add(new NPC07X_Granny_Ending(entity, entityOffset, true));
                                continue;
                            }
                            continue;
                        case "oshirodoor":
                            Add(new MrOshiroDoor(entity, entityOffset));
                            continue;
                        case "payphone":
                            Add(new Payphone(entityOffset + entity.Position));
                            continue;
                        case "picoconsole":
                            Add(new PicoConsole(entity, entityOffset));
                            continue;
                        case "plateau":
                            Add(new Plateau(entity, entityOffset));
                            continue;
                        case "playbackBillboard":
                            Add(new PlaybackBillboard(entity, entityOffset));
                            continue;
                        case "playbackTutorial":
                            Add(new PlayerPlayback(entity, entityOffset));
                            continue;
                        case "playerSeeker":
                            Add(new PlayerSeeker(entity, entityOffset));
                            continue;
                        case "powerSourceNumber":
                            Add(new PowerSourceNumber(entity.Position + entityOffset, entity.Int("number", 1), GotCollectables(entity)));
                            continue;
                        case "redBlocks":
                            ClutterBlockGenerator.Init(this);
                            ClutterBlockGenerator.Add((int) (entity.Position.X / 8.0), (int) (entity.Position.Y / 8.0), entity.Width / 8, entity.Height / 8, ClutterBlock.Colors.Red);
                            continue;
                        case "refill":
                            Add(new Refill(entity, entityOffset));
                            continue;
                        case "reflectionHeartStatue":
                            Add(new ReflectionHeartStatue(entity, entityOffset));
                            continue;
                        case "resortLantern":
                            Add(new ResortLantern(entity, entityOffset));
                            continue;
                        case "resortRoofEnding":
                            Add(new ResortRoofEnding(entity, entityOffset));
                            continue;
                        case "resortmirror":
                            Add(new ResortMirror(entity, entityOffset));
                            continue;
                        case "ridgeGate":
                            if (GotCollectables(entity))
                            {
                                Add(new RidgeGate(entity, entityOffset));
                                continue;
                            }
                            continue;
                        case "risingLava":
                            Add(new RisingLava(entity, entityOffset));
                            continue;
                        case "rotateSpinner":
                            if (Session.Area.ID == 10)
                            {
                                Add(new StarRotateSpinner(entity, entityOffset));
                                continue;
                            }
                            if (Session.Area.ID == 3 || Session.Area.ID == 7 && Session.Level.StartsWith("d-"))
                            {
                                Add(new DustRotateSpinner(entity, entityOffset));
                                continue;
                            }
                            Add(new BladeRotateSpinner(entity, entityOffset));
                            continue;
                        case "rotatingPlatforms":
                            Vector2 vector2_2 = entity.Position + entityOffset;
                            Vector2 center = entity.Nodes[0] + entityOffset;
                            int width = entity.Width;
                            int num2 = entity.Int("platforms");
                            bool clockwise = entity.Bool("clockwise");
                            float length = (vector2_2 - center).Length();
                            float num3 = (vector2_2 - center).Angle();
                            float num4 = 6.28318548f / num2;
                            for (int index2 = 0; index2 < num2; ++index2)
                            {
                                float angleRadians = Calc.WrapAngle(num3 + num4 * index2);
                                Add(new RotatingPlatform(center + Calc.AngleToVector(angleRadians, length), width, center, clockwise));
                            }
                            continue;
                        case "sandwichLava":
                            Add(new SandwichLava(entity, entityOffset));
                            continue;
                        case "seeker":
                            Add(new Seeker(entity, entityOffset));
                            continue;
                        case "seekerBarrier":
                            Add(new SeekerBarrier(entity, entityOffset));
                            continue;
                        case "seekerStatue":
                            Add(new SeekerStatue(entity, entityOffset));
                            continue;
                        case "sinkingPlatform":
                            Add(new SinkingPlatform(entity, entityOffset));
                            continue;
                        case "slider":
                            Add(new Slider(entity, entityOffset));
                            continue;
                        case "soundSource":
                            Add(new SoundSourceEntity(entity, entityOffset));
                            continue;
                        case "spikesDown":
                            Add(new Spikes(entity, entityOffset, Spikes.Directions.Down));
                            continue;
                        case "spikesLeft":
                            Add(new Spikes(entity, entityOffset, Spikes.Directions.Left));
                            continue;
                        case "spikesRight":
                            Add(new Spikes(entity, entityOffset, Spikes.Directions.Right));
                            continue;
                        case "spikesUp":
                            Add(new Spikes(entity, entityOffset, Spikes.Directions.Up));
                            continue;
                        case "spinner":
                            if (Session.Area.ID == 3 || Session.Area.ID == 7 && Session.Level.StartsWith("d-"))
                            {
                                Add(new DustStaticSpinner(entity, entityOffset));
                                continue;
                            }
                            CrystalColor color = CrystalColor.Blue;
                            if (Session.Area.ID == 5)
                                color = CrystalColor.Red;
                            else if (Session.Area.ID == 6)
                                color = CrystalColor.Purple;
                            else if (Session.Area.ID == 10)
                                color = CrystalColor.Rainbow;
                            Add(new CrystalStaticSpinner(entity, entityOffset, color));
                            continue;
                        case "spring":
                            Add(new Spring(entity, entityOffset, Spring.Orientations.Floor));
                            continue;
                        case "starClimbController":
                            Add(new StarJumpController());
                            continue;
                        case "starJumpBlock":
                            Add(new StarJumpBlock(entity, entityOffset));
                            continue;
                        case "strawberry":
                            Add(new Strawberry(entity, entityOffset, entityId));
                            continue;
                        case "summitGemManager":
                            Add(new SummitGemManager(entity, entityOffset));
                            continue;
                        case "summitcheckpoint":
                            Add(new SummitCheckpoint(entity, entityOffset));
                            continue;
                        case "summitcloud":
                            Add(new SummitCloud(entity, entityOffset));
                            continue;
                        case "summitgem":
                            Add(new SummitGem(entity, entityOffset, entityId));
                            continue;
                        case "swapBlock":
                        case "switchBlock":
                            Add(new SwapBlock(entity, entityOffset));
                            continue;
                        case "switchGate":
                            Add(new SwitchGate(entity, entityOffset));
                            continue;
                        case "templeBigEyeball":
                            Add(new TempleBigEyeball(entity, entityOffset));
                            continue;
                        case "templeCrackedBlock":
                            Add(new TempleCrackedBlock(entityId, entity, entityOffset));
                            continue;
                        case "templeEye":
                            Add(new TempleEye(entity, entityOffset));
                            continue;
                        case "templeGate":
                            Add(new TempleGate(entity, entityOffset, levelData.Name));
                            continue;
                        case "templeMirror":
                            Add(new TempleMirror(entity, entityOffset));
                            continue;
                        case "templeMirrorPortal":
                            Add(new TempleMirrorPortal(entity, entityOffset));
                            continue;
                        case "tentacles":
                            Add(new ReflectionTentacles(entity, entityOffset));
                            continue;
                        case "theoCrystal":
                            Add(new TheoCrystal(entity, entityOffset));
                            continue;
                        case "theoCrystalPedestal":
                            Add(new TheoCrystalPedestal(entity, entityOffset));
                            continue;
                        case "torch":
                            Add(new Torch(entity, entityOffset, entityId));
                            continue;
                        case "touchSwitch":
                            Add(new TouchSwitch(entity, entityOffset));
                            continue;
                        case "towerviewer":
                            Add(new Lookout(entity, entityOffset));
                            continue;
                        case "trackSpinner":
                            if (Session.Area.ID == 10)
                            {
                                Add(new StarTrackSpinner(entity, entityOffset));
                                continue;
                            }
                            if (Session.Area.ID == 3 || Session.Area.ID == 7 && Session.Level.StartsWith("d-"))
                            {
                                Add(new DustTrackSpinner(entity, entityOffset));
                                continue;
                            }
                            Add(new BladeTrackSpinner(entity, entityOffset));
                            continue;
                        case "trapdoor":
                            Add(new Trapdoor(entity, entityOffset));
                            continue;
                        case "triggerSpikesDown":
                            Add(new TriggerSpikes(entity, entityOffset, TriggerSpikes.Directions.Down));
                            continue;
                        case "triggerSpikesLeft":
                            Add(new TriggerSpikes(entity, entityOffset, TriggerSpikes.Directions.Left));
                            continue;
                        case "triggerSpikesRight":
                            Add(new TriggerSpikes(entity, entityOffset, TriggerSpikes.Directions.Right));
                            continue;
                        case "triggerSpikesUp":
                            Add(new TriggerSpikes(entity, entityOffset, TriggerSpikes.Directions.Up));
                            continue;
                        case "wallBooster":
                            Add(new WallBooster(entity, entityOffset));
                            continue;
                        case "wallSpringLeft":
                            Add(new Spring(entity, entityOffset, Spring.Orientations.WallLeft));
                            continue;
                        case "wallSpringRight":
                            Add(new Spring(entity, entityOffset, Spring.Orientations.WallRight));
                            continue;
                        case "water":
                            Add(new Water(entity, entityOffset));
                            continue;
                        case "waterfall":
                            Add(new WaterFall(entity, entityOffset));
                            continue;
                        case "wavedashmachine":
                            Add(new WaveDashTutorialMachine(entity, entityOffset));
                            continue;
                        case "whiteblock":
                            Add(new WhiteBlock(entity, entityOffset));
                            continue;
                        case "wire":
                            Add(new Wire(entity, entityOffset));
                            continue;
                        case "yellowBlocks":
                            ClutterBlockGenerator.Init(this);
                            ClutterBlockGenerator.Add((int) (entity.Position.X / 8.0), (int) (entity.Position.Y / 8.0), entity.Width / 8, entity.Height / 8, ClutterBlock.Colors.Yellow);
                            continue;
                        case "zipMover":
                            Add(new ZipMover(entity, entityOffset));
                            continue;
                        default:
                            continue;
                    }
                }
            }
            ClutterBlockGenerator.Generate();
            foreach (EntityData trigger in levelData.Triggers)
            {
                int entityID = trigger.ID + 10000000;
                EntityID id = new(levelData.Name, entityID);
                if (!Session.DoNotLoad.Contains(id))
                {
                    switch (trigger.Name)
                    {
                        case "altMusicTrigger":
                            Add(new AltMusicTrigger(trigger, entityOffset));
                            continue;
                        case "ambienceParamTrigger":
                            Add(new AmbienceParamTrigger(trigger, entityOffset));
                            continue;
                        case "birdPathTrigger":
                            Add(new BirdPathTrigger(trigger, entityOffset));
                            continue;
                        case "blackholeStrength":
                            Add(new BlackholeStrengthTrigger(trigger, entityOffset));
                            continue;
                        case "bloomFadeTrigger":
                            Add(new BloomFadeTrigger(trigger, entityOffset));
                            continue;
                        case "cameraAdvanceTargetTrigger":
                            Add(new CameraAdvanceTargetTrigger(trigger, entityOffset));
                            continue;
                        case "cameraOffsetTrigger":
                            Add(new CameraOffsetTrigger(trigger, entityOffset));
                            continue;
                        case "cameraTargetTrigger":
                            string flag7 = trigger.Attr("deleteFlag");
                            if (string.IsNullOrEmpty(flag7) || !Session.GetFlag(flag7))
                            {
                                Add(new CameraTargetTrigger(trigger, entityOffset));
                                continue;
                            }
                            continue;
                        case "changeRespawnTrigger":
                            Add(new ChangeRespawnTrigger(trigger, entityOffset));
                            continue;
                        case "checkpointBlockerTrigger":
                            Add(new CheckpointBlockerTrigger(trigger, entityOffset));
                            continue;
                        case "creditsTrigger":
                            Add(new CreditsTrigger(trigger, entityOffset));
                            continue;
                        case "detachFollowersTrigger":
                            Add(new DetachStrawberryTrigger(trigger, entityOffset));
                            continue;
                        case "eventTrigger":
                            Add(new EventTrigger(trigger, entityOffset));
                            continue;
                        case "goldenBerryCollectTrigger":
                            Add(new GoldBerryCollectTrigger(trigger, entityOffset));
                            continue;
                        case "interactTrigger":
                            Add(new InteractTrigger(trigger, entityOffset));
                            continue;
                        case "lightFadeTrigger":
                            Add(new LightFadeTrigger(trigger, entityOffset));
                            continue;
                        case "lookoutBlocker":
                            Add(new LookoutBlocker(trigger, entityOffset));
                            continue;
                        case "minitextboxTrigger":
                            Add(new MiniTextboxTrigger(trigger, entityOffset, id));
                            continue;
                        case "moonGlitchBackgroundTrigger":
                            Add(new MoonGlitchBackgroundTrigger(trigger, entityOffset));
                            continue;
                        case "musicFadeTrigger":
                            Add(new MusicFadeTrigger(trigger, entityOffset));
                            continue;
                        case "musicTrigger":
                            Add(new MusicTrigger(trigger, entityOffset));
                            continue;
                        case "noRefillTrigger":
                            Add(new NoRefillTrigger(trigger, entityOffset));
                            continue;
                        case "oshiroTrigger":
                            Add(new OshiroTrigger(trigger, entityOffset));
                            continue;
                        case "respawnTargetTrigger":
                            Add(new RespawnTargetTrigger(trigger, entityOffset));
                            continue;
                        case "rumbleTrigger":
                            Add(new RumbleTrigger(trigger, entityOffset, id));
                            continue;
                        case "spawnFacingTrigger":
                            Add(new SpawnFacingTrigger(trigger, entityOffset));
                            continue;
                        case "stopBoostTrigger":
                            Add(new StopBoostTrigger(trigger, entityOffset));
                            continue;
                        case "windAttackTrigger":
                            Add(new WindAttackTrigger(trigger, entityOffset));
                            continue;
                        case "windTrigger":
                            Add(new WindTrigger(trigger, entityOffset));
                            continue;
                        default:
                            continue;
                    }
                }
            }

            foreach (DecalData fgDecal in levelData.FgDecals)
                Add(new Decal(fgDecal.Texture, entityOffset + fgDecal.Position, fgDecal.Scale, -10500));
            foreach (DecalData bgDecal in levelData.BgDecals)
                Add(new Decal(bgDecal.Texture, entityOffset + bgDecal.Position, bgDecal.Scale, 9000));

            if (playerIntro != Player.IntroTypes.Transition)
            {
                if (Session.JustStarted && !Session.StartedFromBeginning && nullable.HasValue && !StartPosition.HasValue)
                    StartPosition = nullable;
                if (!Session.RespawnPoint.HasValue)
                    Session.RespawnPoint = !StartPosition.HasValue ? new Vector2?(DefaultSpawnPoint) : new Vector2?(GetSpawnPoint(StartPosition.Value));
                Player player = new(Session.RespawnPoint.Value, Session.Inventory.Backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack)
                {
                    IntroType = playerIntro
                };
                Add(player);
                Entities.UpdateLists();
                CameraLockModes cameraLockMode = CameraLockMode;
                CameraLockMode = CameraLockModes.None;
                Camera.Position = GetFullCameraTargetAt(player, player.Position);
                CameraLockMode = cameraLockMode;
                CameraUpwardMaxY = Camera.Y + 180f;
                foreach (EntityID key in Session.Keys)
                    Add(new Key(player, key));
                SpotlightWipe.FocusPoint = Session.RespawnPoint.Value - Camera.Position;
                if (playerIntro is not Player.IntroTypes.Respawn and not Player.IntroTypes.Fall)
                {
                    SpotlightWipe spotlightWipe = new(this, true);
                }
                else
                    DoScreenWipe(true);
                if (isFromLoader)
                    RendererList.UpdateLists();
                Lighting.Alpha = !DarkRoom ? BaseLightingAlpha + Session.LightingAlphaAdd : Session.DarkRoomAlpha;
                Bloom.Base = AreaData.Get(Session).BloomBase + Session.BloomBaseAdd;
            }
            else
                Entities.UpdateLists();
            if (HasCassetteBlocks && ShouldCreateCassetteManager)
                Tracker.GetEntity<CassetteBlockManager>()?.OnLevelStart();
            if (!string.IsNullOrEmpty(levelData.ObjTiles))
            {
                Tileset tileset = new(GFX.Game["tilesets/scenery"], 8, 8);
                int[,] numArray = Calc.ReadCSVIntGrid(levelData.ObjTiles, Bounds.Width / 8, Bounds.Height / 8);
                for (int index3 = 0; index3 < numArray.GetLength(0); ++index3)
                {
                    for (int index4 = 0; index4 < numArray.GetLength(1); ++index4)
                    {
                        if (numArray[index3, index4] != -1)
                            TileInterceptor.TileCheck(this, tileset[numArray[index3, index4]], new Vector2(index3 * 8, index4 * 8) + LevelOffset);
                    }
                }
            }
            LightningRenderer entity3 = Tracker.GetEntity<LightningRenderer>();
            if (entity3 != null)
            {
                if (flag2)
                    entity3.StartAmbience();
                else
                    entity3.StopAmbience();
            }
            Calc.PopRandom();
        }

        public void UnloadLevel()
        {
            List<Entity> excludingTagMask = GetEntitiesExcludingTagMask((int) Tags.Global);
            foreach (Entity entity in Tracker.GetEntities<Textbox>())
                excludingTagMask.Add(entity);
            UnloadEntities(excludingTagMask);
            Entities.UpdateLists();
        }

        public void Reload()
        {
            if (Completed)
                return;
            if (Session.FirstLevel && Session.Strawberries.Count <= 0 && !Session.Cassette && !Session.HeartGem && !Session.HitCheckpoint)
            {
                Session.Time = 0L;
                Session.Deaths = 0;
                TimerStarted = false;
            }
            Session.Dashes = Session.DashesAtLevelStart;
            Glitch.Value = 0.0f;
            Engine.TimeRate = 1f;
            Distort.Anxiety = 0.0f;
            Distort.GameRate = 1f;
            Audio.SetMusicParam("fade", 1f);
            ParticlesBG.Clear();
            Particles.Clear();
            ParticlesFG.Clear();
            TrailManager.Clear();
            UnloadLevel();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            LoadLevel(Player.IntroTypes.Respawn);
            strawberriesDisplay.DrawLerp = 0.0f;
            WindController first = Entities.FindFirst<WindController>();
            if (first != null)
                first.SnapWind();
            else
                Wind = Vector2.Zero;
        }

        private bool ShouldCreateCassetteManager => Session.Area.Mode != AreaMode.Normal || !Session.Cassette;

        private bool GotCollectables(EntityData e)
        {
            bool flag1 = true;
            bool flag2 = true;
            List<EntityID> entityIdList1 = new();
            if (e.Attr("strawberries").Length > 0)
            {
                string str1 = e.Attr("strawberries");
                char[] chArray1 = new char[1]{ ',' };
                foreach (string str2 in str1.Split(chArray1))
                {
                    EntityID none = EntityID.None;
                    char[] chArray2 = new char[1]{ ':' };
                    string[] strArray = str2.Split(chArray2);
                    none.Level = strArray[0];
                    none.ID = Convert.ToInt32(strArray[1]);
                    entityIdList1.Add(none);
                }
            }
            foreach (EntityID entityId in entityIdList1)
            {
                if (!Session.Strawberries.Contains(entityId))
                {
                    flag1 = false;
                    break;
                }
            }
            List<EntityID> entityIdList2 = new();
            if (e.Attr("keys").Length > 0)
            {
                string str3 = e.Attr("keys");
                char[] chArray3 = new char[1]{ ',' };
                foreach (string str4 in str3.Split(chArray3))
                {
                    EntityID none = EntityID.None;
                    char[] chArray4 = new char[1]{ ':' };
                    string[] strArray = str4.Split(chArray4);
                    none.Level = strArray[0];
                    none.ID = Convert.ToInt32(strArray[1]);
                    entityIdList2.Add(none);
                }
            }
            foreach (EntityID entityId in entityIdList2)
            {
                if (!Session.DoNotLoad.Contains(entityId))
                {
                    flag2 = false;
                    break;
                }
            }
            return flag2 & flag1;
        }

        public void TransitionTo(LevelData next, Vector2 direction)
        {
            Session.CoreMode = CoreMode;
            transition = new Coroutine(TransitionRoutine(next, direction));
        }

        private IEnumerator TransitionRoutine(LevelData next, Vector2 direction)
        {
            Level level = this;
            Player player = level.Tracker.GetEntity<Player>();
            List<Entity> toRemove = level.GetEntitiesExcludingTagMask((int) Tags.Persistent | (int) Tags.Global);
            List<Component> transitionOut = level.Tracker.GetComponentsCopy<TransitionListener>();
            player.CleanUpTriggers();
            foreach (SoundSource component in level.Tracker.GetComponents<SoundSource>())
            {
                if (component.DisposeOnTransition)
                    component.Stop();
            }
            level.PreviousBounds = new Rectangle?(level.Bounds);
            level.Session.Level = next.Name;
            level.Session.FirstLevel = false;
            level.Session.DeathsInCurrentLevel = 0;
            level.LoadLevel(Player.IntroTypes.Transition);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "has_conveyors", level.Tracker.GetEntities<WallBooster>().Count > 0 ? 1f : 0.0f);
            List<Component> transitionIn = level.Tracker.GetComponentsCopy<TransitionListener>();
            transitionIn.RemoveAll(c => transitionOut.Contains(c));
            GC.Collect();
            float cameraAt = 0.0f;
            Vector2 cameraFrom = level.Camera.Position;
            Vector2 dirPad = direction * 4f;
            if (direction == Vector2.UnitY)
                dirPad = direction * 12f;
            Vector2 playerTo = player.Position;
            while (direction.X != 0.0 && playerTo.Y >= (double) level.Bounds.Bottom)
                --playerTo.Y;
            while (!level.IsInBounds(playerTo, dirPad))
                playerTo += direction;
            Vector2 cameraTo = level.GetFullCameraTargetAt(player, playerTo);
            Vector2 position = player.Position;
            player.Position = playerTo;
            foreach (Entity entity in player.CollideAll<WindTrigger>())
            {
                if (!toRemove.Contains(entity))
                {
                    level.windController.SetPattern((entity as WindTrigger).Pattern);
                    break;
                }
            }
            level.windController.SetStartPattern();
            player.Position = position;
            foreach (TransitionListener transitionListener in transitionOut)
            {
                if (transitionListener.OnOutBegin != null)
                    transitionListener.OnOutBegin();
            }
            foreach (TransitionListener transitionListener in transitionIn)
            {
                if (transitionListener.OnInBegin != null)
                    transitionListener.OnInBegin();
            }
            float lightingStart = level.Lighting.Alpha;
            float lightingEnd = level.DarkRoom ? level.Session.DarkRoomAlpha : level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
            bool lightingWait = (double) lightingStart >= level.Session.DarkRoomAlpha || (double) lightingEnd >= level.Session.DarkRoomAlpha;
            if ((double) lightingEnd > (double) lightingStart & lightingWait)
            {
                Audio.Play("event:/game/05_mirror_temple/room_lightlevel_down");
                for (; level.Lighting.Alpha != (double) lightingEnd; level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, lightingEnd, 2f * Engine.DeltaTime))
                    yield return null;
            }
            bool cameraFinished = false;
            while (!player.TransitionTo(playerTo, direction) || (double) cameraAt < 1.0)
            {
                yield return null;
                if (!cameraFinished)
                {
                    cameraAt = Calc.Approach(cameraAt, 1f, Engine.DeltaTime / level.NextTransitionDuration);
                    level.Camera.Position = (double) cameraAt <= 0.89999997615814209 ? Vector2.Lerp(cameraFrom, cameraTo, Ease.CubeOut(cameraAt)) : cameraTo;
                    if (!lightingWait && (double) lightingStart < (double) lightingEnd)
                        level.Lighting.Alpha = lightingStart + (lightingEnd - lightingStart) * cameraAt;
                    foreach (TransitionListener transitionListener in transitionOut)
                    {
                        if (transitionListener.OnOut != null)
                            transitionListener.OnOut(cameraAt);
                    }
                    foreach (TransitionListener transitionListener in transitionIn)
                    {
                        if (transitionListener.OnIn != null)
                            transitionListener.OnIn(cameraAt);
                    }
                    if ((double) cameraAt >= 1.0)
                        cameraFinished = true;
                }
            }
            if ((double) lightingEnd < (double) lightingStart & lightingWait)
            {
                Audio.Play("event:/game/05_mirror_temple/room_lightlevel_up");
                for (; level.Lighting.Alpha != (double) lightingEnd; level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, lightingEnd, 2f * Engine.DeltaTime))
                    yield return null;
            }
            level.UnloadEntities(toRemove);
            level.Entities.UpdateLists();
            Rectangle bounds = level.Bounds;
            bounds.Inflate(16, 16);
            level.Particles.ClearRect(bounds, false);
            level.ParticlesBG.ClearRect(bounds, false);
            level.ParticlesFG.ClearRect(bounds, false);
            RespawnTargetTrigger respawnTargetTrigger = player.CollideFirst<RespawnTargetTrigger>();
            Vector2 to = respawnTargetTrigger != null ? respawnTargetTrigger.Target : player.Position;
            level.Session.RespawnPoint = new Vector2?(level.Session.LevelData.Spawns.ClosestTo(to));
            player.OnTransition();
            foreach (TransitionListener transitionListener in transitionIn)
            {
                if (transitionListener.OnInEnd != null)
                    transitionListener.OnInEnd();
            }
            if (level.Session.LevelData.DelayAltMusic)
                Audio.SetAltMusic(SFX.EventnameByHandle(level.Session.LevelData.AltMusic));
            cameraFrom = new Vector2();
            playerTo = new Vector2();
            cameraTo = new Vector2();
            level.NextTransitionDuration = 0.65f;
            level.transition = null;
        }

        public void UnloadEntities(List<Entity> entities)
        {
            foreach (Entity entity in entities)
                Remove(entity);
        }

        public Vector2 DefaultSpawnPoint
        {
            get
            {
                Rectangle bounds = Bounds;
                double left = bounds.Left;
                bounds = Bounds;
                double bottom = bounds.Bottom;
                return GetSpawnPoint(new Vector2((float) left, (float) bottom));
            }
        }

        public Vector2 GetSpawnPoint(Vector2 from) => Session.GetSpawnPoint(from);

        public Vector2 GetFullCameraTargetAt(Player player, Vector2 at)
        {
            Vector2 position = player.Position;
            player.Position = at;
            foreach (Entity entity in Tracker.GetEntities<Trigger>())
            {
                if (entity is CameraTargetTrigger && player.CollideCheck(entity))
                    (entity as CameraTargetTrigger).OnStay(player);
                else if (entity is CameraOffsetTrigger && player.CollideCheck(entity))
                    (entity as CameraOffsetTrigger).OnEnter(player);
            }
            Vector2 cameraTarget = player.CameraTarget;
            player.Position = position;
            return cameraTarget;
        }

        public Rectangle Bounds => Session.LevelData.Bounds;

        public Rectangle? PreviousBounds { get; private set; }

        public void TeleportTo(
            Player player,
            string nextLevel,
            Player.IntroTypes introType,
            Vector2? nearestSpawn = null)
        {
            Leader.StoreStrawberries(player.Leader);
            Vector2 position = player.Position;
            Remove(player);
            UnloadLevel();
            Session.Level = nextLevel;
            Session session = Session;
            Rectangle bounds = Bounds;
            double left = bounds.Left;
            bounds = Bounds;
            double top = bounds.Top;
            Vector2? nullable = new Vector2?(GetSpawnPoint(new Vector2((float) left, (float) top) + (nearestSpawn.HasValue ? nearestSpawn.Value : Vector2.Zero)));
            session.RespawnPoint = nullable;
            if (introType == Player.IntroTypes.Transition)
            {
                player.Position = Session.RespawnPoint.Value;
                player.Hair.MoveHairBy(player.Position - position);
                player.MuffleLanding = true;
                Add(player);
                LoadLevel(Player.IntroTypes.Transition);
                Entities.UpdateLists();
            }
            else
            {
                LoadLevel(introType);
                Entities.UpdateLists();
                player = Tracker.GetEntity<Player>();
            }
            Camera.Position = player.CameraTarget;
            Update();
            Leader.RestoreStrawberries(player.Leader);
        }

        public void AutoSave()
        {
            if (saving != null)
                return;
            saving = new Coroutine(SavingRoutine());
        }

        public bool IsAutoSaving() => saving != null;

        private IEnumerator SavingRoutine()
        {
            UserIO.SaveHandler(true, false);
            while (UserIO.Saving)
                yield return null;
            saving = null;
        }

        public void UpdateTime()
        {
            if (InCredits || Session.Area.ID == 8 || TimerStopped)
                return;
            long ticks = TimeSpan.FromSeconds((double) Engine.RawDeltaTime).Ticks;
            SaveData.Instance.AddTime(Session.Area, ticks);
            if (!TimerStarted && !InCutscene)
            {
                Player entity = Tracker.GetEntity<Player>();
                if (entity != null && !entity.TimePaused)
                    TimerStarted = true;
            }
            if (Completed || !TimerStarted)
                return;
            Session.Time += ticks;
        }

        public override void Update()
        {
            if (unpauseTimer > 0.0)
            {
                unpauseTimer -= Engine.RawDeltaTime;
                UpdateTime();
            }
            else if (Overlay != null)
            {
                Overlay.Update();
                Entities.UpdateLists();
            }
            else
            {
                int num1 = 10;
                if (!InCutscene && Tracker.GetEntity<Player>() != null && Wipe == null && !Frozen)
                    num1 = SaveData.Instance.Assists.GameSpeed;
                Engine.TimeRateB = num1 / 10f;
                if (num1 != 10)
                {
                    if (AssistSpeedSnapshot == null || Level.AssistSpeedSnapshotValue != num1)
                    {
                        Audio.ReleaseSnapshot(Level.AssistSpeedSnapshot);
                        Level.AssistSpeedSnapshot = null;
                        Level.AssistSpeedSnapshotValue = num1;
                        if (Level.AssistSpeedSnapshotValue < 10)
                            Level.AssistSpeedSnapshot = Audio.CreateSnapshot("snapshot:/assist_game_speed/assist_speed_" + Level.AssistSpeedSnapshotValue * 10);
                        else if (Level.AssistSpeedSnapshotValue <= 16)
                            Level.AssistSpeedSnapshot = Audio.CreateSnapshot("snapshot:/variant_speed/variant_speed_" + Level.AssistSpeedSnapshotValue * 10);
                    }
                }
                else if (AssistSpeedSnapshot != null)
                {
                    Audio.ReleaseSnapshot(Level.AssistSpeedSnapshot);
                    Level.AssistSpeedSnapshot = null;
                    Level.AssistSpeedSnapshotValue = -1;
                }
                if (wasPaused && !Paused)
                    EndPauseEffects();
                if (CanPause && Input.QuickRestart.Pressed)
                {
                    Input.QuickRestart.ConsumeBuffer();
                    Pause(quickReset: true);
                }
                else if (CanPause && (Input.Pause.Pressed || Input.ESC.Pressed))
                {
                    Input.Pause.ConsumeBuffer();
                    Input.ESC.ConsumeBuffer();
                    Pause();
                }
                if (wasPaused && !Paused)
                    wasPaused = false;
                if (Paused)
                    wasPausedTimer = 0.0f;
                else
                    wasPausedTimer += Engine.DeltaTime;
                UpdateTime();
                saving?.Update();
                if (!Paused)
                {
                    glitchTimer += Engine.DeltaTime;
                    glitchSeed = Calc.Random.NextFloat();
                }
                if (SkippingCutscene)
                {
                    skipCoroutine?.Update();
                    RendererList.Update();
                }
                else if (FrozenOrPaused)
                {
                    bool disabled = MInput.Disabled;
                    MInput.Disabled = false;
                    if (!Paused)
                    {
                        foreach (Entity entity in this[Tags.FrozenUpdate])
                        {
                            if (entity.Active)
                                entity.Update();
                        }
                    }
                    foreach (Entity entity in this[Tags.PauseUpdate])
                    {
                        if (entity.Active)
                            entity.Update();
                    }
                    MInput.Disabled = disabled;
                    Wipe?.Update(this);
                    HiresSnow?.Update(this);
                    Entities.UpdateLists();
                }
                else if (!Transitioning)
                {
                    if (RetryPlayerCorpse == null)
                    {
                        base.Update();
                    }
                    else
                    {
                        RetryPlayerCorpse.Update();
                        RendererList.Update();
                        foreach (Entity entity in this[Tags.PauseUpdate])
                        {
                            if (entity.Active)
                                entity.Update();
                        }
                    }
                }
                else
                {
                    foreach (Entity entity in this[Tags.TransitionUpdate])
                        entity.Update();
                    transition.Update();
                    RendererList.Update();
                }
                HudRenderer.BackgroundFade = Calc.Approach(HudRenderer.BackgroundFade, Paused ? 1f : 0.0f, 8f * Engine.RawDeltaTime);
                if (!FrozenOrPaused)
                {
                    WindSineTimer += Engine.DeltaTime;
                    WindSine = (float) (Math.Sin(WindSineTimer) + 1.0) / 2f;
                }
                foreach (PostUpdateHook component in Tracker.GetComponents<PostUpdateHook>())
                {
                    if (component.Entity.Active)
                        component.OnPostUpdate();
                }
                if (updateHair)
                {
                    foreach (Component component in Tracker.GetComponents<PlayerHair>())
                    {
                        if (component.Active && component.Entity.Active)
                            (component as PlayerHair).AfterUpdate();
                    }
                    if (FrozenOrPaused)
                        updateHair = false;
                }
                else if (!FrozenOrPaused)
                    updateHair = true;
                if (shakeTimer > 0.0)
                {
                    if (OnRawInterval(0.04f))
                    {
                        int num2 = (int) Math.Ceiling(shakeTimer * 10.0);
                        if (shakeDirection == Vector2.Zero)
                        {
                            ShakeVector = new Vector2(-num2 + Calc.Random.Next(num2 * 2 + 1), -num2 + Calc.Random.Next(num2 * 2 + 1));
                        }
                        else
                        {
                            if (lastDirectionalShake == 0)
                                lastDirectionalShake = 1;
                            else
                                lastDirectionalShake *= -1;
                            ShakeVector = -shakeDirection * lastDirectionalShake * num2;
                        }
                        if (Settings.Instance.ScreenShake == ScreenshakeAmount.Half)
                            ShakeVector = new Vector2(Math.Sign(ShakeVector.X), Math.Sign(ShakeVector.Y));
                    }
                    shakeTimer -= Engine.RawDeltaTime * (Settings.Instance.ScreenShake == ScreenshakeAmount.Half ? 1.5f : 1f);
                }
                else
                    ShakeVector = Vector2.Zero;
                if (doFlash)
                {
                    flash = Calc.Approach(flash, 1f, Engine.DeltaTime * 10f);
                    if (flash >= 1.0)
                        doFlash = false;
                }
                else if (flash > 0.0)
                    flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 3f);
                if (lastColorGrade != Session.ColorGrade)
                {
                    if (colorGradeEase >= 1.0)
                    {
                        colorGradeEase = 0.0f;
                        lastColorGrade = Session.ColorGrade;
                    }
                    else
                        colorGradeEase = Calc.Approach(colorGradeEase, 1f, Engine.DeltaTime * colorGradeEaseSpeed);
                }
                if (Celeste.PlayMode != Celeste.PlayModes.Debug)
                    return;
                if (MInput.Keyboard.Pressed(Keys.Tab) && Engine.Scene.Tracker.GetEntity<KeyboardConfigUI>() == null && Engine.Scene.Tracker.GetEntity<ButtonConfigUI>() == null)
                    Engine.Scene = new MapEditor(Session.Area);
                if (MInput.Keyboard.Pressed(Keys.F1))
                {
                    Celeste.ReloadAssets(true, false, false, new AreaKey?(Session.Area));
                    Engine.Scene = new LevelLoader(Session);
                }
                else if (MInput.Keyboard.Pressed(Keys.F2))
                {
                    Celeste.ReloadAssets(true, true, false, new AreaKey?(Session.Area));
                    Engine.Scene = new LevelLoader(Session);
                }
                else if(MInput.Keyboard.Pressed(Keys.F3))
                {
                    Celeste.ReloadAssets(true, true, true, new AreaKey?(Session.Area));
                    Engine.Scene = new LevelLoader(Session);
                }
            }
        }

        public override void BeforeRender()
        {
            cameraPreShake = Camera.Position;
            Camera.Position += ShakeVector;
            Camera.Position = Camera.Position.Floor();
            foreach (BeforeRenderHook component in Tracker.GetComponents<BeforeRenderHook>())
            {
                if (component.Visible)
                    component.Callback();
            }
            SpeedRing.DrawToBuffer(this);
            base.BeforeRender();
        }

        public override void Render()
        {
            Engine.Instance.GraphicsDevice.SetRenderTarget((RenderTarget2D) GameplayBuffers.Gameplay);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            GameplayRenderer.Render(this);
            Lighting.Render(this);
            Engine.Instance.GraphicsDevice.SetRenderTarget((RenderTarget2D) GameplayBuffers.Level);
            Engine.Instance.GraphicsDevice.Clear(BackgroundColor);
            Background.Render(this);
            Distort.Render((RenderTarget2D)GameplayBuffers.Gameplay, (RenderTarget2D)GameplayBuffers.Displacement, Displacement.HasDisplacement(this));
            Bloom.Apply(GameplayBuffers.Level, this);
            Foreground.Render(this);
            Glitch.Apply(GameplayBuffers.Level, glitchTimer * 2f, glitchSeed, MathHelper.Pi * 2);
            if (Engine.DashAssistFreeze)
            {
                PlayerDashAssist entity = Tracker.GetEntity<PlayerDashAssist>();
                if (entity != null)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
                    entity.Render();
                    Draw.SpriteBatch.End();
                }
            }
            if (flash > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                Draw.Rect(-1f, -1f, 322f, 182f, flashColor * flash);
                Draw.SpriteBatch.End();
                if (flashDrawPlayer)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
                    Player entity = Tracker.GetEntity<Player>();
                    if (entity != null && entity.Visible)
                        entity.Render();
                    Draw.SpriteBatch.End();
                }
            }
            Engine.Instance.GraphicsDevice.SetRenderTarget(null);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            Engine.Instance.GraphicsDevice.Viewport = Engine.Viewport;
            Matrix transformMatrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
            Vector2 vector2_1 = new(320f, 180f);
            Vector2 vector2_2 = vector2_1 / ZoomTarget;
            Vector2 origin = ZoomTarget != 1.0 ? (ZoomFocusPoint - vector2_2 / 2f) / (vector2_1 - vector2_2) * vector2_1 : Vector2.Zero;
            MTexture orDefault1 = GFX.ColorGrades.GetOrDefault(lastColorGrade, GFX.ColorGrades["none"]);
            MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(Session.ColorGrade, GFX.ColorGrades["none"]);
            if (colorGradeEase > 0.0 && orDefault1 != orDefault2)
                ColorGrade.Set(orDefault1, orDefault2, colorGradeEase);
            else
                ColorGrade.Set(orDefault2);
            float scale = Zoom * (320f - ScreenPadding * 2f) / 320f;
            Vector2 vector2_3 = new(ScreenPadding, ScreenPadding * (9f / 16f));
            if (SaveData.Instance.Assists.MirrorMode)
            {
                vector2_3.X = -vector2_3.X;
                origin.X = 160f - (origin.X - 160f);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, transformMatrix);
            Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Level, origin + vector2_3, new Rectangle?(GameplayBuffers.Level.Bounds), Color.White, 0.0f, origin, scale, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
            Draw.SpriteBatch.End();
            if (Pathfinder != null && Pathfinder.DebugRenderEnabled)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix * transformMatrix);
                Pathfinder.Render();
                Draw.SpriteBatch.End();
            }
            if ((!Paused || !PauseMainMenuOpen) && wasPausedTimer >= 1.0 || !Input.MenuJournal.Check || !AllowHudHide)
                HudRenderer.Render(this);
            Wipe?.Render(this);
            if (HiresSnow == null)
                return;
            HiresSnow.Render(this);
        }

        public override void AfterRender()
        {
            base.AfterRender();
            Camera.Position = cameraPreShake;
        }

        private void StartPauseEffects()
        {
            if (Audio.CurrentMusic == "event:/music/lvl0/bridge")
                Audio.PauseMusic = true;
            Audio.PauseGameplaySfx = true;
            Audio.Play("event:/ui/game/pause");
            if (!(PauseSnapshot == null))
                return;
            Level.PauseSnapshot = Audio.CreateSnapshot("snapshot:/pause_menu");
        }

        private void EndPauseEffects()
        {
            Audio.PauseMusic = false;
            Audio.PauseGameplaySfx = false;
            Audio.ReleaseSnapshot(Level.PauseSnapshot);
            Level.PauseSnapshot = null;
        }

        public void Pause(int startIndex = 0, bool minimal = false, bool quickReset = false)
        {
            wasPaused = true;
            Player player = Tracker.GetEntity<Player>();
            if (!Paused)
                StartPauseEffects();
            Paused = true;
            if (quickReset)
            {
                Audio.Play("event:/ui/main/message_confirm");
                PauseMainMenuOpen = false;
                GiveUp(0, true, minimal, false);
            }
            else
            {
                PauseMainMenuOpen = true;
                TextMenu menu = new();
                if (!minimal)
                    menu.Add(new TextMenu.Header(Dialog.Clean("menu_pause_title")));
                menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_resume")).Pressed(() => menu.OnCancel()));
                if (InCutscene && !SkippingCutscene)
                    menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_skip_cutscene")).Pressed(() =>
                    {
                        SkipCutscene();
                        Paused = false;
                        PauseMainMenuOpen = false;
                        menu.RemoveSelf();
                    }));
                if (!minimal && !InCutscene && !SkippingCutscene)
                {
                    TextMenu.Item obj;
                    menu.Add(obj = new TextMenu.Button(Dialog.Clean("menu_pause_retry")).Pressed(() =>
                    {
                        if (player != null && !player.Dead)
                        {
                            Engine.TimeRate = 1f;
                            Distort.GameRate = 1f;
                            Distort.Anxiety = 0.0f;
                            InCutscene = SkippingCutscene = false;
                            RetryPlayerCorpse = player.Die(Vector2.Zero, true);
                            foreach (LevelEndingHook component in Tracker.GetComponents<LevelEndingHook>())
                            {
                                if (component.OnEnd != null)
                                    component.OnEnd();
                            }
                        }
                        Paused = false;
                        PauseMainMenuOpen = false;
                        EndPauseEffects();
                        menu.RemoveSelf();
                    }));
                    obj.Disabled = !CanRetry || player != null && !player.CanRetry || Frozen || Completed;
                }
                if (!minimal && SaveData.Instance.AssistMode)
                {
                    TextMenu.Item item = null;
                    menu.Add(item = new TextMenu.Button(Dialog.Clean("menu_pause_assist")).Pressed(() =>
                    {
                        menu.RemoveSelf();
                        PauseMainMenuOpen = false;
                        AssistMode(menu.IndexOf(item), minimal);
                    }));
                }
                if (!minimal && SaveData.Instance.VariantMode)
                {
                    TextMenu.Item item = null;
                    menu.Add(item = new TextMenu.Button(Dialog.Clean("menu_pause_variant")).Pressed(() =>
                    {
                        menu.RemoveSelf();
                        PauseMainMenuOpen = false;
                        VariantMode(menu.IndexOf(item), minimal);
                    }));
                }
                TextMenu.Item item1 = null;
                menu.Add(item1 = new TextMenu.Button(Dialog.Clean("menu_pause_options")).Pressed(() =>
                {
                    menu.RemoveSelf();
                    PauseMainMenuOpen = false;
                    Options(menu.IndexOf(item1), minimal);
                }));
                if (!minimal && Celeste.PlayMode != Celeste.PlayModes.Event)
                {
                    TextMenu.Item obj;
                    _ = menu.Add(obj = new TextMenu.Button(Dialog.Clean("menu_pause_savequit")).Pressed(() =>
                    {
                        menu.Focused = false;
                        Engine.TimeRate = 1f;
                        Audio.SetMusic(null);
                        Audio.BusStopAll("bus:/gameplay_sfx", true);
                        Session.InArea = true;
                        ++Session.Deaths;
                        ++Session.DeathsInCurrentLevel;
                        SaveData.Instance.AddDeath(Session.Area);
                        DoScreenWipe(false, () => Engine.Scene = new LevelExit(LevelExit.Mode.SaveAndQuit, Session, HiresSnow), true);
                        foreach (LevelEndingHook component in Tracker.GetComponents<LevelEndingHook>())
                        {
                            component.OnEnd?.Invoke();
                        }
                    }));
                    if (SaveQuitDisabled || player != null && player.StateMachine.State == 18)
                        obj.Disabled = true;
                }
                if (!minimal)
                {
                    menu.Add(new TextMenu.SubHeader(""));
                    TextMenu.Item item2 = null;
                    menu.Add(item2 = new TextMenu.Button(Dialog.Clean("menu_pause_restartarea")).Pressed(() =>
                    {
                        PauseMainMenuOpen = false;
                        menu.RemoveSelf();
                        GiveUp(menu.IndexOf(item2), true, minimal, true);
                    }));
                    (item2 as TextMenu.Button).ConfirmSfx = "event:/ui/main/message_confirm";
                    if (SaveData.Instance.Areas[0].Modes[0].Completed || SaveData.Instance.DebugMode || SaveData.Instance.CheatMode)
                    {
                        TextMenu.Item item3 = null;
                        menu.Add(item3 = new TextMenu.Button(Dialog.Clean("menu_pause_return")).Pressed(() =>
                        {
                            PauseMainMenuOpen = false;
                            menu.RemoveSelf();
                            GiveUp(menu.IndexOf(item3), false, minimal, false);
                        }));
                        (item3 as TextMenu.Button).ConfirmSfx = "event:/ui/main/message_confirm";
                    }
                    if (Celeste.PlayMode == Celeste.PlayModes.Event)
                        menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_restartdemo")).Pressed(() =>
                        {
                            EndPauseEffects();
                            Audio.SetMusic(null);
                            menu.Focused = false;
                            DoScreenWipe(false, () => LevelEnter.Go(new Session(new AreaKey(0)), false));
                        }));
                }
                menu.OnESC = menu.OnCancel = menu.OnPause = () =>
                {
                    PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    Paused = false;
                    Audio.Play("event:/ui/game/unpause");
                    unpauseTimer = 0.15f;
                };
                if (startIndex > 0)
                    menu.Selection = startIndex;
                Add(menu);
            }
        }

        private void GiveUp(int returnIndex, bool restartArea, bool minimal, bool showHint)
        {
            Paused = true;
            QuickResetHint quickHint = null;
            ReturnMapHint returnHint = null;
            if (!restartArea)
                Add(returnHint = new ReturnMapHint());
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, (float) (Engine.Height / 2.0 - 100.0));
            menu.Add(new TextMenu.Header(Dialog.Clean(restartArea ? "menu_restart_title" : "menu_return_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean(restartArea ? "menu_restart_continue" : "menu_return_continue")).Pressed(() =>
            {
                Engine.TimeRate = 1f;
                menu.Focused = false;
                Session.InArea = false;
                Audio.SetMusic(null);
                Audio.BusStopAll("bus:/gameplay_sfx", true);
                if (restartArea)
                    DoScreenWipe(false, () => Engine.Scene = new LevelExit(LevelExit.Mode.Restart, Session));
                else
                    DoScreenWipe(false, () => Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, Session, HiresSnow), true);
                foreach (LevelEndingHook component in Tracker.GetComponents<LevelEndingHook>())
                {
                    if (component.OnEnd != null)
                        component.OnEnd();
                }
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean(restartArea ? "menu_restart_cancel" : "menu_return_cancel")).Pressed(() => menu.OnCancel()));
            menu.OnPause = menu.OnESC = () =>
            {
                menu.RemoveSelf();
                quickHint?.RemoveSelf();
                returnHint?.RemoveSelf();
                Paused = false;
                unpauseTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            };
            menu.OnCancel = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                quickHint?.RemoveSelf();
                returnHint?.RemoveSelf();
                Pause(returnIndex, minimal);
            };
            Add(menu);
        }

        private void Options(int returnIndex, bool minimal)
        {
            Paused = true;
            bool oldAllowHudHide = AllowHudHide;
            AllowHudHide = false;
            TextMenu options = MenuOptions.Create(true, Level.PauseSnapshot);
            options.OnESC = options.OnCancel = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                AllowHudHide = oldAllowHudHide;
                options.CloseAndRun(SaveFromOptions(), () => Pause(returnIndex, minimal));
            };
            options.OnPause = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                options.CloseAndRun(SaveFromOptions(), () =>
                {
                    AllowHudHide = oldAllowHudHide;
                    Paused = false;
                    unpauseTimer = 0.15f;
                });
            };
            Add(options);
        }

        private IEnumerator SaveFromOptions()
        {
            UserIO.SaveHandler(false, true);
            while (UserIO.Saving)
                yield return null;
        }

        private void AssistMode(int returnIndex, bool minimal)
        {
            Paused = true;
            TextMenu menu = new();
            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_ASSIST_TITLE")));
            menu.Add(new TextMenu.Slider(Dialog.Clean("MENU_ASSIST_GAMESPEED"), i => (i * 10).ToString() + "%", 5, 10, SaveData.Instance.Assists.GameSpeed).Change(i =>
            {
                SaveData.Instance.Assists.GameSpeed = i;
                Engine.TimeRateB = SaveData.Instance.Assists.GameSpeed / 10f;
            }));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INFINITE_STAMINA"), SaveData.Instance.Assists.InfiniteStamina).Change(on => SaveData.Instance.Assists.InfiniteStamina = on));
            TextMenu textMenu = menu;
            string label = Dialog.Clean("MENU_ASSIST_AIR_DASHES");
            int dashMode = (int) SaveData.Instance.Assists.DashMode;
            TextMenu.Option<int> option1;
            TextMenu.Option<int> option2 = option1 = new TextMenu.Slider(label, i =>
            {
                if (i == 0)
                    return Dialog.Clean("MENU_ASSIST_AIR_DASHES_NORMAL");
                return i == 1 ? Dialog.Clean("MENU_ASSIST_AIR_DASHES_TWO") : Dialog.Clean("MENU_ASSIST_AIR_DASHES_INFINITE");
            }, 0, 2, dashMode).Change(on =>
            {
                SaveData.Instance.Assists.DashMode = (Assists.DashModes)on;
                Player entity = Tracker.GetEntity<Player>();
                if (entity == null)
                    return;
                entity.Dashes = Math.Min(entity.Dashes, entity.MaxDashes);
            });
            textMenu.Add(option1);
            if (Session.Area.ID == 0)
                option2.Disabled = true;
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_DASH_ASSIST"), SaveData.Instance.Assists.DashAssist).Change(on => SaveData.Instance.Assists.DashAssist = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INVINCIBLE"), SaveData.Instance.Assists.Invincible).Change(on => SaveData.Instance.Assists.Invincible = on));
            menu.OnESC = menu.OnCancel = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                Pause(returnIndex, minimal);
                menu.Close();
            };
            menu.OnPause = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                Paused = false;
                unpauseTimer = 0.15f;
                menu.Close();
            };
            Add(menu);
        }

        private void VariantMode(int returnIndex, bool minimal)
        {
            Paused = true;
            TextMenu menu = new();
            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_VARIANT_TITLE")));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_VARIANT_SUBTITLE")));
            TextMenu textMenu1 = menu;
            string label1 = Dialog.Clean("MENU_ASSIST_GAMESPEED");
            int gameSpeed = SaveData.Instance.Assists.GameSpeed;
            TextMenu.Slider speed;
            TextMenu.Slider slider = speed = new TextMenu.Slider(label1, i => (i * 10).ToString() + "%", 5, 16, gameSpeed);
            textMenu1.Add(slider);
            speed.Change(i =>
            {
                if (i > 10)
                {
                    if (speed.Values[speed.PreviousIndex].Item2 > i)
                        --i;
                    else
                        ++i;
                }
                speed.Index = i - 5;
                SaveData.Instance.Assists.GameSpeed = i;
                Engine.TimeRateB = SaveData.Instance.Assists.GameSpeed / 10f;
            });
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_MIRROR"), SaveData.Instance.Assists.MirrorMode).Change(on =>
            {
                SaveData.Instance.Assists.MirrorMode = on;
                Input.MoveX.Inverted = Input.Aim.InvertedX = Input.Feather.InvertedX = on;
            }));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_360DASHING"), SaveData.Instance.Assists.ThreeSixtyDashing).Change(on => SaveData.Instance.Assists.ThreeSixtyDashing = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_INVISMOTION"), SaveData.Instance.Assists.InvisibleMotion).Change(on => SaveData.Instance.Assists.InvisibleMotion = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_NOGRABBING"), SaveData.Instance.Assists.NoGrabbing).Change(on => SaveData.Instance.Assists.NoGrabbing = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_LOWFRICTION"), SaveData.Instance.Assists.LowFriction).Change(on => SaveData.Instance.Assists.LowFriction = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_SUPERDASHING"), SaveData.Instance.Assists.SuperDashing).Change(on => SaveData.Instance.Assists.SuperDashing = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_HICCUPS"), SaveData.Instance.Assists.Hiccups).Change(on => SaveData.Instance.Assists.Hiccups = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_PLAYASBADELINE"), SaveData.Instance.Assists.PlayAsBadeline).Change(on =>
            {
                SaveData.Instance.Assists.PlayAsBadeline = on;
                Player entity = Tracker.GetEntity<Player>();
                if (entity == null)
                    return;
                PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : entity.DefaultSpriteMode;
                if (entity.Active)
                    entity.ResetSpriteNextFrame(mode);
                else
                    entity.ResetSprite(mode);
            }));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_ASSIST_SUBTITLE")));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INFINITE_STAMINA"), SaveData.Instance.Assists.InfiniteStamina).Change(on => SaveData.Instance.Assists.InfiniteStamina = on));
            TextMenu textMenu2 = menu;
            string label2 = Dialog.Clean("MENU_ASSIST_AIR_DASHES");
            int dashMode = (int) SaveData.Instance.Assists.DashMode;
            TextMenu.Option<int> option1;
            TextMenu.Option<int> option2 = option1 = new TextMenu.Slider(label2, i =>
            {
                if (i == 0)
                    return Dialog.Clean("MENU_ASSIST_AIR_DASHES_NORMAL");
                return i == 1 ? Dialog.Clean("MENU_ASSIST_AIR_DASHES_TWO") : Dialog.Clean("MENU_ASSIST_AIR_DASHES_INFINITE");
            }, 0, 2, dashMode).Change(on =>
            {
                SaveData.Instance.Assists.DashMode = (Assists.DashModes)on;
                Player entity = Tracker.GetEntity<Player>();
                if (entity == null)
                    return;
                entity.Dashes = Math.Min(entity.Dashes, entity.MaxDashes);
            });
            textMenu2.Add(option1);
            if (Session.Area.ID == 0)
                option2.Disabled = true;
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_DASH_ASSIST"), SaveData.Instance.Assists.DashAssist).Change(on => SaveData.Instance.Assists.DashAssist = on));
            menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INVINCIBLE"), SaveData.Instance.Assists.Invincible).Change(on => SaveData.Instance.Assists.Invincible = on));
            menu.OnESC = menu.OnCancel = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                Pause(returnIndex, minimal);
                menu.Close();
            };
            menu.OnPause = () =>
            {
                Audio.Play("event:/ui/main/button_back");
                Paused = false;
                unpauseTimer = 0.15f;
                menu.Close();
            };
            Add(menu);
        }

        public void SnapColorGrade(string next)
        {
            if (!(Session.ColorGrade != next))
                return;
            lastColorGrade = next;
            colorGradeEase = 0.0f;
            colorGradeEaseSpeed = 1f;
            Session.ColorGrade = next;
        }

        public void NextColorGrade(string next, float speed = 1f)
        {
            if (!(Session.ColorGrade != next))
                return;
            colorGradeEase = 0.0f;
            colorGradeEaseSpeed = speed;
            Session.ColorGrade = next;
        }

        public void Shake(float time = 0.3f)
        {
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Off)
                return;
            shakeDirection = Vector2.Zero;
            shakeTimer = Math.Max(shakeTimer, time);
        }

        public void StopShake() => shakeTimer = 0.0f;

        public void DirectionalShake(Vector2 dir, float time = 0.3f)
        {
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Off)
                return;
            shakeDirection = dir.SafeNormalize();
            lastDirectionalShake = 0;
            shakeTimer = Math.Max(shakeTimer, time);
        }

        public void Flash(Color color, bool drawPlayerOver = false)
        {
            if (Settings.Instance.DisableFlashes)
                return;
            doFlash = true;
            flashDrawPlayer = drawPlayerOver;
            flash = 1f;
            flashColor = color;
        }

        public void ZoomSnap(Vector2 screenSpaceFocusPoint, float zoom)
        {
            ZoomFocusPoint = screenSpaceFocusPoint;
            ZoomTarget = Zoom = zoom;
        }

        public IEnumerator ZoomTo(Vector2 screenSpaceFocusPoint, float zoom, float duration)
        {
            ZoomFocusPoint = screenSpaceFocusPoint;
            ZoomTarget = zoom;
            float from = Zoom;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / duration)
            {
                Zoom = MathHelper.Lerp(from, ZoomTarget, Ease.SineInOut(MathHelper.Clamp(p, 0.0f, 1f)));
                yield return null;
            }
            Zoom = ZoomTarget;
        }

        public IEnumerator ZoomAcross(
            Vector2 screenSpaceFocusPoint,
            float zoom,
            float duration)
        {
            float fromZoom = Zoom;
            Vector2 fromFocus = ZoomFocusPoint;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / duration)
            {
                float amount = Ease.SineInOut(MathHelper.Clamp(p, 0.0f, 1f));
                Zoom = ZoomTarget = MathHelper.Lerp(fromZoom, zoom, amount);
                ZoomFocusPoint = Vector2.Lerp(fromFocus, screenSpaceFocusPoint, amount);
                yield return null;
            }
            Zoom = ZoomTarget;
            ZoomFocusPoint = screenSpaceFocusPoint;
        }

        public IEnumerator ZoomBack(float duration)
        {
            float from = Zoom;
            float to = 1f;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / duration)
            {
                Zoom = MathHelper.Lerp(from, to, Ease.SineInOut(MathHelper.Clamp(p, 0.0f, 1f)));
                yield return null;
            }
            ResetZoom();
        }

        public void ResetZoom()
        {
            Zoom = 1f;
            ZoomTarget = 1f;
            ZoomFocusPoint = new Vector2(320f, 180f) / 2f;
        }

        public void DoScreenWipe(bool wipeIn, Action onComplete = null, bool hiresSnow = false)
        {
            AreaData.Get(Session).DoScreenWipe(this, wipeIn, onComplete);
            if (!hiresSnow)
                return;
            Add(HiresSnow = new HiresSnow());
            HiresSnow.Alpha = 0.0f;
            HiresSnow.AttachAlphaTo = Wipe;
        }

        public Session.CoreModes CoreMode
        {
            get => coreMode;
            set
            {
                if (coreMode == value)
                    return;
                coreMode = value;
                Session.SetFlag("cold", coreMode == Session.CoreModes.Cold);
                Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "room_state", coreMode == Session.CoreModes.Hot ? 0.0f : 1f);
                if (Audio.CurrentMusic == "event:/music/lvl9/main")
                {
                    Session.Audio.Music.Layer(1, coreMode == Session.CoreModes.Hot);
                    Session.Audio.Music.Layer(2, coreMode == Session.CoreModes.Cold);
                    Session.Audio.Apply();
                }
                foreach (CoreModeListener component in Tracker.GetComponents<CoreModeListener>())
                {
                    if (component.OnChange != null)
                        component.OnChange(value);
                }
            }
        }

        public bool InsideCamera(Vector2 position, float expand = 0.0f) => position.X >= (double) Camera.Left - (double) expand && position.X < (double) Camera.Right + (double) expand && position.Y >= (double) Camera.Top - (double) expand && position.Y < (double) Camera.Bottom + (double) expand;

        public void EnforceBounds(Player player)
        {
            Rectangle bounds = Bounds;
            Rectangle rectangle = new((int) Camera.Left, (int) Camera.Top, 320, 180);
            if (transition != null)
                return;
            if (CameraLockMode == CameraLockModes.FinalBoss && (double) player.Left < rectangle.Left)
            {
                player.Left = rectangle.Left;
                player.OnBoundsH();
            }
            else if ((double) player.Left < bounds.Left)
            {
                if ((double) player.Top >= bounds.Top && (double) player.Bottom < bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitX * -8f))
                {
                    player.BeforeSideTransition();
                    NextLevel(player.Center + Vector2.UnitX * -8f, -Vector2.UnitX);
                    return;
                }
                player.Left = bounds.Left;
                player.OnBoundsH();
            }
            TheoCrystal entity = Tracker.GetEntity<TheoCrystal>();
            if (CameraLockMode == CameraLockModes.FinalBoss && (double) player.Right > rectangle.Right && rectangle.Right < bounds.Right - 4)
            {
                player.Right = rectangle.Right;
                player.OnBoundsH();
            }
            else if (entity != null && (player.Holding == null || !player.Holding.IsHeld) && (double) player.Right > bounds.Right - 1)
                player.Right = bounds.Right - 1;
            else if ((double) player.Right > bounds.Right)
            {
                if ((double) player.Top >= bounds.Top && (double) player.Bottom < bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitX * 8f))
                {
                    player.BeforeSideTransition();
                    NextLevel(player.Center + Vector2.UnitX * 8f, Vector2.UnitX);
                    return;
                }
                player.Right = bounds.Right;
                player.OnBoundsH();
            }
            if (CameraLockMode != CameraLockModes.None && (double) player.Top < rectangle.Top)
            {
                player.Top = rectangle.Top;
                player.OnBoundsV();
            }
            else if ((double) player.CenterY < bounds.Top)
            {
                if (Session.MapData.CanTransitionTo(this, player.Center - Vector2.UnitY * 12f))
                {
                    player.BeforeUpTransition();
                    NextLevel(player.Center - Vector2.UnitY * 12f, -Vector2.UnitY);
                    return;
                }
                if ((double) player.Top < bounds.Top - 24)
                {
                    player.Top = bounds.Top - 24;
                    player.OnBoundsV();
                }
            }
            if (CameraLockMode != CameraLockModes.None && rectangle.Bottom < bounds.Bottom - 4 && (double) player.Top > rectangle.Bottom)
            {
                if (SaveData.Instance.Assists.Invincible)
                {
                    player.Play("event:/game/general/assist_screenbottom");
                    player.Bounce(rectangle.Bottom);
                }
                else
                    player.Die(Vector2.Zero);
            }
            else if ((double) player.Bottom > bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitY * 12f) && !Session.LevelData.DisableDownTransition)
            {
                if (player.CollideCheck<Solid>(player.Position + Vector2.UnitY * 4f))
                    return;
                player.BeforeDownTransition();
                NextLevel(player.Center + Vector2.UnitY * 12f, Vector2.UnitY);
            }
            else if ((double) player.Top > bounds.Bottom && SaveData.Instance.Assists.Invincible)
            {
                player.Play("event:/game/general/assist_screenbottom");
                player.Bounce(bounds.Bottom);
            }
            else
            {
                if ((double) player.Top <= bounds.Bottom + 4)
                    return;
                player.Die(Vector2.Zero);
            }
        }

        public bool IsInBounds(Entity entity)
        {
            Rectangle bounds = Bounds;
            return (double) entity.Right > bounds.Left && (double) entity.Bottom > bounds.Top && (double) entity.Left < bounds.Right && (double) entity.Top < bounds.Bottom;
        }

        public bool IsInBounds(Vector2 position)
        {
            Rectangle bounds = Bounds;
            return position.X >= (double) bounds.Left && position.Y >= (double) bounds.Top && position.X < (double) bounds.Right && position.Y < (double) bounds.Bottom;
        }

        public bool IsInBounds(Vector2 position, float pad)
        {
            Rectangle bounds = Bounds;
            return position.X >= bounds.Left - (double) pad && position.Y >= bounds.Top - (double) pad && position.X < bounds.Right + (double) pad && position.Y < bounds.Bottom + (double) pad;
        }

        public bool IsInBounds(Vector2 position, Vector2 dirPad)
        {
            float num1 = Math.Max(dirPad.X, 0.0f);
            float num2 = Math.Max(-dirPad.X, 0.0f);
            float num3 = Math.Max(dirPad.Y, 0.0f);
            float num4 = Math.Max(-dirPad.Y, 0.0f);
            Rectangle bounds = Bounds;
            return position.X >= bounds.Left + (double) num1 && position.Y >= bounds.Top + (double) num3 && position.X < bounds.Right - (double) num2 && position.Y < bounds.Bottom - (double) num4;
        }

        public bool IsInCamera(Vector2 position, float pad)
        {
            Rectangle rectangle = new((int) Camera.X, (int) Camera.Y, 320, 180);
            return position.X >= rectangle.Left - (double) pad && position.Y >= rectangle.Top - (double) pad && position.X < rectangle.Right + (double) pad && position.Y < rectangle.Bottom + (double) pad;
        }

        public void StartCutscene(
            Action<Level> onSkip,
            bool fadeInOnSkip = true,
            bool endingChapterAfterCutscene = false,
            bool resetZoomOnSkip = true)
        {
            this.endingChapterAfterCutscene = endingChapterAfterCutscene;
            InCutscene = true;
            onCutsceneSkip = onSkip;
            onCutsceneSkipFadeIn = fadeInOnSkip;
            onCutsceneSkipResetZoom = resetZoomOnSkip;
        }

        public void CancelCutscene()
        {
            InCutscene = false;
            SkippingCutscene = false;
        }

        public void SkipCutscene()
        {
            SkippingCutscene = true;
            Engine.TimeRate = 1f;
            Distort.Anxiety = 0.0f;
            Distort.GameRate = 1f;
            if (endingChapterAfterCutscene)
                Audio.BusStopAll("bus:/gameplay_sfx", true);
            List<Entity> entityList = new();
            foreach (Entity entity in Tracker.GetEntities<Textbox>())
                entityList.Add(entity);
            foreach (Entity entity in entityList)
                entity.RemoveSelf();
            skipCoroutine = new Coroutine(SkipCutsceneRoutine());
        }

        private IEnumerator SkipCutsceneRoutine()
        {
            Level level = this;
            FadeWipe fadeWipe1 = new(level, false);
            fadeWipe1.Duration = 0.25f;
            yield return fadeWipe1.Wait();
            level.onCutsceneSkip(level);
            level.strawberriesDisplay.DrawLerp = 0.0f;
            if (level.onCutsceneSkipResetZoom)
                level.ResetZoom();
            GameplayStats first = level.Entities.FindFirst<GameplayStats>();
            if (first != null)
                first.DrawLerp = 0.0f;
            if (level.onCutsceneSkipFadeIn)
            {
                FadeWipe fadeWipe2 = new(level, true);
                fadeWipe2.Duration = 0.25f;
                level.RendererList.UpdateLists();
                yield return fadeWipe2.Wait();
            }
            level.SkippingCutscene = false;
            level.EndCutscene();
        }

        public void EndCutscene()
        {
            if (SkippingCutscene)
                return;
            InCutscene = false;
        }

        private void NextLevel(Vector2 at, Vector2 dir) => OnEndOfFrame += () =>
        {
            Engine.TimeRate = 1f;
            Distort.Anxiety = 0.0f;
            Distort.GameRate = 1f;
            TransitionTo(Session.MapData.GetAt(at), dir);
        };

        public void RegisterAreaComplete()
        {
            if (Completed)
                return;
            Player entity = Tracker.GetEntity<Player>();
            if (entity != null)
            {
                List<Strawberry> strawberryList = new();
                foreach (Follower follower in entity.Leader.Followers)
                {
                    if (follower.Entity is Strawberry)
                        strawberryList.Add(follower.Entity as Strawberry);
                }
                foreach (Strawberry strawberry in strawberryList)
                    strawberry.OnCollect();
            }
            Completed = true;
            SaveData.Instance.RegisterCompletion(Session);
        }

        public ScreenWipe CompleteArea(
            bool spotlightWipe = true,
            bool skipScreenWipe = false,
            bool skipCompleteScreen = false)
        {
            RegisterAreaComplete();
            PauseLock = true;
            Action onComplete = !(AreaData.Get(Session).Interlude | skipCompleteScreen) ? (() => Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session)) : (() => Engine.Scene = new LevelExit(LevelExit.Mode.CompletedInterlude, Session, HiresSnow));
            if (!SkippingCutscene && !skipScreenWipe)
            {
                if (!spotlightWipe)
                    return new FadeWipe(this, false, onComplete);
                Player entity = Tracker.GetEntity<Player>();
                if (entity != null)
                    SpotlightWipe.FocusPoint = entity.Position - Camera.Position - new Vector2(0.0f, 8f);
                return new SpotlightWipe(this, false, onComplete);
            }
            Audio.BusStopAll("bus:/gameplay_sfx", true);
            onComplete();
            return null;
        }

        public enum CameraLockModes
        {
            None,
            BoostSequence,
            FinalBoss,
            FinalBossNoY,
            Lava,
        }

        private enum ConditionBlockModes
        {
            Key,
            Button,
            Strawberry,
        }
    }
}
