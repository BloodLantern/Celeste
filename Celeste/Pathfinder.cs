using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class Pathfinder
    {
        private static readonly Point[] directions = new Point[4]
        {
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(0, -1)
        };
        private const int MapSize = 200;
        private Level level;
        private Tile[,] map;
        private List<Point> active = new List<Point>();
        private PointMapComparer comparer;
        public bool DebugRenderEnabled;
        private List<Vector2> lastPath;
        private Point debugLastStart;
        private Point debugLastEnd;

        public Pathfinder(Level level)
        {
            this.level = level;
            map = new Tile[200, 200];
            comparer = new PointMapComparer(map);
        }

        public bool Find(
            ref List<Vector2> path,
            Vector2 from,
            Vector2 to,
            bool fewerTurns = true,
            bool logging = false)
        {
            lastPath = null;
            int num1 = level.Bounds.Left / 8;
            int num2 = level.Bounds.Top / 8;
            int num3 = level.Bounds.Width / 8;
            int num4 = level.Bounds.Height / 8;
            Point levelSolidOffset = level.LevelSolidOffset;
            for (int index1 = 0; index1 < num3; ++index1)
            {
                for (int index2 = 0; index2 < num4; ++index2)
                {
                    map[index1, index2].Solid = level.SolidsData[index1 + levelSolidOffset.X, index2 + levelSolidOffset.Y] != '0';
                    map[index1, index2].Cost = int.MaxValue;
                    map[index1, index2].Parent = new Point?();
                }
            }
            foreach (Entity entity in level.Tracker.GetEntities<Solid>())
            {
                if (entity.Collidable && entity.Collider is Hitbox)
                {
                    int num5 = (int) Math.Floor(entity.Left / 8.0);
                    for (int index3 = (int) Math.Ceiling(entity.Right / 8.0); num5 < index3; ++num5)
                    {
                        int num6 = (int) Math.Floor(entity.Top / 8.0);
                        for (int index4 = (int) Math.Ceiling(entity.Bottom / 8.0); num6 < index4; ++num6)
                        {
                            int index5 = num5 - num1;
                            int index6 = num6 - num2;
                            if (index5 >= 0 && index6 >= 0 && index5 < num3 && index6 < num4)
                                map[index5, index6].Solid = true;
                        }
                    }
                }
            }
            Point point1 = debugLastStart = new Point((int) Math.Floor(from.X / 8.0) - num1, (int) Math.Floor(from.Y / 8.0) - num2);
            Point point2 = debugLastEnd = new Point((int) Math.Floor(to.X / 8.0) - num1, (int) Math.Floor(to.Y / 8.0) - num2);
            if (point1.X < 0 || point1.Y < 0 || point1.X >= num3 || point1.Y >= num4 || point2.X < 0 || point2.Y < 0 || point2.X >= num3 || point2.Y >= num4)
            {
                if (logging)
                    Calc.Log("PF: FAILED - Start or End outside the level bounds");
                return false;
            }
            if (map[point1.X, point1.Y].Solid)
            {
                if (logging)
                    Calc.Log("PF: FAILED - Start inside a solid");
                return false;
            }
            if (map[point2.X, point2.Y].Solid)
            {
                if (logging)
                    Calc.Log("PF: FAILED - End inside a solid");
                return false;
            }
            active.Clear();
            active.Add(point1);
            map[point1.X, point1.Y].Cost = 0;
            bool flag = false;
            while (active.Count > 0 && !flag)
            {
                Point point3 = active[active.Count - 1];
                active.RemoveAt(active.Count - 1);
                for (int index7 = 0; index7 < 4; ++index7)
                {
                    Point point4 = new Point(Pathfinder.directions[index7].X, Pathfinder.directions[index7].Y);
                    Point point5 = new Point(point3.X + point4.X, point3.Y + point4.Y);
                    int num7 = 1;
                    if (point5.X >= 0 && point5.Y >= 0 && point5.X < num3 && point5.Y < num4 && !map[point5.X, point5.Y].Solid)
                    {
                        for (int index8 = 0; index8 < 4; ++index8)
                        {
                            Point point6 = new Point(point5.X + Pathfinder.directions[index8].X, point5.Y + Pathfinder.directions[index8].Y);
                            if (point6.X >= 0 && point6.Y >= 0 && point6.X < num3 && point6.Y < num4 && map[point6.X, point6.Y].Solid)
                            {
                                num7 = 7;
                                break;
                            }
                        }
                        if (fewerTurns && map[point3.X, point3.Y].Parent.HasValue && point5.X != map[point3.X, point3.Y].Parent.Value.X && point5.Y != map[point3.X, point3.Y].Parent.Value.Y)
                            num7 += 4;
                        int cost = map[point3.X, point3.Y].Cost;
                        if (point4.Y != 0)
                            num7 += (int) (cost * 0.5);
                        int num8 = cost + num7;
                        if (map[point5.X, point5.Y].Cost > num8)
                        {
                            map[point5.X, point5.Y].Cost = num8;
                            map[point5.X, point5.Y].Parent = point3;
                            int index9 = active.BinarySearch(point5, comparer);
                            if (index9 < 0)
                                index9 = ~index9;
                            active.Insert(index9, point5);
                            if (point5 == point2)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                if (logging)
                    Calc.Log("PF: FAILED - ran out of active nodes, can't find ending");
                return false;
            }
            path.Clear();
            Point point7 = point2;
            int num9;
            for (num9 = 0; point7 != point1 && num9++ < 1000; point7 = map[point7.X, point7.Y].Parent.Value)
                path.Add(new Vector2(point7.X + 0.5f, point7.Y + 0.5f) * 8f + level.LevelOffset);
            if (num9 >= 1000)
            {
                Console.WriteLine("WARNING: Pathfinder 'succeeded' but then was unable to work out its path?");
                return false;
            }
            for (int index = 1; index < path.Count - 1 && path.Count > 2; ++index)
            {
                if (path[index].X == (double) path[index - 1].X && path[index].X == (double) path[index + 1].X || path[index].Y == (double) path[index - 1].Y && path[index].Y == (double) path[index + 1].Y)
                {
                    path.RemoveAt(index);
                    --index;
                }
            }
            path.Reverse();
            lastPath = path;
            if (logging)
                Calc.Log("PF: SUCCESS");
            return true;
        }

        public void Render()
        {
            Rectangle bounds;
            for (int index1 = 0; index1 < 200; ++index1)
            {
                for (int index2 = 0; index2 < 200; ++index2)
                {
                    if (map[index1, index2].Solid)
                    {
                        bounds = level.Bounds;
                        double x = bounds.Left + index1 * 8;
                        bounds = level.Bounds;
                        double y = bounds.Top + index2 * 8;
                        Color color = Color.Red * 0.25f;
                        Draw.Rect((float) x, (float) y, 8f, 8f, color);
                    }
                }
            }
            if (lastPath != null)
            {
                Vector2 start = lastPath[0];
                for (int index = 1; index < lastPath.Count; ++index)
                {
                    Vector2 end = lastPath[index];
                    Draw.Line(start, end, Color.Red);
                    Draw.Rect(start.X - 2f, start.Y - 2f, 4f, 4f, Color.Red);
                    start = end;
                }
                Draw.Rect(start.X - 2f, start.Y - 2f, 4f, 4f, Color.Red);
            }
            bounds = level.Bounds;
            double x1 = bounds.Left + debugLastStart.X * 8 + 2;
            bounds = level.Bounds;
            double y1 = bounds.Top + debugLastStart.Y * 8 + 2;
            Color green1 = Color.Green;
            Draw.Rect((float) x1, (float) y1, 4f, 4f, green1);
            bounds = level.Bounds;
            double x2 = bounds.Left + debugLastEnd.X * 8 + 2;
            bounds = level.Bounds;
            double y2 = bounds.Top + debugLastEnd.Y * 8 + 2;
            Color green2 = Color.Green;
            Draw.Rect((float) x2, (float) y2, 4f, 4f, green2);
        }

        private struct Tile
        {
            public bool Solid;
            public int Cost;
            public Point? Parent;
        }

        private class PointMapComparer : IComparer<Point>
        {
            private Tile[,] map;

            public PointMapComparer(Tile[,] map) => this.map = map;

            public int Compare(Point a, Point b) => map[b.X, b.Y].Cost - map[a.X, a.Y].Cost;
        }
    }
}
