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

        public bool CombinesWith(Vector2 position) => Vector2.DistanceSquared(Position, position) <= 100.0;

        public void Render()
        {
            float num1 = Math.Min(0.7f, (float) (0.30000001192092896 + 0.10000000149011612 * Amount));
            int num2 = Math.Min(6, Amount + 1);
            Draw.Rect(Position.X - num2, Position.Y - num2, num2 * 2, num2 * 2, Color.Red * num1);
        }
    }
}
