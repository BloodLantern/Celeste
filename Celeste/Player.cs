using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
    [Tracked]
    public class Player : Actor
    {
        public static ParticleType P_DashA;
        public static ParticleType P_DashB;
        public static ParticleType P_DashBadB;
        public static ParticleType P_CassetteFly;
        public static ParticleType P_Split;
        public static ParticleType P_SummitLandA;
        public static ParticleType P_SummitLandB;
        public static ParticleType P_SummitLandC;
        public const float MaxFall = 160f;
        private const float Gravity = 900f;
        private const float HalfGravThreshold = 40f;
        private const float FastMaxFall = 240f;
        private const float FastMaxAccel = 300f;
        public const float MaxRun = 90f;
        public const float RunAccel = 1000f;
        private const float RunReduce = 400f;
        private const float AirMult = 0.65f;
        private const float HoldingMaxRun = 70f;
        private const float HoldMinTime = 0.35f;
        private const float BounceAutoJumpTime = 0.1f;
        private const float DuckFriction = 500f;
        private const int DuckCorrectCheck = 4;
        private const float DuckCorrectSlide = 50f;
        private const float DodgeSlideSpeedMult = 1.2f;
        private const float DuckSuperJumpXMult = 1.25f;
        private const float DuckSuperJumpYMult = 0.5f;
        private const float JumpGraceTime = 0.1f;
        private const float JumpSpeed = -105f;
        private const float JumpHBoost = 40f;
        private const float VarJumpTime = 0.2f;
        private const float CeilingVarJumpGrace = 0.05f;
        private const int UpwardCornerCorrection = 4;
        private const int DashingUpwardCornerCorrection = 5;
        private const float WallSpeedRetentionTime = 0.06f;
        private const int WallJumpCheckDist = 3;
        private const int SuperWallJumpCheckDist = 5;
        private const float WallJumpForceTime = 0.16f;
        private const float WallJumpHSpeed = 130f;
        public const float WallSlideStartMax = 20f;
        private const float WallSlideTime = 1.2f;
        private const float BounceVarJumpTime = 0.2f;
        private const float BounceSpeed = -140f;
        private const float SuperBounceVarJumpTime = 0.2f;
        private const float SuperBounceSpeed = -185f;
        private const float SuperJumpSpeed = -105f;
        private const float SuperJumpH = 260f;
        private const float SuperWallJumpSpeed = -160f;
        private const float SuperWallJumpVarTime = 0.25f;
        private const float SuperWallJumpForceTime = 0.2f;
        private const float SuperWallJumpH = 170f;
        private const float DashSpeed = 240f;
        private const float EndDashSpeed = 160f;
        private const float EndDashUpMult = 0.75f;
        private const float DashTime = 0.15f;
        private const float SuperDashTime = 0.3f;
        private const float DashCooldown = 0.2f;
        private const float DashRefillCooldown = 0.1f;
        private const int DashHJumpThruNudge = 6;
        private const int DashCornerCorrection = 4;
        private const int DashVFloorSnapDist = 3;
        private const float DashAttackTime = 0.3f;
        private const float BoostMoveSpeed = 80f;
        public const float BoostTime = 0.25f;
        private const float DuckWindMult = 0.0f;
        private const int WindWallDistance = 3;
        private const float ReboundSpeedX = 120f;
        private const float ReboundSpeedY = -120f;
        private const float ReboundVarJumpTime = 0.15f;
        private const float ReflectBoundSpeed = 220f;
        private const float DreamDashSpeed = 240f;
        private const int DreamDashEndWiggle = 5;
        private const float DreamDashMinTime = 0.1f;
        public const float ClimbMaxStamina = 110f;
        private const float ClimbUpCost = 45.4545441f;
        private const float ClimbStillCost = 10f;
        private const float ClimbJumpCost = 27.5f;
        private const int ClimbCheckDist = 2;
        private const int ClimbUpCheckDist = 2;
        private const float ClimbNoMoveTime = 0.1f;
        public const float ClimbTiredThreshold = 20f;
        private const float ClimbUpSpeed = -45f;
        private const float ClimbDownSpeed = 80f;
        private const float ClimbSlipSpeed = 30f;
        private const float ClimbAccel = 900f;
        private const float ClimbGrabYMult = 0.2f;
        private const float ClimbHopY = -120f;
        private const float ClimbHopX = 100f;
        private const float ClimbHopForceTime = 0.2f;
        private const float ClimbJumpBoostTime = 0.2f;
        private const float ClimbHopNoWindTime = 0.3f;
        private const float LaunchSpeed = 280f;
        private const float LaunchCancelThreshold = 220f;
        private const float LiftYCap = -130f;
        private const float LiftXCap = 250f;
        private const float JumpThruAssistSpeed = -40f;
        private const float FlyPowerFlashTime = 0.5f;
        private const float ThrowRecoil = 80f;
        private static readonly Vector2 CarryOffsetTarget = new(0.0f, -12f);
        private const float ChaserStateMaxTime = 4f;
        public const float WalkSpeed = 64f;
        private const float LowFrictionMult = 0.35f;
        private const float LowFrictionAirMult = 0.5f;
        private const float LowFrictionStopTime = 0.15f;
        private const float HiccupTimeMin = 1.2f;
        private const float HiccupTimeMax = 1.8f;
        private const float HiccupDuckMult = 0.5f;
        private const float HiccupAirBoost = -60f;
        private const float HiccupAirVarTime = 0.15f;
        private const float GliderMaxFall = 40f;
        private const float GliderWindMaxFall = 0.0f;
        private const float GliderWindUpFall = -32f;
        public const float GliderFastFall = 120f;
        private const float GliderSlowFall = 24f;
        private const float GliderGravMult = 0.5f;
        private const float GliderMaxRun = 108.000008f;
        private const float GliderRunMult = 0.5f;
        private const float GliderUpMinPickupSpeed = -105f;
        private const float GliderDashMinPickupSpeed = -240f;
        private const float GliderWallJumpForceTime = 0.26f;
        private const float DashGliderBoostTime = 0.55f;
        public const int StNormal = 0;
        public const int StClimb = 1;
        public const int StDash = 2;
        public const int StSwim = 3;
        public const int StBoost = 4;
        public const int StRedDash = 5;
        public const int StHitSquash = 6;
        public const int StLaunch = 7;
        public const int StPickup = 8;
        public const int StDreamDash = 9;
        public const int StSummitLaunch = 10;
        public const int StDummy = 11;
        public const int StIntroWalk = 12;
        public const int StIntroJump = 13;
        public const int StIntroRespawn = 14;
        public const int StIntroWakeUp = 15;
        public const int StBirdDashTutorial = 16;
        public const int StFrozen = 17;
        public const int StReflectionFall = 18;
        public const int StStarFly = 19;
        public const int StTempleFall = 20;
        public const int StCassetteFly = 21;
        public const int StAttract = 22;
        public const int StIntroMoonJump = 23;
        public const int StFlingBird = 24;
        public const int StIntroThinkForABit = 25;
        public const string TalkSfx = "player_talk";
        public Vector2 Speed;
        public Facings Facing;
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        public StateMachine StateMachine;
        public Vector2 CameraAnchor;
        public bool CameraAnchorIgnoreX;
        public bool CameraAnchorIgnoreY;
        public Vector2 CameraAnchorLerp;
        public bool ForceCameraUpdate;
        public Leader Leader;
        public VertexLight Light;
        public int Dashes;
        public float Stamina = 110f;
        public bool StrawberriesBlocked;
        public Vector2 PreviousPosition;
        public bool DummyAutoAnimate = true;
        public Vector2 ForceStrongWindHair;
        public Vector2? OverrideDashDirection;
        public bool FlipInReflection;
        public bool JustRespawned;
        public bool EnforceLevelBounds = true;
        private Level level;
        private readonly Collision onCollideH;
        private readonly Collision onCollideV;
        private bool onGround;
        private bool wasOnGround;
        private int moveX;
        private bool flash;
        private bool wasDucking;
        private int climbTriggerDir;
        private bool holdCannotDuck;
        private bool windMovedUp;
        private float idleTimer;
        private static readonly Chooser<string> idleColdOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f).Add("idleC", 1f);
        private static readonly Chooser<string> idleNoBackpackOptions = new Chooser<string>().Add("idleA", 1f).Add("idleB", 3f).Add("idleC", 3f);
        private static readonly Chooser<string> idleWarmOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f);
        public int StrawberryCollectIndex;
        public float StrawberryCollectResetTimer;
        private Hitbox hurtbox;
        private float jumpGraceTimer;
        public bool AutoJump;
        public float AutoJumpTimer;
        private float varJumpSpeed;
        private float varJumpTimer;
        private int forceMoveX;
        private float forceMoveXTimer;
        private int hopWaitX;
        private float hopWaitXSpeed;
        private Vector2 lastAim;
        private float dashCooldownTimer;
        private float dashRefillCooldownTimer;
        public Vector2 DashDir;
        private float wallSlideTimer = 1.2f;
        private int wallSlideDir;
        private float climbNoMoveTimer;
        private Vector2 carryOffset;
        private Vector2 deadOffset;
        private float introEase;
        private float wallSpeedRetentionTimer;
        private float wallSpeedRetained;
        private int wallBoostDir;
        private float wallBoostTimer;
        private float maxFall;
        private float dashAttackTimer;
        private float gliderBoostTimer;
        public List<ChaserState> ChaserStates;
        private bool wasTired;
        private readonly HashSet<Trigger> triggersInside;
        private float highestAirY;
        private bool dashStartedOnGround;
        private bool fastJump;
        private int lastClimbMove;
        private float noWindTimer;
        private float dreamDashCanEndTimer;
        private Solid climbHopSolid;
        private Vector2 climbHopSolidPosition;
        private readonly SoundSource wallSlideSfx;
        private readonly SoundSource swimSurfaceLoopSfx;
        private float playFootstepOnLand;
        private float minHoldTimer;
        public Booster CurrentBooster;
        public Booster LastBooster;
        private bool calledDashEvents;
        private int lastDashes;
        private readonly Sprite sweatSprite;
        private readonly int startHairCount;
        private bool launched;
        private float launchedTimer;
        private float dashTrailTimer;
        private int dashTrailCounter;
        private bool canCurveDash;
        private float lowFrictionStopTimer;
        private float hiccupTimer;
        private readonly List<ChaserStateSound> activeSounds = new();
        private EventInstance idleSfx;
        public bool MuffleLanding;
        private Vector2 gliderBoostDir;
        private float explodeLaunchBoostTimer;
        private float explodeLaunchBoostSpeed;
        private bool demoDashed;
        private readonly Hitbox normalHitbox = new(8f, 11f, -4f, -11f);
        private readonly Hitbox duckHitbox = new(8f, 6f, -4f, -6f);
        private readonly Hitbox normalHurtbox = new(8f, 9f, -4f, -11f);
        private readonly Hitbox duckHurtbox = new(8f, 4f, -4f, -6f);
        private readonly Hitbox starFlyHitbox = new(8f, 8f, -4f, -10f);
        private readonly Hitbox starFlyHurtbox = new(6f, 6f, -3f, -9f);
        private Vector2 normalLightOffset = new(0.0f, -8f);
        private Vector2 duckingLightOffset = new(0.0f, -3f);
        private readonly List<Entity> temp = new();
        public static readonly Color NormalHairColor = Calc.HexToColor("AC3232");
        public static readonly Color FlyPowerHairColor = Calc.HexToColor("F2EB6D");
        public static readonly Color UsedHairColor = Calc.HexToColor("44B7FF");
        public static readonly Color FlashHairColor = Color.White;
        public static readonly Color TwoDashesHairColor = Calc.HexToColor("ff6def");
        public static readonly Color NormalBadelineHairColor = BadelineOldsite.HairColor;
        public static readonly Color UsedBadelineHairColor = UsedHairColor;
        public static readonly Color TwoDashesBadelineHairColor = TwoDashesHairColor;
        private float hairFlashTimer;
        private bool startHairCalled;
        public Color? OverrideHairColor;
        private Vector2 windDirection;
        private float windTimeout;
        private float windHairTimer;
        public IntroTypes IntroType;
        private readonly MirrorReflection reflection;
        public PlayerSpriteMode DefaultSpriteMode;
        private PlayerSpriteMode? nextSpriteMode;
        private const float LaunchedBoostCheckSpeedSq = 10000f;
        private const float LaunchedJumpCheckSpeedSq = 48400f;
        private const float LaunchedMinSpeedSq = 19600f;
        private const float LaunchedDoubleSpeedSq = 22500f;
        private const float SideBounceSpeed = 240f;
        private const float SideBounceThreshold = 240f;
        private const float SideBounceForceMoveXTime = 0.3f;
        private const float SpacePhysicsMult = 0.6f;
        private EventInstance conveyorLoopSfx;
        private const float WallBoosterSpeed = -160f;
        private const float WallBoosterLiftSpeed = -80f;
        private const float WallBoosterAccel = 600f;
        private const float WallBoostingHopHSpeed = 100f;
        private const float WallBoosterOverTopSpeed = -180f;
        private const float IceBoosterSpeed = 40f;
        private const float IceBoosterAccel = 300f;
        private bool wallBoosting;
        private Vector2 beforeDashSpeed;
        private bool wasDashB;
        private const float SwimYSpeedMult = 0.5f;
        private const float SwimMaxRise = -60f;
        private const float SwimVDeccel = 600f;
        private const float SwimMax = 80f;
        private const float SwimUnderwaterMax = 60f;
        private const float SwimAccel = 600f;
        private const float SwimReduce = 400f;
        private const float SwimDashSpeedMult = 0.75f;
        private Vector2 boostTarget;
        private bool boostRed;
        private const float HitSquashNoMoveTime = 0.1f;
        private const float HitSquashFriction = 800f;
        private float hitSquashNoMoveTimer;
        private float? launchApproachX;
        private float summitLaunchTargetX;
        private float summitLaunchParticleTimer;
        private DreamBlock dreamBlock;
        private SoundSource dreamSfxLoop;
        private bool dreamJump;
        private const float StarFlyTransformDeccel = 1000f;
        private const float StarFlyTime = 2f;
        private const float StarFlyStartSpeed = 250f;
        private const float StarFlyTargetSpeed = 140f;
        private const float StarFlyMaxSpeed = 190f;
        private const float StarFlyMaxLerpTime = 1f;
        private const float StarFlySlowSpeed = 91f;
        private const float StarFlyAccel = 1000f;
        private const float StarFlyRotateSpeed = 5.58505344f;
        private const float StarFlyEndX = 160f;
        private const float StarFlyEndXVarJumpTime = 0.1f;
        private const float StarFlyEndFlashDuration = 0.5f;
        private const float StarFlyEndNoBounceTime = 0.2f;
        private const float StarFlyWallBounce = -0.5f;
        private const float StarFlyMaxExitY = 0.0f;
        private const float StarFlyMaxExitX = 140f;
        private const float StarFlyExitUp = -100f;
        private Color starFlyColor = Calc.HexToColor("ffd65c");
        private BloomPoint starFlyBloom;
        private float starFlyTimer;
        private bool starFlyTransforming;
        private float starFlySpeedLerp;
        private Vector2 starFlyLastDir;
        private SoundSource starFlyLoopSfx;
        private SoundSource starFlyWarningSfx;
        private FlingBird flingBird;
        private SimpleCurve cassetteFlyCurve;
        private float cassetteFlyLerp;
        private Vector2 attractTo;
        public bool DummyMoving;
        public bool DummyGravity = true;
        public bool DummyFriction = true;
        public bool DummyMaxspeed = true;
        private Facings IntroWalkDirection;
        private Tween respawnTween;

        public bool Dead { get; private set; }

        public Player(Vector2 position, PlayerSpriteMode spriteMode)
            : base(new Vector2((int)position.X, (int)position.Y))
        {
            Input.ResetGrab();
            DefaultSpriteMode = spriteMode;
            Depth = 0;
            Tag = (int) Tags.Persistent;
            if (SaveData.Instance != null && SaveData.Instance.Assists.PlayAsBadeline)
                spriteMode = PlayerSpriteMode.MadelineAsBadeline;
            Sprite = new PlayerSprite(spriteMode);
            Add(Hair = new PlayerHair(Sprite));
            Add(Sprite);
            Hair.Color = spriteMode != PlayerSpriteMode.MadelineAsBadeline ? Player.NormalHairColor : Player.NormalBadelineHairColor;
            startHairCount = Sprite.HairCount;
            sweatSprite = GFX.SpriteBank.Create("player_sweat");
            Add(sweatSprite);
            Collider = normalHitbox;
            hurtbox = normalHurtbox;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            StateMachine = new StateMachine(26);
            StateMachine.SetCallbacks(Player.StNormal, NormalUpdate, begin: NormalBegin, end: NormalEnd);
            StateMachine.SetCallbacks(Player.StClimb, ClimbUpdate, begin: ClimbBegin, end: ClimbEnd);
            StateMachine.SetCallbacks(Player.StDash, DashUpdate, DashCoroutine, DashBegin, DashEnd);
            StateMachine.SetCallbacks(Player.StSwim, SwimUpdate, begin: SwimBegin);
            StateMachine.SetCallbacks(Player.StBoost, BoostUpdate, BoostCoroutine, BoostBegin, BoostEnd);
            StateMachine.SetCallbacks(Player.StRedDash, RedDashUpdate, RedDashCoroutine, RedDashBegin, RedDashEnd);
            StateMachine.SetCallbacks(Player.StHitSquash, HitSquashUpdate, begin: HitSquashBegin);
            StateMachine.SetCallbacks(Player.StLaunch, LaunchUpdate, begin: LaunchBegin);
            StateMachine.SetCallbacks(Player.StPickup, null, PickupCoroutine);
            StateMachine.SetCallbacks(Player.StDreamDash, DreamDashUpdate, begin: DreamDashBegin, end: DreamDashEnd);
            StateMachine.SetCallbacks(Player.StSummitLaunch, SummitLaunchUpdate, begin: SummitLaunchBegin);
            StateMachine.SetCallbacks(Player.StDummy, DummyUpdate, begin: DummyBegin);
            StateMachine.SetCallbacks(Player.StIntroWalk, null, IntroWalkCoroutine);
            StateMachine.SetCallbacks(Player.StIntroJump, null, IntroJumpCoroutine);
            StateMachine.SetCallbacks(Player.StIntroRespawn, null, begin: IntroRespawnBegin, end: IntroRespawnEnd);
            StateMachine.SetCallbacks(Player.StIntroWakeUp, null, IntroWakeUpCoroutine);
            StateMachine.SetCallbacks(Player.StTempleFall, TempleFallUpdate, TempleFallCoroutine);
            StateMachine.SetCallbacks(Player.StReflectionFall, ReflectionFallUpdate, ReflectionFallCoroutine, ReflectionFallBegin, ReflectionFallEnd);
            StateMachine.SetCallbacks(Player.StBirdDashTutorial, BirdDashTutorialUpdate, BirdDashTutorialCoroutine, BirdDashTutorialBegin);
            StateMachine.SetCallbacks(Player.StFrozen, FrozenUpdate);
            StateMachine.SetCallbacks(Player.StStarFly, StarFlyUpdate, StarFlyCoroutine, StarFlyBegin, StarFlyEnd);
            StateMachine.SetCallbacks(Player.StCassetteFly, CassetteFlyUpdate, CassetteFlyCoroutine, CassetteFlyBegin, CassetteFlyEnd);
            StateMachine.SetCallbacks(Player.StAttract, AttractUpdate, begin: AttractBegin, end: AttractEnd);
            StateMachine.SetCallbacks(Player.StIntroMoonJump, null, IntroMoonJumpCoroutine);
            StateMachine.SetCallbacks(Player.StFlingBird, FlingBirdUpdate, FlingBirdCoroutine, FlingBirdBegin, FlingBirdEnd);
            StateMachine.SetCallbacks(Player.StIntroThinkForABit, null, IntroThinkForABitCoroutine);
            Add(StateMachine);
            Add(Leader = new Leader(new Vector2(0.0f, -8f)));
            lastAim = Vector2.UnitX;
            Facing = Facings.Right;
            ChaserStates = new List<ChaserState>();
            triggersInside = new HashSet<Trigger>();
            Add(Light = new VertexLight(normalLightOffset, Color.White, 1f, 32, 64));
            Add(new WaterInteraction(() => StateMachine.State is 2 or 18));
            Add(new WindMover(WindMove));
            Add(wallSlideSfx = new SoundSource());
            Add(swimSurfaceLoopSfx = new SoundSource());
            Sprite.OnFrameChange = anim =>
            {
                if (Scene == null || Dead || !Sprite.Visible)
                    return;
                int currentAnimationFrame = Sprite.CurrentAnimationFrame;
                if (anim.Equals("runSlow_carry") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("runFast") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("runSlow") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("walk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("runStumble") && currentAnimationFrame == 6 || anim.Equals("flip") && currentAnimationFrame == 4 || anim.Equals("runWind") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("idleC") && Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack && (currentAnimationFrame == 3 || currentAnimationFrame == 6 || currentAnimationFrame == 8 || currentAnimationFrame == 11) || anim.Equals("carryTheoWalk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6) || anim.Equals("push") && (currentAnimationFrame == 8 || currentAnimationFrame == 15))
                {
                    Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                    if (platformByPriority != null)
                        Play("event:/char/madeline/footstep", "surface_index", platformByPriority.GetStepSoundIndex(this));
                }
                else if (anim.Equals("climbUp") && currentAnimationFrame == 5 || anim.Equals("climbDown") && currentAnimationFrame == 5)
                {
                    Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.UnitX * (float)Facing, temp));
                    if (platformByPriority != null)
                        Play("event:/char/madeline/handhold", "surface_index", platformByPriority.GetWallSoundIndex(this, (int)Facing));
                }
                else if (anim.Equals("wakeUp") && currentAnimationFrame == 19)
                    Play("event:/char/madeline/campfire_stand");
                else if (anim.Equals("sitDown") && currentAnimationFrame == 12)
                    Play("event:/char/madeline/summit_sit");
                if (!anim.Equals("push") || currentAnimationFrame != 8 && currentAnimationFrame != 15)
                    return;
                Dust.BurstFG(Position + new Vector2(-(int)Facing * 5, -1f), new Vector2(-(int)Facing, -0.5f).Angle(), range: 0.0f);
            };
            Sprite.OnLastFrame = anim =>
            {
                if (Scene == null || Dead || !(Sprite.CurrentAnimationID == "idle") || level.InCutscene || idleTimer <= 3.0 || !Calc.Random.Chance(0.2f))
                    return;
                string id = Sprite.Mode != PlayerSpriteMode.Madeline ? Player.idleNoBackpackOptions.Choose() : (level.CoreMode == Session.CoreModes.Hot ? Player.idleWarmOptions : Player.idleColdOptions).Choose();
                if (string.IsNullOrEmpty(id) || !Sprite.Has(id))
                    return;
                Sprite.Play(id);
                if (Sprite.Mode == PlayerSpriteMode.Madeline)
                {
                    if (id == "idleB")
                    {
                        idleSfx = Play("event:/char/madeline/idle_scratch");
                    }
                    else
                    {
                        if (!(id == "idleC"))
                            return;
                        idleSfx = Play("event:/char/madeline/idle_sneeze");
                    }
                }
                else
                {
                    if (!(id == "idleA"))
                        return;
                    idleSfx = Play("event:/char/madeline/idle_crackknuckles");
                }
            };
            Sprite.OnChange = (last, next) =>
            {
                if (!(last == "idleB") && !(last == "idleC") || next == null || next.StartsWith("idle") || !(idleSfx != null))
                    return;
                Audio.Stop(idleSfx);
            };
            Add(reflection = new MirrorReflection());
        }

        public void ResetSpriteNextFrame(PlayerSpriteMode mode) => nextSpriteMode = mode;

        public void ResetSprite(PlayerSpriteMode mode)
        {
            string currentAnimationId = Sprite.CurrentAnimationID;
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            Sprite.RemoveSelf();
            Add(Sprite = new PlayerSprite(mode));
            if (Sprite.Has(currentAnimationId))
            {
                Sprite.Play(currentAnimationId);
                if (currentAnimationFrame < Sprite.CurrentAnimationTotalFrames)
                    Sprite.SetAnimationFrame(currentAnimationFrame);
            }
            Hair.Sprite = Sprite;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            lastDashes = Dashes = MaxDashes;
            SpawnFacingTrigger spawnFacingTrigger = CollideFirst<SpawnFacingTrigger>();
            if (spawnFacingTrigger != null)
                Facing = spawnFacingTrigger.Facing;
            else if ((double) X > level.Bounds.Center.X && IntroType != IntroTypes.None)
                Facing = Facings.Left;
            switch (IntroType)
            {
                case IntroTypes.Respawn:
                    StateMachine.State = 14;
                    JustRespawned = true;
                    break;
                case IntroTypes.WalkInRight:
                    IntroWalkDirection = Facings.Right;
                    StateMachine.State = 12;
                    break;
                case IntroTypes.WalkInLeft:
                    IntroWalkDirection = Facings.Left;
                    StateMachine.State = 12;
                    break;
                case IntroTypes.Jump:
                    StateMachine.State = 13;
                    break;
                case IntroTypes.WakeUp:
                    Sprite.Play("asleep");
                    Facing = Facings.Right;
                    StateMachine.State = 15;
                    break;
                case IntroTypes.Fall:
                    StateMachine.State = 18;
                    break;
                case IntroTypes.TempleMirrorVoid:
                    StartTempleMirrorVoidSleep();
                    break;
                case IntroTypes.None:
                    StateMachine.State = 0;
                    break;
                case IntroTypes.ThinkForABit:
                    StateMachine.State = 25;
                    break;
            }
            IntroType = IntroTypes.Transition;
            StartHair();
            PreviousPosition = Position;
        }

        public void StartTempleMirrorVoidSleep()
        {
            Sprite.Play("asleep");
            Facing = Facings.Right;
            StateMachine.State = 11;
            StateMachine.Locked = true;
            DummyAutoAnimate = false;
            DummyGravity = false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            level = null;
            Audio.Stop(conveyorLoopSfx);
            foreach (Trigger trigger in triggersInside)
            {
                trigger.Triggered = false;
                trigger.OnLeave(this);
            }
            triggersInside.Clear();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(conveyorLoopSfx);
        }

        public override void Render()
        {
            if (SaveData.Instance.Assists.InvisibleMotion && InControl && (!onGround && StateMachine.State != 1 && StateMachine.State != 3 || Speed.LengthSquared() > 800.0))
                return;
            Vector2 renderPosition = Sprite.RenderPosition;
            Sprite.RenderPosition = Sprite.RenderPosition.Floor();
            if (StateMachine.State == 14)
            {
                DeathEffect.Draw(Center + deadOffset, Hair.Color, introEase);
            }
            else
            {
                if (StateMachine.State != 19)
                {
                    if (IsTired && flash)
                        Sprite.Color = Color.Red;
                    else
                        Sprite.Color = Color.White;
                }
                if (reflection.IsRendering && FlipInReflection)
                {
                    Facing = (Facings) (-(int) Facing);
                    Hair.Facing = Facing;
                }
                Sprite.Scale.X *= (float) Facing;
                if (sweatSprite.LastAnimationID == "idle")
                {
                    sweatSprite.Scale = Sprite.Scale;
                }
                else
                {
                    sweatSprite.Scale.Y = Sprite.Scale.Y;
                    sweatSprite.Scale.X = Math.Abs(Sprite.Scale.X) * Math.Sign(sweatSprite.Scale.X);
                }
                base.Render();
                if (Sprite.CurrentAnimationID == "startStarFly")
                {
                    float num = Sprite.CurrentAnimationFrame / (float) Sprite.CurrentAnimationTotalFrames;
                    GFX.Game.GetAtlasSubtexturesAt("characters/player/startStarFlyWhite", Sprite.CurrentAnimationFrame).Draw(Sprite.RenderPosition, Sprite.Origin, starFlyColor * num, Sprite.Scale, Sprite.Rotation, SpriteEffects.None);
                }
                Sprite.Scale.X *= (float) Facing;
                if (reflection.IsRendering && FlipInReflection)
                {
                    Facing = (Facings) (-(int) Facing);
                    Hair.Facing = Facing;
                }
            }
            Sprite.RenderPosition = renderPosition;
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider collider = Collider;
            Collider = hurtbox;
            Draw.HollowRect(Collider, Color.Lime);
            Collider = collider;
        }

        public override void Update()
        {
            if (SaveData.Instance.Assists.InfiniteStamina)
                Stamina = 110f;
            PreviousPosition = Position;
            if (nextSpriteMode.HasValue)
            {
                ResetSprite(nextSpriteMode.Value);
                nextSpriteMode = new PlayerSpriteMode?();
            }
            climbTriggerDir = 0;
            if (SaveData.Instance.Assists.Hiccups)
            {
                if (hiccupTimer <= 0.0)
                    hiccupTimer = level.HiccupRandom.Range(1.2f, 1.8f);
                if (Ducking)
                    hiccupTimer -= Engine.DeltaTime * 0.5f;
                else
                    hiccupTimer -= Engine.DeltaTime;
                if (hiccupTimer <= 0.0)
                    HiccupJump();
            }
            if (gliderBoostTimer > 0.0)
                gliderBoostTimer -= Engine.DeltaTime;
            if (lowFrictionStopTimer > 0.0)
                lowFrictionStopTimer -= Engine.DeltaTime;
            if (explodeLaunchBoostTimer > 0.0)
            {
                if (Input.MoveX.Value == Math.Sign(explodeLaunchBoostSpeed))
                {
                    Speed.X = explodeLaunchBoostSpeed;
                    explodeLaunchBoostTimer = 0.0f;
                }
                else
                    explodeLaunchBoostTimer -= Engine.DeltaTime;
            }
            StrawberryCollectResetTimer -= Engine.DeltaTime;
            if (StrawberryCollectResetTimer <= 0.0)
                StrawberryCollectIndex = 0;
            idleTimer += Engine.DeltaTime;
            if (level != null && level.InCutscene)
                idleTimer = -5f;
            else if (Speed.X != 0.0 || Speed.Y != 0.0)
                idleTimer = 0.0f;
            if (!Dead)
                Audio.MusicUnderwater = UnderwaterMusicCheck();
            if (JustRespawned && Speed != Vector2.Zero)
                JustRespawned = false;
            if (StateMachine.State == 9)
                onGround = OnSafeGround = false;
            else if (Speed.Y >= 0.0)
            {
                Platform platform = (Platform) CollideFirst<Solid>(Position + Vector2.UnitY) ?? CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);
                if (platform != null)
                {
                    onGround = true;
                    OnSafeGround = platform.Safe;
                }
                else
                    onGround = OnSafeGround = false;
            }
            else
                onGround = OnSafeGround = false;
            if (StateMachine.State == 3)
                OnSafeGround = true;
            if (OnSafeGround)
            {
                foreach (SafeGroundBlocker component in Scene.Tracker.GetComponents<SafeGroundBlocker>())
                {
                    if (component.Check(this))
                    {
                        OnSafeGround = false;
                        break;
                    }
                }
            }
            playFootstepOnLand -= Engine.DeltaTime;
            highestAirY = !onGround ? Math.Min(Y, highestAirY) : Y;
            if (Scene.OnInterval(0.05f))
                flash = !flash;
            if (wallSlideDir != 0)
            {
                wallSlideTimer = Math.Max(wallSlideTimer - Engine.DeltaTime, 0.0f);
                wallSlideDir = 0;
            }
            if (wallBoostTimer > 0.0)
            {
                wallBoostTimer -= Engine.DeltaTime;
                if (moveX == wallBoostDir)
                {
                    Speed.X = 130f * moveX;
                    Stamina += 27.5f;
                    wallBoostTimer = 0.0f;
                    sweatSprite.Play("idle");
                }
            }
            if (onGround && StateMachine.State != 1)
            {
                AutoJump = false;
                Stamina = 110f;
                wallSlideTimer = 1.2f;
            }
            if (dashAttackTimer > 0.0)
                dashAttackTimer -= Engine.DeltaTime;
            if (onGround)
            {
                dreamJump = false;
                jumpGraceTimer = 0.1f;
            }
            else if (jumpGraceTimer > 0.0)
                jumpGraceTimer -= Engine.DeltaTime;
            if (dashCooldownTimer > 0.0)
                dashCooldownTimer -= Engine.DeltaTime;
            if (dashRefillCooldownTimer > 0.0)
                dashRefillCooldownTimer -= Engine.DeltaTime;
            else if (SaveData.Instance.Assists.DashMode == Assists.DashModes.Infinite && !level.InCutscene)
                RefillDash();
            else if (!Inventory.NoRefills)
            {
                if (StateMachine.State == 3)
                    RefillDash();
                else if (onGround && (CollideCheck<Solid, NegaBlock>(Position + Vector2.UnitY) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY)) && (!CollideCheck<Spikes>(Position) || SaveData.Instance.Assists.Invincible))
                    RefillDash();
            }
            if (varJumpTimer > 0.0)
                varJumpTimer -= Engine.DeltaTime;
            if (AutoJumpTimer > 0.0)
            {
                if (AutoJump)
                {
                    AutoJumpTimer -= Engine.DeltaTime;
                    if (AutoJumpTimer <= 0.0)
                        AutoJump = false;
                }
                else
                    AutoJumpTimer = 0.0f;
            }
            if (forceMoveXTimer > 0.0)
            {
                forceMoveXTimer -= Engine.DeltaTime;
                moveX = forceMoveX;
            }
            else
            {
                moveX = Input.MoveX.Value;
                climbHopSolid = null;
            }
            if (climbHopSolid != null && !climbHopSolid.Collidable)
                climbHopSolid = null;
            else if (climbHopSolid != null && climbHopSolid.Position != climbHopSolidPosition)
            {
                Vector2 vector2 = climbHopSolid.Position - climbHopSolidPosition;
                climbHopSolidPosition = climbHopSolid.Position;
                MoveHExact((int) vector2.X);
                MoveVExact((int) vector2.Y);
            }
            if (noWindTimer > 0.0)
                noWindTimer -= Engine.DeltaTime;
            if (moveX != 0 && InControl && StateMachine.State != 1 && StateMachine.State != 8 && StateMachine.State != 5 && StateMachine.State != 6)
            {
                Facings moveX = (Facings) this.moveX;
                if (moveX != Facing && Ducking)
                    Sprite.Scale = new Vector2(0.8f, 1.2f);
                Facing = moveX;
            }
            lastAim = Input.GetAimVector(Facing);
            if (wallSpeedRetentionTimer > 0.0)
            {
                if (Math.Sign(Speed.X) == -Math.Sign(wallSpeedRetained))
                    wallSpeedRetentionTimer = 0.0f;
                else if (!CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(wallSpeedRetained)))
                {
                    Speed.X = wallSpeedRetained;
                    wallSpeedRetentionTimer = 0.0f;
                }
                else
                    wallSpeedRetentionTimer -= Engine.DeltaTime;
            }
            if (hopWaitX != 0)
            {
                if (Math.Sign(Speed.X) == -hopWaitX || Speed.Y > 0.0)
                    hopWaitX = 0;
                else if (!CollideCheck<Solid>(Position + Vector2.UnitX * hopWaitX))
                {
                    lowFrictionStopTimer = 0.15f;
                    Speed.X = hopWaitXSpeed;
                    hopWaitX = 0;
                }
            }
            if (windTimeout > 0.0)
                windTimeout -= Engine.DeltaTime;
            Vector2 vector2_1 = windDirection;
            if (ForceStrongWindHair.Length() > 0.0)
                vector2_1 = ForceStrongWindHair;
            if (windTimeout > 0.0 && vector2_1.X != 0.0)
            {
                windHairTimer += Engine.DeltaTime * 8f;
                Hair.StepPerSegment = new Vector2(vector2_1.X * 5f, (float) Math.Sin(windHairTimer));
                Hair.StepInFacingPerSegment = 0.0f;
                Hair.StepApproach = 128f;
                Hair.StepYSinePerSegment = 0.0f;
            }
            else if (Dashes > 1)
            {
                Hair.StepPerSegment = new Vector2((float) Math.Sin(Scene.TimeActive * 2.0) * 0.7f - (int)Facing * 3, (float) Math.Sin(Scene.TimeActive * 1.0));
                Hair.StepInFacingPerSegment = 0.0f;
                Hair.StepApproach = 90f;
                Hair.StepYSinePerSegment = 1f;
                Hair.StepPerSegment.Y += vector2_1.Y * 2f;
            }
            else
            {
                Hair.StepPerSegment = new Vector2(0.0f, 2f);
                Hair.StepInFacingPerSegment = 0.5f;
                Hair.StepApproach = 64f;
                Hair.StepYSinePerSegment = 0.0f;
                Hair.StepPerSegment.Y += vector2_1.Y * 0.5f;
            }
            if (StateMachine.State == 5)
                Sprite.HairCount = 1;
            else if (StateMachine.State != 19)
                Sprite.HairCount = Dashes > 1 ? 5 : startHairCount;
            if (minHoldTimer > 0.0)
                minHoldTimer -= Engine.DeltaTime;
            if (launched)
            {
                if (Speed.LengthSquared() < 19600.0)
                {
                    launched = false;
                }
                else
                {
                    float launchedTimer = this.launchedTimer;
                    this.launchedTimer += Engine.DeltaTime;
                    if (this.launchedTimer >= 0.5)
                    {
                        launched = false;
                        this.launchedTimer = 0.0f;
                    }
                    else if (Calc.OnInterval(this.launchedTimer, launchedTimer, 0.15f))
                        level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, Speed.Angle(), Color.White));
                }
            }
            else
                launchedTimer = 0.0f;
            if (IsTired)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                if (!wasTired)
                    wasTired = true;
            }
            else
                wasTired = false;
            base.Update();
            Light.Position = !Ducking ? normalLightOffset : duckingLightOffset;
            if (!onGround && Speed.Y <= 0.0 && (StateMachine.State != 1 || lastClimbMove == -1) && CollideCheck<JumpThru>() && !JumpThruBoostBlockedCheck())
                MoveV(-40f * Engine.DeltaTime);
            if (!onGround && DashAttacking && DashDir.Y == 0.0 && (CollideCheck<Solid>(Position + Vector2.UnitY * 3f) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY * 3f)) && !DashCorrectCheck(Vector2.UnitY * 3f))
                MoveVExact(3);
            if (Speed.Y > 0.0 && CanUnDuck && Collider != starFlyHitbox && !onGround && jumpGraceTimer <= 0.0)
                Ducking = false;
            if (StateMachine.State is not Player.StDreamDash and not Player.StAttract)
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            if (StateMachine.State is not Player.StDreamDash and not Player.StAttract)
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (StateMachine.State == 3)
            {
                if ((double) Speed.Y is < 0f and >= (-60f))
                {
                    while (!SwimCheck())
                    {
                        Speed.Y = 0.0f;
                        if (MoveVExact(1))
                            break;
                    }
                }
            }
            else if (StateMachine.State == 0 && SwimCheck())
                StateMachine.State = 3;
            else if (StateMachine.State == 1 && SwimCheck())
            {
                Water water = CollideFirst<Water>(Position);
                if (water != null && Center.Y < (double) water.Center.Y)
                {
                    while (SwimCheck() && !MoveVExact(-1));
                    if (SwimCheck())
                        StateMachine.State = 3;
                }
                else
                    StateMachine.State = 3;
            }
            if ((Sprite.CurrentAnimationID == null || !Sprite.CurrentAnimationID.Equals("wallslide") ? 0 : (Speed.Y > 0.0 ? 1 : 0)) != 0)
            {
                if (!wallSlideSfx.Playing)
                    Loop(wallSlideSfx, "event:/char/madeline/wallslide");
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.UnitX * (float) Facing, temp));
                if (platformByPriority != null)
                    wallSlideSfx.Param("surface_index", platformByPriority.GetWallSoundIndex(this, (int)Facing));
            }
            else
                Stop(wallSlideSfx);
            UpdateSprite();
            UpdateCarry();
            if (StateMachine.State != 18)
            {
                foreach (Trigger entity in Scene.Tracker.GetEntities<Trigger>())
                {
                    if (CollideCheck(entity))
                    {
                        if (!entity.Triggered)
                        {
                            entity.Triggered = true;
                            triggersInside.Add(entity);
                            entity.OnEnter(this);
                        }
                        entity.OnStay(this);
                    }
                    else if (entity.Triggered)
                    {
                        triggersInside.Remove(entity);
                        entity.Triggered = false;
                        entity.OnLeave(this);
                    }
                }
            }
            StrawberriesBlocked = CollideCheck<BlockField>();
            if (InControl || ForceCameraUpdate)
            {
                if (StateMachine.State == 18)
                {
                    level.Camera.Position = CameraTarget;
                }
                else
                {
                    Vector2 position = level.Camera.Position;
                    Vector2 cameraTarget = CameraTarget;
                    float num = StateMachine.State == 20 ? 8f : 1f;
                    level.Camera.Position = position + (cameraTarget - position) * (1f - (float) Math.Pow(0.0099999997764825821 / num, Engine.DeltaTime));
                }
            }
            if (!Dead && StateMachine.State != 21)
            {
                Collider collider = Collider;
                Collider = hurtbox;
                foreach (PlayerCollider component in Scene.Tracker.GetComponents<PlayerCollider>())
                {
                    if (component.Check(this) && Dead)
                    {
                        Collider = collider;
                        return;
                    }
                }
                if (Collider == hurtbox)
                    Collider = collider;
            }
            if (InControl && !Dead && StateMachine.State != 9 && EnforceLevelBounds)
                level.EnforceBounds(this);
            UpdateChaserStates();
            UpdateHair(true);
            if (wasDucking != Ducking)
            {
                wasDucking = Ducking;
                if (wasDucking)
                    Play("event:/char/madeline/duck");
                else if (onGround)
                    Play("event:/char/madeline/stand");
            }
            if (Speed.X != 0.0 && (StateMachine.State == 3 && !SwimUnderwaterCheck() || StateMachine.State == 0 && CollideCheck<Water>(Position)))
            {
                if (!swimSurfaceLoopSfx.Playing)
                    swimSurfaceLoopSfx.Play("event:/char/madeline/water_move_shallow");
            }
            else
                swimSurfaceLoopSfx.Stop();
            wasOnGround = onGround;
            windMovedUp = false;
        }

        private void CreateTrail()
        {
            Vector2 scale = new(Math.Abs(Sprite.Scale.X) * (float) Facing, Sprite.Scale.Y);
            if (Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline)
                TrailManager.Add(this, scale, wasDashB ? Player.NormalBadelineHairColor : Player.UsedBadelineHairColor);
            else
                TrailManager.Add(this, scale, wasDashB ? Player.NormalHairColor : UsedHairColor);
        }

        public void CleanUpTriggers()
        {
            if (triggersInside.Count <= 0)
                return;
            foreach (Trigger trigger in triggersInside)
            {
                trigger.OnLeave(this);
                trigger.Triggered = false;
            }
            triggersInside.Clear();
        }

        private void UpdateChaserStates()
        {
            while (ChaserStates.Count > 0 && Scene.TimeActive - (double) ChaserStates[0].TimeStamp > 4.0)
                ChaserStates.RemoveAt(0);
            ChaserStates.Add(new ChaserState(this));
            activeSounds.Clear();
        }

        private void StartHair()
        {
            if (startHairCalled)
                return;
            startHairCalled = true;
            Hair.Facing = Facing;
            Hair.Start();
            UpdateHair(true);
        }

        public void UpdateHair(bool applyGravity)
        {
            if (StateMachine.State == 19)
            {
                Hair.Color = Sprite.Color;
                applyGravity = false;
            }
            else if (Dashes == 0 && Dashes < MaxDashes)
            {
                Hair.Color = Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline ? Color.Lerp(Hair.Color, UsedHairColor, 6f * Engine.DeltaTime) : Color.Lerp(Hair.Color, Player.UsedBadelineHairColor, 6f * Engine.DeltaTime);
            }
            else
            {
                Color color;
                if (lastDashes != Dashes)
                {
                    color = Player.FlashHairColor;
                    hairFlashTimer = 0.12f;
                }
                else if (hairFlashTimer > 0.0)
                {
                    color = Player.FlashHairColor;
                    hairFlashTimer -= Engine.DeltaTime;
                }
                else
                    color = Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline ? (Dashes != 2 ? Player.NormalHairColor : TwoDashesHairColor) : (Dashes != 2 ? Player.NormalBadelineHairColor : Player.TwoDashesBadelineHairColor);
                Hair.Color = color;
            }
            if (OverrideHairColor.HasValue)
                Hair.Color = OverrideHairColor.Value;
            Hair.Facing = Facing;
            Hair.SimulateMotion = applyGravity;
            lastDashes = Dashes;
        }

        private void UpdateSprite()
        {
            Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
            if (InControl && Sprite.CurrentAnimationID != "throw" && StateMachine.State != 20 && StateMachine.State != 18 && StateMachine.State != 19 && StateMachine.State != 21)
            {
                if (StateMachine.State == 22)
                    Sprite.Play("fallFast");
                else if (StateMachine.State == 10)
                    Sprite.Play("launch");
                else if (StateMachine.State == 8)
                    Sprite.Play("pickup");
                else if (StateMachine.State == 3)
                {
                    if (Input.MoveY.Value > 0)
                        Sprite.Play("swimDown");
                    else if (Input.MoveY.Value < 0)
                        Sprite.Play("swimUp");
                    else
                        Sprite.Play("swimIdle");
                }
                else if (StateMachine.State == 9)
                {
                    if (Sprite.CurrentAnimationID is not "dreamDashIn" and not "dreamDashLoop")
                        Sprite.Play("dreamDashIn");
                }
                else if (Sprite.DreamDashing && Sprite.LastAnimationID != "dreamDashOut")
                    Sprite.Play("dreamDashOut");
                else if (Sprite.CurrentAnimationID != "dreamDashOut")
                {
                    if (DashAttacking)
                    {
                        if (onGround && DashDir.Y == 0.0 && !Ducking && Speed.X != 0.0 && moveX == -Math.Sign(Speed.X))
                        {
                            if (Scene.OnInterval(0.02f))
                                Dust.Burst(Position, -1.57079637f);
                            Sprite.Play("skid");
                        }
                        else if (Ducking)
                            Sprite.Play("duck");
                        else
                            Sprite.Play("dash");
                    }
                    else if (StateMachine.State == 1)
                    {
                        if (lastClimbMove < 0)
                            Sprite.Play("climbUp");
                        else if (lastClimbMove > 0)
                            Sprite.Play("wallslide");
                        else if (!CollideCheck<Solid>(Position + new Vector2((float) Facing, 6f)))
                            Sprite.Play("dangling");
                        else if ((double) (float) Input.MoveX == -(int)Facing)
                        {
                            if (Sprite.CurrentAnimationID != "climbLookBack")
                                Sprite.Play("climbLookBackStart");
                        }
                        else
                            Sprite.Play("wallslide");
                    }
                    else if (Ducking && StateMachine.State == 0)
                        Sprite.Play("duck");
                    else if (onGround)
                    {
                        fastJump = false;
                        if (Holding == null && moveX != 0 && CollideCheck<Solid>(Position + Vector2.UnitX * moveX) && !ClimbBlocker.EdgeCheck(level, this, moveX))
                            Sprite.Play("push");
                        else if (Math.Abs(Speed.X) <= 25.0 && moveX == 0)
                        {
                            if (Holding != null)
                                Sprite.Play("idle_carry");
                            else if (!Scene.CollideCheck<Solid>(Position + new Vector2((float) Facing, 2f)) && !Scene.CollideCheck<Solid>(Position + new Vector2((int)Facing * 4, 2f)) && !CollideCheck<JumpThru>(Position + new Vector2((int)Facing * 4, 2f)))
                                Sprite.Play("edge");
                            else if (!Scene.CollideCheck<Solid>(Position + new Vector2(-(int)Facing, 2f)) && !Scene.CollideCheck<Solid>(Position + new Vector2(-(int)Facing * 4, 2f)) && !CollideCheck<JumpThru>(Position + new Vector2(-(int)Facing * 4, 2f)))
                                Sprite.Play("edgeBack");
                            else if (Input.MoveY.Value == -1)
                            {
                                if (Sprite.LastAnimationID != "lookUp")
                                    Sprite.Play("lookUp");
                            }
                            else if (Sprite.CurrentAnimationID != null && (!Sprite.CurrentAnimationID.Contains("idle") || Sprite.CurrentAnimationID == "idle_carry" && Holding == null))
                                Sprite.Play("idle");
                        }
                        else if (Holding != null)
                            Sprite.Play("runSlow_carry");
                        else if (Math.Sign(Speed.X) == -moveX && moveX != 0)
                        {
                            if (Math.Abs(Speed.X) > 90.0)
                                Sprite.Play("skid");
                            else if (Sprite.CurrentAnimationID != "skid")
                                Sprite.Play("flip");
                        }
                        else if (windDirection.X != 0.0 && windTimeout > 0.0 && Facing == (Facings) (-Math.Sign(windDirection.X)))
                            Sprite.Play("runWind");
                        else if (!Sprite.Running || Sprite.CurrentAnimationID == "runWind" || Sprite.CurrentAnimationID == "runSlow_carry" && Holding == null)
                        {
                            if (Math.Abs(Speed.X) < 45.0)
                                Sprite.Play("runSlow");
                            else
                                Sprite.Play("runFast");
                        }
                    }
                    else if (wallSlideDir != 0 && Holding == null)
                        Sprite.Play("wallslide");
                    else if (Speed.Y < 0.0)
                    {
                        if (Holding != null)
                            Sprite.Play("jumpSlow_carry");
                        else if (fastJump || Math.Abs(Speed.X) > 90.0)
                        {
                            fastJump = true;
                            Sprite.Play("jumpFast");
                        }
                        else
                            Sprite.Play("jumpSlow");
                    }
                    else if (Holding != null)
                        Sprite.Play("fallSlow_carry");
                    else if (fastJump || Speed.Y >= 160.0 || level.InSpace)
                    {
                        fastJump = true;
                        if (Sprite.LastAnimationID != "fallFast")
                            Sprite.Play("fallFast");
                    }
                    else
                        Sprite.Play("fallSlow");
                }
            }
            if (StateMachine.State == 11)
                return;
            if (level.InSpace)
                Sprite.Rate = 0.5f;
            else
                Sprite.Rate = 1f;
        }

        public void CreateSplitParticles() => level.Particles.Emit(Player.P_Split, 16, Center, Vector2.One * 6f);

        public Vector2 CameraTarget
        {
            get
            {
                Vector2 cameraTarget = new();
                Vector2 vector2 = new(X - Celeste.GameWidth * 0.5f, Y - Celeste.GameHeight * 0.5f);
                if (StateMachine.State != StReflectionFall)
                    vector2 += new Vector2(level.CameraOffset.X, level.CameraOffset.Y);
                switch (StateMachine.State)
                {
                    case StRedDash:
                        vector2.X += 48 * Math.Sign(Speed.X);
                        vector2.Y += 48 * Math.Sign(Speed.Y);
                        break;
                    
                    case StSummitLaunch:
                        vector2.Y -= 64f;
                        break;
                    
                    case StReflectionFall:
                        vector2.Y += 32f;
                        break;

                    case StStarFly:
                        vector2.X += 0.2f * Speed.X;
                        vector2.Y += 0.2f * Speed.Y;
                        break;
                }
                if (CameraAnchorLerp.Length() > 0f)
                {
                    switch (CameraAnchorIgnoreX)
                    {
                        case true when !CameraAnchorIgnoreY:
                            vector2.Y = MathHelper.Lerp(vector2.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
                            break;
                        
                        case false when CameraAnchorIgnoreY:
                            vector2.X = MathHelper.Lerp(vector2.X, CameraAnchor.X, CameraAnchorLerp.X);
                            break;
                        
                        default:
                        {
                            if (CameraAnchorLerp.X == CameraAnchorLerp.Y)
                            {
                                vector2 = Vector2.Lerp(vector2, CameraAnchor, CameraAnchorLerp.X);
                            }
                            else
                            {
                                vector2.X = MathHelper.Lerp(vector2.X, CameraAnchor.X, CameraAnchorLerp.X);
                                vector2.Y = MathHelper.Lerp(vector2.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
                            }

                            break;
                        }
                    }
                }
                if (EnforceLevelBounds)
                {
                    cameraTarget.X = MathHelper.Clamp(vector2.X, level.Bounds.Left, level.Bounds.Right - Celeste.GameWidth);
                    float top = level.Bounds.Top;
                    float max = level.Bounds.Bottom - Celeste.GameHeight;
                    float num = MathHelper.Clamp(vector2.Y, top, max);
                    cameraTarget.Y = num;
                }
                else
                {
                    cameraTarget = vector2;
                }

                if (level.CameraLockMode != Level.CameraLockModes.None)
                {
                    CameraLocker component = Scene.Tracker.GetComponent<CameraLocker>();
                    if (level.CameraLockMode != Level.CameraLockModes.BoostSequence)
                    {
                        cameraTarget.X = Math.Max(cameraTarget.X, level.Camera.X);
                        if (component != null)
                        {
                            float val2 = Math.Max(level.Bounds.Left, component.Entity.X - component.MaxXOffset);
                            float num = Math.Min(cameraTarget.X, val2);
                            cameraTarget.X = num;
                        }
                    }
                    switch (level.CameraLockMode)
                    {
                        case Level.CameraLockModes.FinalBoss:
                        {
                            cameraTarget.Y = Math.Max(cameraTarget.Y, level.Camera.Y);
                            if (component != null)
                            {
                                float val2 = Math.Max(level.Bounds.Top, component.Entity.Y - component.MaxYOffset);
                                float num = Math.Min(cameraTarget.Y, val2);
                                cameraTarget.Y = num;
                            }

                            break;
                        }
                        
                        case Level.CameraLockModes.BoostSequence:
                        {
                            level.CameraUpwardMaxY = Math.Min(level.Camera.Y + Celeste.GameHeight, level.CameraUpwardMaxY);
                            cameraTarget.Y = Math.Min(cameraTarget.Y, level.CameraUpwardMaxY);
                            if (component != null)
                            {
                                float val2 = Math.Min(level.Bounds.Bottom - Celeste.GameHeight, component.Entity.Y - component.MaxYOffset);
                                float num = Math.Max(cameraTarget.Y, val2);
                                cameraTarget.Y = num;
                            }

                            break;
                        }
                    }
                }
                foreach (Entity entity in Scene.Tracker.GetEntities<Killbox>()
                             .Where(entity => entity.Collidable && Top < entity.Bottom && Right > entity.Left && Left < entity.Right))
                {
                    cameraTarget.Y = Math.Min(cameraTarget.Y, entity.Top - Celeste.GameHeight);
                }
                return cameraTarget;
            }
        }

        public bool GetChasePosition(float sceneTime, float timeAgo, out ChaserState chaseState)
        {
            if (!Dead)
            {
                bool flag = false;
                foreach (ChaserState chaserState in ChaserStates)
                {
                    float num = sceneTime - chaserState.TimeStamp;
                    if (num <= (double) timeAgo)
                    {
                        if (flag || timeAgo - (double) num < 0.019999999552965164)
                        {
                            chaseState = chaserState;
                            return true;
                        }
                        chaseState = new ChaserState();
                        return false;
                    }
                    flag = true;
                }
            }
            chaseState = new ChaserState();
            return false;
        }

        public bool CanRetry
        {
            get
            {
                return StateMachine.State switch
                {
                    12 or 13 or 14 or 15 or 18 or 25 => false,
                    _ => true,
                };
            }
        }

        public bool TimePaused
        {
            get
            {
                if (Dead)
                    return true;
                return StateMachine.State switch
                {
                    10 or 12 or 13 or 14 or 15 or 25 => true,
                    _ => false,
                };
            }
        }

        public bool InControl => StateMachine.State switch
        {
            StDummy
                or StIntroWalk
                or StIntroJump
                or StIntroRespawn
                or StIntroWakeUp
                or StBirdDashTutorial
                or StFrozen
                or StIntroMoonJump
                or StIntroThinkForABit => false,
            _ => true,
        };

        public PlayerInventory Inventory => level is { Session: not null } ? level.Session.Inventory : PlayerInventory.Default;

        public void OnTransition()
        {
            wallSlideTimer = 1.2f;
            jumpGraceTimer = 0.0f;
            forceMoveXTimer = 0.0f;
            ChaserStates.Clear();
            RefillDash();
            RefillStamina();
            Leader.TransferFollowers();
        }

        public bool TransitionTo(Vector2 target, Vector2 direction)
        {
            MoveTowardsX(target.X, 60f * Engine.DeltaTime);
            MoveTowardsY(target.Y, 60f * Engine.DeltaTime);
            UpdateHair(false);
            UpdateCarry();
            if (!(Position == target))
                return false;
            ZeroRemainderX();
            ZeroRemainderY();
            Speed.X = (int)Math.Round(Speed.X);
            Speed.Y = (int)Math.Round(Speed.Y);
            return true;
        }

        public void BeforeSideTransition()
        {
        }

        public void BeforeDownTransition()
        {
            if (StateMachine.State is not 5 and not 18 and not 19)
            {
                StateMachine.State = 0;
                Speed.Y = Math.Max(0.0f, Speed.Y);
                AutoJump = false;
                varJumpTimer = 0.0f;
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<Platform>())
            {
                if (entity is not SolidTiles && CollideCheckOutside(entity, Position + Vector2.UnitY * Height))
                    entity.Collidable = false;
            }
        }

        public void BeforeUpTransition()
        {
            Speed.X = 0.0f;
            if (StateMachine.State is not 5 and not 18 and not 19)
            {
                varJumpSpeed = Speed.Y = -105f;
                StateMachine.State = StateMachine.State != 10 ? 0 : 13;
                AutoJump = true;
                AutoJumpTimer = 0.0f;
                varJumpTimer = 0.2f;
            }
            dashCooldownTimer = 0.2f;
        }

        public bool OnSafeGround { get; private set; }

        public bool LoseShards => onGround;

        private bool LaunchedBoostCheck()
        {
            if (LiftBoost.LengthSquared() >= 10000.0 && Speed.LengthSquared() >= 48400.0)
            {
                launched = true;
                return true;
            }
            launched = false;
            return false;
        }

        public void HiccupJump()
        {
            switch (StateMachine.State)
            {
                case Player.StClimb:
                    StateMachine.State = 0;
                    varJumpSpeed = Speed.Y = -60f;
                    varJumpTimer = 0.15f;
                    Speed.X = 130f * -(int)Facing;
                    AutoJump = true;
                    AutoJumpTimer = 0.0f;
                    sweatSprite.Play("jump", true);
                    break;
                case Player.StBoost:
                case Player.StLaunch:
                case Player.StAttract:
                    sweatSprite.Play("jump", true);
                    break;
                case Player.StRedDash:
                case Player.StDreamDash:
                    Speed = Speed.X < 0.0 || Speed.X == 0.0 && Speed.Y < 0.0 ? Speed.Rotate(0.17453292f) : Speed.Rotate(-0.17453292f);
                    break;
                case 10:
                    return;
                case 11:
                    return;
                case 12:
                    return;
                case 13:
                    return;
                case 14:
                    return;
                case 15:
                    return;
                case 16:
                    return;
                case 17:
                    return;
                case 18:
                    return;
                case Player.StStarFly:
                    Speed = Speed.X <= 0.0 ? Speed.Rotate(-0.6981317f) : Speed.Rotate(0.6981317f);
                    break;
                case 21:
                    return;
                case 24:
                    return;
                default:
                    StateMachine.State = 0;
                    Speed.X = Calc.Approach(Speed.X, 0.0f, 40f);
                    if (Speed.Y > -60.0)
                    {
                        varJumpSpeed = Speed.Y = -60f;
                        varJumpTimer = 0.15f;
                        AutoJump = true;
                        AutoJumpTimer = 0.0f;
                        if (jumpGraceTimer > 0.0)
                            jumpGraceTimer = 0.6f;
                    }
                    sweatSprite.Play("jump", true);
                    break;
            }
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            Play(Ducking ? "event:/new_content/char/madeline/hiccup_ducking" : "event:/new_content/char/madeline/hiccup_standing");
        }

        public void Jump(bool particles = true, bool playSfx = true)
        {
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = false;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            Speed.X += 40f * moveX;
            Speed.Y = -105f;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;
            LaunchedBoostCheck();
            if (playSfx)
            {
                if (launched)
                    Play("event:/char/madeline/jump_assisted");
                if (dreamJump)
                    Play("event:/char/madeline/jump_dreamblock");
                else
                    Play("event:/char/madeline/jump");
            }
            Sprite.Scale = new Vector2(0.6f, 1.4f);
            if (particles)
            {
                int index = -1;
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                if (platformByPriority != null)
                    index = platformByPriority.GetLandSoundIndex(this);
                Dust.Burst(BottomCenter, -1.57079637f, 4, DustParticleFromSurfaceIndex(index));
            }
            ++SaveData.Instance.TotalJumps;
        }

        private void SuperJump()
        {
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = false;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            Speed.X = 260f * (float) Facing;
            Speed.Y = -105f;
            Speed += LiftBoost;
            gliderBoostTimer = 0.55f;
            Play("event:/char/madeline/jump");
            if (Ducking)
            {
                Ducking = false;
                Speed.X *= 1.25f;
                Speed.Y *= 0.5f;
                Play("event:/char/madeline/jump_superslide");
                gliderBoostDir = Calc.AngleToVector(-3f * (float) Math.PI / 16f, 1f);
            }
            else
            {
                gliderBoostDir = Calc.AngleToVector(-0.7853982f, 1f);
                Play("event:/char/madeline/jump_super");
            }
            varJumpSpeed = Speed.Y;
            launched = true;
            Sprite.Scale = new Vector2(0.6f, 1.4f);
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
            if (platformByPriority != null)
                index = platformByPriority.GetLandSoundIndex(this);
            Dust.Burst(BottomCenter, -1.57079637f, 4, DustParticleFromSurfaceIndex(index));
            ++SaveData.Instance.TotalJumps;
        }

        private bool WallJumpCheck(int dir)
        {
            int num = 3;
            bool flag = DashAttacking && DashDir.X == 0.0 && DashDir.Y == -1.0;
            if (flag)
            {
                Spikes.Directions directions = dir <= 0 ? Spikes.Directions.Right : Spikes.Directions.Left;
                foreach (Spikes entity in level.Tracker.GetEntities<Spikes>())
                {
                    if (entity.Direction == directions && CollideCheck(entity, Position + Vector2.UnitX * dir * 5f))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
                num = 5;
            return ClimbBoundsCheck(dir) && !ClimbBlocker.EdgeCheck(level, this, dir * num) && CollideCheck<Solid>(Position + Vector2.UnitX * dir * num);
        }

        private void WallJump(int dir)
        {
            Ducking = false;
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = false;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            lowFrictionStopTimer = 0.15f;
            if (Holding != null && Holding.SlowFall)
            {
                forceMoveX = dir;
                forceMoveXTimer = 0.26f;
            }
            else if (moveX != 0)
            {
                forceMoveX = dir;
                forceMoveXTimer = 0.16f;
            }
            if (LiftSpeed == Vector2.Zero)
            {
                Solid solid = CollideFirst<Solid>(Position + Vector2.UnitX * 3f * -dir);
                if (solid != null)
                    LiftSpeed = solid.LiftSpeed;
            }
            Speed.X = 130f * dir;
            Speed.Y = -105f;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;
            LaunchedBoostCheck();
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * dir * 4f, temp));
            if (platformByPriority != null)
            {
                index = platformByPriority.GetWallSoundIndex(this, -dir);
                Play("event:/char/madeline/landing", "surface_index", index);
                if (platformByPriority is DreamBlock)
                    (platformByPriority as DreamBlock).FootstepRipple(Position + new Vector2(dir * 3, -4f));
            }
            Play(dir < 0 ? "event:/char/madeline/jump_wall_right" : "event:/char/madeline/jump_wall_left");
            Sprite.Scale = new Vector2(0.6f, 1.4f);
            if (dir == -1)
                Dust.Burst(Center + Vector2.UnitX * 2f, -2.3561945f, 4, DustParticleFromSurfaceIndex(index));
            else
                Dust.Burst(Center + Vector2.UnitX * -2f, -0.7853982f, 4, DustParticleFromSurfaceIndex(index));
            ++SaveData.Instance.TotalWallJumps;
        }

        private void SuperWallJump(int dir)
        {
            Ducking = false;
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.25f;
            AutoJump = false;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.55f;
            gliderBoostDir = -Vector2.UnitY;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            Speed.X = 170f * dir;
            Speed.Y = -160f;
            Speed += LiftBoost;
            varJumpSpeed = Speed.Y;
            launched = true;
            Play(dir < 0 ? "event:/char/madeline/jump_wall_right" : "event:/char/madeline/jump_wall_left");
            Play("event:/char/madeline/jump_superwall");
            Sprite.Scale = new Vector2(0.6f, 1.4f);
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * dir * 4f, temp));
            if (platformByPriority != null)
                index = platformByPriority.GetWallSoundIndex(this, dir);
            if (dir == -1)
                Dust.Burst(Center + Vector2.UnitX * 2f, -2.3561945f, 4, DustParticleFromSurfaceIndex(index));
            else
                Dust.Burst(Center + Vector2.UnitX * -2f, -0.7853982f, 4, DustParticleFromSurfaceIndex(index));
            ++SaveData.Instance.TotalWallJumps;
        }

        private void ClimbJump()
        {
            if (!onGround)
            {
                Stamina -= 27.5f;
                sweatSprite.Play("jump", true);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            }
            dreamJump = false;
            Jump(false, false);
            if (moveX == 0)
            {
                wallBoostDir = -(int) Facing;
                wallBoostTimer = 0.2f;
            }
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * (float) Facing * 4f, temp));
            if (platformByPriority != null)
                index = platformByPriority.GetWallSoundIndex(this, (int) Facing);
            if (Facing == Facings.Right)
            {
                Play("event:/char/madeline/jump_climb_right");
                Dust.Burst(Center + Vector2.UnitX * 2f, -2.3561945f, 4, DustParticleFromSurfaceIndex(index));
            }
            else
            {
                Play("event:/char/madeline/jump_climb_left");
                Dust.Burst(Center + Vector2.UnitX * -2f, -0.7853982f, 4, DustParticleFromSurfaceIndex(index));
            }
        }

        public void Bounce(float fromY)
        {
            if (StateMachine.State == 4 && CurrentBooster != null)
            {
                CurrentBooster.PlayerReleased();
                CurrentBooster = null;
            }
            Collider collider = Collider;
            Collider = normalHitbox;
            MoveVExact((int) (fromY - (double) Bottom));
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            StateMachine.State = 0;
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = true;
            AutoJumpTimer = 0.1f;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            varJumpSpeed = Speed.Y = -140f;
            launched = false;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Sprite.Scale = new Vector2(0.6f, 1.4f);
            Collider = collider;
        }

        public void SuperBounce(float fromY)
        {
            if (StateMachine.State == 4 && CurrentBooster != null)
            {
                CurrentBooster.PlayerReleased();
                CurrentBooster = null;
            }
            Collider collider = Collider;
            Collider = normalHitbox;
            MoveV(fromY - Bottom);
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            StateMachine.State = 0;
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = true;
            AutoJumpTimer = 0.0f;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            Speed.X = 0.0f;
            varJumpSpeed = Speed.Y = -185f;
            launched = false;
            level.DirectionalShake(-Vector2.UnitY, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Sprite.Scale = new Vector2(0.5f, 1.5f);
            Collider = collider;
        }

        public bool SideBounce(int dir, float fromX, float fromY)
        {
            if (Math.Abs(Speed.X) > 240.0 && Math.Sign(Speed.X) == dir)
                return false;
            Collider collider = Collider;
            Collider = normalHitbox;
            MoveV(Calc.Clamp(fromY - Bottom, -4f, 4f));
            if (dir > 0)
                MoveH(fromX - Left);
            else if (dir < 0)
                MoveH(fromX - Right);
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            StateMachine.State = 0;
            jumpGraceTimer = 0.0f;
            varJumpTimer = 0.2f;
            AutoJump = true;
            AutoJumpTimer = 0.0f;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            forceMoveX = dir;
            forceMoveXTimer = 0.3f;
            wallBoostTimer = 0.0f;
            launched = false;
            Speed.X = 240f * dir;
            varJumpSpeed = Speed.Y = -140f;
            level.DirectionalShake(Vector2.UnitX * dir, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Sprite.Scale = new Vector2(1.5f, 0.5f);
            Collider = collider;
            return true;
        }

        public void Rebound(int direction = 0)
        {
            Speed.X = direction * 120f;
            Speed.Y = -120f;
            varJumpSpeed = Speed.Y;
            varJumpTimer = 0.15f;
            AutoJump = true;
            AutoJumpTimer = 0.0f;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            launched = false;
            lowFrictionStopTimer = 0.15f;
            forceMoveXTimer = 0.0f;
            StateMachine.State = 0;
        }

        public void ReflectBounce(Vector2 direction)
        {
            if (direction.X != 0.0)
                Speed.X = direction.X * 220f;
            if (direction.Y != 0.0)
                Speed.Y = direction.Y * 220f;
            AutoJumpTimer = 0.0f;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0.0f;
            launched = false;
            dashAttackTimer = 0.0f;
            gliderBoostTimer = 0.0f;
            forceMoveXTimer = 0.0f;
            StateMachine.State = 0;
        }

        public int MaxDashes => SaveData.Instance.Assists.DashMode != Assists.DashModes.Normal && !level.InCutscene ? 2 : Inventory.Dashes;

        public bool RefillDash()
        {
            if (Dashes >= MaxDashes)
                return false;
            Dashes = MaxDashes;
            return true;
        }

        public bool UseRefill(bool twoDashes)
        {
            int num = MaxDashes;
            if (twoDashes)
                num = 2;
            if (Dashes >= num && Stamina >= 20.0)
                return false;
            Dashes = num;
            RefillStamina();
            return true;
        }

        public void RefillStamina() => Stamina = 110f;

        public PlayerDeadBody Die(
            Vector2 direction,
            bool evenIfInvincible = false,
            bool registerDeathInStats = true)
        {
            Session session = level.Session;
            bool flag = !evenIfInvincible && SaveData.Instance.Assists.Invincible;
            if (Dead || flag || StateMachine.State == 18)
                return null;
            Stop(wallSlideSfx);
            if (registerDeathInStats)
            {
                ++session.Deaths;
                ++session.DeathsInCurrentLevel;
                SaveData.Instance.AddDeath(session.Area);
            }
            Strawberry goldenStrawb = null;
            foreach (Follower follower in Leader.Followers)
            {
                if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                    goldenStrawb = follower.Entity as Strawberry;
            }
            Dead = true;
            Leader.LoseFollowers();
            Depth = -1000000;
            Speed = Vector2.Zero;
            StateMachine.Locked = true;
            Collidable = false;
            Drop();
            LastBooster?.PlayerDied();
            level.InCutscene = false;
            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            PlayerDeadBody playerDeadBody = new(this, direction);
            if (goldenStrawb != null)
            {
                playerDeadBody.HasGolden = true;
                playerDeadBody.DeathAction = () => Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session)
                {
                    GoldenStrawberryEntryLevel = goldenStrawb.ID.Level
                };
            }
            Scene.Add(playerDeadBody);
            Scene.Remove(this);
            Scene.Tracker.GetEntity<Lookout>()?.StopInteracting();
            return playerDeadBody;
        }

        private Vector2 LiftBoost
        {
            get
            {
                Vector2 liftSpeed = LiftSpeed;
                if (Math.Abs(liftSpeed.X) > 250.0)
                    liftSpeed.X = 250f * Math.Sign(liftSpeed.X);
                if (liftSpeed.Y > 0.0)
                    liftSpeed.Y = 0.0f;
                else if (liftSpeed.Y < -130.0)
                    liftSpeed.Y = -130f;
                return liftSpeed;
            }
        }

        public bool Ducking
        {
            get => Collider == duckHitbox || Collider == duckHurtbox;
            set
            {
                if (value)
                {
                    Collider = duckHitbox;
                    hurtbox = duckHurtbox;
                }
                else
                {
                    Collider = normalHitbox;
                    hurtbox = normalHurtbox;
                }
            }
        }

        public bool CanUnDuck
        {
            get
            {
                if (!Ducking)
                    return true;
                Collider collider = Collider;
                Collider = normalHitbox;
                int num = !CollideCheck<Solid>() ? 1 : 0;
                Collider = collider;
                return num != 0;
            }
        }

        public bool CanUnDuckAt(Vector2 at)
        {
            Vector2 position = Position;
            Position = at;
            int num = CanUnDuck ? 1 : 0;
            Position = position;
            return num != 0;
        }

        public bool DuckFreeAt(Vector2 at)
        {
            Vector2 position = Position;
            Collider collider = Collider;
            Position = at;
            Collider = duckHitbox;
            int num = !CollideCheck<Solid>() ? 1 : 0;
            Position = position;
            Collider = collider;
            return num != 0;
        }

        private void Duck() => Collider = duckHitbox;

        private void UnDuck() => Collider = normalHitbox;

        public Holdable Holding { get; set; }

        public void UpdateCarry()
        {
            if (Holding == null)
                return;
            if (Holding.Scene == null)
                Holding = null;
            else
                Holding.Carry(Position + carryOffset + Vector2.UnitY * Sprite.CarryYOffset);
        }

        public void Swat(int dir)
        {
            if (Holding == null)
                return;
            Holding.Release(new Vector2(0.8f * dir, -0.25f));
            Holding = null;
        }

        private bool Pickup(Holdable pickup)
        {
            if (!pickup.Pickup(this))
                return false;
            Ducking = false;
            Holding = pickup;
            minHoldTimer = 0.35f;
            return true;
        }

        public void Throw()
        {
            if (Holding == null)
                return;
            if (Input.MoveY.Value == 1)
            {
                Drop();
            }
            else
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                Holding.Release(Vector2.UnitX * (float) Facing);
                Speed.X += 80f * -(int)Facing;
                Play("event:/char/madeline/crystaltheo_throw");
                Sprite.Play("throw");
            }
            Holding = null;
        }

        public void Drop()
        {
            if (Holding == null)
                return;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            Holding.Release(Vector2.Zero);
            Holding = null;
        }

        public void StartJumpGraceTime() => jumpGraceTimer = 0.1f;

        public override bool IsRiding(Solid solid)
        {
            if (StateMachine.State == 23)
                return false;
            if (StateMachine.State == 9)
                return CollideCheck(solid);
            if (StateMachine.State is 1 or 6)
                return CollideCheck(solid, Position + Vector2.UnitX * (float) Facing);
            return climbTriggerDir != 0 ? CollideCheck(solid, Position + Vector2.UnitX * climbTriggerDir) : base.IsRiding(solid);
        }

        public override bool IsRiding(JumpThru jumpThru) => StateMachine.State != 9 && StateMachine.State != 1 && Speed.Y >= 0.0 && base.IsRiding(jumpThru);

        public bool BounceCheck(float y) => Bottom <= y + 3.0;

        public void PointBounce(Vector2 from)
        {
            if (StateMachine.State == 2)
                StateMachine.State = 0;
            if (StateMachine.State == 4 && CurrentBooster != null)
                CurrentBooster.PlayerReleased();
            RefillDash();
            RefillStamina();
            Vector2 vector2 = (Center - from).SafeNormalize();
            if ((double) vector2.Y is > (-0.20000000298023224) and <= 0.40000000596046448)
                vector2.Y = -0.2f;
            Speed = vector2 * 220f;
            Speed.X *= 1.5f;
            if (Math.Abs(Speed.X) >= 100.0)
                return;
            if (Speed.X == 0.0)
                Speed.X = -(int)Facing * 100f;
            else
                Speed.X = Math.Sign(Speed.X) * 100f;
        }

        private void WindMove(Vector2 move)
        {
            if (JustRespawned || noWindTimer > 0.0 || !InControl || StateMachine.State == 4 || StateMachine.State == 2 || StateMachine.State == 10)
                return;
            if (move.X != 0.0 && StateMachine.State != 1)
            {
                windTimeout = 0.2f;
                windDirection.X = Math.Sign(move.X);
                if (!CollideCheck<Solid>(Position + Vector2.UnitX * -Math.Sign(move.X) * 3f))
                {
                    if (Ducking && onGround)
                        move.X *= 0.0f;
                    move.X = move.X >= 0.0 ? Math.Min(move.X, level.Bounds.Right - (ExactPosition.X + Collider.Right)) : Math.Max(move.X, level.Bounds.Left - (ExactPosition.X + Collider.Left));
                    MoveH(move.X);
                }
            }
            if (move.Y == 0.0)
                return;
            windTimeout = 0.2f;
            windDirection.Y = Math.Sign(move.Y);
            if ((double) Bottom <= level.Bounds.Top || Speed.Y >= 0.0 && OnGround())
                return;
            if (StateMachine.State == 1)
            {
                if (move.Y <= 0.0 || climbNoMoveTimer > 0.0)
                    return;
                move.Y *= 0.4f;
            }
            if (move.Y < 0.0)
                windMovedUp = true;
            MoveV(move.Y);
        }

        private void OnCollideH(CollisionData data)
        {
            canCurveDash = false;
            if (StateMachine.State == Player.StStarFly)
            {
                if (starFlyTimer < 0.2f)
                {
                    Speed.X = 0.0f;
                }
                else
                {
                    Play("event:/game/06_reflection/feather_state_bump");
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    Speed.X *= -0.5f;
                }
            }
            else
            {
                if (StateMachine.State == Player.StDreamDash)
                    return;
                if (DashAttacking && data.Hit != null && data.Hit.OnDashCollide != null && data.Direction.X == (double) Math.Sign(DashDir.X))
                {
                    DashCollisionResults collisionResults = data.Hit.OnDashCollide(this, data.Direction);
                    if (collisionResults == DashCollisionResults.NormalOverride)
                        collisionResults = DashCollisionResults.NormalCollision;
                    else if (StateMachine.State == 5)
                        collisionResults = DashCollisionResults.Ignore;
                    switch (collisionResults)
                    {
                        case DashCollisionResults.Rebound:
                            Rebound(-Math.Sign(Speed.X));
                            return;
                        case DashCollisionResults.Bounce:
                            ReflectBounce(new Vector2(-Math.Sign(Speed.X), 0.0f));
                            return;
                        case DashCollisionResults.Ignore:
                            return;
                    }
                }
                if (StateMachine.State is Player.StDash or Player.StRedDash)
                {
                    if (onGround && DuckFreeAt(Position + Vector2.UnitX * Math.Sign(Speed.X)))
                    {
                        Ducking = true;
                        return;
                    }
                    if (Speed.Y == 0.0 && Speed.X != 0.0)
                    {
                        for (int index1 = 1; index1 <= 4; ++index1)
                        {
                            for (int index2 = 1; index2 >= -1; index2 -= 2)
                            {
                                Vector2 add = new(Math.Sign(Speed.X), index1 * index2);
                                Vector2 at = Position + add;
                                if (!CollideCheck<Solid>(at) && CollideCheck<Solid>(at - Vector2.UnitY * index2) && !DashCorrectCheck(add))
                                {
                                    MoveVExact(index1 * index2);
                                    MoveHExact(Math.Sign(Speed.X));
                                    return;
                                }
                            }
                        }
                    }
                }
                if (DreamDashCheck(Vector2.UnitX * Math.Sign(Speed.X)))
                {
                    StateMachine.State = Player.StDreamDash;
                    dashAttackTimer = 0.0f;
                    gliderBoostTimer = 0.0f;
                }
                else
                {
                    if (wallSpeedRetentionTimer <= 0.0)
                    {
                        wallSpeedRetained = Speed.X;
                        wallSpeedRetentionTimer = 0.06f;
                    }
                    if (data.Hit != null && data.Hit.OnCollide != null)
                        data.Hit.OnCollide(data.Direction);
                    Speed.X = 0.0f;
                    dashAttackTimer = 0.0f;
                    gliderBoostTimer = 0.0f;
                    if (StateMachine.State != Player.StRedDash)
                        return;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    level.Displacement.AddBurst(Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                    StateMachine.State = Player.StHitSquash;
                }
            }
        }

        private void OnCollideV(CollisionData data)
        {
            canCurveDash = false;
            if (StateMachine.State == Player.StStarFly)
            {
                if (starFlyTimer < 0.2f)
                {
                    Speed.Y = 0.0f;
                }
                else
                {
                    Play("event:/game/06_reflection/feather_state_bump");
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                    Speed.Y *= -0.5f;
                }
            }
            else if (StateMachine.State == Player.StSwim)
            {
                Speed.Y = 0.0f;
            }
            else
            {
                if (StateMachine.State == Player.StDreamDash)
                    return;
                if (data.Hit != null && data.Hit.OnDashCollide != null)
                {
                    if (DashAttacking && data.Direction.Y == (double) Math.Sign(DashDir.Y))
                    {
                        DashCollisionResults collisionResults = data.Hit.OnDashCollide(this, data.Direction);
                        if (StateMachine.State == 5)
                            collisionResults = DashCollisionResults.Ignore;
                        switch (collisionResults)
                        {
                            case DashCollisionResults.Rebound:
                                Rebound();
                                return;
                            case DashCollisionResults.Bounce:
                                ReflectBounce(new Vector2(0.0f, -Math.Sign(Speed.Y)));
                                return;
                            case DashCollisionResults.Ignore:
                                return;
                        }
                    }
                    else if (StateMachine.State == Player.StSummitLaunch)
                    {
                        int num = (int) data.Hit.OnDashCollide(this, data.Direction);
                        return;
                    }
                }
                if (Speed.Y > 0.0)
                {
                    if ((StateMachine.State == Player.StDash || StateMachine.State == Player.StRedDash) && !dashStartedOnGround)
                    {
                        if (Speed.X <= 0.01f)
                        {
                            for (int index = -1; index >= -4; --index)
                            {
                                if (!OnGround(Position + new Vector2(index, 0.0f)))
                                {
                                    MoveHExact(index);
                                    MoveVExact(1);
                                    return;
                                }
                            }
                        }
                        if (Speed.X >= -0.01f)
                        {
                            for (int index = 1; index <= 4; ++index)
                            {
                                if (!OnGround(Position + new Vector2(index, 0.0f)))
                                {
                                    MoveHExact(index);
                                    MoveVExact(1);
                                    return;
                                }
                            }
                        }
                    }
                    if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
                    {
                        StateMachine.State = 9;
                        dashAttackTimer = 0.0f;
                        gliderBoostTimer = 0.0f;
                        return;
                    }
                    if (DashDir.X != 0.0 && DashDir.Y > 0.0 && Speed.Y > 0.0)
                    {
                        DashDir.X = Math.Sign(DashDir.X);
                        DashDir.Y = 0.0f;
                        Speed.Y = 0.0f;
                        Speed.X *= 1.2f;
                        Ducking = true;
                    }
                    if (StateMachine.State != Player.StClimb)
                    {
                        float amount = Math.Min(Speed.Y / 240f, 1f);
                        Sprite.Scale.X = MathHelper.Lerp(1f, 1.6f, amount);
                        Sprite.Scale.Y = MathHelper.Lerp(1f, 0.4f, amount);
                        if (highestAirY < Y - 50.0 && Speed.Y >= 160.0 && Math.Abs(Speed.X) >= 90.0)
                            Sprite.Play("runStumble");
                        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                        Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + new Vector2(0.0f, 1f), temp));
                        int index = -1;
                        if (platformByPriority != null)
                        {
                            index = platformByPriority.GetLandSoundIndex(this);
                            if (index >= 0 && !MuffleLanding)
                                Play(playFootstepOnLand > 0.0 ? "event:/char/madeline/footstep" : "event:/char/madeline/landing", "surface_index", index);
                            if (platformByPriority is DreamBlock)
                                (platformByPriority as DreamBlock).FootstepRipple(Position);
                            MuffleLanding = false;
                        }
                        if (Speed.Y >= 80.0)
                            Dust.Burst(Position, new Vector2(0.0f, -1f).Angle(), 8, DustParticleFromSurfaceIndex(index));
                        playFootstepOnLand = 0.0f;
                    }
                }
                else
                {
                    if (Speed.Y < 0.0)
                    {
                        int num = 4;
                        if (DashAttacking && Math.Abs(Speed.X) < 0.01f)
                            num = 5;
                        if (Speed.X <= 0.01f)
                        {
                            for (int index = 1; index <= num; ++index)
                            {
                                if (!CollideCheck<Solid>(Position + new Vector2(-index, -1f)))
                                {
                                    Position += new Vector2(-index, -1f);
                                    return;
                                }
                            }
                        }
                        if (Speed.X >= -0.01f)
                        {
                            for (int x = 1; x <= num; ++x)
                            {
                                if (!CollideCheck<Solid>(Position + new Vector2(x, -1f)))
                                {
                                    Position += new Vector2(x, -1f);
                                    return;
                                }
                            }
                        }
                        if (varJumpTimer < 0.15f)
                            varJumpTimer = 0.0f;
                    }
                    if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
                    {
                        StateMachine.State = Player.StDreamDash;
                        dashAttackTimer = 0.0f;
                        gliderBoostTimer = 0.0f;
                        return;
                    }
                }
                if (data.Hit != null && data.Hit.OnCollide != null)
                    data.Hit.OnCollide(data.Direction);
                dashAttackTimer = 0.0f;
                gliderBoostTimer = 0.0f;
                Speed.Y = 0.0f;
                if (StateMachine.State != Player.StRedDash)
                    return;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                level.Displacement.AddBurst(Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                StateMachine.State = Player.StHitSquash;
            }
        }

        private bool DreamDashCheck(Vector2 dir)
        {
            if (Inventory.DreamDash && DashAttacking && (dir.X == (double) Math.Sign(DashDir.X) || dir.Y == (double) Math.Sign(DashDir.Y)))
            {
                DreamBlock dreamBlock = CollideFirst<DreamBlock>(Position + dir);
                if (dreamBlock != null)
                {
                    if (CollideCheck<Solid, DreamBlock>(Position + dir))
                    {
                        Vector2 vector2 = new(Math.Abs(dir.Y), Math.Abs(dir.X));
                        bool flag1;
                        bool flag2;
                        if (dir.X != 0.0)
                        {
                            flag1 = Speed.Y <= 0.0;
                            flag2 = Speed.Y >= 0.0;
                        }
                        else
                        {
                            flag1 = Speed.X <= 0.0;
                            flag2 = Speed.X >= 0.0;
                        }
                        if (flag1)
                        {
                            for (int index = -1; index >= -4; --index)
                            {
                                if (!CollideCheck<Solid, DreamBlock>(Position + dir + vector2 * index))
                                {
                                    Position += vector2 * index;
                                    this.dreamBlock = dreamBlock;
                                    return true;
                                }
                            }
                        }
                        if (flag2)
                        {
                            for (int index = 1; index <= 4; ++index)
                            {
                                if (!CollideCheck<Solid, DreamBlock>(Position + dir + vector2 * index))
                                {
                                    Position += vector2 * index;
                                    this.dreamBlock = dreamBlock;
                                    return true;
                                }
                            }
                        }
                        return false;
                    }
                    this.dreamBlock = dreamBlock;
                    return true;
                }
            }
            return false;
        }

        public void OnBoundsH()
        {
            Speed.X = 0.0f;
            if (StateMachine.State != 5)
                return;
            StateMachine.State = 0;
        }

        public void OnBoundsV()
        {
            Speed.Y = 0.0f;
            if (StateMachine.State != 5)
                return;
            StateMachine.State = 0;
        }

        protected override void OnSquish(CollisionData data)
        {
            bool flag = false;
            if (!Ducking && StateMachine.State != 1)
            {
                flag = true;
                Ducking = true;
                data.Pusher.Collidable = true;
                if (!CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }
                Vector2 position = Position;
                Position = data.TargetPosition;
                if (!CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }
                Position = position;
                data.Pusher.Collidable = false;
            }
            if (!TrySquishWiggle(data, wiggleY: 5))
            {
                bool evenIfInvincible = false;
                if (data.Pusher != null && data.Pusher.SquishEvenInAssistMode)
                    evenIfInvincible = true;
                Die(Vector2.Zero, evenIfInvincible);
            }
            else
            {
                if (!flag || !CanUnDuck)
                    return;
                Ducking = false;
            }
        }

        private void NormalBegin() => maxFall = 160f;

        private void NormalEnd()
        {
            wallBoostTimer = 0.0f;
            wallSpeedRetentionTimer = 0.0f;
            hopWaitX = 0;
        }

        public bool ClimbBoundsCheck(int dir) => (double) Left + dir * 2 >= level.Bounds.Left && (double) Right + dir * 2 < level.Bounds.Right;

        public void ClimbTrigger(int dir) => climbTriggerDir = dir;

        public bool ClimbCheck(int dir, int yAdd = 0) => ClimbBoundsCheck(dir) && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitY * yAdd + Vector2.UnitX * 2f * (float) Facing) && CollideCheck<Solid>(Position + new Vector2(dir * 2, yAdd));

        private int NormalUpdate()
        {
            if (LiftBoost.Y < 0.0 && wasOnGround && !onGround && Speed.Y >= 0.0)
                Speed.Y = LiftBoost.Y;
            if (Holding == null)
            {
                if (Input.GrabCheck && !IsTired && !Ducking)
                {
                    foreach (Holdable component in Scene.Tracker.GetComponents<Holdable>())
                    {
                        if (component.Check(this) && Pickup(component))
                            return 8;
                    }
                    if (Speed.Y >= 0.0 && Math.Sign(Speed.X) != -(int) Facing)
                    {
                        if (ClimbCheck((int) Facing))
                        {
                            Ducking = false;
                            if (!SaveData.Instance.Assists.NoGrabbing)
                                return 1;
                            ClimbTrigger((int) Facing);
                        }
                        if (!SaveData.Instance.Assists.NoGrabbing && (float) Input.MoveY < 1.0 && level.Wind.Y <= 0.0)
                        {
                            for (int index = 1; index <= 2; ++index)
                            {
                                if (!CollideCheck<Solid>(Position + Vector2.UnitY * -index) && ClimbCheck((int) Facing, -index))
                                {
                                    MoveVExact(-index);
                                    Ducking = false;
                                    return 1;
                                }
                            }
                        }
                    }
                }
                if (CanDash)
                {
                    Speed += LiftBoost;
                    return StartDash();
                }
                if (Ducking)
                {
                    if (onGround && (float) Input.MoveY != 1.0)
                    {
                        if (CanUnDuck)
                        {
                            Ducking = false;
                            Sprite.Scale = new Vector2(0.8f, 1.2f);
                        }
                        else if (Speed.X == 0.0)
                        {
                            for (int index = 4; index > 0; --index)
                            {
                                if (CanUnDuckAt(Position + Vector2.UnitX * index))
                                {
                                    MoveH(50f * Engine.DeltaTime);
                                    break;
                                }
                                if (CanUnDuckAt(Position - Vector2.UnitX * index))
                                {
                                    MoveH(-50f * Engine.DeltaTime);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (onGround && (float) Input.MoveY == 1.0 && Speed.Y >= 0.0)
                {
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, 0.6f);
                }
            }
            else
            {
                if (!Input.GrabCheck && minHoldTimer <= 0.0)
                    Throw();
                if (!Ducking && onGround && (float) Input.MoveY == 1.0 && Speed.Y >= 0.0 && !holdCannotDuck)
                {
                    Drop();
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, 0.6f);
                }
                else if (onGround && Ducking && Speed.Y >= 0.0)
                {
                    if (CanUnDuck)
                        Ducking = false;
                    else
                        Drop();
                }
                else if (onGround && (float) Input.MoveY != 1.0 && holdCannotDuck)
                    holdCannotDuck = false;
            }
            if (Ducking && onGround)
            {
                Speed.X = Calc.Approach(Speed.X, 0.0f, 500f * Engine.DeltaTime);
            }
            else
            {
                float groundFriction = onGround ? 1f : 0.65f;
                if (onGround && level.CoreMode == Session.CoreModes.Cold)
                    groundFriction *= 0.3f;
                if (SaveData.Instance.Assists.LowFriction && lowFrictionStopTimer <= 0.0)
                    groundFriction *= onGround ? 0.35f : 0.5f;
                float targetSpeed;
                if (Holding != null && Holding.SlowRun)
                    targetSpeed = 70f;
                else if (Holding != null && Holding.SlowFall && !onGround)
                {
                    targetSpeed = 108.000008f;
                    groundFriction *= 0.5f;
                }
                else
                    targetSpeed = 90f;
                if (level.InSpace)
                    targetSpeed *= 0.6f;
                Speed.X = Math.Abs(Speed.X) <= (double) targetSpeed || Math.Sign(Speed.X) != moveX ? Calc.Approach(Speed.X, targetSpeed * moveX, 1000f * groundFriction * Engine.DeltaTime) : Calc.Approach(Speed.X, targetSpeed * moveX, 400f * groundFriction * Engine.DeltaTime);
            }
            float target1 = 160f;
            float target2 = 240f;
            if (level.InSpace)
            {
                target1 *= 0.6f;
                target2 *= 0.6f;
            }
            if (Holding != null && Holding.SlowFall && forceMoveXTimer <= 0.0)
                maxFall = Calc.Approach(maxFall, (float) Input.GliderMoveY != 1.0 ? (!windMovedUp || (float) Input.GliderMoveY != -1.0 ? ((float) Input.GliderMoveY != -1.0 ? (!windMovedUp ? 40f : 0.0f) : 24f) : -32f) : 120f, 300f * Engine.DeltaTime);
            else if ((float) Input.MoveY == 1.0 && Speed.Y >= (double) target1)
            {
                maxFall = Calc.Approach(maxFall, target2, 300f * Engine.DeltaTime);
                float num = target1 + (float) ((target2 - (double) target1) * 0.5);
                if (Speed.Y >= (double) num)
                {
                    float amount = Math.Min(1f, (float) ((Speed.Y - (double) num) / (target2 - (double) num)));
                    Sprite.Scale.X = MathHelper.Lerp(1f, 0.5f, amount);
                    Sprite.Scale.Y = MathHelper.Lerp(1f, 1.5f, amount);
                }
            }
            else
                maxFall = Calc.Approach(maxFall, target1, 300f * Engine.DeltaTime);
            if (!onGround)
            {
                float target3 = maxFall;
                if (Holding != null && Holding.SlowFall)
                    holdCannotDuck = (float) Input.MoveY == 1.0;
                if (((Facings) moveX == Facing || moveX == 0 && Input.GrabCheck) && Input.MoveY.Value != 1)
                {
                    if (Speed.Y >= 0.0 && wallSlideTimer > 0.0 && Holding == null && ClimbBoundsCheck((int) Facing) && CollideCheck<Solid>(Position + Vector2.UnitX * (float) Facing) && !ClimbBlocker.EdgeCheck(level, this, (int) Facing) && CanUnDuck)
                    {
                        Ducking = false;
                        wallSlideDir = (int) Facing;
                    }
                    if (wallSlideDir != 0)
                    {
                        if (Input.GrabCheck)
                            ClimbTrigger(wallSlideDir);
                        if (wallSlideTimer > 0.60000002384185791 && ClimbBlocker.Check(level, this, Position + Vector2.UnitX * wallSlideDir))
                            wallSlideTimer = 0.6f;
                        target3 = MathHelper.Lerp(160f, 20f, wallSlideTimer / 1.2f);
                        if (wallSlideTimer / 1.2000000476837158 > 0.64999997615814209)
                            CreateWallSlideParticles(wallSlideDir);
                    }
                }
                float num = Math.Abs(Speed.Y) >= 40.0 || !Input.Jump.Check && !AutoJump ? 1f : 0.5f;
                if (Holding != null && Holding.SlowFall && forceMoveXTimer <= 0.0)
                    num *= 0.5f;
                if (level.InSpace)
                    num *= 0.6f;
                Speed.Y = Calc.Approach(Speed.Y, target3, 900f * num * Engine.DeltaTime);
            }
            if (varJumpTimer > 0.0)
            {
                if (AutoJump || Input.Jump.Check)
                    Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
                else
                    varJumpTimer = 0.0f;
            }
            if (Input.Jump.Pressed && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed))
            {
                if (jumpGraceTimer > 0.0)
                    Jump();
                else if (CanUnDuck)
                {
                    bool canUnDuck = CanUnDuck;
                    if (canUnDuck && WallJumpCheck(1))
                    {
                        if (Facing == Facings.Right && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * 3f))
                            ClimbJump();
                        else if (DashAttacking && SuperWallJumpAngleCheck)
                            SuperWallJump(-1);
                        else
                            WallJump(-1);
                    }
                    else if (canUnDuck && WallJumpCheck(-1))
                    {
                        if (Facing == Facings.Left && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -3f))
                            ClimbJump();
                        else if (DashAttacking && SuperWallJumpAngleCheck)
                            SuperWallJump(1);
                        else
                            WallJump(1);
                    }
                    else
                    {
                        Water water;
                        if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2f)) != null)
                        {
                            Jump();
                            water.TopSurface.DoRipple(Position, 1f);
                        }
                    }
                }
            }
            return 0;
        }

        public void CreateWallSlideParticles(int dir)
        {
            if (!Scene.OnInterval(0.01f))
                return;
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitX * dir * 4f, temp));
            if (platformByPriority != null)
                index = platformByPriority.GetWallSoundIndex(this, dir);
            ParticleType particleType = DustParticleFromSurfaceIndex(index);
            float x = particleType == ParticleTypes.Dust ? 5f : 2f;
            Vector2 center = Center;
            Dust.Burst(dir != 1 ? center + new Vector2(-x, 4f) : center + new Vector2(x, 4f), -1.57079637f, particleType: particleType);
        }

        private bool IsTired => CheckStamina < 20.0;

        private float CheckStamina => wallBoostTimer > 0.0 ? Stamina + 27.5f : Stamina;

        private void PlaySweatEffectDangerOverride(string state)
        {
            if (Stamina <= 20.0)
                sweatSprite.Play("danger");
            else
                sweatSprite.Play(state);
        }

        private void ClimbBegin()
        {
            AutoJump = false;
            Speed.X = 0.0f;
            Speed.Y *= 0.2f;
            wallSlideTimer = 1.2f;
            climbNoMoveTimer = 0.1f;
            wallBoostTimer = 0.0f;
            lastClimbMove = 0;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            for (int index = 0; index < 2 && !CollideCheck<Solid>(Position + Vector2.UnitX * (float) Facing); ++index)
                Position += Vector2.UnitX * (float) Facing;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Position + Vector2.UnitX * (float) Facing, temp));
            if (platformByPriority == null)
                return;
            Play("event:/char/madeline/grab", "surface_index", platformByPriority.GetWallSoundIndex(this, (int)Facing));
            if (platformByPriority is not DreamBlock)
                return;
            (platformByPriority as DreamBlock).FootstepRipple(Position + new Vector2((int)Facing * 3, -4f));
        }

        private void ClimbEnd()
        {
            if (conveyorLoopSfx != null)
            {
                int num1 = (int) conveyorLoopSfx.setParameterValue("end", 1f);
                int num2 = (int) conveyorLoopSfx.release();
                conveyorLoopSfx = null;
            }
            wallSpeedRetentionTimer = 0.0f;
            if (sweatSprite == null || !(sweatSprite.CurrentAnimationID != "jump"))
                return;
            sweatSprite.Play("idle");
        }

        private int ClimbUpdate()
        {
            climbNoMoveTimer -= Engine.DeltaTime;
            if (onGround)
                Stamina = 110f;
            if (Input.Jump.Pressed && (!Ducking || CanUnDuck))
            {
                if (moveX == -(int) Facing)
                    WallJump(-(int) Facing);
                else
                    ClimbJump();
                return 0;
            }
            if (CanDash)
            {
                Speed += LiftBoost;
                return StartDash();
            }
            if (!Input.GrabCheck)
            {
                Speed += LiftBoost;
                Play("event:/char/madeline/grab_letgo");
                return 0;
            }
            if (!CollideCheck<Solid>(Position + Vector2.UnitX * (float) Facing))
            {
                if (Speed.Y < 0.0)
                {
                    if (wallBoosting)
                    {
                        Speed += LiftBoost;
                        Play("event:/char/madeline/grab_letgo");
                    }
                    else
                        ClimbHop();
                }
                return 0;
            }
            WallBooster wallBooster = WallBoosterCheck();
            if (climbNoMoveTimer <= 0.0 && wallBooster != null)
            {
                wallBoosting = true;
                if (conveyorLoopSfx == null)
                    conveyorLoopSfx = Audio.Play("event:/game/09_core/conveyor_activate", Position, "end", 0.0f);
                Audio.Position(conveyorLoopSfx, Position);
                Speed.Y = Calc.Approach(Speed.Y, -160f, 600f * Engine.DeltaTime);
                LiftSpeed = Vector2.UnitY * Math.Max(Speed.Y, -80f);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }
            else
            {
                wallBoosting = false;
                if (conveyorLoopSfx != null)
                {
                    int num1 = (int) conveyorLoopSfx.setParameterValue("end", 1f);
                    int num2 = (int) conveyorLoopSfx.release();
                    conveyorLoopSfx = null;
                }
                float target = 0.0f;
                bool flag = false;
                if (climbNoMoveTimer <= 0.0)
                {
                    if (ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * (float) Facing))
                        flag = true;
                    else if (Input.MoveY.Value == -1)
                    {
                        target = -45f;
                        if (CollideCheck<Solid>(Position - Vector2.UnitY) || ClimbHopBlockedCheck() && SlipCheck(-1f))
                        {
                            if (Speed.Y < 0.0)
                                Speed.Y = 0.0f;
                            target = 0.0f;
                            flag = true;
                        }
                        else if (SlipCheck())
                        {
                            ClimbHop();
                            return 0;
                        }
                    }
                    else if (Input.MoveY.Value == 1)
                    {
                        target = 80f;
                        if (onGround)
                        {
                            if (Speed.Y > 0.0)
                                Speed.Y = 0.0f;
                            target = 0.0f;
                        }
                        else
                            CreateWallSlideParticles((int) Facing);
                    }
                    else
                        flag = true;
                }
                else
                    flag = true;
                lastClimbMove = Math.Sign(target);
                if (flag && SlipCheck())
                    target = 30f;
                Speed.Y = Calc.Approach(Speed.Y, target, 900f * Engine.DeltaTime);
            }
            if (Input.MoveY.Value != 1 && Speed.Y > 0.0 && !CollideCheck<Solid>(Position + new Vector2((float) Facing, 1f)))
                Speed.Y = 0.0f;
            if (climbNoMoveTimer <= 0.0)
            {
                if (lastClimbMove == -1)
                {
                    Stamina -= 45.4545441f * Engine.DeltaTime;
                    if (Stamina <= 20.0)
                        sweatSprite.Play("danger");
                    else if (sweatSprite.CurrentAnimationID != "climbLoop")
                        sweatSprite.Play("climb");
                    if (Scene.OnInterval(0.2f))
                        Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                }
                else
                {
                    if (lastClimbMove == 0)
                        Stamina -= 10f * Engine.DeltaTime;
                    if (!onGround)
                    {
                        PlaySweatEffectDangerOverride("still");
                        if (Scene.OnInterval(0.8f))
                            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                    }
                    else
                        PlaySweatEffectDangerOverride("idle");
                }
            }
            else
                PlaySweatEffectDangerOverride("idle");
            if (Stamina > 0.0)
                return 1;
            Speed += LiftBoost;
            return 0;
        }

        private WallBooster WallBoosterCheck()
        {
            if (ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * (float) Facing))
                return null;
            foreach (WallBooster entity in Scene.Tracker.GetEntities<WallBooster>())
            {
                if (entity.Facing == Facing && CollideCheck(entity))
                    return entity;
            }
            return null;
        }

        private void ClimbHop()
        {
            climbHopSolid = CollideFirst<Solid>(Position + Vector2.UnitX * (float) Facing);
            playFootstepOnLand = 0.5f;
            if (climbHopSolid != null)
            {
                climbHopSolidPosition = climbHopSolid.Position;
                hopWaitX = (int) Facing;
                hopWaitXSpeed = (float) Facing * 100f;
            }
            else
            {
                hopWaitX = 0;
                Speed.X = (float) Facing * 100f;
            }
            lowFrictionStopTimer = 0.15f;
            Speed.Y = Math.Min(Speed.Y, -120f);
            forceMoveX = 0;
            forceMoveXTimer = 0.2f;
            fastJump = false;
            noWindTimer = 0.3f;
            Play("event:/char/madeline/climb_ledge");
        }

        private bool SlipCheck(float addY = 0.0f)
        {
            Vector2 point = Facing != Facings.Right ? TopLeft - Vector2.UnitX + Vector2.UnitY * (4f + addY) : TopRight + Vector2.UnitY * (4f + addY);
            return !Scene.CollideCheck<Solid>(point) && !Scene.CollideCheck<Solid>(point + Vector2.UnitY * (addY - 4f));
        }

        private bool ClimbHopBlockedCheck()
        {
            foreach (Component follower in Leader.Followers)
            {
                if (follower.Entity is StrawberrySeed)
                    return true;
            }
            foreach (LedgeBlocker component in Scene.Tracker.GetComponents<LedgeBlocker>())
            {
                if (component.HopBlockCheck(this))
                    return true;
            }
            return CollideCheck<Solid>(Position - Vector2.UnitY * 6f);
        }

        private bool JumpThruBoostBlockedCheck()
        {
            foreach (LedgeBlocker component in Scene.Tracker.GetComponents<LedgeBlocker>())
            {
                if (component.JumpThruBoostCheck(this))
                    return true;
            }
            return false;
        }

        private bool DashCorrectCheck(Vector2 add)
        {
            Vector2 position = Position;
            Collider collider = Collider;
            Position += add;
            Collider = hurtbox;
            foreach (LedgeBlocker component in Scene.Tracker.GetComponents<LedgeBlocker>())
            {
                if (component.DashCorrectCheck(this))
                {
                    Position = position;
                    Collider = collider;
                    return true;
                }
            }
            Position = position;
            Collider = collider;
            return false;
        }

        public int StartDash()
        {
            wasDashB = Dashes == 2;
            Dashes = Math.Max(0, Dashes - 1);
            demoDashed = Input.CrouchDashPressed;
            Input.Dash.ConsumeBuffer();
            Input.CrouchDash.ConsumeBuffer();
            return 2;
        }

        public bool DashAttacking => dashAttackTimer > 0.0 || StateMachine.State == 5;

        public bool CanDash
        {
            get
            {
                if (!Input.CrouchDashPressed && !Input.DashPressed || dashCooldownTimer > 0.0 || Dashes <= 0 || TalkComponent.PlayerOver != null && Input.Talk.Pressed)
                    return false;
                return LastBooster == null || !LastBooster.Ch9HubTransition || !LastBooster.BoostingPlayer;
            }
        }

        public bool StartedDashing { get; private set; }

        private void CallDashEvents()
        {
            if (calledDashEvents)
                return;
            calledDashEvents = true;
            if (CurrentBooster == null)
            {
                ++SaveData.Instance.TotalDashes;
                ++level.Session.Dashes;
                //Stats.Increment(Stat.DASHES);
                bool flag = DashDir.Y < 0.0 || DashDir.Y == 0.0 && DashDir.X > 0.0;
                if (DashDir == Vector2.Zero)
                    flag = Facing == Facings.Right;
                if (flag)
                {
                    if (wasDashB)
                        Play("event:/char/madeline/dash_pink_right");
                    else
                        Play("event:/char/madeline/dash_red_right");
                }
                else if (wasDashB)
                    Play("event:/char/madeline/dash_pink_left");
                else
                    Play("event:/char/madeline/dash_red_left");
                if (SwimCheck())
                    Play("event:/char/madeline/water_dash_gen");
                foreach (DashListener component in Scene.Tracker.GetComponents<DashListener>())
                {
                    if (component.OnDash != null)
                        component.OnDash(DashDir);
                }
            }
            else
            {
                CurrentBooster.PlayerBoosted(this, DashDir);
                CurrentBooster = null;
            }
        }

        private void DashBegin()
        {
            calledDashEvents = false;
            dashStartedOnGround = onGround;
            launched = false;
            canCurveDash = true;
            if (Engine.TimeRate > 0.25)
                Celeste.Freeze(0.05f);
            dashCooldownTimer = 0.2f;
            dashRefillCooldownTimer = 0.1f;
            StartedDashing = true;
            wallSlideTimer = 1.2f;
            dashTrailTimer = 0.0f;
            dashTrailCounter = 0;
            if (!SaveData.Instance.Assists.DashAssist)
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            dashAttackTimer = 0.3f;
            gliderBoostTimer = 0.55f;
            if (SaveData.Instance.Assists.SuperDashing)
                dashAttackTimer += 0.15f;
            beforeDashSpeed = Speed;
            Speed = Vector2.Zero;
            DashDir = Vector2.Zero;
            if (!onGround && Ducking && CanUnDuck)
                Ducking = false;
            else if (!Ducking && (demoDashed || Input.MoveY.Value == 1))
                Ducking = true;
            DashAssistInit();
        }

        private void DashAssistInit()
        {
            if (!SaveData.Instance.Assists.DashAssist || demoDashed)
                return;
            Input.LastAim = Vector2.UnitX * (float) Facing;
            Engine.DashAssistFreeze = true;
            Engine.DashAssistFreezePress = false;
            PlayerDashAssist playerDashAssist = Scene.Tracker.GetEntity<PlayerDashAssist>();
            if (playerDashAssist == null)
                Scene.Add(playerDashAssist = new PlayerDashAssist());
            playerDashAssist.Direction = Input.GetAimVector(Facing).Angle();
            playerDashAssist.Scale = 0.0f;
            playerDashAssist.Offset = CurrentBooster != null || StateMachine.PreviousState == 5 ? new Vector2(0.0f, -4f) : Vector2.Zero;
        }

        private void DashEnd()
        {
            CallDashEvents();
            demoDashed = false;
        }

        private int DashUpdate()
        {
            StartedDashing = false;
            if (dashTrailTimer > 0.0)
            {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0.0)
                {
                    CreateTrail();
                    --dashTrailCounter;
                    if (dashTrailCounter > 0)
                        dashTrailTimer = 0.1f;
                }
            }
            if (SaveData.Instance.Assists.SuperDashing && canCurveDash && Input.Aim.Value != Vector2.Zero && Speed != Vector2.Zero)
            {
                Vector2 vector = CorrectDashPrecision(Input.GetAimVector());
                float num = Vector2.Dot(vector, Speed.SafeNormalize());
                if ((double) num is >= (-0.10000000149011612) and < 0.99000000953674316)
                {
                    Speed = Speed.RotateTowards(vector.Angle(), 4.18879032f * Engine.DeltaTime);
                    DashDir = Speed.SafeNormalize();
                    DashDir = CorrectDashPrecision(DashDir);
                }
            }
            if (SaveData.Instance.Assists.SuperDashing && CanDash)
            {
                StartDash();
                StateMachine.ForceState(2);
                return 2;
            }
            if (Holding == null && DashDir != Vector2.Zero && Input.GrabCheck && !IsTired && CanUnDuck)
            {
                foreach (Holdable component in Scene.Tracker.GetComponents<Holdable>())
                {
                    if (component.Check(this) && Pickup(component))
                        return 8;
                }
            }
            if (Math.Abs(DashDir.Y) < 0.10000000149011612)
            {
                foreach (JumpThru entity in Scene.Tracker.GetEntities<JumpThru>())
                {
                    if (CollideCheck(entity) && Bottom - (double) entity.Top <= 6.0 && !DashCorrectCheck(Vector2.UnitY * (entity.Top - Bottom)))
                        MoveVExact((int) (entity.Top - (double) Bottom));
                }
                if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0.0)
                {
                    SuperJump();
                    return 0;
                }
            }
            if (SuperWallJumpAngleCheck)
            {
                if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        SuperWallJump(-1);
                        return 0;
                    }
                    if (WallJumpCheck(-1))
                    {
                        SuperWallJump(1);
                        return 0;
                    }
                }
            }
            else if (Input.Jump.Pressed && CanUnDuck)
            {
                if (WallJumpCheck(1))
                {
                    if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * 3f))
                        ClimbJump();
                    else
                        WallJump(-1);
                    return 0;
                }
                if (WallJumpCheck(-1))
                {
                    if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -3f))
                        ClimbJump();
                    else
                        WallJump(1);
                    return 0;
                }
            }
            if (Speed != Vector2.Zero && level.OnInterval(0.02f))
                level.ParticlesFG.Emit(wasDashB ? (Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline ? Player.P_DashB : Player.P_DashBadB) : Player.P_DashA, Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), DashDir.Angle());
            return 2;
        }

        private bool SuperWallJumpAngleCheck => Math.Abs(DashDir.X) <= 0.20000000298023224 && DashDir.Y <= -0.75;

        private Vector2 CorrectDashPrecision(Vector2 dir)
        {
            if (dir.X != 0.0 && Math.Abs(dir.X) < 1.0 / 1000.0)
            {
                dir.X = 0.0f;
                dir.Y = Math.Sign(dir.Y);
            }
            else if (dir.Y != 0.0 && Math.Abs(dir.Y) < 1.0 / 1000.0)
            {
                dir.Y = 0.0f;
                dir.X = Math.Sign(dir.X);
            }
            return dir;
        }

        private IEnumerator DashCoroutine()
        {
            Player player = this;
            yield return null;
            if (SaveData.Instance.Assists.DashAssist)
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            player.level.Displacement.AddBurst(player.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
            Vector2 lastAim = player.lastAim;
            if (player.OverrideDashDirection.HasValue)
                lastAim = player.OverrideDashDirection.Value;
            Vector2 vector2_1 = player.CorrectDashPrecision(lastAim);
            Vector2 vector2_2 = vector2_1 * 240f;
            if (Math.Sign(player.beforeDashSpeed.X) == Math.Sign(vector2_2.X) && Math.Abs(player.beforeDashSpeed.X) > (double) Math.Abs(vector2_2.X))
                vector2_2.X = player.beforeDashSpeed.X;
            player.Speed = vector2_2;
            if (player.CollideCheck<Water>())
                player.Speed *= 0.75f;
            player.gliderBoostDir = player.DashDir = vector2_1;
            player.SceneAs<Level>().DirectionalShake(player.DashDir, 0.2f);
            if (player.DashDir.X != 0.0)
                player.Facing = (Facings) Math.Sign(player.DashDir.X);
            player.CallDashEvents();
            if (player.StateMachine.PreviousState == 19)
                player.level.Particles.Emit(FlyFeather.P_Boost, 12, player.Center, Vector2.One * 4f, (-vector2_1).Angle());
            if (player.onGround && player.DashDir.X != 0.0 && player.DashDir.Y > 0.0 && player.Speed.Y > 0.0 && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position + Vector2.UnitY)))
            {
                player.DashDir.X = Math.Sign(player.DashDir.X);
                player.DashDir.Y = 0.0f;
                player.Speed.Y = 0.0f;
                player.Speed.X *= 1.2f;
                player.Ducking = true;
            }
            SlashFx.Burst(player.Center, player.DashDir.Angle());
            player.CreateTrail();
            if (SaveData.Instance.Assists.SuperDashing)
            {
                player.dashTrailTimer = 0.1f;
                player.dashTrailCounter = 2;
            }
            else
            {
                player.dashTrailTimer = 0.08f;
                player.dashTrailCounter = 1;
            }
            if (player.DashDir.X != 0.0 && Input.GrabCheck)
            {
                SwapBlock swapBlock = player.CollideFirst<SwapBlock>(player.Position + Vector2.UnitX * Math.Sign(player.DashDir.X));
                if (swapBlock != null && swapBlock.Direction.X == (double) Math.Sign(player.DashDir.X))
                {
                    player.StateMachine.State = 1;
                    player.Speed = Vector2.Zero;
                    yield break;
                }
            }
            Vector2 swapCancel = Vector2.One;
            foreach (SwapBlock entity in player.Scene.Tracker.GetEntities<SwapBlock>())
            {
                if (player.CollideCheck(entity, player.Position + Vector2.UnitY) && entity != null && entity.Swapping)
                {
                    if (player.DashDir.X != 0.0 && entity.Direction.X == (double) Math.Sign(player.DashDir.X))
                        player.Speed.X = swapCancel.X = 0.0f;
                    if (player.DashDir.Y != 0.0 && entity.Direction.Y == (double) Math.Sign(player.DashDir.Y))
                        player.Speed.Y = swapCancel.Y = 0.0f;
                }
            }
            if (SaveData.Instance.Assists.SuperDashing)
                yield return 0.3f;
            else
                yield return 0.15f;
            player.CreateTrail();
            player.AutoJump = true;
            player.AutoJumpTimer = 0.0f;
            if (player.DashDir.Y <= 0.0)
            {
                player.Speed = player.DashDir * 160f;
                player.Speed.X *= swapCancel.X;
                player.Speed.Y *= swapCancel.Y;
            }
            if (player.Speed.Y < 0.0)
                player.Speed.Y *= 0.75f;
            player.StateMachine.State = 0;
        }

        private bool SwimCheck() => CollideCheck<Water>(Position + Vector2.UnitY * -8f) && CollideCheck<Water>(Position);

        private bool SwimUnderwaterCheck() => CollideCheck<Water>(Position + Vector2.UnitY * -9f);

        private bool SwimJumpCheck() => !CollideCheck<Water>(Position + Vector2.UnitY * -14f);

        private bool SwimRiseCheck() => !CollideCheck<Water>(Position + Vector2.UnitY * -18f);

        private bool UnderwaterMusicCheck() => CollideCheck<Water>(Position) && CollideCheck<Water>(Position + Vector2.UnitY * -12f);

        private void SwimBegin()
        {
            if (Speed.Y > 0.0)
                Speed.Y *= 0.5f;
            Stamina = 110f;
        }

        private int SwimUpdate()
        {
            if (!SwimCheck())
                return 0;
            if (CanUnDuck)
                Ducking = false;
            if (CanDash)
            {
                demoDashed = Input.CrouchDashPressed;
                Input.Dash.ConsumeBuffer();
                Input.CrouchDash.ConsumeBuffer();
                return 2;
            }
            bool flag = SwimUnderwaterCheck();
            if (!flag && Speed.Y >= 0.0 && Input.GrabCheck && !IsTired && CanUnDuck && Math.Sign(Speed.X) != -(int) Facing && ClimbCheck((int) Facing))
            {
                if (SaveData.Instance.Assists.NoGrabbing)
                    ClimbTrigger((int) Facing);
                else if (!MoveVExact(-1))
                {
                    Ducking = false;
                    return 1;
                }
            }
            Vector2 vector2 = Input.Feather.Value.SafeNormalize();
            float num1 = flag ? 60f : 80f;
            float num2 = 80f;
            Speed.X = Math.Abs(Speed.X) <= 80.0 || Math.Sign(Speed.X) != Math.Sign(vector2.X) ? Calc.Approach(Speed.X, num1 * vector2.X, 600f * Engine.DeltaTime) : Calc.Approach(Speed.X, num1 * vector2.X, 400f * Engine.DeltaTime);
            if (vector2.Y == 0.0 && SwimRiseCheck())
                Speed.Y = Calc.Approach(Speed.Y, -60f, 600f * Engine.DeltaTime);
            else if (vector2.Y >= 0.0 || SwimUnderwaterCheck())
                Speed.Y = Math.Abs(Speed.Y) <= 80.0 || Math.Sign(Speed.Y) != Math.Sign(vector2.Y) ? Calc.Approach(Speed.Y, num2 * vector2.Y, 600f * Engine.DeltaTime) : Calc.Approach(Speed.Y, num2 * vector2.Y, 400f * Engine.DeltaTime);
            if (!flag && moveX != 0 && CollideCheck<Solid>(Position + Vector2.UnitX * moveX) && !CollideCheck<Solid>(Position + new Vector2(moveX, -3f)))
                ClimbHop();
            if (!Input.Jump.Pressed || !SwimJumpCheck())
                return 3;
            Jump();
            return 0;
        }

        public void Boost(Booster booster)
        {
            StateMachine.State = 4;
            Speed = Vector2.Zero;
            boostTarget = booster.Center;
            boostRed = false;
            LastBooster = CurrentBooster = booster;
        }

        public void RedBoost(Booster booster)
        {
            StateMachine.State = 4;
            Speed = Vector2.Zero;
            boostTarget = booster.Center;
            boostRed = true;
            LastBooster = CurrentBooster = booster;
        }

        private void BoostBegin()
        {
            RefillDash();
            RefillStamina();
            if (Holding == null)
                return;
            Drop();
        }

        private void BoostEnd()
        {
            Vector2 vector2 = (boostTarget - Collider.Center).Floor();
            MoveToX(vector2.X);
            MoveToY(vector2.Y);
        }

        private int BoostUpdate()
        {
            Vector2 vector2 = Calc.Approach(ExactPosition, boostTarget - Collider.Center + Input.Aim.Value * 3f, 80f * Engine.DeltaTime);
            MoveToX(vector2.X);
            MoveToY(vector2.Y);
            if (!Input.DashPressed && !Input.CrouchDashPressed)
                return 4;
            demoDashed = Input.CrouchDashPressed;
            Input.Dash.ConsumePress();
            Input.CrouchDash.ConsumeBuffer();
            return boostRed ? 5 : 2;
        }

        private IEnumerator BoostCoroutine()
        {
            yield return 0.25f;
            StateMachine.State = !boostRed ? 2 : 5;
        }

        private void RedDashBegin()
        {
            calledDashEvents = false;
            dashStartedOnGround = false;
            Celeste.Freeze(0.05f);
            Dust.Burst(Position, (-DashDir).Angle(), 8);
            dashCooldownTimer = 0.2f;
            dashRefillCooldownTimer = 0.1f;
            StartedDashing = true;
            level.Displacement.AddBurst(Center, 0.5f, 0.0f, 80f, 0.666f, Ease.QuadOut, Ease.QuadOut);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            dashAttackTimer = 0.3f;
            gliderBoostTimer = 0.55f;
            DashDir = Speed = Vector2.Zero;
            if (!onGround && CanUnDuck)
                Ducking = false;
            DashAssistInit();
        }

        private void RedDashEnd() => CallDashEvents();

        private int RedDashUpdate()
        {
            StartedDashing = false;
            bool flag = LastBooster != null && LastBooster.Ch9HubTransition;
            gliderBoostTimer = 0.05f;
            if (CanDash)
                return StartDash();
            if (DashDir.Y == 0.0)
            {
                foreach (JumpThru entity in Scene.Tracker.GetEntities<JumpThru>())
                {
                    if (CollideCheck(entity) && Bottom - (double) entity.Top <= 6.0)
                        MoveVExact((int) (entity.Top - (double) Bottom));
                }
                if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0.0 && !flag)
                {
                    SuperJump();
                    return 0;
                }
            }
            if (!flag)
            {
                if (SuperWallJumpAngleCheck)
                {
                    if (Input.Jump.Pressed && CanUnDuck)
                    {
                        if (WallJumpCheck(1))
                        {
                            SuperWallJump(-1);
                            return 0;
                        }
                        if (WallJumpCheck(-1))
                        {
                            SuperWallJump(1);
                            return 0;
                        }
                    }
                }
                else if (Input.Jump.Pressed && CanUnDuck)
                {
                    if (WallJumpCheck(1))
                    {
                        if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * 3f))
                            ClimbJump();
                        else
                            WallJump(-1);
                        return 0;
                    }
                    if (WallJumpCheck(-1))
                    {
                        if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -3f))
                            ClimbJump();
                        else
                            WallJump(1);
                        return 0;
                    }
                }
            }
            return 5;
        }

        private IEnumerator RedDashCoroutine()
        {
            Player player = this;
            yield return null;
            player.Speed = player.CorrectDashPrecision(player.lastAim) * 240f;
            player.gliderBoostDir = player.DashDir = player.lastAim;
            player.SceneAs<Level>().DirectionalShake(player.DashDir, 0.2f);
            if (player.DashDir.X != 0.0)
                player.Facing = (Facings) Math.Sign(player.DashDir.X);
            player.CallDashEvents();
        }

        private void HitSquashBegin() => hitSquashNoMoveTimer = 0.1f;

        private int HitSquashUpdate()
        {
            Speed.X = Calc.Approach(Speed.X, 0.0f, 800f * Engine.DeltaTime);
            Speed.Y = Calc.Approach(Speed.Y, 0.0f, 800f * Engine.DeltaTime);
            if (Input.Jump.Pressed)
            {
                if (onGround)
                    Jump();
                else if (WallJumpCheck(1))
                {
                    if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * 3f))
                        ClimbJump();
                    else
                        WallJump(-1);
                }
                else if (WallJumpCheck(-1))
                {
                    if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0.0 && Holding == null && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitX * -3f))
                        ClimbJump();
                    else
                        WallJump(1);
                }
                else
                    Input.Jump.ConsumeBuffer();
                return 0;
            }
            if (CanDash)
                return StartDash();
            if (Input.GrabCheck && ClimbCheck((int) Facing))
                return 1;
            if (hitSquashNoMoveTimer <= 0.0)
                return 0;
            hitSquashNoMoveTimer -= Engine.DeltaTime;
            return 6;
        }

        public Vector2 ExplodeLaunch(Vector2 from, bool snapUp = true, bool sidesOnly = false)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            launchApproachX = new float?();
            Vector2 vector2 = (Center - from).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector2, Vector2.UnitY);
            if (snapUp && num <= -0.699999988079071)
            {
                vector2.X = 0.0f;
                vector2.Y = -1f;
            }
            else if ((double) num is <= 0.64999997615814209 and >= (-0.550000011920929))
            {
                vector2.Y = 0.0f;
                vector2.X = Math.Sign(vector2.X);
            }
            if (sidesOnly && vector2.X != 0.0)
            {
                vector2.Y = 0.0f;
                vector2.X = Math.Sign(vector2.X);
            }
            Speed = 280f * vector2;
            if (Speed.Y <= 50.0)
            {
                Speed.Y = Math.Min(-150f, Speed.Y);
                AutoJump = true;
            }
            if (Speed.X != 0.0)
            {
                if (Input.MoveX.Value == Math.Sign(Speed.X))
                {
                    explodeLaunchBoostTimer = 0.0f;
                    Speed.X *= 1.2f;
                }
                else
                {
                    explodeLaunchBoostTimer = 0.01f;
                    explodeLaunchBoostSpeed = Speed.X * 1.2f;
                }
            }
            SlashFx.Burst(Center, Speed.Angle());
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            dashCooldownTimer = 0.2f;
            StateMachine.State = 7;
            return vector2;
        }

        public void FinalBossPushLaunch(int dir)
        {
            launchApproachX = new float?();
            Speed.X = (float) (0.89999997615814209 * dir * 280.0);
            Speed.Y = -150f;
            AutoJump = true;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SlashFx.Burst(Center, Speed.Angle());
            RefillDash();
            RefillStamina();
            dashCooldownTimer = 0.28f;
            StateMachine.State = 7;
        }

        public void BadelineBoostLaunch(float atX)
        {
            launchApproachX = atX;
            Speed.X = 0.0f;
            Speed.Y = -330f;
            AutoJump = true;
            if (Holding != null)
                Drop();
            SlashFx.Burst(Center, Speed.Angle());
            RefillDash();
            RefillStamina();
            dashCooldownTimer = 0.2f;
            StateMachine.State = 7;
        }

        private void LaunchBegin() => launched = true;

        private int LaunchUpdate()
        {
            if (launchApproachX.HasValue)
                MoveTowardsX(launchApproachX.Value, 60f * Engine.DeltaTime);
            if (CanDash)
                return StartDash();
            if (Input.GrabCheck && !IsTired && !Ducking)
            {
                foreach (Holdable component in Scene.Tracker.GetComponents<Holdable>())
                {
                    if (component.Check(this) && Pickup(component))
                        return 8;
                }
            }
            Speed.Y = Speed.Y >= 0.0 ? Calc.Approach(Speed.Y, 160f, 225f * Engine.DeltaTime) : Calc.Approach(Speed.Y, 160f, 450f * Engine.DeltaTime);
            Speed.X = Calc.Approach(Speed.X, 0.0f, 200f * Engine.DeltaTime);
            return Speed.Length() < 220.0 ? 0 : 7;
        }

        public void SummitLaunch(float targetX)
        {
            summitLaunchTargetX = targetX;
            StateMachine.State = 10;
        }

        private void SummitLaunchBegin()
        {
            wallBoostTimer = 0.0f;
            Sprite.Play("launch");
            Speed = -Vector2.UnitY * 240f;
            summitLaunchParticleTimer = 0.4f;
        }

        private int SummitLaunchUpdate()
        {
            summitLaunchParticleTimer -= Engine.DeltaTime;
            if (summitLaunchParticleTimer > 0.0 && Scene.OnInterval(0.03f))
                level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, Center, Vector2.One * 4f);
            Facing = Facings.Right;
            MoveTowardsX(summitLaunchTargetX, 20f * Engine.DeltaTime);
            Speed = -Vector2.UnitY * 240f;
            if (level.OnInterval(0.2f))
                level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, 1.57079637f, Color.White));
            CrystalStaticSpinner crystalStaticSpinner = Scene.CollideFirst<CrystalStaticSpinner>(new Rectangle((int) (X - 4.0), (int) (Y - 40.0), 8, 12));
            if (crystalStaticSpinner != null)
            {
                crystalStaticSpinner.Destroy();
                level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                Celeste.Freeze(0.01f);
            }
            return 10;
        }

        public void StopSummitLaunch()
        {
            StateMachine.State = 0;
            Speed.Y = -140f;
            AutoJump = true;
            varJumpSpeed = Speed.Y;
        }

        private IEnumerator PickupCoroutine()
        {
            Player player = this;
            player.Play("event:/char/madeline/crystaltheo_lift");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            if (player.Holding != null && player.Holding.SlowFall && (player.gliderBoostTimer - 0.15999999642372131 > 0.0 && player.gliderBoostDir.Y < 0.0 || player.Speed.Length() > 180.0 && player.Speed.Y <= 0.0))
                Audio.Play("event:/new_content/game/10_farewell/glider_platform_dissipate", player.Position);
            Vector2 oldSpeed = player.Speed;
            float varJump = player.varJumpTimer;
            player.Speed = Vector2.Zero;
            Vector2 begin = player.Holding.Entity.Position - player.Position;
            Vector2 carryOffsetTarget = Player.CarryOffsetTarget;
            Vector2 control = new(begin.X + Math.Sign(begin.X) * 2, Player.CarryOffsetTarget.Y - 2f);
            SimpleCurve curve = new(begin, carryOffsetTarget, control);
            player.carryOffset = begin;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.16f, true);
            tween.OnUpdate = t => carryOffset = curve.GetPoint(t.Eased);
            player.Add(tween);
            yield return tween.Wait();
            player.Speed = oldSpeed;
            player.Speed.Y = Math.Min(player.Speed.Y, 0.0f);
            player.varJumpTimer = varJump;
            player.StateMachine.State = 0;
            if (player.Holding != null && player.Holding.SlowFall)
            {
                if (player.gliderBoostTimer > 0.0 && player.gliderBoostDir.Y < 0.0)
                {
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    player.gliderBoostTimer = 0.0f;
                    player.Speed.Y = Math.Min(player.Speed.Y, -240f * Math.Abs(player.gliderBoostDir.Y));
                }
                else if (player.Speed.Y < 0.0)
                    player.Speed.Y = Math.Min(player.Speed.Y, -105f);
                if (player.onGround && (float) Input.MoveY == 1.0)
                    player.holdCannotDuck = true;
            }
        }

        private void DreamDashBegin()
        {
            if (dreamSfxLoop == null)
                Add(dreamSfxLoop = new SoundSource());
            Speed = DashDir * 240f;
            TreatNaive = true;
            Depth = -12000;
            dreamDashCanEndTimer = 0.1f;
            Stamina = 110f;
            dreamJump = false;
            Play("event:/char/madeline/dreamblock_enter");
            Loop(dreamSfxLoop, "event:/char/madeline/dreamblock_travel");
        }

        private void DreamDashEnd()
        {
            Depth = 0;
            if (!dreamJump)
            {
                AutoJump = true;
                AutoJumpTimer = 0.0f;
            }
            if (!Inventory.NoRefills)
                RefillDash();
            RefillStamina();
            TreatNaive = false;
            if (dreamBlock != null)
            {
                if (DashDir.X != 0.0)
                {
                    jumpGraceTimer = 0.1f;
                    dreamJump = true;
                }
                else
                    jumpGraceTimer = 0.0f;
                dreamBlock.OnPlayerExit(this);
                dreamBlock = null;
            }
            Stop(dreamSfxLoop);
            Play("event:/char/madeline/dreamblock_exit");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
        }

        private int DreamDashUpdate()
        {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Vector2 position = Position;
            NaiveMove(Speed * Engine.DeltaTime);
            if (dreamDashCanEndTimer > 0.0)
                dreamDashCanEndTimer -= Engine.DeltaTime;
            DreamBlock dreamBlock = CollideFirst<DreamBlock>();
            if (dreamBlock == null)
            {
                if (DreamDashedIntoSolid())
                {
                    if (SaveData.Instance.Assists.Invincible)
                    {
                        Position = position;
                        Speed *= -1f;
                        Play("event:/game/general/assist_dreamblockbounce");
                    }
                    else
                        Die(Vector2.Zero);
                }
                else if (dreamDashCanEndTimer <= 0.0)
                {
                    Celeste.Freeze(0.05f);
                    if (Input.Jump.Pressed && DashDir.X != 0.0)
                    {
                        dreamJump = true;
                        Jump();
                    }
                    else if (DashDir.Y >= 0.0 || DashDir.X != 0.0)
                    {
                        if (DashDir.X > 0.0 && CollideCheck<Solid>(Position - Vector2.UnitX * 5f))
                            MoveHExact(-5);
                        else if (DashDir.X < 0.0 && CollideCheck<Solid>(Position + Vector2.UnitX * 5f))
                            MoveHExact(5);
                        bool flag1 = ClimbCheck(-1);
                        bool flag2 = ClimbCheck(1);
                        if (Input.GrabCheck && (moveX == 1 & flag2 || moveX == -1 & flag1))
                        {
                            Facing = (Facings) moveX;
                            if (!SaveData.Instance.Assists.NoGrabbing)
                                return 1;
                            ClimbTrigger(moveX);
                            Speed.X = 0.0f;
                        }
                    }
                    return 0;
                }
            }
            else
            {
                this.dreamBlock = dreamBlock;
                if (Scene.OnInterval(0.1f))
                    CreateTrail();
                if (level.OnInterval(0.04f))
                {
                    DisplacementRenderer.Burst burst = level.Displacement.AddBurst(Center, 0.3f, 0.0f, 40f);
                    burst.WorldClipCollider = this.dreamBlock.Collider;
                    burst.WorldClipPadding = 2;
                }
            }
            return 9;
        }

        private bool DreamDashedIntoSolid()
        {
            if (!CollideCheck<Solid>())
                return false;
            for (int index1 = 1; index1 <= 5; ++index1)
            {
                for (int index2 = -1; index2 <= 1; index2 += 2)
                {
                    for (int index3 = 1; index3 <= 5; ++index3)
                    {
                        for (int index4 = -1; index4 <= 1; index4 += 2)
                        {
                            Vector2 vector2 = new(index1 * index2, index3 * index4);
                            if (!CollideCheck<Solid>(Position + vector2))
                            {
                                Position += vector2;
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool StartStarFly()
        {
            RefillStamina();
            if (StateMachine.State == 18)
                return false;
            if (StateMachine.State == 19)
            {
                starFlyTimer = 2f;
                Sprite.Color = starFlyColor;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
                StateMachine.State = 19;
            return true;
        }

        private void StarFlyBegin()
        {
            Sprite.Play("startStarFly");
            starFlyTransforming = true;
            starFlyTimer = 2f;
            starFlySpeedLerp = 0.0f;
            jumpGraceTimer = 0.0f;
            if (starFlyBloom == null)
                Add(starFlyBloom = new BloomPoint(new Vector2(0.0f, -6f), 0.0f, 16f));
            starFlyBloom.Visible = true;
            starFlyBloom.Alpha = 0.0f;
            Collider = starFlyHitbox;
            hurtbox = starFlyHurtbox;
            if (starFlyLoopSfx == null)
            {
                Add(starFlyLoopSfx = new SoundSource());
                starFlyLoopSfx.DisposeOnTransition = false;
                Add(starFlyWarningSfx = new SoundSource());
                starFlyWarningSfx.DisposeOnTransition = false;
            }
            starFlyLoopSfx.Play("event:/game/06_reflection/feather_state_loop", "feather_speed", 1f);
            starFlyWarningSfx.Stop();
        }

        private void StarFlyEnd()
        {
            Play("event:/game/06_reflection/feather_state_end");
            starFlyWarningSfx.Stop();
            starFlyLoopSfx.Stop();
            Hair.DrawPlayerSpriteOutline = false;
            Sprite.Color = Color.White;
            level.Displacement.AddBurst(Center, 0.25f, 8f, 32f);
            starFlyBloom.Visible = false;
            Sprite.HairCount = startHairCount;
            StarFlyReturnToNormalHitbox();
            if (StateMachine.State == 2)
                return;
            level.Particles.Emit(FlyFeather.P_Boost, 12, Center, Vector2.One * 4f, (-Speed).Angle());
        }

        private void StarFlyReturnToNormalHitbox()
        {
            Collider = normalHitbox;
            hurtbox = normalHurtbox;
            if (!CollideCheck<Solid>())
                return;
            Vector2 position = Position;
            Y -= normalHitbox.Bottom - starFlyHitbox.Bottom;
            if (!CollideCheck<Solid>())
                return;
            Position = position;
            Ducking = true;
            Y -= duckHitbox.Bottom - starFlyHitbox.Bottom;
            if (CollideCheck<Solid>())
            {
                Position = position;
                throw new Exception("Could not get out of solids when exiting Star Fly State!");
            }
        }

        private IEnumerator StarFlyCoroutine()
        {
            Player player = this;
            while (player.Sprite.CurrentAnimationID == "startStarFly")
                yield return null;
            while (player.Speed != Vector2.Zero)
                yield return null;
            yield return 0.1f;
            player.Sprite.Color = player.starFlyColor;
            player.Sprite.HairCount = 7;
            player.Hair.DrawPlayerSpriteOutline = true;
            player.level.Displacement.AddBurst(player.Center, 0.25f, 8f, 32f);
            player.starFlyTransforming = false;
            player.starFlyTimer = 2f;
            player.RefillDash();
            player.RefillStamina();
            Vector2 vector2 = Input.Feather.Value;
            if (vector2 == Vector2.Zero)
                vector2 = Vector2.UnitX * (float) player.Facing;
            player.Speed = vector2 * 250f;
            player.starFlyLastDir = vector2;
            player.level.Particles.Emit(FlyFeather.P_Boost, 12, player.Center, Vector2.One * 4f, (-vector2).Angle());
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            player.level.DirectionalShake(player.starFlyLastDir);
            while (player.starFlyTimer > 0.5)
                yield return null;
            player.starFlyWarningSfx.Play("event:/game/06_reflection/feather_state_warning");
        }

        private int StarFlyUpdate()
        {
            starFlyBloom.Alpha = Calc.Approach(starFlyBloom.Alpha, 0.7f, Engine.DeltaTime * 2f);
            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
            if (starFlyTransforming)
            {
                Speed = Calc.Approach(Speed, Vector2.Zero, 1000f * Engine.DeltaTime);
            }
            else
            {
                Vector2 starFlyLastDir = Input.Feather.Value;
                bool flag1 = false;
                if (starFlyLastDir == Vector2.Zero)
                {
                    flag1 = true;
                    starFlyLastDir = this.starFlyLastDir;
                }
                Vector2 vec = Speed.SafeNormalize(Vector2.Zero);
                Vector2 vector2 = !(vec == Vector2.Zero) ? vec.RotateTowards(starFlyLastDir.Angle(), 5.58505344f * Engine.DeltaTime) : starFlyLastDir;
                this.starFlyLastDir = vector2;
                float target;
                if (flag1)
                {
                    starFlySpeedLerp = 0.0f;
                    target = 91f;
                }
                else if (vector2 != Vector2.Zero && Vector2.Dot(vector2, starFlyLastDir) >= 0.44999998807907104)
                {
                    starFlySpeedLerp = Calc.Approach(starFlySpeedLerp, 1f, Engine.DeltaTime / 1f);
                    target = MathHelper.Lerp(140f, 190f, starFlySpeedLerp);
                }
                else
                {
                    starFlySpeedLerp = 0.0f;
                    target = 140f;
                }
                starFlyLoopSfx.Param("feather_speed", flag1 ? 0.0f : 1f);
                float num = Calc.Approach(Speed.Length(), target, 1000f * Engine.DeltaTime);
                Speed = vector2 * num;
                if (level.OnInterval(0.02f))
                    level.Particles.Emit(FlyFeather.P_Flying, 1, Center, Vector2.One * 2f, (-Speed).Angle());
                if (Input.Jump.Pressed)
                {
                    if (OnGround(3))
                    {
                        Jump();
                        return 0;
                    }
                    if (WallJumpCheck(-1))
                    {
                        WallJump(1);
                        return 0;
                    }
                    if (WallJumpCheck(1))
                    {
                        WallJump(-1);
                        return 0;
                    }
                }
                if (Input.GrabCheck)
                {
                    bool flag2 = false;
                    int dir = 0;
                    if (Input.MoveX.Value != -1 && ClimbCheck(1))
                    {
                        Facing = Facings.Right;
                        dir = 1;
                        flag2 = true;
                    }
                    else if (Input.MoveX.Value != 1 && ClimbCheck(-1))
                    {
                        Facing = Facings.Left;
                        dir = -1;
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        if (!SaveData.Instance.Assists.NoGrabbing)
                            return 1;
                        Speed = Vector2.Zero;
                        ClimbTrigger(dir);
                        return 0;
                    }
                }
                if (CanDash)
                    return StartDash();
                starFlyTimer -= Engine.DeltaTime;
                if (starFlyTimer <= 0.0)
                {
                    if (Input.MoveY.Value == -1)
                        Speed.Y = -100f;
                    if (Input.MoveY.Value < 1)
                    {
                        varJumpSpeed = Speed.Y;
                        AutoJump = true;
                        AutoJumpTimer = 0.0f;
                        varJumpTimer = 0.2f;
                    }
                    if (Speed.Y > 0.0)
                        Speed.Y = 0.0f;
                    if (Math.Abs(Speed.X) > 140.0)
                        Speed.X = 140f * Math.Sign(Speed.X);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    return 0;
                }
                if (starFlyTimer < 0.5 && Scene.OnInterval(0.05f))
                {
                    if (Sprite.Color == starFlyColor)
                        Sprite.Color = Player.NormalHairColor;
                    else
                        Sprite.Color = starFlyColor;
                }
            }
            return 19;
        }

        public bool DoFlingBird(FlingBird bird)
        {
            if (Dead || StateMachine.State == 24)
                return false;
            flingBird = bird;
            StateMachine.State = 24;
            if (Holding != null)
                Drop();
            return true;
        }

        public void FinishFlingBird()
        {
            StateMachine.State = 0;
            AutoJump = true;
            forceMoveX = 1;
            forceMoveXTimer = 0.2f;
            Speed = FlingBird.FlingSpeed;
            varJumpTimer = 0.2f;
            varJumpSpeed = Speed.Y;
            launched = true;
        }

        private void FlingBirdBegin()
        {
            RefillDash();
            RefillStamina();
        }

        private void FlingBirdEnd()
        {
        }

        private int FlingBirdUpdate()
        {
            MoveTowardsX(flingBird.X, 250f * Engine.DeltaTime);
            MoveTowardsY(flingBird.Y + 8f + Collider.Height, 250f * Engine.DeltaTime);
            return 24;
        }

        private IEnumerator FlingBirdCoroutine()
        {
            yield break;
        }

        public void StartCassetteFly(Vector2 targetPosition, Vector2 control)
        {
            StateMachine.State = 21;
            cassetteFlyCurve = new SimpleCurve(Position, targetPosition, control);
            cassetteFlyLerp = 0.0f;
            Speed = Vector2.Zero;
            if (Holding == null)
                return;
            Drop();
        }

        private void CassetteFlyBegin()
        {
            Sprite.Play("bubble");
            Sprite.Y += 5f;
        }

        private void CassetteFlyEnd()
        {
        }

        private int CassetteFlyUpdate() => 21;

        private IEnumerator CassetteFlyCoroutine()
        {
            Player player = this;
            player.level.CanRetry = false;
            player.level.FormationBackdrop.Display = true;
            player.level.FormationBackdrop.Alpha = 0.5f;
            player.Sprite.Scale = Vector2.One * 1.25f;
            player.Depth = -2000000;
            yield return 0.4f;
            while (player.cassetteFlyLerp < 1.0)
            {
                if (player.level.OnInterval(0.03f))
                    player.level.Particles.Emit(Player.P_CassetteFly, 2, player.Center, Vector2.One * 4f);
                player.cassetteFlyLerp = Calc.Approach(player.cassetteFlyLerp, 1f, 1.6f * Engine.DeltaTime);
                player.Position = player.cassetteFlyCurve.GetPoint(Ease.SineInOut(player.cassetteFlyLerp));
                player.level.Camera.Position = player.CameraTarget;
                yield return null;
            }
            player.Position = player.cassetteFlyCurve.End;
            player.Sprite.Scale = Vector2.One * 1.25f;
            player.Sprite.Y -= 5f;
            player.Sprite.Play("fallFast");
            yield return 0.2f;
            player.level.CanRetry = true;
            player.level.FormationBackdrop.Display = false;
            player.level.FormationBackdrop.Alpha = 0.5f;
            player.StateMachine.State = 0;
            player.Depth = 0;
        }

        public void StartAttract(Vector2 attractTo)
        {
            this.attractTo = attractTo.Round();
            StateMachine.State = 22;
        }

        private void AttractBegin() => Speed = Vector2.Zero;

        private void AttractEnd()
        {
        }

        private int AttractUpdate()
        {
            if (Vector2.Distance(attractTo, ExactPosition) <= 1.5)
            {
                Position = attractTo;
                ZeroRemainderX();
                ZeroRemainderY();
            }
            else
            {
                Vector2 vector2 = Calc.Approach(ExactPosition, attractTo, 200f * Engine.DeltaTime);
                MoveToX(vector2.X);
                MoveToY(vector2.Y);
            }
            return 22;
        }

        public bool AtAttractTarget => StateMachine.State == 22 && ExactPosition == attractTo;

        private void DummyBegin()
        {
            DummyMoving = false;
            DummyGravity = true;
            DummyAutoAnimate = true;
        }

        private int DummyUpdate()
        {
            if (CanUnDuck)
                Ducking = false;
            if (!onGround && DummyGravity)
            {
                float num = Math.Abs(Speed.Y) >= 40.0 || !Input.Jump.Check && !AutoJump ? 1f : 0.5f;
                if (level.InSpace)
                    num *= 0.6f;
                Speed.Y = Calc.Approach(Speed.Y, 160f, 900f * num * Engine.DeltaTime);
            }
            if (varJumpTimer > 0.0)
            {
                if (AutoJump || Input.Jump.Check)
                    Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
                else
                    varJumpTimer = 0.0f;
            }
            if (!DummyMoving)
            {
                if (Math.Abs(Speed.X) > 90.0 && DummyMaxspeed)
                    Speed.X = Calc.Approach(Speed.X, 90f * Math.Sign(Speed.X), 2500f * Engine.DeltaTime);
                if (DummyFriction)
                    Speed.X = Calc.Approach(Speed.X, 0.0f, 1000f * Engine.DeltaTime);
            }
            if (DummyAutoAnimate)
            {
                if (onGround)
                {
                    if (Speed.X == 0.0)
                        Sprite.Play("idle");
                    else
                        Sprite.Play("walk");
                }
                else if (Speed.Y < 0.0)
                    Sprite.Play("jumpSlow");
                else
                    Sprite.Play("fallSlow");
            }
            return 11;
        }

        public IEnumerator DummyWalkTo(
            float x,
            bool walkBackwards = false,
            float speedMultiplier = 1f,
            bool keepWalkingIntoWalls = false)
        {
            Player player = this;
            player.StateMachine.State = 11;
            if (Math.Abs(player.X - x) > 4.0 && !player.Dead)
            {
                player.DummyMoving = true;
                if (walkBackwards)
                {
                    player.Sprite.Rate = -1f;
                    player.Facing = (Facings) Math.Sign(player.X - x);
                }
                else
                    player.Facing = (Facings) Math.Sign(x - player.X);
                while (Math.Abs(x - player.X) > 4.0 && player.Scene != null && (keepWalkingIntoWalls || !player.CollideCheck<Solid>(player.Position + Vector2.UnitX * Math.Sign(x - player.X))))
                {
                    player.Speed.X = Calc.Approach(player.Speed.X, Math.Sign(x - player.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                    yield return null;
                }
                player.Sprite.Rate = 1f;
                player.Sprite.Play("idle");
                player.DummyMoving = false;
            }
        }

        public IEnumerator DummyWalkToExact(
            int x,
            bool walkBackwards = false,
            float speedMultiplier = 1f,
            bool cancelOnFall = false)
        {
            Player player = this;
            player.StateMachine.State = 11;
            if ((double) player.X != x)
            {
                player.DummyMoving = true;
                if (walkBackwards)
                {
                    player.Sprite.Rate = -1f;
                    player.Facing = (Facings) Math.Sign(player.X - x);
                }
                else
                    player.Facing = (Facings) Math.Sign(x - player.X);
                int last = Math.Sign(player.X - x);
                while (!player.Dead && (double) player.X != x && !player.CollideCheck<Solid>(player.Position + new Vector2((float) player.Facing, 0.0f)) && (!cancelOnFall || player.OnGround()))
                {
                    player.Speed.X = Calc.Approach(player.Speed.X, Math.Sign(x - player.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                    int num = Math.Sign(player.X - x);
                    if (num != last)
                    {
                        player.X = x;
                        break;
                    }
                    last = num;
                    yield return null;
                }
                player.Speed.X = 0.0f;
                player.Sprite.Rate = 1f;
                player.Sprite.Play("idle");
                player.DummyMoving = false;
            }
        }

        public IEnumerator DummyRunTo(float x, bool fastAnim = false)
        {
            Player player = this;
            player.StateMachine.State = 11;
            if (Math.Abs(player.X - x) > 4.0)
            {
                player.DummyMoving = true;
                if (fastAnim)
                    player.Sprite.Play("runFast");
                else if (!player.Sprite.LastAnimationID.StartsWith("run"))
                    player.Sprite.Play("runSlow");
                player.Facing = (Facings) Math.Sign(x - player.X);
                while (Math.Abs(player.X - x) > 4.0)
                {
                    player.Speed.X = Calc.Approach(player.Speed.X, Math.Sign(x - player.X) * 90f, 1000f * Engine.DeltaTime);
                    yield return null;
                }
                player.Sprite.Play("idle");
                player.DummyMoving = false;
            }
        }

        private int FrozenUpdate() => 17;

        private int TempleFallUpdate()
        {
            Facing = Facings.Right;
            if (!onGround)
            {
                int num = level.Bounds.Left + 160;
                Speed.X = Calc.Approach(Speed.X, 54.0000038f * (Math.Abs(num - X) <= 4.0 ? 0.0f : Math.Sign(num - X)), 325f * Engine.DeltaTime);
            }
            if (!onGround && DummyGravity)
                Speed.Y = Calc.Approach(Speed.Y, 320f, 225f * Engine.DeltaTime);
            return 20;
        }

        private IEnumerator TempleFallCoroutine()
        {
            Player player = this;
            player.Sprite.Play("fallFast");
            while (!player.onGround)
                yield return null;
            player.Play("event:/char/madeline/mirrortemple_big_landing");
            if (player.Dashes <= 1)
                player.Sprite.Play("fallPose");
            else
                player.Sprite.Play("idle");
            player.Sprite.Scale.Y = 0.7f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            player.level.DirectionalShake(new Vector2(0.0f, 1f), 0.5f);
            player.Speed.X = 0.0f;
            player.level.Particles.Emit(Player.P_SummitLandA, 12, player.BottomCenter, Vector2.UnitX * 3f, -1.57079637f);
            player.level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
            player.level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -0.2617994f);
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
                yield return null;
            player.StateMachine.State = 0;
        }

        private void ReflectionFallBegin() => IgnoreJumpThrus = true;

        private void ReflectionFallEnd()
        {
            FallEffects.Show(false);
            IgnoreJumpThrus = false;
        }

        private int ReflectionFallUpdate()
        {
            Facing = Facings.Right;
            if (Scene.OnInterval(0.05f))
            {
                wasDashB = true;
                CreateTrail();
            }
            Speed.Y = !CollideCheck<Water>() ? Calc.Approach(Speed.Y, 320f, 225f * Engine.DeltaTime) : Calc.Approach(Speed.Y, -20f, 400f * Engine.DeltaTime);
            foreach (Entity entity in Scene.Tracker.GetEntities<FlyFeather>())
                entity.RemoveSelf();
            CrystalStaticSpinner crystalStaticSpinner = Scene.CollideFirst<CrystalStaticSpinner>(new Rectangle((int) (X - 6.0), (int) (Y - 6.0), 12, 12));
            if (crystalStaticSpinner != null)
            {
                crystalStaticSpinner.Destroy();
                level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Celeste.Freeze(0.01f);
            }
            return 18;
        }

        private IEnumerator ReflectionFallCoroutine()
        {
            Player player = this;
            player.Sprite.Play("bigFall");
            player.level.StartCutscene(player.OnReflectionFallSkip);
            for (float t = 0.0f; t < 2.0; t += Engine.DeltaTime)
            {
                player.Speed.Y = 0.0f;
                yield return null;
            }
            FallEffects.Show(true);
            player.Speed.Y = 320f;
            while (!player.CollideCheck<Water>())
                yield return null;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            FallEffects.Show(false);
            player.Sprite.Play("bigFallRecover");
            player.level.Session.Audio.Music.Event = "event:/music/lvl6/main";
            player.level.Session.Audio.Apply();
            player.level.EndCutscene();
            yield return 1.2f;
            player.StateMachine.State = 0;
        }

        private void OnReflectionFallSkip(Level level) => level.OnEndOfFrame += () =>
        {
            level.Remove(this);
            level.UnloadLevel();
            level.Session.Level = "00";
            Session session = level.Session;
            Level level1 = level;
            Rectangle bounds = level.Bounds;
            double left = bounds.Left;
            bounds = level.Bounds;
            double bottom = bounds.Bottom;
            Vector2 from = new((float)left, (float)bottom);
            Vector2? nullable = level1.GetSpawnPoint(from);
            session.RespawnPoint = nullable;
            level.LoadLevel(IntroTypes.None);
            FallEffects.Show(false);
            level.Session.Audio.Music.Event = "event:/music/lvl6/main";
            level.Session.Audio.Apply();
        };

        public IEnumerator IntroWalkCoroutine()
        {
            Player player = this;
            Vector2 start = player.Position;
            if (player.IntroWalkDirection == Facings.Right)
            {
                player.X = player.level.Bounds.Left - 16;
                player.Facing = Facings.Right;
            }
            else
            {
                player.X = player.level.Bounds.Right + 16;
                player.Facing = Facings.Left;
            }
            yield return 0.3f;
            player.Sprite.Play("runSlow");
            while (Math.Abs(player.X - start.X) > 2.0 && !player.CollideCheck<Solid>(player.Position + new Vector2((float) player.Facing, 0.0f)))
            {
                player.MoveTowardsX(start.X, 64f * Engine.DeltaTime);
                yield return null;
            }
            player.Position = start;
            player.Sprite.Play("idle");
            yield return 0.2f;
            player.StateMachine.State = 0;
        }

        private IEnumerator IntroJumpCoroutine()
        {
            Player player = this;
            Vector2 start = player.Position;
            bool wasSummitJump = player.StateMachine.PreviousState == 10;
            player.Depth = -1000000;
            player.Facing = Facings.Right;
            if (!wasSummitJump)
            {
                player.Y = player.level.Bounds.Bottom + 16;
                yield return 0.5f;
            }
            else
            {
                start.Y = player.level.Bounds.Bottom - 24;
                player.MoveToX((int)Math.Round(player.X / 8.0) * 8);
            }
            if (!wasSummitJump)
                player.Sprite.Play("jumpSlow");
            while (player.Y > start.Y - 8.0)
            {
                player.Y += -120f * Engine.DeltaTime;
                yield return null;
            }
            player.Y = (float) Math.Round(player.Y);
            player.Speed.Y = -100f;
            while (player.Speed.Y < 0.0)
            {
                player.Speed.Y += Engine.DeltaTime * 800f;
                yield return null;
            }
            player.Speed.Y = 0.0f;
            if (wasSummitJump)
            {
                yield return 0.2f;
                player.Play("event:/char/madeline/summit_areastart");
                player.Sprite.Play("launchRecover");
                yield return 0.1f;
            }
            else
                yield return 0.1f;
            if (!wasSummitJump)
                player.Sprite.Play("fallSlow");
            while (!player.onGround)
            {
                player.Speed.Y += Engine.DeltaTime * 800f;
                yield return null;
            }
            if (player.StateMachine.PreviousState != 10)
                player.Position = start;
            player.Depth = 0;
            player.level.DirectionalShake(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (wasSummitJump)
            {
                player.level.Particles.Emit(Player.P_SummitLandA, 12, player.BottomCenter, Vector2.UnitX * 3f, -1.57079637f);
                player.level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
                player.level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -0.2617994f);
                player.level.ParticlesBG.Emit(Player.P_SummitLandC, 30, player.BottomCenter, Vector2.UnitX * 5f);
                yield return 0.35f;
                for (int index = 0; index < player.Hair.Nodes.Count; ++index)
                    player.Hair.Nodes[index] = new Vector2(0.0f, 2 + index);
            }
            player.StateMachine.State = 0;
        }

        private IEnumerator IntroMoonJumpCoroutine()
        {
            Player player = this;
            Vector2 start = player.Position;
            player.Facing = Facings.Right;
            player.Speed = Vector2.Zero;
            player.Visible = false;
            player.Y = player.level.Bounds.Bottom + 16;
            yield return 0.5f;
            yield return player.MoonLanding(start);
            player.StateMachine.State = 0;
        }

        public IEnumerator MoonLanding(Vector2 groundPosition)
        {
            Player player = this;
            player.Depth = -1000000;
            player.Speed = Vector2.Zero;
            player.Visible = true;
            player.Sprite.Play("jumpSlow");
            while (player.Y > groundPosition.Y - 8.0)
            {
                player.MoveV(-200f * Engine.DeltaTime);
                yield return null;
            }
            player.Speed.Y = -200f;
            while (player.Speed.Y < 0.0)
            {
                player.Speed.Y += Engine.DeltaTime * 400f;
                yield return null;
            }
            player.Speed.Y = 0.0f;
            yield return 0.2f;
            player.Sprite.Play("fallSlow");
            float s = 100f;
            while (!player.OnGround())
            {
                player.Speed.Y += Engine.DeltaTime * s;
                s = Calc.Approach(s, 2f, Engine.DeltaTime * 50f);
                yield return null;
            }
            player.Depth = 0;
        }

        private IEnumerator IntroWakeUpCoroutine()
        {
            Sprite.Play("asleep");
            yield return 0.5f;
            yield return Sprite.PlayRoutine("wakeUp");
            yield return 0.2f;
            StateMachine.State = 0;
        }

        private void IntroRespawnBegin()
        {
            Play("event:/char/madeline/revive");
            Depth = -1000000;
            introEase = 1f;
            Vector2 from = Position;
            ref Vector2 local1 = ref from;
            double x = from.X;
            Rectangle bounds1 = level.Bounds;
            double min1 = bounds1.Left + 40.0;
            bounds1 = level.Bounds;
            double max1 = bounds1.Right - 40.0;
            double num1 = MathHelper.Clamp((float) x, (float) min1, (float) max1);
            local1.X = (float) num1;
            ref Vector2 local2 = ref from;
            double y = from.Y;
            Rectangle bounds2 = level.Bounds;
            double min2 = bounds2.Top + 40.0;
            bounds2 = level.Bounds;
            double max2 = bounds2.Bottom - 40.0;
            double num2 = MathHelper.Clamp((float) y, (float) min2, (float) max2);
            local2.Y = (float) num2;
            deadOffset = from;
            from -= Position;
            respawnTween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.6f, start: true);
            respawnTween.OnUpdate = t =>
            {
                deadOffset = Vector2.Lerp(from, Vector2.Zero, t.Eased);
                introEase = 1f - t.Eased;
            };
            respawnTween.OnComplete = t =>
            {
                if (StateMachine.State != 14)
                    return;
                StateMachine.State = 0;
                Sprite.Scale = new Vector2(1.5f, 0.5f);
            };
            Add(respawnTween);
        }

        private void IntroRespawnEnd()
        {
            Depth = 0;
            deadOffset = Vector2.Zero;
            Remove(respawnTween);
            respawnTween = null;
        }

        public IEnumerator IntroThinkForABitCoroutine()
        {
            Player player = this;
            (player.Scene as Level).Camera.X += 8f;
            yield return 0.1f;
            player.Sprite.Play("walk");
            float target = player.X + 8f;
            while (player.X < (double) target)
            {
                player.MoveH(32f * Engine.DeltaTime);
                yield return null;
            }
            player.Sprite.Play("idle");
            yield return 0.3f;
            player.Facing = Facings.Left;
            yield return 0.8f;
            player.Facing = Facings.Right;
            yield return 0.1f;
            player.StateMachine.State = 0;
        }

        private void BirdDashTutorialBegin()
        {
            DashBegin();
            Play("event:/char/madeline/dash_red_right");
            Sprite.Play("dash");
        }

        private int BirdDashTutorialUpdate() => 16;

        private IEnumerator BirdDashTutorialCoroutine()
        {
            Player player = this;
            yield return null;
            player.CreateTrail();
            player.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, player.CreateTrail, 0.08f, true));
            player.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, player.CreateTrail, 0.15f, true));
            Vector2 vector2 = new Vector2(1f, -1f).SafeNormalize();
            player.Facing = Facings.Right;
            player.Speed = vector2 * 240f;
            player.DashDir = vector2;
            player.SceneAs<Level>().DirectionalShake(player.DashDir, 0.2f);
            SlashFx.Burst(player.Center, player.DashDir.Angle());
            float time;
            for (time = 0.0f; time < 0.15000000596046448; time += Engine.DeltaTime)
            {
                if (player.Speed != Vector2.Zero && player.level.OnInterval(0.02f))
                    player.level.ParticlesFG.Emit(Player.P_DashA, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), player.DashDir.Angle());
                yield return null;
            }
            player.AutoJump = true;
            player.AutoJumpTimer = 0.0f;
            if (player.DashDir.Y <= 0.0)
                player.Speed = player.DashDir * 160f;
            if (player.Speed.Y < 0.0)
                player.Speed.Y *= 0.75f;
            player.Sprite.Play("fallFast");
            bool climbing = false;
            while (!player.OnGround() && !climbing)
            {
                player.Speed.Y = Calc.Approach(player.Speed.Y, 160f, 900f * Engine.DeltaTime);
                if (player.CollideCheck<Solid>(player.Position + new Vector2(1f, 0.0f)))
                    climbing = true;
                if ((double) player.Top > player.level.Bounds.Bottom)
                {
                    player.level.CancelCutscene();
                    player.Die(Vector2.Zero);
                }
                yield return null;
            }
            if (climbing)
            {
                player.Sprite.Play("wallslide");
                Dust.Burst(player.Position + new Vector2(4f, -6f), new Vector2(-4f, 0.0f).Angle());
                player.Speed.Y = 0.0f;
                yield return 0.2f;
                player.Sprite.Play("climbUp");
                while (player.CollideCheck<Solid>(player.Position + new Vector2(1f, 0.0f)))
                {
                    player.Y += -45f * Engine.DeltaTime;
                    yield return null;
                }
                player.Y = (float) Math.Round(player.Y);
                player.Play("event:/char/madeline/climb_ledge");
                player.Sprite.Play("jumpFast");
                player.Speed.Y = -105f;
                while (!player.OnGround())
                {
                    player.Speed.Y = Calc.Approach(player.Speed.Y, 160f, 900f * Engine.DeltaTime);
                    player.Speed.X = 20f;
                    yield return null;
                }
                player.Speed.X = 0.0f;
                player.Speed.Y = 0.0f;
                player.Sprite.Play("walk");
                for (time = 0.0f; time < 0.5; time += Engine.DeltaTime)
                {
                    player.X += 32f * Engine.DeltaTime;
                    yield return null;
                }
                player.Sprite.Play("tired");
            }
            else
            {
                player.Sprite.Play("tired");
                player.Speed.Y = 0.0f;
                while (player.Speed.X != 0.0)
                {
                    player.Speed.X = Calc.Approach(player.Speed.X, 0.0f, 240f * Engine.DeltaTime);
                    if (player.Scene.OnInterval(0.04f))
                        Dust.Burst(player.BottomCenter + new Vector2(0.0f, -2f), -2.3561945f);
                    yield return null;
                }
            }
        }

        public EventInstance Play(string sound, string param = null, float value = 0.0f)
        {
            float num = 0.0f;
            if (Scene is Level scene && scene.Raining)
                num = 1f;
            AddChaserStateSound(sound, param, value);
            return Audio.Play(sound, Center, param, value, "raining", num);
        }

        public void Loop(SoundSource sfx, string sound)
        {
            AddChaserStateSound(sound, null, 0.0f, ChaserStateSound.Actions.Loop);
            sfx.Play(sound);
        }

        public void Stop(SoundSource sfx)
        {
            if (!sfx.Playing)
                return;
            AddChaserStateSound(sfx.EventName, null, 0.0f, ChaserStateSound.Actions.Stop);
            sfx.Stop();
        }

        private void AddChaserStateSound(string sound, ChaserStateSound.Actions action) => AddChaserStateSound(sound, null, 0.0f, action);

        private void AddChaserStateSound(
            string sound,
            string param = null,
            float value = 0.0f,
            ChaserStateSound.Actions action = ChaserStateSound.Actions.Oneshot)
        {
            SFX.MadelineToBadelineSound.TryGetValue(sound, out string str);
            if (str == null)
                return;
            activeSounds.Add(new ChaserStateSound
            {
                Event = str,
                Parameter = param,
                ParameterValue = value,
                Action = action
            });
        }

        private ParticleType DustParticleFromSurfaceIndex(int index) => index == 40 ? ParticleTypes.SparkyDust : ParticleTypes.Dust;

        public enum IntroTypes
        {
            Transition,
            Respawn,
            WalkInRight,
            WalkInLeft,
            Jump,
            WakeUp,
            Fall,
            TempleMirrorVoid,
            None,
            ThinkForABit,
        }

        public struct ChaserStateSound
        {
            public string Event;
            public string Parameter;
            public float ParameterValue;
            public Actions Action;

            public enum Actions
            {
                Oneshot,
                Loop,
                Stop,
            }
        }

        public struct ChaserState
        {
            public Vector2 Position;
            public float TimeStamp;
            public string Animation;
            public Facings Facing;
            public bool OnGround;
            public Color HairColor;
            public int Depth;
            public Vector2 Scale;
            public Vector2 DashDirection;
            private ChaserStateSound sound0;
            private ChaserStateSound sound1;
            private ChaserStateSound sound2;
            private ChaserStateSound sound3;
            private ChaserStateSound sound4;
            public int Sounds;

            public ChaserState(Player player)
            {
                Position = player.Position;
                TimeStamp = player.Scene.TimeActive;
                Animation = player.Sprite.CurrentAnimationID;
                Facing = player.Facing;
                OnGround = player.onGround;
                HairColor = player.Hair.Color;
                Depth = player.Depth;
                Scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float) player.Facing, player.Sprite.Scale.Y);
                DashDirection = player.DashDir;
                List<ChaserStateSound> activeSounds = player.activeSounds;
                Sounds = Math.Min(5, activeSounds.Count);
                sound0 = Sounds > 0 ? activeSounds[0] : new ChaserStateSound();
                sound1 = Sounds > 1 ? activeSounds[1] : new ChaserStateSound();
                sound2 = Sounds > 2 ? activeSounds[2] : new ChaserStateSound();
                sound3 = Sounds > 3 ? activeSounds[3] : new ChaserStateSound();
                sound4 = Sounds > 4 ? activeSounds[4] : new ChaserStateSound();
            }

            public ChaserStateSound this[int index]
            {
                get
                {
                    return index switch
                    {
                        0 => sound0,
                        1 => sound1,
                        2 => sound2,
                        3 => sound3,
                        4 => sound4,
                        _ => new ChaserStateSound(),
                    };
                }
            }
        }
    }
}
