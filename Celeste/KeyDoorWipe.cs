// Decompiled with JetBrains decompiler
// Type: Celeste.KeyDoorWipe
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
  public class KeyDoorWipe : ScreenWipe
  {
    private VertexPositionColor[] vertex = new VertexPositionColor[57];

    public KeyDoorWipe(Scene scene, bool wipeIn, Action onComplete = null)
      : base(scene, wipeIn, onComplete)
    {
      for (int index = 0; index < this.vertex.Length; ++index)
        this.vertex[index].Color = ScreenWipe.WipeColor;
    }

    public override void Render(Scene scene)
    {
      int y1 = 1090;
      int num1 = 540;
      float num2 = this.WipeIn ? 1f - this.Percent : this.Percent;
      float num3 = Ease.SineInOut(Math.Min(1f, num2 / 0.5f));
      float num4 = Ease.SineInOut(1f - Calc.Clamp((float) (((double) num2 - 0.5) / 0.30000001192092896), 0.0f, 1f));
      float num5 = num3;
      float num6 = (float) (1.0 + (1.0 - (double) num3) * 0.5);
      float x1 = 960f * num3;
      float x2 = 128f * num4 * num5;
      float y2 = 128f * num4 * num6;
      float y3 = (float) num1 - (float) num1 * 0.3f * num4 * num6;
      float y4 = (float) num1 + (float) num1 * 0.5f * num4 * num6;
      float angleRadians1 = 0.0f;
      int num7 = 0;
      VertexPositionColor[] vertex1 = this.vertex;
      int index1 = num7;
      int num8 = index1 + 1;
      vertex1[index1].Position = new Vector3(-10f, -10f, 0.0f);
      VertexPositionColor[] vertex2 = this.vertex;
      int index2 = num8;
      int num9 = index2 + 1;
      vertex2[index2].Position = new Vector3(x1, -10f, 0.0f);
      VertexPositionColor[] vertex3 = this.vertex;
      int index3 = num9;
      int num10 = index3 + 1;
      vertex3[index3].Position = new Vector3(x1, y3 - y2, 0.0f);
      for (int index4 = 1; index4 <= 8; ++index4)
      {
        float angleRadians2 = (float) (-1.5707963705062866 - (double) (index4 - 1) / 8.0 * 1.5707963705062866);
        angleRadians1 = (float) (-1.5707963705062866 - (double) index4 / 8.0 * 1.5707963705062866);
        VertexPositionColor[] vertex4 = this.vertex;
        int index5 = num10;
        int num11 = index5 + 1;
        vertex4[index5].Position = new Vector3(-10f, -10f, 0.0f);
        VertexPositionColor[] vertex5 = this.vertex;
        int index6 = num11;
        int num12 = index6 + 1;
        vertex5[index6].Position = new Vector3(new Vector2(x1, y3) + Calc.AngleToVector(angleRadians2, 1f) * new Vector2(x2, y2), 0.0f);
        VertexPositionColor[] vertex6 = this.vertex;
        int index7 = num12;
        num10 = index7 + 1;
        vertex6[index7].Position = new Vector3(new Vector2(x1, y3) + Calc.AngleToVector(angleRadians1, 1f) * new Vector2(x2, y2), 0.0f);
      }
      VertexPositionColor[] vertex7 = this.vertex;
      int index8 = num10;
      int num13 = index8 + 1;
      vertex7[index8].Position = new Vector3(-10f, -10f, 0.0f);
      VertexPositionColor[] vertex8 = this.vertex;
      int index9 = num13;
      int num14 = index9 + 1;
      vertex8[index9].Position = new Vector3(x1 - x2, y3, 0.0f);
      VertexPositionColor[] vertex9 = this.vertex;
      int index10 = num14;
      int num15 = index10 + 1;
      vertex9[index10].Position = new Vector3(-10f, (float) y1, 0.0f);
      for (int index11 = 1; index11 <= 6; ++index11)
      {
        float angleRadians3 = (float) (3.1415927410125732 - (double) (index11 - 1) / 8.0 * 1.5707963705062866);
        angleRadians1 = (float) (3.1415927410125732 - (double) index11 / 8.0 * 1.5707963705062866);
        VertexPositionColor[] vertex10 = this.vertex;
        int index12 = num15;
        int num16 = index12 + 1;
        vertex10[index12].Position = new Vector3(-10f, (float) y1, 0.0f);
        VertexPositionColor[] vertex11 = this.vertex;
        int index13 = num16;
        int num17 = index13 + 1;
        vertex11[index13].Position = new Vector3(new Vector2(x1, y3) + Calc.AngleToVector(angleRadians3, 1f) * new Vector2(x2, y2), 0.0f);
        VertexPositionColor[] vertex12 = this.vertex;
        int index14 = num17;
        num15 = index14 + 1;
        vertex12[index14].Position = new Vector3(new Vector2(x1, y3) + Calc.AngleToVector(angleRadians1, 1f) * new Vector2(x2, y2), 0.0f);
      }
      VertexPositionColor[] vertex13 = this.vertex;
      int index15 = num15;
      int num18 = index15 + 1;
      vertex13[index15].Position = new Vector3(-10f, (float) y1, 0.0f);
      VertexPositionColor[] vertex14 = this.vertex;
      int index16 = num18;
      int num19 = index16 + 1;
      vertex14[index16].Position = new Vector3(new Vector2(x1, y3) + Calc.AngleToVector(angleRadians1, 1f) * new Vector2(x2, y2), 0.0f);
      VertexPositionColor[] vertex15 = this.vertex;
      int index17 = num19;
      int num20 = index17 + 1;
      vertex15[index17].Position = new Vector3(x1 - x2 * 0.8f, y4, 0.0f);
      VertexPositionColor[] vertex16 = this.vertex;
      int index18 = num20;
      int num21 = index18 + 1;
      vertex16[index18].Position = new Vector3(-10f, (float) y1, 0.0f);
      VertexPositionColor[] vertex17 = this.vertex;
      int index19 = num21;
      int num22 = index19 + 1;
      vertex17[index19].Position = new Vector3(x1 - x2 * 0.8f, y4, 0.0f);
      VertexPositionColor[] vertex18 = this.vertex;
      int index20 = num22;
      int num23 = index20 + 1;
      vertex18[index20].Position = new Vector3(x1, y4, 0.0f);
      VertexPositionColor[] vertex19 = this.vertex;
      int index21 = num23;
      int num24 = index21 + 1;
      vertex19[index21].Position = new Vector3(-10f, (float) y1, 0.0f);
      VertexPositionColor[] vertex20 = this.vertex;
      int index22 = num24;
      int num25 = index22 + 1;
      vertex20[index22].Position = new Vector3(x1, y4, 0.0f);
      VertexPositionColor[] vertex21 = this.vertex;
      int index23 = num25;
      int num26 = index23 + 1;
      vertex21[index23].Position = new Vector3(x1, (float) y1, 0.0f);
      ScreenWipe.DrawPrimitives(this.vertex);
      for (int index24 = 0; index24 < this.vertex.Length; ++index24)
        this.vertex[index24].Position.X = 1920f - this.vertex[index24].Position.X;
      ScreenWipe.DrawPrimitives(this.vertex);
    }
  }
}
