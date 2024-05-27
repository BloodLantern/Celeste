using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS10_HubIntro : CutsceneEntity
    {
        public const string Flag = "hub_intro";
        public const float BirdOffset = 190f;
        private Player player;
        private List<LockBlock> locks;
        private Booster booster;
        private Bird bird;
        private Vector2 spawn;
        private List<EventInstance> sfxs = new List<EventInstance>();

        public CS10_HubIntro(Scene scene, Player player)
        {
            this.player = player;
            spawn = (scene as Level).GetSpawnPoint(player.Position);
            locks = scene.Entities.FindAll<LockBlock>();
            locks.Sort((a, b) => (int) (a.Y - (double) b.Y));
            foreach (Entity entity in locks)
                entity.Visible = false;
            booster = scene.Entities.FindFirst<Booster>();
            if (booster == null)
                return;
            booster.Visible = false;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS10_HubIntro cs10HubIntro1 = this;
            if (cs10HubIntro1.player.Holding != null)
                cs10HubIntro1.player.Throw();
            cs10HubIntro1.player.StateMachine.State = 11;
            cs10HubIntro1.player.ForceCameraUpdate = true;
            while (!cs10HubIntro1.player.OnGround())
                yield return null;
            cs10HubIntro1.player.ForceCameraUpdate = false;
            yield return 0.1f;
            cs10HubIntro1.player.DummyAutoAnimate = false;
            cs10HubIntro1.player.Sprite.Play("lookUp");
            yield return 0.25f;
            Level level1 = level;
            CS10_HubIntro cs10HubIntro2 = cs10HubIntro1;
            double x = cs10HubIntro1.spawn.X;
            double y = level.Bounds.Top + 190.0;
            Bird bird1;
            Bird bird2 = bird1 = new Bird(new Vector2((float) x, (float) y));
            cs10HubIntro2.bird = bird1;
            Bird bird3 = bird2;
            level1.Add(bird3);
            Audio.Play("event:/new_content/game/10_farewell/bird_camera_pan_up");
            yield return CutsceneEntity.CameraTo(new Vector2(cs10HubIntro1.spawn.X - 160f, (float) (level.Bounds.Top + 190.0 - 90.0)), 2f, Ease.CubeInOut);
            yield return cs10HubIntro1.bird.IdleRoutine();
            cs10HubIntro1.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X, level.Bounds.Top), 0.8f, Ease.CubeInOut, 0.1f)));
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            yield return cs10HubIntro1.bird.FlyAwayRoutine();
            cs10HubIntro1.bird.RemoveSelf();
            cs10HubIntro1.bird = null;
            yield return 0.5f;
            float duration = 6f;
            string sfx = "event:/new_content/game/10_farewell/locked_door_appear_1".Substring(0, "event:/new_content/game/10_farewell/locked_door_appear_1".Length - 1);
            int doorIndex = 1;
            cs10HubIntro1.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X, level.Bounds.Bottom - 180), duration, Ease.SineInOut)));
            cs10HubIntro1.Add(new Coroutine(cs10HubIntro1.Level.ZoomTo(new Vector2(160f, 90f), 1.5f, duration)));
            for (float t = 0.0f; t < (double) duration; t += Engine.DeltaTime)
            {
                foreach (LockBlock lockBlock in cs10HubIntro1.locks)
                {
                    if (!lockBlock.Visible && level.Camera.Y + 90.0 > lockBlock.Y - 20.0)
                    {
                        cs10HubIntro1.sfxs.Add(Audio.Play(sfx + doorIndex, lockBlock.Center));
                        lockBlock.Appear();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        ++doorIndex;
                    }
                }
                yield return null;
            }
            sfx = null;
            yield return 0.5f;
            if (cs10HubIntro1.booster != null)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                cs10HubIntro1.booster.Appear();
            }
            yield return 0.3f;
            yield return cs10HubIntro1.Level.ZoomBack(0.3f);
            cs10HubIntro1.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                foreach (EventInstance instance in sfxs)
                    Audio.Stop(instance);
                if (bird != null)
                    Audio.Stop(bird.sfx);
            }
            foreach (Entity entity in locks)
                entity.Visible = true;
            if (booster != null)
                booster.Visible = true;
            if (bird != null)
                bird.RemoveSelf();
            if (WasSkipped)
                player.Position = spawn;
            player.Speed = Vector2.Zero;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            player.StateMachine.State = 0;
            level.Camera.Y = level.Bounds.Bottom - 180;
            level.Session.SetFlag("hub_intro");
            level.ResetZoom();
        }

        private class Bird : Entity
        {
            private Sprite sprite;
            public EventInstance sfx;

            public Bird(Vector2 position)
            {
                Position = position;
                Depth = -8500;
                Add(sprite = GFX.SpriteBank.Create("bird"));
                sprite.Play("hover");
                sprite.OnFrameChange = spr => BirdNPC.FlapSfxCheck(sprite);
            }

            public IEnumerator IdleRoutine()
            {
                yield return 0.5f;
            }

            public IEnumerator FlyAwayRoutine()
            {
                Bird bird = this;
                Level level = bird.Scene as Level;
                bird.sfx = Audio.Play("event:/new_content/game/10_farewell/bird_fly_uptonext", bird.Position);
                bird.sprite.Play("flyup");
                float spd = -32f;
                while (bird.Y > (double) (level.Bounds.Top - 32))
                {
                    spd -= 400f * Engine.DeltaTime;
                    bird.Y += spd * Engine.DeltaTime;
                    yield return null;
                }
            }
        }
    }
}
