// Decompiled with JetBrains decompiler
// Type: Celeste.BadelineBoost
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
        private readonly Sprite sprite;
        private readonly Image stretch;
        private readonly Wiggler wiggler;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private readonly bool canSkip;
        private readonly bool finalCh9Boost;
        private readonly bool finalCh9GoldenBoost;
        private readonly bool finalCh9Dialog;
        private readonly Vector2[] nodes;
        private int nodeIndex;
        private bool travelling;
        private Player holding;
        private readonly SoundSource relocateSfx;
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
            Depth = -1000000;
            this.nodes = nodes;
            this.canSkip = canSkip;
            this.finalCh9Boost = finalCh9Boost;
            this.finalCh9GoldenBoost = finalCh9GoldenBoost;
            this.finalCh9Dialog = finalCh9Dialog;
            Collider = new Circle(16f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(sprite = GFX.SpriteBank.Create("badelineBoost"));
            Add(stretch = new Image(GFX.Game["objects/badelineboost/stretch"]));
            stretch.Visible = false;
            _ = stretch.CenterOrigin();
            Add(light = new VertexLight(Color.White, 0.7f, 12, 20));
            Add(bloom = new BloomPoint(0.5f, 12f));
            Add(wiggler = Wiggler.Create(0.4f, 3f, f => sprite.Scale = Vector2.One * (1 + wiggler.Value * 0.4f)));
            if (lockCamera)
                Add(new CameraLocker(Level.CameraLockModes.BoostSequence, 0.0f, 160f));

            Add(relocateSfx = new SoundSource());
        }

        public BadelineBoost(EntityData data, Vector2 offset)
            : this(data.NodesWithPosition(offset), data.Bool("lockCamera", true), data.Bool(nameof(canSkip)), data.Bool(nameof(finalCh9Boost)), data.Bool(nameof(finalCh9GoldenBoost)), data.Bool(nameof(finalCh9Dialog)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CollideCheck<FakeWall>())
                return;

            Depth = -12500;
        }

        private void OnPlayer(Player player)
        {
            Add(new Coroutine(BoostRoutine(player)));
        }

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
                endLevel = true;
            else
            {
                bool flag = false;
                foreach (Component follower in player.Leader.Followers)
                    if (follower.Entity is Strawberry entity && entity.Golden)
                    {
                        flag = true;
                        break;
                    }
                endLevel = finalBoost && boost.finalCh9Boost && !flag;
            }
            Stopwatch sw = new();
            sw.Start();
            _ = boost.finalCh9Boost
                ? Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", boost.Position)
                : !finalBoost
                    ? Audio.Play("event:/char/badeline/booster_begin", boost.Position)
                    : Audio.Play("event:/char/badeline/booster_final", boost.Position);

            if (player.Holding != null)
                player.Drop();

            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            if (player.Inventory.Dashes > 1)
                player.Dashes = 1;
            else
                _ = player.RefillDash();

            player.RefillStamina();
            player.Speed = Vector2.Zero;
            int num = Math.Sign(player.X - boost.X);
            if (num == 0)
                num = -1;

            BadelineDummy badeline = new(boost.Position);
            boost.Scene.Add(badeline);
            player.Facing = (Facings)(-num);
            badeline.Sprite.Scale.X = num;
            Vector2 playerFrom = player.Position;
            Vector2 playerTo = boost.Position + new Vector2(num * 4, -3f);
            Vector2 badelineFrom = badeline.Position;
            Vector2 badelineTo = boost.Position + new Vector2(-num * 4, 3f);
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.2f)
            {
                Vector2 vector2 = Vector2.Lerp(playerFrom, playerTo, p);
                if (player.Scene != null)
                    player.MoveToX(vector2.X);

                if (player.Scene != null)
                    player.MoveToY(vector2.Y);

                badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
                yield return null;
            }
            playerFrom = new Vector2();
            playerTo = new Vector2();
            badelineFrom = new Vector2();
            badelineTo = new Vector2();
            if (finalBoost)
            {
                Vector2 screenSpaceFocusPoint = new(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
                boost.Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
                Engine.TimeRate = 0.5f;
            }
            else
                _ = Audio.Play("event:/char/badeline/booster_throw", boost.Position);

            badeline.Sprite.Play("boost");
            yield return 0.1f;
            if (!player.Dead)
                _ = player.MoveV(5f);

            yield return 0.1f;
            if (endLevel)
            {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            if (finalBoost && boost.finalCh9Boost)
            {
                boost.Scene.Add(new CS10_FinalLaunch(player, boost, boost.finalCh9Dialog));
                player.Active = false;
                badeline.Active = false;
                boost.Active = false;
                yield return null;
                player.Active = true;
                badeline.Active = true;
            }
            boost.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
            {
                if (player.Dashes < player.Inventory.Dashes)
                    ++player.Dashes;

                Scene.Remove(badeline);
                _ = (Scene as Level).Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
            }, 0.15f, true));
            (boost.Scene as Level).Shake();
            boost.holding = null;
            if (!finalBoost)
            {
                player.BadelineBoostLaunch(boost.CenterX);
                Vector2 from = boost.Position;
                Vector2 to = boost.nodes[boost.nodeIndex];
                float duration = Math.Min(3f, Vector2.Distance(from, to) / MoveSpeed);
                boost.stretch.Visible = true;
                boost.stretch.Rotation = (to - from).Angle();
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
                tween.OnUpdate = t =>
                {
                    Position = Vector2.Lerp(from, to, t.Eased);
                    stretch.Scale.X = 1 + Calc.YoYo(t.Eased) * 2;
                    stretch.Scale.Y = 1 - Calc.YoYo(t.Eased) * 0.75f;
                    if (t.Eased >= 0.9f || !Scene.OnInterval(0.03f))
                        return;

                    TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f);
                    level.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
                };
                tween.OnComplete = t =>
                {
                    if (X >= level.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                    else
                    {
                        travelling = false;
                        stretch.Visible = false;
                        sprite.Visible = true;
                        Collidable = true;
                        _ = Audio.Play("event:/char/badeline/booster_reappear", Position);
                    }
                };
                boost.Add(tween);
                _ = boost.relocateSfx.Play("event:/char/badeline/booster_relocate");
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(-Vector2.UnitY);
                _ = level.Displacement.AddBurst(boost.Center, 0.4f, 8f, 32f, 0.5f);
            }
            else
            {
                if (boost.finalCh9Boost)
                    boost.Ch9FinalBoostSfx = Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", boost.Position);

                Console.WriteLine("TIME: " + sw.ElapsedMilliseconds);
                Engine.FreezeTimer = 0.1f;
                yield return null;
                if (endLevel)
                    level.TimerHidden = true;

                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level.Flash(Color.White * 0.5f, true);
                level.DirectionalShake(-Vector2.UnitY, 0.6f);
                _ = level.Displacement.AddBurst(boost.Center, 0.6f, 8f, 64f, 0.5f);
                level.ResetZoom();
                player.SummitLaunch(boost.X);
                Engine.TimeRate = 1f;
                boost.Finish();
            }
        }

        private void Skip()
        {
            travelling = true;
            ++nodeIndex;
            Collidable = false;
            Level level = SceneAs<Level>();
            Vector2 from = Position;
            Vector2 to = nodes[nodeIndex];
            float duration = Math.Min(3f, Vector2.Distance(from, to) / MoveSpeed);
            stretch.Visible = true;
            stretch.Rotation = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween.OnUpdate = t =>
            {
                Position = Vector2.Lerp(from, to, t.Eased);
                stretch.Scale.X = 1 + Calc.YoYo(t.Eased) * 2;
                stretch.Scale.Y = 1 - Calc.YoYo(t.Eased) * 0.75f;
                if (t.Eased >= 0.9f || !Scene.OnInterval(0.03f))
                    return;

                TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f);
                level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, Center, Vector2.One * 4f);
            };
            tween.OnComplete = t =>
            {
                if (X >= level.Bounds.Right)
                {
                    RemoveSelf();
                }
                else
                {
                    travelling = false;
                    stretch.Visible = false;
                    sprite.Visible = true;
                    Collidable = true;
                    _ = Audio.Play("event:/char/badeline/booster_reappear", Position);
                }
            };
            Add(tween);
            _ = relocateSfx.Play("event:/char/badeline/booster_relocate");
            _ = level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
        }

        public void Wiggle()
        {
            wiggler.Start();
            _ = (Scene as Level).Displacement.AddBurst(Position, 0.3f, 4f, 16f, 0.25f);
            _ = Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public override void Update()
        {
            if (sprite.Visible && Scene.OnInterval(0.05f))
                SceneAs<Level>().ParticlesBG.Emit(P_Ambience, 1, Center, Vector2.One * 3f);

            if (holding != null)
                holding.Speed = Vector2.Zero;

            if (!travelling)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    float num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0.0f);
                    sprite.Position = Calc.Approach(sprite.Position, (entity.Center - Position).SafeNormalize() * num, 32f * Engine.DeltaTime);
                    if (canSkip && entity.Position.X - X >= 100 && nodeIndex + 1 < nodes.Length)
                        Skip();
                }
            }
            light.Visible = bloom.Visible = sprite.Visible || stretch.Visible;
            base.Update();
        }

        private void Finish()
        {
            _ = SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
            SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
            SceneAs<Level>().CameraOffset = new Vector2(0.0f, -16f);
            RemoveSelf();
        }
    }
}
