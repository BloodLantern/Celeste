using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class Godrays : Backdrop
    {
        private const int RayCount = 6;
        private VertexPositionColor[] vertices = new VertexPositionColor[36];
        private int vertexCount;
        private Color rayColor = Calc.HexToColor("f52b63") * 0.5f;
        private Ray[] rays = new Ray[6];
        private float fade;

        public Godrays()
        {
            UseSpritebatch = false;
            for (int index = 0; index < rays.Length; ++index)
            {
                rays[index].Reset();
                rays[index].Percent = Calc.Random.NextFloat();
            }
        }

        public override void Update(Scene scene)
        {
            Level level = scene as Level;
            fade = Calc.Approach(fade, IsVisible(level) ? 1f : 0.0f, Engine.DeltaTime);
            Visible = fade > 0.0;
            if (!Visible)
                return;
            Player entity = level.Tracker.GetEntity<Player>();
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
                float x = Mod(rays[index1].X - level.Camera.X * 0.9f, 384f) - 32f;
                float y = Mod(rays[index1].Y - level.Camera.Y * 0.9f, 244f) - 32f;
                float width = rays[index1].Width;
                float length = rays[index1].Length;
                Vector2 vector2_2 = new Vector2((int) x, (int) y);
                Color color = rayColor * Ease.CubeInOut(Calc.Clamp((float) ((percent < 0.5 ? percent : 1.0 - percent) * 2.0), 0.0f, 1f)) * fade;
                if (entity != null)
                {
                    float num2 = (vector2_2 + level.Camera.Position - entity.Position).Length();
                    if (num2 < 64.0)
                        color *= (float) (0.25 + 0.75 * (num2 / 64.0));
                }
                VertexPositionColor vertexPositionColor1 = new VertexPositionColor(new Vector3(vector2_2 + vector2_1 * width + vector * length, 0.0f), color);
                VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector2_2 - vector2_1 * width, 0.0f), color);
                VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector2_2 + vector2_1 * width, 0.0f), color);
                VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(vector2_2 - vector2_1 * width - vector * length, 0.0f), color);
                VertexPositionColor[] vertices1 = vertices;
                int index2 = num1;
                int num3 = index2 + 1;
                VertexPositionColor vertexPositionColor5 = vertexPositionColor1;
                vertices1[index2] = vertexPositionColor5;
                VertexPositionColor[] vertices2 = vertices;
                int index3 = num3;
                int num4 = index3 + 1;
                VertexPositionColor vertexPositionColor6 = vertexPositionColor2;
                vertices2[index3] = vertexPositionColor6;
                VertexPositionColor[] vertices3 = vertices;
                int index4 = num4;
                int num5 = index4 + 1;
                VertexPositionColor vertexPositionColor7 = vertexPositionColor3;
                vertices3[index4] = vertexPositionColor7;
                VertexPositionColor[] vertices4 = vertices;
                int index5 = num5;
                int num6 = index5 + 1;
                VertexPositionColor vertexPositionColor8 = vertexPositionColor2;
                vertices4[index5] = vertexPositionColor8;
                VertexPositionColor[] vertices5 = vertices;
                int index6 = num6;
                int num7 = index6 + 1;
                VertexPositionColor vertexPositionColor9 = vertexPositionColor3;
                vertices5[index6] = vertexPositionColor9;
                VertexPositionColor[] vertices6 = vertices;
                int index7 = num7;
                num1 = index7 + 1;
                VertexPositionColor vertexPositionColor10 = vertexPositionColor4;
                vertices6[index7] = vertexPositionColor10;
            }
            vertexCount = num1;
        }

        private float Mod(float x, float m) => (x % m + m) % m;

        public override void Render(Scene scene)
        {
            if (vertexCount <= 0 || fade <= 0.0)
                return;
            GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
        }

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
                X = Calc.Random.NextFloat(384f);
                Y = Calc.Random.NextFloat(244f);
                Duration = (float) (4.0 + Calc.Random.NextFloat() * 8.0);
                Width = Calc.Random.Next(8, 16);
                Length = Calc.Random.Next(20, 40);
            }
        }
    }
}
