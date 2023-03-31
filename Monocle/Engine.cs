// Decompiled with JetBrains decompiler
// Type: Monocle.Engine
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;

namespace Monocle
{
    public class Engine : Game
    {
        public string Title;
        public System.Version Version;
        public static Action OverloadGameLoop;
        private static int viewPadding = 0;
        private static bool resizing;
        public static float TimeRate = 1f;
        public static float TimeRateB = 1f;
        public static float FreezeTimer;
        public static bool DashAssistFreeze;
        public static bool DashAssistFreezePress;
        public static int FPS;
        private TimeSpan counterElapsed = TimeSpan.Zero;
        private int fpsCounter;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static Color ClearColor;
        public static bool ExitOnEscapeKeypress;
        private readonly Scene scene;
        private Scene nextScene;
        public static Matrix ScreenMatrix;

        public static Engine Instance { get; private set; }

        public static GraphicsDeviceManager Graphics { get; private set; }

        public static Commands Commands { get; private set; }

        public static Pooler Pooler { get; private set; }

        public static int Width { get; private set; }

        public static int Height { get; private set; }

        public static int ViewWidth { get; private set; }

        public static int ViewHeight { get; private set; }

        public static int ViewPadding
        {
            get => Engine.viewPadding;
            set
            {
                Engine.viewPadding = value;
                Engine.Instance.UpdateView();
            }
        }

        public static float DeltaTime { get; private set; }

        public static float RawDeltaTime { get; private set; }

        public static ulong FrameCounter { get; private set; }

        public static string ContentDirectory => Path.Combine(Engine.AssemblyDirectory, Engine.Instance.Content.RootDirectory);

        public Engine(
            int width,
            int height,
            int windowWidth,
            int windowHeight,
            string windowTitle,
            bool fullscreen,
            bool vsync)
        {
            Engine.Instance = this;
            Title = Window.Title = windowTitle;
            Engine.Width = width;
            Engine.Height = height;
            Engine.ClearColor = Color.Black;
            InactiveSleepTime = new TimeSpan(0L);
            Engine.Graphics = new GraphicsDeviceManager((Game) this);
            Engine.Graphics.DeviceReset += new EventHandler<EventArgs>(OnGraphicsReset);
            Engine.Graphics.DeviceCreated += new EventHandler<EventArgs>(OnGraphicsCreate);
            Engine.Graphics.SynchronizeWithVerticalRetrace = vsync;
            Engine.Graphics.PreferMultiSampling = false;
            Engine.Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Engine.Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            Engine.Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(OnClientSizeChanged);
            if (fullscreen)
            {
                Engine.Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Engine.Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Engine.Graphics.IsFullScreen = true;
            }
            else
            {
                Engine.Graphics.PreferredBackBufferWidth = windowWidth;
                Engine.Graphics.PreferredBackBufferHeight = windowHeight;
                Engine.Graphics.IsFullScreen = false;
            }
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            Engine.ExitOnEscapeKeypress = true;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        protected virtual void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width <= 0 || Window.ClientBounds.Height <= 0 || Engine.resizing)
                return;
            Engine.resizing = true;
            Engine.Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            Engine.Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            UpdateView();
            Engine.resizing = false;
        }

        protected virtual void OnGraphicsReset(object sender, EventArgs e)
        {
            UpdateView();
            scene?.HandleGraphicsReset();
            if (nextScene == null || nextScene == scene)
                return;
            nextScene.HandleGraphicsReset();
        }

        protected virtual void OnGraphicsCreate(object sender, EventArgs e)
        {
            UpdateView();
            scene?.HandleGraphicsCreate();
            if (nextScene == null || nextScene == scene)
                return;
            nextScene.HandleGraphicsCreate();
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            if (scene == null)
                return;
            scene.GainFocus();
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            if (scene == null)
                return;
            scene.LoseFocus();
        }

