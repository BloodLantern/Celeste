﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class DreamWipe : ScreenWipe
    {
        private readonly int circleColumns = 15;
        private readonly int circleRows = 8;
        private const int circleSegments = 32;
        private const float circleFillSpeed = 400f;
        private static Circle[] circles;
        private static VertexPositionColor[] vertexBuffer;

        public DreamWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            if (DreamWipe.vertexBuffer == null)
                DreamWipe.vertexBuffer = new VertexPositionColor[(circleColumns + 2) * (circleRows + 2) * 32 * 3];
            if (DreamWipe.circles == null)
                DreamWipe.circles = new Circle[(circleColumns + 2) * (circleRows + 2)];
            for (int index = 0; index < DreamWipe.vertexBuffer.Length; ++index)
                DreamWipe.vertexBuffer[index].Color = ScreenWipe.WipeColor;
            int num1 = 1920 / circleColumns;
            int num2 = 1080 / circleRows;
            int num3 = 0;
            int index1 = 0;
            for (; num3 < circleColumns + 2; ++num3)
            {
                for (int index2 = 0; index2 < circleRows + 2; ++index2)
                {
                    DreamWipe.circles[index1].Position = new Vector2((num3 - 1 + 0.2f + Calc.Random.NextFloat(0.6f)) * num1, (index2 - 1 + 0.2f + Calc.Random.NextFloat(0.6f)) * num2);
                    DreamWipe.circles[index1].Delay = Calc.Random.NextFloat(0.05f) + (float) ((WipeIn ? circleColumns - num3 : (double) num3) * 0.017999999225139618);
                    DreamWipe.circles[index1].Radius = WipeIn ? (float) (400.0 * (Duration - (double) DreamWipe.circles[index1].Delay)) : 0.0f;
                    ++index1;
                }
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            for (int index = 0; index < DreamWipe.circles.Length; ++index)
            {
                if (!WipeIn)
                {
                    DreamWipe.circles[index].Delay -= Engine.DeltaTime;
                    if (DreamWipe.circles[index].Delay <= 0.0)
                        DreamWipe.circles[index].Radius += Engine.DeltaTime * 400f;
                }
                else if (DreamWipe.circles[index].Radius > 0.0)
                    DreamWipe.circles[index].Radius -= Engine.DeltaTime * 400f;
                else
                    DreamWipe.circles[index].Radius = 0.0f;
            }
        }

        public override void Render(Scene scene)
        {
            int num1 = 0;
            for (int index1 = 0; index1 < DreamWipe.circles.Length; ++index1)
            {
                Circle circle = DreamWipe.circles[index1];
                Vector2 vector2 = new Vector2(1f, 0.0f);
                for (float num2 = 0.0f; num2 < 32.0; ++num2)
                {
                    Vector2 vector = Calc.AngleToVector((float) ((num2 + 1.0) / 32.0 * 6.2831854820251465), 1f);
                    VertexPositionColor[] vertexBuffer1 = DreamWipe.vertexBuffer;
                    int index2 = num1;
                    int num3 = index2 + 1;
                    vertexBuffer1[index2].Position = new Vector3(circle.Position, 0.0f);
                    VertexPositionColor[] vertexBuffer2 = DreamWipe.vertexBuffer;
                    int index3 = num3;
                    int num4 = index3 + 1;
                    vertexBuffer2[index3].Position = new Vector3(circle.Position + vector2 * circle.Radius, 0.0f);
                    VertexPositionColor[] vertexBuffer3 = DreamWipe.vertexBuffer;
                    int index4 = num4;
                    num1 = index4 + 1;
                    vertexBuffer3[index4].Position = new Vector3(circle.Position + vector * circle.Radius, 0.0f);
                    vector2 = vector;
                }
            }
            ScreenWipe.DrawPrimitives(DreamWipe.vertexBuffer);
        }

        private struct Circle
        {
            public Vector2 Position;
            public float Radius;
            public float Delay;
        }
    }
}
