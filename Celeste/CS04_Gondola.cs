﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS04_Gondola : CutsceneEntity
    {
        private NPC theo;
        private Gondola gondola;
        private Player player;
        private BadelineDummy evil;
        private Parallax loopingCloud;
        private Parallax bottomCloud;
        private WindSnowFG windSnowFg;
        private float LoopCloudsAt;
        private List<ReflectionTentacles> tentacles = new List<ReflectionTentacles>();
        private SoundSource moveLoopSfx;
        private SoundSource haltLoopSfx;
        private float gondolaPercent;
        private bool AutoSnapCharacters;
        private float theoXOffset;
        private float playerXOffset;
        private float gondolaSpeed;
        private float shakeTimer;
        private const float gondolaMaxSpeed = 64f;
        private float anxiety;
        private float anxietyStutter;
        private float anxietyRumble;
        private BreathingRumbler rumbler;
        private GondolaStates gondolaState;

        public CS04_Gondola(NPC theo, Gondola gondola, Player player)
            : base(false, true)
        {
            this.theo = theo;
            this.gondola = gondola;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            foreach (Backdrop backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is WindSnowFG)
                    windSnowFg = backdrop as WindSnowFG;
            }
            Add(moveLoopSfx = new SoundSource());
            Add(haltLoopSfx = new SoundSource());
            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator Cutscene()
        {
            CS04_Gondola cs04Gondola = this;
            cs04Gondola.player.StateMachine.State = 11;
            yield return cs04Gondola.player.DummyWalkToExact((int) cs04Gondola.gondola.X + 16);
            while (!cs04Gondola.player.OnGround())
                yield return null;
            Audio.SetMusic("event:/music/lvl1/theo");
            yield return Textbox.Say("CH4_GONDOLA", cs04Gondola.EnterTheo, cs04Gondola.CheckOnTheo, cs04Gondola.GetUpTheo, cs04Gondola.LookAtLever, cs04Gondola.PullLever, cs04Gondola.WaitABit, cs04Gondola.WaitForCenter, cs04Gondola.SelfieThenStallsOut, cs04Gondola.MovePlayerLeft, cs04Gondola.SnapLeverOff, cs04Gondola.DarknessAppears, cs04Gondola.DarknessConsumes, cs04Gondola.CantBreath, cs04Gondola.StartBreathing, cs04Gondola.Ascend, cs04Gondola.WaitABit, cs04Gondola.TheoTakesOutPhone, cs04Gondola.FaceTheo);
            yield return cs04Gondola.ShowPhoto();
            cs04Gondola.EndCutscene(cs04Gondola.Level);
        }

        public override void OnEnd(Level level)
        {
            if (rumbler != null)
            {
                rumbler.RemoveSelf();
                rumbler = null;
            }
            level.CompleteArea();
            if (WasSkipped)
                return;
            SpotlightWipe.Modifier = 120f;
            SpotlightWipe.FocusPoint = new Vector2(320f, 180f) / 2f;
        }

        private IEnumerator EnterTheo()
        {
            CS04_Gondola cs04Gondola = this;
            cs04Gondola.player.Facing = Facings.Left;
            yield return 0.2f;
            yield return cs04Gondola.PanCamera(new Vector2(cs04Gondola.Level.Bounds.Left, cs04Gondola.theo.Y - 90f), 1f);
            cs04Gondola.theo.Visible = true;
            float theoStartX = cs04Gondola.theo.X;
            yield return cs04Gondola.theo.MoveTo(new Vector2(theoStartX + 35f, cs04Gondola.theo.Y));
            yield return 0.6f;
            yield return cs04Gondola.theo.MoveTo(new Vector2(theoStartX + 60f, cs04Gondola.theo.Y));
            Audio.Play("event:/game/04_cliffside/gondola_theo_fall", cs04Gondola.theo.Position);
            cs04Gondola.theo.Sprite.Play("idleEdge");
            yield return 1f;
            cs04Gondola.theo.Sprite.Play("falling");
            cs04Gondola.theo.X += 4f;
            cs04Gondola.theo.Depth = -10010;
            float speed = 80f;
            while (cs04Gondola.theo.Y < (double) cs04Gondola.player.Y)
            {
                cs04Gondola.theo.Y += speed * Engine.DeltaTime;
                speed += 120f * Engine.DeltaTime;
                yield return null;
            }
            cs04Gondola.Level.DirectionalShake(new Vector2(0.0f, 1f));
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            cs04Gondola.theo.Y = cs04Gondola.player.Y;
            cs04Gondola.theo.Sprite.Play("hitGround");
            cs04Gondola.theo.Sprite.Rate = 0.0f;
            cs04Gondola.theo.Depth = 1000;
            cs04Gondola.theo.Sprite.Scale = new Vector2(1.3f, 0.8f);
            yield return 0.5f;
            Vector2 start = cs04Gondola.theo.Sprite.Scale;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 2f, start: true);
            tween.OnUpdate = t =>
            {
                theo.Sprite.Scale.X = MathHelper.Lerp(start.X, 1f, t.Eased);
                theo.Sprite.Scale.Y = MathHelper.Lerp(start.Y, 1f, t.Eased);
            };
            cs04Gondola.Add(tween);
            yield return cs04Gondola.PanCamera(new Vector2(cs04Gondola.Level.Bounds.Left, cs04Gondola.theo.Y - 120f), 1f);
            yield return 0.6f;
        }

        private IEnumerator CheckOnTheo()
        {
            yield return player.DummyWalkTo(gondola.X - 18f);
        }

        private IEnumerator GetUpTheo()
        {
            yield return 1.4f;
            Audio.Play("event:/game/04_cliffside/gondola_theo_recover", theo.Position);
            theo.Sprite.Rate = 1f;
            theo.Sprite.Play("recoverGround");
            yield return 1.6f;
            yield return theo.MoveTo(new Vector2(gondola.X - 50f, player.Y));
            yield return 0.2f;
        }

        private IEnumerator LookAtLever()
        {
            yield return theo.MoveTo(new Vector2(gondola.X + 7f, theo.Y));
            player.Facing = Facings.Right;
            theo.Sprite.Scale.X = -1f;
        }

        private IEnumerator PullLever()
        {
            CS04_Gondola cs04Gondola = this;
            cs04Gondola.Add(new Coroutine(cs04Gondola.player.DummyWalkToExact((int) cs04Gondola.gondola.X - 7)));
            cs04Gondola.theo.Sprite.Scale.X = -1f;
            yield return 0.2f;
            Audio.Play("event:/game/04_cliffside/gondola_theo_lever_start", cs04Gondola.theo.Position);
            cs04Gondola.theo.Sprite.Play("pullVent");
            yield return 1f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            cs04Gondola.gondola.Lever.Play("pulled");
            cs04Gondola.theo.Sprite.Play("fallVent");
            yield return 0.6f;
            cs04Gondola.Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            yield return 0.5f;
            yield return cs04Gondola.PanCamera(cs04Gondola.gondola.Position + new Vector2(-160f, -120f), 1f);
            yield return 0.5f;
            cs04Gondola.Level.Background.Backdrops.Add(cs04Gondola.loopingCloud = new Parallax(GFX.Game["bgs/04/bgCloudLoop"]));
            cs04Gondola.Level.Background.Backdrops.Add(cs04Gondola.bottomCloud = new Parallax(GFX.Game["bgs/04/bgCloud"]));
            cs04Gondola.loopingCloud.LoopX = cs04Gondola.bottomCloud.LoopX = true;
            cs04Gondola.loopingCloud.LoopY = cs04Gondola.bottomCloud.LoopY = false;
            cs04Gondola.loopingCloud.Position.Y = cs04Gondola.Level.Camera.Top - cs04Gondola.loopingCloud.Texture.Height - cs04Gondola.bottomCloud.Texture.Height;
            cs04Gondola.bottomCloud.Position.Y = cs04Gondola.Level.Camera.Top - cs04Gondola.bottomCloud.Texture.Height;
            cs04Gondola.LoopCloudsAt = cs04Gondola.bottomCloud.Position.Y;
            cs04Gondola.AutoSnapCharacters = true;
            cs04Gondola.theoXOffset = cs04Gondola.theo.X - cs04Gondola.gondola.X;
            cs04Gondola.playerXOffset = cs04Gondola.player.X - cs04Gondola.gondola.X;
            cs04Gondola.player.StateMachine.State = 17;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 16f, start: true);
            tween.OnUpdate = t =>
            {
                if (!(Audio.CurrentMusic == "event:/music/lvl1/theo"))
                    return;
                Audio.SetMusicParam("fade", 1f - t.Eased);
            };
            cs04Gondola.Add(tween);
            SoundSource soundSource = new SoundSource();
            soundSource.Position = cs04Gondola.gondola.LeftCliffside.Position;
            soundSource.Play("event:/game/04_cliffside/gondola_cliffmechanism_start");
            cs04Gondola.Add(soundSource);
            cs04Gondola.moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
            cs04Gondola.Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
            cs04Gondola.gondolaSpeed = 32f;
            cs04Gondola.gondola.RotationSpeed = 1f;
            cs04Gondola.gondolaState = GondolaStates.MovingToCenter;
            yield return 1f;
            yield return cs04Gondola.MoveTheoOnGondola(12f, false);
            yield return 0.2f;
            cs04Gondola.theo.Sprite.Scale.X = -1f;
        }

        private IEnumerator WaitABit()
        {
            yield return 1f;
        }

        private IEnumerator WaitForCenter()
        {
            while (gondolaState != GondolaStates.InCenter)
                yield return null;
            theo.Sprite.Scale.X = 1f;
            yield return 1f;
            yield return MovePlayerOnGondola(-20f);
            yield return 0.5f;
        }

        private IEnumerator SelfieThenStallsOut()
        {
            CS04_Gondola cs04Gondola = this;
            Audio.SetMusic("event:/music/lvl4/minigame");
            cs04Gondola.Add(new Coroutine(cs04Gondola.Level.ZoomTo(new Vector2(160f, 110f), 2f, 0.5f)));
            yield return 0.3f;
            cs04Gondola.theo.Sprite.Scale.X = 1f;
            yield return 0.2f;
            cs04Gondola.Add(new Coroutine(cs04Gondola.MovePlayerOnGondola(cs04Gondola.theoXOffset - 8f)));
            yield return 0.4f;
            Audio.Play("event:/game/04_cliffside/gondola_theoselfie_halt", cs04Gondola.theo.Position);
            cs04Gondola.theo.Sprite.Play("holdOutPhone");
            yield return 1.5f;
            cs04Gondola.theoXOffset += 4f;
            cs04Gondola.playerXOffset += 4f;
            cs04Gondola.gondola.RotationSpeed = -1f;
            cs04Gondola.gondolaState = GondolaStates.Stopped;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            cs04Gondola.theo.Sprite.Play("takeSelfieImmediate");
            cs04Gondola.Add(new Coroutine(cs04Gondola.PanCamera(cs04Gondola.gondola.Position + (cs04Gondola.gondola.Destination - cs04Gondola.gondola.Position).SafeNormalize() * 32f + new Vector2(-160f, -120f), 0.3f, Ease.CubeOut)));
            yield return 0.5f;
            cs04Gondola.Level.Flash(Color.White);
            cs04Gondola.Level.Add(cs04Gondola.evil = new BadelineDummy(Vector2.Zero));
            cs04Gondola.evil.Appear(cs04Gondola.Level);
            cs04Gondola.evil.Floatness = 0.0f;
            cs04Gondola.evil.Depth = -1000000;
            cs04Gondola.moveLoopSfx.Stop();
            cs04Gondola.haltLoopSfx.Play("event:/game/04_cliffside/gondola_halted_loop");
            cs04Gondola.gondolaState = GondolaStates.Shaking;
            yield return cs04Gondola.PanCamera(cs04Gondola.gondola.Position + new Vector2(-160f, -120f), 1f);
            yield return 1f;
        }

        private IEnumerator MovePlayerLeft()
        {
            yield return MovePlayerOnGondola(-20f);
            theo.Sprite.Scale.X = -1f;
            yield return 0.5f;
            yield return MovePlayerOnGondola(20f);
            yield return 0.5f;
            yield return MovePlayerOnGondola(-10f);
            yield return 0.5f;
            player.Facing = Facings.Right;
        }

        private IEnumerator SnapLeverOff()
        {
            CS04_Gondola cs04Gondola = this;
            yield return cs04Gondola.MoveTheoOnGondola(7f);
            Audio.Play("event:/game/04_cliffside/gondola_theo_lever_fail", cs04Gondola.theo.Position);
            cs04Gondola.theo.Sprite.Play("pullVent");
            yield return 1f;
            cs04Gondola.theo.Sprite.Play("fallVent");
            yield return 1f;
            cs04Gondola.gondola.BreakLever();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            cs04Gondola.Level.Shake();
            yield return 2.5f;
        }

        private IEnumerator DarknessAppears()
        {
            CS04_Gondola cs04Gondola = this;
            Audio.SetMusicParam("calm", 0.0f);
            yield return 0.25f;
            cs04Gondola.player.Sprite.Play("tired");
            yield return 0.25f;
            cs04Gondola.evil.Vanish();
            cs04Gondola.evil = null;
            yield return 0.3f;
            cs04Gondola.Level.NextColorGrade("panicattack");
            cs04Gondola.Level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            cs04Gondola.BurstTentacles(3, 90f);
            Audio.Play("event:/game/04_cliffside/gondola_scaryhair_01", cs04Gondola.gondola.Position);
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 2f)
            {
                yield return null;
                cs04Gondola.Level.Background.Fade = p;
                cs04Gondola.anxiety = p;
                if (cs04Gondola.windSnowFg != null)
                    cs04Gondola.windSnowFg.Alpha = 1f - p;
            }
            yield return 0.25f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator DarknessConsumes()
        {
                Level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/04_cliffside/gondola_scaryhair_02", gondola.Position);
                BurstTentacles(2, 60f);
                yield return MoveTheoOnGondola(0f);
                theo.Sprite.Play("comfortStart");
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator CantBreath()
        {
                Level.Shake();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("event:/game/04_cliffside/gondola_scaryhair_03", gondola.Position);
                BurstTentacles(1, 30f);
                BurstTentacles(0, 0f, 100f);
                rumbler = new BreathingRumbler();
                Scene.Add(rumbler);
                yield return null;
        }

        private IEnumerator StartBreathing()
        {
            CS04_Gondola cs04Gondola = this;
            BreathingMinigame breathing = new BreathingMinigame(rumbler: cs04Gondola.rumbler);
            cs04Gondola.Scene.Add(breathing);
            while (!breathing.Completed)
                yield return null;
            foreach (Entity tentacle in cs04Gondola.tentacles)
                tentacle.RemoveSelf();
            cs04Gondola.anxiety = 0.0f;
            cs04Gondola.Level.Background.Fade = 0.0f;
            cs04Gondola.Level.SnapColorGrade(null);
            cs04Gondola.gondola.CancelPullSides();
            cs04Gondola.Level.ResetZoom();
            yield return 0.5f;
            Audio.Play("event:/game/04_cliffside/gondola_restart", cs04Gondola.gondola.Position);
            yield return 1f;
            cs04Gondola.moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
            cs04Gondola.haltLoopSfx.Stop();
            cs04Gondola.Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            cs04Gondola.gondolaState = GondolaStates.InCenter;
            cs04Gondola.gondola.RotationSpeed = 0.5f;
            yield return 1.2f;
        }

        private IEnumerator Ascend()
        {
            CS04_Gondola cs04Gondola = this;
            cs04Gondola.gondolaState = GondolaStates.MovingToEnd;
            while (cs04Gondola.gondolaState != GondolaStates.Stopped)
                yield return null;
            cs04Gondola.Level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            cs04Gondola.moveLoopSfx.Stop();
            Audio.Play("event:/game/04_cliffside/gondola_finish", cs04Gondola.gondola.Position);
            cs04Gondola.gondola.RotationSpeed = 0.5f;
            yield return 0.1f;
            while (cs04Gondola.gondola.Rotation > 0.0)
                yield return null;
            cs04Gondola.gondola.Rotation = cs04Gondola.gondola.RotationSpeed = 0.0f;
            cs04Gondola.Level.Shake();
            cs04Gondola.AutoSnapCharacters = false;
            cs04Gondola.player.StateMachine.State = 11;
            cs04Gondola.player.Position = cs04Gondola.player.Position.Floor();
            while (cs04Gondola.player.CollideCheck<Solid>())
                --cs04Gondola.player.Y;
            cs04Gondola.theo.Position.Y = cs04Gondola.player.Position.Y;
            cs04Gondola.theo.Sprite.Play("comfortRecover");
            cs04Gondola.theo.Sprite.Scale.X = 1f;
            yield return cs04Gondola.player.DummyWalkTo(cs04Gondola.gondola.X + 80f);
            cs04Gondola.player.DummyAutoAnimate = false;
            cs04Gondola.player.Sprite.Play("tired");
            yield return cs04Gondola.theo.MoveTo(new Vector2(cs04Gondola.gondola.X + 64f, cs04Gondola.theo.Y));
            yield return 0.5f;
        }

        private IEnumerator TheoTakesOutPhone()
        {
            player.Facing = Facings.Right;
            yield return 0.25f;
            theo.Sprite.Play("usePhone");
            yield return 2f;
        }

        private IEnumerator FaceTheo()
        {
            player.DummyAutoAnimate = true;
            yield return 0.2f;
            player.Facing = Facings.Left;
            yield return 0.2f;
        }

        private IEnumerator ShowPhoto()
        {
            CS04_Gondola cs04Gondola = this;
            cs04Gondola.theo.Sprite.Scale.X = -1f;
            yield return 0.25f;
            yield return cs04Gondola.player.DummyWalkTo(cs04Gondola.theo.X + 5f);
            yield return 1f;
            Selfie selfie = new Selfie(cs04Gondola.SceneAs<Level>());
            cs04Gondola.Scene.Add(selfie);
            yield return selfie.OpenRoutine("selfieGondola");
            yield return selfie.WaitForInput();
        }

        public override void Update()
        {
            base.Update();
            if (anxietyRumble > 0.0)
                Input.RumbleSpecific(anxietyRumble, 0.1f);
            if (Scene.OnInterval(0.05f))
                anxietyStutter = Calc.Random.NextFloat(0.1f);
            Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
            Distort.Anxiety = (float) (anxiety * 0.20000000298023224 + anxietyStutter * (double) anxiety);
            if (moveLoopSfx != null && gondola != null)
                moveLoopSfx.Position = gondola.Position;
            if (haltLoopSfx != null && gondola != null)
                haltLoopSfx.Position = gondola.Position;
            if (gondolaState == GondolaStates.MovingToCenter)
            {
                MoveGondolaTowards(0.5f);
                if (gondolaPercent >= 0.5)
                    gondolaState = GondolaStates.InCenter;
            }
            else if (gondolaState == GondolaStates.InCenter)
            {
                Vector2 vector2 = (gondola.Destination - gondola.Position).SafeNormalize() * gondolaSpeed;
                loopingCloud.CameraOffset.X += vector2.X * Engine.DeltaTime;
                loopingCloud.CameraOffset.Y += vector2.Y * Engine.DeltaTime;
                windSnowFg.CameraOffset = loopingCloud.CameraOffset;
                loopingCloud.LoopY = true;
            }
            else if (gondolaState != GondolaStates.Stopped)
            {
                if (gondolaState == GondolaStates.Shaking)
                {
                    Level.Wind.X = -400f;
                    if (shakeTimer <= 0.0 && (gondola.Rotation == 0.0 || gondola.Rotation < -0.25))
                    {
                        shakeTimer = 1f;
                        gondola.RotationSpeed = 0.5f;
                    }
                    shakeTimer -= Engine.DeltaTime;
                }
                else if (gondolaState == GondolaStates.MovingToEnd)
                {
                    MoveGondolaTowards(1f);
                    if (gondolaPercent >= 1.0)
                        gondolaState = GondolaStates.Stopped;
                }
            }
            if (loopingCloud != null && !loopingCloud.LoopY && Level.Camera.Bottom < (double) LoopCloudsAt)
                loopingCloud.LoopY = true;
            if (!AutoSnapCharacters)
                return;
            theo.Position = gondola.GetRotatedFloorPositionAt(theoXOffset);
            player.Position = gondola.GetRotatedFloorPositionAt(playerXOffset);
            if (evil == null)
                return;
            evil.Position = gondola.GetRotatedFloorPositionAt(-24f, 20f);
        }

        private void MoveGondolaTowards(float percent)
        {
            float num = (gondola.Start - gondola.Destination).Length();
            gondolaSpeed = Calc.Approach(gondolaSpeed, 64f, 120f * Engine.DeltaTime);
            gondolaPercent = Calc.Approach(gondolaPercent, percent, gondolaSpeed / num * Engine.DeltaTime);
            gondola.Position = (gondola.Start + (gondola.Destination - gondola.Start) * gondolaPercent).Floor();
            Level.Camera.Position = gondola.Position + new Vector2(-160f, -120f);
        }

        private IEnumerator PanCamera(Vector2 to, float duration, Ease.Easer ease = null)
        {
            CS04_Gondola cs04Gondola = this;
            if (ease == null)
                ease = Ease.CubeInOut;
            Vector2 from = cs04Gondola.Level.Camera.Position;
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / duration)
            {
                yield return null;
                cs04Gondola.Level.Camera.Position = from + (to - from) * ease(Math.Min(t, 1f));
            }
        }

        private IEnumerator MovePlayerOnGondola(float x)
        {
            player.Sprite.Play("walk");
            player.Facing = (Facings) Math.Sign(x - playerXOffset);
            while (playerXOffset != (double) x)
            {
                playerXOffset = Calc.Approach(playerXOffset, x, 48f * Engine.DeltaTime);
                yield return null;
            }
            player.Sprite.Play("idle");
        }

        private IEnumerator MoveTheoOnGondola(float x, bool changeFacing = true)
        {
            theo.Sprite.Play("walk");
            if (changeFacing)
                theo.Sprite.Scale.X = Math.Sign(x - theoXOffset);
            while (theoXOffset != (double) x)
            {
                theoXOffset = Calc.Approach(theoXOffset, x, 48f * Engine.DeltaTime);
                yield return null;
            }
            theo.Sprite.Play("idle");
        }

        private void BurstTentacles(int layer, float dist, float from = 200f)
        {
            Vector2 vector2 = Level.Camera.Position + new Vector2(160f, 90f);
            ReflectionTentacles reflectionTentacles1 = new ReflectionTentacles();
            reflectionTentacles1.Create(0.0f, 0, layer, new List<Vector2>
            {
                vector2 + new Vector2(-from, 0.0f),
                vector2 + new Vector2(-800f, 0.0f)
            });
            reflectionTentacles1.SnapTentacles();
            reflectionTentacles1.Nodes[0] = vector2 + new Vector2(-dist, 0.0f);
            ReflectionTentacles reflectionTentacles2 = new ReflectionTentacles();
            reflectionTentacles2.Create(0.0f, 0, layer, new List<Vector2>
            {
                vector2 + new Vector2(from, 0.0f),
                vector2 + new Vector2(800f, 0.0f)
            });
            reflectionTentacles2.SnapTentacles();
            reflectionTentacles2.Nodes[0] = vector2 + new Vector2(dist, 0.0f);
            tentacles.Add(reflectionTentacles1);
            tentacles.Add(reflectionTentacles2);
            Level.Add(reflectionTentacles1);
            Level.Add(reflectionTentacles2);
        }

        private enum GondolaStates
        {
            Stopped,
            MovingToCenter,
            InCenter,
            Shaking,
            MovingToEnd,
        }
    }
}
