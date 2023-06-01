// Decompiled with JetBrains decompiler
// Type: Celeste.DustStaticSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class DustStaticSpinner : Entity
    {
        public static ParticleType P_Move;
        public const float ParticleInterval = 0.02f;
        public DustGraphic Sprite;
        private float offset = Calc.Random.NextFloat();

        public DustStaticSpinner(Vector2 position, bool attachToSolid, bool ignoreSolids = false)
            : base(position)
        {
            this.Collider = (Collider) new ColliderList(new Collider[2]
            {
                (Collider) new Monocle.Circle(6f),
                (Collider) new Hitbox(16f, 4f, -8f, -3f)
            });
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) new HoldableCollider(new Action<Holdable>(this.OnHoldable)));
            this.Add((Component) new LedgeBlocker());
            this.Add((Component) (this.Sprite = new DustGraphic(ignoreSolids, true, true)));
            this.Depth = -50;
            if (!attachToSolid)
                return;
            this.Add((Component) new StaticMover()
            {
                OnShake = new Action<Vector2>(this.OnShake),
                SolidChecker = new Func<Solid, bool>(this.IsRiding)
            });
        }

        public DustStaticSpinner(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("attachToSolid"))
        {
        }

        public void ForceInstantiate() => this.Sprite.AddDustNodesIfInCamera();

        public override void Update()
        {
            base.Update();
            if (!this.Scene.OnInterval(0.05f, this.offset) || !this.Sprite.Estableshed)
                return;
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            this.Collidable = (double) Math.Abs(entity.X - this.X) < 128.0 && (double) Math.Abs(entity.Y - this.Y) < 128.0;
        }

        private void OnShake(Vector2 pos) => this.Sprite.Position = pos;

        private bool IsRiding(Solid solid) => this.CollideCheck((Entity) solid);

        private void OnPlayer(Player player)
        {
            player.Die((player.Position - this.Position).SafeNormalize());
            this.Sprite.OnHitPlayer();
        }

        private void OnHoldable(Holdable h) => h.HitSpinner((Entity) this);
    }
}
