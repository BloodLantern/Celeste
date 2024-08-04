using Microsoft.Xna.Framework;

namespace Monocle
{
    public struct SimpleCurve
    {
        public Vector2 Begin;
        public Vector2 End;
        public Vector2 Control;

        public SimpleCurve(Vector2 begin, Vector2 end, Vector2 control)
        {
            Begin = begin;
            End = end;
            Control = control;
        }

        public void DoubleControl() => Control += Control - (Begin + (End - Begin) / 2f);

        public Vector2 GetPoint(float percent)
        {
            float num = 1f - percent;
            return num * num * Begin + 2f * num * percent * Control + percent * percent * End;
        }

        public float GetLengthParametric(int resolution)
        {
            Vector2 vector2 = Begin;
            float lengthParametric = 0.0f;
            for (int index = 1; index <= resolution; ++index)
            {
                Vector2 point = GetPoint(index / (float) resolution);
                lengthParametric += (point - vector2).Length();
                vector2 = point;
            }
            return lengthParametric;
        }

        public void Render(Vector2 offset, Color color, int resolution)
        {
            Vector2 start = offset + Begin;
            for (int index = 1; index <= resolution; ++index)
            {
                Vector2 end = offset + GetPoint(index / (float) resolution);
                Draw.Line(start, end, color);
                start = end;
            }
        }

        public void Render(Vector2 offset, Color color, int resolution, float thickness)
        {
            Vector2 start = offset + Begin;
            for (int index = 1; index <= resolution; ++index)
            {
                Vector2 end = offset + GetPoint(index / (float) resolution);
                Draw.Line(start, end, color, thickness);
                start = end;
            }
        }

        public void Render(Color color, int resolution) => Render(Vector2.Zero, color, resolution);

        public void Render(Color color, int resolution, float thickness) => Render(Vector2.Zero, color, resolution, thickness);
    }
}
