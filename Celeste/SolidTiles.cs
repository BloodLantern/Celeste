﻿// Decompiled with JetBrains decompiler
// Type: Celeste.SolidTiles
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class SolidTiles : Solid
    {
        public TileGrid Tiles;
        public AnimatedTiles AnimatedTiles;
        public Grid Grid;
        private readonly VirtualMap<char> tileTypes;

        public SolidTiles(Vector2 position, VirtualMap<char> data)
            : base(position, 0.0f, 0.0f, true)
        {
            Tag = (int)Tags.Global;
            Depth = -10000;
            tileTypes = data;
            EnableAssistModeChecks = false;
            AllowStaticMovers = false;
            Collider = Grid = new Grid(data.Columns, data.Rows, 8f, 8f);
            for (int x1 = 0; x1 < data.Columns; x1 += 50)
            {
                for (int y1 = 0; y1 < data.Rows; y1 += 50)
                {
                    if (data.AnyInSegmentAtTile(x1, y1))
                    {
                        int x2 = x1;
                        for (int index1 = Math.Min(x2 + 50, data.Columns); x2 < index1; ++x2)
                        {
                            int y2 = y1;
                            for (int index2 = Math.Min(y2 + 50, data.Rows); y2 < index2; ++y2)
                            {
                                if (data[x2, y2] != '0')
                                {
                                    Grid[x2, y2] = true;
                                }
                            }
                        }
                    }
                }
            }
            Autotiler.Generated map = GFX.FGAutotiler.GenerateMap(data, true);
            Tiles = map.TileGrid;
            Tiles.VisualExtend = 1;
            Add(Tiles);
            Add(AnimatedTiles = map.SpriteOverlay);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Tiles.ClipCamera = SceneAs<Level>().Camera;
            AnimatedTiles.ClipCamera = Tiles.ClipCamera;
        }

        private int CoreTileSurfaceIndex()
        {
            Level scene = Scene as Level;
            return scene.CoreMode == Session.CoreModes.Hot ? 37 : scene.CoreMode == Session.CoreModes.Cold ? 36 : 3;
        }

        private int SurfaceSoundIndexAt(Vector2 readPosition)
        {
            int x = (int)((readPosition.X - (double)X) / 8.0);
            int y = (int)((readPosition.Y - (double)Y) / 8.0);
            if (x >= 0 && y >= 0 && x < Grid.CellsX && y < Grid.CellsY)
            {
                char tileType = tileTypes[x, y];
                switch (tileType)
                {
                    case '0':
                        break;
                    case 'k':
                        return CoreTileSurfaceIndex();
                    default:
                        if (SurfaceIndex.TileToIndex.ContainsKey(tileType))
                        {
                            return SurfaceIndex.TileToIndex[tileType];
                        }

                        break;
                }
            }
            return -1;
        }

        public override int GetWallSoundIndex(Player player, int side)
        {
            int wallSoundIndex = SurfaceSoundIndexAt(player.Center + (Vector2.UnitX * side * 8f));
            if (wallSoundIndex < 0)
            {
                wallSoundIndex = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, -6f));
            }

            if (wallSoundIndex < 0)
            {
                wallSoundIndex = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, 6f));
            }

            return wallSoundIndex;
        }

        public override int GetStepSoundIndex(Entity entity)
        {
            int stepSoundIndex = SurfaceSoundIndexAt(entity.BottomCenter + (Vector2.UnitY * 4f));
            if (stepSoundIndex == -1)
            {
                stepSoundIndex = SurfaceSoundIndexAt(entity.BottomLeft + (Vector2.UnitY * 4f));
            }

            if (stepSoundIndex == -1)
            {
                stepSoundIndex = SurfaceSoundIndexAt(entity.BottomRight + (Vector2.UnitY * 4f));
            }

            return stepSoundIndex;
        }

        public override int GetLandSoundIndex(Entity entity)
        {
            int landSoundIndex = SurfaceSoundIndexAt(entity.BottomCenter + (Vector2.UnitY * 4f));
            if (landSoundIndex == -1)
            {
                landSoundIndex = SurfaceSoundIndexAt(entity.BottomLeft + (Vector2.UnitY * 4f));
            }

            if (landSoundIndex == -1)
            {
                landSoundIndex = SurfaceSoundIndexAt(entity.BottomRight + (Vector2.UnitY * 4f));
            }

            return landSoundIndex;
        }
    }
}
