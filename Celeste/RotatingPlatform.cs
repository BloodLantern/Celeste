// Decompiled with JetBrains decompiler
// Type: Celeste.RotatingPlatform
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class RotatingPlatform : JumpThru
  {
    private const float RotateSpeed = 1.04719758f;
    private Vector2 center;
    private bool clockwise;
    private float length;
    private float currentAngle;

    public RotatingPlatform(Vector2 position, int width, Vector2 center, bool clockwise)
      : base(position, width, false)
    {
      this.Collider.Position.X = (float) (-width / 2);
      this.Collider.Position.Y = (float) (-(double) this.Height / 2.0);
      this.center = center;
      this.clockwise = clockwise;
      this.length = (position - center).Length();
      this.currentAngle = (position - center).Angle();
      this.SurfaceSoundIndex = 5;
      this.Add((Component) new LightOcclude(0.2f));
    }

    public override void Update()
    {
      base.Update();
      if (this.clockwise)
        this.currentAngle -= 1.04719758f * Engine.DeltaTime;
      else
        this.currentAngle += 1.04719758f * Engine.DeltaTime;
      this.currentAngle = Calc.WrapAngle(this.currentAngle);
      this.MoveTo(this.center + Calc.AngleToVector(this.currentAngle, this.length));
    }

    public override void Render()
    {
      base.Render();
      Draw.Rect(this.Collider, Color.White);
    }
  }
}
