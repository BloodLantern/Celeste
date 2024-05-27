using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class Spikes : Entity
    {
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right,
        }

        public const string TentacleType = "tentacles";
        public Directions Direction;
        private readonly PlayerCollider pc;
        private Vector2 imageOffset;
        private readonly int size;
        private readonly string overrideType;
        private string spikeType;
        public Color EnabledColor = Color.White;
        public Color DisabledColor = Color.White;
        public bool VisibleWhenDisabled;

        public Spikes(Vector2 position, int size, Directions direction, string type)
            : base(position)
        {
            Depth = -1;
            Direction = direction;
            this.size = size;
            overrideType = type;
            switch (direction)
            {
                case Directions.Up:
                    Collider = new Hitbox(size, 3f, y: -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Down:
                    Collider = new Hitbox(size, 3f);
                    break;
                case Directions.Left:
                    Collider = new Hitbox(3f, size, -3f);
                    Add(new LedgeBlocker());
                    break;
                case Directions.Right:
                    Collider = new Hitbox(3f, size);
                    Add(new LedgeBlocker());
                    break;
            }
            Add(pc = new PlayerCollider(OnCollide));
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
                OnEnable = OnEnable,
                OnDisable = OnDisable
            });
        }

        public Spikes(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"))
        {
        }

        public void SetSpikeColor(Color color)
        {
            foreach (Component component in Components)
            {
                if (component is Image image)
                    image.Color = color;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            spikeType = AreaData.Get(scene).Spike;
            if (!string.IsNullOrEmpty(overrideType) && !overrideType.Equals("default"))
                spikeType = overrideType;
            string directionStr = Direction.ToString().ToLower();
            if (spikeType == TentacleType)
            {
                for (int i = 0; i < size / 16; i++)
                    AddTentacle(i);
                if (size / 8 % 2 != 1)
                    return;
                AddTentacle(size / 16 - 0.5f);
            }
            else
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + directionStr);
                for (int i = 0; i < size / 8; i++)
                {
                    Image image = new(Calc.Random.Choose(atlasSubtextures));
                    switch (Direction)
                    {
                        case Directions.Up:
                            image.JustifyOrigin(0.5f, 1f);
                            image.Position = Vector2.UnitX * (i + 0.5f) * 8f + Vector2.UnitY;
                            break;
                        case Directions.Down:
                            image.JustifyOrigin(0.5f, 0f);
                            image.Position = Vector2.UnitX * (i + 0.5f) * 8f - Vector2.UnitY;
                            break;
                        case Directions.Left:
                            image.JustifyOrigin(1f, 0.5f);
                            image.Position = Vector2.UnitY * (i + 0.5f) * 8f + Vector2.UnitX;
                            break;
                        case Directions.Right:
                            image.JustifyOrigin(0f, 0.5f);
                            image.Position = Vector2.UnitY * (i + 0.5f) * 8f - Vector2.UnitX;
                            break;
                    }
                    Add(image);
                }
            }
        }

        private void AddTentacle(float i)
        {
            Sprite sprite = GFX.SpriteBank.Create(TentacleType);
            sprite.Play(Calc.Random.Next(3).ToString(), true, true);
            sprite.Position = (Direction is Directions.Up or Directions.Down ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * 16f;
            sprite.Scale.X = Calc.Random.Choose(-1, 1);
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            if (Direction == Directions.Up)
            {
                sprite.Rotation = -1.57079637f;
                ++sprite.Y;
            }
            else if (Direction == Directions.Right)
            {
                sprite.Rotation = 0.0f;
                --sprite.X;
            }
            else if (Direction == Directions.Left)
            {
                sprite.Rotation = 3.14159274f;
                ++sprite.X;
            }
            else if (Direction == Directions.Down)
            {
                sprite.Rotation = 1.57079637f;
                --sprite.Y;
            }
            sprite.Rotation += 1.57079637f;
            Add(sprite);
        }

        private void OnEnable()
        {
            Active = Visible = Collidable = true;
            SetSpikeColor(EnabledColor);
        }

        private void OnDisable()
        {
            Active = Collidable = false;
            if (VisibleWhenDisabled)
            {
                foreach (Component component in Components)
                {
                    if (component is Image image)
                        image.Color = DisabledColor;
                }
            }
            else
                Visible = false;
        }

        private void OnShake(Vector2 amount) => imageOffset += amount;

        public override void Render()
        {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        public void SetOrigins(Vector2 origin)
        {
            foreach (Component component in Components)
            {
                if (component is Image image)
                {
                    Vector2 vector2 = origin - Position;
                    image.Origin = image.Origin + vector2 - image.Position;
                    image.Position = vector2;
                }
            }
        }

        private void OnCollide(Player player)
        {
            switch (Direction)
            {
                case Directions.Up:
                    if (player.Speed.Y < 0.0 || player.Bottom > (double) Bottom)
                        break;
                    player.Die(new Vector2(0.0f, -1f));
                    break;
                case Directions.Down:
                    if (player.Speed.Y > 0.0)
                        break;
                    player.Die(new Vector2(0.0f, 1f));
                    break;
                case Directions.Left:
                    if (player.Speed.X < 0.0)
                        break;
                    player.Die(new Vector2(-1f, 0.0f));
                    break;
                case Directions.Right:
                    if (player.Speed.X > 0.0)
                        break;
                    player.Die(new Vector2(1f, 0.0f));
                    break;
            }
        }

        private static int GetSize(EntityData data, Directions dir)
        {
            switch (dir)
            {
                case Directions.Up:
                case Directions.Down:
                    return data.Width;
                default:
                    int num = (int) (dir - 2); // ???
                    return data.Height;
            }
        }

        private bool IsRiding(Solid solid)
        {
            return Direction switch
            {
                Directions.Up => CollideCheckOutside(solid, Position + Vector2.UnitY),
                Directions.Down => CollideCheckOutside(solid, Position - Vector2.UnitY),
                Directions.Left => CollideCheckOutside(solid, Position + Vector2.UnitX),
                Directions.Right => CollideCheckOutside(solid, Position - Vector2.UnitX),
                _ => false,
            };
        }

        private bool IsRiding(JumpThru jumpThru) => Direction == Directions.Up && CollideCheck(jumpThru, Position + Vector2.UnitY);
    }
}
