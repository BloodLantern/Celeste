// Decompiled with JetBrains decompiler
// Type: Celeste.BloomPoint
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class BloomPoint : Component
  {
    public Vector2 Position = Vector2.Zero;
    public float Alpha = 1f;
    public float Radius = 8f;

    public float X
    {
      get => this.Position.X;
      set => this.Position.X = value;
    }

    public float Y
    {
      get => this.Position.Y;
      set => this.Position.Y = value;
    }

    public BloomPoint(float alpha, float radius)
      : base(false, true)
    {
      this.Alpha = alpha;
      this.Radius = radius;
    }

    public BloomPoint(Vector2 position, float alpha, float radius)
      : base(false, true)
    {
      this.Position = position;
      this.Alpha = alpha;
      this.Radius = radius;
    }
  }
}
