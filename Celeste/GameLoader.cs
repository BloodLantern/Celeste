// Decompiled with JetBrains decompiler
// Type: Celeste.GameLoader
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Celeste
{
    public class GameLoader : Scene
    {
        public HiresSnow Snow;
        private readonly Atlas opening;
        private bool loaded;
        private bool audioLoaded;
        private bool audioStarted;
        private bool dialogLoaded;
        private Entity handler;
        private Thread activeThread;
        private bool skipped;
        private bool ready;
        private List<MTexture> loadingTextures;
        private float loadingFrame;
        private float loadingAlpha;

        public GameLoader()
        {
            Console.WriteLine("GAME DISPLAYED (in " + Celeste.LoadTimer.ElapsedMilliseconds + "ms)");
            Snow = new HiresSnow();
            opening = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Opening"), Atlas.AtlasDataFormat.PackerNoAtlas);
        }

        public override void Begin()
        {
            Add(new HudRenderer());
            Add(Snow);
            FadeWipe fadeWipe = new(this, true);
            RendererList.UpdateLists();
            Add(handler = new Entity());
            handler.Tag = (int)Tags.HUD;
            handler.Add(new Coroutine(IntroRoutine()));
            activeThread = Thread.CurrentThread;
            activeThread.Priority = ThreadPriority.Lowest;
            RunThread.Start(new Action(LoadThread), "GAME_LOADER", true);
        }

        private void LoadThread()
        {
            MInput.Disabled = true;
            Stopwatch stopwatch1 = Stopwatch.StartNew();
            Audio.Init();
            Audio.Banks.Master = Audio.Banks.Load("Master Bank", true);
            Audio.Banks.Music = Audio.Banks.Load("music", false);
            Audio.Banks.Sfxs = Audio.Banks.Load("sfx", false);
            Audio.Banks.UI = Audio.Banks.Load("ui", false);
            Audio.Banks.DlcMusic = Audio.Banks.Load("dlc_music", false);
            Audio.Banks.DlcSfxs = Audio.Banks.Load("dlc_sfx", false);
            Settings.Instance.ApplyVolumes();
            audioLoaded = true;
            Console.WriteLine(" - AUDIO LOAD: " + stopwatch1.ElapsedMilliseconds + "ms");
            GFX.Load();
            MTN.Load();
            GFX.LoadData();
            MTN.LoadData();
            Stopwatch stopwatch2 = Stopwatch.StartNew();
            Fonts.Prepare();
            Dialog.Load();
            _ = Fonts.Load(Dialog.Languages["english"].FontFace);
            _ = Fonts.Load(Dialog.Languages[Settings.Instance.Language].FontFace);
            dialogLoaded = true;
            Console.WriteLine(" - DIA/FONT LOAD: " + stopwatch2.ElapsedMilliseconds + "ms");
            MInput.Disabled = false;
            Stopwatch stopwatch3 = Stopwatch.StartNew();
            AreaData.Load();
            Console.WriteLine(" - LEVELS LOAD: " + stopwatch3.ElapsedMilliseconds + "ms");
            Console.WriteLine("DONE LOADING (in " + Celeste.LoadTimer.ElapsedMilliseconds + "ms)");
            Celeste.LoadTimer.Stop();
            Celeste.LoadTimer = null;
            loaded = true;
        }

        public IEnumerator IntroRoutine()
        {
            GameLoader gameLoader = this;
            if (Celeste.PlayMode != Celeste.PlayModes.Debug)
            {
                float p;
                for (p = 0.0f; (double)p > 1.0 && !gameLoader.skipped; p += Engine.DeltaTime)
                {
                    yield return null;
                }

                if (!gameLoader.skipped)
                {
                    Monocle.Image img = new(gameLoader.opening["presentedby"]);
                    yield return gameLoader.FadeInOut(img);
                }
                if (!gameLoader.skipped)
                {
                    Monocle.Image img = new(gameLoader.opening["gameby"]);
                    yield return gameLoader.FadeInOut(img);
                }
                bool flag = true;
                if (!gameLoader.skipped & flag)
                {
                    while (!gameLoader.dialogLoaded)
                    {
                        yield return null;
                    }

                    AutoSavingNotice notice = new();
                    gameLoader.Add(notice);
                    for (p = 0.0f; (double)p < 1.0 && !gameLoader.skipped; p += Engine.DeltaTime)
                    {
                        yield return null;
                    }

                    notice.Display = false;
                    while (notice.StillVisible)
                    {
                        notice.ForceClose = gameLoader.skipped;
                        yield return null;
                    }
                    gameLoader.Remove(notice);
                }
            }
            gameLoader.ready = true;
            if (!gameLoader.loaded)
            {
                gameLoader.loadingTextures = OVR.Atlas.GetAtlasSubtextures("loading/");
                Monocle.Image img = new(gameLoader.loadingTextures[0]);
                _ = img.CenterOrigin();
                img.Scale = Vector2.One * 0.5f;
                gameLoader.handler.Add(img);
                while (!gameLoader.loaded || gameLoader.loadingAlpha > 0.0)
                {
                    gameLoader.loadingFrame += Engine.DeltaTime * 10f;
                    gameLoader.loadingAlpha = Calc.Approach(gameLoader.loadingAlpha, gameLoader.loaded ? 0.0f : 1f, Engine.DeltaTime * 4f);
                    img.Texture = gameLoader.loadingTextures[(int)(gameLoader.loadingFrame % (double)gameLoader.loadingTextures.Count)];
                    img.Color = Color.White * Ease.CubeOut(gameLoader.loadingAlpha);
                    img.Position = new Vector2(1792f, (float)(1080.0 - (128.0 * (double)Ease.CubeOut(gameLoader.loadingAlpha))));
                    yield return null;
                }
            }
            gameLoader.opening.Dispose();
            gameLoader.activeThread.Priority = ThreadPriority.Normal;
            MInput.Disabled = false;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen, gameLoader.Snow);
        }

        private IEnumerator FadeInOut(Monocle.Image img)
        {
            float alpha = 0.0f;
            img.Color = Color.White * 0.0f;
            handler.Add(img);
            for (float i = 0.0f; (double)i < 4.5 && !skipped; i += Engine.DeltaTime)
            {
                alpha = Ease.CubeOut(Math.Min(i, 1f));
                img.Color = Color.White * alpha;
                yield return null;
            }
            while ((double)alpha > 0.0)
            {
                alpha -= Engine.DeltaTime * (skipped ? 8f : 1f);
                img.Color = Color.White * alpha;
                yield return null;
            }
        }

        public override void Update()
        {
            if (audioLoaded && !audioStarted)
            {
                _ = Audio.SetAmbience("event:/env/amb/worldmap");
                audioStarted = true;
            }
            if (!ready)
            {
                int num = MInput.Disabled ? 1 : 0;
                MInput.Disabled = false;
                if (Input.MenuConfirm.Pressed)
                {
                    skipped = true;
                }

                MInput.Disabled = num != 0;
            }
            base.Update();
        }
    }
}
