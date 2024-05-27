using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS07_Credits : CutsceneEntity
    {
        public const float CameraXOffset = 70f;
        public const float CameraYOffset = -24f;
        public static CS07_Credits Instance;
        public string Event;
        private MTexture gradient = GFX.Gui["creditsgradient"].GetSubtexture(0, 1, 1920, 1);
        private Credits credits;
        private Player player;
        private bool autoWalk = true;
        private bool autoUpdateCamera = true;
        private BadelineDummy badeline;
        private bool badelineAutoFloat = true;
        private bool badelineAutoWalk;
        private float badelineWalkApproach;
        private Vector2 badelineWalkApproachFrom;
        private float walkOffset;
        private bool wasDashAssistOn;
        private Fill fillbg;
        private float fade = 1f;
        private HiresSnow snow;
        private bool gotoEpilogue;

        public CS07_Credits()
        {
            MInput.Disabled = true;
            CS07_Credits.Instance = this;
            Tag = (int) Tags.Global | (int) Tags.HUD;
            wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            SaveData.Instance.Assists.DashAssist = false;
        }

        public override void OnBegin(Level level)
        {
            Audio.BusMuted("bus:/gameplay_sfx", true);
            gotoEpilogue = level.Session.OldStats.Modes[0].Completed;
            gotoEpilogue = true;
            Add(new Coroutine(Routine()));
            Add(new PostUpdateHook(PostUpdate));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (Scene as Level).InCredits = true;
        }

        private IEnumerator Routine()
        {
            CS07_Credits cs07Credits1 = this;
            cs07Credits1.Level.Background.Backdrops.Add(cs07Credits1.fillbg = new Fill());
            cs07Credits1.Level.Completed = true;
            cs07Credits1.Level.Entities.FindFirst<SpeedrunTimerDisplay>()?.RemoveSelf();
            cs07Credits1.Level.Entities.FindFirst<TotalStrawberriesDisplay>()?.RemoveSelf();
            cs07Credits1.Level.Entities.FindFirst<GameplayStats>()?.RemoveSelf();
            yield return null;
            cs07Credits1.Level.Wipe.Cancel();
            yield return 0.5f;
            float alignment = 1f;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                alignment = 0.0f;
            cs07Credits1.credits = new Credits(alignment, 0.6f, false, true);
            cs07Credits1.credits.AllowInput = false;
            yield return 3f;
            cs07Credits1.SetBgFade(0.0f);
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return cs07Credits1.SetupLevel();
            yield return cs07Credits1.WaitForPlayer();
            yield return cs07Credits1.FadeTo(1f);
            yield return 1f;
            cs07Credits1.SetBgFade(0.1f);
            yield return cs07Credits1.NextLevel("credits-dashes");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return cs07Credits1.WaitForPlayer();
            yield return cs07Credits1.FadeTo(1f);
            yield return 1f;
            cs07Credits1.SetBgFade(0.2f);
            yield return cs07Credits1.NextLevel("credits-walking");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return 5.8f;
            cs07Credits1.badelineAutoFloat = false;
            yield return 0.5f;
            cs07Credits1.badeline.Sprite.Scale.X = 1f;
            yield return 0.5f;
            cs07Credits1.autoWalk = false;
            cs07Credits1.player.Speed = Vector2.Zero;
            cs07Credits1.player.Facing = Facings.Right;
            yield return 1.5f;
            cs07Credits1.badeline.Sprite.Scale.X = -1f;
            yield return 1f;
            cs07Credits1.badeline.Sprite.Scale.X = -1f;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badelineWalkApproachFrom = cs07Credits1.badeline.Position;
            cs07Credits1.Add(new Coroutine(cs07Credits1.BadelineApproachWalking()));
            yield return 0.7f;
            cs07Credits1.autoWalk = true;
            cs07Credits1.player.Facing = Facings.Left;
            yield return cs07Credits1.WaitForPlayer();
            yield return cs07Credits1.FadeTo(1f);
            yield return 1f;
            cs07Credits1.SetBgFade(0.3f);
            yield return cs07Credits1.NextLevel("credits-tree");
            yield return cs07Credits1.SetupLevel();
            Petals petals = new Petals();
            cs07Credits1.Level.Foreground.Backdrops.Add(petals);
            cs07Credits1.autoUpdateCamera = false;
            Vector2 target1 = cs07Credits1.Level.Camera.Position + new Vector2(-220f, 32f);
            cs07Credits1.Level.Camera.Position += new Vector2(-100f, 0.0f);
            cs07Credits1.badelineWalkApproach = 1f;
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badeline.Floatness = 0.0f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            cs07Credits1.Add(new Coroutine(CutsceneEntity.CameraTo(target1, 12f, Ease.Linear)));
            yield return 3.5f;
            cs07Credits1.badeline.Sprite.Play("idle");
            cs07Credits1.badelineAutoWalk = false;
            yield return 0.25f;
            cs07Credits1.autoWalk = false;
            cs07Credits1.player.Sprite.Play("idle");
            cs07Credits1.player.Speed = Vector2.Zero;
            cs07Credits1.player.DummyAutoAnimate = false;
            cs07Credits1.player.Facing = Facings.Right;
            yield return 0.5f;
            cs07Credits1.player.Sprite.Play("sitDown");
            yield return 4f;
            cs07Credits1.badeline.Sprite.Play("laugh");
            yield return 1.75f;
            yield return cs07Credits1.FadeTo(1f);
            cs07Credits1.Level.Foreground.Backdrops.Remove(petals);
            petals = null;
            yield return 1f;
            cs07Credits1.SetBgFade(0.4f);
            yield return cs07Credits1.NextLevel("credits-clouds");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.autoWalk = false;
            cs07Credits1.player.Speed = Vector2.Zero;
            cs07Credits1.autoUpdateCamera = false;
            cs07Credits1.player.ForceCameraUpdate = false;
            cs07Credits1.badeline.Visible = false;
            Player other = null;
            foreach (CreditsTrigger entity in cs07Credits1.Scene.Tracker.GetEntities<CreditsTrigger>())
            {
                if (entity.Event == "BadelineOffset")
                {
                    other = new Player(entity.Position, PlayerSpriteMode.Badeline);
                    other.OverrideHairColor = BadelineOldsite.HairColor;
                    yield return null;
                    other.StateMachine.State = 11;
                    other.Facing = Facings.Left;
                    cs07Credits1.Scene.Add(other);
                }
            }
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            cs07Credits1.Level.Camera.Position += new Vector2(0.0f, -100f);
            Vector2 target2 = cs07Credits1.Level.Camera.Position + new Vector2(0.0f, 160f);
            cs07Credits1.Add(new Coroutine(CutsceneEntity.CameraTo(target2, 12f, Ease.Linear)));
            float playerHighJump = 0.0f;
            float baddyHighJump = 0.0f;
            for (float p = 0.0f; p < 10.0; p += Engine.DeltaTime)
            {
                if ((p > 3.0 && p < 6.0 || p > 9.0) && cs07Credits1.player.Speed.Y < 0.0 && cs07Credits1.player.OnGround(4))
                    playerHighJump = 0.25f;
                if (p > 5.0 && p < 8.0 && other.Speed.Y < 0.0 && other.OnGround(4))
                    baddyHighJump = 0.25f;
                if (playerHighJump > 0.0)
                {
                    playerHighJump -= Engine.DeltaTime;
                    cs07Credits1.player.Speed.Y = -200f;
                }
                if (baddyHighJump > 0.0)
                {
                    baddyHighJump -= Engine.DeltaTime;
                    other.Speed.Y = -200f;
                }
                yield return null;
            }
            yield return cs07Credits1.FadeTo(1f);
            other = null;
            yield return 1f;
            CS07_Credits cs07Credits = cs07Credits1;
            cs07Credits1.SetBgFade(0.5f);
            yield return cs07Credits1.NextLevel("credits-resort");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            cs07Credits1.badelineWalkApproach = 1f;
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badeline.Floatness = 0.0f;
            Vector2 vector2 = Vector2.Zero;
            foreach (CreditsTrigger creditsTrigger in cs07Credits1.Scene.Entities.FindAll<CreditsTrigger>())
            {
                if (creditsTrigger.Event == "Oshiro")
                    vector2 = creditsTrigger.Position;
            }
            NPC oshiro = new NPC(vector2 + new Vector2(0.0f, 4f));
            oshiro.Add(oshiro.Sprite = new OshiroSprite(1));
            oshiro.MoveAnim = "sweeping";
            oshiro.IdleAnim = "sweeping";
            oshiro.Sprite.Play("sweeping");
            oshiro.Maxspeed = 10f;
            oshiro.Depth = -60;
            cs07Credits1.Scene.Add(oshiro);
            cs07Credits1.Add(new Coroutine(cs07Credits1.DustyRoutine(oshiro)));
            yield return 4.8f;
            Vector2 oshiroTarget = oshiro.Position + new Vector2(116f, 0.0f);
            Coroutine oshiroRoutine = new Coroutine(oshiro.MoveTo(oshiroTarget));
            cs07Credits1.Add(oshiroRoutine);
            yield return 2f;
            cs07Credits1.autoUpdateCamera = false;
            yield return CutsceneEntity.CameraTo(new Vector2(cs07Credits1.Level.Bounds.Left + 64, cs07Credits1.Level.Bounds.Top), 2f);
            yield return 5f;
            BirdNPC bird = new BirdNPC(oshiro.Position + new Vector2(280f, -160f), BirdNPC.Modes.None);
            bird.Depth = 10010;
            bird.Light.Visible = false;
            cs07Credits1.Scene.Add(bird);
            bird.Facing = Facings.Left;
            bird.Sprite.Play("fall");
            Vector2 from = bird.Position;
            Vector2 to = oshiroTarget + new Vector2(50f, -12f);
            baddyHighJump = 0.0f;
            while (baddyHighJump < 1.0)
            {
                bird.Position = from + (to - from) * Ease.QuadOut(baddyHighJump);
                if (baddyHighJump > 0.5)
                {
                    bird.Sprite.Play("fly");
                    bird.Depth = -1000000;
                    bird.Light.Visible = true;
                }
                baddyHighJump += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            bird.Position = to;
            oshiroRoutine.RemoveSelf();
            oshiro.Sprite.Play("putBroomAway");
            oshiro.Sprite.OnFrameChange = anim =>
            {
                if (oshiro.Sprite.CurrentAnimationFrame != 10)
                    return;
                Entity entity = new Entity(oshiro.Position);
                entity.Depth = oshiro.Depth + 1;
                cs07Credits.Scene.Add(entity);
                entity.Add(new Image(GFX.Game["characters/oshiro/broom"])
                {
                    Origin = oshiro.Sprite.Origin
                });
                oshiro.Sprite.OnFrameChange = null;
            };
            bird.Sprite.Play("idle");
            yield return 0.5f;
            bird.Sprite.Play("croak");
            yield return 0.6f;
            from = new Vector2();
            to = new Vector2();
            oshiro.Maxspeed = 40f;
            oshiro.MoveAnim = "move";
            oshiro.IdleAnim = "idle";
            yield return oshiro.MoveTo(oshiroTarget + new Vector2(14f, 0.0f));
            yield return 2f;
            cs07Credits1.Add(new Coroutine(bird.StartleAndFlyAway()));
            yield return 0.75f;
            bird.Light.Visible = false;
            bird.Depth = 10010;
            oshiro.Sprite.Scale.X = -1f;
            yield return cs07Credits1.FadeTo(1f);
            oshiroTarget = new Vector2();
            oshiroRoutine = null;
            bird = null;
            yield return 1f;
            cs07Credits1.SetBgFade(0.6f);
            yield return cs07Credits1.NextLevel("credits-wallslide");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.badeline.Floatness = 0.0f;
            cs07Credits1.badeline.Sprite.Play("idle");
            cs07Credits1.badeline.Sprite.Scale.X = 1f;
            foreach (CreditsTrigger entity in cs07Credits1.Scene.Tracker.GetEntities<CreditsTrigger>())
            {
                if (entity.Event == "BadelineOffset")
                    cs07Credits1.badeline.Position = entity.Position + new Vector2(8f, 16f);
            }
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            cs07Credits1.Add(new Coroutine(cs07Credits1.WaitForPlayer()));
            while (cs07Credits1.player.X > cs07Credits1.badeline.X - 16.0)
                yield return null;
            cs07Credits1.badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badelineWalkApproachFrom = cs07Credits1.badeline.Position;
            cs07Credits1.badelineWalkApproach = 0.0f;
            cs07Credits1.badeline.Sprite.Play("walk");
            while (cs07Credits1.badelineWalkApproach != 1.0)
            {
                cs07Credits1.badelineWalkApproach = Calc.Approach(cs07Credits1.badelineWalkApproach, 1f, Engine.DeltaTime * 4f);
                yield return null;
            }
            while (cs07Credits1.player.X > (double) (cs07Credits1.Level.Bounds.X + 160))
                yield return null;
            yield return cs07Credits1.FadeTo(1f);
            yield return 1f;
            cs07Credits1.SetBgFade(0.7f);
            yield return cs07Credits1.NextLevel("credits-payphone");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.player.Speed = Vector2.Zero;
            cs07Credits1.player.Facing = Facings.Left;
            cs07Credits1.autoWalk = false;
            cs07Credits1.badeline.Sprite.Play("idle");
            cs07Credits1.badeline.Floatness = 0.0f;
            cs07Credits1.badeline.Y = cs07Credits1.player.Y;
            cs07Credits1.badeline.Sprite.Scale.X = 1f;
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.autoUpdateCamera = false;
            cs07Credits1.Level.Camera.X += 100f;
            Vector2 target3 = cs07Credits1.Level.Camera.Position + new Vector2(-200f, 0.0f);
            cs07Credits1.Add(new Coroutine(CutsceneEntity.CameraTo(target3, 14f, Ease.Linear)));
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return 1.5f;
            cs07Credits1.badeline.Sprite.Scale.X = -1f;
            yield return 0.5f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.badeline.FloatTo(cs07Credits1.badeline.Position + new Vector2(16f, -12f), -1, false)));
            yield return 0.5f;
            cs07Credits1.player.Facing = Facings.Right;
            yield return 1.5f;
            oshiroTarget = cs07Credits1.badeline.Position;
            to = cs07Credits1.player.Center;
            cs07Credits1.Add(new Coroutine(cs07Credits1.BadelineAround(oshiroTarget, to, cs07Credits1.badeline)));
            yield return 0.5f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.BadelineAround(oshiroTarget, to)));
            yield return 0.5f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.BadelineAround(oshiroTarget, to)));
            yield return 3f;
            cs07Credits1.badeline.Sprite.Play("laugh");
            yield return 0.5f;
            cs07Credits1.player.Facing = Facings.Left;
            yield return 0.5f;
            cs07Credits1.player.DummyAutoAnimate = false;
            cs07Credits1.player.Sprite.Play("sitDown");
            yield return 3f;
            yield return cs07Credits1.FadeTo(1f);
            oshiroTarget = new Vector2();
            to = new Vector2();
            yield return 1f;
            cs07Credits1.SetBgFade(0.8f);
            yield return cs07Credits1.NextLevel("credits-city");
            yield return cs07Credits1.SetupLevel();
            BirdNPC first = cs07Credits1.Scene.Entities.FindFirst<BirdNPC>();
            if (first != null)
                first.Facing = Facings.Right;
            cs07Credits1.badelineWalkApproach = 1f;
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badeline.Floatness = 0.0f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return cs07Credits1.WaitForPlayer();
            yield return cs07Credits1.FadeTo(1f);
            yield return 1f;
            cs07Credits1.SetBgFade(0.0f);
            yield return cs07Credits1.NextLevel("credits-prologue");
            yield return cs07Credits1.SetupLevel();
            cs07Credits1.badelineWalkApproach = 1f;
            cs07Credits1.badelineAutoFloat = false;
            cs07Credits1.badelineAutoWalk = true;
            cs07Credits1.badeline.Floatness = 0.0f;
            cs07Credits1.Add(new Coroutine(cs07Credits1.FadeTo(0.0f)));
            yield return cs07Credits1.WaitForPlayer();
            yield return cs07Credits1.FadeTo(1f);
            while (cs07Credits1.credits.BottomTimer < 2.0)
                yield return null;
            if (!cs07Credits1.gotoEpilogue)
            {
                cs07Credits1.snow = new HiresSnow();
                cs07Credits1.snow.Alpha = 0.0f;
                // ISSUE: reference to a compiler-generated method
                cs07Credits1.snow.AttachAlphaTo = new FadeWipe(Level, false, delegate
                {
                    EndCutscene(Level);
                });
                cs07Credits1.Level.Add(cs07Credits1.Level.HiresSnow = cs07Credits1.snow);
            }
            else
            {
                // ISSUE: reference to a compiler-generated method
                FadeWipe fadeWipe = new FadeWipe(Level, false, delegate
                {
                        EndCutscene(Level);
                });
            }
        }

        private IEnumerator SetupLevel()
        {
            CS07_Credits cs07Credits = this;
            cs07Credits.Level.SnapColorGrade("credits");
            cs07Credits.player = null;
            while ((cs07Credits.player = cs07Credits.Scene.Tracker.GetEntity<Player>()) == null)
                yield return null;
            cs07Credits.Level.Add(cs07Credits.badeline = new BadelineDummy(cs07Credits.player.Position + new Vector2(16f, -16f)));
            cs07Credits.badeline.Floatness = 4f;
            cs07Credits.badelineAutoFloat = true;
            cs07Credits.badelineAutoWalk = false;
            cs07Credits.badelineWalkApproach = 0.0f;
            cs07Credits.Level.Session.Inventory.Dashes = 1;
            cs07Credits.player.Dashes = 1;
            cs07Credits.player.StateMachine.State = 11;
            cs07Credits.player.DummyFriction = false;
            cs07Credits.player.DummyMaxspeed = false;
            cs07Credits.player.Facing = Facings.Left;
            cs07Credits.autoWalk = true;
            cs07Credits.autoUpdateCamera = true;
            cs07Credits.Level.CameraOffset.X = 70f;
            cs07Credits.Level.CameraOffset.Y = -24f;
            cs07Credits.Level.Camera.Position = cs07Credits.player.CameraTarget;
        }

        private IEnumerator WaitForPlayer()
        {
            CS07_Credits cs07Credits = this;
            while (cs07Credits.player.X > (double) (cs07Credits.Level.Bounds.X + 160))
            {
                if (cs07Credits.Event != null)
                    yield return cs07Credits.DoEvent(cs07Credits.Event);
                cs07Credits.Event = null;
                yield return null;
            }
        }

        private IEnumerator NextLevel(string name)
        {
            CS07_Credits cs07Credits = this;
            if (cs07Credits.player != null)
                cs07Credits.player.RemoveSelf();
            cs07Credits.player = null;
            cs07Credits.Level.OnEndOfFrame += () =>
            {
                Level.UnloadLevel();
                Level.Session.Level = name;
                Session session = Level.Session;
                Level level = Level;
                Rectangle bounds = Level.Bounds;
                double left = bounds.Left;
                bounds = Level.Bounds;
                double top = bounds.Top;
                Vector2 from = new Vector2((float) left, (float) top);
                Vector2? nullable = level.GetSpawnPoint(from);
                session.RespawnPoint = nullable;
                Level.LoadLevel(Player.IntroTypes.None);
                Level.Wipe.Cancel();
            };
            yield return null;
            yield return null;
        }

        private IEnumerator FadeTo(float value)
        {
            while ((fade = Calc.Approach(fade, value, Engine.DeltaTime * 0.5f)) != (double) value)
                yield return null;
            fade = value;
        }

        private IEnumerator BadelineApproachWalking()
        {
            while (badelineWalkApproach < 1.0)
            {
                badeline.Floatness = Calc.Approach(badeline.Floatness, 0.0f, Engine.DeltaTime * 8f);
                badelineWalkApproach = Calc.Approach(badelineWalkApproach, 1f, Engine.DeltaTime * 0.6f);
                yield return null;
            }
        }

        private IEnumerator DustyRoutine(Entity oshiro)
        {
            CS07_Credits cs07Credits = this;
            List<Entity> dusty = new List<Entity>();
            float timer = 0.0f;
            Vector2 offset = oshiro.Position + new Vector2(220f, -24f);
            Vector2 start = offset;
            for (int index = 0; index < 3; ++index)
            {
                Entity entity = new Entity(offset + new Vector2(index * 24, 0.0f))
                {
                    Depth = -50
                };
                entity.Add(new DustGraphic(true, autoExpandDust: true));
                Image image = new Image(GFX.Game["decals/3-resort/brokenbox_" + ((char) (97 + index))]);
                image.JustifyOrigin(0.5f, 1f);
                image.Position = new Vector2(0.0f, -4f);
                entity.Add(image);
                cs07Credits.Scene.Add(entity);
                dusty.Add(entity);
            }
            yield return 3.8f;
            while (true)
            {
                for (int index = 0; index < dusty.Count; ++index)
                {
                    Entity entity = dusty[index];
                    entity.X = offset.X + index * 24;
                    entity.Y = offset.Y + (float) Math.Sin(timer * 4.0 + index * 0.800000011920929) * 4f;
                }
                if (offset.X < (double) (cs07Credits.Level.Bounds.Left + 120))
                    offset.Y = Calc.Approach(offset.Y, start.Y + 16f, Engine.DeltaTime * 16f);
                offset.X -= 26f * Engine.DeltaTime;
                timer += Engine.DeltaTime;
                yield return null;
            }
        }

        private IEnumerator BadelineAround(
            Vector2 start,
            Vector2 around,
            BadelineDummy badeline = null)
        {
            CS07_Credits cs07Credits = this;
            bool removeAtEnd = badeline == null;
            if (badeline == null)
                cs07Credits.Scene.Add(badeline = new BadelineDummy(start));
            badeline.Sprite.Play("fallSlow");
            float angle = Calc.Angle(around, start);
            float dist = (around - start).Length();
            float duration = 3f;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / duration)
            {
                badeline.Position = around + Calc.AngleToVector(angle - p * 2f * 6.28318548f, (float) (dist + Calc.YoYo(p) * 16.0 + Math.Sin(p * 6.2831854820251465 * 4.0) * 5.0));
                badeline.Sprite.Scale.X = Math.Sign(around.X - badeline.X);
                if (!removeAtEnd)
                    cs07Credits.player.Facing = (Facings) Math.Sign(badeline.X - cs07Credits.player.X);
                if (cs07Credits.Scene.OnInterval(0.1f))
                    TrailManager.Add(badeline, Player.NormalHairColor);
                yield return null;
            }
            if (removeAtEnd)
                badeline.Vanish();
            else
                badeline.Sprite.Play("laugh");
        }

        private IEnumerator DoEvent(string e)
        {
            if (e == "WaitJumpDash")
                yield return EventWaitJumpDash();
            else if (e == "WaitJumpDoubleDash")
                yield return EventWaitJumpDoubleDash();
            else if (e == "ClimbDown")
                yield return EventClimbDown();
            else if (e == "Wait")
                yield return EventWait();
        }

        private IEnumerator EventWaitJumpDash()
        {
            autoWalk = false;
            player.DummyFriction = true;
            yield return 0.1f;
            PlayerJump(-1);
            yield return 0.2f;
            player.OverrideDashDirection = new Vector2(-1f, -1f);
            player.StateMachine.State = player.StartDash();
            yield return 0.6f;
            player.OverrideDashDirection = new Vector2?();
            player.StateMachine.State = 11;
            autoWalk = true;
        }

        private IEnumerator EventWaitJumpDoubleDash()
        {
            CS07_Credits cs07Credits = this;
            cs07Credits.autoWalk = false;
            cs07Credits.player.DummyFriction = true;
            yield return 0.1f;
            cs07Credits.player.Facing = Facings.Right;
            yield return 0.25f;
            yield return cs07Credits.BadelineCombine();
            cs07Credits.player.Dashes = 2;
            yield return 0.5f;
            cs07Credits.player.Facing = Facings.Left;
            yield return 0.7f;
            cs07Credits.PlayerJump(-1);
            yield return 0.4f;
            cs07Credits.player.OverrideDashDirection = new Vector2(-1f, -1f);
            cs07Credits.player.StateMachine.State = cs07Credits.player.StartDash();
            yield return 0.6f;
            cs07Credits.player.OverrideDashDirection = new Vector2(-1f, 0.0f);
            cs07Credits.player.StateMachine.State = cs07Credits.player.StartDash();
            yield return 0.6f;
            cs07Credits.player.OverrideDashDirection = new Vector2?();
            cs07Credits.player.StateMachine.State = 11;
            cs07Credits.autoWalk = true;
            while (!cs07Credits.player.OnGround())
                yield return null;
            cs07Credits.autoWalk = false;
            cs07Credits.player.DummyFriction = true;
            cs07Credits.player.Dashes = 2;
            yield return 0.5f;
            cs07Credits.player.Facing = Facings.Right;
            yield return 1f;
            cs07Credits.Level.Displacement.AddBurst(cs07Credits.player.Position, 0.4f, 8f, 32f, 0.5f);
            cs07Credits.badeline.Position = cs07Credits.player.Position;
            cs07Credits.badeline.Visible = true;
            cs07Credits.badelineAutoFloat = true;
            cs07Credits.player.Dashes = 1;
            yield return 0.8f;
            cs07Credits.player.Facing = Facings.Left;
            cs07Credits.autoWalk = true;
            cs07Credits.player.DummyFriction = false;
        }

        private IEnumerator EventClimbDown()
        {
            autoWalk = false;
            player.DummyFriction = true;
            yield return 0.1f;
            PlayerJump(-1);
            yield return 0.4f;
            while (!player.CollideCheck<Solid>(player.Position + new Vector2(-1f, 0.0f)))
                yield return null;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("wallslide");
            while (player.CollideCheck<Solid>(player.Position + new Vector2(-1f, 32f)))
            {
                player.CreateWallSlideParticles(-1);
                player.Speed.Y = Math.Min(player.Speed.Y, 40f);
                yield return null;
            }
            PlayerJump(1);
            yield return 0.4f;
            while (!player.CollideCheck<Solid>(player.Position + new Vector2(1f, 0.0f)))
                yield return null;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("wallslide");
            while (!player.CollideCheck<Solid>(player.Position + new Vector2(0.0f, 32f)))
            {
                player.CreateWallSlideParticles(1);
                player.Speed.Y = Math.Min(player.Speed.Y, 40f);
                yield return null;
            }
            PlayerJump(-1);
            yield return 0.4f;
            autoWalk = true;
        }

        private IEnumerator EventWait()
        {
            CS07_Credits cs07Credits = this;
            cs07Credits.badeline.Sprite.Play("idle");
            cs07Credits.badelineAutoWalk = false;
            cs07Credits.autoWalk = false;
            cs07Credits.player.DummyFriction = true;
            yield return 0.1f;
            cs07Credits.player.DummyAutoAnimate = false;
            cs07Credits.player.Speed = Vector2.Zero;
            yield return 0.5f;
            cs07Credits.player.Sprite.Play("lookUp");
            yield return 2f;
            BirdNPC first = cs07Credits.Scene.Entities.FindFirst<BirdNPC>();
            if (first != null)
                first.AutoFly = true;
            yield return 0.1f;
            cs07Credits.player.Sprite.Play("idle");
            yield return 1f;
            cs07Credits.autoWalk = true;
            cs07Credits.player.DummyFriction = false;
            cs07Credits.player.DummyAutoAnimate = true;
            cs07Credits.badelineAutoWalk = true;
            cs07Credits.badelineWalkApproach = 0.0f;
            cs07Credits.badelineWalkApproachFrom = cs07Credits.badeline.Position;
            cs07Credits.badeline.Sprite.Play("walk");
            while (cs07Credits.badelineWalkApproach < 1.0)
            {
                cs07Credits.badelineWalkApproach += Engine.DeltaTime * 4f;
                yield return null;
            }
        }

        private IEnumerator BadelineCombine()
        {
            CS07_Credits cs07Credits = this;
            Vector2 from = cs07Credits.badeline.Position;
            cs07Credits.badelineAutoFloat = false;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.25f)
            {
                cs07Credits.badeline.Position = Vector2.Lerp(from, cs07Credits.player.Position, Ease.CubeIn(p));
                yield return null;
            }
            cs07Credits.badeline.Visible = false;
            cs07Credits.Level.Displacement.AddBurst(cs07Credits.player.Position, 0.4f, 8f, 32f, 0.5f);
        }

        private void PlayerJump(int direction)
        {
            player.Facing = (Facings) direction;
            player.DummyFriction = false;
            player.DummyAutoAnimate = true;
            player.Speed.X = direction * 120;
            player.Jump();
            player.AutoJump = true;
            player.AutoJumpTimer = 2f;
        }

        private void SetBgFade(float alpha) => fillbg.Color = Color.Black * alpha;

        public override void Update()
        {
            MInput.Disabled = false;
            if (Level.CanPause && (Input.Pause.Pressed || Input.ESC.Pressed))
            {
                Input.Pause.ConsumeBuffer();
                Input.ESC.ConsumeBuffer();
                Level.Pause(minimal: true);
            }
            MInput.Disabled = true;
            if (player != null && player.Scene != null)
            {
                if (player.OverrideDashDirection.HasValue)
                {
                    Input.MoveX.Value = (int) player.OverrideDashDirection.Value.X;
                    Input.MoveY.Value = (int) player.OverrideDashDirection.Value.Y;
                }
                if (autoWalk)
                {
                    if (player.OnGround())
                    {
                        player.Speed.X = -44.8f;
                        bool flag1 = player.CollideCheck<Solid>(player.Position + new Vector2(-20f, 0.0f));
                        bool flag2 = !player.CollideCheck<Solid>(player.Position + new Vector2(-8f, 1f)) && !player.CollideCheck<Solid>(player.Position + new Vector2(-8f, 32f));
                        if (flag1 | flag2)
                        {
                            player.Jump();
                            player.AutoJump = true;
                            player.AutoJumpTimer = flag1 ? 0.6f : 2f;
                        }
                    }
                    else
                        player.Speed.X = -64f;
                }
                if (badeline != null && badelineAutoFloat)
                {
                    Vector2 position = badeline.Position;
                    Vector2 vector2 = player.Position + new Vector2(16f, -16f);
                    badeline.Position = position + (vector2 - position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                    badeline.Sprite.Scale.X = -1f;
                }
                if (badeline != null && badelineAutoWalk)
                {
                    Player.ChaserState chaseState;
                    player.GetChasePosition(Scene.TimeActive, (float) (0.34999999403953552 + Math.Sin(walkOffset) * 0.10000000149011612), out chaseState);
                    if (chaseState.OnGround)
                        walkOffset += Engine.DeltaTime;
                    if (badelineWalkApproach >= 1.0)
                    {
                        badeline.Position = chaseState.Position;
                        if (badeline.Sprite.Has(chaseState.Animation))
                            badeline.Sprite.Play(chaseState.Animation);
                        badeline.Sprite.Scale.X = (float) chaseState.Facing;
                    }
                    else
                        badeline.Position = Vector2.Lerp(badelineWalkApproachFrom, chaseState.Position, badelineWalkApproach);
                }
                if (Math.Abs(player.Speed.X) > 90.0)
                    player.Speed.X = Calc.Approach(player.Speed.X, 90f * Math.Sign(player.Speed.X), 1000f * Engine.DeltaTime);
            }
            if (credits != null)
                credits.Update();
            base.Update();
        }

        public void PostUpdate()
        {
            if (player == null || player.Scene == null || !autoUpdateCamera)
                return;
            Vector2 position = Level.Camera.Position;
            Vector2 cameraTarget = player.CameraTarget;
            if (!player.OnGround())
                cameraTarget.Y = (float) ((Level.Camera.Y * 2.0 + cameraTarget.Y) / 3.0);
            Level.Camera.Position = position + (cameraTarget - position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
            Level.Camera.X = (int) cameraTarget.X;
        }

        public override void Render()
        {
            bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
            if (!Level.Paused)
            {
                if (flag)
                    gradient.Draw(new Vector2(1720f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(-1f, 1100f));
                else
                    gradient.Draw(new Vector2(200f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(1f, 1100f));
            }
            if (fade > 0.0)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeInOut(fade));
            if (credits != null && !Level.Paused)
                credits.Render(new Vector2(flag ? 100f : 1820f, 0.0f));
            base.Render();
        }

        public override void OnEnd(Level level)
        {
            SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
            Audio.BusMuted("bus:/gameplay_sfx", false);
            CS07_Credits.Instance = null;
            MInput.Disabled = false;
            if (!gotoEpilogue)
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, snow);
            else
                LevelEnter.Go(new Session(new AreaKey(8)), false);
        }

        private class Fill : Backdrop
        {
            public override void Render(Scene scene) => Draw.Rect(-10f, -10f, 340f, 200f, Color);
        }
    }
}
