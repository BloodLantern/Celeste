using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste
{
    public class LevelLoader : Scene
    {
        private readonly Session session;
        private Vector2? startPosition;
        private bool started;
        public Player.IntroTypes? PlayerIntroTypeOverride;

        public Level Level { get; private set; }

        public bool Loaded { get; private set; }

        public LevelLoader(Session session, Vector2? startPosition = null)
        {
            this.session = session;
            this.startPosition = startPosition.HasValue ? startPosition : session.RespawnPoint;
            Level = new Level();
            RunThread.Start(new Action(LoadingThread), "LEVEL_LOADER");
        }

        private void LoadingThread()
        {
            MapData mapData = session.MapData;
            AreaData areaData = AreaData.Get(session);

            if (session.Area.ID == 0)
                SaveData.Instance.Assists.DashMode = Assists.DashModes.Normal;

            Level.Add(Level.GameplayRenderer = new GameplayRenderer());
            Level.Add(Level.Lighting = new LightingRenderer());
            Level.Add(Level.Bloom = new BloomRenderer());
            Level.Add(Level.Displacement = new DisplacementRenderer());
            Level.Add(Level.Background = new BackdropRenderer());
            Level.Add(Level.Foreground = new BackdropRenderer());

            Level.Add(new DustEdges());
            Level.Add(new WaterSurface());
            Level.Add(new MirrorSurfaces());
            Level.Add(new GlassBlockBg());
            Level.Add(new LightningRenderer());
            Level.Add(new SeekerBarrierRenderer());

            Level.Add(Level.HudRenderer = new HudRenderer());

            if (session.Area.ID == 9)
                Level.Add(new IceTileOverlay());

            Level.BaseLightingAlpha = Level.Lighting.Alpha = areaData.DarknessAlpha;
            Level.Bloom.Base = areaData.BloomBase;
            Level.Bloom.Strength = areaData.BloomStrength;
            Level.BackgroundColor = mapData.BackgroundColor;
            Level.Background.Backdrops = mapData.CreateBackdrops(mapData.Background);

            foreach (Backdrop backdrop in Level.Background.Backdrops)
                backdrop.Renderer = Level.Background;
            Level.Foreground.Backdrops = mapData.CreateBackdrops(mapData.Foreground);
            foreach (Backdrop backdrop in Level.Foreground.Backdrops)
                backdrop.Renderer = Level.Foreground;

            Level.RendererList.UpdateLists();
            Level.Add(Level.FormationBackdrop = new FormationBackdrop());

            Level.Camera = Level.GameplayRenderer.Camera;
            Audio.SetCamera(Level.Camera);

            Level.Session = session;
            SaveData.Instance.StartSession(Level.Session);

            Level.Particles = new ParticleSystem(-8000, 400)
            {
                Tag = (int)Tags.Global
            };
            Level.Add(Level.Particles);

            Level.ParticlesBG = new ParticleSystem(8000, 400)
            {
                Tag = (int)Tags.Global
            };
            Level.Add(Level.ParticlesBG);

            Level.ParticlesFG = new ParticleSystem(-50000, 800)
            {
                Tag = (int)Tags.Global
            };
            Level.ParticlesFG.Add(new MirrorReflection());

            Level.Add(Level.ParticlesFG);

            Level.Add(Level.strawberriesDisplay = new TotalStrawberriesDisplay());
            Level.Add(new SpeedrunTimerDisplay());
            Level.Add(new GameplayStats());
            Level.Add(new GrabbyIcon());

            Rectangle mapTileBounds = mapData.TileBounds;

            GFX.FGAutotiler.LevelBounds.Clear();

            VirtualMap<char> bgMap = new(mapTileBounds.Width, mapTileBounds.Height, '0');
            VirtualMap<char> fgMap = new(mapTileBounds.Width, mapTileBounds.Height, '0');
            VirtualMap<bool> solidMap = new(mapTileBounds.Width, mapTileBounds.Height);

            Regex lineSeparator = new("\\r\\n|\\n\\r|\\n|\\r");

            foreach (LevelData level in mapData.Levels)
            {
                Rectangle tileBounds2 = level.TileBounds;
                int left1 = tileBounds2.Left;
                tileBounds2 = level.TileBounds;
                int top1 = tileBounds2.Top;
                string[] strArray1 = lineSeparator.Split(level.Bg);
                for (int index1 = top1; index1 < top1 + strArray1.Length; ++index1)
                {
                    for (int index2 = left1; index2 < left1 + strArray1[index1 - top1].Length; ++index2)
                        bgMap[index2 - mapTileBounds.X, index1 - mapTileBounds.Y] = strArray1[index1 - top1][index2 - left1];
                }
                string[] strArray2 = lineSeparator.Split(level.Solids);
                for (int index3 = top1; index3 < top1 + strArray2.Length; ++index3)
                {
                    for (int index4 = left1; index4 < left1 + strArray2[index3 - top1].Length; ++index4)
                        fgMap[index4 - mapTileBounds.X, index3 - mapTileBounds.Y] = strArray2[index3 - top1][index4 - left1];
                }
                tileBounds2 = level.TileBounds;
                int left2 = tileBounds2.Left;
                while (true)
                {
                    int num1 = left2;
                    tileBounds2 = level.TileBounds;
                    int right = tileBounds2.Right;
                    if (num1 < right)
                    {
                        tileBounds2 = level.TileBounds;
                        int top2 = tileBounds2.Top;
                        while (true)
                        {
                            int num2 = top2;
                            tileBounds2 = level.TileBounds;
                            int bottom = tileBounds2.Bottom;
                            if (num2 < bottom)
                            {
                                solidMap[left2 - mapTileBounds.Left, top2 - mapTileBounds.Top] = true;
                                ++top2;
                            }
                            else
                                break;
                        }
                        ++left2;
                    }
                    else
                        break;
                }
                GFX.FGAutotiler.LevelBounds.Add(new Rectangle(level.TileBounds.X - mapTileBounds.X, level.TileBounds.Y - mapTileBounds.Y, level.TileBounds.Width, level.TileBounds.Height));
            }

            foreach (Rectangle filler in mapData.Filler)
            {
                for (int x = filler.Left; x < filler.Right; x++)
                {
                    for (int y = filler.Top; y < filler.Bottom; y++)
                    {
                        char fillerType = '0';

                        if (filler.Top - mapTileBounds.Y > 0)
                        {
                            char topType = fgMap[x - mapTileBounds.X, filler.Top - mapTileBounds.Y - 1];
                            if (topType != '0')
                                fillerType = topType;
                        }

                        if (fillerType == '0' && filler.Left - mapTileBounds.X > 0)
                        {
                            char leftType = fgMap[filler.Left - mapTileBounds.X - 1, y - mapTileBounds.Y];
                            if (leftType != '0')
                                fillerType = leftType;
                        }

                        if (fillerType == '0' && filler.Right - mapTileBounds.X < mapTileBounds.Width - 1)
                        {
                            char rightType = fgMap[filler.Right - mapTileBounds.X, y - mapTileBounds.Y];
                            if (rightType != '0')
                                fillerType = rightType;
                        }

                        if (fillerType == '0' && filler.Bottom - mapTileBounds.Y < mapTileBounds.Height - 1)
                        {
                            char bottomType = fgMap[x - mapTileBounds.X, filler.Bottom - mapTileBounds.Y];
                            if (bottomType != '0')
                                fillerType = bottomType;
                        }

                        if (fillerType == '0')
                            fillerType = '1';

                        fgMap[x - mapTileBounds.X, y - mapTileBounds.Y] = fillerType;
                        solidMap[x - mapTileBounds.X, y - mapTileBounds.Y] = true;
                    }
                }
            }

            using (List<LevelData>.Enumerator enumerator = mapData.Levels.GetEnumerator())
            {
                const int halfTileSize = 4;
                while (enumerator.MoveNext())
                {
                    LevelData level = enumerator.Current;
                    Rectangle levelTileBounds = level.TileBounds;

                    for (int x = levelTileBounds.Left; x < levelTileBounds.Right; x++)
                    {
                        int top = levelTileBounds.Top;
                        char ch6 = bgMap[x - mapTileBounds.X, top - mapTileBounds.Y];
                        for (int i = 1; i < halfTileSize && !solidMap[x - mapTileBounds.X, top - mapTileBounds.Y - i]; i++)
                            bgMap[x - mapTileBounds.X, top - mapTileBounds.Y - i] = ch6;

                        int bottom = levelTileBounds.Bottom - 1;
                        char ch7 = bgMap[x - mapTileBounds.X, bottom - mapTileBounds.Y];
                        for (int i = 1; i < halfTileSize && !solidMap[x - mapTileBounds.X, bottom - mapTileBounds.Y + i]; i++)
                            bgMap[x - mapTileBounds.X, bottom - mapTileBounds.Y + i] = ch7;
                    }
                    
                    for (int y = levelTileBounds.Top - halfTileSize; y < levelTileBounds.Bottom + halfTileSize; y++)
                    {
                        int left = levelTileBounds.Left;
                        char ch8 = bgMap[left - mapTileBounds.X, y - mapTileBounds.Y];
                        for (int i = 1; i < halfTileSize && !solidMap[left - mapTileBounds.X - i, y - mapTileBounds.Y]; i++)
                            bgMap[left - mapTileBounds.X - i, y - mapTileBounds.Y] = ch8;

                        int right = levelTileBounds.Right - 1;
                        char ch9 = bgMap[right - mapTileBounds.X, y - mapTileBounds.Y];
                        for (int i = 1; i < halfTileSize && !solidMap[right - mapTileBounds.X + i, y - mapTileBounds.Y]; i++)
                            bgMap[right - mapTileBounds.X + i, y - mapTileBounds.Y] = ch9;
                    }
                }
            }
            using (List<LevelData>.Enumerator enumerator = mapData.Levels.GetEnumerator())
            {
label_96:
                while (enumerator.MoveNext())
                {
                    LevelData current = enumerator.Current;
                    Rectangle tileBounds4 = current.TileBounds;
                    int left = tileBounds4.Left;
                    while (true)
                    {
                        int num9 = left;
                        tileBounds4 = current.TileBounds;
                        int right = tileBounds4.Right;
                        if (num9 < right)
                        {
                            tileBounds4 = current.TileBounds;
                            int top = tileBounds4.Top;
                            if (fgMap[left - mapTileBounds.X, top - mapTileBounds.Y] == '0')
                            {
                                for (int index = 1; index < 8; ++index)
                                    solidMap[left - mapTileBounds.X, top - mapTileBounds.Y - index] = true;
                            }
                            tileBounds4 = current.TileBounds;
                            int num10 = tileBounds4.Bottom - 1;
                            if (fgMap[left - mapTileBounds.X, num10 - mapTileBounds.Y] == '0')
                            {
                                for (int index = 1; index < 8; ++index)
                                    solidMap[left - mapTileBounds.X, num10 - mapTileBounds.Y + index] = true;
                            }
                            ++left;
                        }
                        else
                            goto label_96;
                    }
                }
            }
            using (List<LevelData>.Enumerator enumerator = mapData.Levels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    LevelData current = enumerator.Current;
                    Rectangle tileBounds5 = current.TileBounds;
                    int left5 = tileBounds5.Left;

                    while (true)
                    {
                        int num11 = left5;
                        tileBounds5 = current.TileBounds;
                        int right = tileBounds5.Right;
                        if (num11 < right)
                        {
                            tileBounds5 = current.TileBounds;
                            int top = tileBounds5.Top;
                            char ch10 = fgMap[left5 - mapTileBounds.X, top - mapTileBounds.Y];
                            for (int index = 1; index < 4 && !solidMap[left5 - mapTileBounds.X, top - mapTileBounds.Y - index]; ++index)
                                fgMap[left5 - mapTileBounds.X, top - mapTileBounds.Y - index] = ch10;
                            tileBounds5 = current.TileBounds;
                            int num12 = tileBounds5.Bottom - 1;
                            char ch11 = fgMap[left5 - mapTileBounds.X, num12 - mapTileBounds.Y];
                            for (int index = 1; index < 4 && !solidMap[left5 - mapTileBounds.X, num12 - mapTileBounds.Y + index]; ++index)
                                fgMap[left5 - mapTileBounds.X, num12 - mapTileBounds.Y + index] = ch11;
                            ++left5;
                        }
                        else
                            break;
                    }

                    tileBounds5 = current.TileBounds;
                    int num13 = tileBounds5.Top - 4;

                    while (true)
                    {
                        int num14 = num13;
                        tileBounds5 = current.TileBounds;
                        int num15 = tileBounds5.Bottom + 4;
                        if (num14 < num15)
                        {
                            tileBounds5 = current.TileBounds;
                            int left6 = tileBounds5.Left;
                            char ch12 = fgMap[left6 - mapTileBounds.X, num13 - mapTileBounds.Y];
                            for (int index = 1; index < 4 && !solidMap[left6 - mapTileBounds.X - index, num13 - mapTileBounds.Y]; ++index)
                                fgMap[left6 - mapTileBounds.X - index, num13 - mapTileBounds.Y] = ch12;
                            tileBounds5 = current.TileBounds;
                            int num16 = tileBounds5.Right - 1;
                            char ch13 = fgMap[num16 - mapTileBounds.X, num13 - mapTileBounds.Y];
                            for (int index = 1; index < 4 && !solidMap[num16 - mapTileBounds.X + index, num13 - mapTileBounds.Y]; ++index)
                                fgMap[num16 - mapTileBounds.X + index, num13 - mapTileBounds.Y] = ch13;
                            ++num13;
                        }
                        else
                            break;
                    }
                }
            }

            Vector2 position = new Vector2(mapTileBounds.X, mapTileBounds.Y) * 8f;
            Calc.PushRandom(mapData.LoadSeed);
            BackgroundTiles backgroundTiles = new(position, bgMap);
            Level.BgTiles = backgroundTiles;
            Level.Add(backgroundTiles);
            SolidTiles solidTiles = new(position, fgMap);
            Level.SolidTiles = solidTiles;
            Level.Add(solidTiles);
            Level.BgData = bgMap;
            Level.SolidsData = fgMap;
            Calc.PopRandom();
            new Entity(position)
            {
                 (Level.FgTilesLightMask = new TileGrid(8, 8, mapTileBounds.Width, mapTileBounds.Height))
            };
            Level.FgTilesLightMask.Color = Color.Black;
            foreach (LevelData level in mapData.Levels)
            {
                Rectangle tileBounds6 = level.TileBounds;
                int left = tileBounds6.Left;
                tileBounds6 = level.TileBounds;
                int top = tileBounds6.Top;
                int width = level.TileBounds.Width;
                int height = level.TileBounds.Height;
                if (!string.IsNullOrEmpty(level.BgTiles))
                {
                    int[,] tiles = Calc.ReadCSVIntGrid(level.BgTiles, width, height);
                    backgroundTiles.Tiles.Overlay(GFX.SceneryTiles, tiles, left - mapTileBounds.X, top - mapTileBounds.Y);
                }
                if (!string.IsNullOrEmpty(level.FgTiles))
                {
                    int[,] tiles = Calc.ReadCSVIntGrid(level.FgTiles, width, height);
                    solidTiles.Tiles.Overlay(GFX.SceneryTiles, tiles, left - mapTileBounds.X, top - mapTileBounds.Y);
                    Level.FgTilesLightMask.Overlay(GFX.SceneryTiles, tiles, left - mapTileBounds.X, top - mapTileBounds.Y);
                }
            }
            areaData.OnLevelBegin?.Invoke(Level);
            Level.StartPosition = startPosition;
            Level.Pathfinder = new Pathfinder(Level);
            Loaded = true;
        }

        private void StartLevel()
        {
            started = true;
            Session session = Level.Session;
            Level.LoadLevel(PlayerIntroTypeOverride ?? (!session.FirstLevel || !session.StartedFromBeginning || !session.JustStarted ? Player.IntroTypes.Respawn : (session.Area.Mode != AreaMode.CSide ? AreaData.Get(Level).IntroType : Player.IntroTypes.WalkInRight)), true);
            Level.Session.JustStarted = false;
            if (Engine.Scene != this)
                return;
            Engine.Scene = Level;
        }

        public override void Update()
        {
            base.Update();
            if (!Loaded || started)
                return;
            StartLevel();
        }
    }
}
