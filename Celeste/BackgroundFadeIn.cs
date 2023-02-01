// Decompiled with JetBrains decompiler
// Type: Celeste.BackgroundFadeIn
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class BackgroundFadeIn : Entity
  {
    public Color Color;
    public float Duration;
    public float Delay;
    public float Percent;

    public BackgroundFadeIn(Color color, float delay, float duration)
    {
      this.Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
      this.Depth = 10100;
      this.Color = color;
      this.Delay = delay;
      this.Duration = duration;
      this.Percent = 0.0f;
    }

    public override void Update()
    {
      if ((double) this.Delay <= 0.0)
      {
        if ((double) this.Percent >= 1.0)
          this.RemoveSelf();
        this.Percent += Engine.DeltaTime / this.Duration;
      }
      else
        this.Delay -= Engine.DeltaTime;
      base.Update();
    }

    public override void Render()
    {
      Vector2 position = (this.Scene as Level).Camera.Position;
      Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, this.Color * (1f - this.Percent));
    }
  }
}
