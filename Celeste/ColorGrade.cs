// Decompiled with JetBrains decompiler
// Type: Celeste.ColorGrade
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public static class ColorGrade
    {
        public static bool Enabled = true;
        private static MTexture from;
        private static MTexture to;
        private static float percent = 0.0f;

        public static Effect Effect => GFX.FxColorGrading;

        public static void Set(MTexture grade)
        {
            Set(grade, grade, 0.0f);
        }

        public static void Set(MTexture fromTex, MTexture toTex, float p)
        {
            if (!Enabled || fromTex == null || toTex == null)
            {
                from = GFX.ColorGrades["none"];
                to = GFX.ColorGrades["none"];
            }
            else
            {
                from = fromTex;
                to = toTex;
            }
            percent = Calc.Clamp(p, 0f, 1f);
            if (from == to || percent <= 0)
            {
                Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture;
            }
            else if (percent >= 1)
            {
                Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = to.Texture.Texture;
            }
            else
            {
                Effect.CurrentTechnique = Effect.Techniques[nameof(ColorGrade)];
                Effect.Parameters["percent"].SetValue(percent);
                Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture;
                Engine.Graphics.GraphicsDevice.Textures[2] = to.Texture.Texture;
            }
        }

        public static float Percent
        {
            get => percent;
            set => Set(from, to, value);
        }
    }
}
