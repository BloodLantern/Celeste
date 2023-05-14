// Decompiled with JetBrains decompiler
// Type: Monocle.Pnt
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
    public struct Pnt
    {
        public static readonly Pnt Zero = new(0, 0);
        public static readonly Pnt UnitX = new(1, 0);
        public static readonly Pnt UnitY = new(0, 1);
        public static readonly Pnt One = new(1, 1);
        public int X;
        public int Y;

        public Pnt(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Pnt a, Pnt b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Pnt a, Pnt b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static Pnt operator +(Pnt a, Pnt b)
        {
            return new Pnt(a.X + b.X, a.Y + b.Y);
        }

        public static Pnt operator -(Pnt a, Pnt b)
        {
            return new Pnt(a.X - b.X, a.Y - b.Y);
        }

        public static Pnt operator *(Pnt a, Pnt b)
        {
            return new Pnt(a.X * b.X, a.Y * b.Y);
        }

        public static Pnt operator /(Pnt a, Pnt b)
        {
            return new Pnt(a.X / b.X, a.Y / b.Y);
        }

        public static Pnt operator %(Pnt a, Pnt b)
        {
            return new Pnt(a.X % b.X, a.Y % b.Y);
        }

        public static bool operator ==(Pnt a, int b)
        {
            return a.X == b && a.Y == b;
        }

        public static bool operator !=(Pnt a, int b)
        {
            return a.X != b || a.Y != b;
        }

        public static Pnt operator +(Pnt a, int b)
        {
            return new Pnt(a.X + b, a.Y + b);
        }

        public static Pnt operator -(Pnt a, int b)
        {
            return new Pnt(a.X - b, a.Y - b);
        }

        public static Pnt operator *(Pnt a, int b)
        {
            return new Pnt(a.X * b, a.Y * b);
        }

        public static Pnt operator /(Pnt a, int b)
        {
            return new Pnt(a.X / b, a.Y / b);
        }

        public static Pnt operator %(Pnt a, int b)
        {
            return new Pnt(a.X % b, a.Y % b);
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return (X * 10000) + Y;
        }

        public override string ToString()
        {
            return "{ X: " + X + ", Y: " + Y + " }";
        }
    }
}
