// Decompiled with JetBrains decompiler
// Type: Celeste.AreaCompleteTitle
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class AreaCompleteTitle : Entity
    {
        public float Alpha = 1f;
        private Vector2 origin;
        private readonly List<Letter> letters = new();
        private float rectangleEase;
        private readonly float scale;

        public AreaCompleteTitle(Vector2 origin, string text, float scale, bool rainbow = false)
        {
            this.origin = origin;
            this.scale = scale;
            Vector2 vector2_1 = ActiveFont.Measure(text) * scale;
            Vector2 vector2_2 = origin + (Vector2.UnitY * vector2_1.Y * 0.5f) + (Vector2.UnitX * vector2_1.X * -0.5f);
            for (int index1 = 0; index1 < text.Length; ++index1)
            {
                char ch = text[index1];
                Vector2 vector2_3 = ActiveFont.Measure(ch.ToString()) * scale;
                if (text[index1] != ' ')
                {
                    int index2 = index1;
                    ch = text[index1];
                    string str = ch.ToString();
                    Vector2 position = vector2_2 + (Vector2.UnitX * vector2_3.X * 0.5f);
                    Letter letter = new(index2, str, position);
                    if (rainbow)
                    {
                        float hue = index1 / (float)text.Length;
                        letter.Color = Calc.HsvToColor(hue, 0.8f, 0.9f);
                        letter.Shadow = Color.Lerp(letter.Color, Color.Black, 0.7f);
                    }
                    letters.Add(letter);
                }
                vector2_2 += Vector2.UnitX * vector2_3.X;
            }
            _ = Alarm.Set(this, 2.6f, () =>
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, true);
                tween.OnUpdate = t => rectangleEase = t.Eased;
                Add(tween);
            });
        }

        public override void Update()
        {
            base.Update();
            foreach (Letter letter in letters)
            {
                letter.Update();
            }
        }

        public void DrawLineUI()
        {
            Draw.Rect(X, Y + origin.Y - 40f, 1920f * rectangleEase, 80f, Color.Black * 0.65f);
        }

        public override void Render()
        {
            base.Render();
            foreach (Letter letter in letters)
            {
                letter.Render(Position, scale, Alpha);
            }
        }

        public class Letter
        {
            public string Value;
            public Vector2 Position;
            public Color Color = Color.White;
            public Color Shadow = Color.Black;
            private float delay;
            private float ease;
            private Vector2 scale;
            private SimpleCurve curve;

            public Letter(int index, string value, Vector2 position)
            {
                Value = value;
                Position = position;
                delay = 0.2f + (index * 0.2f);
                curve = new SimpleCurve(position + (Vector2.UnitY * 60f), position, position - (Vector2.UnitY * 100f));
                scale = new Vector2(0.75f, 1.5f);
            }

            public void Update()
            {
                scale.X = Calc.Approach(scale.X, 1f, 3f * Engine.DeltaTime);
                scale.Y = Calc.Approach(scale.Y, 1f, 3f * Engine.DeltaTime);
                if (delay > 0.0)
                {
                    delay -= Engine.DeltaTime;
                }
                else
                {
                    if (ease >= 1.0)
                    {
                        return;
                    }

                    ease += 4f * Engine.DeltaTime;
                    if (ease < 1.0)
                    {
                        return;
                    }

                    ease = 1f;
                    scale = new Vector2(1.5f, 0.75f);
                }
            }

            public void Render(Vector2 offset, float scale, float alphaMultiplier)
            {
                if (ease <= 0)
                {
                    return;
                }

                Vector2 position = offset + curve.GetPoint(ease);
                float num = Calc.LerpClamp(0.0f, 1f, ease * 3f) * alphaMultiplier;
                Vector2 scale1 = this.scale * scale;
                if (num < 1)
                {
                    ActiveFont.Draw(Value, position, new Vector2(0.5f, 1f), scale1, Color * num);
                }
                else
                {
                    ActiveFont.Draw(Value, position + (Vector2.UnitY * 3.5f * scale), new Vector2(0.5f, 1f), scale1, Shadow);
                    ActiveFont.DrawOutline(Value, position, new Vector2(0.5f, 1f), scale1, Color, 2f, Shadow);
                }
            }
        }
    }
}
