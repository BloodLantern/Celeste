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
        public Orientations Orientation;
        private readonly bool playerCanUse;
        public Color DisabledColor = Color.White;
        public bool VisibleWhenDisabled;

        public Spring(Vector2 position, Orientations orientation, bool playerCanUse)
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
                case Orientations.Floor:
                    staticMover.SolidChecker = s => CollideCheck(s, Position + Vector2.UnitY);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position + Vector2.UnitY);
                    Add(staticMover);
                    break;
                case Orientations.WallLeft:
                    staticMover.SolidChecker = s => CollideCheck(s, Position - Vector2.UnitX);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position - Vector2.UnitX);
                    Add(staticMover);
                    break;
                case Orientations.WallRight:
                    staticMover.SolidChecker = s => CollideCheck(s, Position + Vector2.UnitX);
                    staticMover.JumpThruChecker = jt => CollideCheck(jt, Position + Vector2.UnitX);
                    Add(staticMover);
                    break;
            }
            Add(wiggler = Wiggler.Create(1f, 4f, v => sprite.Scale.Y = 1f + v * 0.2f));
            switch (orientation)
            {
                case Orientations.Floor:
                    Collider = new Hitbox(16f, 6f, -8f, -6f);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                    break;
                case Orientations.WallLeft:
                    Collider = new Hitbox(6f, 16f, y: -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, y: -8f);
                    sprite.Rotation = MathHelper.PiOver2;
                    break;
                case Orientations.WallRight:
                    Collider = new Hitbox(6f, 16f, -6f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                    sprite.Rotation = -MathHelper.PiOver2;
                    break;
                default:
                    throw new Exception("Orientation not supported!");
            }
            staticMover.OnEnable = new Action(OnEnable);
            staticMover.OnDisable = new Action(OnDisable);
        }

        public Spring(EntityData data, Vector2 offset, Orientations orientation)
            : this(data.Position + offset, orientation, data.Bool(nameof (playerCanUse), true))
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
                Visible = false;
        }

        private void OnCollide(Player player)
        {
            if (player.StateMachine.State == Player.StDreamDash || !playerCanUse)
                return;

            if (Orientation == Orientations.Floor)
            {
                if (player.Speed.Y < 0.0)
                    return;
                BounceAnimate();
                player.SuperBounce(Top);
            }
            else if (Orientation == Orientations.WallLeft)
            {
                if (!player.SideBounce(1, Right, CenterY))
                    return;
                BounceAnimate();
            }
            else
            {
                if (Orientation != Orientations.WallRight)
                    throw new Exception("Orientation not supported!");
                if (!player.SideBounce(-1, Left, CenterY))
                    return;
                BounceAnimate();
            }
        }

        private void BounceAnimate()
        {
            Audio.Play("event:/game/general/spring", BottomCenter);
            staticMover.TriggerPlatform();
            sprite.Play("bounce", true);
            wiggler.Start();
        }

        private void OnHoldable(Holdable h)
        {
            if (!h.HitSpring(this))
                return;
            BounceAnimate();
        }

        private void OnPuffer(Puffer p)
        {
            if (!p.HitSpring(this))
                return;
            BounceAnimate();
        }

        private void OnSeeker(Seeker seeker)
        {
            if (seeker.Speed.Y < -120f)
                return;
            BounceAnimate();
            seeker.HitSpring();
        }

        public override void Render()
        {
            if (Collidable)
                sprite.DrawOutline();
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
