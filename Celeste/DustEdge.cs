// Decompiled with JetBrains decompiler
// Type: Celeste.DustEdge
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
  [Tracked(false)]
  public class DustEdge : Component
  {
    public Action RenderDust;

    public DustEdge(Action onRenderDust)
      : base(false, true)
    {
      this.RenderDust = onRenderDust;
    }
  }
}
