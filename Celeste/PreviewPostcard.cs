// Decompiled with JetBrains decompiler
// Type: Celeste.PreviewPostcard
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
  public class PreviewPostcard : Scene
  {
    private Postcard postcard;

    public PreviewPostcard(Postcard postcard)
    {
      Audio.SetMusic((string) null);
      Audio.SetAmbience((string) null);
      this.postcard = postcard;
      this.Add(new Entity()
      {
        (Component) new Coroutine(this.Routine(postcard))
      });
      this.Add((Monocle.Renderer) new HudRenderer());
    }

    private IEnumerator Routine(Postcard postcard)
    {
      PreviewPostcard previewPostcard = this;
      yield return (object) 0.25f;
      previewPostcard.Add((Entity) postcard);
      yield return (object) postcard.DisplayRoutine();
      Engine.Scene = (Scene) new OverworldLoader(Overworld.StartMode.MainMenu);
    }

    public override void BeforeRender()
    {
      base.BeforeRender();
      if (this.postcard == null)
        return;
      this.postcard.BeforeRender();
    }
  }
}
