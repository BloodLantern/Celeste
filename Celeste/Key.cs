using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Key : Entity
    {
        public static ParticleType P_Shimmer;
        public static ParticleType P_Insert;
        public static ParticleType P_Collect;
        public EntityID ID;
        public bool IsUsed;
        public bool StartedUsing;
        private Follower follower;
        private Sprite sprite;
        private Wiggler wiggler;
        private VertexLight light;
        private ParticleEmitter shimmerParticles;
        private float wobble;
        private bool wobbleActive;
        private Tween tween;
        private Alarm alarm;
        private Vector2[] nodes;

        public bool Turning { get; private set; }

        public Key(Vector2 position, EntityID id, Vector2[] nodes)
            : base(position)
        {
            ID = id;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            this.nodes = nodes;
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(sprite = GFX.SpriteBank.Create("key"));
            sprite.CenterOrigin();
            sprite.Play("idle");
            Add(new TransitionListener
            {
                OnOut = f =>
                {
                    StartedUsing = false;
                    if (IsUsed)
                        return;
                    if (tween != null)
                    {
                        tween.RemoveSelf();
                        tween = null;
                    }
                    if (alarm != null)
                    {
                        alarm.RemoveSelf();
                        alarm = null;
                    }
                    Turning = false;
                    Visible = true;
                    sprite.Visible = true;
                    sprite.Rate = 1f;
                    sprite.Scale = Vector2.One;
                    sprite.Play("idle");
                    sprite.Rotation = 0.0f;
                    wiggler.Stop();
                    follower.MoveTowardsLeader = true;
                }
            });
            Add(wiggler = Wiggler.Create(0.4f, 4f, v => sprite.Scale = Vector2.One * (float) (1.0 + v * 0.34999999403953552)));
            Add(light = new VertexLight(Color.White, 1f, 32, 48));
        }

        public Key(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.NodesOffset(offset))
        {
        }

        public Key(Player player, EntityID id)
            : this(player.Position + new Vector2(-12 * (int) player.Facing, -8f), id, null)
        {
            player.Leader.GainFollower(follower);
            Collidable = false;
            Depth = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(shimmerParticles = new ParticleEmitter((scene as Level).ParticlesFG, Key.P_Shimmer, Vector2.Zero, new Vector2(6f, 6f), 1, 0.1f));
            shimmerParticles.SimulateCycle();
        }

        public override void Update()
        {
            if (wobbleActive)
            {
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = (float) Math.Sin(wobble);
            }
            base.Update();
        }

        private void OnPlayer(Player player)
        {
            SceneAs<Level>().Particles.Emit(Key.P_Collect, 10, Position, Vector2.One * 3f);
            Audio.Play("event:/game/general/key_get", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            player.Leader.GainFollower(follower);
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            session.DoNotLoad.Add(ID);
            session.Keys.Add(ID);
            session.UpdateLevelStartDashes();
            wiggler.Start();
            Depth = -1000000;
            if (nodes == null || nodes.Length < 2)
                return;
            Add(new Coroutine(NodeRoutine(player)));
        }

        private IEnumerator NodeRoutine(Player player)
        {
            Key key = this;
            yield return 0.3f;
            if (!player.Dead)
            {
                Audio.Play("event:/game/general/cassette_bubblereturn", key.SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(key.nodes[1], key.nodes[0]);
            }
        }

        public void RegisterUsed()
        {
            IsUsed = true;
            if (follower.Leader != null)
                follower.Leader.LoseFollower(follower);
            SceneAs<Level>().Session.Keys.Remove(ID);
        }

        public IEnumerator UseRoutine(Vector2 target)
        {
            Key key = this;
            key.Turning = true;
            key.follower.MoveTowardsLeader = false;
            key.wiggler.Start();
            key.wobbleActive = false;
            key.sprite.Y = 0.0f;
            Vector2 position = key.Position;
            SimpleCurve curve = new SimpleCurve(position, target, (target + position) / 2f + new Vector2(0.0f, -48f));
            key.tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, start: true);
            key.tween.OnUpdate = t =>
            {
                Position = curve.GetPoint(t.Eased);
                sprite.Rate = (float) (1.0 + t.Eased * 2.0);
            };
            key.Add(key.tween);
            yield return key.tween.Wait();
            key.tween = null;
            while (key.sprite.CurrentAnimationFrame != 4)
                yield return null;
            key.shimmerParticles.Active = false;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int index = 0; index < 16; ++index)
                key.SceneAs<Level>().ParticlesFG.Emit(Key.P_Insert, key.Center, 0.3926991f * index);
            key.sprite.Play("enter");
            yield return 0.3f;
            key.tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.3f, true);
            key.tween.OnUpdate = t => sprite.Rotation = t.Eased * 1.57079637f;
            key.Add(key.tween);
            yield return key.tween.Wait();
            key.tween = null;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            key.alarm = Alarm.Set(key, 1f, () =>
            {
                alarm = null;
                tween = Tween.Create(Tween.TweenMode.Oneshot, start: true);
                tween.OnUpdate = t => light.Alpha = 1f - t.Eased;
                tween.OnComplete = t => RemoveSelf();
                Add(tween);
            });
            yield return 0.2f;
            for (int index = 0; index < 8; ++index)
                key.SceneAs<Level>().ParticlesFG.Emit(Key.P_Insert, key.Center, 0.7853982f * index);
            key.sprite.Visible = false;
            key.Turning = false;
        }
    }
}
