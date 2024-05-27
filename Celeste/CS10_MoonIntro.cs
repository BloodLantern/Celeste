using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS10_MoonIntro : CutsceneEntity
    {
        public const string Flag = "moon_intro";
        private Player player;
        private BadelineDummy badeline;
        private BirdNPC bird;
        private float fade = 1f;
        private float targetX;

        public CS10_MoonIntro(Player player)
        {
            Depth = -8500;
            this.player = player;
            targetX = player.CameraTarget.X + 8f;
        }

        public override void OnBegin(Level level)
        {
            bird = Scene.Entities.FindFirst<BirdNPC>();
            player.StateMachine.State = 11;
            if (level.Wipe != null)
                level.Wipe.Cancel();
            level.Wipe = new FadeWipe(level, true);
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS10_MoonIntro cs10MoonIntro = this;
            cs10MoonIntro.player.StateMachine.State = 11;
            cs10MoonIntro.player.Visible = false;
            cs10MoonIntro.player.Active = false;
            cs10MoonIntro.player.Dashes = 2;
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / 0.9f)
            {
                level.Wipe.Percent = 0.0f;
                yield return null;
            }
            cs10MoonIntro.Add(new Coroutine(cs10MoonIntro.FadeIn(5f)));
            level.Camera.Position = level.LevelOffset + new Vector2(-100f, 0.0f);
            yield return CutsceneEntity.CameraTo(new Vector2(cs10MoonIntro.targetX, level.Camera.Y), 6f, Ease.SineOut);
            level.Camera.Position = new Vector2(cs10MoonIntro.targetX, level.Camera.Y);
            if (cs10MoonIntro.bird != null)
            {
                yield return cs10MoonIntro.bird.StartleAndFlyAway();
                level.Session.DoNotLoad.Add(cs10MoonIntro.bird.EntityID);
                cs10MoonIntro.bird = null;
            }
            yield return 0.5f;
            cs10MoonIntro.player.Speed = Vector2.Zero;
            cs10MoonIntro.player.Position = level.GetSpawnPoint(cs10MoonIntro.player.Position);
            cs10MoonIntro.player.Active = true;
            cs10MoonIntro.player.StateMachine.State = 23;
            while (cs10MoonIntro.player.Top > (double) level.Bounds.Bottom)
                yield return null;
            yield return 0.2f;
            Audio.Play("event:/new_content/char/madeline/screenentry_lowgrav", cs10MoonIntro.player.Position);
            while (cs10MoonIntro.player.StateMachine.State == 23)
                yield return null;
            cs10MoonIntro.player.X = (int) cs10MoonIntro.player.X;
            cs10MoonIntro.player.Y = (int) cs10MoonIntro.player.Y;
            while (!cs10MoonIntro.player.OnGround() && cs10MoonIntro.player.Bottom < (double) level.Bounds.Bottom)
                cs10MoonIntro.player.MoveVExact(16);
            cs10MoonIntro.player.StateMachine.State = 11;
            yield return 0.5f;
            yield return cs10MoonIntro.BadelineAppears();
            yield return Textbox.Say("CH9_LANDING", cs10MoonIntro.BadelineTurns, cs10MoonIntro.BadelineVanishes);
            cs10MoonIntro.EndCutscene(level);
        }

        private IEnumerator BadelineTurns()
        {
            CS10_MoonIntro cs10MoonIntro = this;
            yield return 0.1f;
            int target = Math.Sign(cs10MoonIntro.badeline.Sprite.Scale.X) * -1;
            Wiggler wiggler = Wiggler.Create(0.5f, 3f, v => badeline.Sprite.Scale = new Vector2(target, 1f) * (float) (1.0 + 0.20000000298023224 * v), true, true);
            cs10MoonIntro.Add(wiggler);
            Audio.Play(target < 0 ? "event:/char/badeline/jump_wall_left" : "event:/char/badeline/jump_wall_left", cs10MoonIntro.badeline.Position);
            yield return 0.6f;
        }

        private IEnumerator BadelineAppears()
        {
            CS10_MoonIntro cs10MoonIntro = this;
            cs10MoonIntro.Level.Session.Inventory.Dashes = 1;
            cs10MoonIntro.player.Dashes = 1;
            cs10MoonIntro.Level.Add(cs10MoonIntro.badeline = new BadelineDummy(cs10MoonIntro.player.Position));
            cs10MoonIntro.Level.Displacement.AddBurst(cs10MoonIntro.player.Center, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("event:/char/badeline/maddy_split", cs10MoonIntro.player.Position);
            cs10MoonIntro.badeline.Sprite.Scale.X = 1f;
            yield return cs10MoonIntro.badeline.FloatTo(cs10MoonIntro.player.Position + new Vector2(-16f, -16f), 1, false);
            cs10MoonIntro.player.Facing = Facings.Left;
            yield return null;
        }

        private IEnumerator BadelineVanishes()
        {
            yield return 0.5f;
            badeline.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            badeline = null;
            yield return 0.8f;
            player.Facing = Facings.Right;
            yield return 0.6f;
        }

        private IEnumerator FadeIn(float duration)
        {
            while (fade > 0.0)
            {
                fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime / duration);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            level.Session.Inventory.Dashes = 1;
            player.Dashes = 1;
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.Position = level.GetSpawnPoint(player.Position) + new Vector2(0.0f, -32f);
            player.Active = true;
            player.Visible = true;
            player.StateMachine.State = 0;
            player.X = (int) player.X;
            player.Y = (int) player.Y;
            while (!player.OnGround() && player.Bottom < (double) level.Bounds.Bottom)
                player.MoveVExact(16);
            if (badeline != null)
                badeline.RemoveSelf();
            if (bird != null)
            {
                bird.RemoveSelf();
                level.Session.DoNotLoad.Add(bird.EntityID);
            }
            level.Camera.Position = new Vector2(targetX, level.Camera.Y);
            level.Session.SetFlag("moon_intro");
        }

        public override void Render()
        {
            Camera camera = (Scene as Level).Camera;
            Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
        }
    }
}
