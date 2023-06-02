using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Diagnostics;

namespace Celeste
{
    public class BadelineBoost : Entity
    {
        public static ParticleType P_Ambience;
        public static ParticleType P_Move;
        private const float MoveSpeed = 320f;
        private Sprite sprite;
        private Monocle.Image stretch;
        private Wiggler wiggler;
        private VertexLight light;
        private BloomPoint bloom;
        private bool canSkip;
        private bool finalCh9Boost;
        private bool finalCh9GoldenBoost;
        private bool finalCh9Dialog;
        private Vector2[] nodes;
        private int nodeIndex;
        private bool travelling;
        private Player holding;
        private SoundSource relocateSfx;
        public FMOD.Studio.EventInstance Ch9FinalBoostSfx;

        public BadelineBoost(
            Vector2[] nodes,
            bool lockCamera,
            bool canSkip = false,
            bool finalCh9Boost = false,
            bool finalCh9GoldenBoost = false,
            bool finalCh9Dialog = false)
            : base(nodes[0])
        {
            this.Depth = -1000000;
            this.nodes = nodes;
            this.canSkip = canSkip;
            this.finalCh9Boost = finalCh9Boost;
            this.finalCh9GoldenBoost = finalCh9GoldenBoost;
            this.finalCh9Dialog = finalCh9Dialog;
            this.Collider = (Collider) new Monocle.Circle(16f);
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) (this.sprite = GFX.SpriteBank.Create("badelineBoost")));
            this.Add((Component) (this.stretch = new Monocle.Image(GFX.Game["objects/badelineboost/stretch"])));
            this.stretch.Visible = false;
            this.stretch.CenterOrigin();
            this.Add((Component) (this.light = new VertexLight(Color.White, 0.7f, 12, 20)));
            this.Add((Component) (this.bloom = new BloomPoint(0.5f, 12f)));
            this.Add((Component) (this.wiggler = Wiggler.Create(0.4f, 3f, (Action<float>) (f => this.sprite.Scale = Vector2.One * (float) (1.0 + (double) this.wiggler.Value * 0.40000000596046448)))));
            if (lockCamera)
                this.Add((Component) new CameraLocker(Level.CameraLockModes.BoostSequence, 0.0f, 160f));
            this.Add((Component) (this.relocateSfx = new SoundSource()));
        }

        public BadelineBoost(EntityData data, Vector2 offset)
            : this(data.NodesWithPosition(offset), data.Bool("lockCamera", true), data.Bool(nameof (canSkip)), data.Bool(nameof (finalCh9Boost)), data.Bool(nameof (finalCh9GoldenBoost)), data.Bool(nameof (finalCh9Dialog)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!this.CollideCheck<FakeWall>())
                return;
            this.Depth = -12500;
        }

        private void OnPlayer(Player player) => this.Add((Component) new Coroutine(this.BoostRoutine(player)));

        private IEnumerator BoostRoutine(Player player)
        {
            BadelineBoost boost = this;
            boost.holding = player;
            boost.travelling = true;
            ++boost.nodeIndex;
            boost.sprite.Visible = false;
            boost.sprite.Position = Vector2.Zero;
            boost.Collidable = false;
            bool finalBoost = boost.nodeIndex >= boost.nodes.Length;
            Level level = boost.Scene as Level;
            bool endLevel = false;
            if (finalBoost && boost.finalCh9GoldenBoost)
            {
                endLevel = true;
            }
            else
            {
                bool flag = false;
                foreach (Component follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry entity && entity.Golden)
                    {
                        flag = true;
                        break;
                    }
                }
                endLevel = finalBoost && boost.finalCh9Boost && !flag;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (boost.finalCh9Boost)
                Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", boost.Position);
            else if (!finalBoost)
                Audio.Play("event:/char/badeline/booster_begin", boost.Position);
            else
                Audio.Play("event:/char/badeline/booster_final", boost.Position);
            if (player.Holding != null)
                player.Drop();
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            if (player.Inventory.Dashes > 1)
                player.Dashes = 1;
            else
                player.RefillDash();
            player.RefillStamina();
            player.Speed = Vector2.Zero;
            int num = Math.Sign(player.X - boost.X);
            if (num == 0)
                num = -1;
            BadelineDummy badeline = new BadelineDummy(boost.Position);
            boost.Scene.Add((Entity) badeline);
            player.Facing = (Facings) (-num);
            badeline.Sprite.Scale.X = (float) num;
            Vector2 playerFrom = player.Position;
            Vector2 playerTo = boost.Position + new Vector2((float) (num * 4), -3f);
            Vector2 badelineFrom = badeline.Position;
            Vector2 badelineTo = boost.Position + new Vector2((float) (-num * 4), 3f);
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.2f)
            {
                Vector2 vector2 = Vector2.Lerp(playerFrom, playerTo, p);
                if (player.Scene != null)
                    player.MoveToX(vector2.X);
                if (player.Scene != null)
                    player.MoveToY(vector2.Y);
                badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
                yield return (object) null;
            }
            playerFrom = new Vector2();
            playerTo = new Vector2();
            badelineFrom = new Vector2();
            badelineTo = new Vector2();
            if (finalBoost)
            {
                Vector2 screenSpaceFocusPoint = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
                boost.Add((Component) new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
                Engine.TimeRate = 0.5f;
            }
            else
                Audio.Play("event:/char/badeline/booster_throw", boost.Position);
            badeline.Sprite.Play("boost");
            yield return (object) 0.1f;
            if (!player.Dead)
                player.MoveV(5f);
            yield return (object) 0.1f;
            if (endLevel)
            {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            if (finalBoost && boost.finalCh9Boost)
            {
                boost.Scene.Add((Entity) new CS10_FinalLaunch(player, boost, boost.finalCh9Dialog));
                player.Active = false;
                badeline.Active = false;
                boost.Active = false;
                yield return (object) null;
                player.Active = true;
                badeline.Active = true;
            }
            boost.Add((Component) Alarm.Create(Alarm.AlarmMode.Oneshot, (Action) (() =>
            {
                if (player.Dashes < player.Inventory.Dashes)
                    ++player.Dashes;
                this.Scene.Remove((Entity) badeline);
                (this.Scene as Level).Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
            }), 0.15f, true));
            (boost.Scene as Level).Shake();
            boost.holding = (Player) null;
            if (!finalBoost)
            {
                player.BadelineBoostLaunch(boost.CenterX);
                Vector2 from = boost.Position;
                Vector2 to = boost.nodes[boost.nodeIndex];
                float duration = Math.Min(3f, Vector2.Distance(from, to) / 320f);
                boost.stretch.Visible = true;
                boost.stretch.Rotation = (to - from).Angle();
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
                tween.OnUpdate = (Action<Tween>) (t =>
                {
                    this.Position = Vector2.Lerp(from, to, t.Eased);
                    this.stretch.Scale.X = (float) (1.0 + (double) Calc.YoYo(t.Eased) * 2.0);
                    this.stretch.Scale.Y = (float) (1.0 - (double) Calc.YoYo(t.Eased) * 0.75);
                    if ((double) t.Eased >= 0.89999997615814209 || !this.Scene.OnInterval(0.03f))
                        return;
                    TrailManager.Add((Entity) this, Player.TwoDashesHairColor, 0.5f);
                    level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, this.Center, Vector2.One * 4f);
                });
                tween.OnComplete = (Action<Tween>) (t =>
                {
                    if ((double) this.X >= (double) level.Bounds.Right)
                    {
                        this.RemoveSelf();
                    }
                    else
                    {
                        this.travelling = false;
                        this.stretch.Visible = false;
                        this.sprite.Visible = true;
                        this.Collidable = true;
                        Audio.Play("event:/char/badeline/booster_reappear", this.Position);
                    }
                });
                boost.Add((Component) tween);
                boost.relocateSfx.Play("event:/char/badeline/booster_relocate");
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(-Vector2.UnitY);
                level.Displacement.AddBurst(boost.Center, 0.4f, 8f, 32f, 0.5f);
            }
            else
            {
                if (boost.finalCh9Boost)
                    boost.Ch9FinalBoostSfx = Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", boost.Position);
                Console.WriteLine("TIME: " + (object) sw.ElapsedMilliseconds);
                Engine.FreezeTimer = 0.1f;
                yield return (object) null;
                if (endLevel)
                    level.TimerHidden = true;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level.Flash(Color.White * 0.5f, true);
                level.DirectionalShake(-Vector2.UnitY, 0.6f);
                level.Displacement.AddBurst(boost.Center, 0.6f, 8f, 64f, 0.5f);
                level.ResetZoom();
                player.SummitLaunch(boost.X);
                Engine.TimeRate = 1f;
                boost.Finish();
            }
        }

        private void Skip()
        {
            this.travelling = true;
            ++this.nodeIndex;
            this.Collidable = false;
            Level level = this.SceneAs<Level>();
            Vector2 from = this.Position;
            Vector2 to = this.nodes[this.nodeIndex];
            float duration = Math.Min(3f, Vector2.Distance(from, to) / 320f);
            this.stretch.Visible = true;
            this.stretch.Rotation = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween.OnUpdate = (Action<Tween>) (t =>
            {
                this.Position = Vector2.Lerp(from, to, t.Eased);
                this.stretch.Scale.X = (float) (1.0 + (double) Calc.YoYo(t.Eased) * 2.0);
                this.stretch.Scale.Y = (float) (1.0 - (double) Calc.YoYo(t.Eased) * 0.75);
                if ((double) t.Eased >= 0.89999997615814209 || !this.Scene.OnInterval(0.03f))
                    return;
                TrailManager.Add((Entity) this, Player.TwoDashesHairColor, 0.5f);
                level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, this.Center, Vector2.One * 4f);
            });
            tween.OnComplete = (Action<Tween>) (t =>
            {
                if ((double) this.X >= (double) level.Bounds.Right)
                {
                    this.RemoveSelf();
                }
                else
                {
                    this.travelling = false;
                    this.stretch.Visible = false;
                    this.sprite.Visible = true;
                    this.Collidable = true;
                    Audio.Play("event:/char/badeline/booster_reappear", this.Position);
                }
            });
            this.Add((Component) tween);
            this.relocateSfx.Play("event:/char/badeline/booster_relocate");
            level.Displacement.AddBurst(this.Center, 0.4f, 8f, 32f, 0.5f);
        }

        public void Wiggle()
        {
            this.wiggler.Start();
            (this.Scene as Level).Displacement.AddBurst(this.Position, 0.3f, 4f, 16f, 0.25f);
            Audio.Play("event:/game/general/crystalheart_pulse", this.Position);
        }

        public override void Update()
        {
            if (this.sprite.Visible && this.Scene.OnInterval(0.05f))
                this.SceneAs<Level>().ParticlesBG.Emit(BadelineBoost.P_Ambience, 1, this.Center, Vector2.One * 3f);
            if (this.holding != null)
                this.holding.Speed = Vector2.Zero;
            if (!this.travelling)
            {
                Player entity = this.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    float num = Calc.ClampedMap((entity.Center - this.Position).Length(), 16f, 64f, 12f, 0.0f);
                    this.sprite.Position = Calc.Approach(this.sprite.Position, (entity.Center - this.Position).SafeNormalize() * num, 32f * Engine.DeltaTime);
                    if (this.canSkip && (double) entity.Position.X - (double) this.X >= 100.0 && this.nodeIndex + 1 < this.nodes.Length)
                        this.Skip();
                }
            }
            this.light.Visible = this.bloom.Visible = this.sprite.Visible || this.stretch.Visible;
            base.Update();
        }

        private void Finish()
        {
            this.SceneAs<Level>().Displacement.AddBurst(this.Center, 0.5f, 24f, 96f, 0.4f);
            this.SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, this.Center, Vector2.One * 6f);
            this.SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
            this.SceneAs<Level>().CameraOffset = new Vector2(0.0f, -16f);
            this.RemoveSelf();
        }
    }
}
