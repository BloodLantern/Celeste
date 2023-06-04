using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;

namespace Monocle
{
    public class Engine : Game
    {
        public string Title;
        public Version Version;
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
        private Scene scene;
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
            get => viewPadding;
            set
            {
                viewPadding = value;
                Instance.UpdateView();
            }
        }

        public static float DeltaTime { get; private set; }

        public static float RawDeltaTime { get; private set; }

        public static ulong FrameCounter { get; private set; }

        public static string ContentDirectory => Path.Combine(AssemblyDirectory, Instance.Content.RootDirectory);

        public Engine(
            int width,
            int height,
            int windowWidth,
            int windowHeight,
            string windowTitle,
            bool fullscreen,
            bool vsync)
        {
            Instance = this;
            Title = Window.Title = windowTitle;
            Width = width;
            Height = height;
            ClearColor = Color.Black;
            InactiveSleepTime = new TimeSpan(0L);
            Graphics = new GraphicsDeviceManager(this);
            Graphics.DeviceReset += new EventHandler<EventArgs>(OnGraphicsReset);
            Graphics.DeviceCreated += new EventHandler<EventArgs>(OnGraphicsCreate);
            Graphics.SynchronizeWithVerticalRetrace = vsync;
            Graphics.PreferMultiSampling = false;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(OnClientSizeChanged);
            if (fullscreen)
            {
                Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Graphics.IsFullScreen = true;
            }
            else
            {
                Graphics.PreferredBackBufferWidth = windowWidth;
                Graphics.PreferredBackBufferHeight = windowHeight;
                Graphics.IsFullScreen = false;
            }
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            ExitOnEscapeKeypress = true;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        protected virtual void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width <= 0 || Window.ClientBounds.Height <= 0 || resizing)
                return;
            resizing = true;
            Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            UpdateView();
            resizing = false;
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
            Pooler = new Pooler();
            Commands = new Commands();
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
            RawDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            DeltaTime = RawDeltaTime * TimeRate * TimeRateB;
            ++FrameCounter;
            MInput.Update();
            if (ExitOnEscapeKeypress && MInput.Keyboard.Pressed(Keys.Escape))
                Exit();
            else if (OverloadGameLoop != null)
            {
                OverloadGameLoop();
                base.Update(gameTime);
            }
            else
            {
                if (DashAssistFreeze)
                {
                    if (Celeste.Input.Dash.Check || !DashAssistFreezePress)
                    {
                        if (Celeste.Input.Dash.Check)
                            DashAssistFreezePress = true;
                        if (scene != null)
                        {
                            scene.Tracker.GetEntity<PlayerDashAssist>()?.Update();
                            if (scene is Level)
                                (scene as Level).UpdateTime();
                            scene.Entities.UpdateLists();
                        }
                    }
                    else
                        DashAssistFreeze = false;
                }
                if (!DashAssistFreeze)
                {
                    if (FreezeTimer > 0.0)
                        FreezeTimer = Math.Max(FreezeTimer - RawDeltaTime, 0.0f);
                    else if (scene != null)
                    {
                        scene.BeforeUpdate();
                        scene.Update();
                        scene.AfterUpdate();
                    }
                }
                if (Commands.Open)
                    Commands.UpdateOpen();
                else if (Commands.Enabled)
                    Commands.UpdateClosed();
                if (scene != nextScene)
                {
                    Scene scene = this.scene;
                    this.scene?.End();
                    this.scene = nextScene;
                    OnSceneTransition(scene, nextScene);
                    this.scene?.Begin();
                }
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            RenderCore();
            base.Draw(gameTime);
            if (Commands.Open)
                Commands.Render();
            ++fpsCounter;
            counterElapsed += gameTime.ElapsedGameTime;
            if (!(counterElapsed >= TimeSpan.FromSeconds(1.0)))
                return;
            FPS = fpsCounter;
            fpsCounter = 0;
            counterElapsed -= TimeSpan.FromSeconds(1.0);
        }

        protected virtual void RenderCore()
        {
            scene?.BeforeRender();
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = Viewport;
            GraphicsDevice.Clear(ClearColor);
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
            TimeRate = 1f;
            DashAssistFreeze = false;
        }

        public static Scene Scene
        {
            get => Instance.scene;
            set => Instance.nextScene = value;
        }

        public static Viewport Viewport { get; private set; }

        public static void SetWindowed(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;
            resizing = true;
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();
            Console.WriteLine("WINDOW-" + width + "x" + height);
            resizing = false;
        }

        public static void SetFullscreen()
        {
            resizing = true;
            Graphics.PreferredBackBufferWidth = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            Graphics.IsFullScreen = true;
            Graphics.ApplyChanges();
            Console.WriteLine("FULLSCREEN");
            resizing = false;
        }

        private void UpdateView()
        {
            float backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            float backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            if ((double) backBufferWidth / Width > (double) backBufferHeight / Height)
            {
                ViewWidth = (int) ((double) backBufferHeight / Height * Width);
                ViewHeight = (int) backBufferHeight;
            }
            else
            {
                ViewWidth = (int) backBufferWidth;
                ViewHeight = (int) ((double) backBufferWidth / Width * Height);
            }
            float num = ViewHeight / (float) ViewWidth;
            ViewWidth -= ViewPadding * 2;
            ViewHeight -= (int) ((double) num * ViewPadding * 2.0);
            ScreenMatrix = Matrix.CreateScale(ViewWidth / (float) Width);
            Viewport = new Viewport()
            {
                X = (int) ((double) backBufferWidth / 2.0 - ViewWidth / 2),
                Y = (int) ((double) backBufferHeight / 2.0 - ViewHeight / 2),
                Width = ViewWidth,
                Height = ViewHeight,
                MinDepth = 0.0f,
                MaxDepth = 1f
            };
        }

        public static void ReloadGraphics(bool hires) { }
    }
}
