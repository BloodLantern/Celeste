﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class Tentacles : Backdrop
    {
        private const int NodesPerTentacle = 10;
        private Side side;
        private float width;
        private Vector2 origin;
        private Vector2 outwards;
        private float outwardsOffset;
        private VertexPositionColor[] vertices;
        private int vertexCount;
        private Tentacle[] tentacles;
        private int tentacleCount;
        private float hideTimer = 5f;

        public Tentacles(Side side, Color color, float outwardsOffset)
        {
            this.side = side;
            this.outwardsOffset = outwardsOffset;
            UseSpritebatch = false;
            switch (side)
            {
                case Side.Right:
                    outwards = new Vector2(-1f, 0.0f);
                    width = 180f;
                    origin = new Vector2(320f, 90f);
                    break;
                case Side.Left:
                    outwards = new Vector2(1f, 0.0f);
                    width = 180f;
                    origin = new Vector2(0.0f, 90f);
                    break;
                case Side.Top:
                    outwards = new Vector2(0.0f, 1f);
                    width = 320f;
                    origin = new Vector2(160f, 0.0f);
                    break;
                case Side.Bottom:
                    outwards = new Vector2(0.0f, -1f);
                    width = 320f;
                    origin = new Vector2(160f, 180f);
                    break;
            }
            float num = 0.0f;
            tentacles = new Tentacle[100];
            for (int index = 0; index < tentacles.Length && num < width + 40.0; ++index)
            {
                tentacles[index].Length = Calc.Random.NextFloat();
                tentacles[index].Offset = Calc.Random.NextFloat();
                tentacles[index].Step = Calc.Random.NextFloat();
                tentacles[index].Position = -200f;
                tentacles[index].Approach = Calc.Random.NextFloat();
                num += tentacles[index].Width = 6f + Calc.Random.NextFloat(20f);
                ++tentacleCount;
            }
            vertices = new VertexPositionColor[tentacleCount * 11 * 6];
            for (int index = 0; index < vertices.Length; ++index)
                vertices[index].Color = color;
        }

        public override void Update(Scene scene)
        {
            int num1 = IsVisible(scene as Level) ? 1 : 0;
            float num2 = 0.0f;
            if (num1 != 0)
            {
                Camera camera = (scene as Level).Camera;
                Player entity = scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    if (side == Side.Right)
                        num2 = (float) (320.0 - (entity.X - (double) camera.X) - 160.0);
                    else if (side == Side.Bottom)
                        num2 = (float) (180.0 - (entity.Y - (double) camera.Y) - 180.0);
                }
                hideTimer = 0.0f;
            }
            else
            {
                num2 = -200f;
                hideTimer += Engine.DeltaTime;
            }
            float num3 = num2 + outwardsOffset;
            Visible = hideTimer < 5.0;
            if (!Visible)
                return;
            Vector2 vector2_1 = -outwards.Perpendicular();
            int num4 = 0;
            Vector2 vector2_2 = origin - vector2_1 * (float) (width / 2.0 + 2.0);
            for (int index1 = 0; index1 < tentacleCount; ++index1)
            {
                Vector2 vector2_3 = vector2_2 + vector2_1 * tentacles[index1].Width * 0.5f;
                tentacles[index1].Position += (float) ((num3 - (double) tentacles[index1].Position) * (1.0 - Math.Pow(0.5 * (0.5 + tentacles[index1].Approach * 0.5), Engine.DeltaTime)));
                Vector2 vector2_4 = (float) (tentacles[index1].Position + Math.Sin(scene.TimeActive + tentacles[index1].Offset * 4.0) * 8.0 + (origin - vector2_3).Length() * 0.699999988079071) * outwards;
                Vector2 vector2_5 = vector2_3 + vector2_4;
                float num5 = (float) (2.0 + tentacles[index1].Length * 8.0);
                Vector2 vector2_6 = vector2_5;
                Vector2 vector2_7 = vector2_1 * tentacles[index1].Width * 0.5f;
                VertexPositionColor[] vertices1 = vertices;
                int index2 = num4;
                int num6 = index2 + 1;
                vertices1[index2].Position = new Vector3(vector2_3 + vector2_7, 0.0f);
                VertexPositionColor[] vertices2 = vertices;
                int index3 = num6;
                int num7 = index3 + 1;
                vertices2[index3].Position = new Vector3(vector2_3 - vector2_7, 0.0f);
                VertexPositionColor[] vertices3 = vertices;
                int index4 = num7;
                int num8 = index4 + 1;
                vertices3[index4].Position = new Vector3(vector2_5 - vector2_7, 0.0f);
                VertexPositionColor[] vertices4 = vertices;
                int index5 = num8;
                int num9 = index5 + 1;
                vertices4[index5].Position = new Vector3(vector2_5 - vector2_7, 0.0f);
                VertexPositionColor[] vertices5 = vertices;
                int index6 = num9;
                int num10 = index6 + 1;
                vertices5[index6].Position = new Vector3(vector2_3 + vector2_7, 0.0f);
                VertexPositionColor[] vertices6 = vertices;
                int index7 = num10;
                num4 = index7 + 1;
                vertices6[index7].Position = new Vector3(vector2_5 + vector2_7, 0.0f);
                for (int y = 1; y < 10; ++y)
                {
                    double num11 = scene.TimeActive * (double) tentacles[index1].Offset * Math.Pow(1.1000000238418579, y) * 2.0;
                    float num12 = (float) (tentacles[index1].Offset * 3.0 + y * (0.10000000149011612 + tentacles[index1].Step * 0.20000000298023224) + num5 * (double) y * 0.10000000149011612);
                    float num13 = (float) (2.0 + 4.0 * (y / 10.0));
                    double num14 = num12;
                    Vector2 vector2_8 = (float) Math.Sin(num11 + num14) * vector2_1 * num13;
                    float num15 = (float) ((1.0 - y / 10.0) * tentacles[index1].Width * 0.5);
                    Vector2 vector2_9 = vector2_6 + outwards * num5 + vector2_8;
                    Vector2 vector2_10 = (vector2_6 - vector2_9).SafeNormalize().Perpendicular() * num15;
                    VertexPositionColor[] vertices7 = vertices;
                    int index8 = num4;
                    int num16 = index8 + 1;
                    vertices7[index8].Position = new Vector3(vector2_6 + vector2_7, 0.0f);
                    VertexPositionColor[] vertices8 = vertices;
                    int index9 = num16;
                    int num17 = index9 + 1;
                    vertices8[index9].Position = new Vector3(vector2_6 - vector2_7, 0.0f);
                    VertexPositionColor[] vertices9 = vertices;
                    int index10 = num17;
                    int num18 = index10 + 1;
                    vertices9[index10].Position = new Vector3(vector2_9 - vector2_10, 0.0f);
                    VertexPositionColor[] vertices10 = vertices;
                    int index11 = num18;
                    int num19 = index11 + 1;
                    vertices10[index11].Position = new Vector3(vector2_9 - vector2_10, 0.0f);
                    VertexPositionColor[] vertices11 = vertices;
                    int index12 = num19;
                    int num20 = index12 + 1;
                    vertices11[index12].Position = new Vector3(vector2_6 + vector2_7, 0.0f);
                    VertexPositionColor[] vertices12 = vertices;
                    int index13 = num20;
                    num4 = index13 + 1;
                    vertices12[index13].Position = new Vector3(vector2_9 + vector2_10, 0.0f);
                    vector2_6 = vector2_9;
                    vector2_7 = vector2_10;
                }
                vector2_2 = vector2_3 + vector2_1 * tentacles[index1].Width * 0.5f;
            }
            vertexCount = num4;
        }

        public override void Render(Scene scene)
        {
            if (vertexCount <= 0)
                return;
            GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
        }

        public enum Side
        {
            Right,
            Left,
            Top,
            Bottom,
        }

        private struct Tentacle
        {
            public float Length;
            public float Offset;
            public float Step;
            public float Position;
            public float Approach;
            public float Width;
        }
    }
}
