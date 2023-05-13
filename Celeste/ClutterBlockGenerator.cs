// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterBlockGenerator
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public static class ClutterBlockGenerator
    {
        private static Level level;
        private static Tile[,] tiles;
        private static readonly List<Point> active = new();
        private static List<List<TextureSet>> textures;
        private static int columns;
        private static int rows;
        private static readonly bool[] enabled = new bool[3];
        private static bool initialized;

        public static void Init(Level lvl)
        {
            if (initialized)
                return;

            initialized = true;
            level = lvl;
            columns = level.Bounds.Width / 8;
            rows = (level.Bounds.Height / 8) + 1;
            tiles ??= new Tile[200, 200];
            for (int index1 = 0; index1 < columns; ++index1)
                for (int index2 = 0; index2 < rows; ++index2)
                {
                    tiles[index1, index2].Color = -1;
                    tiles[index1, index2].Block = null;
                }
            for (int index = 0; index < enabled.Length; ++index)
                enabled[index] = !level.Session.GetFlag("oshiro_clutter_cleared_" + index);

            if (textures == null)
            {
                textures = new List<List<TextureSet>>();
                for (int index = 0; index < 3; ++index)
                {
                    List<TextureSet> textureSetList1 = new();
                    foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("objects/resortclutter/" + ((ClutterBlock.Colors)index).ToString() + "_"))
                    {
                        int num1 = atlasSubtexture.Width / 8;
                        int num2 = atlasSubtexture.Height / 8;
                        TextureSet textureSet1 = null;
                        foreach (TextureSet textureSet2 in textureSetList1)
                            if (textureSet2.Columns == num1 && textureSet2.Rows == num2)
                            {
                                textureSet1 = textureSet2;
                                break;
                            }
                        if (textureSet1 == null)
                        {
                            List<TextureSet> textureSetList2 = textureSetList1;
                            TextureSet textureSet3 = new()
                            {
                                Columns = num1,
                                Rows = num2
                            };
                            textureSet1 = textureSet3;
                            textureSetList2.Add(textureSet3);
                        }
                        textureSet1.textures.Add(atlasSubtexture);
                    }
                    textureSetList1.Sort((a, b) => -Math.Sign((a.Columns * a.Rows) - (b.Columns * b.Rows)));
                    textures.Add(textureSetList1);
                }
            }
            Point levelSolidOffset = level.LevelSolidOffset;
            for (int index3 = 0; index3 < columns; ++index3)
                for (int index4 = 0; index4 < rows; ++index4)
                    tiles[index3, index4].Wall = level.SolidsData[levelSolidOffset.X + index3, levelSolidOffset.Y + index4] != '0';
        }

        public static void Dispose()
        {
            textures = null;
            tiles = null;
            initialized = false;
        }

        public static void Add(int x, int y, int w, int h, ClutterBlock.Colors color)
        {
            level.Add(new ClutterBlockBase(new Vector2(level.Bounds.X, level.Bounds.Y) + (new Vector2(x, y) * 8f), w * 8, h * 8, enabled[(int) color], color));
            if (!enabled[(int) color])
                return;

            int x1 = Math.Max(0, x);
            for (int index1 = Math.Min(columns, x + w); x1 < index1; ++x1)
            {
                int y1 = Math.Max(0, y);
                for (int index2 = Math.Min(rows, y + h); y1 < index2; ++y1)
                {
                    Point point = new(x1, y1);
                    tiles[point.X, point.Y].Color = (int) color;
                    active.Add(point);
                }
            }
        }

        public static void Generate()
        {
            if (!initialized)
                return;

            active.Shuffle();
            List<ClutterBlock> clutterBlockList = new();
            Rectangle bounds = level.Bounds;
            foreach (Point point in active)
            {
                if (tiles[point.X, point.Y].Block == null)
                {
                    int index1 = 0;
                    int color;
                    TextureSet textureSet;
                    while (true)
                    {
                        color = tiles[point.X, point.Y].Color;
                        textureSet = textures[color][index1];
                        bool flag = true;
                        if (point.X + textureSet.Columns <= columns && point.Y + textureSet.Rows <= rows)
                        {
                            int x = point.X;
                            for (int index2 = point.X + textureSet.Columns; flag && x < index2; ++x)
                            {
                                int y = point.Y;
                                for (int index3 = point.Y + textureSet.Rows; flag && y < index3; ++y)
                                {
                                    Tile tile = tiles[x, y];
                                    if (tile.Block != null || tile.Color != color)
                                        flag = false;
                                }
                            }
                            if (flag)
                                break;
                        }
                        ++index1;
                    }
                    ClutterBlock clutterBlock = new(new Vector2(bounds.X, bounds.Y) + (new Vector2(point.X, point.Y) * 8f), Calc.Random.Choose<MTexture>(textureSet.textures), (ClutterBlock.Colors)color);
                    for (int x = point.X; x < point.X + textureSet.Columns; ++x)
                        for (int y = point.Y; y < point.Y + textureSet.Rows; ++y)
                            tiles[x, y].Block = clutterBlock;
                    clutterBlockList.Add(clutterBlock);
                    level.Add(clutterBlock);
                }
            }
            for (int index4 = 0; index4 < columns; ++index4)
            {
                for (int index5 = 0; index5 < rows; ++index5)
                {
                    Tile tile1 = tiles[index4, index5];
                    if (tile1.Block != null)
                    {
                        ClutterBlock block = tile1.Block;
                        if (!block.TopSideOpen && (index5 == 0 || tiles[index4, index5 - 1].Empty))
                            block.TopSideOpen = true;

                        if (!block.LeftSideOpen && (index4 == 0 || tiles[index4 - 1, index5].Empty))
                            block.LeftSideOpen = true;

                        if (!block.RightSideOpen && (index4 == columns - 1 || tiles[index4 + 1, index5].Empty))
                            block.RightSideOpen = true;

                        if (!block.OnTheGround && index5 < rows - 1)
                        {
                            Tile tile2 = tiles[index4, index5 + 1];
                            if (tile2.Wall)
                                block.OnTheGround = true;
                            else if (tile2.Block != null && tile2.Block != block && !block.HasBelow.Contains(tile2.Block))
                            {
                                _ = block.HasBelow.Add(tile2.Block);
                                block.Below.Add(tile2.Block);
                                tile2.Block.Above.Add(block);
                            }
                        }
                    }
                }
            }
            foreach (ClutterBlock block in clutterBlockList)
                if (block.OnTheGround)
                    SetAboveToOnGround(block);
            initialized = false;
            level = null;
            active.Clear();
        }

        private static void SetAboveToOnGround(ClutterBlock block)
        {
            foreach (ClutterBlock block1 in block.Above)
                if (!block1.OnTheGround)
                {
                    block1.OnTheGround = true;
                    SetAboveToOnGround(block1);
                }
        }

        private struct Tile
        {
            public int Color;
            public bool Wall;
            public ClutterBlock Block;

            public bool Empty => !Wall && Color == -1;
        }

        private class TextureSet
        {
            public int Columns;
            public int Rows;
            public List<MTexture> textures = new();
        }
    }
}
