// Decompiled with JetBrains decompiler
// Type: Celeste.DustEdges
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class DustEdges : Entity
    {
        public static int DustGraphicEstabledCounter;
        private bool hasDust;
        private float noiseEase;
        private Vector2 noiseFromPos;
        private Vector2 noiseToPos;
        private VirtualTexture DustNoiseFrom;
        private VirtualTexture DustNoiseTo;

        public DustEdges()
        {
            AddTag((int)Tags.Global | (int)Tags.TransitionUpdate);
            Depth = -48;
            Add(new BeforeRenderHook(new Action(BeforeRender)));
        }

        private void CreateTextures()
        {
            DustNoiseFrom = VirtualContent.CreateTexture("dust-noise-a", 128, 72, Color.White);
            DustNoiseTo = VirtualContent.CreateTexture("dust-noise-b", 128, 72, Color.White);
            Color[] data = new Color[DustNoiseFrom.Width * DustNoiseTo.Height];
            for (int index = 0; index < data.Length; ++index)
            {
                data[index] = new Color(Calc.Random.NextFloat(), 0.0f, 0.0f, 0.0f);
            }

            DustNoiseFrom.Texture.SetData<Color>(data);
            for (int index = 0; index < data.Length; ++index)
            {
                data[index] = new Color(Calc.Random.NextFloat(), 0.0f, 0.0f, 0.0f);
            }

            DustNoiseTo.Texture.SetData<Color>(data);
        }

        public override void Update()
        {
            noiseEase = Calc.Approach(noiseEase, 1f, Engine.DeltaTime);
            if (noiseEase == 1.0)
            {
                (DustNoiseTo, DustNoiseFrom) = (DustNoiseFrom, DustNoiseTo);
                noiseFromPos = noiseToPos;
                noiseToPos = new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat());
                noiseEase = 0.0f;
            }
            DustEdges.DustGraphicEstabledCounter = 0;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            Dispose();
        }

        private void Dispose()
        {
            DustNoiseFrom?.Dispose();
            if (DustNoiseTo == null)
            {
                return;
            }

            DustNoiseTo.Dispose();
        }

        public void BeforeRender()
        {
            List<Component> components = Scene.Tracker.GetComponents<DustEdge>();
            hasDust = components.Count > 0;
            if (!hasDust)
            {
                return;
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)GameplayBuffers.TempA);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, (Scene as Level).Camera.Matrix);
            foreach (Component component in components)
            {
                DustEdge dustEdge = component as DustEdge;
                if (dustEdge.Visible && dustEdge.Entity.Visible)
                {
                    dustEdge.RenderDust();
                }
            }
            Draw.SpriteBatch.End();
            if (DustNoiseFrom == null || DustNoiseFrom.IsDisposed)
            {
                CreateTextures();
            }

            Vector2 vector2 = FlooredCamera();
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)GameplayBuffers.ResortDust);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.Textures[1] = DustNoiseFrom.Texture;
            Engine.Graphics.GraphicsDevice.Textures[2] = DustNoiseTo.Texture;
            GFX.FxDust.Parameters["colors"].SetValue(DustStyles.Get(Scene).EdgeColors);
            GFX.FxDust.Parameters["noiseEase"].SetValue(noiseEase);
            GFX.FxDust.Parameters["noiseFromPos"].SetValue(noiseFromPos + new Vector2(vector2.X / 320f, vector2.Y / 180f));
            GFX.FxDust.Parameters["noiseToPos"].SetValue(noiseToPos + new Vector2(vector2.X / 320f, vector2.Y / 180f));
            GFX.FxDust.Parameters["pixel"].SetValue(new Vector2(1f / 320f, 0.00555555569f));
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDust, Matrix.Identity);
            Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.TempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            if (!hasDust)
            {
                return;
            }

            Vector2 position = FlooredCamera();
            Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.ResortDust, position, Color.White);
        }

        private Vector2 FlooredCamera()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            position.X = (int)Math.Floor(position.X);
            position.Y = (int)Math.Floor(position.Y);
            return position;
        }
    }
}
