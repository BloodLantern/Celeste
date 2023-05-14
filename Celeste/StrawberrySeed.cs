// Decompiled with JetBrains decompiler
// Type: Celeste.StrawberrySeed
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
    public class StrawberrySeed : Entity
    {
        public static ParticleType P_Burst;
        private const float LoseDelay = 0.25f;
        private const float LoseGraceTime = 0.15f;
        public Strawberry Strawberry;
        private Sprite sprite;
        private readonly Follower follower;
        private readonly Wiggler wiggler;
        private Platform attached;
        private readonly SineWave sine;
        private readonly Tween lightTween;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private readonly Shaker shaker;
        private readonly int index;
        private Vector2 start;
        private Player player;
        private Level level;
        private float canLoseTimer;
        private float loseTimer;
        private bool finished;
        private bool losing;
        private readonly bool ghost;

        public bool Collected => follower.HasLeader || finished;

        public StrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost)
            : base(position)
        {
            Strawberry = strawberry;
            Depth = -100;
            start = Position;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            this.index = index;
            this.ghost = ghost;
            Add(follower = new Follower(new Action(OnGainLeader), new Action(OnLoseLeader)));
            follower.FollowDelay = 0.2f;
            follower.PersistentFollow = false;
            Add(new StaticMover()
            {
                SolidChecker = s => s.CollideCheck(this),
                OnAttach = p =>
                {
                    Depth = -1000000;
                    Collider = new Hitbox(24f, 24f, -12f, -12f);
                    attached = p;
                    start = Position - p.Position;
                }
            });
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(wiggler = Wiggler.Create(0.5f, 4f, v => sprite.Scale = Vector2.One * (float)(1.0 + (0.20000000298023224 * (double)v))));
            Add(sine = new SineWave(0.5f).Randomize());
            Add(shaker = new Shaker(false));
            Add(bloom = new BloomPoint(1f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
        }

        public override void Awake(Scene scene)
        {
            level = scene as Level;
            base.Awake(scene);
            sprite = GFX.SpriteBank.Create(ghost ? "ghostberrySeed" : (level.Session.Area.Mode == AreaMode.CSide ? "goldberrySeed" : "strawberrySeed"));
            sprite.Position = new Vector2(sine.Value * 2f, sine.ValueOverTwo * 1f);
            Add(sprite);
            if (ghost)
            {
                sprite.Color = Color.White * 0.8f;
            }

            sprite.PlayOffset("idle", (float)(0.25 + ((1.0 - (index / (Scene.Tracker.CountEntities<StrawberrySeed>() + 1.0))) * 0.75)));
            sprite.OnFrameChange = s =>
            {
                if (!Visible || !(sprite.CurrentAnimationID == "idle") || sprite.CurrentAnimationFrame != 19)
                {
                    return;
                }

                _ = Audio.Play("event:/game/general/seed_pulse", Position, "count", index);
                lightTween.Start();
                _ = level.Displacement.AddBurst(Position, 0.6f, 8f, 20f, 0.2f);
            };
            StrawberrySeed.P_Burst.Color = sprite.Color;
        }

        public override void Update()
        {
            base.Update();
            if (!finished)
            {
                if (canLoseTimer > 0.0)
                {
                    canLoseTimer -= Engine.DeltaTime;
                }
                else if (follower.HasLeader && player.LoseShards)
                {
                    losing = true;
                }

                if (losing)
                {
                    if (loseTimer <= 0.0 || player.Speed.Y < 0.0)
                    {
                        player.Leader.LoseFollower(follower);
                        losing = false;
                    }
                    else if (player.LoseShards)
                    {
                        loseTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        loseTimer = 0.15f;
                        losing = false;
                    }
                }
                sprite.Position = new Vector2(sine.Value * 2f, sine.ValueOverTwo * 1f) + shaker.Value;
            }
            else
            {
                light.Alpha = Calc.Approach(light.Alpha, 0.0f, Engine.DeltaTime * 4f);
            }
        }

        private void OnPlayer(Player player)
        {
            _ = Audio.Play("event:/game/general/seed_touch", Position, "count", index);
            this.player = player;
            player.Leader.GainFollower(follower);
            Collidable = false;
            Depth = -1000000;
            bool flag = true;
            foreach (StrawberrySeed seed in Strawberry.Seeds)
            {
                if (!seed.follower.HasLeader)
                {
                    flag = false;
                }
            }
            if (!flag)
            {
                return;
            }

            Scene.Add(new CSGEN_StrawberrySeeds(Strawberry));
        }

        private void OnGainLeader()
        {
            wiggler.Start();
            canLoseTimer = 0.25f;
            loseTimer = 0.15f;
        }

        private void OnLoseLeader()
        {
            if (finished)
            {
                return;
            }

            Add(new Coroutine(ReturnRoutine()));
        }

        private IEnumerator ReturnRoutine()
        {
            StrawberrySeed strawberrySeed = this;
            _ = Audio.Play("event:/game/general/seed_poof", strawberrySeed.Position);
            strawberrySeed.Collidable = false;
            strawberrySeed.sprite.Scale = Vector2.One * 2f;
            yield return 0.05f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int index = 0; index < 6; ++index)
            {
                float num = Calc.Random.NextFloat(6.28318548f);
                strawberrySeed.level.ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, strawberrySeed.Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
            }
            strawberrySeed.Visible = false;
            yield return (float)(0.30000001192092896 + (strawberrySeed.index * 0.10000000149011612));
            _ = Audio.Play("event:/game/general/seed_reappear", strawberrySeed.Position, "count", strawberrySeed.index);
            strawberrySeed.Position = strawberrySeed.start;
            if (strawberrySeed.attached != null)
            {
                strawberrySeed.Position += strawberrySeed.attached.Position;
            }

            _ = strawberrySeed.shaker.ShakeFor(0.4f, false);
            strawberrySeed.sprite.Scale = Vector2.One;
            strawberrySeed.Visible = true;
            strawberrySeed.Collidable = true;
            _ = strawberrySeed.level.Displacement.AddBurst(strawberrySeed.Position, 0.2f, 8f, 28f, 0.2f);
        }

        public void OnAllCollected()
        {
            finished = true;
            follower.Leader.LoseFollower(follower);
            Depth = -2000002;
            Tag = (int)Tags.FrozenUpdate;
            wiggler.Start();
        }

        public void StartSpinAnimation(
            Vector2 averagePos,
            Vector2 centerPos,
            float angleOffset,
            float time)
        {
            float spinLerp = 0.0f;
            Vector2 start = Position;
            sprite.Play("noFlash");
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, time / 2f, true);
            tween1.OnUpdate = t => spinLerp = t.Eased;
            Add(tween1);
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, time, true);
            tween2.OnUpdate = t =>
            {
                float angleRadians = 1.57079637f + angleOffset - MathHelper.Lerp(0.0f, 32.2013245f, t.Eased);
                Position = Vector2.Lerp(start, Vector2.Lerp(averagePos, centerPos, spinLerp) + Calc.AngleToVector(angleRadians, 25f), spinLerp);
            };
            Add(tween2);
        }

        public void StartCombineAnimation(Vector2 centerPos, float time, ParticleSystem particleSystem)
        {
            Vector2 position = Position;
            float startAngle = Calc.Angle(centerPos, position);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BigBackIn, time, true);
            tween.OnUpdate = t => Position = centerPos + Calc.AngleToVector(MathHelper.Lerp(startAngle, startAngle - 6.28318548f, Ease.CubeIn(t.Percent)), MathHelper.Lerp(25f, 0.0f, t.Eased));
            tween.OnComplete = t =>
            {
                Visible = false;
                for (int index = 0; index < 6; ++index)
                {
                    float num = Calc.Random.NextFloat(6.28318548f);
                    particleSystem.Emit(StrawberrySeed.P_Burst, 1, Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
                }
                RemoveSelf();
            };
            Add(tween);
        }
    }
}
