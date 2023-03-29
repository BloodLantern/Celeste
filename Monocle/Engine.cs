// Decompiled with JetBrains decompiler
// Type: Monocle.Engine
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
    private static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
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
      this.Title = this.Window.Title = windowTitle;
      Width = width;
      Height = height;
      ClearColor = Color.Black;
      this.InactiveSleepTime = new TimeSpan(0L);
      Graphics = new GraphicsDeviceManager((Game) this);
      Graphics.DeviceReset += new EventHandler<EventArgs>(this.OnGraphicsReset);
      Graphics.DeviceCreated += new EventHandler<EventArgs>(this.OnGraphicsCreate);
      Graphics.SynchronizeWithVerticalRetrace = vsync;
      Graphics.PreferMultiSampling = false;
      Graphics.GraphicsProfile = GraphicsProfile.HiDef;
      Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
      Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
      this.Window.AllowUserResizing = true;
      this.Window.ClientSizeChanged += new EventHandler<EventArgs>(this.OnClientSizeChanged);
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
      this.Content.RootDirectory = "Content";
      this.IsMouseVisible = false;
      ExitOnEscapeKeypress = true;
      GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    protected virtual void OnClientSizeChanged(object sender, EventArgs e)
    {
      if (this.Window.ClientBounds.Width <= 0 || this.Window.ClientBounds.Height <= 0 || resizing)
        return;
      resizing = true;
      Graphics.PreferredBackBufferWidth = this.Window.ClientBounds.Width;
      Graphics.PreferredBackBufferHeight = this.Window.ClientBounds.Height;
      this.UpdateView();
      resizing = false;
    }

    protected virtual void OnGraphicsReset(object sender, EventArgs e)
    {
      this.UpdateView();
      if (this.scene != null)
        this.scene.HandleGraphicsReset();
      if (this.nextScene == null || this.nextScene == this.scene)
        return;
      this.nextScene.HandleGraphicsReset();
    }

    protected virtual void OnGraphicsCreate(object sender, EventArgs e)
    {
      this.UpdateView();
      if (this.scene != null)
        this.scene.HandleGraphicsCreate();
      if (this.nextScene == null || this.nextScene == this.scene)
        return;
      this.nextScene.HandleGraphicsCreate();
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
      base.OnActivated(sender, args);
      if (this.scene == null)
        return;
      this.scene.GainFocus();
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
      base.OnDeactivated(sender, args);
      if (this.scene == null)
        return;
      this.scene.LoseFocus();
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
      Monocle.Draw.Initialize(this.GraphicsDevice);
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
        this.Exit();
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
            if (this.scene != null)
            {
              this.scene.Tracker.GetEntity<PlayerDashAssist>()?.Update();
              if (this.scene is Level)
                (this.scene as Level).UpdateTime();
              this.scene.Entities.UpdateLists();
            }
          }
          else
            DashAssistFreeze = false;
        }
        if (!DashAssistFreeze)
        {
          if ((double) FreezeTimer > 0.0)
            FreezeTimer = Math.Max(FreezeTimer - RawDeltaTime, 0.0f);
          else if (this.scene != null)
          {
            this.scene.BeforeUpdate();
            this.scene.Update();
            this.scene.AfterUpdate();
          }
        }
        if (Commands.Open)
          Commands.UpdateOpen();
        else if (Commands.Enabled)
          Commands.UpdateClosed();
        if (this.scene != this.nextScene)
        {
          Scene scene = this.scene;
          if (this.scene != null)
            this.scene.End();
          this.scene = this.nextScene;
          this.OnSceneTransition(scene, this.nextScene);
          if (this.scene != null)
            this.scene.Begin();
        }
        base.Update(gameTime);
      }
    }

    protected override void Draw(GameTime gameTime)
    {
      this.RenderCore();
      base.Draw(gameTime);
      if (Commands.Open)
        Commands.Render();
      ++this.fpsCounter;
      this.counterElapsed += gameTime.ElapsedGameTime;
      if (!(this.counterElapsed >= TimeSpan.FromSeconds(1.0)))
        return;
      FPS = this.fpsCounter;
      this.fpsCounter = 0;
      this.counterElapsed -= TimeSpan.FromSeconds(1.0);
    }

    protected virtual void RenderCore()
    {
      if (this.scene != null)
        this.scene.BeforeRender();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
      this.GraphicsDevice.Viewport = Viewport;
      this.GraphicsDevice.Clear(ClearColor);
      if (this.scene == null)
        return;
      this.scene.Render();
      this.scene.AfterRender();
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
        this.Run();
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
      Console.WriteLine("WINDOW-" + (object) width + "x" + (object) height);
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
      float backBufferWidth = (float) this.GraphicsDevice.PresentationParameters.BackBufferWidth;
      float backBufferHeight = (float) this.GraphicsDevice.PresentationParameters.BackBufferHeight;
      if ((double) backBufferWidth / (double) Width > (double) backBufferHeight / (double) Height)
      {
        ViewWidth = (int) ((double) backBufferHeight / (double) Height * (double) Width);
        ViewHeight = (int) backBufferHeight;
      }
      else
      {
        ViewWidth = (int) backBufferWidth;
        ViewHeight = (int) ((double) backBufferWidth / (double) Width * (double) Height);
      }
      float num = (float) ViewHeight / (float) ViewWidth;
      ViewWidth -= ViewPadding * 2;
      ViewHeight -= (int) ((double) num * (double) ViewPadding * 2.0);
      ScreenMatrix = Matrix.CreateScale((float) ViewWidth / (float) Width);
      Viewport = new Viewport()
      {
        X = (int) ((double) backBufferWidth / 2.0 - (double) (ViewWidth / 2)),
        Y = (int) ((double) backBufferHeight / 2.0 - (double) (ViewHeight / 2)),
        Width = ViewWidth,
        Height = ViewHeight,
        MinDepth = 0.0f,
        MaxDepth = 1f
      };
    }
  }
}
