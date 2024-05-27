using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public static class Distort
    {
        private static Vector2 anxietyOrigin;
        private static float anxiety;
        private static float gamerate = 1f;
        private static float waterSine;
        public static float WaterSineDirection = 1f;
        private static float waterCameraY;
        private static float waterAlpha = 1f;

        public static Vector2 AnxietyOrigin
        {
            get => Distort.anxietyOrigin;
            set => GFX.FxDistort.Parameters["anxietyOrigin"].SetValue(Distort.anxietyOrigin = value);
        }

        public static float Anxiety
        {
            get => Distort.anxiety;
            set
            {
                Distort.anxiety = value;
                GFX.FxDistort.Parameters["anxiety"].SetValue(!Settings.Instance.DisableFlashes ? Distort.anxiety : 0.0f);
            }
        }

        public static float GameRate
        {
            get => Distort.gamerate;
            set => GFX.FxDistort.Parameters["gamerate"].SetValue(Distort.gamerate = value);
        }

        public static float WaterSine
        {
            get => Distort.waterSine;
            set => GFX.FxDistort.Parameters["waterSine"].SetValue(Distort.waterSine = Distort.WaterSineDirection * value);
        }

        public static float WaterCameraY
        {
            get => Distort.waterCameraY;
            set => GFX.FxDistort.Parameters["waterCameraY"].SetValue(Distort.waterCameraY = value);
        }

        public static float WaterAlpha
        {
            get => Distort.waterAlpha;
            set => GFX.FxDistort.Parameters["waterAlpha"].SetValue(Distort.waterAlpha = value);
        }

        public static void Render(Texture2D source, Texture2D map, bool hasDistortion)
        {
            Effect fxDistort = GFX.FxDistort;
            if (fxDistort != null && ((Distort.anxiety > 0.0 ? 1 : (Distort.gamerate < 1.0 ? 1 : 0)) | (hasDistortion ? 1 : 0)) != 0)
            {
                fxDistort.CurrentTechnique = Distort.anxiety > 0.0 || Distort.gamerate < 1.0 ? fxDistort.Techniques[nameof (Distort)] : fxDistort.Techniques["Displace"];
                Engine.Graphics.GraphicsDevice.Textures[1] = map;
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, fxDistort);
                Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
            else
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }
    }
}
