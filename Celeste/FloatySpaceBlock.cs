// Decompiled with JetBrains decompiler
// Type: Celeste.FloatySpaceBlock
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
    public class FloatySpaceBlock : Solid
    {
        private TileGrid tiles;
        private readonly char tileType;
        private float yLerp;
        private float sinkTimer;
        private float sineWave;
        private float dashEase;
        private Vector2 dashDirection;
        private FloatySpaceBlock master;
        private bool awake;
        public List<FloatySpaceBlock> Group;
        public List<JumpThru> Jumpthrus;
        public Dictionary<Platform, Vector2> Moves;
        public Point GroupBoundsMin;
        public Point GroupBoundsMax;

        public bool HasGroup { get; private set; }

        public bool MasterOfGroup { get; private set; }

        public FloatySpaceBlock(
            Vector2 position,
            float width,
            float height,
            char tileType,
            bool disableSpawnOffset)
            : base(position, width, height, true)
        {
            this.tileType = tileType;
            Depth = -9000;
            Add(new LightOcclude());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            sineWave = !disableSpawnOffset ? Calc.Random.NextFloat(6.28318548f) : 0.0f;
        }

        public FloatySpaceBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("disableSpawnOffset"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            awake = true;
            if (!HasGroup)
            {
                MasterOfGroup = true;
                Moves = new Dictionary<Platform, Vector2>();
                Group = new List<FloatySpaceBlock>();
                Jumpthrus = new List<JumpThru>();
                GroupBoundsMin = new Point((int)X, (int)Y);
                GroupBoundsMax = new Point((int)Right, (int)Bottom);
                AddToGroupAndFindChildren(this);
                _ = Scene;
                Rectangle rectangle = new(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, ((GroupBoundsMax.X - GroupBoundsMin.X) / 8) + 1, ((GroupBoundsMax.Y - GroupBoundsMin.Y) / 8) + 1);
                VirtualMap<char> mapData = new(rectangle.Width, rectangle.Height, '0');
                foreach (FloatySpaceBlock floatySpaceBlock in Group)
                {
                    int num1 = (int)((double)floatySpaceBlock.X / 8.0) - rectangle.X;
                    int num2 = (int)((double)floatySpaceBlock.Y / 8.0) - rectangle.Y;
                    int num3 = (int)((double)floatySpaceBlock.Width / 8.0);
                    int num4 = (int)((double)floatySpaceBlock.Height / 8.0);
                    for (int x = num1; x < num1 + num3; ++x)
                    {
                        for (int y = num2; y < num2 + num4; ++y)
                        {
                            mapData[x, y] = tileType;
                        }
                    }
                }
                tiles = GFX.FGAutotiler.GenerateMap(mapData, new Autotiler.Behaviour()
                {
                    EdgesExtend = false,
                    EdgesIgnoreOutOfLevel = false,
                    PaddingIgnoreOutOfLevel = false
                }).TileGrid;
                tiles.Position = new Vector2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);
                Add(tiles);
            }
            TryToInitPosition();
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            if (sm.Entity is not Spring)
            {
                return;
            }

            switch ((sm.Entity as Spring).Orientation)
            {
                case Spring.Orientations.Floor:
                    sinkTimer = 0.5f;
                    break;
                case Spring.Orientations.WallLeft:
                    dashEase = 1f;
                    dashDirection = -Vector2.UnitX;
                    break;
                case Spring.Orientations.WallRight:
                    dashEase = 1f;
                    dashDirection = Vector2.UnitX;
                    break;
            }
        }

        private void TryToInitPosition()
        {
            if (MasterOfGroup)
            {
                foreach (FloatySpaceBlock floatySpaceBlock in Group)
                {
                    if (!floatySpaceBlock.awake)
                    {
                        return;
                    }
                }
                MoveToTarget();
            }
            else
            {
                master.TryToInitPosition();
            }
        }

        private void AddToGroupAndFindChildren(FloatySpaceBlock from)
        {
            if ((double)from.X < GroupBoundsMin.X)
            {
                GroupBoundsMin.X = (int)from.X;
            }

            if ((double)from.Y < GroupBoundsMin.Y)
            {
                GroupBoundsMin.Y = (int)from.Y;
            }

            if ((double)from.Right > GroupBoundsMax.X)
            {
                GroupBoundsMax.X = (int)from.Right;
            }

            if ((double)from.Bottom > GroupBoundsMax.Y)
            {
                GroupBoundsMax.Y = (int)from.Bottom;
            }

            from.HasGroup = true;
            from.OnDashCollide = new DashCollision(OnDash);
            Group.Add(from);
            Moves.Add(from, from.Position);
            if (from != this)
            {
                from.master = this;
            }

            foreach (JumpThru jp in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)))
            {
                if (!Jumpthrus.Contains(jp))
                {
                    AddJumpThru(jp);
                }
            }
            foreach (JumpThru jp in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)))
            {
                if (!Jumpthrus.Contains(jp))
                {
                    AddJumpThru(jp);
                }
            }
            foreach (FloatySpaceBlock entity in Scene.Tracker.GetEntities<FloatySpaceBlock>())
            {
                if (!entity.HasGroup && entity.tileType == tileType && (Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
                {
                    AddToGroupAndFindChildren(entity);
                }
            }
        }

        private void AddJumpThru(JumpThru jp)
        {
            jp.OnDashCollide = new DashCollision(OnDash);
            Jumpthrus.Add(jp);
            Moves.Add(jp, jp.Position);
            foreach (FloatySpaceBlock entity in Scene.Tracker.GetEntities<FloatySpaceBlock>())
            {
                if (!entity.HasGroup && entity.tileType == tileType && Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), entity))
                {
                    AddToGroupAndFindChildren(entity);
                }
            }
        }

        private DashCollisionResults OnDash(Player player, Vector2 direction)
        {
            if (MasterOfGroup && dashEase <= 0.20000000298023224)
            {
                dashEase = 1f;
                dashDirection = direction;
            }
            return DashCollisionResults.NormalOverride;
        }

        public override void Update()
        {
            base.Update();
            if (MasterOfGroup)
            {
                bool flag = false;
                foreach (Solid solid in Group)
                {
                    if (solid.HasPlayerRider())
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (JumpThru jumpthru in Jumpthrus)
                    {
                        if (jumpthru.HasPlayerRider())
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    sinkTimer = 0.3f;
                }
                else if (sinkTimer > 0.0)
                {
                    sinkTimer -= Engine.DeltaTime;
                }

                yLerp = sinkTimer <= 0.0 ? Calc.Approach(yLerp, 0.0f, 1f * Engine.DeltaTime) : Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
                sineWave += Engine.DeltaTime;
                dashEase = Calc.Approach(dashEase, 0.0f, Engine.DeltaTime * 1.5f);
                MoveToTarget();
            }
            LiftSpeed = Vector2.Zero;
        }

        private void MoveToTarget()
        {
            float num1 = (float)Math.Sin(sineWave) * 4f;
            Vector2 vector2_1 = Calc.YoYo(Ease.QuadIn(dashEase)) * dashDirection * 8f;
            for (int index = 0; index < 2; ++index)
            {
                foreach (KeyValuePair<Platform, Vector2> move in Moves)
                {
                    Platform key = move.Key;
                    bool flag = false;
                    JumpThru jumpThru = key as JumpThru;
                    Solid solid = key as Solid;
                    if ((jumpThru != null && jumpThru.HasRider()) || (solid != null && solid.HasRider()))
                    {
                        flag = true;
                    }

                    if ((flag || index != 0) && (!flag || index != 1))
                    {
                        Vector2 vector2_2 = move.Value;
                        float num2 = MathHelper.Lerp(vector2_2.Y, vector2_2.Y + 12f, Ease.SineInOut(yLerp)) + num1;
                        key.MoveToY(num2 + vector2_1.Y);
                        key.MoveToX(vector2_2.X + vector2_1.X);
                    }
                }
            }
        }

        public override void OnShake(Vector2 amount)
        {
            if (!MasterOfGroup)
            {
                return;
            }

            base.OnShake(amount);
            tiles.Position += amount;
            foreach (Entity jumpthru in Jumpthrus)
            {
                foreach (Component component in jumpthru.Components)
                {
                    if (component is Monocle.Image image)
                    {
                        image.Position += amount;
                    }
                }
            }
        }
    }
}
