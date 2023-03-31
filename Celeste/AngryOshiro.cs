// Decompiled with JetBrains decompiler
// Type: Celeste.AngryOshiro
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
    public class AngryOshiro : Entity
    {
        private const int StChase = 0;
        private const int StChargeUp = 1;
        private const int StAttack = 2;
        private const int StDummy = 3;
        private const int StWaiting = 4;
        private const int StHurt = 5;
        private const float HitboxBackRange = 4f;
        public Sprite Sprite;
        private readonly Sprite lightning;
        private bool lightningVisible;
        private readonly VertexLight light;
        private Level level;
        private readonly SineWave sine;
        private float cameraXOffset;
        private readonly StateMachine state;
        private int attackIndex;
        private float targetAnxiety;
        private float anxietySpeed;
        private bool easeBackFromRightEdge;
        private readonly bool fromCutscene;
        private bool doRespawnAnim;
        private bool leaving;
        private readonly Shaker shaker;
        private readonly PlayerCollider bounceCollider;
        private Vector2 colliderTargetPosition;
        private bool canControlTimeRate = true;
        private readonly SoundSource prechargeSfx;
        private readonly SoundSource chargeSfx;
        private bool hasEnteredSfx;
        private const float minCameraOffsetX = -48f;
        private const float yApproachTargetSpeed = 100f;
        private float yApproachSpeed = 100f;
        private static readonly float[] ChaseWaitTimes = new float[5]
        {
            1f,
            2f,
            3f,
            2f,
            3f
        };
        private float attackSpeed;
        private const float HurtXSpeed = 100f;
        private const float HurtYSpeed = 200f;

        public AngryOshiro(Vector2 position, bool fromCutscene)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("oshiro_boss"));
            Sprite.Play("idle");
            Add(lightning = GFX.SpriteBank.Create("oshiro_boss_lightning"));
            lightning.Visible = false;
            lightning.OnFinish = s => lightningVisible = false;
            Collider = new Monocle.Circle(14f)
            {
                Position = colliderTargetPosition = new Vector2(3f, HitboxBackRange)
            };
            Add(sine = new SineWave(0.5f));
            Add(bounceCollider = new PlayerCollider(new Action<Player>(OnPlayerBounce), new Hitbox(28f, 6f, -11f, -11f)));
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Depth = -12500;
            Visible = false;
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(shaker = new Shaker(false));
            state = new StateMachine();
            state.SetCallbacks(StChase, new Func<int>(ChaseUpdate), new Func<IEnumerator>(ChaseCoroutine), new Action(ChaseBegin));
            state.SetCallbacks(StChargeUp, new Func<int>(ChargeUpUpdate), new Func<IEnumerator>(ChargeUpCoroutine), end: new Action(ChargeUpEnd));
            state.SetCallbacks(StAttack, new Func<int>(AttackUpdate), new Func<IEnumerator>(AttackCoroutine), new Action(AttackBegin), new Action(AttackEnd));
            state.SetCallbacks(StDummy, null);
            state.SetCallbacks(StWaiting, new Func<int>(WaitingUpdate));
            state.SetCallbacks(StHurt, new Func<int>(HurtUpdate), begin: new Action(HurtBegin));
            Add(state);
            if (fromCutscene)
            {
                yApproachSpeed = 0.0f;
            }

            this.fromCutscene = fromCutscene;
            Add(new TransitionListener()
            {
                OnOutBegin = () =>
                {
                    if ((double)X > level.Bounds.Left + ((double)Sprite.Width / 2.0))
                    {
                        Visible = false;
                    }
                    else
                    {
                        easeBackFromRightEdge = true;
                    }
                },
                OnOut = f =>
                {
                    lightning.Update();
                    if (!easeBackFromRightEdge)
                    {
                        return;
                    }

                    X -= 128f * Engine.RawDeltaTime;
                }
            });
            Add(prechargeSfx = new SoundSource());
            Add(chargeSfx = new SoundSource());
            Distort.AnxietyOrigin = new Vector2(1f, 0.5f);
        }

        public AngryOshiro(EntityData data, Vector2 offset)
            : this(data.Position + offset, false)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            if (level.Session.GetFlag("oshiroEnding") || (!level.Session.GetFlag("oshiro_resort_roof") && level.Session.Level.Equals("roof00")))
            {
                RemoveSelf();
            }

            if (state.State != StDummy && !fromCutscene)
            {
                state.State = StWaiting;
            }

            if (!fromCutscene)
            {
                Y = TargetY;
                cameraXOffset = -48f;
            }
            else
            {
                cameraXOffset = X - level.Camera.Left;
            }
        }

        private float TargetY
        {
            get
            {
                Player entity = level.Tracker.GetEntity<Player>();
                if (entity == null)
                {
                    return Y;
                }

                double centerY = (double)entity.CenterY;
                Rectangle bounds = level.Bounds;
                double min = bounds.Top + 8;
                bounds = level.Bounds;
                double max = bounds.Bottom - 8;
                return MathHelper.Clamp((float)centerY, (float)min, (float)max);
            }
        }

        private void OnPlayer(Player player)
        {
            if (state.State == StHurt || (CenterX >= player.CenterX + 4f && Sprite.CurrentAnimationID == "respawn"))
            {
                return;
            }

            _ = player.Die((player.Center - Center).SafeNormalize(Vector2.UnitX));
        }

        private void OnPlayerBounce(Player player)
        {
            if (state.State != StAttack || player.Bottom > Top + 6f)
            {
                return;
            }

            _ = Audio.Play("event:/game/general/thing_booped", Position);
            Celeste.Freeze(0.2f);
            player.Bounce(Top + 2f);
            state.State = StHurt;
            _ = prechargeSfx.Stop();
            _ = chargeSfx.Stop();
        }

        public override void Update()
        {
            base.Update();
            Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 0.6f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 0.6f * Engine.DeltaTime);
            if (!doRespawnAnim)
            {
                Visible = (double)X > level.Bounds.Left - ((double)Width / 2.0);
            }

            yApproachSpeed = Calc.Approach(yApproachSpeed, 100f, 300f * Engine.DeltaTime);
            if (state.State != StDummy && canControlTimeRate)
            {
                if (state.State == StAttack && attackSpeed > 200.0)
                {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    Engine.TimeRate = entity == null || entity.Dead || (double)CenterX >= (double)entity.CenterX + 4.0 ? 1f : MathHelper.Lerp(Calc.ClampedMap(entity.CenterX - CenterX, 30f, 80f, 0.5f), 1f, Calc.ClampedMap(Math.Abs(entity.CenterY - CenterY), 32f, 48f));
                }
                else
                {
                    Engine.TimeRate = 1f;
                }

                Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f), Engine.DeltaTime * 8f);
                Distort.Anxiety = Calc.Approach(Distort.Anxiety, targetAnxiety, anxietySpeed * Engine.DeltaTime);
            }
            else
            {
                Distort.GameRate = 1f;
                Distort.Anxiety = 0.0f;
            }
        }

        public void StopControllingTime()
        {
            canControlTimeRate = false;
        }

        public override void Render()
        {
            if (lightningVisible)
            {
                lightning.RenderPosition = new Vector2(level.Camera.Left - 2f, Top + 16f);
                lightning.Render();
            }
            Sprite.Position = shaker.Value * 2f;
            base.Render();
        }

        public void Leave()
        {
            leaving = true;
        }

        public void Squish()
        {
            Sprite.Scale = new Vector2(1.3f, 0.5f);
            _ = shaker.ShakeFor(0.5f, false);
        }

        private void ChaseBegin()
        {
            Sprite.Play("idle");
        }

        private int ChaseUpdate()
        {
            if (!hasEnteredSfx && cameraXOffset >= -16.0 && !doRespawnAnim)
            {
                _ = Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
                hasEnteredSfx = true;
            }
            if (doRespawnAnim && cameraXOffset >= 0.0)
            {
                Collider.Position.X = -48f;
                Visible = true;
                Sprite.Play("respawn");
                doRespawnAnim = false;
                if (Scene.Tracker.GetEntity<Player>() != null)
                {
                    _ = Audio.Play("event:/char/oshiro/boss_reform", Position);
                }
            }
            cameraXOffset = Calc.Approach(cameraXOffset, 20f, 80f * Engine.DeltaTime);
            X = level.Camera.Left + cameraXOffset;
            Collider.Position.X = Calc.Approach(Collider.Position.X, colliderTargetPosition.X, Engine.DeltaTime * 128f);
            Collidable = Visible;
            if (level.Tracker.GetEntity<Player>() != null && Sprite.CurrentAnimationID != "respawn")
            {
                CenterY = Calc.Approach(CenterY, TargetY, yApproachSpeed * Engine.DeltaTime);
            }

            return 0;
        }

        private IEnumerator ChaseCoroutine()
        {
            AngryOshiro angryOshiro = this;
            if (angryOshiro.level.Session.Area.Mode != AreaMode.Normal)
            {
                yield return 1f;
            }
            else
            {
                yield return ChaseWaitTimes[angryOshiro.attackIndex];
                ++angryOshiro.attackIndex;
                angryOshiro.attackIndex %= ChaseWaitTimes.Length;
            }
            _ = angryOshiro.prechargeSfx.Play("event:/char/oshiro/boss_precharge");
            angryOshiro.Sprite.Play("charge");
            yield return 0.7f;
            if (angryOshiro.Scene.Tracker.GetEntity<Player>() != null)
            {
                _ = Alarm.Set(angryOshiro, 0.216f, delegate
                {
                    _ = angryOshiro.chargeSfx.Play("event:/char/oshiro/boss_charge", null, 0f);
                });
                angryOshiro.state.State = StChargeUp;
            }
            else
            {
                angryOshiro.Sprite.Play("idle");
            }
        }

        private int ChargeUpUpdate()
        {
            if (level.OnInterval(0.05f))
            {
                Sprite.Position = Calc.Random.ShakeVector();
            }

            cameraXOffset = Calc.Approach(cameraXOffset, 0f, 40f * Engine.DeltaTime);
            X = level.Camera.Left + cameraXOffset;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                float centerY1 = CenterY;
                float centerY2 = entity.CenterY;
                Rectangle bounds = level.Bounds;
                float min = bounds.Top + 8;
                bounds = level.Bounds;
                float max = bounds.Bottom - 8;
                float target = MathHelper.Clamp(centerY2, min, max);
                float maxMove = 30f * Engine.DeltaTime;
                CenterY = Calc.Approach(centerY1, target, maxMove);
            }
            return StChargeUp;
        }

        private void ChargeUpEnd()
        {
            Sprite.Position = Vector2.Zero;
        }

        private IEnumerator ChargeUpCoroutine()
        {
            AngryOshiro angryOshiro = this;
            Celeste.Freeze(0.05f);
            Distort.Anxiety = 0.3f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            angryOshiro.lightningVisible = true;
            angryOshiro.lightning.Play("once", true);
            yield return 0.3f;
            angryOshiro.state.State = angryOshiro.Scene.Tracker.GetEntity<Player>() == null ? StChase : StAttack;
        }

        private void AttackBegin()
        {
            attackSpeed = 0.0f;
            targetAnxiety = 0.3f;
            anxietySpeed = 4f;
            level.DirectionalShake(Vector2.UnitX);
        }

        private void AttackEnd()
        {
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        private int AttackUpdate()
        {
            X += attackSpeed * Engine.DeltaTime;
            attackSpeed = Calc.Approach(attackSpeed, 500f, 2000f * Engine.DeltaTime);
            if (X >= level.Camera.Right + 48f)
            {
                if (leaving)
                {
                    RemoveSelf();
                    return StAttack;
                }
                X = level.Camera.Left - 48f;
                cameraXOffset = -48f;
                doRespawnAnim = true;
                Visible = false;
                return StChase;
            }
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            if (Scene.OnInterval(0.05f))
            {
                TrailManager.Add(this, Color.Red * 0.6f, 0.5f);
            }

            return StAttack;
        }

        private IEnumerator AttackCoroutine()
        {
            yield return 0.1f;
            targetAnxiety = 0.0f;
            anxietySpeed = 0.5f;
        }

        public bool DummyMode => state.State == StDummy;

        public void EnterDummyMode()
        {
            state.State = StDummy;
        }

        public void LeaveDummyMode()
        {
            state.State = StChase;
        }

        private int WaitingUpdate()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            return entity != null && entity.Speed != Vector2.Zero && (double)entity.X > level.Bounds.Left + 48 ? StChase : StWaiting;
        }

        private void HurtBegin()
        {
            Sprite.Play("hurt", true);
        }

        private int HurtUpdate()
        {
            X += 100f * Engine.DeltaTime;
            Y += 200f * Engine.DeltaTime;
            if (Top <= level.Bounds.Bottom + 20)
            {
                return StHurt;
            }

            if (leaving)
            {
                RemoveSelf();
                return StHurt;
            }
            X = level.Camera.Left - 48f;
            cameraXOffset = -48f;
            doRespawnAnim = true;
            Visible = false;
            return StChase;
        }
    }
}
