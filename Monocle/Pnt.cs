// Decompiled with JetBrains decompiler
// Type: Monocle.Pnt
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
  public struct Pnt
  {
    public static readonly Pnt Zero = new Pnt(0, 0);
    public static readonly Pnt UnitX = new Pnt(1, 0);
    public static readonly Pnt UnitY = new Pnt(0, 1);
    public static readonly Pnt One = new Pnt(1, 1);
    public int X;
    public int Y;

    public Pnt(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public static bool operator ==(Pnt a, Pnt b) => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(Pnt a, Pnt b) => a.X != b.X || a.Y != b.Y;

    public static Pnt operator +(Pnt a, Pnt b) => new Pnt(a.X + b.X, a.Y + b.Y);

    public static Pnt operator -(Pnt a, Pnt b) => new Pnt(a.X - b.X, a.Y - b.Y);

    public static Pnt operator *(Pnt a, Pnt b) => new Pnt(a.X * b.X, a.Y * b.Y);

    public static Pnt operator /(Pnt a, Pnt b) => new Pnt(a.X / b.X, a.Y / b.Y);

    public static Pnt operator %(Pnt a, Pnt b) => new Pnt(a.X % b.X, a.Y % b.Y);

    public static bool operator ==(Pnt a, int b) => a.X == b && a.Y == b;

    public static bool operator !=(Pnt a, int b) => a.X != b || a.Y != b;

    public static Pnt operator +(Pnt a, int b) => new Pnt(a.X + b, a.Y + b);

    public static Pnt operator -(Pnt a, int b) => new Pnt(a.X - b, a.Y - b);

    public static Pnt operator *(Pnt a, int b) => new Pnt(a.X * b, a.Y * b);

    public static Pnt operator /(Pnt a, int b) => new Pnt(a.X / b, a.Y / b);

    public static Pnt operator %(Pnt a, int b) => new Pnt(a.X % b, a.Y % b);

    public override bool Equals(object obj) => false;

    public override int GetHashCode() => this.X * 10000 + this.Y;

    public override string ToString() => "{ X: " + (object) this.X + ", Y: " + (object) this.Y + " }";
  }
}
