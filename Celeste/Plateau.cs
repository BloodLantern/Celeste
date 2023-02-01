// Decompiled with JetBrains decompiler
// Type: Celeste.Plateau
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class Plateau : Solid
  {
    private Monocle.Image sprite;
    public LightOcclude Occluder;

    public Plateau(EntityData e, Vector2 offset)
      : base(e.Position + offset, 104f, 4f, true)
    {
      this.Collider.Left += 8f;
      this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["scenery/fallplateau"])));
      this.Add((Component) (this.Occluder = new LightOcclude()));
      this.SurfaceSoundIndex = 23;
      this.EnableAssistModeChecks = false;
    }
  }
}
