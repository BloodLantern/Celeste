using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class BirdNPC : Actor
    {
        public static ParticleType P_Feather;
        private static string FlownFlag = "bird_fly_away_";
        public Facings Facing = Facings.Left;
        public Sprite Sprite;
        public Vector2 StartPosition;
        public VertexLight Light;
        public bool AutoFly;
        public EntityID EntityID;
        public bool FlyAwayUp = true;
        public float WaitForLightningPostDelay;
        public bool DisableFlapSfx;
        private Coroutine tutorialRoutine;
        private Modes mode;
        private BirdTutorialGui gui;
        private Level level;
        private Vector2[] nodes;
        private StaticMover staticMover;
        private bool onlyOnce;
        private bool onlyIfPlayerLeft;

        public BirdNPC(Vector2 position, Modes mode)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.Scale.X = (float) Facing;
            Sprite.UseRawDeltaTime = true;
            Sprite.OnFrameChange = spr =>
            {
                if (level != null && X > level.Camera.Left + 64.0 && X < level.Camera.Right - 64.0 && (spr.Equals("peck") || spr.Equals("peckRare")) && Sprite.CurrentAnimationFrame == 6)
                    Audio.Play("event:/game/general/bird_peck", Position);
                if (level == null || level.Session.Area.ID != 10 || DisableFlapSfx)
                    return;
                BirdNPC.FlapSfxCheck(Sprite);
            };
            Add(Light = new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 8, 32));
            StartPosition = Position;
            SetMode(mode);
        }

        public BirdNPC(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Enum(nameof (mode), Modes.None))
        {
            EntityID = new EntityID(data.Level.Name, data.ID);
            nodes = data.NodesOffset(offset);
            onlyOnce = data.Bool(nameof (onlyOnce));
            onlyIfPlayerLeft = data.Bool(nameof (onlyIfPlayerLeft));
        }

        public void SetMode(Modes mode)
        {
            this.mode = mode;
            if (tutorialRoutine != null)
                tutorialRoutine.RemoveSelf();
            switch (mode)
            {
                case Modes.ClimbingTutorial:
                    Add(tutorialRoutine = new Coroutine(ClimbingTutorial()));
                    break;
                case Modes.DashingTutorial:
                    Add(tutorialRoutine = new Coroutine(DashingTutorial()));
                    break;
                case Modes.DreamJumpTutorial:
                    Add(tutorialRoutine = new Coroutine(DreamJumpTutorial()));
                    break;
                case Modes.SuperWallJumpTutorial:
                    Add(tutorialRoutine = new Coroutine(SuperWallJumpTutorial()));
                    break;
                case Modes.HyperJumpTutorial:
                    Add(tutorialRoutine = new Coroutine(HyperJumpTutorial()));
                    break;
                case Modes.FlyAway:
                    Add(tutorialRoutine = new Coroutine(WaitRoutine()));
                    break;
                case Modes.Sleeping:
                    Sprite.Play("sleep");
                    Facing = Facings.Right;
                    break;
                case Modes.MoveToNodes:
                    Add(tutorialRoutine = new Coroutine(MoveToNodesRoutine()));
                    break;
                case Modes.WaitForLightningOff:
                    Add(tutorialRoutine = new Coroutine(WaitForLightningOffRoutine()));
                    break;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            if (mode == Modes.ClimbingTutorial && level.Session.GetLevelFlag("2"))
            {
                RemoveSelf();
            }
            else
            {
                if (mode != Modes.FlyAway || !level.Session.GetFlag(BirdNPC.FlownFlag + level.Session.Level))
                    return;
                RemoveSelf();
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (mode == Modes.SuperWallJumpTutorial)
            {
                Player entity = scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Y < Y + 32.0)
                    RemoveSelf();
            }
            if (!onlyIfPlayerLeft)
                return;
            Player entity1 = level.Tracker.GetEntity<Player>();
            if (entity1 == null || entity1.X <= (double) X)
                return;
            RemoveSelf();
        }

        public override bool IsRiding(Solid solid) => Scene.CollideCheck(new Rectangle((int) X - 4, (int) Y, 8, 2), solid);

        public override void Update()
        {
            Sprite.Scale.X = (float) Facing;
            base.Update();
        }

        public IEnumerator Caw()
        {
            BirdNPC birdNpc = this;
            birdNpc.Sprite.Play("croak");
            while (birdNpc.Sprite.CurrentAnimationFrame < 9)
                yield return null;
            Audio.Play("event:/game/general/bird_squawk", birdNpc.Position);
        }

        public IEnumerator ShowTutorial(BirdTutorialGui gui, bool caw = false)
        {
            BirdNPC birdNpc = this;
            if (caw)
                yield return birdNpc.Caw();
            birdNpc.gui = gui;
            gui.Open = true;
            birdNpc.Scene.Add(gui);
            while (gui.Scale < 1.0)
                yield return null;
        }

        public IEnumerator HideTutorial()
        {
            BirdNPC birdNpc = this;
            if (birdNpc.gui != null)
            {
                birdNpc.gui.Open = false;
                while (birdNpc.gui.Scale > 0.0)
                    yield return null;
                birdNpc.Scene.Remove(birdNpc.gui);
                birdNpc.gui = null;
            }
        }

        public IEnumerator StartleAndFlyAway()
        {
            BirdNPC birdNpc = this;
            birdNpc.Depth = -1000000;
            birdNpc.level.Session.SetFlag(BirdNPC.FlownFlag + birdNpc.level.Session.Level);
            yield return birdNpc.Startle("event:/game/general/bird_startle");
            yield return birdNpc.FlyAway();
        }

        public IEnumerator FlyAway(float upwardsMultiplier = 1f)
        {
            BirdNPC birdNpc = this;
            if (birdNpc.staticMover != null)
            {
                birdNpc.staticMover.RemoveSelf();
                birdNpc.staticMover = null;
            }
            birdNpc.Sprite.Play("fly");
            birdNpc.Facing = (Facings) (-(int) birdNpc.Facing);
            Vector2 speed = new Vector2((int) birdNpc.Facing * 20, -40f * upwardsMultiplier);
            while (birdNpc.Y > (double) birdNpc.level.Bounds.Top)
            {
                speed += new Vector2((int) birdNpc.Facing * 140, -120f * upwardsMultiplier) * Engine.DeltaTime;
                birdNpc.Position += speed * Engine.DeltaTime;
                yield return null;
            }
            birdNpc.RemoveSelf();
        }

        private IEnumerator ClimbingTutorial()
        {
            BirdNPC birdNpc = this;
            yield return 0.25f;
            Player p = birdNpc.Scene.Tracker.GetEntity<Player>();
            while (Math.Abs(p.X - birdNpc.X) > 120.0)
                yield return null;
            BirdTutorialGui tut1 = new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
            BirdTutorialGui tut2 = new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), Dialog.Clean("tutorial_climb"), BirdTutorialGui.ButtonPrompt.Grab, "+", new Vector2(0.0f, -1f));
            bool first = true;
            bool willEnd;
            do
            {
                yield return birdNpc.ShowTutorial(tut1, first);
                first = false;
                while (p.StateMachine.State != 1 && p.Y > (double) birdNpc.Y)
                    yield return null;
                if (p.Y > (double) birdNpc.Y)
                {
                    Audio.Play("event:/ui/game/tutorial_note_flip_back");
                    yield return birdNpc.HideTutorial();
                    yield return birdNpc.ShowTutorial(tut2);
                }
                while (p.Scene != null && (!p.OnGround() || p.StateMachine.State == 1))
                    yield return null;
                willEnd = p.Y <= birdNpc.Y + 4.0;
                if (!willEnd)
                    Audio.Play("event:/ui/game/tutorial_note_flip_front");
                yield return birdNpc.HideTutorial();
            }
            while (!willEnd);
            yield return birdNpc.StartleAndFlyAway();
        }

        private IEnumerator DashingTutorial()
        {
            BirdNPC bird = this;
            bird.Y = bird.level.Bounds.Top;
            bird.X += 32f;
            yield return 1f;
            Player player = bird.Scene.Tracker.GetEntity<Player>();
            Bridge bridge = bird.Scene.Entities.FindFirst<Bridge>();
            while ((player == null || player.X <= bird.StartPosition.X - 92.0 || player.Y <= bird.StartPosition.Y - 20.0 || player.Y >= bird.StartPosition.Y - 10.0) && (!SaveData.Instance.Assists.Invincible || player == null || player.X <= bird.StartPosition.X - 60.0 || player.Y <= (double) bird.StartPosition.Y || player.Y >= bird.StartPosition.Y + 34.0))
                yield return null;
            bird.Scene.Add(new CS00_Ending(player, bird, bridge));
        }

        private IEnumerator DreamJumpTutorial()
        {
            BirdNPC birdNpc = this;
            yield return birdNpc.ShowTutorial(new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), Dialog.Clean("tutorial_dreamjump"), new Vector2(1f, 0.0f), "+", BirdTutorialGui.ButtonPrompt.Jump), true);
            while (true)
            {
                Player entity = birdNpc.Scene.Tracker.GetEntity<Player>();
                if (entity == null || entity.X <= (double) birdNpc.X && (birdNpc.Position - entity.Position).Length() >= 32.0)
                    yield return null;
                else
                    break;
            }
            yield return birdNpc.HideTutorial();
            while (true)
            {
                Player entity = birdNpc.Scene.Tracker.GetEntity<Player>();
                if (entity == null || (birdNpc.Position - entity.Position).Length() >= 24.0)
                    yield return null;
                else
                    break;
            }
            yield return birdNpc.StartleAndFlyAway();
        }

        private IEnumerator SuperWallJumpTutorial()
        {
            BirdNPC birdNpc = this;
            birdNpc.Facing = Facings.Right;
            yield return 0.25f;
            bool caw = true;
            BirdTutorialGui tut1 = new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), GFX.Gui["hyperjump/tutorial00"], Dialog.Clean("TUTORIAL_DASH"), new Vector2(0.0f, -1f));
            BirdTutorialGui tut2 = new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), GFX.Gui["hyperjump/tutorial01"], Dialog.Clean("TUTORIAL_DREAMJUMP"));
            Player entity;
            do
            {
                yield return birdNpc.ShowTutorial(tut1, caw);
                birdNpc.Sprite.Play("idleRarePeck");
                yield return 2f;
                birdNpc.gui = tut2;
                birdNpc.gui.Open = true;
                birdNpc.gui.Scale = 1f;
                birdNpc.Scene.Add(birdNpc.gui);
                yield return null;
                tut1.Open = false;
                tut1.Scale = 0.0f;
                birdNpc.Scene.Remove(tut1);
                yield return 2f;
                yield return birdNpc.HideTutorial();
                yield return 2f;
                caw = false;
                entity = birdNpc.Scene.Tracker.GetEntity<Player>();
            }
            while (entity == null || entity.Y > (double) birdNpc.Y || entity.X <= birdNpc.X + 144.0);
            yield return birdNpc.StartleAndFlyAway();
        }

        private IEnumerator HyperJumpTutorial()
        {
            BirdNPC birdNpc = this;
            birdNpc.Facing = Facings.Left;
            BirdTutorialGui tut = new BirdTutorialGui(birdNpc, new Vector2(0.0f, -16f), Dialog.Clean("TUTORIAL_DREAMJUMP"), new Vector2(1f, 1f), "+", BirdTutorialGui.ButtonPrompt.Dash, GFX.Gui["tinyarrow"], BirdTutorialGui.ButtonPrompt.Jump);
            yield return 0.3f;
            yield return birdNpc.ShowTutorial(tut, true);
        }

        private IEnumerator WaitRoutine()
        {
            BirdNPC birdNpc = this;
            while (!birdNpc.AutoFly)
            {
                Player entity = birdNpc.Scene.Tracker.GetEntity<Player>();
                if (entity == null || Math.Abs(entity.X - birdNpc.X) >= 120.0)
                    yield return null;
                else
                    break;
            }
            yield return birdNpc.Caw();
            while (!birdNpc.AutoFly)
            {
                Player entity = birdNpc.Scene.Tracker.GetEntity<Player>();
                if (entity == null || (entity.Center - birdNpc.Position).Length() >= 32.0)
                    yield return null;
                else
                    break;
            }
            yield return birdNpc.StartleAndFlyAway();
        }

        public IEnumerator Startle(string startleSound, float duration = 0.8f, Vector2? multiplier = null)
        {
            BirdNPC birdNpc = this;
            if (!multiplier.HasValue)
                multiplier = new Vector2(1f, 1f);
            if (!string.IsNullOrWhiteSpace(startleSound))
                Audio.Play(startleSound, birdNpc.Position);
            Dust.Burst(birdNpc.Position, -1.57079637f, 8);
            birdNpc.Sprite.Play("jump");
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, true);
            tween.OnUpdate = t =>
            {
                if (t.Eased < 0.5 && Scene.OnInterval(0.05f))
                    level.Particles.Emit(BirdNPC.P_Feather, 2, Position + Vector2.UnitY * -6f, Vector2.One * 4f);
                Vector2 vector2 = Vector2.Lerp(new Vector2(100f, -100f) * multiplier.Value, new Vector2(20f, -20f) * multiplier.Value, t.Eased);
                vector2.X *= -(int) Facing;
                Position += vector2 * Engine.DeltaTime;
            };
            birdNpc.Add(tween);
            while (tween.Active)
                yield return null;
        }

        public IEnumerator FlyTo(Vector2 target, float durationMult = 1f, bool relocateSfx = true)
        {
            BirdNPC birdNpc = this;
            birdNpc.Sprite.Play("fly");
            if (relocateSfx)
                birdNpc.Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
            int num = Math.Sign(target.X - birdNpc.X);
            if (num != 0)
                birdNpc.Facing = (Facings) num;
            Vector2 position = birdNpc.Position;
            Vector2 end = target;
            SimpleCurve curve = new SimpleCurve(position, end, position + (end - position) * 0.75f - Vector2.UnitY * 30f);
            float duration = (end - position).Length() / 100f * durationMult;
            for (float p = 0.0f; p < 0.949999988079071; p += Engine.DeltaTime / duration)
            {
                birdNpc.Position = curve.GetPoint(Ease.SineInOut(p)).Floor();
                birdNpc.Sprite.Rate = (float) (1.0 - p * 0.5);
                yield return null;
            }
            Dust.Burst(birdNpc.Position, -1.57079637f, 8);
            birdNpc.Position = target;
            birdNpc.Facing = Facings.Left;
            birdNpc.Sprite.Rate = 1f;
            birdNpc.Sprite.Play("idle");
        }

        private IEnumerator MoveToNodesRoutine()
        {
            BirdNPC birdNpc = this;
            int index = 0;
            while (true)
            {
                Player entity = birdNpc.Scene.Tracker.GetEntity<Player>();
                if (entity == null || (entity.Center - birdNpc.Position).Length() >= 80.0)
                {
                    yield return null;
                }
                else
                {
                    birdNpc.Depth = -1000000;
                    yield return birdNpc.Startle("event:/new_content/game/10_farewell/bird_startle", 0.2f);
                    if (index < birdNpc.nodes.Length)
                    {
                        yield return birdNpc.FlyTo(birdNpc.nodes[index], 0.6f);
                        ++index;
                    }
                    else
                    {
                        birdNpc.Tag = (int) Tags.Persistent;
                        birdNpc.Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
                        if (birdNpc.onlyOnce)
                            birdNpc.level.Session.DoNotLoad.Add(birdNpc.EntityID);
                        birdNpc.Sprite.Play("fly");
                        birdNpc.Facing = Facings.Right;
                        Vector2 speed = new Vector2((int) birdNpc.Facing * 20, -40f);
                        while (birdNpc.Y > (double) (birdNpc.level.Bounds.Top - 200))
                        {
                            speed += new Vector2((int) birdNpc.Facing * 140, -60f) * Engine.DeltaTime;
                            birdNpc.Position += speed * Engine.DeltaTime;
                            yield return null;
                        }
                        birdNpc.RemoveSelf();
                        speed = new Vector2();
                    }
                }
            }
        }

        private IEnumerator WaitForLightningOffRoutine()
        {
            BirdNPC birdNpc = this;
            birdNpc.Sprite.Play("hoverStressed");
            while (birdNpc.Scene.Entities.FindFirst<Lightning>() != null)
                yield return null;
            if (birdNpc.WaitForLightningPostDelay > 0.0)
                yield return birdNpc.WaitForLightningPostDelay;
            Vector2 speed;
            if (!birdNpc.FlyAwayUp)
            {
                birdNpc.Sprite.Play("fly");
                speed = new Vector2((int) birdNpc.Facing * 20, -10f);
                while (birdNpc.Y > (double) birdNpc.level.Bounds.Top)
                {
                    speed += new Vector2((int) birdNpc.Facing * 140, -10f) * Engine.DeltaTime;
                    birdNpc.Position += speed * Engine.DeltaTime;
                    yield return null;
                }
                speed = new Vector2();
            }
            else
            {
                birdNpc.Sprite.Play("flyup");
                speed = new Vector2(0.0f, -32f);
                while (birdNpc.Y > (double) birdNpc.level.Bounds.Top)
                {
                    speed += new Vector2(0.0f, -100f) * Engine.DeltaTime;
                    birdNpc.Position += speed * Engine.DeltaTime;
                    yield return null;
                }
                speed = new Vector2();
            }
            birdNpc.RemoveSelf();
        }

        public override void SceneEnd(Scene scene)
        {
            Engine.TimeRate = 1f;
            base.SceneEnd(scene);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (mode != Modes.DashingTutorial)
                return;
            float x1 = StartPosition.X - 92f;
            float right1 = level.Bounds.Right;
            float y1 = StartPosition.Y - 20f;
            float y2 = StartPosition.Y - 10f;
            Draw.Line(new Vector2(x1, y1), new Vector2(x1, y2), Color.Aqua);
            Draw.Line(new Vector2(x1, y1), new Vector2(right1, y1), Color.Aqua);
            Draw.Line(new Vector2(right1, y1), new Vector2(right1, y2), Color.Aqua);
            Draw.Line(new Vector2(x1, y2), new Vector2(right1, y2), Color.Aqua);
            float x2 = StartPosition.X - 60f;
            float right2 = level.Bounds.Right;
            float y3 = StartPosition.Y;
            float y4 = StartPosition.Y + 34f;
            Draw.Line(new Vector2(x2, y3), new Vector2(x2, y4), Color.Aqua);
            Draw.Line(new Vector2(x2, y3), new Vector2(right2, y3), Color.Aqua);
            Draw.Line(new Vector2(right2, y3), new Vector2(right2, y4), Color.Aqua);
            Draw.Line(new Vector2(x2, y4), new Vector2(right2, y4), Color.Aqua);
        }

        public static void FlapSfxCheck(Sprite sprite)
        {
            if (sprite.Entity != null && sprite.Entity.Scene != null)
            {
                Camera camera = (sprite.Entity.Scene as Level).Camera;
                Vector2 renderPosition = sprite.RenderPosition;
                if (renderPosition.X < camera.X - 32.0 || renderPosition.Y < camera.Y - 32.0 || renderPosition.X > camera.X + 320.0 + 32.0 || renderPosition.Y > camera.Y + 180.0 + 32.0)
                    return;
            }
            string currentAnimationId = sprite.CurrentAnimationID;
            int currentAnimationFrame = sprite.CurrentAnimationFrame;
            if ((!(currentAnimationId == "hover") || currentAnimationFrame != 0) && (!(currentAnimationId == "hoverStressed") || currentAnimationFrame != 0) && (!(currentAnimationId == "fly") || currentAnimationFrame != 0))
                return;
            Audio.Play("event:/new_content/game/10_farewell/bird_wingflap", sprite.RenderPosition);
        }

        public enum Modes
        {
            ClimbingTutorial,
            DashingTutorial,
            DreamJumpTutorial,
            SuperWallJumpTutorial,
            HyperJumpTutorial,
            FlyAway,
            None,
            Sleeping,
            MoveToNodes,
            WaitForLightningOff,
        }
    }
}
