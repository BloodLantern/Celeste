using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class DustStaticSpinner : Entity
    {
        public static ParticleType P_Move;
        public const float ParticleInterval = 0.02f;
        public DustGraphic Sprite;
        private readonly float offset = Calc.Random.NextFloat();

        public DustStaticSpinner(Vector2 position, bool attachToSolid, bool ignoreSolids = false)
            : base(position)
        {
            Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(new LedgeBlocker());
            Add(Sprite = new DustGraphic(ignoreSolids, true, true));
            Depth = -50;
            if (!attachToSolid)
                return;
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding
            });
        }

        public DustStaticSpinner(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("attachToSolid"))
        {
        }

        public void ForceInstantiate() => Sprite.AddDustNodesIfInCamera();

        public override void Update()
        {
            base.Update();
            if (!Scene.OnInterval(0.05f, offset) || !Sprite.Estableshed)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            Collidable = Math.Abs(entity.X - X) < 128.0 && Math.Abs(entity.Y - Y) < 128.0;
        }

        private void OnShake(Vector2 pos) => Sprite.Position = pos;

        private bool IsRiding(Solid solid) => CollideCheck(solid);

        private void OnPlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
            Sprite.OnHitPlayer();
        }

        private void OnHoldable(Holdable h) => h.HitSpinner(this);
    }
}
