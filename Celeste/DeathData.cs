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
            this.Position = position;
            this.Amount = 1;
        }

        public DeathData(DeathData old, Vector2 add)
        {
            this.Position = Vector2.Lerp(old.Position, add, 1f / (float) (old.Amount + 1));
            this.Amount = old.Amount + 1;
        }

        public bool CombinesWith(Vector2 position) => (double) Vector2.DistanceSquared(this.Position, position) <= 100.0;

        public void Render()
        {
            float num1 = Math.Min(0.7f, (float) (0.30000001192092896 + 0.10000000149011612 * (double) this.Amount));
            int num2 = Math.Min(6, this.Amount + 1);
            Draw.Rect(this.Position.X - (float) num2, this.Position.Y - (float) num2, (float) (num2 * 2), (float) (num2 * 2), Color.Red * num1);
        }
    }
}
