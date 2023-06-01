// Decompiled with JetBrains decompiler
// Type: Celeste.WindMover
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class WindMover : Component
    {
        public Action<Vector2> Move;

        public WindMover(Action<Vector2> move)
            : base(false, false)
        {
            this.Move = move;
        }
    }
}
