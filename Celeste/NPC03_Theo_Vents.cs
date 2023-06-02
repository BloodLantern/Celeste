using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC03_Theo_Vents : NPC
    {
        private const string AppeardFlag = "theoVentsAppeared";
        private const string TalkedFlag = "theoVentsTalked";
        private const int SpriteAppearY = -8;
        private float particleDelay;
        private bool appeared;
        private NPC03_Theo_Vents.Grate grate;

        public NPC03_Theo_Vents(Vector2 position)
            : base(position)
        {
            this.Tag = (int) Tags.TransitionUpdate;
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("theo")));
            this.Sprite.Scale.Y = -1f;
            this.Sprite.Scale.X = -1f;
            this.Visible = false;
            this.Maxspeed = 48f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (this.Session.GetFlag("theoVentsTalked"))
                this.RemoveSelf();
            else
                this.Add((Component) new Coroutine(this.Appear()));
        }

        public override void Update()
        {
            base.Update();
            if (this.appeared)
                return;
            this.particleDelay -= Engine.DeltaTime;
            if ((double) this.particleDelay > 0.0)
                return;
            this.Level.ParticlesFG.Emit(ParticleTypes.VentDust, 8, this.Position, new Vector2(6f, 0.0f));
            if (this.grate != null)
                this.grate.Shake();
            this.particleDelay = Calc.Random.Choose<float>(1f, 2f, 3f);
        }

        private IEnumerator Appear()
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            if (!npC03TheoVents.Session.GetFlag("theoVentsAppeared"))
            {
                npC03TheoVents.grate = new NPC03_Theo_Vents.Grate(npC03TheoVents.Position);
                npC03TheoVents.Scene.Add((Entity) npC03TheoVents.grate);
                Player entity;
                do
                {
                    yield return (object) null;
                    entity = npC03TheoVents.Scene.Tracker.GetEntity<Player>();
                }
                while (entity == null || (double) entity.X <= (double) npC03TheoVents.X - 32.0);
                Audio.Play("event:/char/theo/resort_ceilingvent_hey", npC03TheoVents.Position);
                npC03TheoVents.Level.ParticlesFG.Emit(ParticleTypes.VentDust, 24, npC03TheoVents.Position, new Vector2(6f, 0.0f));
                npC03TheoVents.grate.Fall();
                int from = -24;
                for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 2f)
                {
                    yield return (object) null;
                    npC03TheoVents.Visible = true;
                    npC03TheoVents.Sprite.Y = (float) from + (float) (-8 - from) * Ease.CubeOut(p);
                }
                npC03TheoVents.Session.SetFlag("theoVentsAppeared");
            }
            npC03TheoVents.appeared = true;
            npC03TheoVents.Sprite.Y = -8f;
            npC03TheoVents.Visible = true;
            npC03TheoVents.Add((Component) (npC03TheoVents.Talker = new TalkComponent(new Rectangle(-16, 0, 32, 100), new Vector2(0.0f, -8f), new Action<Player>(npC03TheoVents.OnTalk))));
        }

        private void OnTalk(Player player)
        {
            this.Level.StartCutscene(new Action<Level>(this.OnTalkEnd));
            this.Add((Component) new Coroutine(this.Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            yield return (object) npC03TheoVents.PlayerApproach(player, spacing: new float?(10f), side: new int?(-1));
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookUp");
            yield return (object) CutsceneEntity.CameraTo(new Vector2((float) (npC03TheoVents.Level.Bounds.Right - 320), (float) npC03TheoVents.Level.Bounds.Top), 0.5f);
            yield return (object) npC03TheoVents.Level.ZoomTo(new Vector2(240f, 70f), 2f, 0.5f);
            yield return (object) Textbox.Say("CH3_THEO_VENTS");
            yield return (object) npC03TheoVents.Disappear();
            yield return (object) 0.25f;
            yield return (object) npC03TheoVents.Level.ZoomBack(0.5f);
            npC03TheoVents.Level.EndCutscene();
            npC03TheoVents.OnTalkEnd(npC03TheoVents.Level);
        }

        private void OnTalkEnd(Level level)
        {
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.DummyAutoAnimate = true;
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
            this.Session.SetFlag("theoVentsTalked");
            this.RemoveSelf();
        }

        private IEnumerator Disappear()
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            Audio.Play("event:/char/theo/resort_ceilingvent_seeya", npC03TheoVents.Position);
            int to = -24;
            float from = npC03TheoVents.Sprite.Y;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 2f)
            {
                yield return (object) null;
                npC03TheoVents.Level.ParticlesFG.Emit(ParticleTypes.VentDust, 1, npC03TheoVents.Position, new Vector2(6f, 0.0f));
                npC03TheoVents.Sprite.Y = from + ((float) to - from) * Ease.BackIn(p);
            }
        }

        private class Grate : Entity
        {
            private Monocle.Image sprite;
            private float shake;
            private Vector2 speed;
            private bool falling;
            private float alpha = 1f;

            public Grate(Vector2 position)
                : base(position)
            {
                this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["scenery/grate"])));
                this.sprite.JustifyOrigin(0.5f, 0.0f);
            }

            public void Shake()
            {
                if (this.falling)
                    return;
                Audio.Play("event:/char/theo/resort_ceilingvent_shake", this.Position);
                this.shake = 0.5f;
            }

            public void Fall()
            {
                Audio.Play("event:/char/theo/resort_ceilingvent_popoff", this.Position);
                this.falling = true;
                this.speed = new Vector2(40f, 200f);
                this.Collider = (Collider) new Hitbox(2f, 2f, -1f);
            }

            public override void Update()
            {
                if ((double) this.shake > 0.0)
                {
                    this.shake -= Engine.DeltaTime;
                    if (this.Scene.OnInterval(0.05f))
                        this.sprite.X = 1f - this.sprite.X;
                }
                if (this.falling)
                {
                    this.speed.X = Calc.Approach(this.speed.X, 0.0f, Engine.DeltaTime * 80f);
                    this.speed.Y += 200f * Engine.DeltaTime;
                    this.Position = this.Position + this.speed * Engine.DeltaTime;
                    if (this.CollideCheck<Solid>(this.Position + new Vector2(0.0f, 2f)) && (double) this.speed.Y > 0.0)
                        this.speed.Y = (float) (-(double) this.speed.Y * 0.25);
                    this.alpha -= Engine.DeltaTime;
                    this.sprite.Rotation += Engine.DeltaTime;
                    this.sprite.Color = Color.White * this.alpha;
                    if ((double) this.alpha <= 0.0)
                        this.RemoveSelf();
                }
                base.Update();
            }
        }
    }
}
