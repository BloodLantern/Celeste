using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(true)]
    public abstract class Trigger : Entity
    {
        public bool Triggered;

        public bool PlayerIsInside { get; private set; }

        public Trigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Visible = false;
        }

        public virtual void OnEnter(Player player) => PlayerIsInside = true;

        public virtual void OnStay(Player player)
        {
        }

        public virtual void OnLeave(Player player) => PlayerIsInside = false;

        protected float GetPositionLerp(Player player, PositionModes mode)
        {
            return mode switch
            {
                PositionModes.HorizontalCenter => Math.Min(Calc.ClampedMap(player.CenterX, Left, CenterX), Calc.ClampedMap(player.CenterX, Right, CenterX)),
                PositionModes.VerticalCenter => Math.Min(Calc.ClampedMap(player.CenterY, Top, CenterY), Calc.ClampedMap(player.CenterY, Bottom, CenterY)),
                PositionModes.TopToBottom => Calc.ClampedMap(player.CenterY, Top, Bottom),
                PositionModes.BottomToTop => Calc.ClampedMap(player.CenterY, Bottom, Top),
                PositionModes.LeftToRight => Calc.ClampedMap(player.CenterX, Left, Right),
                PositionModes.RightToLeft => Calc.ClampedMap(player.CenterX, Right, Left),
                _ => 1f,
            };
        }

        public enum PositionModes
        {
            NoEffect,
            HorizontalCenter,
            VerticalCenter,
            TopToBottom,
            BottomToTop,
            LeftToRight,
            RightToLeft,
        }
    }
}
