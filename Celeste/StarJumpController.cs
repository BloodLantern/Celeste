﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class StarJumpController : Entity
    {
        private Level level;
        private Random random;
        private float minY;
        private float maxY;
        private float minX;
        private float maxX;
        private float cameraOffsetMarker;
        private float cameraOffsetTimer;
        public VirtualRenderTarget BlockFill;
        private const int RayCount = 100;
        private VertexPositionColor[] vertices = new VertexPositionColor[600];
        private int vertexCount;
        private Color rayColor = Calc.HexToColor("a3ffff") * 0.25f;
        private Ray[] rays = new Ray[100];

        public StarJumpController() => InitBlockFill();

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            minY = level.Bounds.Top + 80;
            maxY = level.Bounds.Top + 1800;
            minX = level.Bounds.Left + 80;
            maxX = level.Bounds.Right - 80;
            level.Session.Audio.Music.Event = "event:/music/lvl6/starjump";
            level.Session.Audio.Music.Layer(1, 1f);
            level.Session.Audio.Music.Layer(2, 0.0f);
            level.Session.Audio.Music.Layer(3, 0.0f);
            level.Session.Audio.Music.Layer(4, 0.0f);
            level.Session.Audio.Apply();
            random = new Random(666);
            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                float centerY = entity.CenterY;
                level.Session.Audio.Music.Layer(1, Calc.ClampedMap(centerY, maxY, minY, 1f, 0.0f));
                level.Session.Audio.Music.Layer(2, Calc.ClampedMap(centerY, maxY, minY));
                level.Session.Audio.Apply();
                if (level.CameraOffset.Y == -38.400001525878906)
                {
                    if (entity.StateMachine.State != 19)
                    {
                        cameraOffsetTimer += Engine.DeltaTime;
                        if (cameraOffsetTimer >= 0.5)
                        {
                            cameraOffsetTimer = 0.0f;
                            level.CameraOffset.Y = -12.8f;
                        }
                    }
                    else
                        cameraOffsetTimer = 0.0f;
                }
                else if (entity.StateMachine.State == 19)
                {
                    cameraOffsetTimer += Engine.DeltaTime;
                    if (cameraOffsetTimer >= 0.10000000149011612)
                    {
                        cameraOffsetTimer = 0.0f;
                        level.CameraOffset.Y = -38.4f;
                    }
                }
                else
                    cameraOffsetTimer = 0.0f;
                cameraOffsetMarker = level.Camera.Y;
            }
            else
            {
                level.Session.Audio.Music.Layer(1, 1f);
                level.Session.Audio.Music.Layer(2, 0.0f);
                level.Session.Audio.Apply();
            }
            UpdateBlockFill();
        }

        private void InitBlockFill()
        {
            for (int index = 0; index < rays.Length; ++index)
            {
                rays[index].Reset();
                rays[index].Percent = Calc.Random.NextFloat();
            }
        }

        private void UpdateBlockFill()
        {
            Level scene = Scene as Level;
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f);
            Vector2 vector2_1 = new Vector2(-vector.Y, vector.X);
            int num1 = 0;
            for (int index1 = 0; index1 < rays.Length; ++index1)
            {
                if (rays[index1].Percent >= 1.0)
                    rays[index1].Reset();
                rays[index1].Percent += Engine.DeltaTime / rays[index1].Duration;
                rays[index1].Y += 8f * Engine.DeltaTime;
                float percent = rays[index1].Percent;
                float x = mod(rays[index1].X - scene.Camera.X * 0.9f, 320f);
                float y = mod(rays[index1].Y - scene.Camera.Y * 0.7f, 580f) - 200f;
                float width = rays[index1].Width;
                float length = rays[index1].Length;
                Vector2 vector2_2 = new Vector2((int) x, (int) y);
                Color color = rayColor * Ease.CubeInOut(Calc.YoYo(percent));
                VertexPositionColor vertexPositionColor1 = new VertexPositionColor(new Vector3(vector2_2 + vector2_1 * width + vector * length, 0.0f), color);
                VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector2_2 - vector2_1 * width, 0.0f), color);
                VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector2_2 + vector2_1 * width, 0.0f), color);
                VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(vector2_2 - vector2_1 * width - vector * length, 0.0f), color);
                VertexPositionColor[] vertices1 = vertices;
                int index2 = num1;
                int num2 = index2 + 1;
                VertexPositionColor vertexPositionColor5 = vertexPositionColor1;
                vertices1[index2] = vertexPositionColor5;
                VertexPositionColor[] vertices2 = vertices;
                int index3 = num2;
                int num3 = index3 + 1;
                VertexPositionColor vertexPositionColor6 = vertexPositionColor2;
                vertices2[index3] = vertexPositionColor6;
                VertexPositionColor[] vertices3 = vertices;
                int index4 = num3;
                int num4 = index4 + 1;
                VertexPositionColor vertexPositionColor7 = vertexPositionColor3;
                vertices3[index4] = vertexPositionColor7;
                VertexPositionColor[] vertices4 = vertices;
                int index5 = num4;
                int num5 = index5 + 1;
                VertexPositionColor vertexPositionColor8 = vertexPositionColor2;
                vertices4[index5] = vertexPositionColor8;
                VertexPositionColor[] vertices5 = vertices;
                int index6 = num5;
                int num6 = index6 + 1;
                VertexPositionColor vertexPositionColor9 = vertexPositionColor3;
                vertices5[index6] = vertexPositionColor9;
                VertexPositionColor[] vertices6 = vertices;
                int index7 = num6;
                num1 = index7 + 1;
                VertexPositionColor vertexPositionColor10 = vertexPositionColor4;
                vertices6[index7] = vertexPositionColor10;
            }
            vertexCount = num1;
        }

        private void BeforeRender()
        {
            if (BlockFill == null)
                BlockFill = VirtualContent.CreateRenderTarget("block-fill", 320, 180);
            if (vertexCount <= 0)
                return;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BlockFill);
            Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Color.Black, Color.LightSkyBlue, 0.3f));
            GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (BlockFill != null)
                BlockFill.Dispose();
            BlockFill = null;
        }

        private float mod(float x, float m) => (x % m + m) % m;

        private struct Ray
        {
            public float X;
            public float Y;
            public float Percent;
            public float Duration;
            public float Width;
            public float Length;

            public void Reset()
            {
                Percent = 0.0f;
                X = Calc.Random.NextFloat(320f);
                Y = Calc.Random.NextFloat(580f);
                Duration = (float) (4.0 + Calc.Random.NextFloat() * 8.0);
                Width = Calc.Random.Next(8, 80);
                Length = Calc.Random.Next(20, 200);
            }
        }
    }
}
