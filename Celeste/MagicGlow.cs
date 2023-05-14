// Decompiled with JetBrains decompiler
// Type: Celeste.MagicGlow
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public static class MagicGlow
    {
        public static void Render(Texture2D texture, float noiseEase, float direction, Matrix matrix)
        {
            GFX.FxMagicGlow.Parameters["alpha"].SetValue(0.5f);
            GFX.FxMagicGlow.Parameters["pixel"].SetValue(new Vector2(1f / texture.Width, 1f / texture.Height) * 3f);
            GFX.FxMagicGlow.Parameters["noiseSample"].SetValue(new Vector2(1f, 0.5f));
            GFX.FxMagicGlow.Parameters["noiseDistort"].SetValue(new Vector2(1f, 1f));
            GFX.FxMagicGlow.Parameters[nameof(noiseEase)].SetValue(noiseEase * 0.05f);
            GFX.FxMagicGlow.Parameters[nameof(direction)].SetValue(-direction);
            Engine.Graphics.GraphicsDevice.Textures[1] = GFX.MagicGlowNoise.Texture;
            Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, GFX.FxMagicGlow, matrix);
            Draw.SpriteBatch.Draw(texture, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}
