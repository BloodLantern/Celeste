using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroRooftop : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_roof";
        private const float playerEndPosition = 170f;
        private Player player;
        private NPC oshiro;
        private BadelineDummy evil;
        private Vector2 bossSpawnPosition;
        private float anxiety;
        private float anxietyFlicker;
        private Sprite bossSprite = GFX.SpriteBank.Create("oshiro_boss");
        private float bossSpriteOffset;
        private bool oshiroRumble;

        public CS03_OshiroRooftop(NPC oshiro)
        {
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level)
        {
            bossSpawnPosition = new Vector2(oshiro.X, level.Bounds.Bottom - 40);
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            while (cs03OshiroRooftop.player == null)
            {
                cs03OshiroRooftop.player = cs03OshiroRooftop.Scene.Tracker.GetEntity<Player>();
                if (cs03OshiroRooftop.player == null)
                    yield return null;
                else
                    break;
            }
            cs03OshiroRooftop.player.StateMachine.State = 11;
            cs03OshiroRooftop.player.StateMachine.Locked = true;
            while (!cs03OshiroRooftop.player.OnGround() || cs03OshiroRooftop.player.Speed.Y < 0.0)
                yield return null;
            yield return 0.6f;
            cs03OshiroRooftop.evil = new BadelineDummy(new Vector2(cs03OshiroRooftop.oshiro.X - 40f, level.Bounds.Bottom - 60));
            cs03OshiroRooftop.evil.Sprite.Scale.X = 1f;
            cs03OshiroRooftop.evil.Appear(level);
            level.Add(cs03OshiroRooftop.evil);
            yield return 0.1f;
            cs03OshiroRooftop.player.Facing = Facings.Left;
            yield return Textbox.Say("CH3_OSHIRO_START_CHASE", cs03OshiroRooftop.MaddyWalkAway, cs03OshiroRooftop.MaddyTurnAround, cs03OshiroRooftop.EnterOshiro, cs03OshiroRooftop.OshiroGetsAngry);
            yield return cs03OshiroRooftop.OshiroTransform();
            cs03OshiroRooftop.Add(new Coroutine(cs03OshiroRooftop.AnxietyAndCameraOut()));
            yield return level.ZoomBack(0.5f);
            yield return 0.25f;
            cs03OshiroRooftop.EndCutscene(level);
        }

        private IEnumerator MaddyWalkAway()
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            Level scene = cs03OshiroRooftop.Scene as Level;
            cs03OshiroRooftop.Add(new Coroutine(cs03OshiroRooftop.player.DummyWalkTo(scene.Bounds.Left + 170f)));
            yield return 0.2f;
            Audio.Play("event:/game/03_resort/suite_bad_moveroof", cs03OshiroRooftop.evil.Position);
            cs03OshiroRooftop.Add(new Coroutine(cs03OshiroRooftop.evil.FloatTo(cs03OshiroRooftop.evil.Position + new Vector2(80f, 30f))));
            yield return null;
        }

        private IEnumerator MaddyTurnAround()
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            yield return 0.25f;
            cs03OshiroRooftop.player.Facing = Facings.Left;
            yield return 0.1f;
            Level level = cs03OshiroRooftop.SceneAs<Level>();
            yield return level.ZoomTo(new Vector2(150f, (float) (cs03OshiroRooftop.bossSpawnPosition.Y - (double) level.Bounds.Y - 8.0)), 2f, 0.5f);
        }

        private IEnumerator EnterOshiro()
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            yield return 0.3f;
            cs03OshiroRooftop.bossSpriteOffset = (cs03OshiroRooftop.bossSprite.Justify.Value.Y - cs03OshiroRooftop.oshiro.Sprite.Justify.Value.Y) * cs03OshiroRooftop.bossSprite.Height;
            cs03OshiroRooftop.oshiro.Visible = true;
            cs03OshiroRooftop.oshiro.Sprite.Scale.X = 1f;
            cs03OshiroRooftop.Add(new Coroutine(cs03OshiroRooftop.oshiro.MoveTo(cs03OshiroRooftop.bossSpawnPosition - new Vector2(0.0f, cs03OshiroRooftop.bossSpriteOffset))));
            cs03OshiroRooftop.oshiro.Add(new SoundSource("event:/char/oshiro/move_07_roof00_enter"));
            float from = cs03OshiroRooftop.Level.ZoomFocusPoint.X;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.7f)
            {
                cs03OshiroRooftop.Level.ZoomFocusPoint.X = from + (126f - from) * Ease.CubeInOut(p);
                yield return null;
            }
            yield return 0.3f;
            cs03OshiroRooftop.player.Facing = Facings.Left;
            yield return 0.1f;
            cs03OshiroRooftop.evil.Sprite.Scale.X = -1f;
        }

        private IEnumerator OshiroGetsAngry()
        {
            yield return 0.1f;
            evil.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            evil = null;
            yield return 0.8f;
            Audio.Play("event:/char/oshiro/boss_transform_begin", oshiro.Position);
            oshiro.Remove(oshiro.Sprite);
            oshiro.Sprite = bossSprite;
            oshiro.Sprite.Play("transformStart");
            oshiro.Y += bossSpriteOffset;
            oshiro.Add(oshiro.Sprite);
            oshiro.Depth = -12500;
            oshiroRumble = true;
            yield return 1f;
        }

        private IEnumerator OshiroTransform()
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            yield return 0.2f;
            Audio.Play("event:/char/oshiro/boss_transform_burst", cs03OshiroRooftop.oshiro.Position);
            cs03OshiroRooftop.oshiro.Sprite.Play("transformFinish");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            cs03OshiroRooftop.SceneAs<Level>().Shake(0.5f);
            cs03OshiroRooftop.SetChaseMusic();
            while (cs03OshiroRooftop.anxiety < 0.5)
            {
                cs03OshiroRooftop.anxiety = Calc.Approach(cs03OshiroRooftop.anxiety, 0.5f, Engine.DeltaTime * 0.5f);
                yield return null;
            }
            yield return 0.25f;
        }

        private IEnumerator AnxietyAndCameraOut()
        {
            CS03_OshiroRooftop cs03OshiroRooftop = this;
            Level level = cs03OshiroRooftop.Scene as Level;
            Vector2 from = level.Camera.Position;
            Vector2 to = cs03OshiroRooftop.player.CameraTarget;
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime * 2f)
            {
                cs03OshiroRooftop.anxiety = Calc.Approach(cs03OshiroRooftop.anxiety, 0.0f, Engine.DeltaTime * 4f);
                level.Camera.Position = from + (to - from) * Ease.CubeInOut(t);
                yield return null;
            }
        }

        private void SetChaseMusic()
        {
            Level scene = Scene as Level;
            scene.Session.Audio.Music.Event = "event:/music/lvl3/oshiro_chase";
            scene.Session.Audio.Apply();
        }

        public override void OnEnd(Level level)
        {
            Distort.Anxiety = anxiety = anxietyFlicker = 0.0f;
            if (evil != null)
                level.Remove(evil);
            player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                player.X = level.Bounds.Left + 170f;
                player.Speed.Y = 0.0f;
                while (player.CollideCheck<Solid>())
                    --player.Y;
                level.Camera.Position = player.CameraTarget;
            }
            if (WasSkipped)
                SetChaseMusic();
            oshiro.RemoveSelf();
            Scene.Add(new AngryOshiro(bossSpawnPosition, true));
            level.Session.RespawnPoint = new Vector2(level.Bounds.Left + 170f, level.Bounds.Top + 160);
            level.Session.SetFlag("oshiro_resort_roof");
        }

        public override void Update()
        {
            Distort.Anxiety = anxiety + anxiety * anxietyFlicker;
            if (Scene.OnInterval(0.05f))
                anxietyFlicker = Calc.Random.NextFloat(0.4f) - 0.2f;
            base.Update();
            if (!oshiroRumble)
                return;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }
    }
}
