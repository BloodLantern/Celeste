// Decompiled with JetBrains decompiler
// Type: Celeste.GrannyLaughSfx
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
  public class GrannyLaughSfx : Component
  {
    public bool FirstPlay;
    private Sprite sprite;
    private bool ready = true;

    public GrannyLaughSfx(Sprite sprite)
      : base(true, false)
    {
      this.sprite = sprite;
    }

    public override void Update()
    {
      if (this.sprite.CurrentAnimationID == "laugh" && this.sprite.CurrentAnimationFrame == 0 && this.ready)
      {
        if (this.FirstPlay)
          Audio.Play("event:/char/granny/laugh_firstphrase", this.Entity.Position);
        else
          Audio.Play("event:/char/granny/laugh_oneha", this.Entity.Position);
        this.ready = false;
      }
      if (this.FirstPlay || !(this.sprite.CurrentAnimationID != "laugh") && this.sprite.CurrentAnimationFrame <= 0)
        return;
      this.ready = true;
    }
  }
}
