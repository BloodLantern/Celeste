using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class Strawberry : Entity
    {
        public static ParticleType P_Glow;
        public static ParticleType P_GhostGlow;
        public static ParticleType P_GoldGlow;
        public static ParticleType P_MoonGlow;
        public static ParticleType P_WingsBurst;
        public EntityID ID;
        private Sprite sprite;
        public Follower Follower;
        private Wiggler wiggler;
        private Wiggler rotateWiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private float wobble;
        private Vector2 start;
        private float collectTimer;
        private bool collected;
        private readonly bool isGhostBerry;
        private bool flyingAway;
        private float flapSpeed;
        public bool ReturnHomeWhenLost = true;
        public bool WaitingOnSeeds;
        public List<StrawberrySeed> Seeds;

        public bool Winged { get; private set; }

        public bool Golden { get; private set; }

        public bool Moon { get; private set; }

        private string gotSeedFlag => "collected_seeds_of_" + ID;

        public Strawberry(EntityData data, Vector2 offset, EntityID gid)
        {
            ID = gid;
            Position = start = data.Position + offset;
            Winged = data.Bool("winged") || data.Name == "memorialTextController";
            Golden = data.Name is "memorialTextController" or "goldenBerry";
            Moon = data.Bool("moon");
            isGhostBerry = SaveData.Instance.CheckStrawberry(ID);
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, onLoseLeader: OnLoseLeader));
            Follower.FollowDelay = 0.3f;
            if (Winged)
                Add(new DashListener
                {
                    OnDash = OnDash
                });
            if (data.Nodes == null || data.Nodes.Length == 0)
                return;
            Seeds = new List<StrawberrySeed>();
            for (int index = 0; index < data.Nodes.Length; ++index)
                Seeds.Add(new StrawberrySeed(this, offset + data.Nodes[index], index, isGhostBerry));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SaveData.Instance.CheckStrawberry(ID))
            {
                sprite = !Moon ? (!Golden ? GFX.SpriteBank.Create("ghostberry") : GFX.SpriteBank.Create("goldghostberry")) : GFX.SpriteBank.Create("moonghostberry");
                sprite.Color = Color.White * 0.8f;
            }
            else
                sprite = !Moon ? (!Golden ? GFX.SpriteBank.Create("strawberry") : GFX.SpriteBank.Create("goldberry")) : GFX.SpriteBank.Create("moonberry");
            Add(sprite);
            if (Winged)
                sprite.Play("flap");
            sprite.OnFrameChange = OnAnimate;
            Add(wiggler = Wiggler.Create(0.4f, 4f, v => sprite.Scale = Vector2.One * (float)(1.0 + v * 0.34999999403953552)));
            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, v => sprite.Rotation = (float)(v * 30.0 * (Math.PI / 180.0))));
            Add(bloom = new BloomPoint(Golden || Moon || isGhostBerry ? 0.5f : 1f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
            if (Seeds != null && Seeds.Count > 0 && !(scene as Level).Session.GetFlag(gotSeedFlag))
            {
                foreach (StrawberrySeed seed in Seeds)
                    scene.Add(seed);
                Visible = false;
                Collidable = false;
                WaitingOnSeeds = true;
                bloom.Visible = light.Visible = false;
            }
            if ((scene as Level).Session.BloomBaseAdd <= 0.10000000149011612)
                return;
            bloom.Alpha *= 0.5f;
        }

        public override void Update()
        {
            if (WaitingOnSeeds)
                return;
            if (!collected)
            {
                if (!Winged)
                {
                    wobble += Engine.DeltaTime * 4f;
                    sprite.Y = bloom.Y = light.Y = (float) Math.Sin(wobble) * 2f;
                }
                int followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0.0 && IsFirstStrawberry)
                {
                    bool flag = false;
                    if (Follower.Leader.Entity is Player entity && entity.Scene != null && !entity.StrawberriesBlocked)
                    {
                        if (Golden)
                        {
                            if (entity.CollideCheck<GoldBerryCollectTrigger>() || (Scene as Level).Completed)
                                flag = true;
                        }
                        else if (entity.OnSafeGround && (!Moon || entity.StateMachine.State != 13))
                            flag = true;
                    }
                    if (flag)
                    {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15000000596046448)
                            OnCollect();
                    }
                    else
                        collectTimer = Math.Min(collectTimer, 0.0f);
                }
                else
                {
                    if (followIndex > 0)
                        collectTimer = -0.15f;
                    if (Winged)
                    {
                        Y += flapSpeed * Engine.DeltaTime;
                        if (flyingAway)
                        {
                            if ((double) Y < SceneAs<Level>().Bounds.Top - 16)
                                RemoveSelf();
                        }
                        else
                        {
                            flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
                            if (Y < start.Y - 5.0)
                                Y = start.Y - 5f;
                            else if (Y > start.Y + 5.0)
                                Y = start.Y + 5f;
                        }
                    }
                }
            }
            base.Update();
            if (Follower.Leader == null || !Scene.OnInterval(0.08f))
                return;
            ParticleType type = !isGhostBerry ? (!Golden ? (!Moon ? Strawberry.P_Glow : Strawberry.P_MoonGlow) : Strawberry.P_GoldGlow) : Strawberry.P_GhostGlow;
            SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }

        private void OnDash(Vector2 dir)
        {
            if (flyingAway || !Winged || WaitingOnSeeds)
                return;
            Depth = -1000000;
            Add(new Coroutine(FlyAwayRoutine()));
            flyingAway = true;
        }

        private bool IsFirstStrawberry
        {
            get
            {
                for (int index = Follower.FollowIndex - 1; index >= 0; --index)
                {
                    if (Follower.Leader.Followers[index].Entity is Strawberry entity && !entity.Golden)
                        return false;
                }
                return true;
            }
        }

        private void OnAnimate(string id)
        {
            if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4)
            {
                Audio.Play("event:/game/general/strawberry_wingflap", Position);
                flapSpeed = -50f;
            }
            if (sprite.CurrentAnimationFrame != (!(id == "flap") ? (!Golden ? (!Moon ? 35 : 30) : 30) : 25))
                return;
            lightTween.Start();
            if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
            }
            else
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            }
        }

        public void OnPlayer(Player player)
        {
            if (Follower.Leader != null || collected || WaitingOnSeeds)
                return;
            ReturnHomeWhenLost = true;
            if (Winged)
            {
                Level level = SceneAs<Level>();
                Winged = false;
                sprite.Rate = 0.0f;
                Alarm.Set(this, Follower.FollowDelay, () =>
                {
                    sprite.Rate = 1f;
                    sprite.Play("idle");
                    level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position + new Vector2(8f, 0.0f), new Vector2(4f, 2f));
                    level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position - new Vector2(8f, 0.0f), new Vector2(4f, 2f));
                });
            }
            if (Golden)
                (Scene as Level).Session.GrabbedGolden = true;
            Audio.Play(isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }

        public void OnCollect()
        {
            if (collected)
                return;
            int collectIndex = 0;
            collected = true;
            if (Follower.Leader != null)
            {
                Player entity = Follower.Leader.Entity as Player;
                collectIndex = entity.StrawberryCollectIndex;
                ++entity.StrawberryCollectIndex;
                entity.StrawberryCollectResetTimer = 2.5f;
                Follower.Leader.LoseFollower(Follower);
            }
            if (Moon)
                Achievements.Register(Achievement.WOW);
            SaveData.Instance.AddStrawberry(ID, Golden);
            Session session = (Scene as Level).Session;
            session.DoNotLoad.Add(ID);
            session.Strawberries.Add(ID);
            session.UpdateLevelStartDashes();
            Add(new Coroutine(CollectRoutine(collectIndex)));
        }

        private IEnumerator FlyAwayRoutine()
        {
            Strawberry strawberry = this;
            strawberry.rotateWiggler.Start();
            strawberry.flapSpeed = -200f;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, true);
            // ISSUE: reference to a compiler-generated method
            tween1.OnUpdate = delegate (Tween t)
            {
                    flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
            };
            strawberry.Add(tween1);
            yield return 0.1f;
            Audio.Play("event:/game/general/strawberry_laugh", strawberry.Position);
            yield return 0.2f;
            if (!strawberry.Follower.HasLeader)
                Audio.Play("event:/game/general/strawberry_flyaway", strawberry.Position);
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.5f, start: true);
            // ISSUE: reference to a compiler-generated method
            tween2.OnUpdate = delegate (Tween t)
            {
                    flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
            };
            strawberry.Add(tween2);
        }

        private IEnumerator CollectRoutine(int collectIndex)
        {
            Strawberry strawberry = this;
            Scene scene = strawberry.Scene;
            strawberry.Tag = (int) Tags.TransitionUpdate;
            strawberry.Depth = -2000010;
            int num = 0;
            if (strawberry.Moon)
                num = 3;
            else if (strawberry.isGhostBerry)
                num = 1;
            else if (strawberry.Golden)
                num = 2;
            Audio.Play("event:/game/general/strawberry_get", strawberry.Position, "colour", num, "count", collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            strawberry.sprite.Play("collect");
            while (strawberry.sprite.Animating)
                yield return null;
            strawberry.Scene.Add(new StrawberryPoints(strawberry.Position, strawberry.isGhostBerry, collectIndex, strawberry.Moon));
            strawberry.RemoveSelf();
        }

        private void OnLoseLeader()
        {
            if (collected || !ReturnHomeWhenLost)
                return;
            Alarm.Set(this, 0.15f, () =>
            {
                Vector2 vector = (start - Position).SafeNormalize();
                float val = Vector2.Distance(Position, start);
                float num = Calc.ClampedMap(val, 16f, 120f, 16f, 96f);
                SimpleCurve curve = new(Position, start, start + vector * 16f + vector.Perpendicular() * num * Calc.Random.Choose(1, -1));
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(val / 100f, 0.4f), true);
                tween.OnUpdate = f => Position = curve.GetPoint(f.Eased);
                tween.OnComplete = f => Depth = 0;
                Add(tween);
            });
        }

        public void CollectedSeeds()
        {
            WaitingOnSeeds = false;
            Visible = true;
            Collidable = true;
            bloom.Visible = light.Visible = true;
            (Scene as Level).Session.SetFlag(gotSeedFlag);
        }
    }
}
