// Decompiled with JetBrains decompiler
// Type: Celeste.CollisionData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
    public struct CollisionData
    {
        public Vector2 Direction;
        public Vector2 Moved;
        public Vector2 TargetPosition;
        public Platform Hit;
        public Solid Pusher;
        public static readonly CollisionData Empty;
    }
}
