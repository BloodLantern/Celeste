// Decompiled with JetBrains decompiler
// Type: Celeste.Trigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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

        public virtual void OnEnter(Player player)
        {
            PlayerIsInside = true;
        }

        public virtual void OnStay(Player player)
        {
        }

        public virtual void OnLeave(Player player)
        {
            PlayerIsInside = false;
        }

        protected float GetPositionLerp(Player player, Trigger.PositionModes mode)
        {
            return mode switch
            {
                Trigger.PositionModes.HorizontalCenter => Math.Min(Calc.ClampedMap(player.CenterX, Left, CenterX), Calc.ClampedMap(player.CenterX, Right, CenterX)),
                Trigger.PositionModes.VerticalCenter => Math.Min(Calc.ClampedMap(player.CenterY, Top, CenterY), Calc.ClampedMap(player.CenterY, Bottom, CenterY)),
                Trigger.PositionModes.TopToBottom => Calc.ClampedMap(player.CenterY, Top, Bottom),
                Trigger.PositionModes.BottomToTop => Calc.ClampedMap(player.CenterY, Bottom, Top),
                Trigger.PositionModes.LeftToRight => Calc.ClampedMap(player.CenterX, Left, Right),
                Trigger.PositionModes.RightToLeft => Calc.ClampedMap(player.CenterX, Right, Left),
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
