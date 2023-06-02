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
        private Follower follower;
        private Wiggler wiggler;
        private Platform attached;
        private SineWave sine;
        private Tween lightTween;
        private VertexLight light;
        private BloomPoint bloom;
        private Shaker shaker;
        private int index;
        private Vector2 start;
        private Player player;
        private Level level;
        private float canLoseTimer;
        private float loseTimer;
        private bool finished;
        private bool losing;
        private bool ghost;

        public bool Collected => this.follower.HasLeader || this.finished;

        public StrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost)
            : base(position)
        {
            this.Strawberry = strawberry;
            this.Depth = -100;
            this.start = this.Position;
            this.Collider = (Collider) new Hitbox(12f, 12f, -6f, -6f);
            this.index = index;
            this.ghost = ghost;
            this.Add((Component) (this.follower = new Follower(new Action(this.OnGainLeader), new Action(this.OnLoseLeader))));
            this.follower.FollowDelay = 0.2f;
            this.follower.PersistentFollow = false;
            this.Add((Component) new StaticMover()
            {
                SolidChecker = (Func<Solid, bool>) (s => s.CollideCheck((Entity) this)),
                OnAttach = (Action<Platform>) (p =>
                {
                    this.Depth = -1000000;
                    this.Collider = (Collider) new Hitbox(24f, 24f, -12f, -12f);
                    this.attached = p;
                    this.start = this.Position - p.Position;
                })
            });
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) (this.wiggler = Wiggler.Create(0.5f, 4f, (Action<float>) (v => this.sprite.Scale = Vector2.One * (float) (1.0 + 0.20000000298023224 * (double) v)))));
            this.Add((Component) (this.sine = new SineWave(0.5f).Randomize()));
            this.Add((Component) (this.shaker = new Shaker(false)));
            this.Add((Component) (this.bloom = new BloomPoint(1f, 12f)));
            this.Add((Component) (this.light = new VertexLight(Color.White, 1f, 16, 24)));
            this.Add((Component) (this.lightTween = this.light.CreatePulseTween()));
        }

        public override void Awake(Scene scene)
        {
            this.level = scene as Level;
            base.Awake(scene);
            this.sprite = GFX.SpriteBank.Create(this.ghost ? "ghostberrySeed" : (this.level.Session.Area.Mode == AreaMode.CSide ? "goldberrySeed" : "strawberrySeed"));
            this.sprite.Position = new Vector2(this.sine.Value * 2f, this.sine.ValueOverTwo * 1f);
            this.Add((Component) this.sprite);
            if (this.ghost)
                this.sprite.Color = Color.White * 0.8f;
            this.sprite.PlayOffset("idle", (float) (0.25 + (1.0 - (double) this.index / ((double) this.Scene.Tracker.CountEntities<StrawberrySeed>() + 1.0)) * 0.75));
            this.sprite.OnFrameChange = (Action<string>) (s =>
            {
                if (!this.Visible || !(this.sprite.CurrentAnimationID == "idle") || this.sprite.CurrentAnimationFrame != 19)
                    return;
                Audio.Play("event:/game/general/seed_pulse", this.Position, "count", (float) this.index);
                this.lightTween.Start();
                this.level.Displacement.AddBurst(this.Position, 0.6f, 8f, 20f, 0.2f);
            });
            StrawberrySeed.P_Burst.Color = this.sprite.Color;
        }

        public override void Update()
        {
            base.Update();
            if (!this.finished)
            {
                if ((double) this.canLoseTimer > 0.0)
                    this.canLoseTimer -= Engine.DeltaTime;
                else if (this.follower.HasLeader && this.player.LoseShards)
                    this.losing = true;
                if (this.losing)
                {
                    if ((double) this.loseTimer <= 0.0 || (double) this.player.Speed.Y < 0.0)
                    {
                        this.player.Leader.LoseFollower(this.follower);
                        this.losing = false;
                    }
                    else if (this.player.LoseShards)
                    {
                        this.loseTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        this.loseTimer = 0.15f;
                        this.losing = false;
                    }
                }
                this.sprite.Position = new Vector2(this.sine.Value * 2f, this.sine.ValueOverTwo * 1f) + this.shaker.Value;
            }
            else
                this.light.Alpha = Calc.Approach(this.light.Alpha, 0.0f, Engine.DeltaTime * 4f);
        }

        private void OnPlayer(Player player)
        {
            Audio.Play("event:/game/general/seed_touch", this.Position, "count", (float) this.index);
            this.player = player;
            player.Leader.GainFollower(this.follower);
            this.Collidable = false;
            this.Depth = -1000000;
            bool flag = true;
            foreach (StrawberrySeed seed in this.Strawberry.Seeds)
            {
                if (!seed.follower.HasLeader)
                    flag = false;
            }
            if (!flag)
                return;
            this.Scene.Add((Entity) new CSGEN_StrawberrySeeds(this.Strawberry));
        }

        private void OnGainLeader()
        {
            this.wiggler.Start();
            this.canLoseTimer = 0.25f;
            this.loseTimer = 0.15f;
        }

        private void OnLoseLeader()
        {
            if (this.finished)
                return;
            this.Add((Component) new Coroutine(this.ReturnRoutine()));
        }

        private IEnumerator ReturnRoutine()
        {
            StrawberrySeed strawberrySeed = this;
            Audio.Play("event:/game/general/seed_poof", strawberrySeed.Position);
            strawberrySeed.Collidable = false;
            strawberrySeed.sprite.Scale = Vector2.One * 2f;
            yield return (object) 0.05f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int index = 0; index < 6; ++index)
            {
                float num = Calc.Random.NextFloat(6.28318548f);
                strawberrySeed.level.ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, strawberrySeed.Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
            }
            strawberrySeed.Visible = false;
            yield return (object) (float) (0.30000001192092896 + (double) strawberrySeed.index * 0.10000000149011612);
            Audio.Play("event:/game/general/seed_reappear", strawberrySeed.Position, "count", (float) strawberrySeed.index);
            strawberrySeed.Position = strawberrySeed.start;
            if (strawberrySeed.attached != null)
                strawberrySeed.Position = strawberrySeed.Position + strawberrySeed.attached.Position;
            strawberrySeed.shaker.ShakeFor(0.4f, false);
            strawberrySeed.sprite.Scale = Vector2.One;
            strawberrySeed.Visible = true;
            strawberrySeed.Collidable = true;
            strawberrySeed.level.Displacement.AddBurst(strawberrySeed.Position, 0.2f, 8f, 28f, 0.2f);
        }

        public void OnAllCollected()
        {
            this.finished = true;
            this.follower.Leader.LoseFollower(this.follower);
            this.Depth = -2000002;
            this.Tag = (int) Tags.FrozenUpdate;
            this.wiggler.Start();
        }

        public void StartSpinAnimation(
            Vector2 averagePos,
            Vector2 centerPos,
            float angleOffset,
            float time)
        {
            float spinLerp = 0.0f;
            Vector2 start = this.Position;
            this.sprite.Play("noFlash");
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, time / 2f, true);
            tween1.OnUpdate = (Action<Tween>) (t => spinLerp = t.Eased);
            this.Add((Component) tween1);
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, time, true);
            tween2.OnUpdate = (Action<Tween>) (t =>
            {
                float angleRadians = 1.57079637f + angleOffset - MathHelper.Lerp(0.0f, 32.2013245f, t.Eased);
                this.Position = Vector2.Lerp(start, Vector2.Lerp(averagePos, centerPos, spinLerp) + Calc.AngleToVector(angleRadians, 25f), spinLerp);
            });
            this.Add((Component) tween2);
        }

        public void StartCombineAnimation(Vector2 centerPos, float time, ParticleSystem particleSystem)
        {
            Vector2 position = this.Position;
            float startAngle = Calc.Angle(centerPos, position);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BigBackIn, time, true);
            tween.OnUpdate = (Action<Tween>) (t => this.Position = centerPos + Calc.AngleToVector(MathHelper.Lerp(startAngle, startAngle - 6.28318548f, Ease.CubeIn(t.Percent)), MathHelper.Lerp(25f, 0.0f, t.Eased)));
            tween.OnComplete = (Action<Tween>) (t =>
            {
                this.Visible = false;
                for (int index = 0; index < 6; ++index)
                {
                    float num = Calc.Random.NextFloat(6.28318548f);
                    particleSystem.Emit(StrawberrySeed.P_Burst, 1, this.Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
                }
                this.RemoveSelf();
            });
            this.Add((Component) tween);
        }
    }
}
