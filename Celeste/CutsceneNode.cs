// Decompiled with JetBrains decompiler
// Type: Celeste.CutsceneNode
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  [Tracked(false)]
  public class CutsceneNode : Entity
  {
    public string Name;

    public CutsceneNode(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Name = data.Attr("nodeName");
    }

    public static CutsceneNode Find(string name)
    {
      foreach (CutsceneNode entity in Engine.Scene.Tracker.GetEntities<CutsceneNode>())
      {
        if (entity.Name != null && entity.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
          return entity;
      }
      return (CutsceneNode) null;
    }
  }
}
