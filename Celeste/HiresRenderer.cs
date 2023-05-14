﻿// Decompiled with JetBrains decompiler
// Type: Celeste.HiresRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class HiresRenderer : Monocle.Renderer
    {
        public static VirtualRenderTarget Buffer => Celeste.HudTarget;

        public static bool DrawToBuffer => HiresRenderer.Buffer != null && (Engine.ViewWidth < 1920 || Engine.ViewHeight < 1080);

        public static void BeginRender(BlendState blend = null, SamplerState sampler = null)
        {
            blend ??= BlendState.AlphaBlend;
            sampler ??= SamplerState.LinearClamp;
            Matrix transformMatrix = HiresRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, transformMatrix);
        }

        public static void EndRender()
        {
            Draw.SpriteBatch.End();
        }

        public override void BeforeRender(Scene scene)
        {
            if (!HiresRenderer.DrawToBuffer)
            {
                return;
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)HiresRenderer.Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            RenderContent(scene);
        }

        public override void Render(Scene scene)
        {
            if (HiresRenderer.DrawToBuffer)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                Draw.SpriteBatch.Draw((RenderTarget2D)HiresRenderer.Buffer, new Vector2(-1f, -1f), Color.White);
                Draw.SpriteBatch.End();
            }
            else
            {
                RenderContent(scene);
            }
        }

        public virtual void RenderContent(Scene scene)
        {
        }
    }
}