        protected override void Initialize()
        {
            base.Initialize();
            MInput.Initialize();
            Tracker.Initialize();
            Engine.Pooler = new Pooler();
            Engine.Commands = new Commands();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            VirtualContent.Reload();
            Monocle.Draw.Initialize(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            VirtualContent.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.RawDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            Engine.DeltaTime = Engine.RawDeltaTime * Engine.TimeRate * Engine.TimeRateB;
            ++Engine.FrameCounter;
            MInput.Update();
            if (Engine.ExitOnEscapeKeypress && MInput.Keyboard.Pressed(Keys.Escape))
                Exit();
            else if (Engine.OverloadGameLoop != null)
            {
                Engine.OverloadGameLoop();
                base.Update(gameTime);
            }
            else
            {
                if (Engine.DashAssistFreeze)
                {
                    if (Celeste.Input.Dash.Check || !Engine.DashAssistFreezePress)
                    {
                        if (Celeste.Input.Dash.Check)
                            Engine.DashAssistFreezePress = true;
                        if (scene != null)
                        {
                            scene.Tracker.GetEntity<PlayerDashAssist>()?.Update();
                            if (scene is Level)
                                (scene as Level).UpdateTime();
                            scene.Entities.UpdateLists();
                        }
                    }
                    else
                        Engine.DashAssistFreeze = false;
                }
                if (!Engine.DashAssistFreeze)
                {
                    if ((double) Engine.FreezeTimer > 0.0)
                        Engine.FreezeTimer = Math.Max(Engine.FreezeTimer - Engine.RawDeltaTime, 0.0f);
                    else if (scene != null)
                    {
                        scene.BeforeUpdate();
                        scene.Update();
                        scene.AfterUpdate();
                    }
                }
                if (Engine.Commands.Open)
                    Engine.Commands.UpdateOpen();
                else if (Engine.Commands.Enabled)
                    Engine.Commands.UpdateClosed();
                if (scene != nextScene)
                {
                    Scene scene = this.scene;
                    scene?.End();
                    scene = nextScene;
                    OnSceneTransition(scene, nextScene);
                    scene?.Begin();
                }
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            RenderCore();
            base.Draw(gameTime);
            if (Engine.Commands.Open)
                Engine.Commands.Render();
            ++fpsCounter;
            counterElapsed += gameTime.ElapsedGameTime;
            if (!(counterElapsed >= TimeSpan.FromSeconds(1.0)))
                return;
            Engine.FPS = fpsCounter;
            fpsCounter = 0;
            counterElapsed -= TimeSpan.FromSeconds(1.0);
        }

        protected virtual void RenderCore()
        {
            scene?.BeforeRender();
            GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
            GraphicsDevice.Viewport = Engine.Viewport;
            GraphicsDevice.Clear(Engine.ClearColor);
            if (scene == null)
                return;
            scene.Render();
            scene.AfterRender();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            MInput.Shutdown();
        }

        public void RunWithLogging()
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ErrorLog.Write(ex);
                ErrorLog.Open();
            }
        }

        protected virtual void OnSceneTransition(Scene from, Scene to)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Engine.TimeRate = 1f;
            Engine.DashAssistFreeze = false;
        }

        public static Scene Scene
        {
            get => Engine.Instance.scene;
            set => Engine.Instance.nextScene = value;
        }

        public static Viewport Viewport { get; private set; }

        public static void SetWindowed(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;
            Engine.resizing = true;
            Engine.Graphics.PreferredBackBufferWidth = width;
            Engine.Graphics.PreferredBackBufferHeight = height;
            Engine.Graphics.IsFullScreen = false;
            Engine.Graphics.ApplyChanges();
            Console.WriteLine("WINDOW-" + (object) width + "x" + (object) height);
            Engine.resizing = false;
        }

        public static void SetFullscreen()
        {
            Engine.resizing = true;
            Engine.Graphics.PreferredBackBufferWidth = Engine.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            Engine.Graphics.PreferredBackBufferHeight = Engine.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            Engine.Graphics.IsFullScreen = true;
            Engine.Graphics.ApplyChanges();
            Console.WriteLine("FULLSCREEN");
            Engine.resizing = false;
        }

        private void UpdateView()
        {
            float backBufferWidth = (float) GraphicsDevice.PresentationParameters.BackBufferWidth;
            float backBufferHeight = (float) GraphicsDevice.PresentationParameters.BackBufferHeight;
            if ((double) backBufferWidth / (double) Engine.Width > (double) backBufferHeight / (double) Engine.Height)
            {
                Engine.ViewWidth = (int) ((double) backBufferHeight / (double) Engine.Height * (double) Engine.Width);
                Engine.ViewHeight = (int) backBufferHeight;
            }
            else
            {
                Engine.ViewWidth = (int) backBufferWidth;
                Engine.ViewHeight = (int) ((double) backBufferWidth / (double) Engine.Width * (double) Engine.Height);
            }
            float num = (float) Engine.ViewHeight / (float) Engine.ViewWidth;
            Engine.ViewWidth -= Engine.ViewPadding * 2;
            Engine.ViewHeight -= (int) ((double) num * (double) Engine.ViewPadding * 2.0);
            Engine.ScreenMatrix = Matrix.CreateScale((float) Engine.ViewWidth / (float) Engine.Width);
            Engine.Viewport = new Viewport()
            {
                X = (int) ((double) backBufferWidth / 2.0 - (double) (Engine.ViewWidth / 2)),
                Y = (int) ((double) backBufferHeight / 2.0 - (double) (Engine.ViewHeight / 2)),
                Width = Engine.ViewWidth,
                Height = Engine.ViewHeight,
                MinDepth = 0.0f,
                MaxDepth = 1f
            };
        }

        public static void ReloadGraphics(bool hires) { }
    }
}
