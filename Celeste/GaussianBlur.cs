using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public static class GaussianBlur
    {
        private static string[] techniques = new string[3]
        {
            "GaussianBlur3",
            "GaussianBlur5",
            "GaussianBlur9"
        };

        public static Texture2D Blur(
            Texture2D texture,
            VirtualRenderTarget temp,
            VirtualRenderTarget output,
            float fade = 0.0f,
            bool clear = true,
            Samples samples = Samples.Nine,
            float sampleScale = 1f,
            Direction direction = Direction.Both,
            float alpha = 1f)
        {
            Effect fxGaussianBlur = GFX.FxGaussianBlur;
            string technique = GaussianBlur.techniques[(int) samples];
            if (fxGaussianBlur == null)
                return texture;
            fxGaussianBlur.CurrentTechnique = fxGaussianBlur.Techniques[technique];
            fxGaussianBlur.Parameters[nameof (fade)].SetValue(fade);
            fxGaussianBlur.Parameters["pixel"].SetValue(new Vector2(1f / temp.Width, 0.0f) * sampleScale);
            Engine.Instance.GraphicsDevice.SetRenderTarget(temp);
            if (clear)
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, direction != Direction.Vertical ? fxGaussianBlur : null);
            Draw.SpriteBatch.Draw(texture, new Rectangle(0, 0, temp.Width, temp.Height), Color.White);
            Draw.SpriteBatch.End();
            fxGaussianBlur.Parameters["pixel"].SetValue(new Vector2(0.0f, 1f / output.Height) * sampleScale);
            Engine.Instance.GraphicsDevice.SetRenderTarget(output);
            if (clear)
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, direction != Direction.Horizontal ? fxGaussianBlur : null);
            Draw.SpriteBatch.Draw((RenderTarget2D) temp, new Rectangle(0, 0, output.Width, output.Height), Color.White);
            Draw.SpriteBatch.End();
            return (RenderTarget2D) output;
        }

        public enum Samples
        {
            Three,
            Five,
            Nine,
        }

        public enum Direction
        {
            Both,
            Horizontal,
            Vertical,
        }
    }
}
