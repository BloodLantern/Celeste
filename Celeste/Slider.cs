// Decompiled with JetBrains decompiler
// Type: Celeste.Slider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Slider : Entity
    {
        private const float MaxSpeed = 80f;
        private const float Accel = 400f;
        private Vector2 dir;
        private Vector2 surface;
        private bool foundSurfaceAfterCorner;
        private bool gotOutOfWall;
        private float speed;
        private bool moving;

        public Slider(Vector2 position, bool clockwise, Slider.Surfaces surface)
            : base(position)
        {
            Collider = new Monocle.Circle(10f);
            Add(new StaticMover());
            switch (surface)
            {
                case Slider.Surfaces.Ceiling:
                    dir = -Vector2.UnitX;
                    this.surface = -Vector2.UnitY;
                    break;
                case Slider.Surfaces.LeftWall:
                    dir = -Vector2.UnitY;
                    this.surface = -Vector2.UnitX;
                    break;
                case Slider.Surfaces.RightWall:
                    dir = Vector2.UnitY;
                    this.surface = Vector2.UnitX;
                    break;
                default:
                    dir = Vector2.UnitX;
                    this.surface = Vector2.UnitY;
                    break;
            }
            if (!clockwise)
            {
                dir *= -1f;
            }

            moving = true;
            foundSurfaceAfterCorner = gotOutOfWall = true;
            speed = 80f;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public Slider(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Bool("clockwise", true), e.Enum<Slider.Surfaces>(nameof(surface)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int num = 0;
            while (!Scene.CollideCheck<Solid>(Position))
            {
                Position += surface;
                if (num >= 100)
                {
                    throw new Exception("Couldn't find surface");
                }
            }
        }

        private void OnPlayer(Player Player)
        {
            _ = Player.Die((Player.Center - Center).SafeNormalize(-Vector2.UnitY));
            moving = false;
        }

        public override void Update()
        {
            base.Update();
            if (!moving)
            {
                return;
            }

            speed = Calc.Approach(speed, 80f, 400f * Engine.DeltaTime);
            Position += dir * speed * Engine.DeltaTime;
            if (!OnSurfaceCheck())
            {
                if (!foundSurfaceAfterCorner)
                {
                    return;
                }

                Position = Position.Round();
                int num = 0;
                while (!OnSurfaceCheck())
                {
                    Position -= this.dir;
                    ++num;
                    if (num >= 100)
                    {
                        throw new Exception("Couldn't get back onto corner!");
                    }
                }
                foundSurfaceAfterCorner = false;
                Vector2 dir = this.dir;
                this.dir = surface;
                surface = -dir;
            }
            else
            {
                foundSurfaceAfterCorner = true;
                if (InWallCheck())
                {
                    if (!gotOutOfWall)
                    {
                        return;
                    }

                    Position = Position.Round();
                    int num = 0;
                    while (InWallCheck())
                    {
                        Position -= dir;
                        ++num;
                        if (num >= 100)
                        {
                            throw new Exception("Couldn't get out of wall!");
                        }
                    }
                    Position += dir - this.surface;
                    gotOutOfWall = false;
                    Vector2 surface = this.surface;
                    this.surface = dir;
                    dir = -surface;
                }
                else
                {
                    gotOutOfWall = true;
                }
            }
        }

        private bool OnSurfaceCheck()
        {
            return Scene.CollideCheck<Solid>(Position.Round() + surface);
        }

        private bool InWallCheck()
        {
            return Scene.CollideCheck<Solid>(Position.Round() - surface);
        }

        public override void Render()
        {
            Draw.Circle(Position, 12f, Color.Red, 8);
        }

        public enum Surfaces
        {
            Floor,
            Ceiling,
            LeftWall,
            RightWall,
        }
    }
}
