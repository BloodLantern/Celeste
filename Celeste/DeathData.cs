// Decompiled with JetBrains decompiler
// Type: Celeste.DeathData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class DeathData
    {
        public Vector2 Position;
        public int Amount;

        public DeathData(Vector2 position)
        {
            Position = position;
            Amount = 1;
        }

        public DeathData(DeathData old, Vector2 add)
        {
            Position = Vector2.Lerp(old.Position, add, 1f / (old.Amount + 1));
            Amount = old.Amount + 1;
        }

        public bool CombinesWith(Vector2 position)
        {
            return (double)Vector2.DistanceSquared(Position, position) <= 100.0;
        }

        public void Render()
        {
            float num1 = Math.Min(0.7f, (float)(0.30000001192092896 + (0.10000000149011612 * Amount)));
            int num2 = Math.Min(6, Amount + 1);
            Draw.Rect(Position.X - num2, Position.Y - num2, num2 * 2, num2 * 2, Color.Red * num1);
        }
    }
}
