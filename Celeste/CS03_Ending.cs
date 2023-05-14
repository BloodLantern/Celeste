// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS03_Ending : CutsceneEntity
    {
        public const string Flag = "oshiroEnding";
        private readonly ResortRoofEnding roof;
        private AngryOshiro angryOshiro;
        private readonly Player player;
        private Entity oshiro;
        private Sprite oshiroSprite;
        private EventInstance smashSfx;
        private bool smashRumble;

        public CS03_Ending(ResortRoofEnding roof, Player player)
            : base(false, true)
        {
            this.roof = roof;
            this.player = player;
            Depth = -1000000;
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            if (!smashRumble)
            {
                return;
            }

            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_Ending cs03Ending1 = this;
            cs03Ending1.player.StateMachine.State = 11;
            cs03Ending1.player.StateMachine.Locked = true;
            cs03Ending1.player.ForceCameraUpdate = false;
            cs03Ending1.Add(new Coroutine(cs03Ending1.player.DummyRunTo((float)((double)cs03Ending1.roof.X + (double)cs03Ending1.roof.Width - 32.0), true)));
            yield return null;
            cs03Ending1.player.DummyAutoAnimate = false;
            yield return 0.5f;
            cs03Ending1.angryOshiro = cs03Ending1.Scene.Entities.FindFirst<AngryOshiro>();
            cs03Ending1.Add(new Coroutine(cs03Ending1.MoveGhostTo(new Vector2(cs03Ending1.roof.X + 40f, cs03Ending1.roof.Y - 12f))));
            yield return 1f;
            cs03Ending1.player.DummyAutoAnimate = true;
            yield return level.ZoomTo(new Vector2(130f, 60f), 2f, 0.5f);
            cs03Ending1.player.Facing = Facings.Left;
            yield return 0.5f;
            yield return Textbox.Say("CH3_OSHIRO_CHASE_END", new Func<IEnumerator>(cs03Ending1.GhostSmash));
            yield return cs03Ending1.GhostSmash(0.5f, true);
            _ = Audio.SetMusic(null);
            cs03Ending1.oshiroSprite = null;
            CS03_Ending.BgFlash bgFlash = new()
            {
                Alpha = 1f
            };
            level.Add(bgFlash);
            Distort.GameRate = 0.0f;
            Sprite sprite = GFX.SpriteBank.Create("oshiro_boss_lightning");
            sprite.Position = cs03Ending1.angryOshiro.Position + new Vector2(140f, -100f);
            sprite.Rotation = Calc.Angle(sprite.Position, cs03Ending1.angryOshiro.Position + new Vector2(0.0f, 10f));
            sprite.Play("once");
            cs03Ending1.Add(sprite);
            yield return null;
            Celeste.Freeze(0.3f);
            yield return null;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            cs03Ending1.smashRumble = false;
            yield return 0.2f;
            Distort.GameRate = 1f;
            level.Flash(Color.White);
            cs03Ending1.player.DummyGravity = false;
            cs03Ending1.angryOshiro.Sprite.Play("transformBack");
            cs03Ending1.player.Sprite.Play("fall");
            cs03Ending1.roof.BeginFalling = true;
            yield return null;
            Engine.TimeRate = 0.01f;
            cs03Ending1.player.Sprite.Play("fallFast");
            cs03Ending1.player.DummyGravity = true;
            cs03Ending1.player.Speed.Y = -200f;
            cs03Ending1.player.Speed.X = 300f;
            Vector2 oshiroFallSpeed = new(-100f, -250f);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 1.5f, true);
            // ISSUE: reference to a compiler-generated method
            tween.OnUpdate = delegate (Tween t)
            {
                angryOshiro.Sprite.Rotation = t.Eased * -100f * 0.017453292f;
            };
            cs03Ending1.Add(tween);
            float t;
            for (t = 0.0f; (double)t < 2.0; t += Engine.DeltaTime)
            {
                oshiroFallSpeed.X = Calc.Approach(oshiroFallSpeed.X, 0.0f, Engine.DeltaTime * 400f);
                oshiroFallSpeed.Y += Engine.DeltaTime * 800f;
                AngryOshiro angryOshiro = cs03Ending1.angryOshiro;
                angryOshiro.Position += oshiroFallSpeed * Engine.DeltaTime;
                bgFlash.Alpha = Calc.Approach(bgFlash.Alpha, 0.0f, Engine.RawDeltaTime);
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, Engine.RawDeltaTime * 0.6f);
                yield return null;
            }
            level.DirectionalShake(new Vector2(0.0f, -1f), 0.5f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
            yield return 1f;
            bgFlash = null;
            oshiroFallSpeed = new Vector2();
            while (!cs03Ending1.player.OnGround())
            {
                _ = cs03Ending1.player.MoveV(1f);
            }

            cs03Ending1.player.DummyAutoAnimate = false;
            cs03Ending1.player.Sprite.Play("tired");
            cs03Ending1.angryOshiro.RemoveSelf();
            Scene scene = cs03Ending1.Scene;
            CS03_Ending cs03Ending2 = cs03Ending1;
            Rectangle bounds = level.Bounds;
            Entity entity1;
            Entity entity2 = entity1 = new Entity(new Vector2(bounds.Left + 110, cs03Ending1.player.Y));
            cs03Ending2.oshiro = entity1;
            Entity entity3 = entity2;
            scene.Add(entity3);
            cs03Ending1.oshiro.Add(cs03Ending1.oshiroSprite = GFX.SpriteBank.Create("oshiro"));
            cs03Ending1.oshiroSprite.Play("fall");
            cs03Ending1.oshiroSprite.Scale.X = 1f;
            cs03Ending1.oshiro.Collider = new Hitbox(8f, 8f, -4f, -8f);
            cs03Ending1.oshiro.Add(new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 16, 32));
            yield return CutsceneEntity.CameraTo(cs03Ending1.player.CameraTarget + new Vector2(0.0f, 40f), 1f, Ease.CubeOut);
            yield return 1.5f;
            _ = Audio.SetMusic("event:/music/lvl3/intro");
            yield return 3f;
            _ = Audio.Play("event:/char/oshiro/chat_get_up", cs03Ending1.oshiro.Position);
            cs03Ending1.oshiroSprite.Play("recover");
            float target = cs03Ending1.oshiro.Y + 4f;
            while ((double)cs03Ending1.oshiro.Y != (double)target)
            {
                cs03Ending1.oshiro.Y = Calc.Approach(cs03Ending1.oshiro.Y, target, 6f * Engine.DeltaTime);
                yield return null;
            }
            yield return 0.6f;
            yield return Textbox.Say("CH3_ENDING", new Func<IEnumerator>(cs03Ending1.OshiroTurns));
            cs03Ending1.Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(-80f, 0.0f), 3f)));
            yield return 0.5f;
            cs03Ending1.oshiroSprite.Scale.X = -1f;
            yield return 0.2f;
            t = 0.0f;
            cs03Ending1.oshiro.Add(new SoundSource("event:/char/oshiro/move_08_roof07_exit"));
            while ((double)cs03Ending1.oshiro.X > level.Bounds.Left - 16)
            {
                cs03Ending1.oshiro.X -= 40f * Engine.DeltaTime;
                cs03Ending1.oshiroSprite.Y = (float)Math.Sin((double)(t += Engine.DeltaTime * 2f)) * 2f;
                cs03Ending1.oshiro.CollideFirst<Door>()?.Open(cs03Ending1.oshiro.X);
                yield return null;
            }
            cs03Ending1.Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(80f, 0.0f), 2f)));
            yield return 1.2f;
            cs03Ending1.player.DummyAutoAnimate = true;
            yield return cs03Ending1.player.DummyWalkTo(cs03Ending1.player.X - 16f);
            yield return 2f;
            cs03Ending1.player.Facing = Facings.Right;
            yield return 1f;
            cs03Ending1.player.ForceCameraUpdate = false;
            cs03Ending1.player.Add(new Coroutine(cs03Ending1.RunPlayerRight()));
            cs03Ending1.EndCutscene(level);
        }

        private IEnumerator OshiroTurns()
        {
            yield return 1f;
            oshiroSprite.Scale.X = -1f;
            yield return 0.2f;
        }

        private IEnumerator MoveGhostTo(Vector2 target)
        {
            if (angryOshiro != null)
            {
                target.Y -= angryOshiro.Height / 2f;
                angryOshiro.EnterDummyMode();
                angryOshiro.Collidable = false;
                while (angryOshiro.Position != target)
                {
                    angryOshiro.Position = Calc.Approach(angryOshiro.Position, target, 64f * Engine.DeltaTime);
                    yield return null;
                }
            }
        }

        private IEnumerator GhostSmash()
        {
            yield return GhostSmash(0.0f, false);
        }

        private IEnumerator GhostSmash(float topDelay, bool final)
        {
            CS03_Ending cs03Ending = this;
            if (cs03Ending.angryOshiro != null)
            {
                cs03Ending.smashSfx = !final ? Audio.Play("event:/char/oshiro/boss_slam_first", cs03Ending.angryOshiro.Position) : Audio.Play("event:/char/oshiro/boss_slam_final", cs03Ending.angryOshiro.Position);
                float from = cs03Ending.angryOshiro.Y;
                float to = cs03Ending.angryOshiro.Y - 32f;
                float p;
                for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 2f)
                {
                    cs03Ending.angryOshiro.Y = MathHelper.Lerp(from, to, Ease.CubeOut(p));
                    yield return null;
                }
                yield return topDelay;
                float ground = from + 20f;
                for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 8f)
                {
                    cs03Ending.angryOshiro.Y = MathHelper.Lerp(to, ground, Ease.CubeOut(p));
                    yield return null;
                }
                cs03Ending.angryOshiro.Squish();
                cs03Ending.Level.Shake(0.5f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                cs03Ending.smashRumble = true;
                cs03Ending.roof.StartShaking(0.5f);
                if (!final)
                {
                    for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 16f)
                    {
                        cs03Ending.angryOshiro.Y = MathHelper.Lerp(ground, from, Ease.CubeOut(p));
                        yield return null;
                    }
                }
                else
                {
                    cs03Ending.angryOshiro.Y = (float)(((double)ground + (double)from) / 2.0);
                }

                if (cs03Ending.angryOshiro != null)
                {
                    cs03Ending.player.DummyAutoAnimate = false;
                    cs03Ending.player.Sprite.Play("shaking");
                    cs03Ending.roof.Wobble(cs03Ending.angryOshiro, final);
                    if (!final)
                    {
                        yield return 0.5f;
                    }
                }
            }
        }

        private IEnumerator RunPlayerRight()
        {
            yield return 0.75f;
            yield return player.DummyRunTo(player.X + 128f);
        }

        public override void OnEnd(Level level)
        {
            _ = Audio.SetMusic("event:/music/lvl3/intro");
            Audio.Stop(smashSfx);
            _ = level.CompleteArea();
            SpotlightWipe.FocusPoint = new Vector2(192f, 120f);
        }

        private class BgFlash : Entity
        {
            public float Alpha;

            public BgFlash()
            {
                Depth = 10100;
            }

            public override void Render()
            {
                Camera camera = (Scene as Level).Camera;
                Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * Alpha);
            }
        }
    }
}
