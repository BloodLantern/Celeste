// Decompiled with JetBrains decompiler
// Type: Celeste.FakeHeart
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class FakeHeart : Entity
    {
        private const float RespawnTime = 3f;
        private Sprite sprite;
        private ParticleType shineParticle;
        public Wiggler ScaleWiggler;
        private Wiggler moveWiggler;
        private Vector2 moveWiggleDir;
        private BloomPoint bloom;
        private VertexLight light;
        private readonly HoldableCollider crystalCollider;
        private float timer;
        private float bounceSfxDelay;
        private float respawnTimer;

        public FakeHeart(Vector2 position)
            : base(position)
        {
            Add(crystalCollider = new HoldableCollider(new Action<Holdable>(OnHoldable)));
            Add(new MirrorReflection());
        }

        public FakeHeart(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AreaMode areaMode = Calc.Random.Choose<AreaMode>(AreaMode.Normal, AreaMode.BSide, AreaMode.CSide);
            Add(sprite = GFX.SpriteBank.Create("heartgem" + (int)areaMode));
            sprite.Play("spin");
            sprite.OnLoop = anim =>
            {
                if (!Visible || !(anim == "spin"))
                {
                    return;
                }

                _ = Audio.Play("event:/game/general/crystalheart_pulse", Position);
                ScaleWiggler.Start();
                _ = (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
            };
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (float)(1.0 + ((double)f * 0.25))));
            Add(bloom = new BloomPoint(0.75f, 16f));
            Color color;
            switch (areaMode)
            {
                case AreaMode.Normal:
                    color = Color.Aqua;
                    shineParticle = HeartGem.P_BlueShine;
                    break;
                case AreaMode.BSide:
                    color = Color.Red;
                    shineParticle = HeartGem.P_RedShine;
                    break;
                default:
                    color = Color.Gold;
                    shineParticle = HeartGem.P_GoldShine;
                    break;
            }
            Add(light = new VertexLight(Color.Lerp(color, Color.White, 0.5f), 1f, 32, 64));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
        }

        public override void Update()
        {
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = (Vector2.UnitY * (float)Math.Sin(timer * 2.0) * 2f) + (moveWiggleDir * moveWiggler.Value * -8f);
            if (respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0)
                {
                    Collidable = Visible = true;
                    ScaleWiggler.Start();
                }
            }
            base.Update();
            if (!Visible || !Scene.OnInterval(0.1f))
            {
                return;
            }

            SceneAs<Level>().Particles.Emit(shineParticle, 1, Center, Vector2.One * 8f);
        }

        public void OnHoldable(Holdable h)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (!Visible || !h.Dangerous(crystalCollider))
            {
                return;
            }

            Collect(entity, h.GetSpeed().Angle());
        }

        public void OnPlayer(Player player)
        {
            if (!Visible || (Scene as Level).Frozen)
            {
                return;
            }

            if (player.DashAttacking)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collect(player, player.Speed.Angle());
            }
            else
            {
                if (bounceSfxDelay <= 0.0)
                {
                    _ = Audio.Play("event:/game/general/crystalheart_bounce", Position);
                    bounceSfxDelay = 0.1f;
                }
                player.PointBounce(Center);
                moveWiggler.Start();
                ScaleWiggler.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }

        private void Collect(Player player, float angle)
        {
            if (!Collidable)
            {
                return;
            }

            Collidable = Visible = false;
            respawnTimer = 3f;
            Celeste.Freeze(0.05f);
            SceneAs<Level>().Shake();
            _ = SlashFx.Burst(Position, angle);
            _ = (player?.RefillDash());
        }
    }
}
