﻿// Decompiled with JetBrains decompiler
// Type: Celeste.Bumper
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Bumper : Entity
    {
        public static ParticleType P_Ambience;
        public static ParticleType P_Launch;
        public static ParticleType P_FireAmbience;
        public static ParticleType P_FireHit;
        private const float RespawnTime = 0.6f;
        private const float MoveCycleTime = 1.81818187f;
        private const float SineCycleFreq = 0.44f;
        private readonly Sprite sprite;
        private readonly Sprite spriteEvil;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private Vector2? node;
        private bool goBack;
        private Vector2 anchor;
        private readonly SineWave sine;
        private float respawnTimer;
        private bool fireMode;
        private readonly Wiggler hitWiggler;
        private Vector2 hitDir;

        public Bumper(Vector2 position, Vector2? node)
            : base(position)
        {
            Collider = new Monocle.Circle(12f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(sine = new SineWave(0.44f).Randomize());
            Add(sprite = GFX.SpriteBank.Create("bumper"));
            Add(spriteEvil = GFX.SpriteBank.Create("bumper_evil"));
            spriteEvil.Visible = false;
            Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
            this.node = node;
            anchor = Position;
            if (node.HasValue)
            {
                Vector2 start = Position;
                Vector2 end = node.Value;
                Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeInOut, 1.81818187f, true);
                tween.OnUpdate = t =>
                {
                    anchor = goBack ? Vector2.Lerp(end, start, t.Eased) : Vector2.Lerp(start, end, t.Eased);
                };
                tween.OnComplete = t => goBack = !goBack;
                Add(tween);
            }
            UpdatePosition();
            Add(hitWiggler = Wiggler.Create(1.2f, 2f, v => spriteEvil.Position = hitDir * hitWiggler.Value * 8f));
            Add(new CoreModeListener(new Action<Session.CoreModes>(OnChangeMode)));
        }

        public Bumper(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.FirstNodeNullable(new Vector2?(offset)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        private void OnChangeMode(Session.CoreModes coreMode)
        {
            fireMode = coreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        private void UpdatePosition()
        {
            Position = anchor + new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f);
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0.0)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0.0)
                {
                    light.Visible = true;
                    bloom.Visible = true;
                    sprite.Play("on");
                    spriteEvil.Play("on");
                    if (!fireMode)
                    {
                        _ = Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                    }
                }
            }
            else if (Scene.OnInterval(0.05f))
            {
                float angleRadians = Calc.Random.NextAngle();
                ParticleType type = fireMode ? Bumper.P_FireAmbience : Bumper.P_Ambience;
                float direction = fireMode ? -1.57079637f : angleRadians;
                float length = fireMode ? 12f : 8f;
                SceneAs<Level>().Particles.Emit(type, 1, Center + Calc.AngleToVector(angleRadians, length), Vector2.One * 2f, direction);
            }
            UpdatePosition();
        }

        private void OnPlayer(Player player)
        {
            if (fireMode)
            {
                if (SaveData.Instance.Assists.Invincible)
                {
                    return;
                }

                Vector2 vector2 = (player.Center - Center).SafeNormalize();
                hitDir = -vector2;
                hitWiggler.Start();
                _ = Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                respawnTimer = 0.6f;
                _ = player.Die(vector2);
                SceneAs<Level>().Particles.Emit(Bumper.P_FireHit, 12, Center + (vector2 * 12f), Vector2.One * 3f, vector2.Angle());
            }
            else
            {
                if (respawnTimer > 0.0)
                {
                    return;
                }

                _ = (Scene as Level).Session.Area.ID == 9
                    ? Audio.Play("event:/game/09_core/pinballbumper_hit", Position)
                    : Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);

                respawnTimer = 0.6f;
                Vector2 vector2 = player.ExplodeLaunch(Position, false);
                sprite.Play("hit", true);
                spriteEvil.Play("hit", true);
                light.Visible = false;
                bloom.Visible = false;
                SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                _ = SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
                SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, Center + (vector2 * 12f), Vector2.One * 3f, vector2.Angle());
            }
        }
    }
}
