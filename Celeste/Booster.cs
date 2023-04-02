// Decompiled with JetBrains decompiler
// Type: Celeste.Booster
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class Booster : Entity
    {
        private const float RespawnTime = 1f;
        public static ParticleType P_Burst;
        public static ParticleType P_BurstRed;
        public static ParticleType P_Appear;
        public static ParticleType P_RedAppear;
        public static readonly Vector2 playerOffset = new(0f, -2f);
        private readonly Sprite sprite;
        private Entity outline;
        private readonly Wiggler wiggler;
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private readonly Coroutine dashRoutine;
        private readonly DashListener dashListener;
        private readonly ParticleType particleType;
        private float respawnTimer;
        private float cannotUseTimer;
        private readonly bool red;
        private readonly SoundSource loopingSfx;
        public bool Ch9HubBooster;
        public bool Ch9HubTransition;

        public bool BoostingPlayer { get; private set; }

        public Booster(Vector2 position, bool red)
            : base(position)
        {
            Depth = -8500;
            Collider = new Circle(10f, y: 2f);
            this.red = red;
            Add(sprite = GFX.SpriteBank.Create(red ? "boosterRed" : "booster"));
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(light = new VertexLight(Color.White, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (1 + f * 0.25f)));
            Add(dashRoutine = new Coroutine(false));
            Add(dashListener = new DashListener());
            Add(new MirrorReflection());
            Add(loopingSfx = new SoundSource());
            dashListener.OnDash = new Action<Vector2>(OnPlayerDashed);
            particleType = red ? P_BurstRed : P_Burst;
        }

        public Booster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool(nameof(red)))
        {
            Ch9HubBooster = data.Bool("ch9_hub_booster");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Image image = new(GFX.Game["objects/booster/outline"]);
            _ = image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            outline = new Entity(Position)
            {
                Depth = 8999,
                Visible = false
            };
            outline.Add(image);
            outline.Add(new MirrorReflection());
            scene.Add(outline);
        }

        public void Appear()
        {
            _ = Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
            sprite.Play("appear");
            wiggler.Start();
            Visible = true;
            AppearParticles();
        }

        private void AppearParticles()
        {
            ParticleSystem particlesBg = SceneAs<Level>().ParticlesBG;
            for (int angleDegree = 0; angleDegree < 360; angleDegree += 30)
                particlesBg.Emit(red ? P_RedAppear : P_Appear, 1, Center, Vector2.One * 2f, angleDegree * (float) Math.PI / 180f);
        }

        private void OnPlayer(Player player)
        {
            if (respawnTimer > 0 || cannotUseTimer > 0 || BoostingPlayer)
                return;

            cannotUseTimer = 0.45f;
            if (red)
                player.RedBoost(this);
            else
                player.Boost(this);

            _ = Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
            wiggler.Start();
            sprite.Play("inside");
            sprite.FlipX = player.Facing == Facings.Left;
        }

        public void PlayerBoosted(Player player, Vector2 direction)
        {
            _ = Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", Position);
            if (red)
            {
                _ = loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
                loopingSfx.DisposeOnTransition = false;
            }
            if (Ch9HubBooster && direction.Y < 0)
            {
                bool flag = true;
                List<LockBlock> all = Scene.Entities.FindAll<LockBlock>();
                if (all.Count > 0)
                {
                    foreach (LockBlock lockBlock in all)
                        if (!lockBlock.UnlockingRegistered)
                        {
                            flag = false;
                            break;
                        }
                }
                if (flag)
                {
                    Ch9HubTransition = true;
                    Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => Add(new SoundSource("event:/new_content/timeline_bubble_to_remembered")
                    {
                        DisposeOnTransition = false
                    }), 2f, true));
                }
            }
            BoostingPlayer = true;
            Tag = Tags.Persistent | Tags.TransitionUpdate;
            sprite.Play("spin");
            sprite.FlipX = player.Facing == Facings.Left;
            outline.Visible = true;
            wiggler.Start();
            dashRoutine.Replace(BoostRoutine(player, direction));
        }

        private IEnumerator BoostRoutine(Player player, Vector2 dir)
        {
            Booster booster = this;
            float angle = (-dir).Angle();
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && booster.BoostingPlayer)
            {
                booster.sprite.RenderPosition = player.Center + playerOffset;
                booster.loopingSfx.Position = booster.sprite.Position;
                if (booster.Scene.OnInterval(0.02f))
                    (booster.Scene as Level).ParticlesBG.Emit(booster.particleType, 2, player.Center - (dir * 3f) + new Vector2(0.0f, -2f), new Vector2(3f, 3f), angle);

                yield return null;
            }
            booster.PlayerReleased();
            if (player.StateMachine.State == 4)
                booster.sprite.Visible = false;

            while (booster.SceneAs<Level>().Transitioning)
                yield return null;

            booster.Tag = 0;
        }

        public void OnPlayerDashed(Vector2 direction)
        {
            if (!BoostingPlayer)
                return;

            BoostingPlayer = false;
        }

        public void PlayerReleased()
        {
            _ = Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_end" : "event:/game/04_cliffside/greenbooster_end", sprite.RenderPosition);
            sprite.Play("pop");
            cannotUseTimer = 0.0f;
            respawnTimer = RespawnTime;
            BoostingPlayer = false;
            wiggler.Stop();
            _ = loopingSfx.Stop();
        }

        public void PlayerDied()
        {
            if (!BoostingPlayer)
                return;

            PlayerReleased();
            dashRoutine.Active = false;
            Tag = 0;
        }

        public void Respawn()
        {
            _ = Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
            sprite.Position = Vector2.Zero;
            sprite.Play("loop", true);
            wiggler.Start();
            sprite.Visible = true;
            outline.Visible = false;
            AppearParticles();
        }

        public override void Update()
        {
            base.Update();
            if (cannotUseTimer > 0)
                cannotUseTimer -= Engine.DeltaTime;

            if (respawnTimer > 0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0)
                    Respawn();
            }
            if (!dashRoutine.Active && respawnTimer <= 0)
            {
                Vector2 target = Vector2.Zero;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && CollideCheck(entity))
                    target = entity.Center + playerOffset - Position;

                sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
            }
            if (!(sprite.CurrentAnimationID == "inside") || BoostingPlayer || CollideCheck<Player>())
                return;

            sprite.Play("loop");
        }

        public override void Render()
        {
            Vector2 position = sprite.Position;
            sprite.Position = position.Floor();
            if (sprite.CurrentAnimationID != "pop" && sprite.Visible)
            {
                sprite.DrawOutline();
            }

            base.Render();
            sprite.Position = position;
        }

        public override void Removed(Scene scene)
        {
            if (Ch9HubTransition)
            {
                Level level = scene as Level;
                foreach (Backdrop backdrop in level.Background.GetEach<Backdrop>("bright"))
                {
                    backdrop.ForceVisible = false;
                    backdrop.FadeAlphaMultiplier = 1f;
                }
                level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
                level.Session.BloomBaseAdd = 0.25f;
            }
            base.Removed(scene);
        }
    }
}
