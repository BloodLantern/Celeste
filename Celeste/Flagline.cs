using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Flagline : Component
    {
        private Color[] colors;
        private Color[] highlights;
        private Color lineColor;
        private Color pinColor;
        private Cloth[] clothes;
        private float waveTimer;
        public float ClothDroopAmount = 0.6f;
        public Vector2 To;

        public Vector2 From => Entity.Position;

        public Flagline(
            Vector2 to,
            Color lineColor,
            Color pinColor,
            Color[] colors,
            int minFlagHeight,
            int maxFlagHeight,
            int minFlagLength,
            int maxFlagLength,
            int minSpace,
            int maxSpace)
            : base(true, true)
        {
            To = to;
            this.colors = colors;
            this.lineColor = lineColor;
            this.pinColor = pinColor;
            waveTimer = Calc.Random.NextFloat() * 6.28318548f;
            highlights = new Color[colors.Length];
            for (int index = 0; index < colors.Length; ++index)
                highlights[index] = Color.Lerp(colors[index], Color.White, 0.1f);
            clothes = new Cloth[10];
            for (int index = 0; index < clothes.Length; ++index)
                clothes[index] = new Cloth
                {
                    Color = Calc.Random.Next(colors.Length),
                    Height = Calc.Random.Next(minFlagHeight, maxFlagHeight),
                    Length = Calc.Random.Next(minFlagLength, maxFlagLength),
                    Step = Calc.Random.Next(minSpace, maxSpace)
                };
        }

        public override void Update()
        {
            waveTimer += Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            Vector2 begin = From.X < (double) To.X ? From : To;
            Vector2 end = From.X < (double) To.X ? To : From;
            float num1 = (begin - end).Length();
            float num2 = num1 / 8f;
            SimpleCurve simpleCurve1 = new SimpleCurve(begin, end, (end + begin) / 2f + Vector2.UnitY * (num2 + (float) (Math.Sin(waveTimer) * num2 * 0.30000001192092896)));
            Vector2 vector2_1 = begin;
            float percent = 0.0f;
            int num3 = 0;
            bool flag = false;
            while (percent < 1.0)
            {
                Cloth clothe = clothes[num3 % clothes.Length];
                percent += (flag ? clothe.Length : (float) clothe.Step) / num1;
                Vector2 point1 = simpleCurve1.GetPoint(percent);
                Draw.Line(vector2_1, point1, lineColor);
                if (percent < 1.0 & flag)
                {
                    float num4 = clothe.Length * ClothDroopAmount;
                    SimpleCurve simpleCurve2 = new SimpleCurve(vector2_1, point1, (vector2_1 + point1) / 2f + new Vector2(0.0f, num4 + (float) (Math.Sin(waveTimer * 2.0 + percent) * num4 * 0.40000000596046448)));
                    Vector2 vector2_2 = vector2_1;
                    for (float num5 = 1f; num5 <= (double) clothe.Length; ++num5)
                    {
                        Vector2 point2 = simpleCurve2.GetPoint(num5 / clothe.Length);
                        if (point2.X != (double) vector2_2.X)
                        {
                            Draw.Rect(vector2_2.X, vector2_2.Y, (float) (point2.X - (double) vector2_2.X + 1.0), clothe.Height, colors[clothe.Color]);
                            vector2_2 = point2;
                        }
                    }
                    Draw.Rect(vector2_1.X, vector2_1.Y, 1f, clothe.Height, highlights[clothe.Color]);
                    Draw.Rect(point1.X, point1.Y, 1f, clothe.Height, highlights[clothe.Color]);
                    Draw.Rect(vector2_1.X, vector2_1.Y - 1f, 1f, 3f, pinColor);
                    Draw.Rect(point1.X, point1.Y - 1f, 1f, 3f, pinColor);
                    ++num3;
                }
                vector2_1 = point1;
                flag = !flag;
            }
        }

        private struct Cloth
        {
            public int Color;
            public int Height;
            public int Length;
            public int Step;
        }
    }
}
