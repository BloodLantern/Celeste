// Decompiled with JetBrains decompiler
// Type: Celeste.Refill
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Refill : Entity
    {
        public static ParticleType P_Shatter;
        public static ParticleType P_Regen;
        public static ParticleType P_Glow;
        public static ParticleType P_ShatterTwo;
        public static ParticleType P_RegenTwo;
        public static ParticleType P_GlowTwo;
        private readonly Sprite sprite;
        private readonly Sprite flash;
        private readonly Monocle.Image outline;
        private readonly Wiggler wiggler;
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private Level level;
        private readonly SineWave sine;
        private readonly bool twoDashes;
        private readonly bool oneUse;
        private readonly ParticleType p_shatter;
        private readonly ParticleType p_regen;
        private readonly ParticleType p_glow;
        private float respawnTimer;

        public Refill(Vector2 position, bool twoDashes, bool oneUse)
            : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            this.twoDashes = twoDashes;
            this.oneUse = oneUse;
            string str;
            if (twoDashes)
            {
                str = "objects/refillTwo/";
                p_shatter = Refill.P_ShatterTwo;
                p_regen = Refill.P_RegenTwo;
                p_glow = Refill.P_GlowTwo;
            }
            else
            {
                str = "objects/refill/";
                p_shatter = Refill.P_Shatter;
                p_regen = Refill.P_Regen;
                p_glow = Refill.P_Glow;
            }
            Add(outline = new Monocle.Image(GFX.Game[str + nameof(outline)]));
            _ = outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            _ = sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + nameof(flash)));
            flash.Add(nameof(flash), "", 0.05f);
            flash.OnFinish = anim => flash.Visible = false;
            _ = flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, v => sprite.Scale = flash.Scale = Vector2.One * (float)(1.0 + ((double)v * 0.20000000298023224))));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f));
            _ = sine.Randomize();
            UpdateY();
            Depth = -100;
        }

        public Refill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("twoDash"), data.Bool(nameof(oneUse)))
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
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }

            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0.0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (!Scene.OnInterval(2f) || !sprite.Visible)
            {
                return;
            }

            flash.Play("flash", true);
            flash.Visible = true;
        }

        private void Respawn()
        {
            if (Collidable)
            {
                return;
            }

            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            Depth = -100;
            wiggler.Start();
            _ = Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }

        private void UpdateY()
        {
            flash.Y = sprite.Y = bloom.Y = sine.Value * 2f;
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }

            base.Render();
        }

        private void OnPlayer(Player player)
        {
            if (!player.UseRefill(twoDashes))
            {
                return;
            }

            _ = Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Refill refill = this;
            Celeste.Freeze(0.05f);
            yield return null;
            refill.level.Shake();
            refill.sprite.Visible = refill.flash.Visible = false;
            if (!refill.oneUse)
            {
                refill.outline.Visible = true;
            }

            refill.Depth = 8999;
            yield return 0.05f;
            float direction = player.Speed.Angle();
            refill.level.ParticlesFG.Emit(refill.p_shatter, 5, refill.Position, Vector2.One * 4f, direction - 1.57079637f);
            refill.level.ParticlesFG.Emit(refill.p_shatter, 5, refill.Position, Vector2.One * 4f, direction + 1.57079637f);
            _ = SlashFx.Burst(refill.Position, direction);
            if (refill.oneUse)
            {
                refill.RemoveSelf();
            }
        }
    }
}
