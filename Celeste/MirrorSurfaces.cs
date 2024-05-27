using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class MirrorSurfaces : Entity
    {
        public const int MaxMirrorOffset = 32;
        private bool hasReflections;
        private VirtualRenderTarget target;

        public MirrorSurfaces()
        {
            Depth = 9490;
            Tag = (int) Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
        }

        public void BeforeRender()
        {
            Level scene = Scene as Level;
            List<Component> components1 = Scene.Tracker.GetComponents<MirrorReflection>();
            List<Component> components2 = Scene.Tracker.GetComponents<MirrorSurface>();
            if (!(hasReflections = components2.Count > 0 && components1.Count > 0))
                return;
            if (target == null)
                target = VirtualContent.CreateRenderTarget("mirror-surfaces", 320, 180);
            Matrix transformMatrix = Matrix.CreateTranslation(32f, 32f, 0.0f) * scene.Camera.Matrix;
            components1.Sort((a, b) => b.Entity.Depth - a.Entity.Depth);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.MirrorSources);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, transformMatrix);
            foreach (MirrorReflection mirrorReflection in components1)
            {
                if ((mirrorReflection.Entity.Visible || mirrorReflection.IgnoreEntityVisible) && mirrorReflection.Visible)
                {
                    mirrorReflection.IsRendering = true;
                    mirrorReflection.Entity.Render();
                    mirrorReflection.IsRendering = false;
                }
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.MirrorMasks);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, transformMatrix);
            foreach (MirrorSurface mirrorSurface in components2)
            {
                if (mirrorSurface.Visible && mirrorSurface.OnRender != null)
                    mirrorSurface.OnRender();
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.Textures[1] = (RenderTarget2D) GameplayBuffers.MirrorSources;
            GFX.FxMirrors.Parameters["pixel"].SetValue(new Vector2(1f / GameplayBuffers.MirrorMasks.Width, 1f / GameplayBuffers.MirrorMasks.Height));
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, GFX.FxMirrors, Matrix.Identity);
            Draw.SpriteBatch.Draw((RenderTarget2D) GameplayBuffers.MirrorMasks, new Vector2(-32f, -32f), Color.White);
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            if (!hasReflections)
                return;
            Draw.SpriteBatch.Draw((RenderTarget2D) target, FlooredCamera(), Color.White * 0.5f);
        }

        private Vector2 FlooredCamera()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            position.X = (int) Math.Floor(position.X);
            position.Y = (int) Math.Floor(position.Y);
            return position;
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

        public void Dispose()
        {
            if (target != null && !target.IsDisposed)
                target.Dispose();
            target = null;
        }
    }
}
