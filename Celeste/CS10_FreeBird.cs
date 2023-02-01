// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_FreeBird
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
  public class CS10_FreeBird : CutsceneEntity
  {
    public CS10_FreeBird()
      : base()
    {
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS10_FreeBird cs10FreeBird = this;
      yield return (object) Textbox.Say("CH9_FREE_BIRD");
      FadeWipe fadeWipe = new FadeWipe((Scene) level, false);
      fadeWipe.Duration = 3f;
      yield return (object) fadeWipe.Duration;
      cs10FreeBird.EndCutscene(level);
    }

    public override void OnEnd(Level level) => level.CompleteArea(false, true);
  }
}
