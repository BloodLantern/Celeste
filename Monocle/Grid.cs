// Decompiled with JetBrains decompiler
// Type: Monocle.Grid
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public class Grid : Collider
    {
        public VirtualMap<bool> Data;

        public float CellWidth { get; private set; }

        public float CellHeight { get; private set; }

        public Grid(int cellsX, int cellsY, float cellWidth, float cellHeight)
        {
            Data = new VirtualMap<bool>(cellsX, cellsY);
            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }

        public Grid(float cellWidth, float cellHeight, string bitstring)
        {
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            int maxCols = 0;
            int currentCols = 0;
            int rows = 1;
            for (int index = 0; index < bitstring.Length; ++index)
            {
                if (bitstring[index] == '\n')
                {
                    ++rows;
                    maxCols = Math.Max(currentCols, maxCols);
                    currentCols = 0;
                }
                else
                {
                    ++currentCols;
                }
            }
            Data = new VirtualMap<bool>(maxCols, rows);
            LoadBitstring(bitstring);
        }

        public Grid(float cellWidth, float cellHeight, bool[,] data)
        {
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            Data = new VirtualMap<bool>(data);
        }

        public Grid(float cellWidth, float cellHeight, VirtualMap<bool> data)
        {
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            Data = data;
        }

        public void Extend(int left, int right, int up, int down)
        {
            Position -= new Vector2(left * CellWidth, up * CellHeight);
            int columns = Data.Columns + left + right;
            int rows = Data.Rows + up + down;
            if (columns <= 0 || rows <= 0)
            {
                Data = new VirtualMap<bool>(0, 0);
            }
            else
            {
                VirtualMap<bool> virtualMap = new(columns, rows);
                for (int x1 = 0; x1 < Data.Columns; ++x1)
                {
                    for (int y1 = 0; y1 < Data.Rows; ++y1)
                    {
                        int x2 = x1 + left;
                        int y2 = y1 + up;
                        if (x2 >= 0 && x2 < columns && y2 >= 0 && y2 < rows)
                        {
                            virtualMap[x2, y2] = Data[x1, y1];
                        }
                    }
                }
                for (int x = 0; x < left; ++x)
                {
                    for (int y = 0; y < rows; ++y)
                    {
                        virtualMap[x, y] = Data[0, Calc.Clamp(y - up, 0, Data.Rows - 1)];
                    }
                }
                for (int x = columns - right; x < columns; ++x)
                {
                    for (int y = 0; y < rows; ++y)
                    {
                        virtualMap[x, y] = Data[Data.Columns - 1, Calc.Clamp(y - up, 0, Data.Rows - 1)];
                    }
                }
                for (int y = 0; y < up; ++y)
                {
                    for (int x = 0; x < columns; ++x)
                    {
                        virtualMap[x, y] = Data[Calc.Clamp(x - left, 0, Data.Columns - 1), 0];
                    }
                }
                for (int y = rows - down; y < rows; ++y)
                {
                    for (int x = 0; x < columns; ++x)
                    {
                        virtualMap[x, y] = Data[Calc.Clamp(x - left, 0, Data.Columns - 1), Data.Rows - 1];
                    }
                }
                Data = virtualMap;
            }
        }

        public void LoadBitstring(string bitstring)
        {
            int x = 0;
            int y = 0;
            for (int index = 0; index < bitstring.Length; ++index)
            {
                if (bitstring[index] == '\n')
                {
                    for (; x < CellsX; ++x)
                    {
                        Data[x, y] = false;
                    }

                    x = 0;
                    ++y;

                    if (y >= CellsY)
                    {
                        break;
                    }
                }
                else if (x < CellsX)
                {
                    if (bitstring[index] == '0')
                    {
                        Data[x, y] = false;
                        ++x;
                    }
                    else
                    {
                        Data[x, y] = true;
                        ++x;
                    }
                }
            }
        }

        public string GetBitstring()
        {
            string bitstring = "";
            for (int y = 0; y < CellsY; ++y)
            {
                if (y != 0)
                {
                    bitstring += "\n";
                }

                for (int x = 0; x < CellsX; ++x)
                {
                    bitstring = !Data[x, y] ? bitstring + "0" : bitstring + "1";
                }
            }
            return bitstring;
        }

        public void Clear(bool to = false)
        {
            for (int x = 0; x < CellsX; ++x)
            {
                for (int y = 0; y < CellsY; ++y)
                {
                    Data[x, y] = to;
                }
            }
        }

        public void SetRect(int x, int y, int width, int height, bool to = true)
        {
            if (x < 0)
            {
                width += x;
                x = 0;
            }
            if (y < 0)
            {
                height += y;
                y = 0;
            }
            if (x + width > CellsX)
            {
                width = CellsX - x;
            }

            if (y + height > CellsY)
            {
                height = CellsY - y;
            }

            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                {
                    Data[x + index1, y + index2] = to;
                }
            }
        }

        public bool CheckRect(int x, int y, int width, int height)
        {
            if (x < 0)
            {
                width += x;
                x = 0;
            }
            if (y < 0)
            {
                height += y;
                y = 0;
            }
            if (x + width > CellsX)
            {
                width = CellsX - x;
            }

            if (y + height > CellsY)
            {
                height = CellsY - y;
            }

            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                {
                    if (Data[x + index1, y + index2])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckColumn(int x)
        {
            for (int y = 0; y < CellsY; ++y)
            {
                if (!Data[x, y])
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckRow(int y)
        {
            for (int x = 0; x < CellsX; ++x)
            {
                if (!Data[x, y])
                {
                    return false;
                }
            }
            return true;
        }

        public bool this[int x, int y]
        {
            get => x >= 0 && y >= 0 && x < CellsX && y < CellsY && Data[x, y];
            set => Data[x, y] = value;
        }

        public int CellsX => Data.Columns;

        public int CellsY => Data.Rows;

        public override float Width
        {
            get => CellWidth * CellsX;
            set => throw new NotImplementedException();
        }

        public override float Height
        {
            get => CellHeight * CellsY;
            set => throw new NotImplementedException();
        }

        public bool IsEmpty
        {
            get
            {
                for (int x = 0; x < CellsX; ++x)
                {
                    for (int y = 0; y < CellsY; ++y)
                    {
                        if (Data[x, y])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public override float Left
        {
            get => Position.X;
            set => Position.X = value;
        }

        public override float Top
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        public override float Right
        {
            get => Position.X + Width;
            set => Position.X = value - Width;
        }

        public override float Bottom
        {
            get => Position.Y + Height;
            set => Position.Y = value - Height;
        }

        public override Collider Clone()
        {
            return new Grid(CellWidth, CellHeight, Data.Clone());
        }

        public override void Render(Camera camera, Color color)
        {
            if (camera == null)
            {
                for (int x = 0; x < CellsX; ++x)
                {
                    for (int y = 0; y < CellsY; ++y)
                    {
                        if (Data[x, y])
                        {
                            Draw.HollowRect(AbsoluteLeft + (x * CellWidth), AbsoluteTop + (y * CellHeight), CellWidth, CellHeight, color);
                        }
                    }
                }
            }
            else
            {
                int num1 = (int)Math.Max(0, (camera.Left - AbsoluteLeft) / CellWidth);
                int num2 = (int)Math.Min(CellsX - 1, Math.Ceiling((camera.Right - AbsoluteLeft) / CellWidth));
                int num3 = (int)Math.Max(0, (camera.Top - AbsoluteTop) / CellHeight);
                int num4 = (int)Math.Min(CellsY - 1, Math.Ceiling((camera.Bottom - AbsoluteTop) / CellHeight));
                for (int x = num1; x <= num2; ++x)
                {
                    for (int y = num3; y <= num4; ++y)
                    {
                        if (Data[x, y])
                        {
                            Draw.HollowRect(AbsoluteLeft + (x * CellWidth), AbsoluteTop + (y * CellHeight), CellWidth, CellHeight, color);
                        }
                    }
                }
            }
        }

        public override bool Collide(Vector2 point)
        {
            return point.X >= (double)AbsoluteLeft && point.Y >= (double)AbsoluteTop && point.X < (double)AbsoluteRight && point.Y < (double)AbsoluteBottom && Data[(int)((point.X - (double)AbsoluteLeft) / (double)CellWidth), (int)((point.Y - (double)AbsoluteTop) / (double)CellHeight)];
        }

        public override bool Collide(Rectangle rect)
        {
            if (!rect.Intersects(Bounds))
            {
                return false;
            }

            int x = (int)((rect.Left - AbsoluteLeft) / CellWidth);
            int y = (int)((rect.Top - AbsoluteTop) / CellHeight);
            int width = (int)((rect.Right - AbsoluteLeft - 1) / CellWidth) - x + 1;
            int height = (int)((rect.Bottom - AbsoluteTop - 1) / CellHeight) - y + 1;
            return CheckRect(x, y, width, height);
        }

        public override bool Collide(Vector2 from, Vector2 to)
        {
            from -= AbsolutePosition;
            to -= AbsolutePosition;
            from /= new Vector2(CellWidth, CellHeight);
            to /= new Vector2(CellWidth, CellHeight);
            bool flag = Math.Abs(to.Y - from.Y) > Math.Abs(to.X - from.X);
            if (flag)
            {
                (from.Y, from.X) = (from.X, from.Y);
                (to.Y, to.X) = (to.X, to.Y);
            }
            if (from.X > (double)to.X)
            {
                (to, from) = (from, to);
            }
            float num1 = 0.0f;
            float num2 = Math.Abs(to.Y - from.Y) / (to.X - from.X);
            int num3 = from.Y < (double)to.Y ? 1 : -1;
            int y = (int)from.Y;
            int x3 = (int)to.X;
            for (int x4 = (int)from.X; x4 <= x3; ++x4)
            {
                if (flag)
                {
                    if (this[y, x4])
                    {
                        return true;
                    }
                }
                else if (this[x4, y])
                {
                    return true;
                }

                num1 += num2;
                if ((double)num1 >= 0.5)
                {
                    y += num3;
                    --num1;
                }
            }
            return false;
        }

        public override bool Collide(Hitbox hitbox)
        {
            return Collide(hitbox.Bounds);
        }

        public override bool Collide(Grid grid)
        {
            throw new NotImplementedException();
        }

        public override bool Collide(Circle circle)
        {
            return false;
        }

        public override bool Collide(ColliderList list)
        {
            return list.Collide(this);
        }

        public static bool IsBitstringEmpty(string bitstring)
        {
            for (int index = 0; index < bitstring.Length; ++index)
            {
                if (bitstring[index] == '1')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
