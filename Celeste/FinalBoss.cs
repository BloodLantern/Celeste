// Decompiled with JetBrains decompiler
// Type: Celeste.FinalBoss
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
    [Tracked(false)]
    public class FinalBoss : Entity
    {
        public static ParticleType P_Burst;
        public const float CameraXPastMax = 140f;
        private const float MoveSpeed = 600f;
        private const float AvoidRadius = 12f;
        public Sprite Sprite;
        public PlayerSprite NormalSprite;
        private PlayerHair normalHair;
        private Vector2 avoidPos;
        public float CameraYPastMax;
        public bool Moving;
        public bool Sitting;
        private int facing;
        private Level level;
        private readonly Monocle.Circle circle;
        private readonly Vector2[] nodes;
        private int nodeIndex;
        private readonly int patternIndex;
        private readonly Coroutine attackCoroutine;
        private readonly Coroutine triggerBlocksCoroutine;
        private List<Entity> fallingBlocks;
        private List<Entity> movingBlocks;
        private bool playerHasMoved;
        private readonly SineWave floatSine;
        private readonly bool dialog;
        private readonly bool startHit;
        private readonly VertexLight light;
        private readonly Wiggler scaleWiggler;
        private FinalBossStarfield bossBg;
        private readonly SoundSource chargeSfx;
        private readonly SoundSource laserSfx;

        public FinalBoss(
            Vector2 position,
            Vector2[] nodes,
            int patternIndex,
            float cameraYPastMax,
            bool dialog,
            bool startHit,
            bool cameraLockY)
            : base(position)
        {
            this.patternIndex = patternIndex;
            CameraYPastMax = cameraYPastMax;
            this.dialog = dialog;
            this.startHit = startHit;
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Collider = circle = new Monocle.Circle(14f, y: -6f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = Position;
            for (int index = 0; index < nodes.Length; ++index)
            {
                this.nodes[index + 1] = nodes[index];
            }

            attackCoroutine = new Coroutine(false);
            Add(attackCoroutine);
            triggerBlocksCoroutine = new Coroutine(false);
            Add(triggerBlocksCoroutine);
            Add(new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
            Add(floatSine = new SineWave(0.6f));
            Add(scaleWiggler = Wiggler.Create(0.6f, 3f));
            Add(chargeSfx = new SoundSource());
            Add(laserSfx = new SoundSource());
        }

        public FinalBoss(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.NodesOffset(offset), e.Int(nameof(patternIndex)), e.Float("cameraPastY", 120f), e.Bool(nameof(dialog)), e.Bool(nameof(startHit)), e.Bool("cameraLockY", true))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            if (patternIndex == 0)
            {
                NormalSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
                NormalSprite.Scale.X = -1f;
                NormalSprite.Play("laugh");
                normalHair = new PlayerHair(NormalSprite)
                {
                    Color = BadelineOldsite.HairColor,
                    Border = Color.Black,
                    Facing = Facings.Left
                };
                Add(normalHair);
                Add(NormalSprite);
            }
            else
            {
                CreateBossSprite();
            }

            bossBg = level.Background.Get<FinalBossStarfield>();
            if (patternIndex == 0 && !level.Session.GetFlag("boss_intro") && level.Session.Level.Equals("boss-00"))
            {
                level.Session.Audio.Music.Event = "event:/music/lvl2/phone_loop";
                level.Session.Audio.Apply();
                if (bossBg != null)
                {
                    bossBg.Alpha = 0.0f;
                }

                Sitting = true;
                Position.Y += 16f;
                NormalSprite.Play("pretendDead");
                NormalSprite.Scale.X = 1f;
            }
            else if (patternIndex == 0 && !level.Session.GetFlag("boss_mid") && level.Session.Level.Equals("boss-14"))
            {
                level.Add(new CS06_BossMid());
            }
            else if (startHit)
            {
                _ = Alarm.Set(this, 0.5f, () => OnPlayer(null));
            }

            light.Position = (Sprite ?? (GraphicsComponent)NormalSprite).Position + new Vector2(0.0f, -10f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            fallingBlocks = Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            fallingBlocks.Sort((a, b) => (int)((double)a.X - (double)b.X));
            movingBlocks = Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
            movingBlocks.Sort((a, b) => (int)((double)a.X - (double)b.X));
        }

        private void CreateBossSprite()
        {
            Add(Sprite = GFX.SpriteBank.Create("badeline_boss"));
            Sprite.OnFrameChange = anim =>
            {
                if (!(anim == "idle") || Sprite.CurrentAnimationFrame != 18)
                {
                    return;
                }

                _ = Audio.Play("event:/char/badeline/boss_idle_air", Position);
            };
            facing = -1;
            if (NormalSprite != null)
            {
                Sprite.Position = NormalSprite.Position;
                Remove(NormalSprite);
            }
            if (normalHair != null)
            {
                Remove(normalHair);
            }

            NormalSprite = null;
            normalHair = null;
        }

        public Vector2 BeamOrigin => Center + Sprite.Position + new Vector2(0.0f, -14f);

        public Vector2 ShotOrigin => Center + Sprite.Position + new Vector2(6f * Sprite.Scale.X, 2f);

        public override void Update()
        {
            base.Update();
            Sprite sprite = Sprite ?? NormalSprite;
            if (!Sitting)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (!Moving && entity != null)
                {
                    if (facing == -1 && (double)entity.X > (double)X + 20.0)
                    {
                        facing = 1;
                        scaleWiggler.Start();
                    }
                    else if (facing == 1 && (double)entity.X < (double)X - 20.0)
                    {
                        facing = -1;
                        scaleWiggler.Start();
                    }
                }
                if (!playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
                {
                    playerHasMoved = true;
                    if (patternIndex != 0)
                    {
                        StartAttacking();
                    }

                    TriggerMovingBlocks(0);
                }
                sprite.Position = !Moving
                    ? avoidPos + new Vector2(floatSine.Value * 3f, floatSine.ValueOverTwo * 4f)
                    : Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
                float radius = circle.Radius;
                circle.Radius = 6f;
                CollideFirst<DashBlock>()?.Break(Center, -Vector2.UnitY);
                circle.Radius = radius;
                if (!level.IsInBounds(Position, 24f))
                {
                    Active = Visible = Collidable = false;
                    return;
                }
                Vector2 target;
                if (!Moving && entity != null)
                {
                    float length = Calc.ClampedMap((Center - entity.Center).Length(), 32f, 88f, 12f, 0.0f);
                    target = (double)length > 0.0 ? (Center - entity.Center).SafeNormalize(length) : Vector2.Zero;
                }
                else
                {
                    target = Vector2.Zero;
                }

                avoidPos = Calc.Approach(avoidPos, target, 40f * Engine.DeltaTime);
            }
            light.Position = sprite.Position + new Vector2(0.0f, -10f);
        }

        public override void Render()
        {
            if (Sprite != null)
            {
                Sprite.Scale.X = facing;
                Sprite.Scale.Y = 1f;
                Sprite sprite = Sprite;
                sprite.Scale *= (float)(1.0 + ((double)scaleWiggler.Value * 0.20000000298023224));
            }
            if (NormalSprite != null)
            {
                Vector2 position = NormalSprite.Position;
                NormalSprite.Position = NormalSprite.Position.Floor();
                base.Render();
                NormalSprite.Position = position;
            }
            else
            {
                base.Render();
            }
        }

        public void OnPlayer(Player player)
        {
            if (Sprite == null)
            {
                CreateBossSprite();
            }

            Sprite.Play("getHit");
            _ = Audio.Play("event:/char/badeline/boss_hug", Position);
            _ = chargeSfx.Stop();
            if (laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && laserSfx.Playing)
            {
                _ = laserSfx.Stop();
            }

            Collidable = false;
            avoidPos = Vector2.Zero;
            ++nodeIndex;
            if (dialog)
            {
                if (nodeIndex == 1)
                {
                    Scene.Add(new MiniTextbox("ch6_boss_tired_a"));
                }
                else if (nodeIndex == 2)
                {
                    Scene.Add(new MiniTextbox("ch6_boss_tired_b"));
                }
                else if (nodeIndex == 3)
                {
                    Scene.Add(new MiniTextbox("ch6_boss_tired_c"));
                }
            }
            foreach (FinalBossShot entity in level.Tracker.GetEntities<FinalBossShot>())
            {
                entity.Destroy();
            }

            foreach (FinalBossBeam entity in level.Tracker.GetEntities<FinalBossBeam>())
            {
                entity.Destroy();
            }

            TriggerFallingBlocks(X);
            TriggerMovingBlocks(nodeIndex);
            attackCoroutine.Active = false;
            Moving = true;
            bool lastHit = nodeIndex == nodes.Length - 1;
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                if (lastHit && level.Session.Level.Equals("boss-19"))
                {
                    _ = Alarm.Set(this, 0.25f, () =>
                    {
                        _ = Audio.Play("event:/game/06_reflection/boss_spikes_burst");
                        foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                        {
                            entity.Destroy(true);
                        }
                    });
                    Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 1f);
                    _ = Audio.SetMusic(null);
                }
                else if (startHit && level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_glitch")
                {
                    level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_glitch";
                    level.Session.Audio.Apply();
                }
                else if (level.Session.Audio.Music.Event is not "event:/music/lvl6/badeline_fight" and not "event:/music/lvl6/badeline_glitch")
                {
                    level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_fight";
                    level.Session.Audio.Apply();
                }
            }
            Add(new Coroutine(MoveSequence(player, lastHit)));
        }

        private IEnumerator MoveSequence(Player player, bool lastHit)
        {
            FinalBoss finalBoss = this;
            if (lastHit)
            {
                Audio.SetMusicParam("boss_pitch", 1f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = t => Glitch.Value = 0.6f * t.Eased;
                finalBoss.Add(tween);
            }
            else
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = t => Glitch.Value = (float)(0.5 * (1.0 - (double)t.Eased));
                finalBoss.Add(tween);
            }
            if (player != null && !player.Dead)
            {
                player.StartAttract(finalBoss.Center + (Vector2.UnitY * 4f));
            }

            float timer = 0.15f;
            while (player != null && !player.Dead && !player.AtAttractTarget)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            if ((double)timer > 0.0)
            {
                yield return timer;
            }

            foreach (ReflectionTentacles entity in finalBoss.Scene.Tracker.GetEntities<ReflectionTentacles>())
            {
                entity.Retreat();
            }

            if (player != null)
            {
                Celeste.Freeze(0.1f);
                Engine.TimeRate = !lastHit ? 0.75f : 0.5f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            finalBoss.PushPlayer(player);
            finalBoss.level.Shake();
            yield return 0.05f;
            for (float direction = 0.0f; (double)direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = finalBoss.Center + finalBoss.Sprite.Position + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(16, 20));
                finalBoss.level.Particles.Emit(FinalBoss.P_Burst, position, direction);
            }
            yield return 0.05f;
            Audio.SetMusicParam("boss_pitch", 0.0f);
            float from1 = Engine.TimeRate;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.35f / Engine.TimeRateB, start: true);
            tween1.UseRawDeltaTime = true;
            tween1.OnUpdate = t =>
            {
                if (bossBg != null && bossBg.Alpha < (double)t.Eased)
                {
                    bossBg.Alpha = t.Eased;
                }

                Engine.TimeRate = MathHelper.Lerp(from1, 1f, t.Eased);
                if (!lastHit)
                {
                    return;
                }

                Glitch.Value = (float)(0.60000002384185791 * (1.0 - (double)t.Eased));
            };
            finalBoss.Add(tween1);
            yield return 0.2f;
            Vector2 from2 = finalBoss.Position;
            Vector2 to = finalBoss.nodes[finalBoss.nodeIndex];
            float duration = Vector2.Distance(from2, to) / 600f;
            float dir = (to - from2).Angle();
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween2.OnUpdate = t =>
            {
                Position = Vector2.Lerp(from2, to, t.Eased);
                if ((double)t.Eased < 0.10000000149011612 || (double)t.Eased > 0.89999997615814209 || !Scene.OnInterval(0.02f))
                {
                    return;
                }

                TrailManager.Add(this, Player.NormalHairColor, 0.5f);
                level.Particles.Emit(Player.P_DashB, 2, Center, Vector2.One * 3f, dir);
            };
            tween2.OnComplete = t =>
            {
                Sprite.Play("recoverHit");
                Moving = false;
                Collidable = true;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    facing = Math.Sign(entity.X - X);
                    if (facing == 0)
                    {
                        facing = -1;
                    }
                }
                StartAttacking();
                floatSine.Reset();
            };
            finalBoss.Add(tween2);
        }

        private void PushPlayer(Player player)
        {
            if (player != null && !player.Dead)
            {
                int dir = Math.Sign(X - nodes[nodeIndex].X);
                if (dir == 0)
                {
                    dir = -1;
                }

                player.FinalBossPushLaunch(dir);
            }
            _ = SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            _ = SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            _ = SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        }

        private void TriggerFallingBlocks(float leftOfX)
        {
            while (fallingBlocks.Count > 0 && fallingBlocks[0].Scene == null)
            {
                fallingBlocks.RemoveAt(0);
            }

            int num = 0;
            while (fallingBlocks.Count > 0 && (double)fallingBlocks[0].X < (double)leftOfX)
            {
                FallingBlock fallingBlock = fallingBlocks[0] as FallingBlock;
                fallingBlock.StartShaking();
                fallingBlock.Triggered = true;
                fallingBlock.FallDelay = 0.4f * num;
                ++num;
                fallingBlocks.RemoveAt(0);
            }
        }

        private void TriggerMovingBlocks(int nodeIndex)
        {
            if (nodeIndex > 0)
            {
                DestroyMovingBlocks(nodeIndex - 1);
            }

            float delay = 0.0f;
            foreach (FinalBossMovingBlock movingBlock in movingBlocks)
            {
                if (movingBlock.BossNodeIndex == nodeIndex)
                {
                    movingBlock.StartMoving(delay);
                    delay += 0.15f;
                }
            }
        }

        private void DestroyMovingBlocks(int nodeIndex)
        {
            float delay = 0.0f;
            foreach (FinalBossMovingBlock movingBlock in movingBlocks)
            {
                if (movingBlock.BossNodeIndex == nodeIndex)
                {
                    movingBlock.Destroy(delay);
                    delay += 0.05f;
                }
            }
        }

        private void StartAttacking()
        {
            switch (patternIndex)
            {
                case 0:
                case 1:
                    attackCoroutine.Replace(Attack01Sequence());
                    break;
                case 2:
                    attackCoroutine.Replace(Attack02Sequence());
                    break;
                case 3:
                    attackCoroutine.Replace(Attack03Sequence());
                    break;
                case 4:
                    attackCoroutine.Replace(Attack04Sequence());
                    break;
                case 5:
                    attackCoroutine.Replace(Attack05Sequence());
                    break;
                case 6:
                    attackCoroutine.Replace(Attack06Sequence());
                    break;
                case 7:
                    attackCoroutine.Replace(Attack07Sequence());
                    break;
                case 8:
                    attackCoroutine.Replace(Attack08Sequence());
                    break;
                case 9:
                    attackCoroutine.Replace(Attack09Sequence());
                    break;
                case 10:
                    attackCoroutine.Replace(Attack10Sequence());
                    break;
                case 11:
                    attackCoroutine.Replace(Attack11Sequence());
                    break;
                case 13:
                    attackCoroutine.Replace(Attack13Sequence());
                    break;
                case 14:
                    attackCoroutine.Replace(Attack14Sequence());
                    break;
                case 15:
                    attackCoroutine.Replace(Attack15Sequence());
                    break;
            }
        }

        private void StartShootCharge()
        {
            Sprite.Play("attack1Begin");
            _ = chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        private IEnumerator Attack01Sequence()
        {
            StartShootCharge();
            while (true)
            {
                yield return 0.5f;
                Shoot();
                yield return 1f;
                StartShootCharge();
                yield return 0.15f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack02Sequence()
        {
            while (true)
            {
                yield return 0.5f;
                yield return Beam();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.3f;
                Shoot();
                yield return 0.5f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack03Sequence()
        {
            StartShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; ++i)
                {
                    Player entity = level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                        _ = new Vector2();
                    }
                    if (i < 4)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 2f;
                StartShootCharge();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack04Sequence()
        {
            StartShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; ++i)
                {
                    Player entity = level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                        _ = new Vector2();
                    }
                    if (i < 4)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 1.5f;
                yield return Beam();
                yield return 1.5f;
                StartShootCharge();
            }
        }

        private IEnumerator Attack05Sequence()
        {
            yield return 0.2f;
            while (true)
            {
                yield return Beam();
                yield return 0.6f;
                StartShootCharge();
                yield return 0.3f;
                for (int i = 0; i < 3; ++i)
                {
                    Player entity = level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                        _ = new Vector2();
                    }
                    if (i < 2)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 0.8f;
            }
        }

        private IEnumerator Attack06Sequence()
        {
            while (true)
            {
                yield return Beam();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack07Sequence()
        {
            while (true)
            {
                Shoot();
                yield return 0.8f;
                StartShootCharge();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack08Sequence()
        {
            while (true)
            {
                yield return 0.1f;
                yield return Beam();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack09Sequence()
        {
            StartShootCharge();
            while (true)
            {
                yield return 0.5f;
                Shoot();
                yield return 0.15f;
                StartShootCharge();
                Shoot();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.1f;
            }
        }

        private IEnumerator Attack10Sequence()
        {
            yield break;
        }

        private IEnumerator Attack11Sequence()
        {
            if (nodeIndex == 0)
            {
                StartShootCharge();
                yield return 0.6f;
            }
            while (true)
            {
                Shoot();
                yield return 1.9f;
                StartShootCharge();
                yield return 0.6f;
            }
        }

        private IEnumerator Attack13Sequence()
        {
            if (nodeIndex != 0)
            {
                yield return Attack01Sequence();
            }
        }

        private IEnumerator Attack14Sequence()
        {
            while (true)
            {
                yield return 0.2f;
                yield return Beam();
                yield return 0.3f;
            }
        }

        private IEnumerator Attack15Sequence()
        {
            while (true)
            {
                yield return 0.2f;
                yield return Beam();
                yield return 1.2f;
            }
        }

        private void Shoot(float angleOffset = 0.0f)
        {
            _ = !chargeSfx.Playing ? chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f) : chargeSfx.Param("end", 1f);

            Sprite.Play("attack1Recoil", true);
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            level.Add(Engine.Pooler.Create<FinalBossShot>().Init(this, entity, angleOffset));
        }

        private void ShootAt(Vector2 at)
        {
            _ = !chargeSfx.Playing ? chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f) : chargeSfx.Param("end", 1f);

            Sprite.Play("attack1Recoil", true);
            level.Add(Engine.Pooler.Create<FinalBossShot>().Init(this, at));
        }

        private IEnumerator Beam()
        {
            FinalBoss boss = this;
            _ = boss.laserSfx.Play("event:/char/badeline/boss_laser_charge");
            boss.Sprite.Play("attack2Begin", true);
            yield return 0.1f;
            Player entity = boss.level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                boss.level.Add(Engine.Pooler.Create<FinalBossBeam>().Init(boss, entity));
            }

            yield return 0.9f;
            boss.Sprite.Play("attack2Lock", true);
            yield return 0.5f;
            _ = boss.laserSfx.Stop();
            _ = Audio.Play("event:/char/badeline/boss_laser_fire", boss.Position);
            boss.Sprite.Play("attack2Recoil");
        }

        public override void Removed(Scene scene)
        {
            if (bossBg != null && patternIndex == 0)
            {
                bossBg.Alpha = 1f;
            }

            base.Removed(scene);
        }
    }
}
