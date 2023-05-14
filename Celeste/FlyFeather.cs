// Decompiled with JetBrains decompiler
// Type: Celeste.FlyFeather
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked(false)]
    public class FlyFeather : Entity
    {
        public static ParticleType P_Collect;
        public static ParticleType P_Boost;
        public static ParticleType P_Flying;
        public static ParticleType P_Respawn;
        private const float RespawnTime = 3f;
        private readonly Sprite sprite;
        private readonly Monocle.Image outline;
        private readonly Wiggler wiggler;
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private Level level;
        private readonly SineWave sine;
        private readonly bool shielded;
        private readonly bool singleUse;
        private readonly Wiggler shieldRadiusWiggle;
        private readonly Wiggler moveWiggle;
        private Vector2 moveWiggleDir;
        private float respawnTimer;

        public FlyFeather(Vector2 position, bool shielded, bool singleUse)
            : base(position)
        {
            this.shielded = shielded;
            this.singleUse = singleUse;
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(sprite = GFX.SpriteBank.Create("flyFeather"));
            Add(wiggler = Wiggler.Create(1f, 4f, v => sprite.Scale = Vector2.One * (float)(1.0 + ((double)v * 0.20000000298023224))));
            Add(bloom = new BloomPoint(0.5f, 20f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f).Randomize());
            Add(outline = new Monocle.Image(GFX.Game["objects/flyFeather/outline"]));
            _ = outline.CenterOrigin();
            outline.Visible = false;
            shieldRadiusWiggle = Wiggler.Create(0.5f, 4f);
            Add(shieldRadiusWiggle);
            moveWiggle = Wiggler.Create(0.8f, 2f);
            moveWiggle.StartZero = true;
            Add(moveWiggle);
            UpdateY();
        }

        public FlyFeather(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool(nameof(shielded)), data.Bool(nameof(singleUse)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0)
                {
                    Respawn();
                }
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0.0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }

        public override void Render()
        {
            base.Render();
            if (!shielded || !sprite.Visible)
            {
                return;
            }

            Draw.Circle(Position + sprite.Position, (float)(10.0 - ((double)shieldRadiusWiggle.Value * 2.0)), Color.White, 3);
        }

        private void Respawn()
        {
            if (Collidable)
            {
                return;
            }

            outline.Visible = false;
            Collidable = true;
            sprite.Visible = true;
            wiggler.Start();
            _ = Audio.Play("event:/game/06_reflection/feather_reappear", Position);
            level.ParticlesFG.Emit(FlyFeather.P_Respawn, 16, Position, Vector2.One * 2f);
        }

        private void UpdateY()
        {
            this.sprite.X = 0.0f;
            this.sprite.Y = bloom.Y = sine.Value * 2f;
            Sprite sprite = this.sprite;
            sprite.Position += moveWiggleDir * moveWiggle.Value * -8f;
        }

        private void OnPlayer(Player player)
        {
            Vector2 speed = player.Speed;
            if (shielded && !player.DashAttacking)
            {
                player.PointBounce(Center);
                moveWiggle.Start();
                shieldRadiusWiggle.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                _ = Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
            {
                bool flag = player.StateMachine.State == 19;
                if (!player.StartStarFly())
                {
                    return;
                }

                _ = !flag
                    ? Audio.Play(shielded ? "event:/game/06_reflection/feather_bubble_get" : "event:/game/06_reflection/feather_get", Position)
                    : Audio.Play(shielded ? "event:/game/06_reflection/feather_bubble_renew" : "event:/game/06_reflection/feather_renew", Position);

                Collidable = false;
                Add(new Coroutine(CollectRoutine(player, speed)));
                if (singleUse)
                {
                    return;
                }

                outline.Visible = true;
                respawnTimer = 3f;
            }
        }

        private IEnumerator CollectRoutine(Player player, Vector2 playerSpeed)
        {
            FlyFeather flyFeather = this;
            flyFeather.level.Shake();
            flyFeather.sprite.Visible = false;
            yield return 0.05f;
            float direction = !(playerSpeed != Vector2.Zero) ? (flyFeather.Position - player.Center).Angle() : playerSpeed.Angle();
            flyFeather.level.ParticlesFG.Emit(FlyFeather.P_Collect, 10, flyFeather.Position, Vector2.One * 6f);
            _ = SlashFx.Burst(flyFeather.Position, direction);
        }
    }
}
