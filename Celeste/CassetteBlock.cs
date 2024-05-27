using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class CassetteBlock : Solid
    {
        public int Index;
        public float Tempo;
        public bool Activated;
        public Modes Mode;
        public EntityID ID;
        private int blockHeight = 2;
        private List<CassetteBlock> group;
        private bool groupLeader;
        private Vector2 groupOrigin;
        private Color color;
        private List<Image> pressed = new List<Image>();
        private List<Image> solid = new List<Image>();
        private List<Image> all = new List<Image>();
        private LightOcclude occluder;
        private Wiggler wiggler;
        private Vector2 wigglerScaler;
        private BoxSide side;

        public CassetteBlock(
            Vector2 position,
            EntityID id,
            float width,
            float height,
            int index,
            float tempo)
            : base(position, width, height, false)
        {
            SurfaceSoundIndex = 35;
            Index = index;
            Tempo = tempo;
            Collidable = false;
            ID = id;
            switch (Index)
            {
                case 1:
                    color = Calc.HexToColor("f049be");
                    break;
                case 2:
                    color = Calc.HexToColor("fcdc3a");
                    break;
                case 3:
                    color = Calc.HexToColor("38e04e");
                    break;
                default:
                    color = Calc.HexToColor("49aaf0");
                    break;
            }
            Add(occluder = new LightOcclude());
        }

        public CassetteBlock(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Color color1 = Calc.HexToColor("667da5");
            Color color2 = new Color((float) (color1.R / (double) byte.MaxValue * (color.R / (double) byte.MaxValue)), (float) (color1.G / (double) byte.MaxValue * (color.G / (double) byte.MaxValue)), (float) (color1.B / (double) byte.MaxValue * (color.B / (double) byte.MaxValue)), 1f);
            scene.Add(side = new BoxSide(this, color2));
            foreach (StaticMover staticMover in staticMovers)
            {
                if (staticMover.Entity is Spikes entity1)
                {
                    entity1.EnabledColor = color;
                    entity1.DisabledColor = color2;
                    entity1.VisibleWhenDisabled = true;
                    entity1.SetSpikeColor(color);
                }
                if (staticMover.Entity is Spring entity2)
                {
                    entity2.DisabledColor = color2;
                    entity2.VisibleWhenDisabled = true;
                }
            }
            if (group == null)
            {
                groupLeader = true;
                group = new List<CassetteBlock>();
                group.Add(this);
                FindInGroup(this);
                float num1 = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float y = float.MinValue;
                foreach (CassetteBlock cassetteBlock in group)
                {
                    if (cassetteBlock.Left < (double) num1)
                        num1 = cassetteBlock.Left;
                    if (cassetteBlock.Right > (double) num2)
                        num2 = cassetteBlock.Right;
                    if (cassetteBlock.Bottom > (double) y)
                        y = cassetteBlock.Bottom;
                    if (cassetteBlock.Top < (double) num3)
                        num3 = cassetteBlock.Top;
                }
                groupOrigin = new Vector2((int) (num1 + (num2 - (double) num1) / 2.0), (int) y);
                wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num1, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(y - num3, 32f, 96f, 1f, 0.2f));
                Add(wiggler = Wiggler.Create(0.3f, 3f));
                foreach (CassetteBlock cassetteBlock in group)
                {
                    cassetteBlock.wiggler = wiggler;
                    cassetteBlock.wigglerScaler = wigglerScaler;
                    cassetteBlock.groupOrigin = groupOrigin;
                }
            }
            foreach (Component staticMover in staticMovers)
            {
                if (staticMover.Entity is Spikes entity)
                    entity.SetOrigins(groupOrigin);
            }
            for (float left = Left; left < (double) Right; left += 8f)
            {
                for (float top = Top; top < (double) Bottom; top += 8f)
                {
                    bool flag1 = CheckForSame(left - 8f, top);
                    bool flag2 = CheckForSame(left + 8f, top);
                    bool flag3 = CheckForSame(left, top - 8f);
                    bool flag4 = CheckForSame(left, top + 8f);
                    if (flag1 & flag2 & flag3 & flag4)
                    {
                        if (!CheckForSame(left + 8f, top - 8f))
                            SetImage(left, top, 3, 0);
                        else if (!CheckForSame(left - 8f, top - 8f))
                            SetImage(left, top, 3, 1);
                        else if (!CheckForSame(left + 8f, top + 8f))
                            SetImage(left, top, 3, 2);
                        else if (!CheckForSame(left - 8f, top + 8f))
                            SetImage(left, top, 3, 3);
                        else
                            SetImage(left, top, 1, 1);
                    }
                    else if (((!(flag1 & flag2) ? 0 : (!flag3 ? 1 : 0)) & (flag4 ? 1 : 0)) != 0)
                        SetImage(left, top, 1, 0);
                    else if (flag1 & flag2 & flag3 && !flag4)
                        SetImage(left, top, 1, 2);
                    else if (((!flag1 ? 0 : (!flag2 ? 1 : 0)) & (flag3 ? 1 : 0) & (flag4 ? 1 : 0)) != 0)
                        SetImage(left, top, 2, 1);
                    else if (!flag1 & flag2 & flag3 & flag4)
                        SetImage(left, top, 0, 1);
                    else if (((!flag1 || flag2 ? 0 : (!flag3 ? 1 : 0)) & (flag4 ? 1 : 0)) != 0)
                        SetImage(left, top, 2, 0);
                    else if (((!(!flag1 & flag2) ? 0 : (!flag3 ? 1 : 0)) & (flag4 ? 1 : 0)) != 0)
                        SetImage(left, top, 0, 0);
                    else if (((!flag1 ? 0 : (!flag2 ? 1 : 0)) & (flag3 ? 1 : 0)) != 0 && !flag4)
                        SetImage(left, top, 2, 2);
                    else if (!flag1 & flag2 & flag3 && !flag4)
                        SetImage(left, top, 0, 2);
                }
            }
            UpdateVisualState();
        }

        private void FindInGroup(CassetteBlock block)
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
            {
                if (entity != this && entity != block && entity.Index == Index && (entity.CollideRect(new Rectangle((int) block.X - 1, (int) block.Y, (int) block.Width + 2, (int) block.Height)) ? 1 : (entity.CollideRect(new Rectangle((int) block.X, (int) block.Y - 1, (int) block.Width, (int) block.Height + 2)) ? 1 : 0)) != 0 && !group.Contains(entity))
                {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        private bool CheckForSame(float x, float y)
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
            {
                if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int) x, (int) y, 8, 8)))
                    return true;
            }
            return false;
        }

        private void SetImage(float x, float y, int tx, int ty)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/cassetteblock/pressed");
            pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
            solid.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/cassetteblock/solid"]));
        }

        private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
        {
            Vector2 vector2_1 = new Vector2(x - X, y - Y);
            Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
            Vector2 vector2_2 = groupOrigin - Position;
            image.Origin = vector2_2 - vector2_1;
            image.Position = vector2_2;
            image.Color = color;
            Add(image);
            all.Add(image);
            return image;
        }

        public override void Update()
        {
            base.Update();
            if (groupLeader && Activated && !Collidable)
            {
                bool flag = false;
                foreach (CassetteBlock cassetteBlock in group)
                {
                    if (cassetteBlock.BlockedCheck())
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (CassetteBlock cassetteBlock in group)
                    {
                        cassetteBlock.Collidable = true;
                        cassetteBlock.EnableStaticMovers();
                        cassetteBlock.ShiftSize(-1);
                    }
                    wiggler.Start();
                }
            }
            else if (!Activated && Collidable)
            {
                ShiftSize(1);
                Collidable = false;
                DisableStaticMovers();
            }
            UpdateVisualState();
        }

        public bool BlockedCheck()
        {
            TheoCrystal actor1 = CollideFirst<TheoCrystal>();
            if (actor1 != null && !TryActorWiggleUp(actor1))
                return true;
            Player actor2 = CollideFirst<Player>();
            return actor2 != null && !TryActorWiggleUp(actor2);
        }

        private void UpdateVisualState()
        {
            if (!Collidable)
            {
                Depth = 8990;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Top >= Bottom - 1.0)
                    Depth = 10;
                else
                    Depth = -10;
            }
            foreach (Component staticMover in staticMovers)
                staticMover.Entity.Depth = Depth + 1;
            side.Depth = Depth + 5;
            side.Visible = blockHeight > 0;
            occluder.Visible = Collidable;
            foreach (Component component in solid)
                component.Visible = Collidable;
            foreach (Component component in pressed)
                component.Visible = !Collidable;
            if (!groupLeader)
                return;
            Vector2 vector2 = new Vector2((float) (1.0 + wiggler.Value * 0.05000000074505806 * wigglerScaler.X), (float) (1.0 + wiggler.Value * 0.15000000596046448 * wigglerScaler.Y));
            foreach (CassetteBlock cassetteBlock in group)
            {
                foreach (GraphicsComponent graphicsComponent in cassetteBlock.all)
                    graphicsComponent.Scale = vector2;
                foreach (Component staticMover in cassetteBlock.staticMovers)
                {
                    if (staticMover.Entity is Spikes entity)
                    {
                        foreach (Component component in entity.Components)
                        {
                            if (component is Image image)
                                image.Scale = vector2;
                        }
                    }
                }
            }
        }

        public void SetActivatedSilently(bool activated)
        {
            Activated = Collidable = activated;
            UpdateVisualState();
            if (activated)
            {
                EnableStaticMovers();
            }
            else
            {
                ShiftSize(2);
                DisableStaticMovers();
            }
        }

        public void Finish() => Activated = false;

        public void WillToggle()
        {
            ShiftSize(Collidable ? 1 : -1);
            UpdateVisualState();
        }

        private void ShiftSize(int amount)
        {
            MoveV(amount);
            blockHeight -= amount;
        }

        private bool TryActorWiggleUp(Entity actor)
        {
            foreach (CassetteBlock cassetteBlock in group)
            {
                if (cassetteBlock != this && cassetteBlock.CollideCheck(actor, cassetteBlock.Position + Vector2.UnitY * 4f))
                    return false;
            }
            bool collidable = Collidable;
            Collidable = true;
            for (int index = 1; index <= 4; ++index)
            {
                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * index))
                {
                    actor.Position -= Vector2.UnitY * index;
                    Collidable = collidable;
                    return true;
                }
            }
            Collidable = collidable;
            return false;
        }

        public enum Modes
        {
            Solid,
            Leaving,
            Disabled,
            Returning,
        }

        private class BoxSide : Entity
        {
            private CassetteBlock block;
            private Color color;

            public BoxSide(CassetteBlock block, Color color)
            {
                this.block = block;
                this.color = color;
            }

            public override void Render() => Draw.Rect(block.X, (float) (block.Y + (double) block.Height - 8.0), block.Width, 8 + block.blockHeight, color);
        }
    }
}
