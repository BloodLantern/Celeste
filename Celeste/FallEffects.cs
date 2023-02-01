// Decompiled with JetBrains decompiler
// Type: Celeste.FallEffects
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class FallEffects : Entity
  {
    private static readonly Color[] colors = new Color[2]
    {
      Color.White,
      Color.LightGray
    };
    private static readonly Color[] faded = new Color[2];
    private FallEffects.Particle[] particles = new FallEffects.Particle[50];
    private float fade;
    private bool enabled;
    public static float SpeedMultiplier = 1f;

    public FallEffects()
    {
      this.Tag = (int) Tags.Global;
      this.Depth = -1000000;
      for (int index = 0; index < this.particles.Length; ++index)
      {
        this.particles[index].Position = new Vector2((float) Calc.Random.Range(0, 320), (float) Calc.Random.Range(0, 180));
        this.particles[index].Speed = (float) Calc.Random.Range(120, 240);
        this.particles[index].Color = Calc.Random.Next(FallEffects.colors.Length);
      }
    }

    public static void Show(bool visible)
    {
      FallEffects fallEffects = Engine.Scene.Tracker.GetEntity<FallEffects>();
      if (fallEffects == null & visible)
        Engine.Scene.Add((Entity) (fallEffects = new FallEffects()));
      if (fallEffects != null)
        fallEffects.enabled = visible;
      FallEffects.SpeedMultiplier = 1f;
    }

    public override void Update()
    {
      base.Update();
      for (int index = 0; index < this.particles.Length; ++index)
        this.particles[index].Position -= Vector2.UnitY * this.particles[index].Speed * FallEffects.SpeedMultiplier * Engine.DeltaTime;
      this.fade = Calc.Approach(this.fade, this.enabled ? 1f : 0.0f, (this.enabled ? 1f : 4f) * Engine.DeltaTime);
    }

    public override void Render()
    {
      if ((double) this.fade <= 0.0)
        return;
      Camera camera = (this.Scene as Level).Camera;
      for (int index = 0; index < FallEffects.faded.Length; ++index)
        FallEffects.faded[index] = FallEffects.colors[index] * this.fade;
      for (int index = 0; index < this.particles.Length; ++index)
      {
        float height = 8f * FallEffects.SpeedMultiplier;
        Draw.Rect(new Vector2()
        {
          X = this.mod(this.particles[index].Position.X - camera.X, 320f),
          Y = this.mod((float) ((double) this.particles[index].Position.Y - (double) camera.Y - 16.0), 212f)
        } + camera.Position - new Vector2(0.0f, height / 2f), 1f, height, FallEffects.faded[this.particles[index].Color]);
      }
    }

    private float mod(float x, float m) => (x % m + m) % m;

    private struct Particle
    {
      public Vector2 Position;
      public float Speed;
      public int Color;
    }
  }
}
