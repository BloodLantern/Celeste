// Decompiled with JetBrains decompiler
// Type: Celeste.Tags
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
  public static class Tags
  {
    public static BitTag PauseUpdate;
    public static BitTag FrozenUpdate;
    public static BitTag TransitionUpdate;
    public static BitTag HUD;
    public static BitTag Persistent;
    public static BitTag Global;

    public static void Initialize()
    {
      Tags.PauseUpdate = new BitTag("pauseUpdate");
      Tags.FrozenUpdate = new BitTag("frozenUpdate");
      Tags.TransitionUpdate = new BitTag("transitionUpdate");
      Tags.HUD = new BitTag("hud");
      Tags.Persistent = new BitTag("persistent");
      Tags.Global = new BitTag("global");
    }
  }
}
