// Decompiled with JetBrains decompiler
// Type: Celeste.Spring
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Spring : Entity
    {
        private readonly Sprite sprite;
        private readonly Wiggler wiggler;
        private readonly StaticMover staticMover;
        public Spring.Orientations Orientation;
        private readonly bool playerCanUse;
        public Color DisabledColor = Color.White;
        public bool VisibleWhenDisabled;

        public Spring(Vector2 position, Spring.Orientations orientation, bool playerCanUse)
            : base(position)
        {
            Orientation = orientation;
            this.playerCanUse = playerCanUse;
            Add(new PlayerCollider(new Action<Player>(OnCollide)));
            Add(new HoldableCollider(new Action<Holdable>(OnHoldable)));
            PufferCollider pufferCollider = new(new Action<Puffer>(OnPuffer));
            Add(pufferCollider);
            Add(sprite = new Sprite(GFX.Game, "objects/spring/"));
            sprite.Add("idle", "", 0.0f, new int[1]);
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;
            Depth = -8501;
            staticMover = new StaticMover
            {
                OnAttach = p => Depth = p.Depth + 1
            };
            switch (orientation)
            {
                case Spring.Orientations.Floor:
                    staticMover.SolidChecker = s => CollideCheck(s, Position + Vector2.UnitY);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position + Vector2.UnitY);
                    Add(staticMover);
                    break;
                case Spring.Orientations.WallLeft:
                    staticMover.SolidChecker = s => CollideCheck(s, Position - Vector2.UnitX);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position - Vector2.UnitX);
                    Add(staticMover);
                    break;
                case Spring.Orientations.WallRight:
                    staticMover.SolidChecker = s => CollideCheck(s, Position + Vector2.UnitX);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position + Vector2.UnitX);
                    Add(staticMover);
                    break;
            }
            Add(wiggler = Wiggler.Create(1f, 4f, v => sprite.Scale.Y = (float)(1.0 + ((double)v * 0.20000000298023224))));
            if (orientation == Spring.Orientations.Floor)
            {
                Collider = new Hitbox(16f, 6f, -8f, -6f);
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
            }
            else if (orientation == Spring.Orientations.WallLeft)
            {
                Collider = new Hitbox(6f, 16f, y: -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, y: -8f);
                sprite.Rotation = 1.57079637f;
            }
            else
            {
                if (orientation != Spring.Orientations.WallRight)
                {
                    throw new Exception("Orientation not supported!");
                }

                Collider = new Hitbox(6f, 16f, -6f, -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                sprite.Rotation = -1.57079637f;
            }
            staticMover.OnEnable = new Action(OnEnable);
            staticMover.OnDisable = new Action(OnDisable);
        }

        public Spring(EntityData data, Vector2 offset, Spring.Orientations orientation)
            : this(data.Position + offset, orientation, data.Bool(nameof(playerCanUse), true))
        {
        }

        private void OnEnable()
        {
            Visible = Collidable = true;
            sprite.Color = Color.White;
            sprite.Play("idle");
        }

        private void OnDisable()
        {
            Collidable = false;
            if (VisibleWhenDisabled)
            {
                sprite.Play("disabled");
                sprite.Color = DisabledColor;
            }
            else
            {
                Visible = false;
            }
        }

        private void OnCollide(Player player)
        {
            if (player.StateMachine.State == 9 || !playerCanUse)
            {
                return;
            }

            if (Orientation == Spring.Orientations.Floor)
            {
                if (player.Speed.Y < 0.0)
                {
                    return;
                }

                BounceAnimate();
                player.SuperBounce(Top);
            }
            else if (Orientation == Spring.Orientations.WallLeft)
            {
                if (!player.SideBounce(1, Right, CenterY))
                {
                    return;
                }

                BounceAnimate();
            }
            else
            {
                if (Orientation != Spring.Orientations.WallRight)
                {
                    throw new Exception("Orientation not supported!");
                }

                if (!player.SideBounce(-1, Left, CenterY))
                {
                    return;
                }

                BounceAnimate();
            }
        }

        private void BounceAnimate()
        {
            _ = Audio.Play("event:/game/general/spring", BottomCenter);
            staticMover.TriggerPlatform();
            sprite.Play("bounce", true);
            wiggler.Start();
        }

        private void OnHoldable(Holdable h)
        {
            if (!h.HitSpring(this))
            {
                return;
            }

            BounceAnimate();
        }

        private void OnPuffer(Puffer p)
        {
            if (!p.HitSpring(this))
            {
                return;
            }

            BounceAnimate();
        }

        private void OnSeeker(Seeker seeker)
        {
            if (seeker.Speed.Y < -120.0)
            {
                return;
            }

            BounceAnimate();
            seeker.HitSpring();
        }

        public override void Render()
        {
            if (Collidable)
            {
                sprite.DrawOutline();
            }

            base.Render();
        }

        public enum Orientations
        {
            Floor,
            WallLeft,
            WallRight,
        }
    }
}
