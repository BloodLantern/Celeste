using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class GlassBlockBg : Entity
    {
        private static readonly Color[] starColors = new Color[3]
        {
            Calc.HexToColor("7f9fba"),
            Calc.HexToColor("9bd1cd"),
            Calc.HexToColor("bacae3")
        };
        private const int StarCount = 100;
        private const int RayCount = 50;
        private Star[] stars = new Star[100];
        private Ray[] rays = new Ray[50];
        private VertexPositionColor[] verts = new VertexPositionColor[2700];
        private Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();
        private Color bgColor = Calc.HexToColor("0d2e89");
        private VirtualRenderTarget beamsTarget;
        private VirtualRenderTarget starsTarget;
        private bool hasBlocks;

        public GlassBlockBg()
        {
            Tag = (int) Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
            Add(new DisplacementRenderHook(OnDisplacementRender));
            Depth = -9990;
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("particles/stars/");
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index].Position.X = Calc.Random.Next(320);
                stars[index].Position.Y = Calc.Random.Next(180);
                stars[index].Texture = Calc.Random.Choose(atlasSubtextures);
                stars[index].Color = Calc.Random.Choose(GlassBlockBg.starColors);
                stars[index].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
            }
            for (int index = 0; index < rays.Length; ++index)
            {
                rays[index].Position.X = Calc.Random.Next(320);
                rays[index].Position.Y = Calc.Random.Next(180);
                rays[index].Width = Calc.Random.Range(4f, 16f);
                rays[index].Length = Calc.Random.Choose(48, 96, 128);
                rays[index].Color = Color.White * Calc.Random.Range(0.2f, 0.4f);
            }
        }

        private void BeforeRender()
        {
            if (!(hasBlocks = Scene.Tracker.GetEntities<GlassBlock>().Count > 0))
                return;
            Camera camera = (Scene as Level).Camera;
            int num1 = 320;
            int num2 = 180;
            if (starsTarget == null)
                starsTarget = VirtualContent.CreateRenderTarget("glass-block-surfaces", 320, 180);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Vector2 origin = new Vector2(8f, 8f);
            for (int index = 0; index < stars.Length; ++index)
            {
                MTexture texture = stars[index].Texture;
                Color color = stars[index].Color;
                Vector2 scroll = stars[index].Scroll;
                Vector2 position = new Vector2();
                position.X = Mod(stars[index].Position.X - camera.X * (1f - scroll.X), num1);
                position.Y = Mod(stars[index].Position.Y - camera.Y * (1f - scroll.Y), num2);
                texture.Draw(position, origin, color);
                if (position.X < (double) origin.X)
                    texture.Draw(position + new Vector2(num1, 0.0f), origin, color);
                else if (position.X > num1 - (double) origin.X)
                    texture.Draw(position - new Vector2(num1, 0.0f), origin, color);
                if (position.Y < (double) origin.Y)
                    texture.Draw(position + new Vector2(0.0f, num2), origin, color);
                else if (position.Y > num2 - (double) origin.Y)
                    texture.Draw(position - new Vector2(0.0f, num2), origin, color);
            }
            Draw.SpriteBatch.End();
            int vertex = 0;
            for (int index = 0; index < rays.Length; ++index)
            {
                Vector2 position = new Vector2();
                position.X = Mod(rays[index].Position.X - camera.X * 0.9f, num1);
                position.Y = Mod(rays[index].Position.Y - camera.Y * 0.9f, num2);
                DrawRay(position, ref vertex, ref rays[index]);
                if (position.X < 64.0)
                    DrawRay(position + new Vector2(num1, 0.0f), ref vertex, ref rays[index]);
                else if (position.X > (double) (num1 - 64))
                    DrawRay(position - new Vector2(num1, 0.0f), ref vertex, ref rays[index]);
                if (position.Y < 64.0)
                    DrawRay(position + new Vector2(0.0f, num2), ref vertex, ref rays[index]);
                else if (position.Y > (double) (num2 - 64))
                    DrawRay(position - new Vector2(0.0f, num2), ref vertex, ref rays[index]);
            }
            if (beamsTarget == null)
                beamsTarget = VirtualContent.CreateRenderTarget("glass-block-beams", 320, 180);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GFX.DrawVertices(Matrix.Identity, verts, vertex);
        }

        private void OnDisplacementRender()
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<GlassBlock>())
                Draw.Rect(entity.X, entity.Y, entity.Width, entity.Height, new Color(0.5f, 0.5f, 0.2f, 1f));
        }

        private void DrawRay(Vector2 position, ref int vertex, ref Ray ray)
        {
            Vector2 vector2_1 = new Vector2(-rayNormal.Y, rayNormal.X);
            Vector2 vector2_2 = rayNormal * ray.Width * 0.5f;
            Vector2 vector2_3 = vector2_1 * ray.Length * 0.25f * 0.5f;
            Vector2 vector2_4 = vector2_1 * ray.Length * 0.5f * 0.5f;
            Vector2 v0 = position + vector2_2 - vector2_3 - vector2_4;
            Vector2 v3 = position - vector2_2 - vector2_3 - vector2_4;
            Vector2 vector2_5 = position + vector2_2 - vector2_3;
            Vector2 vector2_6 = position - vector2_2 - vector2_3;
            Vector2 vector2_7 = position + vector2_2 + vector2_3;
            Vector2 vector2_8 = position - vector2_2 + vector2_3;
            Vector2 v1 = position + vector2_2 + vector2_3 + vector2_4;
            Vector2 v2 = position - vector2_2 + vector2_3 + vector2_4;
            Color transparent = Color.Transparent;
            Color color = ray.Color;
            Quad(ref vertex, v0, vector2_5, vector2_6, v3, transparent, color, color, transparent);
            Quad(ref vertex, vector2_5, vector2_7, vector2_8, vector2_6, color, color, color, color);
            Quad(ref vertex, vector2_7, v1, v2, vector2_8, color, transparent, transparent, color);
        }

        private void Quad(
            ref int vertex,
            Vector2 v0,
            Vector2 v1,
            Vector2 v2,
            Vector2 v3,
            Color c0,
            Color c1,
            Color c2,
            Color c3)
        {
            verts[vertex].Position.X = v0.X;
            verts[vertex].Position.Y = v0.Y;
            verts[vertex++].Color = c0;
            verts[vertex].Position.X = v1.X;
            verts[vertex].Position.Y = v1.Y;
            verts[vertex++].Color = c1;
            verts[vertex].Position.X = v2.X;
            verts[vertex].Position.Y = v2.Y;
            verts[vertex++].Color = c2;
            verts[vertex].Position.X = v0.X;
            verts[vertex].Position.Y = v0.Y;
            verts[vertex++].Color = c0;
            verts[vertex].Position.X = v2.X;
            verts[vertex].Position.Y = v2.Y;
            verts[vertex++].Color = c2;
            verts[vertex].Position.X = v3.X;
            verts[vertex].Position.Y = v3.Y;
            verts[vertex++].Color = c3;
        }

        public override void Render()
        {
            if (!hasBlocks)
                return;
            Vector2 position = (Scene as Level).Camera.Position;
            List<Entity> entities = Scene.Tracker.GetEntities<GlassBlock>();
            foreach (Entity entity in entities)
                Draw.Rect(entity.X, entity.Y, entity.Width, entity.Height, bgColor);
            if (starsTarget != null && !starsTarget.IsDisposed)
            {
                foreach (Entity entity in entities)
                {
                    Rectangle rectangle = new Rectangle((int) (entity.X - (double) position.X), (int) (entity.Y - (double) position.Y), (int) entity.Width, (int) entity.Height);
                    Draw.SpriteBatch.Draw((RenderTarget2D) starsTarget, entity.Position, rectangle, Color.White);
                }
            }
            if (beamsTarget == null || beamsTarget.IsDisposed)
                return;
            foreach (Entity entity in entities)
            {
                Rectangle rectangle = new Rectangle((int) (entity.X - (double) position.X), (int) (entity.Y - (double) position.Y), (int) entity.Width, (int) entity.Height);
                Draw.SpriteBatch.Draw((RenderTarget2D) beamsTarget, entity.Position, rectangle, Color.White);
            }
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
            if (starsTarget != null && !starsTarget.IsDisposed)
                starsTarget.Dispose();
            if (beamsTarget != null && !beamsTarget.IsDisposed)
                beamsTarget.Dispose();
            starsTarget = null;
            beamsTarget = null;
        }

        private float Mod(float x, float m) => (x % m + m) % m;

        private struct Star
        {
            public Vector2 Position;
            public MTexture Texture;
            public Color Color;
            public Vector2 Scroll;
        }

        private struct Ray
        {
            public Vector2 Position;
            public float Width;
            public float Length;
            public Color Color;
        }
    }
}
