﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle
{
    public static class Collide
    {
        public static bool Check(Entity a, Entity b) => a.Collider != null && b.Collider != null && a != b && b.Collidable && a.Collider.Collide(b);

        public static bool Check(Entity a, Entity b, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            int num = Collide.Check(a, b) ? 1 : 0;
            a.Position = position;
            return num != 0;
        }

        public static bool Check(Entity a, IEnumerable<Entity> b)
        {
            foreach (Entity b1 in b)
            {
                if (Collide.Check(a, b1))
                    return true;
            }
            return false;
        }

        public static bool Check(Entity a, IEnumerable<Entity> b, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            int num = Collide.Check(a, b) ? 1 : 0;
            a.Position = position;
            return num != 0;
        }

        public static Entity First(Entity a, IEnumerable<Entity> b)
        {
            foreach (Entity b1 in b)
            {
                if (Collide.Check(a, b1))
                    return b1;
            }
            return null;
        }

        public static Entity First(Entity a, IEnumerable<Entity> b, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            Entity entity = Collide.First(a, b);
            a.Position = position;
            return entity;
        }

        public static List<Entity> All(Entity a, IEnumerable<Entity> b, List<Entity> into)
        {
            foreach (Entity b1 in b)
            {
                if (Collide.Check(a, b1))
                    into.Add(b1);
            }
            return into;
        }

        public static List<Entity> All(
            Entity a,
            IEnumerable<Entity> b,
            List<Entity> into,
            Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            List<Entity> entityList = Collide.All(a, b, into);
            a.Position = position;
            return entityList;
        }

        public static List<Entity> All(Entity a, IEnumerable<Entity> b) => Collide.All(a, b, new List<Entity>());

        public static List<Entity> All(Entity a, IEnumerable<Entity> b, Vector2 at) => Collide.All(a, b, new List<Entity>(), at);

        public static bool CheckPoint(Entity a, Vector2 point) => a.Collider != null && a.Collider.Collide(point);

        public static bool CheckPoint(Entity a, Vector2 point, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            int num = Collide.CheckPoint(a, point) ? 1 : 0;
            a.Position = position;
            return num != 0;
        }

        public static bool CheckLine(Entity a, Vector2 from, Vector2 to) => a.Collider != null && a.Collider.Collide(from, to);

        public static bool CheckLine(Entity a, Vector2 from, Vector2 to, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            int num = Collide.CheckLine(a, from, to) ? 1 : 0;
            a.Position = position;
            return num != 0;
        }

        public static bool CheckRect(Entity a, Rectangle rect) => a.Collider != null && a.Collider.Collide(rect);

        public static bool CheckRect(Entity a, Rectangle rect, Vector2 at)
        {
            Vector2 position = a.Position;
            a.Position = at;
            int num = Collide.CheckRect(a, rect) ? 1 : 0;
            a.Position = position;
            return num != 0;
        }

        public static bool LineCheck(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 vector2_1 = a2 - a1;
            Vector2 vector2_2 = b2 - b1;
            float num1 = (float) (vector2_1.X * (double) vector2_2.Y - vector2_1.Y * (double) vector2_2.X);
            if (num1 == 0.0)
                return false;
            Vector2 vector2_3 = b1 - a1;
            float num2 = (float) (vector2_3.X * (double) vector2_2.Y - vector2_3.Y * (double) vector2_2.X) / num1;
            if (num2 < 0.0 || num2 > 1.0)
                return false;
            float num3 = (float) (vector2_3.X * (double) vector2_1.Y - vector2_3.Y * (double) vector2_1.X) / num1;
            return num3 >= 0.0 && num3 <= 1.0;
        }

        public static bool LineCheck(
            Vector2 a1,
            Vector2 a2,
            Vector2 b1,
            Vector2 b2,
            out Vector2 intersection)
        {
            intersection = Vector2.Zero;
            Vector2 vector2_1 = a2 - a1;
            Vector2 vector2_2 = b2 - b1;
            float num1 = (float) (vector2_1.X * (double) vector2_2.Y - vector2_1.Y * (double) vector2_2.X);
            if (num1 == 0.0)
                return false;
            Vector2 vector2_3 = b1 - a1;
            float num2 = (float) (vector2_3.X * (double) vector2_2.Y - vector2_3.Y * (double) vector2_2.X) / num1;
            if (num2 < 0.0 || num2 > 1.0)
                return false;
            float num3 = (float) (vector2_3.X * (double) vector2_1.Y - vector2_3.Y * (double) vector2_1.X) / num1;
            if (num3 < 0.0 || num3 > 1.0)
                return false;
            intersection = a1 + num2 * vector2_1;
            return true;
        }

        public static bool CircleToLine(
            Vector2 cPosiition,
            float cRadius,
            Vector2 lineFrom,
            Vector2 lineTo)
        {
            return Vector2.DistanceSquared(cPosiition, Calc.ClosestPointOnLine(lineFrom, lineTo, cPosiition)) < cRadius * (double) cRadius;
        }

        public static bool CircleToPoint(Vector2 cPosition, float cRadius, Vector2 point) => Vector2.DistanceSquared(cPosition, point) < cRadius * (double) cRadius;

        public static bool CircleToRect(
            Vector2 cPosition,
            float cRadius,
            float rX,
            float rY,
            float rW,
            float rH)
        {
            return Collide.RectToCircle(rX, rY, rW, rH, cPosition, cRadius);
        }

        public static bool CircleToRect(Vector2 cPosition, float cRadius, Rectangle rect) => Collide.RectToCircle(rect, cPosition, cRadius);

        public static bool RectToCircle(
            float rX,
            float rY,
            float rW,
            float rH,
            Vector2 cPosition,
            float cRadius)
        {
            if (Collide.RectToPoint(rX, rY, rW, rH, cPosition))
                return true;
            PointSectors sector = Collide.GetSector(rX, rY, rW, rH, cPosition);
            Vector2 lineFrom;
            Vector2 lineTo;
            if ((sector & PointSectors.Top) != PointSectors.Center)
            {
                lineFrom = new Vector2(rX, rY);
                lineTo = new Vector2(rX + rW, rY);
                if (Collide.CircleToLine(cPosition, cRadius, lineFrom, lineTo))
                    return true;
            }
            if ((sector & PointSectors.Bottom) != PointSectors.Center)
            {
                lineFrom = new Vector2(rX, rY + rH);
                lineTo = new Vector2(rX + rW, rY + rH);
                if (Collide.CircleToLine(cPosition, cRadius, lineFrom, lineTo))
                    return true;
            }
            if ((sector & PointSectors.Left) != PointSectors.Center)
            {
                lineFrom = new Vector2(rX, rY);
                lineTo = new Vector2(rX, rY + rH);
                if (Collide.CircleToLine(cPosition, cRadius, lineFrom, lineTo))
                    return true;
            }
            if ((sector & PointSectors.Right) != PointSectors.Center)
            {
                lineFrom = new Vector2(rX + rW, rY);
                lineTo = new Vector2(rX + rW, rY + rH);
                if (Collide.CircleToLine(cPosition, cRadius, lineFrom, lineTo))
                    return true;
            }
            return false;
        }

        public static bool RectToCircle(Rectangle rect, Vector2 cPosition, float cRadius) => Collide.RectToCircle(rect.X, rect.Y, rect.Width, rect.Height, cPosition, cRadius);

        public static bool RectToLine(
            float rX,
            float rY,
            float rW,
            float rH,
            Vector2 lineFrom,
            Vector2 lineTo)
        {
            PointSectors sector1 = Collide.GetSector(rX, rY, rW, rH, lineFrom);
            PointSectors sector2 = Collide.GetSector(rX, rY, rW, rH, lineTo);
            if (sector1 == PointSectors.Center || sector2 == PointSectors.Center)
                return true;
            if ((sector1 & sector2) != PointSectors.Center)
                return false;
            PointSectors pointSectors = sector1 | sector2;
            Vector2 vector2;
            if ((pointSectors & PointSectors.Top) != PointSectors.Center)
            {
                Vector2 a1 = new Vector2(rX, rY);
                vector2 = new Vector2(rX + rW, rY);
                Vector2 a2 = vector2;
                Vector2 b1 = lineFrom;
                Vector2 b2 = lineTo;
                if (Collide.LineCheck(a1, a2, b1, b2))
                    return true;
            }
            if ((pointSectors & PointSectors.Bottom) != PointSectors.Center)
            {
                Vector2 a1 = new Vector2(rX, rY + rH);
                vector2 = new Vector2(rX + rW, rY + rH);
                Vector2 a2 = vector2;
                Vector2 b1 = lineFrom;
                Vector2 b2 = lineTo;
                if (Collide.LineCheck(a1, a2, b1, b2))
                    return true;
            }
            if ((pointSectors & PointSectors.Left) != PointSectors.Center)
            {
                Vector2 a1 = new Vector2(rX, rY);
                vector2 = new Vector2(rX, rY + rH);
                Vector2 a2 = vector2;
                Vector2 b1 = lineFrom;
                Vector2 b2 = lineTo;
                if (Collide.LineCheck(a1, a2, b1, b2))
                    return true;
            }
            if ((pointSectors & PointSectors.Right) != PointSectors.Center)
            {
                Vector2 a1 = new Vector2(rX + rW, rY);
                vector2 = new Vector2(rX + rW, rY + rH);
                Vector2 a2 = vector2;
                Vector2 b1 = lineFrom;
                Vector2 b2 = lineTo;
                if (Collide.LineCheck(a1, a2, b1, b2))
                    return true;
            }
            return false;
        }

        public static bool RectToLine(Rectangle rect, Vector2 lineFrom, Vector2 lineTo) => Collide.RectToLine(rect.X, rect.Y, rect.Width, rect.Height, lineFrom, lineTo);

        public static bool RectToPoint(float rX, float rY, float rW, float rH, Vector2 point) => point.X >= (double) rX && point.Y >= (double) rY && point.X < rX + (double) rW && point.Y < rY + (double) rH;

        public static bool RectToPoint(Rectangle rect, Vector2 point) => Collide.RectToPoint(rect.X, rect.Y, rect.Width, rect.Height, point);

        public static PointSectors GetSector(Rectangle rect, Vector2 point)
        {
            PointSectors sector = PointSectors.Center;
            if (point.X < (double) rect.Left)
                sector |= PointSectors.Left;
            else if (point.X >= (double) rect.Right)
                sector |= PointSectors.Right;
            if (point.Y < (double) rect.Top)
                sector |= PointSectors.Top;
            else if (point.Y >= (double) rect.Bottom)
                sector |= PointSectors.Bottom;
            return sector;
        }

        public static PointSectors GetSector(
            float rX,
            float rY,
            float rW,
            float rH,
            Vector2 point)
        {
            PointSectors sector = PointSectors.Center;
            if (point.X < (double) rX)
                sector |= PointSectors.Left;
            else if (point.X >= rX + (double) rW)
                sector |= PointSectors.Right;
            if (point.Y < (double) rY)
                sector |= PointSectors.Top;
            else if (point.Y >= rY + (double) rH)
                sector |= PointSectors.Bottom;
            return sector;
        }
    }
}
