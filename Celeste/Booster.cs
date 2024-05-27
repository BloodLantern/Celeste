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
        public static readonly Vector2 playerOffset = new Vector2(0.0f, -2f);
        private Sprite sprite;
        private Entity outline;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Coroutine dashRoutine;
        private DashListener dashListener;
        private ParticleType particleType;
        private float respawnTimer;
        private float cannotUseTimer;
        private bool red;
        private SoundSource loopingSfx;
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
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Color.White, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (float) (1.0 + f * 0.25)));
            Add(dashRoutine = new Coroutine(false));
            Add(dashListener = new DashListener());
            Add(new MirrorReflection());
            Add(loopingSfx = new SoundSource());
            dashListener.OnDash = OnPlayerDashed;
            particleType = red ? Booster.P_BurstRed : Booster.P_Burst;
        }

        public Booster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool(nameof (red)))
        {
            Ch9HubBooster = data.Bool("ch9_hub_booster");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Image image = new Image(GFX.Game["objects/booster/outline"]);
            image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            outline = new Entity(Position);
            outline.Depth = 8999;
            outline.Visible = false;
            outline.Add(image);
            outline.Add(new MirrorReflection());
            scene.Add(outline);
        }

        public void Appear()
        {
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
            sprite.Play("appear");
            wiggler.Start();
            Visible = true;
            AppearParticles();
        }

        private void AppearParticles()
        {
            ParticleSystem particlesBg = SceneAs<Level>().ParticlesBG;
            for (int index = 0; index < 360; index += 30)
                particlesBg.Emit(red ? Booster.P_RedAppear : Booster.P_Appear, 1, Center, Vector2.One * 2f, index * ((float) Math.PI / 180f));
        }

        private void OnPlayer(Player player)
        {
            if (respawnTimer > 0.0 || cannotUseTimer > 0.0 || BoostingPlayer)
                return;
            cannotUseTimer = 0.45f;
            if (red)
                player.RedBoost(this);
            else
                player.Boost(this);
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
            wiggler.Start();
            sprite.Play("inside");
            sprite.FlipX = player.Facing == Facings.Left;
        }

        public void PlayerBoosted(Player player, Vector2 direction)
        {
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", Position);
            if (red)
            {
                loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
                loopingSfx.DisposeOnTransition = false;
            }
            if (Ch9HubBooster && direction.Y < 0.0)
            {
                bool flag = true;
                List<LockBlock> all = Scene.Entities.FindAll<LockBlock>();
                if (all.Count > 0)
                {
                    foreach (LockBlock lockBlock in all)
                    {
                        if (!lockBlock.UnlockingRegistered)
                        {
                            flag = false;
                            break;
                        }
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
            Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
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
                booster.sprite.RenderPosition = player.Center + Booster.playerOffset;
                booster.loopingSfx.Position = booster.sprite.Position;
                if (booster.Scene.OnInterval(0.02f))
                    (booster.Scene as Level).ParticlesBG.Emit(booster.particleType, 2, player.Center - dir * 3f + new Vector2(0.0f, -2f), new Vector2(3f, 3f), angle);
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
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_end" : "event:/game/04_cliffside/greenbooster_end", sprite.RenderPosition);
            sprite.Play("pop");
            cannotUseTimer = 0.0f;
            respawnTimer = 1f;
            BoostingPlayer = false;
            wiggler.Stop();
            loopingSfx.Stop();
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
            Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
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
            if (cannotUseTimer > 0.0)
                cannotUseTimer -= Engine.DeltaTime;
            if (respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0)
                    Respawn();
            }
            if (!dashRoutine.Active && respawnTimer <= 0.0)
            {
                Vector2 target = Vector2.Zero;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && CollideCheck(entity))
                    target = entity.Center + Booster.playerOffset - Position;
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
                sprite.DrawOutline();
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
