using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Celeste
{
    public class WaveDashPresentation : Entity
    {
        public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;
        public readonly int ScreenWidth = 1920;
        public readonly int ScreenHeight = 1080;
        private float ease;
        private bool loading;
        private float waitingForInputTime;
        private VirtualRenderTarget screenBuffer;
        private VirtualRenderTarget prevPageBuffer;
        private VirtualRenderTarget currPageBuffer;
        private int pageIndex;
        private List<WaveDashPage> pages = new List<WaveDashPage>();
        private float pageEase;
        private bool pageTurning;
        private bool pageUpdating;
        private bool waitingForPageTurn;
        private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];
        private EventInstance usingSfx;

        public bool Viewing { get; private set; }

        public Atlas Gfx { get; private set; }

        public bool ShowInput
        {
            get
            {
                if (waitingForPageTurn)
                    return true;
                return CurrPage != null && CurrPage.WaitingForInput;
            }
        }

        private WaveDashPage PrevPage => pageIndex <= 0 ? null : pages[pageIndex - 1];

        private WaveDashPage CurrPage => pageIndex >= pages.Count ? null : pages[pageIndex];

        public WaveDashPresentation(EventInstance usingSfx = null)
        {
            Tag = (int) Tags.HUD;
            Viewing = true;
            loading = true;
            Add(new Coroutine(Routine()));
            this.usingSfx = usingSfx;
            RunThread.Start(LoadingThread, "Wave Dash Presentation Loading", true);
        }

        private void LoadingThread()
        {
            Gfx = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "WaveDashing"), Atlas.AtlasDataFormat.Packer);
            loading = false;
        }

        private IEnumerator Routine()
        {
            WaveDashPresentation presentation = this;
            while (presentation.loading)
                yield return null;
            presentation.pages.Add(new WaveDashPage00());
            presentation.pages.Add(new WaveDashPage01());
            presentation.pages.Add(new WaveDashPage02());
            presentation.pages.Add(new WaveDashPage03());
            presentation.pages.Add(new WaveDashPage04());
            presentation.pages.Add(new WaveDashPage05());
            presentation.pages.Add(new WaveDashPage06());
            foreach (WaveDashPage page in presentation.pages)
                page.Added(presentation);
            presentation.Add(new BeforeRenderHook(presentation.BeforeRender));
            while (presentation.ease < 1.0)
            {
                presentation.ease = Calc.Approach(presentation.ease, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
            while (presentation.pageIndex < presentation.pages.Count)
            {
                presentation.pageUpdating = true;
                yield return presentation.CurrPage.Routine();
                if (!presentation.CurrPage.AutoProgress)
                {
                    presentation.waitingForPageTurn = true;
                    while (!Input.MenuConfirm.Pressed)
                        yield return null;
                    presentation.waitingForPageTurn = false;
                    Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
                }
                presentation.pageUpdating = false;
                ++presentation.pageIndex;
                if (presentation.pageIndex < presentation.pages.Count)
                {
                    float duration = 0.5f;
                    if (presentation.CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
                        duration = 1.5f;
                    else if (presentation.CurrPage.Transition == WaveDashPage.Transitions.Blocky)
                        duration = 1f;
                    presentation.pageTurning = true;
                    presentation.pageEase = 0.0f;
                    presentation.Add(new Coroutine(presentation.TurnPage(duration)));
                    yield return (float) (duration * 0.800000011920929);
                }
            }
            if (presentation.usingSfx != null)
            {
                Audio.SetParameter(presentation.usingSfx, "end", 1f);
                int num = (int) presentation.usingSfx.release();
            }
            Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
            while (presentation.ease > 0.0)
            {
                presentation.ease = Calc.Approach(presentation.ease, 0.0f, Engine.DeltaTime * 2f);
                yield return null;
            }
            presentation.Viewing = false;
            presentation.RemoveSelf();
        }

        private IEnumerator TurnPage(float duration)
        {
            if (CurrPage.Transition != WaveDashPage.Transitions.ScaleIn && CurrPage.Transition != WaveDashPage.Transitions.FadeIn)
            {
                if (CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
                    Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
                else if (CurrPage.Transition == WaveDashPage.Transitions.Blocky)
                    Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
                else if (CurrPage.Transition == WaveDashPage.Transitions.Spiral)
                    Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
            }
            while (pageEase < 1.0)
            {
                pageEase += Engine.DeltaTime / duration;
                yield return null;
            }
            pageTurning = false;
        }

        private void BeforeRender()
        {
            if (loading)
                return;
            if (screenBuffer == null || screenBuffer.IsDisposed)
                screenBuffer = VirtualContent.CreateRenderTarget("WaveDash-Buffer", ScreenWidth, ScreenHeight, true);
            if (prevPageBuffer == null || prevPageBuffer.IsDisposed)
                prevPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen1", ScreenWidth, ScreenHeight);
            if (currPageBuffer == null || currPageBuffer.IsDisposed)
                currPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen2", ScreenWidth, ScreenHeight);
            if (pageTurning && PrevPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
                Draw.SpriteBatch.Begin();
                PrevPage.Render();
                Draw.SpriteBatch.End();
            }
            if (CurrPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
                Draw.SpriteBatch.Begin();
                CurrPage.Render();
                Draw.SpriteBatch.End();
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            if (pageTurning)
            {
                if (CurrPage.Transition == WaveDashPage.Transitions.ScaleIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D) prevPageBuffer, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw((RenderTarget2D) currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0.0f, ScaleInPoint, Vector2.One * pageEase, SpriteEffects.None, 0.0f);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == WaveDashPage.Transitions.FadeIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D) prevPageBuffer, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw((RenderTarget2D) currPageBuffer, Vector2.Zero, Color.White * pageEase);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
                {
                    float rotation = -1.57079637f * pageEase;
                    RenderQuad((RenderTarget2D) prevPageBuffer, pageEase, rotation);
                    RenderQuad((RenderTarget2D) currPageBuffer, pageEase, 1.57079637f + rotation);
                }
                else if (CurrPage.Transition == WaveDashPage.Transitions.Blocky)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D) prevPageBuffer, Vector2.Zero, Color.White);
                    uint seed = 1;
                    int num = ScreenWidth / 60;
                    for (int x = 0; x < ScreenWidth; x += num)
                    {
                        for (int y = 0; y < ScreenHeight; y += num)
                        {
                            if (WaveDashPresentation.PseudoRandRange(ref seed, 0.0f, 1f) <= (double) pageEase)
                                Draw.SpriteBatch.Draw((RenderTarget2D) currPageBuffer, new Rectangle(x, y, num, num), new Rectangle(x, y, num, num), Color.White);
                        }
                    }
                    Draw.SpriteBatch.End();
                }
                else
                {
                    if (CurrPage.Transition != WaveDashPage.Transitions.Spiral)
                        return;
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D) prevPageBuffer, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw((RenderTarget2D) currPageBuffer, Celeste.TargetCenter, currPageBuffer.Bounds, Color.White, (float) ((1.0 - pageEase) * 12.0), Celeste.TargetCenter, Vector2.One * pageEase, SpriteEffects.None, 0.0f);
                    Draw.SpriteBatch.End();
                }
            }
            else
            {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D) currPageBuffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }

        private void RenderQuad(Texture texture, float ease, float rotation)
        {
            float num1 = screenBuffer.Width / (float) screenBuffer.Height;
            float x = num1;
            float y = 1f;
            Vector3 vector3_1 = new Vector3(-x, y, 0.0f);
            Vector3 vector3_2 = new Vector3(x, y, 0.0f);
            Vector3 vector3_3 = new Vector3(x, -y, 0.0f);
            Vector3 vector3_4 = new Vector3(-x, -y, 0.0f);
            verts[0].Position = vector3_1;
            verts[0].TextureCoordinate = new Vector2(0.0f, 0.0f);
            verts[0].Color = Color.White;
            verts[1].Position = vector3_2;
            verts[1].TextureCoordinate = new Vector2(1f, 0.0f);
            verts[1].Color = Color.White;
            verts[2].Position = vector3_3;
            verts[2].TextureCoordinate = new Vector2(1f, 1f);
            verts[2].Color = Color.White;
            verts[3].Position = vector3_1;
            verts[3].TextureCoordinate = new Vector2(0.0f, 0.0f);
            verts[3].Color = Color.White;
            verts[4].Position = vector3_3;
            verts[4].TextureCoordinate = new Vector2(1f, 1f);
            verts[4].Color = Color.White;
            verts[5].Position = vector3_4;
            verts[5].TextureCoordinate = new Vector2(0.0f, 1f);
            verts[5].Color = Color.White;
            float num2 = (float) (4.1500000953674316 + Calc.YoYo(ease) * 1.7000000476837158);
            Matrix matrix = Matrix.CreateTranslation(0.0f, 0.0f, num1) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0.0f, 0.0f, -num2) * Matrix.CreatePerspectiveFieldOfView(0.7853982f, num1, 1f, 10f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Engine.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Engine.Instance.GraphicsDevice.Textures[0] = texture;
            GFX.FxTexture.Parameters["World"].SetValue(matrix);
            foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
            }
        }

        public override void Update()
        {
            base.Update();
            if (ShowInput)
                waitingForInputTime += Engine.DeltaTime;
            else
                waitingForInputTime = 0.0f;
            if (loading || CurrPage == null || !pageUpdating)
                return;
            CurrPage.Update();
        }

        public override void Render()
        {
            if (loading || screenBuffer == null || screenBuffer.IsDisposed)
                return;
            float width = ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0.0f, 0.5f));
            float height = ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
            Rectangle rectangle = new Rectangle((int) ((1920.0 - width) / 2.0), (int) ((1080.0 - height) / 2.0), (int) width, (int) height);
            Draw.SpriteBatch.Draw((RenderTarget2D) screenBuffer, rectangle, Color.White);
            if (ShowInput && waitingForInputTime > 0.20000000298023224)
                GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + (Scene.TimeActive % 1.0 < 0.25 ? 6 : 0)), Color.Black);
            if (!(Scene as Level).Paused)
                return;
            Draw.Rect(rectangle, Color.Black * 0.7f);
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

        private void Dispose()
        {
            while (loading)
                Thread.Sleep(1);
            if (screenBuffer != null)
                screenBuffer.Dispose();
            screenBuffer = null;
            if (prevPageBuffer != null)
                prevPageBuffer.Dispose();
            prevPageBuffer = null;
            if (currPageBuffer != null)
                currPageBuffer.Dispose();
            currPageBuffer = null;
            Gfx.Dispose();
            Gfx = null;
        }

        private static uint PseudoRand(ref uint seed)
        {
            uint num1 = seed;
            uint num2 = num1 ^ num1 << 13;
            uint num3 = num2 ^ num2 >> 17;
            uint num4 = num3 ^ num3 << 5;
            seed = num4;
            return num4;
        }

        public static float PseudoRandRange(ref uint seed, float min, float max) => min + (float) (WaveDashPresentation.PseudoRand(ref seed) % 1000U / 1000.0 * (max - (double) min));
    }
}
