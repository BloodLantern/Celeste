// Decompiled with JetBrains decompiler
// Type: Celeste.Spikes
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class Spikes : Entity
    {
        public const string TentacleType = "tentacles";
        public Spikes.Directions Direction;
        private PlayerCollider pc;
        private Vector2 imageOffset;
        private int size;
        private string overrideType;
        private string spikeType;
        public Color EnabledColor = Color.White;
        public Color DisabledColor = Color.White;
        public bool VisibleWhenDisabled;

        public Spikes(Vector2 position, int size, Spikes.Directions direction, string type)
            : base(position)
        {
            this.Depth = -1;
            this.Direction = direction;
            this.size = size;
            this.overrideType = type;
            switch (direction)
            {
                case Spikes.Directions.Up:
                    this.Collider = (Collider) new Hitbox((float) size, 3f, y: -3f);
                    this.Add((Component) new LedgeBlocker());
                    break;
                case Spikes.Directions.Down:
                    this.Collider = (Collider) new Hitbox((float) size, 3f);
                    break;
                case Spikes.Directions.Left:
                    this.Collider = (Collider) new Hitbox(3f, (float) size, -3f);
                    this.Add((Component) new LedgeBlocker());
                    break;
                case Spikes.Directions.Right:
                    this.Collider = (Collider) new Hitbox(3f, (float) size);
                    this.Add((Component) new LedgeBlocker());
                    break;
            }
            this.Add((Component) (this.pc = new PlayerCollider(new Action<Player>(this.OnCollide))));
            this.Add((Component) new StaticMover()
            {
                OnShake = new Action<Vector2>(this.OnShake),
                SolidChecker = new Func<Solid, bool>(this.IsRiding),
                JumpThruChecker = new Func<JumpThru, bool>(this.IsRiding),
                OnEnable = new Action(this.OnEnable),
                OnDisable = new Action(this.OnDisable)
            });
        }

        public Spikes(EntityData data, Vector2 offset, Spikes.Directions dir)
            : this(data.Position + offset, Spikes.GetSize(data, dir), dir, data.Attr("type", "default"))
        {
        }

        public void SetSpikeColor(Color color)
        {
            foreach (Component component in this.Components)
            {
                if (component is Monocle.Image image)
                    image.Color = color;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.spikeType = AreaData.Get(scene).Spike;
            if (!string.IsNullOrEmpty(this.overrideType) && !this.overrideType.Equals("default"))
                this.spikeType = this.overrideType;
            string lower = this.Direction.ToString().ToLower();
            if (this.spikeType == "tentacles")
            {
                for (int i = 0; i < this.size / 16; ++i)
                    this.AddTentacle((float) i);
                if (this.size / 8 % 2 != 1)
                    return;
                this.AddTentacle((float) (this.size / 16) - 0.5f);
            }
            else
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + this.spikeType + "_" + lower);
                for (int index = 0; index < this.size / 8; ++index)
                {
                    Monocle.Image image = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures));
                    switch (this.Direction)
                    {
                        case Spikes.Directions.Up:
                            image.JustifyOrigin(0.5f, 1f);
                            image.Position = Vector2.UnitX * ((float) index + 0.5f) * 8f + Vector2.UnitY;
                            break;
                        case Spikes.Directions.Down:
                            image.JustifyOrigin(0.5f, 0.0f);
                            image.Position = Vector2.UnitX * ((float) index + 0.5f) * 8f - Vector2.UnitY;
                            break;
                        case Spikes.Directions.Left:
                            image.JustifyOrigin(1f, 0.5f);
                            image.Position = Vector2.UnitY * ((float) index + 0.5f) * 8f + Vector2.UnitX;
                            break;
                        case Spikes.Directions.Right:
                            image.JustifyOrigin(0.0f, 0.5f);
                            image.Position = Vector2.UnitY * ((float) index + 0.5f) * 8f - Vector2.UnitX;
                            break;
                    }
                    this.Add((Component) image);
                }
            }
        }

        private void AddTentacle(float i)
        {
            Sprite sprite = GFX.SpriteBank.Create("tentacles");
            sprite.Play(Calc.Random.Next(3).ToString(), true, true);
            sprite.Position = (this.Direction == Spikes.Directions.Up || this.Direction == Spikes.Directions.Down ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * 16f;
            sprite.Scale.X = (float) Calc.Random.Choose<int>(-1, 1);
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            if (this.Direction == Spikes.Directions.Up)
            {
                sprite.Rotation = -1.57079637f;
                ++sprite.Y;
            }
            else if (this.Direction == Spikes.Directions.Right)
            {
                sprite.Rotation = 0.0f;
                --sprite.X;
            }
            else if (this.Direction == Spikes.Directions.Left)
            {
                sprite.Rotation = 3.14159274f;
                ++sprite.X;
            }
            else if (this.Direction == Spikes.Directions.Down)
            {
                sprite.Rotation = 1.57079637f;
                --sprite.Y;
            }
            sprite.Rotation += 1.57079637f;
            this.Add((Component) sprite);
        }

        private void OnEnable()
        {
            this.Active = this.Visible = this.Collidable = true;
            this.SetSpikeColor(this.EnabledColor);
        }

        private void OnDisable()
        {
            this.Active = this.Collidable = false;
            if (this.VisibleWhenDisabled)
            {
                foreach (Component component in this.Components)
                {
                    if (component is Monocle.Image image)
                        image.Color = this.DisabledColor;
                }
            }
            else
                this.Visible = false;
        }

        private void OnShake(Vector2 amount) => this.imageOffset += amount;

        public override void Render()
        {
            Vector2 position = this.Position;
            this.Position = this.Position + this.imageOffset;
            base.Render();
            this.Position = position;
        }

        public void SetOrigins(Vector2 origin)
        {
            foreach (Component component in this.Components)
            {
                if (component is Monocle.Image image)
                {
                    Vector2 vector2 = origin - this.Position;
                    image.Origin = image.Origin + vector2 - image.Position;
                    image.Position = vector2;
                }
            }
        }

        private void OnCollide(Player player)
        {
            switch (this.Direction)
            {
                case Spikes.Directions.Up:
                    if ((double) player.Speed.Y < 0.0 || (double) player.Bottom > (double) this.Bottom)
                        break;
                    player.Die(new Vector2(0.0f, -1f));
                    break;
                case Spikes.Directions.Down:
                    if ((double) player.Speed.Y > 0.0)
                        break;
                    player.Die(new Vector2(0.0f, 1f));
                    break;
                case Spikes.Directions.Left:
                    if ((double) player.Speed.X < 0.0)
                        break;
                    player.Die(new Vector2(-1f, 0.0f));
                    break;
                case Spikes.Directions.Right:
                    if ((double) player.Speed.X > 0.0)
                        break;
                    player.Die(new Vector2(1f, 0.0f));
                    break;
            }
        }

        private static int GetSize(EntityData data, Spikes.Directions dir)
        {
            switch (dir)
            {
                case Spikes.Directions.Up:
                case Spikes.Directions.Down:
                    return data.Width;
                default:
                    int num = (int) (dir - 2);
                    return data.Height;
            }
        }

        private bool IsRiding(Solid solid)
        {
            switch (this.Direction)
            {
                case Spikes.Directions.Up:
                    return this.CollideCheckOutside((Entity) solid, this.Position + Vector2.UnitY);
                case Spikes.Directions.Down:
                    return this.CollideCheckOutside((Entity) solid, this.Position - Vector2.UnitY);
                case Spikes.Directions.Left:
                    return this.CollideCheckOutside((Entity) solid, this.Position + Vector2.UnitX);
                case Spikes.Directions.Right:
                    return this.CollideCheckOutside((Entity) solid, this.Position - Vector2.UnitX);
                default:
                    return false;
            }
        }

        private bool IsRiding(JumpThru jumpThru) => this.Direction == Spikes.Directions.Up && this.CollideCheck((Entity) jumpThru, this.Position + Vector2.UnitY);

        public enum Directions
        {
            Up,
            Down,
            Left,
            Right,
        }
    }
}
