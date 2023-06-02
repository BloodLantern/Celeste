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
        private Monocle.Circle circle;
        private Vector2[] nodes;
        private int nodeIndex;
        private int patternIndex;
        private Coroutine attackCoroutine;
        private Coroutine triggerBlocksCoroutine;
        private List<Entity> fallingBlocks;
        private List<Entity> movingBlocks;
        private bool playerHasMoved;
        private SineWave floatSine;
        private bool dialog;
        private bool startHit;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private FinalBossStarfield bossBg;
        private SoundSource chargeSfx;
        private SoundSource laserSfx;

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
            this.CameraYPastMax = cameraYPastMax;
            this.dialog = dialog;
            this.startHit = startHit;
            this.Add((Component) (this.light = new VertexLight(Color.White, 1f, 32, 64)));
            this.Collider = (Collider) (this.circle = new Monocle.Circle(14f, y: -6f));
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = this.Position;
            for (int index = 0; index < nodes.Length; ++index)
                this.nodes[index + 1] = nodes[index];
            this.attackCoroutine = new Coroutine(false);
            this.Add((Component) this.attackCoroutine);
            this.triggerBlocksCoroutine = new Coroutine(false);
            this.Add((Component) this.triggerBlocksCoroutine);
            this.Add((Component) new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
            this.Add((Component) (this.floatSine = new SineWave(0.6f)));
            this.Add((Component) (this.scaleWiggler = Wiggler.Create(0.6f, 3f)));
            this.Add((Component) (this.chargeSfx = new SoundSource()));
            this.Add((Component) (this.laserSfx = new SoundSource()));
        }

        public FinalBoss(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.NodesOffset(offset), e.Int(nameof (patternIndex)), e.Float("cameraPastY", 120f), e.Bool(nameof (dialog)), e.Bool(nameof (startHit)), e.Bool("cameraLockY", true))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.level = this.SceneAs<Level>();
            if (this.patternIndex == 0)
            {
                this.NormalSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
                this.NormalSprite.Scale.X = -1f;
                this.NormalSprite.Play("laugh");
                this.normalHair = new PlayerHair(this.NormalSprite);
                this.normalHair.Color = BadelineOldsite.HairColor;
                this.normalHair.Border = Color.Black;
                this.normalHair.Facing = Facings.Left;
                this.Add((Component) this.normalHair);
                this.Add((Component) this.NormalSprite);
            }
            else
                this.CreateBossSprite();
            this.bossBg = this.level.Background.Get<FinalBossStarfield>();
            if (this.patternIndex == 0 && !this.level.Session.GetFlag("boss_intro") && this.level.Session.Level.Equals("boss-00"))
            {
                this.level.Session.Audio.Music.Event = "event:/music/lvl2/phone_loop";
                this.level.Session.Audio.Apply();
                if (this.bossBg != null)
                    this.bossBg.Alpha = 0.0f;
                this.Sitting = true;
                this.Position.Y += 16f;
                this.NormalSprite.Play("pretendDead");
                this.NormalSprite.Scale.X = 1f;
            }
            else if (this.patternIndex == 0 && !this.level.Session.GetFlag("boss_mid") && this.level.Session.Level.Equals("boss-14"))
                this.level.Add((Entity) new CS06_BossMid());
            else if (this.startHit)
                Alarm.Set((Entity) this, 0.5f, (Action) (() => this.OnPlayer((Player) null)));
            this.light.Position = (this.Sprite != null ? (GraphicsComponent) this.Sprite : (GraphicsComponent) this.NormalSprite).Position + new Vector2(0.0f, -10f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.fallingBlocks = this.Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            this.fallingBlocks.Sort((Comparison<Entity>) ((a, b) => (int) ((double) a.X - (double) b.X)));
            this.movingBlocks = this.Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
            this.movingBlocks.Sort((Comparison<Entity>) ((a, b) => (int) ((double) a.X - (double) b.X)));
        }

        private void CreateBossSprite()
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("badeline_boss")));
            this.Sprite.OnFrameChange = (Action<string>) (anim =>
            {
                if (!(anim == "idle") || this.Sprite.CurrentAnimationFrame != 18)
                    return;
                Audio.Play("event:/char/badeline/boss_idle_air", this.Position);
            });
            this.facing = -1;
            if (this.NormalSprite != null)
            {
                this.Sprite.Position = this.NormalSprite.Position;
                this.Remove((Component) this.NormalSprite);
            }
            if (this.normalHair != null)
                this.Remove((Component) this.normalHair);
            this.NormalSprite = (PlayerSprite) null;
            this.normalHair = (PlayerHair) null;
        }

        public Vector2 BeamOrigin => this.Center + this.Sprite.Position + new Vector2(0.0f, -14f);

        public Vector2 ShotOrigin => this.Center + this.Sprite.Position + new Vector2(6f * this.Sprite.Scale.X, 2f);

        public override void Update()
        {
            base.Update();
            Sprite sprite = this.Sprite != null ? this.Sprite : (Sprite) this.NormalSprite;
            if (!this.Sitting)
            {
                Player entity = this.Scene.Tracker.GetEntity<Player>();
                if (!this.Moving && entity != null)
                {
                    if (this.facing == -1 && (double) entity.X > (double) this.X + 20.0)
                    {
                        this.facing = 1;
                        this.scaleWiggler.Start();
                    }
                    else if (this.facing == 1 && (double) entity.X < (double) this.X - 20.0)
                    {
                        this.facing = -1;
                        this.scaleWiggler.Start();
                    }
                }
                if (!this.playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
                {
                    this.playerHasMoved = true;
                    if (this.patternIndex != 0)
                        this.StartAttacking();
                    this.TriggerMovingBlocks(0);
                }
                if (!this.Moving)
                    sprite.Position = this.avoidPos + new Vector2(this.floatSine.Value * 3f, this.floatSine.ValueOverTwo * 4f);
                else
                    sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
                float radius = this.circle.Radius;
                this.circle.Radius = 6f;
                this.CollideFirst<DashBlock>()?.Break(this.Center, -Vector2.UnitY);
                this.circle.Radius = radius;
                if (!this.level.IsInBounds(this.Position, 24f))
                {
                    this.Active = this.Visible = this.Collidable = false;
                    return;
                }
                Vector2 target;
                if (!this.Moving && entity != null)
                {
                    float length = Calc.ClampedMap((this.Center - entity.Center).Length(), 32f, 88f, 12f, 0.0f);
                    target = (double) length > 0.0 ? (this.Center - entity.Center).SafeNormalize(length) : Vector2.Zero;
                }
                else
                    target = Vector2.Zero;
                this.avoidPos = Calc.Approach(this.avoidPos, target, 40f * Engine.DeltaTime);
            }
            this.light.Position = sprite.Position + new Vector2(0.0f, -10f);
        }

        public override void Render()
        {
            if (this.Sprite != null)
            {
                this.Sprite.Scale.X = (float) this.facing;
                this.Sprite.Scale.Y = 1f;
                Sprite sprite = this.Sprite;
                sprite.Scale = sprite.Scale * (float) (1.0 + (double) this.scaleWiggler.Value * 0.20000000298023224);
            }
            if (this.NormalSprite != null)
            {
                Vector2 position = this.NormalSprite.Position;
                this.NormalSprite.Position = this.NormalSprite.Position.Floor();
                base.Render();
                this.NormalSprite.Position = position;
            }
            else
                base.Render();
        }

        public void OnPlayer(Player player)
        {
            if (this.Sprite == null)
                this.CreateBossSprite();
            this.Sprite.Play("getHit");
            Audio.Play("event:/char/badeline/boss_hug", this.Position);
            this.chargeSfx.Stop();
            if (this.laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && this.laserSfx.Playing)
                this.laserSfx.Stop();
            this.Collidable = false;
            this.avoidPos = Vector2.Zero;
            ++this.nodeIndex;
            if (this.dialog)
            {
                if (this.nodeIndex == 1)
                    this.Scene.Add((Entity) new MiniTextbox("ch6_boss_tired_a"));
                else if (this.nodeIndex == 2)
                    this.Scene.Add((Entity) new MiniTextbox("ch6_boss_tired_b"));
                else if (this.nodeIndex == 3)
                    this.Scene.Add((Entity) new MiniTextbox("ch6_boss_tired_c"));
            }
            foreach (FinalBossShot entity in this.level.Tracker.GetEntities<FinalBossShot>())
                entity.Destroy();
            foreach (FinalBossBeam entity in this.level.Tracker.GetEntities<FinalBossBeam>())
                entity.Destroy();
            this.TriggerFallingBlocks(this.X);
            this.TriggerMovingBlocks(this.nodeIndex);
            this.attackCoroutine.Active = false;
            this.Moving = true;
            bool lastHit = this.nodeIndex == this.nodes.Length - 1;
            if (this.level.Session.Area.Mode == AreaMode.Normal)
            {
                if (lastHit && this.level.Session.Level.Equals("boss-19"))
                {
                    Alarm.Set((Entity) this, 0.25f, (Action) (() =>
                    {
                        Audio.Play("event:/game/06_reflection/boss_spikes_burst");
                        foreach (CrystalStaticSpinner entity in this.Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                            entity.Destroy(true);
                    }));
                    Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 1f);
                    Audio.SetMusic((string) null);
                }
                else if (this.startHit && this.level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_glitch")
                {
                    this.level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_glitch";
                    this.level.Session.Audio.Apply();
                }
                else if (this.level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_fight" && this.level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_glitch")
                {
                    this.level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_fight";
                    this.level.Session.Audio.Apply();
                }
            }
            this.Add((Component) new Coroutine(this.MoveSequence(player, lastHit)));
        }

        private IEnumerator MoveSequence(Player player, bool lastHit)
        {
            FinalBoss finalBoss = this;
            if (lastHit)
            {
                Audio.SetMusicParam("boss_pitch", 1f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = (Action<Tween>) (t => Glitch.Value = 0.6f * t.Eased);
                finalBoss.Add((Component) tween);
            }
            else
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = (Action<Tween>) (t => Glitch.Value = (float) (0.5 * (1.0 - (double) t.Eased)));
                finalBoss.Add((Component) tween);
            }
            if (player != null && !player.Dead)
                player.StartAttract(finalBoss.Center + Vector2.UnitY * 4f);
            float timer = 0.15f;
            while (player != null && !player.Dead && !player.AtAttractTarget)
            {
                yield return (object) null;
                timer -= Engine.DeltaTime;
            }
            if ((double) timer > 0.0)
                yield return (object) timer;
            foreach (ReflectionTentacles entity in finalBoss.Scene.Tracker.GetEntities<ReflectionTentacles>())
                entity.Retreat();
            if (player != null)
            {
                Celeste.Freeze(0.1f);
                Engine.TimeRate = !lastHit ? 0.75f : 0.5f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            finalBoss.PushPlayer(player);
            finalBoss.level.Shake();
            yield return (object) 0.05f;
            for (float direction = 0.0f; (double) direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = finalBoss.Center + finalBoss.Sprite.Position + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f), (float) Calc.Random.Range(16, 20));
                finalBoss.level.Particles.Emit(FinalBoss.P_Burst, position, direction);
            }
            yield return (object) 0.05f;
            Audio.SetMusicParam("boss_pitch", 0.0f);
            float from1 = Engine.TimeRate;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, duration: (0.35f / Engine.TimeRateB), start: true);
            tween1.UseRawDeltaTime = true;
            tween1.OnUpdate = (Action<Tween>) (t =>
            {
                if (this.bossBg != null && (double) this.bossBg.Alpha < (double) t.Eased)
                    this.bossBg.Alpha = t.Eased;
                Engine.TimeRate = MathHelper.Lerp(from1, 1f, t.Eased);
                if (!lastHit)
                    return;
                Glitch.Value = (float) (0.60000002384185791 * (1.0 - (double) t.Eased));
            });
            finalBoss.Add((Component) tween1);
            yield return (object) 0.2f;
            Vector2 from2 = finalBoss.Position;
            Vector2 to = finalBoss.nodes[finalBoss.nodeIndex];
            float duration = Vector2.Distance(from2, to) / 600f;
            float dir = (to - from2).Angle();
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween2.OnUpdate = (Action<Tween>) (t =>
            {
                this.Position = Vector2.Lerp(from2, to, t.Eased);
                if ((double) t.Eased < 0.10000000149011612 || (double) t.Eased > 0.89999997615814209 || !this.Scene.OnInterval(0.02f))
                    return;
                TrailManager.Add((Entity) this, Player.NormalHairColor, 0.5f);
                this.level.Particles.Emit(Player.P_DashB, 2, this.Center, Vector2.One * 3f, dir);
            });
            tween2.OnComplete = (Action<Tween>) (t =>
            {
                this.Sprite.Play("recoverHit");
                this.Moving = false;
                this.Collidable = true;
                Player entity = this.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    this.facing = Math.Sign(entity.X - this.X);
                    if (this.facing == 0)
                        this.facing = -1;
                }
                this.StartAttacking();
                this.floatSine.Reset();
            });
            finalBoss.Add((Component) tween2);
        }

        private void PushPlayer(Player player)
        {
            if (player != null && !player.Dead)
            {
                int dir = Math.Sign(this.X - this.nodes[this.nodeIndex].X);
                if (dir == 0)
                    dir = -1;
                player.FinalBossPushLaunch(dir);
            }
            this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.4f, 12f, 36f, 0.5f);
            this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.4f, 24f, 48f, 0.5f);
            this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.4f, 36f, 60f, 0.5f);
        }

        private void TriggerFallingBlocks(float leftOfX)
        {
            while (this.fallingBlocks.Count > 0 && this.fallingBlocks[0].Scene == null)
                this.fallingBlocks.RemoveAt(0);
            int num = 0;
            while (this.fallingBlocks.Count > 0 && (double) this.fallingBlocks[0].X < (double) leftOfX)
            {
                FallingBlock fallingBlock = this.fallingBlocks[0] as FallingBlock;
                fallingBlock.StartShaking();
                fallingBlock.Triggered = true;
                fallingBlock.FallDelay = 0.4f * (float) num;
                ++num;
                this.fallingBlocks.RemoveAt(0);
            }
        }

        private void TriggerMovingBlocks(int nodeIndex)
        {
            if (nodeIndex > 0)
                this.DestroyMovingBlocks(nodeIndex - 1);
            float delay = 0.0f;
            foreach (FinalBossMovingBlock movingBlock in this.movingBlocks)
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
            foreach (FinalBossMovingBlock movingBlock in this.movingBlocks)
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
            switch (this.patternIndex)
            {
                case 0:
                case 1:
                    this.attackCoroutine.Replace(this.Attack01Sequence());
                    break;
                case 2:
                    this.attackCoroutine.Replace(this.Attack02Sequence());
                    break;
                case 3:
                    this.attackCoroutine.Replace(this.Attack03Sequence());
                    break;
                case 4:
                    this.attackCoroutine.Replace(this.Attack04Sequence());
                    break;
                case 5:
                    this.attackCoroutine.Replace(this.Attack05Sequence());
                    break;
                case 6:
                    this.attackCoroutine.Replace(this.Attack06Sequence());
                    break;
                case 7:
                    this.attackCoroutine.Replace(this.Attack07Sequence());
                    break;
                case 8:
                    this.attackCoroutine.Replace(this.Attack08Sequence());
                    break;
                case 9:
                    this.attackCoroutine.Replace(this.Attack09Sequence());
                    break;
                case 10:
                    this.attackCoroutine.Replace(this.Attack10Sequence());
                    break;
                case 11:
                    this.attackCoroutine.Replace(this.Attack11Sequence());
                    break;
                case 13:
                    this.attackCoroutine.Replace(this.Attack13Sequence());
                    break;
                case 14:
                    this.attackCoroutine.Replace(this.Attack14Sequence());
                    break;
                case 15:
                    this.attackCoroutine.Replace(this.Attack15Sequence());
                    break;
            }
        }

        private void StartShootCharge()
        {
            this.Sprite.Play("attack1Begin");
            this.chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        private IEnumerator Attack01Sequence()
        {
            this.StartShootCharge();
            while (true)
            {
                yield return (object) 0.5f;
                this.Shoot();
                yield return (object) 1f;
                this.StartShootCharge();
                yield return (object) 0.15f;
                yield return (object) 0.3f;
            }
        }

        private IEnumerator Attack02Sequence()
        {
            while (true)
            {
                yield return (object) 0.5f;
                yield return (object) this.Beam();
                yield return (object) 0.4f;
                this.StartShootCharge();
                yield return (object) 0.3f;
                this.Shoot();
                yield return (object) 0.5f;
                yield return (object) 0.3f;
            }
        }

        private IEnumerator Attack03Sequence()
        {
            this.StartShootCharge();
            yield return (object) 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; ++i)
                {
                    Player entity = this.level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            this.ShootAt(at);
                            yield return (object) 0.15f;
                        }
                        at = new Vector2();
                    }
                    if (i < 4)
                    {
                        this.StartShootCharge();
                        yield return (object) 0.5f;
                    }
                }
                yield return (object) 2f;
                this.StartShootCharge();
                yield return (object) 0.7f;
            }
        }

        private IEnumerator Attack04Sequence()
        {
            this.StartShootCharge();
            yield return (object) 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; ++i)
                {
                    Player entity = this.level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            this.ShootAt(at);
                            yield return (object) 0.15f;
                        }
                        at = new Vector2();
                    }
                    if (i < 4)
                    {
                        this.StartShootCharge();
                        yield return (object) 0.5f;
                    }
                }
                yield return (object) 1.5f;
                yield return (object) this.Beam();
                yield return (object) 1.5f;
                this.StartShootCharge();
            }
        }

        private IEnumerator Attack05Sequence()
        {
            yield return (object) 0.2f;
            while (true)
            {
                yield return (object) this.Beam();
                yield return (object) 0.6f;
                this.StartShootCharge();
                yield return (object) 0.3f;
                for (int i = 0; i < 3; ++i)
                {
                    Player entity = this.level.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; ++j)
                        {
                            this.ShootAt(at);
                            yield return (object) 0.15f;
                        }
                        at = new Vector2();
                    }
                    if (i < 2)
                    {
                        this.StartShootCharge();
                        yield return (object) 0.5f;
                    }
                }
                yield return (object) 0.8f;
            }
        }

        private IEnumerator Attack06Sequence()
        {
            while (true)
            {
                yield return (object) this.Beam();
                yield return (object) 0.7f;
            }
        }

        private IEnumerator Attack07Sequence()
        {
            while (true)
            {
                this.Shoot();
                yield return (object) 0.8f;
                this.StartShootCharge();
                yield return (object) 0.8f;
            }
        }

        private IEnumerator Attack08Sequence()
        {
            while (true)
            {
                yield return (object) 0.1f;
                yield return (object) this.Beam();
                yield return (object) 0.8f;
            }
        }

        private IEnumerator Attack09Sequence()
        {
            this.StartShootCharge();
            while (true)
            {
                yield return (object) 0.5f;
                this.Shoot();
                yield return (object) 0.15f;
                this.StartShootCharge();
                this.Shoot();
                yield return (object) 0.4f;
                this.StartShootCharge();
                yield return (object) 0.1f;
            }
        }

        private IEnumerator Attack10Sequence()
        {
            yield break;
        }

        private IEnumerator Attack11Sequence()
        {
            if (this.nodeIndex == 0)
            {
                this.StartShootCharge();
                yield return (object) 0.6f;
            }
            while (true)
            {
                this.Shoot();
                yield return (object) 1.9f;
                this.StartShootCharge();
                yield return (object) 0.6f;
            }
        }

        private IEnumerator Attack13Sequence()
        {
            if (this.nodeIndex != 0)
                yield return (object) this.Attack01Sequence();
        }

        private IEnumerator Attack14Sequence()
        {
            while (true)
            {
                yield return (object) 0.2f;
                yield return (object) this.Beam();
                yield return (object) 0.3f;
            }
        }

        private IEnumerator Attack15Sequence()
        {
            while (true)
            {
                yield return (object) 0.2f;
                yield return (object) this.Beam();
                yield return (object) 1.2f;
            }
        }

        private void Shoot(float angleOffset = 0.0f)
        {
            if (!this.chargeSfx.Playing)
                this.chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                this.chargeSfx.Param("end", 1f);
            this.Sprite.Play("attack1Recoil", true);
            Player entity = this.level.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            this.level.Add((Entity) Engine.Pooler.Create<FinalBossShot>().Init(this, entity, angleOffset));
        }

        private void ShootAt(Vector2 at)
        {
            if (!this.chargeSfx.Playing)
                this.chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                this.chargeSfx.Param("end", 1f);
            this.Sprite.Play("attack1Recoil", true);
            this.level.Add((Entity) Engine.Pooler.Create<FinalBossShot>().Init(this, at));
        }

        private IEnumerator Beam()
        {
            FinalBoss boss = this;
            boss.laserSfx.Play("event:/char/badeline/boss_laser_charge");
            boss.Sprite.Play("attack2Begin", true);
            yield return (object) 0.1f;
            Player entity = boss.level.Tracker.GetEntity<Player>();
            if (entity != null)
                boss.level.Add((Entity) Engine.Pooler.Create<FinalBossBeam>().Init(boss, entity));
            yield return (object) 0.9f;
            boss.Sprite.Play("attack2Lock", true);
            yield return (object) 0.5f;
            boss.laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", boss.Position);
            boss.Sprite.Play("attack2Recoil");
        }

        public override void Removed(Scene scene)
        {
            if (this.bossBg != null && this.patternIndex == 0)
                this.bossBg.Alpha = 1f;
            base.Removed(scene);
        }
    }
}
