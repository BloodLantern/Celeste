using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class TempleBigEyeball : Entity
    {
        private Sprite sprite;
        private Image pupil;
        private bool triggered;
        private Vector2 pupilTarget;
        private float pupilDelay;
        private Wiggler bounceWiggler;
        private Wiggler pupilWiggler;
        private float shockwaveTimer;
        private bool shockwaveFlag;
        private float pupilSpeed = 40f;
        private bool bursting;

        public TempleBigEyeball(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("temple_eyeball"));
            Add(pupil = new Image(GFX.Game["danger/templeeye/pupil"]));
            pupil.CenterOrigin();
            Collider = new Hitbox(48f, 64f, -24f, -32f);
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(bounceWiggler = Wiggler.Create(0.5f, 3f));
            Add(pupilWiggler = Wiggler.Create(0.5f, 3f));
            shockwaveTimer = 2f;
        }

        private void OnPlayer(Player player)
        {
            if (triggered)
                return;
            Audio.Play("event:/game/05_mirror_temple/eyewall_bounce", player.Position);
            player.ExplodeLaunch(player.Center + Vector2.UnitX * 20f);
            player.Swat(-1);
            bounceWiggler.Start();
        }

        private void OnHoldable(Holdable h)
        {
            if (!(h.Entity is TheoCrystal))
                return;
            TheoCrystal entity = h.Entity as TheoCrystal;
            if (triggered || entity.Speed.X <= 32.0 || entity.Hold.IsHeld)
                return;
            entity.Speed.X = -50f;
            entity.Speed.Y = -10f;
            triggered = true;
            bounceWiggler.Start();
            Collidable = false;
            Audio.SetAmbience(null);
            Audio.Play("event:/game/05_mirror_temple/eyewall_destroy", Position);
            Alarm.Set(this, 1.3f, () => Audio.SetMusic(null));
            Add(new Coroutine(Burst()));
        }

        private IEnumerator Burst()
        {
            TempleBigEyeball templeBigEyeball = this;
            templeBigEyeball.bursting = true;
            Level level = templeBigEyeball.Scene as Level;
            level.StartCutscene(templeBigEyeball.OnSkip, false, true);
            level.RegisterAreaComplete();
            Celeste.Freeze(0.1f);
            yield return null;
            float start = Glitch.Value;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.5f, start: true);
            tween.OnUpdate = t => Glitch.Value = MathHelper.Lerp(start, 0.0f, t.Eased);
            templeBigEyeball.Add(tween);
            Player player = templeBigEyeball.Scene.Tracker.GetEntity<Player>();
            TheoCrystal entity = templeBigEyeball.Scene.Tracker.GetEntity<TheoCrystal>();
            if (player != null)
            {
                player.StateMachine.State = 11;
                player.StateMachine.Locked = true;
                if (player.OnGround())
                {
                    player.DummyAutoAnimate = false;
                    player.Sprite.Play("shaking");
                }
            }
            templeBigEyeball.Add(new Coroutine(level.ZoomTo(entity.TopCenter - level.Camera.Position, 2f, 0.5f)));
            templeBigEyeball.Add(new Coroutine(entity.Shatter()));
            foreach (TempleEye templeEye in templeBigEyeball.Scene.Entities.FindAll<TempleEye>())
                templeEye.Burst();
            templeBigEyeball.sprite.Play("burst");
            templeBigEyeball.pupil.Visible = false;
            level.Shake(0.4f);
            yield return 2f;
            if (player != null && player.OnGround())
            {
                player.DummyAutoAnimate = false;
                player.Sprite.Play("shaking");
            }
            templeBigEyeball.Visible = false;
            Fader fade = new Fader();
            level.Add(fade);
            while ((fade.Fade += Engine.DeltaTime) < 1.0)
                yield return null;
            yield return 1f;
            fade = null;
            level.EndCutscene();
            level.CompleteArea(false);
        }

        private void OnSkip(Level level) => level.CompleteArea(false);

        public override void Update()
        {
            base.Update();
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            Rectangle bounds;
            if (entity1 != null)
            {
                double x1 = entity1.X;
                bounds = (Scene as Level).Bounds;
                double left = bounds.Left;
                double x2 = X;
                Audio.SetMusicParam("eye_distance", Calc.ClampedMap((float) x1, (float) left, (float) x2));
            }
            if (entity1 != null && !bursting)
                Glitch.Value = Calc.ClampedMap(Math.Abs(X - entity1.X), 100f, 900f, 0.2f, 0.0f);
            if (!triggered && shockwaveTimer > 0.0)
            {
                shockwaveTimer -= Engine.DeltaTime;
                if (shockwaveTimer <= 0.0)
                {
                    if (entity1 != null)
                    {
                        shockwaveTimer = Calc.ClampedMap(Math.Abs(X - entity1.X), 100f, 500f, 2f, 3f);
                        shockwaveFlag = !shockwaveFlag;
                        if (shockwaveFlag)
                            --shockwaveTimer;
                    }
                    Scene.Add(Engine.Pooler.Create<TempleBigEyeballShockwave>().Init(Center + new Vector2(50f, 0.0f)));
                    pupilWiggler.Start();
                    pupilTarget = new Vector2(-1f, 0.0f);
                    pupilSpeed = 120f;
                    pupilDelay = Math.Max(0.5f, pupilDelay);
                }
            }
            pupil.Position = Calc.Approach(pupil.Position, pupilTarget * 12f, Engine.DeltaTime * pupilSpeed);
            pupilSpeed = Calc.Approach(pupilSpeed, 40f, Engine.DeltaTime * 400f);
            TheoCrystal entity2 = Scene.Tracker.GetEntity<TheoCrystal>();
            if (entity2 != null && Math.Abs(X - entity2.X) < 64.0 && Math.Abs(Y - entity2.Y) < 64.0)
                pupilTarget = (entity2.Center - Position).SafeNormalize();
            else if (pupilDelay < 0.0)
            {
                pupilTarget = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
                pupilDelay = Calc.Random.Choose(0.2f, 1f, 2f);
            }
            else
                pupilDelay -= Engine.DeltaTime;
            if (entity1 == null)
                return;
            Level scene = Scene as Level;
            double x = entity1.X;
            bounds = scene.Bounds;
            double min = bounds.Left + 32;
            double max = X - 32.0;
            Audio.SetMusicParam("eye_distance", Calc.ClampedMap((float) x, (float) min, (float) max, 1f, 0.0f));
        }

        public override void Render()
        {
            sprite.Scale.X = (float) (1.0 + 0.15000000596046448 * bounceWiggler.Value);
            pupil.Scale = Vector2.One * (float) (1.0 + pupilWiggler.Value * 0.15000000596046448);
            base.Render();
        }

        private class Fader : Entity
        {
            public float Fade;

            public Fader() => Tag = (int) Tags.HUD;

            public override void Render() => Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, Color.White * Fade);
        }
    }
}
