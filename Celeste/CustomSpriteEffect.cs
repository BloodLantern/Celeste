// Decompiled with JetBrains decompiler
// Type: Celeste.CustomSpriteEffect
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste
{
  public class CustomSpriteEffect : Effect
  {
    private EffectParameter matrixParam;

    public CustomSpriteEffect(Effect effect)
      : base(effect)
    {
      this.matrixParam = this.Parameters["MatrixTransform"];
    }

    protected override void OnApply()
    {
      Viewport viewport = this.GraphicsDevice.Viewport;
      Matrix orthographicOffCenter = Matrix.CreateOrthographicOffCenter(0.0f, (float) viewport.Width, (float) viewport.Height, 0.0f, 0.0f, 1f);
      this.matrixParam.SetValue(Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * orthographicOffCenter);
      base.OnApply();
    }
  }
}
