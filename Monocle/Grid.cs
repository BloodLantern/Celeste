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
      this.Data = new VirtualMap<bool>(cellsX, cellsY);
      this.CellWidth = cellWidth;
      this.CellHeight = cellHeight;
    }

    public Grid(float cellWidth, float cellHeight, string bitstring)
    {
      this.CellWidth = cellWidth;
      this.CellHeight = cellHeight;
      int num = 0;
      int val1 = 0;
      int rows = 1;
      for (int index = 0; index < bitstring.Length; ++index)
      {
        if (bitstring[index] == '\n')
        {
          ++rows;
          num = Math.Max(val1, num);
          val1 = 0;
        }
        else
          ++val1;
      }
      this.Data = new VirtualMap<bool>(num, rows);
      this.LoadBitstring(bitstring);
    }

    public Grid(float cellWidth, float cellHeight, bool[,] data)
    {
      this.CellWidth = cellWidth;
      this.CellHeight = cellHeight;
      this.Data = new VirtualMap<bool>(data);
    }

    public Grid(float cellWidth, float cellHeight, VirtualMap<bool> data)
    {
      this.CellWidth = cellWidth;
      this.CellHeight = cellHeight;
      this.Data = data;
    }

    public void Extend(int left, int right, int up, int down)
    {
      this.Position = this.Position - new Vector2((float) left * this.CellWidth, (float) up * this.CellHeight);
      int columns = this.Data.Columns + left + right;
      int rows = this.Data.Rows + up + down;
      if (columns <= 0 || rows <= 0)
      {
        this.Data = new VirtualMap<bool>(0, 0);
      }
      else
      {
        VirtualMap<bool> virtualMap = new VirtualMap<bool>(columns, rows);
        for (int x1 = 0; x1 < this.Data.Columns; ++x1)
        {
          for (int y1 = 0; y1 < this.Data.Rows; ++y1)
          {
            int x2 = x1 + left;
            int y2 = y1 + up;
            if (x2 >= 0 && x2 < columns && y2 >= 0 && y2 < rows)
              virtualMap[x2, y2] = this.Data[x1, y1];
          }
        }
        for (int x = 0; x < left; ++x)
        {
          for (int y = 0; y < rows; ++y)
            virtualMap[x, y] = this.Data[0, Calc.Clamp(y - up, 0, this.Data.Rows - 1)];
        }
        for (int x = columns - right; x < columns; ++x)
        {
          for (int y = 0; y < rows; ++y)
            virtualMap[x, y] = this.Data[this.Data.Columns - 1, Calc.Clamp(y - up, 0, this.Data.Rows - 1)];
        }
        for (int y = 0; y < up; ++y)
        {
          for (int x = 0; x < columns; ++x)
            virtualMap[x, y] = this.Data[Calc.Clamp(x - left, 0, this.Data.Columns - 1), 0];
        }
        for (int y = rows - down; y < rows; ++y)
        {
          for (int x = 0; x < columns; ++x)
            virtualMap[x, y] = this.Data[Calc.Clamp(x - left, 0, this.Data.Columns - 1), this.Data.Rows - 1];
        }
        this.Data = virtualMap;
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
          for (; x < this.CellsX; ++x)
            this.Data[x, y] = false;
          x = 0;
          ++y;
          if (y >= this.CellsY)
            break;
        }
        else if (x < this.CellsX)
        {
          if (bitstring[index] == '0')
          {
            this.Data[x, y] = false;
            ++x;
          }
          else
          {
            this.Data[x, y] = true;
            ++x;
          }
        }
      }
    }

    public string GetBitstring()
    {
      string bitstring = "";
      for (int y = 0; y < this.CellsY; ++y)
      {
        if (y != 0)
          bitstring += "\n";
        for (int x = 0; x < this.CellsX; ++x)
          bitstring = !this.Data[x, y] ? bitstring + "0" : bitstring + "1";
      }
      return bitstring;
    }

    public void Clear(bool to = false)
    {
      for (int x = 0; x < this.CellsX; ++x)
      {
        for (int y = 0; y < this.CellsY; ++y)
          this.Data[x, y] = to;
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
      if (x + width > this.CellsX)
        width = this.CellsX - x;
      if (y + height > this.CellsY)
        height = this.CellsY - y;
      for (int index1 = 0; index1 < width; ++index1)
      {
        for (int index2 = 0; index2 < height; ++index2)
          this.Data[x + index1, y + index2] = to;
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
      if (x + width > this.CellsX)
        width = this.CellsX - x;
      if (y + height > this.CellsY)
        height = this.CellsY - y;
      for (int index1 = 0; index1 < width; ++index1)
      {
        for (int index2 = 0; index2 < height; ++index2)
        {
          if (this.Data[x + index1, y + index2])
            return true;
        }
      }
      return false;
    }

    public bool CheckColumn(int x)
    {
      for (int y = 0; y < this.CellsY; ++y)
      {
        if (!this.Data[x, y])
          return false;
      }
      return true;
    }

    public bool CheckRow(int y)
    {
      for (int x = 0; x < this.CellsX; ++x)
      {
        if (!this.Data[x, y])
          return false;
      }
      return true;
    }

    public bool this[int x, int y]
    {
      get => x >= 0 && y >= 0 && x < this.CellsX && y < this.CellsY && this.Data[x, y];
      set => this.Data[x, y] = value;
    }

    public int CellsX => this.Data.Columns;

    public int CellsY => this.Data.Rows;

    public override float Width
    {
      get => this.CellWidth * (float) this.CellsX;
      set => throw new NotImplementedException();
    }

    public override float Height
    {
      get => this.CellHeight * (float) this.CellsY;
      set => throw new NotImplementedException();
    }

    public bool IsEmpty
    {
      get
      {
        for (int x = 0; x < this.CellsX; ++x)
        {
          for (int y = 0; y < this.CellsY; ++y)
          {
            if (this.Data[x, y])
              return false;
          }
        }
        return true;
      }
    }

    public override float Left
    {
      get => this.Position.X;
      set => this.Position.X = value;
    }

    public override float Top
    {
      get => this.Position.Y;
      set => this.Position.Y = value;
    }

    public override float Right
    {
      get => this.Position.X + this.Width;
      set => this.Position.X = value - this.Width;
    }

    public override float Bottom
    {
      get => this.Position.Y + this.Height;
      set => this.Position.Y = value - this.Height;
    }

    public override Collider Clone() => (Collider) new Grid(this.CellWidth, this.CellHeight, this.Data.Clone());

    public override void Render(Camera camera, Color color)
    {
      if (camera == null)
      {
        for (int x = 0; x < this.CellsX; ++x)
        {
          for (int y = 0; y < this.CellsY; ++y)
          {
            if (this.Data[x, y])
              Draw.HollowRect(this.AbsoluteLeft + (float) x * this.CellWidth, this.AbsoluteTop + (float) y * this.CellHeight, this.CellWidth, this.CellHeight, color);
          }
        }
      }
      else
      {
        int num1 = (int) Math.Max(0.0f, (camera.Left - this.AbsoluteLeft) / this.CellWidth);
        int num2 = (int) Math.Min((double) (this.CellsX - 1), Math.Ceiling(((double) camera.Right - (double) this.AbsoluteLeft) / (double) this.CellWidth));
        int num3 = (int) Math.Max(0.0f, (camera.Top - this.AbsoluteTop) / this.CellHeight);
        int num4 = (int) Math.Min((double) (this.CellsY - 1), Math.Ceiling(((double) camera.Bottom - (double) this.AbsoluteTop) / (double) this.CellHeight));
        for (int x = num1; x <= num2; ++x)
        {
          for (int y = num3; y <= num4; ++y)
          {
            if (this.Data[x, y])
              Draw.HollowRect(this.AbsoluteLeft + (float) x * this.CellWidth, this.AbsoluteTop + (float) y * this.CellHeight, this.CellWidth, this.CellHeight, color);
          }
        }
      }
    }

    public override bool Collide(Vector2 point) => (double) point.X >= (double) this.AbsoluteLeft && (double) point.Y >= (double) this.AbsoluteTop && (double) point.X < (double) this.AbsoluteRight && (double) point.Y < (double) this.AbsoluteBottom && this.Data[(int) (((double) point.X - (double) this.AbsoluteLeft) / (double) this.CellWidth), (int) (((double) point.Y - (double) this.AbsoluteTop) / (double) this.CellHeight)];

    public override bool Collide(Rectangle rect)
    {
      if (!rect.Intersects(this.Bounds))
        return false;
      int x = (int) (((double) rect.Left - (double) this.AbsoluteLeft) / (double) this.CellWidth);
      int y = (int) (((double) rect.Top - (double) this.AbsoluteTop) / (double) this.CellHeight);
      int width = (int) (((double) rect.Right - (double) this.AbsoluteLeft - 1.0) / (double) this.CellWidth) - x + 1;
      int height = (int) (((double) rect.Bottom - (double) this.AbsoluteTop - 1.0) / (double) this.CellHeight) - y + 1;
      return this.CheckRect(x, y, width, height);
    }

    public override bool Collide(Vector2 from, Vector2 to)
    {
      from -= this.AbsolutePosition;
      to -= this.AbsolutePosition;
      from /= new Vector2(this.CellWidth, this.CellHeight);
      to /= new Vector2(this.CellWidth, this.CellHeight);
      bool flag = (double) Math.Abs(to.Y - from.Y) > (double) Math.Abs(to.X - from.X);
      if (flag)
      {
        float x1 = from.X;
        from.X = from.Y;
        from.Y = x1;
        float x2 = to.X;
        to.X = to.Y;
        to.Y = x2;
      }
      if ((double) from.X > (double) to.X)
      {
        Vector2 vector2 = from;
        from = to;
        to = vector2;
      }
      float num1 = 0.0f;
      float num2 = Math.Abs(to.Y - from.Y) / (to.X - from.X);
      int num3 = (double) from.Y < (double) to.Y ? 1 : -1;
      int y = (int) from.Y;
      int x3 = (int) to.X;
      for (int x4 = (int) from.X; x4 <= x3; ++x4)
      {
        if (flag)
        {
          if (this[y, x4])
            return true;
        }
        else if (this[x4, y])
          return true;
        num1 += num2;
        if ((double) num1 >= 0.5)
        {
          y += num3;
          --num1;
        }
      }
      return false;
    }

    public override bool Collide(Hitbox hitbox) => this.Collide(hitbox.Bounds);

    public override bool Collide(Grid grid) => throw new NotImplementedException();

    public override bool Collide(Circle circle) => false;

    public override bool Collide(ColliderList list) => list.Collide(this);

    public static bool IsBitstringEmpty(string bitstring)
    {
      for (int index = 0; index < bitstring.Length; ++index)
      {
        if (bitstring[index] == '1')
          return false;
      }
      return true;
    }
  }
}
