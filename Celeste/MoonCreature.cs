using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class MoonCreature : Entity
    {
        private TrailNode[] trail;
        private Vector2 start;
        private Vector2 target;
        private float targetTimer;
        private Vector2 speed;
        private Vector2 bump;
        private Player following;
        private Vector2 followingOffset;
        private float followingTime;
        private Color OrbColor;
        private Color CenterColor;
        private Sprite Sprite;
        private const float Acceleration = 90f;
        private const float FollowAcceleration = 120f;
        private const float MaxSpeed = 40f;
        private const float MaxFollowSpeed = 70f;
        private const float MaxFollowDistance = 200f;
        private readonly int spawn;
        private Rectangle originLevelBounds;

        public MoonCreature(Vector2 position)
        {
            Tag = (int) Tags.TransitionUpdate;
            Depth = -13010;
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            start = position;
            targetTimer = 0.0f;
            GetRandomTarget();
            Position = target;
            Add(new PlayerCollider(OnPlayer));
            OrbColor = Calc.HexToColor("b0e6ff");
            CenterColor = Calc.Random.Choose(Calc.HexToColor("c34fc7"), Calc.HexToColor("4f95c7"), Calc.HexToColor("53c74f"));
            Color color1 = Color.Lerp(CenterColor, Calc.HexToColor("bde4ee"), 0.5f);
            Color color2 = Color.Lerp(CenterColor, Calc.HexToColor("2f2941"), 0.5f);
            trail = new TrailNode[10];
            for (int index = 0; index < 10; ++index)
                trail[index] = new TrailNode
                {
                    Position = Position,
                    Color = Color.Lerp(color1, color2, index / 9f)
                };
            Add(Sprite = GFX.SpriteBank.Create("moonCreatureTiny"));
        }

        public MoonCreature(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            spawn = data.Int("number", 1) - 1;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int index = 0; index < spawn; ++index)
                scene.Add(new MoonCreature(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4))));
            originLevelBounds = (scene as Level).Bounds;
        }

        private void OnPlayer(Player player)
        {
            Vector2 vector2 = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
            if (vector2.LengthSquared() <= (double) bump.LengthSquared())
                return;
            bump = vector2;
            if ((player.Center - start).Length() >= 200.0)
                return;
            following = player;
            followingTime = Calc.Random.Range(6f, 12f);
            GetFollowOffset();
        }

        private void GetFollowOffset() => followingOffset = new Vector2(Calc.Random.Choose(-1, 1) * Calc.Random.Range(8, 16), Calc.Random.Range(-20f, 0.0f));

        private void GetRandomTarget()
        {
            Vector2 target = this.target;
            do
            {
                float length = Calc.Random.NextFloat(32f);
                this.target = start + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), length);
            }
            while ((target - this.target).Length() < 8.0);
        }

        public override void Update()
        {
            base.Update();
            if (following == null)
            {
                targetTimer -= Engine.DeltaTime;
                if (targetTimer <= 0.0)
                {
                    targetTimer = Calc.Random.Range(0.8f, 4f);
                    GetRandomTarget();
                }
            }
            else
            {
                followingTime -= Engine.DeltaTime;
                targetTimer -= Engine.DeltaTime;
                if (targetTimer <= 0.0)
                {
                    targetTimer = Calc.Random.Range(0.8f, 2f);
                    GetFollowOffset();
                }
                target = following.Center + followingOffset;
                if ((Position - start).Length() > 200.0 || followingTime <= 0.0)
                {
                    following = null;
                    targetTimer = 0.0f;
                }
            }
            speed += (this.target - Position).SafeNormalize() * (following == null ? 90f : 120f) * Engine.DeltaTime;
            speed = speed.SafeNormalize() * Math.Min(speed.Length(), following == null ? 40f : 70f);
            bump = bump.SafeNormalize() * Calc.Approach(bump.Length(), 0.0f, Engine.DeltaTime * 80f);
            Position += (speed + bump) * Engine.DeltaTime;
            Vector2 position = Position;
            for (int index = 0; index < trail.Length; ++index)
            {
                Vector2 vector2 = (trail[index].Position - position).SafeNormalize();
                if (vector2 == Vector2.Zero)
                    vector2 = new Vector2(0.0f, 1f);
                vector2.Y += 0.05f;
                Vector2 target = position + vector2 * 2f;
                trail[index].Position = Calc.Approach(trail[index].Position, target, 128f * Engine.DeltaTime);
                position = trail[index].Position;
            }
            X = Calc.Clamp(X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
            Y = Calc.Clamp(Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);
        }

        public override void Render()
        {
            Vector2 position1 = Position;
            Position = Position.Floor();
            for (int val = trail.Length - 1; val >= 0; --val)
            {
                Vector2 position2 = trail[val].Position;
                float num = Calc.ClampedMap(val, 0.0f, trail.Length - 1, 3f);
                Draw.Rect(position2.X - num / 2f, position2.Y - num / 2f, num, num, trail[val].Color);
            }
            base.Render();
            Position = position1;
        }

        private struct TrailNode
        {
            public Vector2 Position;
            public Color Color;
        }
    }
}
