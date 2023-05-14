﻿// Decompiled with JetBrains decompiler
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
        private readonly float offset = Calc.Random.NextFloat();

        public DustStaticSpinner(Vector2 position, bool attachToSolid, bool ignoreSolids = false)
            : base(position)
        {
            Collider = new ColliderList(new Collider[2]
            {
                 new Monocle.Circle(6f),
                 new Hitbox(16f, 4f, -8f, -3f)
            });
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(new HoldableCollider(new Action<Holdable>(OnHoldable)));
            Add(new LedgeBlocker());
            Add(Sprite = new DustGraphic(ignoreSolids, true, true));
            Depth = -50;
            if (!attachToSolid)
            {
                return;
            }

            Add(new StaticMover()
            {
                OnShake = new Action<Vector2>(OnShake),
                SolidChecker = new Func<Solid, bool>(IsRiding)
            });
        }

        public DustStaticSpinner(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("attachToSolid"))
        {
        }

        public void ForceInstantiate()
        {
            Sprite.AddDustNodesIfInCamera();
        }

        public override void Update()
        {
            base.Update();
            if (!Scene.OnInterval(0.05f, offset) || !Sprite.Estableshed)
            {
                return;
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            Collidable = (double)Math.Abs(entity.X - X) < 128.0 && (double)Math.Abs(entity.Y - Y) < 128.0;
        }

        private void OnShake(Vector2 pos)
        {
            Sprite.Position = pos;
        }

        private bool IsRiding(Solid solid)
        {
            return CollideCheck(solid);
        }

        private void OnPlayer(Player player)
        {
            _ = player.Die((player.Position - Position).SafeNormalize());
            Sprite.OnHitPlayer();
        }

        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }
    }
}
