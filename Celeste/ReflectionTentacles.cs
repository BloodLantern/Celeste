using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class ReflectionTentacles : Entity
    {
        public int Index;
        public List<Vector2> Nodes = new List<Vector2>();
        private Vector2 outwards;
        private Vector2 lastOutwards;
        private float ease;
        private Vector2 p;
        private Player player;
        private float fearDistance;
        private float offset;
        private bool createdFromLevel;
        private int slideUntilIndex;
        private int layer;
        private const int NodesPerTentacle = 10;
        private Tentacle[] tentacles;
        private int tentacleCount;
        private VertexPositionColorTexture[] vertices;
        private int vertexCount;
        private Color color = Color.Purple;
        private float soundDelay = 0.25f;
        private List<MTexture[]> arms = new List<MTexture[]>();
        private List<MTexture> fillers;

        public ReflectionTentacles()
        {
        }

        public ReflectionTentacles(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Nodes.Add(Position);
            foreach (Vector2 node in data.Nodes)
                Nodes.Add(offset + node);
            string str = data.Attr("fear_distance");
            if (str == "close")
                fearDistance = 16f;
            else if (str == "medium")
                fearDistance = 40f;
            else if (str == "far")
                fearDistance = 80f;
            Create(fearDistance, data.Int("slide_until"), 0, Nodes);
            createdFromLevel = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!createdFromLevel)
                return;
            for (int layer = 1; layer < 4; ++layer)
            {
                ReflectionTentacles reflectionTentacles = new ReflectionTentacles();
                reflectionTentacles.Create(fearDistance, slideUntilIndex, layer, Nodes);
                scene.Add(reflectionTentacles);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = Scene.Tracker.GetEntity<Player>();
            bool flag = false;
            while (entity != null && Index < Nodes.Count - 1)
            {
                Vector2 vector2;
                p = vector2 = Calc.ClosestPointOnLine(Nodes[Index], Nodes[Index] + outwards * 10000f, entity.Center);
                vector2 = Nodes[Index] - vector2;
                if (vector2.Length() < (double) fearDistance)
                {
                    flag = true;
                    Retreat();
                }
                else
                    break;
            }
            if (!flag)
                return;
            ease = 1f;
            SnapTentacles();
        }

        public void Create(
            float fearDistance,
            int slideUntilIndex,
            int layer,
            List<Vector2> startNodes)
        {
            Nodes = new List<Vector2>();
            foreach (Vector2 startNode in startNodes)
                Nodes.Add(startNode + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-8, 8)));
            Tag = (int) Tags.TransitionUpdate;
            Position = Nodes[0];
            outwards = (Nodes[0] - Nodes[1]).SafeNormalize();
            this.fearDistance = fearDistance;
            this.slideUntilIndex = slideUntilIndex;
            this.layer = layer;
            switch (layer)
            {
                case 0:
                    Depth = -1000000;
                    color = Calc.HexToColor("3f2a4f");
                    offset = 110f;
                    break;
                case 1:
                    Depth = 8990;
                    color = Calc.HexToColor("7b3555");
                    offset = 80f;
                    break;
                case 2:
                    Depth = 10010;
                    color = Calc.HexToColor("662847");
                    offset = 50f;
                    break;
                case 3:
                    Depth = 10011;
                    color = Calc.HexToColor("492632");
                    offset = 20f;
                    break;
            }
            foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("scenery/tentacles/arms"))
            {
                MTexture[] mtextureArray = new MTexture[10];
                int width = atlasSubtexture.Width / 10;
                for (int index = 0; index < 10; ++index)
                    mtextureArray[index] = atlasSubtexture.GetSubtexture(width * (10 - index - 1), 0, width, atlasSubtexture.Height);
                arms.Add(mtextureArray);
            }
            fillers = GFX.Game.GetAtlasSubtextures("scenery/tentacles/filler");
            tentacles = new Tentacle[100];
            float along = 0.0f;
            int index1 = 0;
            while (index1 < tentacles.Length && along < 440.0)
            {
                tentacles[index1].Approach = (float) (0.25 + Calc.Random.NextFloat() * 0.75);
                tentacles[index1].Length = 32f + Calc.Random.NextFloat(64f);
                tentacles[index1].Width = 4f + Calc.Random.NextFloat(16f);
                tentacles[index1].Position = TargetTentaclePosition(tentacles[index1], Nodes[0], along);
                tentacles[index1].WaveOffset = Calc.Random.NextFloat();
                tentacles[index1].TexIndex = Calc.Random.Next(arms.Count);
                tentacles[index1].FillerTexIndex = Calc.Random.Next(fillers.Count);
                tentacles[index1].LerpDuration = 0.5f + Calc.Random.NextFloat(0.25f);
                along += tentacles[index1].Width;
                ++index1;
                ++tentacleCount;
            }
            vertices = new VertexPositionColorTexture[tentacleCount * 12 * 6];
            for (int index2 = 0; index2 < vertices.Length; ++index2)
                vertices[index2].Color = color;
        }

        private Vector2 TargetTentaclePosition(
            Tentacle tentacle,
            Vector2 position,
            float along)
        {
            Vector2 vector2_1;
            Vector2 vector2_2 = vector2_1 = position - outwards * offset;
            if (player != null)
            {
                Vector2 vector2_3 = outwards.Perpendicular();
                vector2_2 = Calc.ClosestPointOnLine(vector2_2 - vector2_3 * 200f, vector2_2 + vector2_3 * 200f, player.Position);
            }
            Vector2 vector2_4 = outwards.Perpendicular() * (float) (along - 220.0 + tentacle.Width * 0.5);
            Vector2 vector2_5 = vector2_1 + vector2_4;
            float num = (vector2_2 - vector2_5).Length();
            return vector2_5 + outwards * num * 0.6f;
        }

        public void Retreat()
        {
            if (Index >= Nodes.Count - 1)
                return;
            lastOutwards = outwards;
            ease = 0.0f;
            ++Index;
            if (layer == 0 && soundDelay <= 0.0)
                Audio.Play((Nodes[Index - 1] - Nodes[Index]).Length() > 180.0 ? "event:/game/06_reflection/scaryhair_whoosh" : "event:/game/06_reflection/scaryhair_move");
            for (int index = 0; index < tentacleCount; ++index)
            {
                tentacles[index].LerpPercent = 0.0f;
                tentacles[index].LerpPositionFrom = tentacles[index].Position;
            }
        }

        public override void Update()
        {
            soundDelay -= Engine.DeltaTime;
            if (slideUntilIndex > Index)
            {
                player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    Vector2 vector2 = p = Calc.ClosestPointOnLine(Nodes[Index] - outwards * 10000f, Nodes[Index] + outwards * 10000f, player.Center);
                    if ((vector2 - Nodes[Index]).Length() < 32.0)
                    {
                        Retreat();
                        outwards = (Nodes[Index - 1] - Nodes[Index]).SafeNormalize();
                    }
                    else
                        MoveTentacles(vector2 - outwards * 190f);
                }
            }
            else
            {
                FinalBoss entity = Scene.Tracker.GetEntity<FinalBoss>();
                player = Scene.Tracker.GetEntity<Player>();
                if (entity == null && player != null && Index < Nodes.Count - 1 && (Nodes[Index] - (p = Calc.ClosestPointOnLine(Nodes[Index], Nodes[Index] + outwards * 10000f, player.Center))).Length() < (double) fearDistance)
                    Retreat();
                if (Index > 0)
                {
                    ease = Calc.Approach(ease, 1f, (Index == Nodes.Count - 1 ? 2f : 1f) * Engine.DeltaTime);
                    outwards = Calc.AngleToVector(Calc.AngleLerp(lastOutwards.Angle(), (Nodes[Index - 1] - Nodes[Index]).Angle(), Ease.QuadOut(ease)), 1f);
                    float along = 0.0f;
                    for (int index = 0; index < tentacleCount; ++index)
                    {
                        Vector2 vector2 = TargetTentaclePosition(tentacles[index], Nodes[Index], along);
                        if (tentacles[index].LerpPercent < 1.0)
                        {
                            tentacles[index].LerpPercent += Engine.DeltaTime / tentacles[index].LerpDuration;
                            tentacles[index].Position = Vector2.Lerp(tentacles[index].LerpPositionFrom, vector2, Ease.CubeInOut(tentacles[index].LerpPercent));
                        }
                        else
                            tentacles[index].Position += (vector2 - tentacles[index].Position) * (1f - (float) Math.Pow(0.10000000149011612 * tentacles[index].Approach, Engine.DeltaTime));
                        along += tentacles[index].Width;
                    }
                }
                else
                    MoveTentacles(Nodes[Index]);
            }
            if (Index == Nodes.Count - 1)
            {
                Color color = this.color * (1f - ease);
                for (int index = 0; index < vertices.Length; ++index)
                    vertices[index].Color = color;
            }
            UpdateVertices();
        }

        private void MoveTentacles(Vector2 pos)
        {
            float along = 0.0f;
            for (int index = 0; index < tentacleCount; ++index)
            {
                Vector2 vector2 = TargetTentaclePosition(tentacles[index], pos, along);
                tentacles[index].Position += (vector2 - tentacles[index].Position) * (1f - (float) Math.Pow(0.10000000149011612 * tentacles[index].Approach, Engine.DeltaTime));
                along += tentacles[index].Width;
            }
        }

        public void SnapTentacles()
        {
            float along = 0.0f;
            for (int index = 0; index < tentacleCount; ++index)
            {
                tentacles[index].LerpPercent = 1f;
                tentacles[index].Position = TargetTentaclePosition(tentacles[index], Nodes[Index], along);
                along += tentacles[index].Width;
            }
        }

        private void UpdateVertices()
        {
            Vector2 vector2_1 = -outwards.Perpendicular();
            int n = 0;
            for (int index = 0; index < tentacleCount; ++index)
            {
                Vector2 position = tentacles[index].Position;
                Vector2 vector2_2 = vector2_1 * (float) (tentacles[index].Width * 0.5 + 2.0);
                MTexture[] arm = arms[tentacles[index].TexIndex];
                Quad(ref n, position + vector2_2, position + vector2_2 * 1.5f - outwards * 240f, position - vector2_2 * 1.5f - outwards * 240f, position - vector2_2, fillers[tentacles[index].FillerTexIndex]);
                Vector2 vector2_3 = position;
                Vector2 vector2_4 = vector2_2;
                float num1 = tentacles[index].Length / 10f + Calc.YoYo(tentacles[index].LerpPercent) * 6f;
                for (int y = 1; y <= 10; ++y)
                {
                    float num2 = y / 10f;
                    float num3 = (float) (Scene.TimeActive * (double) tentacles[index].WaveOffset * Math.Pow(1.1000000238418579, y) * 2.0);
                    float num4 = (float) (tentacles[index].WaveOffset * 3.0 + y * 0.05000000074505806);
                    float num5 = (float) (2.0 + 4.0 * num2);
                    Vector2 vector2_5 = vector2_1 * (float) Math.Sin(num3 + (double) num4) * num5;
                    Vector2 vector2_6 = vector2_3 + outwards * num1 + vector2_5;
                    Vector2 vector2_7 = vector2_2 * (1f - num2);
                    Quad(ref n, vector2_6 - vector2_7, vector2_3 - vector2_4, vector2_3 + vector2_4, vector2_6 + vector2_7, arm[y - 1]);
                    vector2_3 = vector2_6;
                    vector2_4 = vector2_7;
                }
            }
            vertexCount = n;
        }

        private void Quad(
            ref int n,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d,
            MTexture subtexture = null)
        {
            if (subtexture == null)
                subtexture = GFX.Game["util/pixel"];
            float num1 = 1f / subtexture.Texture.Texture.Width;
            float num2 = 1f / subtexture.Texture.Texture.Height;
            double x1 = subtexture.ClipRect.Left * (double) num1;
            Rectangle clipRect1 = subtexture.ClipRect;
            double y1 = clipRect1.Top * (double) num2;
            Vector2 local1 = new Vector2((float) x1, (float) y1);
            clipRect1 = subtexture.ClipRect;
            double x2 = clipRect1.Right * (double) num1;
            Rectangle clipRect2 = subtexture.ClipRect;
            double y2 = clipRect2.Top * (double) num2;
            Vector2 local2 = new Vector2((float) x2, (float) y2);
            clipRect2 = subtexture.ClipRect;
            double x3 = clipRect2.Left * (double) num1;
            Rectangle clipRect3 = subtexture.ClipRect;
            double y3 = clipRect3.Bottom * (double) num2;
            Vector2 local3 = new Vector2((float) x3, (float) y3);
            clipRect3 = subtexture.ClipRect;
            double x4 = clipRect3.Right * (double) num1;
            double y4 = subtexture.ClipRect.Bottom * (double) num2;
            Vector2 local4 = new Vector2((float) x4, (float) y4);
            vertices[n].Position = new Vector3(a, 0.0f);
            vertices[n++].TextureCoordinate = local1;
            vertices[n].Position = new Vector3(b, 0.0f);
            vertices[n++].TextureCoordinate = local2;
            vertices[n].Position = new Vector3(d, 0.0f);
            vertices[n++].TextureCoordinate = local3;
            vertices[n].Position = new Vector3(d, 0.0f);
            vertices[n++].TextureCoordinate = local3;
            vertices[n].Position = new Vector3(b, 0.0f);
            vertices[n++].TextureCoordinate = local2;
            vertices[n].Position = new Vector3(c, 0.0f);
            vertices[n++].TextureCoordinate = local4;
        }

        public override void Render()
        {
            if (vertexCount <= 0)
                return;
            GameplayRenderer.End();
            Engine.Graphics.GraphicsDevice.Textures[0] = arms[0][0].Texture.Texture;
            GFX.DrawVertices((Scene as Level).Camera.Matrix, vertices, vertexCount, GFX.FxTexture);
            GameplayRenderer.Begin();
        }

        private struct Tentacle
        {
            public Vector2 Position;
            public float Width;
            public float Length;
            public float Approach;
            public float WaveOffset;
            public int TexIndex;
            public int FillerTexIndex;
            public Vector2 LerpPositionFrom;
            public float LerpPercent;
            public float LerpDuration;
        }
    }
}
