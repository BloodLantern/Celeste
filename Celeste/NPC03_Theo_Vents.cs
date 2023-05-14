// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Theo_Vents
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            Tag = (int)Tags.TransitionUpdate;
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Scale.Y = -1f;
            Sprite.Scale.X = -1f;
            Visible = false;
            Maxspeed = 48f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("theoVentsTalked"))
            {
                RemoveSelf();
            }
            else
            {
                Add(new Coroutine(Appear()));
            }
        }

        public override void Update()
        {
            base.Update();
            if (appeared)
            {
                return;
            }

            particleDelay -= Engine.DeltaTime;
            if (particleDelay > 0.0)
            {
                return;
            }

            Level.ParticlesFG.Emit(ParticleTypes.VentDust, 8, Position, new Vector2(6f, 0.0f));
            grate?.Shake();
            particleDelay = Calc.Random.Choose<float>(1f, 2f, 3f);
        }

        private IEnumerator Appear()
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            if (!npC03TheoVents.Session.GetFlag("theoVentsAppeared"))
            {
                npC03TheoVents.grate = new NPC03_Theo_Vents.Grate(npC03TheoVents.Position);
                npC03TheoVents.Scene.Add(npC03TheoVents.grate);
                Player entity;
                do
                {
                    yield return null;
                    entity = npC03TheoVents.Scene.Tracker.GetEntity<Player>();
                }
                while (entity == null || (double)entity.X <= (double)npC03TheoVents.X - 32.0);
                _ = Audio.Play("event:/char/theo/resort_ceilingvent_hey", npC03TheoVents.Position);
                npC03TheoVents.Level.ParticlesFG.Emit(ParticleTypes.VentDust, 24, npC03TheoVents.Position, new Vector2(6f, 0.0f));
                npC03TheoVents.grate.Fall();
                int from = -24;
                for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 2f)
                {
                    yield return null;
                    npC03TheoVents.Visible = true;
                    npC03TheoVents.Sprite.Y = from + ((-8 - from) * Ease.CubeOut(p));
                }
                npC03TheoVents.Session.SetFlag("theoVentsAppeared");
            }
            npC03TheoVents.appeared = true;
            npC03TheoVents.Sprite.Y = -8f;
            npC03TheoVents.Visible = true;
            npC03TheoVents.Add(npC03TheoVents.Talker = new TalkComponent(new Rectangle(-16, 0, 32, 100), new Vector2(0.0f, -8f), new Action<Player>(npC03TheoVents.OnTalk)));
        }

        private void OnTalk(Player player)
        {
            Level.StartCutscene(new Action<Level>(OnTalkEnd));
            Add(new Coroutine(Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            yield return npC03TheoVents.PlayerApproach(player, spacing: new float?(10f), side: new int?(-1));
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookUp");
            yield return CutsceneEntity.CameraTo(new Vector2(npC03TheoVents.Level.Bounds.Right - 320, npC03TheoVents.Level.Bounds.Top), 0.5f);
            yield return npC03TheoVents.Level.ZoomTo(new Vector2(240f, 70f), 2f, 0.5f);
            yield return Textbox.Say("CH3_THEO_VENTS");
            yield return npC03TheoVents.Disappear();
            yield return 0.25f;
            yield return npC03TheoVents.Level.ZoomBack(0.5f);
            npC03TheoVents.Level.EndCutscene();
            npC03TheoVents.OnTalkEnd(npC03TheoVents.Level);
        }

        private void OnTalkEnd(Level level)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.DummyAutoAnimate = true;
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
            Session.SetFlag("theoVentsTalked");
            RemoveSelf();
        }

        private IEnumerator Disappear()
        {
            NPC03_Theo_Vents npC03TheoVents = this;
            _ = Audio.Play("event:/char/theo/resort_ceilingvent_seeya", npC03TheoVents.Position);
            int to = -24;
            float from = npC03TheoVents.Sprite.Y;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 2f)
            {
                yield return null;
                npC03TheoVents.Level.ParticlesFG.Emit(ParticleTypes.VentDust, 1, npC03TheoVents.Position, new Vector2(6f, 0.0f));
                npC03TheoVents.Sprite.Y = from + ((to - from) * Ease.BackIn(p));
            }
        }

        private class Grate : Entity
        {
            private readonly Monocle.Image sprite;
            private float shake;
            private Vector2 speed;
            private bool falling;
            private float alpha = 1f;

            public Grate(Vector2 position)
                : base(position)
            {
                Add(sprite = new Monocle.Image(GFX.Game["scenery/grate"]));
                _ = sprite.JustifyOrigin(0.5f, 0.0f);
            }

            public void Shake()
            {
                if (falling)
                {
                    return;
                }

                _ = Audio.Play("event:/char/theo/resort_ceilingvent_shake", Position);
                shake = 0.5f;
            }

            public void Fall()
            {
                _ = Audio.Play("event:/char/theo/resort_ceilingvent_popoff", Position);
                falling = true;
                speed = new Vector2(40f, 200f);
                Collider = new Hitbox(2f, 2f, -1f);
            }

            public override void Update()
            {
                if (shake > 0.0)
                {
                    shake -= Engine.DeltaTime;
                    if (Scene.OnInterval(0.05f))
                    {
                        sprite.X = 1f - sprite.X;
                    }
                }
                if (falling)
                {
                    speed.X = Calc.Approach(speed.X, 0.0f, Engine.DeltaTime * 80f);
                    speed.Y += 200f * Engine.DeltaTime;
                    Position += speed * Engine.DeltaTime;
                    if (CollideCheck<Solid>(Position + new Vector2(0.0f, 2f)) && speed.Y > 0.0)
                    {
                        speed.Y = (float)(-(double)speed.Y * 0.25);
                    }

                    alpha -= Engine.DeltaTime;
                    sprite.Rotation += Engine.DeltaTime;
                    sprite.Color = Color.White * alpha;
                    if (alpha <= 0.0)
                    {
                        RemoveSelf();
                    }
                }
                base.Update();
            }
        }
    }
}
