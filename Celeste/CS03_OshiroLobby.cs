using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroLobby : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_talked_1";
        private Player player;
        private NPC oshiro;
        private float startLightAlpha;
        private bool createSparks;
        private SoundSource sfx = new SoundSource();

        public CS03_OshiroLobby(Player player, NPC oshiro)
        {
            this.player = player;
            this.oshiro = oshiro;
            Add(sfx);
        }

        public override void Update()
        {
            base.Update();
            if (!createSparks || !Level.OnInterval(0.025f))
                return;
            Vector2 position = oshiro.Position + new Vector2(0.0f, -12f) + new Vector2(Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1), Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1));
            Level.Particles.Emit(NPC03_Oshiro_Lobby.P_AppearSpark, position, (position - oshiro.Position).Angle());
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroLobby cs03OshiroLobby = this;
            cs03OshiroLobby.startLightAlpha = level.Lighting.Alpha;
            float endLightAlpha = 1f;
            float from = cs03OshiroLobby.oshiro.Y;
            cs03OshiroLobby.player.StateMachine.State = 11;
            cs03OshiroLobby.player.StateMachine.Locked = true;
            yield return 0.5f;
            yield return cs03OshiroLobby.player.DummyWalkTo(cs03OshiroLobby.oshiro.X - 16f);
            cs03OshiroLobby.player.Facing = Facings.Right;
            cs03OshiroLobby.sfx.Play("event:/game/03_resort/sequence_oshiro_intro");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 1.4f;
            level.Shake();
            level.Lighting.Alpha += 0.5f;
            while (level.Lighting.Alpha > (double) cs03OshiroLobby.startLightAlpha)
            {
                level.Lighting.Alpha -= Engine.DeltaTime * 4f;
                yield return null;
            }
            VertexLight light = new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 32, 64);
            BloomPoint bloom = new BloomPoint(new Vector2(0.0f, -8f), 1f, 16f);
            level.Lighting.SetSpotlight(light);
            cs03OshiroLobby.oshiro.Add(light);
            cs03OshiroLobby.oshiro.Add(bloom);
            cs03OshiroLobby.oshiro.Y -= 16f;
            Vector2 target = light.Position;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, true);
            tween1.OnUpdate = t =>
            {
                light.Alpha = bloom.Alpha = t.Percent;
                light.Position = Vector2.Lerp(target - Vector2.UnitY * 48f, target, t.Percent);
                level.Lighting.Alpha = MathHelper.Lerp(startLightAlpha, endLightAlpha, t.Eased);
            };
            cs03OshiroLobby.Add(tween1);
            yield return tween1.Wait();
            yield return 0.2f;
            yield return level.ZoomTo(new Vector2(170f, 126f), 2f, 0.5f);
            yield return 0.6f;
            level.Shake();
            cs03OshiroLobby.oshiro.Sprite.Visible = true;
            cs03OshiroLobby.oshiro.Sprite.Play("appear");
            yield return cs03OshiroLobby.player.DummyWalkToExact((int) (cs03OshiroLobby.player.X - 12.0), true);
            cs03OshiroLobby.player.DummyAutoAnimate = false;
            cs03OshiroLobby.player.Sprite.Play("shaking");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
            yield return 0.6f;
            cs03OshiroLobby.createSparks = true;
            yield return 0.4f;
            cs03OshiroLobby.createSparks = false;
            yield return 0.2f;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            yield return 1.4f;
            level.Lighting.UnsetSpotlight();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.5f, true);
            tween.OnUpdate = t =>
            {
                level.Lighting.Alpha = MathHelper.Lerp(endLightAlpha, startLightAlpha, t.Percent);
                bloom.Alpha = 1f - t.Percent;
            };
            cs03OshiroLobby.Add(tween);
            while (cs03OshiroLobby.oshiro.Y != (double) from)
            {
                cs03OshiroLobby.oshiro.Y = Calc.Approach(cs03OshiroLobby.oshiro.Y, from, Engine.DeltaTime * 40f);
                yield return null;
            }
            yield return tween.Wait();
            tween = null;
            Audio.SetMusic("event:/music/lvl3/oshiro_theme");
            cs03OshiroLobby.player.DummyAutoAnimate = true;
            yield return Textbox.Say("CH3_OSHIRO_FRONT_DESK", cs03OshiroLobby.ZoomOut);
            foreach (MrOshiroDoor mrOshiroDoor in cs03OshiroLobby.Scene.Entities.FindAll<MrOshiroDoor>())
                mrOshiroDoor.Open();
            cs03OshiroLobby.oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, cs03OshiroLobby.oshiro.Y));
            cs03OshiroLobby.oshiro.Add(new SoundSource("event:/char/oshiro/move_01_0xa_exit"));
            yield return 1.5f;
            cs03OshiroLobby.EndCutscene(level);
        }

        private IEnumerator ZoomOut()
        {
            CS03_OshiroLobby cs03OshiroLobby = this;
            yield return 0.2f;
            yield return cs03OshiroLobby.Level.ZoomBack(0.5f);
            yield return 0.2f;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            if (WasSkipped)
            {
                foreach (MrOshiroDoor mrOshiroDoor in Scene.Entities.FindAll<MrOshiroDoor>())
                    mrOshiroDoor.InstantOpen();
            }
            level.Lighting.Alpha = startLightAlpha;
            level.Lighting.UnsetSpotlight();
            level.Session.SetFlag("oshiro_resort_talked_1");
            level.Session.Audio.Music.Event = "event:/music/lvl3/explore";
            level.Session.Audio.Music.Progress = 1;
            level.Session.Audio.Apply();
            if (!WasSkipped)
                return;
            level.Remove(oshiro);
        }
    }
}
