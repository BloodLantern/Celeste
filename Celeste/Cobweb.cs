// Decompiled with JetBrains decompiler
// Type: Celeste.Cobweb
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class Cobweb : Entity
    {
        private Color color;
        private Color edge;
        private Vector2 anchorA;
        private Vector2 anchorB;
        private readonly List<Vector2> offshoots;
        private readonly List<float> offshootEndings;
        private float waveTimer;

        public Cobweb(EntityData data, Vector2 offset)
        {
            Depth = -1;
            anchorA = Position = data.Position + offset;
            anchorB = data.Nodes[0] + offset;
            foreach (Vector2 node in data.Nodes)
            {
                if (offshoots == null)
                {
                    offshoots = new List<Vector2>();
                    offshootEndings = new List<float>();
                }
                else
                {
                    offshoots.Add(node + offset);
                    offshootEndings.Add(0.3f + Calc.Random.NextFloat(0.4f));
                }
            }
            waveTimer = Calc.Random.NextFloat();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            color = Calc.Random.Choose<Color>(AreaData.Get(scene).CobwebColor);
            edge = Color.Lerp(color, Calc.HexToColor("0f0e17"), 0.2f);
            if (!Scene.CollideCheck<Solid>(new Rectangle((int)anchorA.X - 2, (int)anchorA.Y - 2, 4, 4)) || !Scene.CollideCheck<Solid>(new Rectangle((int)anchorB.X - 2, (int)anchorB.Y - 2, 4, 4)))
            {
                RemoveSelf();
            }

            for (int index = 0; index < offshoots.Count; ++index)
            {
                Vector2 offshoot = offshoots[index];
                if (!Scene.CollideCheck<Solid>(new Rectangle((int)offshoot.X - 2, (int)offshoot.Y - 2, 4, 4)))
                {
                    offshoots.RemoveAt(index);
                    offshootEndings.RemoveAt(index);
                    --index;
                }
            }
        }

        public override void Update()
        {
            waveTimer += Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            DrawCobweb(anchorA, anchorB, 12, true);
        }

        private void DrawCobweb(Vector2 a, Vector2 b, int steps, bool drawOffshoots)
        {
            SimpleCurve simpleCurve = new(a, b, ((a + b) / 2f) + (Vector2.UnitY * (float)(8.0 + (Math.Sin(waveTimer) * 4.0))));
            if (drawOffshoots && offshoots != null)
            {
                for (int index = 0; index < offshoots.Count; ++index)
                {
                    DrawCobweb(offshoots[index], simpleCurve.GetPoint(offshootEndings[index]), 4, false);
                }
            }
            Vector2 start = simpleCurve.Begin;
            for (int index = 1; index <= steps; ++index)
            {
                float percent = index / (float)steps;
                Vector2 point = simpleCurve.GetPoint(percent);
                Draw.Line(start, point, index <= 2 || index >= steps - 1 ? edge : color);
                start = point + (start - point).SafeNormalize();
            }
        }
    }
}
