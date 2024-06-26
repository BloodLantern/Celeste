﻿using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;
#if ENABLE_STEAM
using Steamworks;
#endif
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Celeste
{
    public class Celeste : Engine
    {
        public enum PlayModes
        {
            Normal,
            Debug,
            Event,
            Demo,
        }

        public const int GameWidth = 320;
        public const int GameHeight = 180;
        public const int TargetWidth = 1920;
        public const int TargetHeight = 1080;
        public static PlayModes PlayMode = PlayModes.Normal;
        public const string EventName = "";
        public const bool Beta = false;
        public const string PLATFORM = "PC";
        public static new Celeste Instance;
        public static VirtualRenderTarget HudTarget;
        public static VirtualRenderTarget WipeTarget;
        public static DisconnectedControllerUI DisconnectUI;
        private bool firstLoad = true;
        public AutoSplitterInfo AutoSplitterInfo = new();
        public static Coroutine SaveRoutine;
        public static Stopwatch LoadTimer;
#if ENABLE_STEAM
        public static readonly AppId_t SteamID = new(504230U);
#endif
        private static int _mainThreadId;

        public static Vector2 TargetCenter => new Vector2(TargetWidth, TargetHeight) / 2f;

        public Celeste() : base(TargetWidth, TargetHeight, GameWidth * 3, GameHeight * 3, nameof(Celeste), Settings.Instance.Fullscreen, Settings.Instance.VSync)
        {
            Version = new
#if ENABLE_STEAM
                System.
#endif
                Version(1, 4, 0, 0);
            Instance = this;
            ExitOnEscapeKeypress = false;
            IsFixedTimeStep = true;
#if ENABLE_STEAM
            Stats.MakeRequest();
#endif
            StatsForStadia.MakeRequest();
            Console.WriteLine("CELESTE : " + Version);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Settings.Instance.AfterLoad();
            if (Settings.Instance.Fullscreen)
                ViewPadding = Settings.Instance.ViewportPadding;
            Settings.Instance.ApplyScreen();
            SFX.Initialize();
            Tags.Initialize();
            Input.Initialize();
            Commands.Enabled = PlayMode == PlayModes.Debug;
            Scene = new GameLoader();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Console.WriteLine("BEGIN LOAD");
            LoadTimer = Stopwatch.StartNew();
            PlaybackData.Load();
            if (firstLoad)
            {
                firstLoad = false;
                HudTarget = VirtualContent.CreateRenderTarget("hud-target", TargetWidth + 2, TargetHeight + 2);
                WipeTarget = VirtualContent.CreateRenderTarget("wipe-target", TargetWidth + 2, TargetHeight + 2);
                OVR.Load();
                GFX.Load();
                MTN.Load();
            }
            if (GFX.Game != null)
            {
                Monocle.Draw.Particle = GFX.Game["util/particle"];
                Monocle.Draw.Pixel = new MTexture(GFX.Game["util/pixel"], 1, 1, 1, 1);
            }
            GFX.LoadEffects();
        }

        protected override void Update(GameTime gameTime)
        {
#if ENABLE_STEAM
            SteamAPI.RunCallbacks();
#endif
            SaveRoutine?.Update();
            AutoSplitterInfo.Update();
            Audio.Update();
            base.Update(gameTime);
            Input.UpdateGrab();
        }

        protected override void OnSceneTransition(Scene last, Scene next)
        {
            if (last is not OverworldLoader || next is not Overworld)
                base.OnSceneTransition(last, next);
            TimeRate = 1f;
            Audio.PauseGameplaySfx = false;
            Audio.SetMusicParam("fade", 1f);
            Distort.Anxiety = 0.0f;
            Distort.GameRate = 1f;
            Glitch.Value = 0.0f;
        }

        protected override void RenderCore()
        {
            base.RenderCore();
            if (DisconnectUI == null)
                return;
            DisconnectUI.Render();
        }

        public static void Freeze(float time)
        {
            if (FreezeTimer >= (double)time)
                return;
            FreezeTimer = time;
            if (Scene == null)
                return;
            Scene.Tracker.GetEntity<CassetteBlockManager>()?.AdvanceMusic(time);
        }

        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        private static void Main(string[] args)
        {
            Celeste celeste;
            try
            {
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
                Settings.Initialize();
#if ENABLE_STEAM
                if (SteamAPI.RestartAppIfNecessary(SteamID))
                  return;
                if (!SteamAPI.Init())
                {
                  ErrorLog.Write("Steam not found!");
                  ErrorLog.Open();
                  return;
                }
                if (!Settings.Existed)
                  Settings.Instance.Language = SteamApps.GetCurrentGameLanguage();
#endif
                int num = Settings.Existed ? 1 : 0;
                for (int index = 0; index < args.Length - 1; ++index)
                {
                    if (args[index] is "--language" or "-l")
                        Settings.Instance.Language = args[++index];
                    else if (args[index] is "--default-language" or "-dl")
                    {
                        if (!Settings.Existed)
                            Settings.Instance.Language = args[++index];
                    }
                    else if (args[index] is "--gui" or "-g")
                        Input.OverrideInputPrefix = args[++index];
                }
                celeste = new Celeste();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ErrorLog.Write(ex);
                try
                {
                    ErrorLog.Open();
                    return;
                }
                catch
                {
                    Console.WriteLine("Failed to open the log!");
                    return;
                }
            }
            celeste.RunWithLogging();
            RunThread.WaitAll();
            celeste.Dispose();
            Audio.Unload();
        }

        public static void ReloadAssets(bool levels, bool graphics, bool hires, AreaKey? area = null)
        {
            if (levels)
                ReloadLevels(area);
            if (!graphics)
                return;
            ReloadGraphics(hires);
        }

        public static void ReloadLevels(AreaKey? area = null)
        {
            if (area is null)
            {
                throw new ArgumentNullException(nameof(area));
            }
        }

        public static void ReloadPortraits() { }

        public static void ReloadDialog() { }

        private static void CallProcess(string path, string args = "", bool createWindow = false)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    RedirectStandardOutput = false,
                    CreateNoWindow = !createWindow,
                    UseShellExecute = false,
                    Arguments = args
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public static bool PauseAnywhere()
        {
            switch (Scene)
            {
                case Level _:
                    Level scene1 = Scene as Level;
                    if (scene1.CanPause)
                    {
                        scene1.Pause();
                        return true;
                    }
                    break;
                case Emulator _:
                    Emulator scene2 = Scene as Emulator;
                    if (scene2.CanPause)
                    {
                        scene2.CreatePauseMenu();
                        return true;
                    }
                    break;
                case IntroVignette _:
                    IntroVignette scene3 = Scene as IntroVignette;
                    if (scene3.CanPause)
                    {
                        scene3.OpenMenu();
                        return true;
                    }
                    break;
                case CoreVignette _:
                    CoreVignette scene4 = Scene as CoreVignette;
                    if (scene4.CanPause)
                    {
                        scene4.OpenMenu();
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
