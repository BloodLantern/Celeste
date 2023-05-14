// Decompiled with JetBrains decompiler
// Type: Celeste.LightningStrike
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class LightningStrike : Entity
    {
        private bool on;
        private float scale;
        private readonly Random rand;
        private readonly float strikeHeight;
        private LightningStrike.Node strike;

        public LightningStrike(Vector2 position, int seed, float height, float delay = 0.0f)
        {
            Position = position;
            Depth = 10010;
            rand = new Random(seed);
            strikeHeight = height;
            Add(new Coroutine(Routine(delay)));
        }

        private IEnumerator Routine(float delay)
        {
            LightningStrike lightningStrike = this;
            if ((double)delay > 0.0)
            {
                yield return delay;
            }

            lightningStrike.scale = 1f;
            lightningStrike.GenerateStikeNodes(-1, 10f);
            for (int j = 0; j < 5; ++j)
            {
                lightningStrike.on = true;
                yield return (float)((1.0 - (j / 5.0)) * 0.10000000149011612);
                lightningStrike.scale -= 0.2f;
                lightningStrike.on = false;
                lightningStrike.strike.Wiggle(lightningStrike.rand);
                yield return 0.01f;
            }
            lightningStrike.RemoveSelf();
        }

        private void GenerateStikeNodes(int direction, float size, LightningStrike.Node parent = null)
        {
            parent ??= strike = new LightningStrike.Node(0.0f, 0.0f, size);
            if (parent.Position.Y >= (double)strikeHeight)
            {
                return;
            }

            float x = direction * rand.Range(-8, 20);
            float y = rand.Range(8, 16);
            float size1 = (float)(0.25 + ((1.0 - ((parent.Position.Y + (double)y) / strikeHeight)) * 0.75)) * size;
            LightningStrike.Node parent1 = new(parent.Position + new Vector2(x, y), size1);
            parent.Children.Add(parent1);
            GenerateStikeNodes(direction, size, parent1);
            if (!rand.Chance(0.1f))
            {
                return;
            }

            LightningStrike.Node parent2 = new(parent.Position + new Vector2(-x, y * 1.5f), size1);
            parent.Children.Add(parent2);
            GenerateStikeNodes(-direction, size, parent2);
        }

        public override void Render()
        {
            if (!on)
            {
                return;
            }

            strike.Render(Position, scale);
        }

        private class Node
        {
            public Vector2 Position;
            public float Size;
            public List<LightningStrike.Node> Children;

            public Node(float x, float y, float size)
                : this(new Vector2(x, y), size)
            {
            }

            public Node(Vector2 position, float size)
            {
                Position = position;
                Children = new List<LightningStrike.Node>();
                Size = size;
            }

            public void Wiggle(Random rand)
            {
                Position.X += rand.Range(-2, 2);
                if (Position.Y != 0.0)
                {
                    Position.Y += rand.Range(-1, 1);
                }

                foreach (LightningStrike.Node child in Children)
                {
                    child.Wiggle(rand);
                }
            }

            public void Render(Vector2 offset, float scale)
            {
                float thickness = Size * scale;
                foreach (LightningStrike.Node child in Children)
                {
                    Vector2 vector2 = (child.Position - Position).SafeNormalize();
                    Draw.Line(offset + Position, offset + child.Position + (vector2 * thickness * 0.5f), Color.White, thickness);
                    child.Render(offset, scale);
                }
            }
        }
    }
}
