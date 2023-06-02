using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class HeartWipe : ScreenWipe
    {
        private VertexPositionColor[] vertex = new VertexPositionColor[111];

        public HeartWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int index = 0; index < this.vertex.Length; ++index)
                this.vertex[index].Color = ScreenWipe.WipeColor;
        }

        public override void Render(Scene scene)
        {
            float num1 = (float) (((this.WipeIn ? (double) this.Percent : 1.0 - (double) this.Percent) - 0.20000000298023224) / 0.800000011920929);
            if ((double) num1 <= 0.0)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, (DepthStencilState) null, (RasterizerState) null, (Effect) null, Engine.ScreenMatrix);
                Draw.Rect(-1f, -1f, (float) (Engine.Width + 2), (float) (Engine.Height + 2), ScreenWipe.WipeColor);
                Draw.SpriteBatch.End();
            }
            else
            {
                Vector2 vector2_1 = new Vector2((float) Engine.Width, (float) Engine.Height) / 2f;
                float length = (float) Engine.Width * 0.75f * num1;
                float num2 = (float) Engine.Width * num1;
                float d = -0.25f;
                float num3 = -1.57079637f;
                Vector2 vector2_2 = vector2_1 + new Vector2((float) -Math.Cos((double) d) * length, (float) (-(double) length / 2.0));
                int num4 = 0;
                for (int index1 = 1; index1 <= 16; ++index1)
                {
                    float angleRadians1 = d + (float) (((double) num3 - (double) d) * ((double) (index1 - 1) / 16.0));
                    float angleRadians2 = d + (float) (((double) num3 - (double) d) * ((double) index1 / 16.0));
                    VertexPositionColor[] vertex1 = this.vertex;
                    int index2 = num4;
                    int num5 = index2 + 1;
                    vertex1[index2].Position = new Vector3(vector2_1.X, -num2, 0.0f);
                    VertexPositionColor[] vertex2 = this.vertex;
                    int index3 = num5;
                    int num6 = index3 + 1;
                    vertex2[index3].Position = new Vector3(vector2_2 + Calc.AngleToVector(angleRadians1, length), 0.0f);
                    VertexPositionColor[] vertex3 = this.vertex;
                    int index4 = num6;
                    num4 = index4 + 1;
                    vertex3[index4].Position = new Vector3(vector2_2 + Calc.AngleToVector(angleRadians2, length), 0.0f);
                }
                VertexPositionColor[] vertex4 = this.vertex;
                int index5 = num4;
                int num7 = index5 + 1;
                vertex4[index5].Position = new Vector3(vector2_1.X, -num2, 0.0f);
                VertexPositionColor[] vertex5 = this.vertex;
                int index6 = num7;
                int num8 = index6 + 1;
                vertex5[index6].Position = new Vector3(vector2_2 + new Vector2(0.0f, -length), 0.0f);
                VertexPositionColor[] vertex6 = this.vertex;
                int index7 = num8;
                int num9 = index7 + 1;
                vertex6[index7].Position = new Vector3(-num2, -num2, 0.0f);
                VertexPositionColor[] vertex7 = this.vertex;
                int index8 = num9;
                int num10 = index8 + 1;
                vertex7[index8].Position = new Vector3(-num2, -num2, 0.0f);
                VertexPositionColor[] vertex8 = this.vertex;
                int index9 = num10;
                int num11 = index9 + 1;
                vertex8[index9].Position = new Vector3(vector2_2 + new Vector2(0.0f, -length), 0.0f);
                VertexPositionColor[] vertex9 = this.vertex;
                int index10 = num11;
                int num12 = index10 + 1;
                vertex9[index10].Position = new Vector3(-num2, vector2_2.Y, 0.0f);
                float num13 = 2.3561945f;
                for (int index11 = 1; index11 <= 16; ++index11)
                {
                    float angleRadians3 = (float) (-1.5707963705062866 - (double) (index11 - 1) / 16.0 * (double) num13);
                    float angleRadians4 = (float) (-1.5707963705062866 - (double) index11 / 16.0 * (double) num13);
                    VertexPositionColor[] vertex10 = this.vertex;
                    int index12 = num12;
                    int num14 = index12 + 1;
                    vertex10[index12].Position = new Vector3(-num2, vector2_2.Y, 0.0f);
                    VertexPositionColor[] vertex11 = this.vertex;
                    int index13 = num14;
                    int num15 = index13 + 1;
                    vertex11[index13].Position = new Vector3(vector2_2 + Calc.AngleToVector(angleRadians3, length), 0.0f);
                    VertexPositionColor[] vertex12 = this.vertex;
                    int index14 = num15;
                    num12 = index14 + 1;
                    vertex12[index14].Position = new Vector3(vector2_2 + Calc.AngleToVector(angleRadians4, length), 0.0f);
                }
                Vector2 vector2_3 = vector2_2 + Calc.AngleToVector(-1.57079637f - num13, length);
                Vector2 vector2_4 = vector2_1 + new Vector2(0.0f, length * 1.8f);
                VertexPositionColor[] vertex13 = this.vertex;
                int index15 = num12;
                int num16 = index15 + 1;
                vertex13[index15].Position = new Vector3(-num2, vector2_2.Y, 0.0f);
                VertexPositionColor[] vertex14 = this.vertex;
                int index16 = num16;
                int num17 = index16 + 1;
                vertex14[index16].Position = new Vector3(vector2_3, 0.0f);
                VertexPositionColor[] vertex15 = this.vertex;
                int index17 = num17;
                int num18 = index17 + 1;
                vertex15[index17].Position = new Vector3(-num2, (float) Engine.Height + num2, 0.0f);
                VertexPositionColor[] vertex16 = this.vertex;
                int index18 = num18;
                int num19 = index18 + 1;
                vertex16[index18].Position = new Vector3(-num2, (float) Engine.Height + num2, 0.0f);
                VertexPositionColor[] vertex17 = this.vertex;
                int index19 = num19;
                int num20 = index19 + 1;
                vertex17[index19].Position = new Vector3(vector2_3, 0.0f);
                VertexPositionColor[] vertex18 = this.vertex;
                int index20 = num20;
                int num21 = index20 + 1;
                vertex18[index20].Position = new Vector3(vector2_4, 0.0f);
                VertexPositionColor[] vertex19 = this.vertex;
                int index21 = num21;
                int num22 = index21 + 1;
                vertex19[index21].Position = new Vector3(-num2, (float) Engine.Height + num2, 0.0f);
                VertexPositionColor[] vertex20 = this.vertex;
                int index22 = num22;
                int num23 = index22 + 1;
                vertex20[index22].Position = new Vector3(vector2_4, 0.0f);
                VertexPositionColor[] vertex21 = this.vertex;
                int index23 = num23;
                int num24 = index23 + 1;
                vertex21[index23].Position = new Vector3(vector2_1.X, (float) Engine.Height + num2, 0.0f);
                ScreenWipe.DrawPrimitives(this.vertex);
                for (int index24 = 0; index24 < this.vertex.Length; ++index24)
                    this.vertex[index24].Position.X = 1920f - this.vertex[index24].Position.X;
                ScreenWipe.DrawPrimitives(this.vertex);
            }
        }
    }
}
