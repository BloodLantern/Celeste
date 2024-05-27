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
        private static List<Point> active = new List<Point>();
        private static List<List<TextureSet>> textures;
        private static int columns;
        private static int rows;
        private static bool[] enabled = new bool[3];
        private static bool initialized;

        public static void Init(Level lvl)
        {
            if (ClutterBlockGenerator.initialized)
                return;
            ClutterBlockGenerator.initialized = true;
            ClutterBlockGenerator.level = lvl;
            ClutterBlockGenerator.columns = ClutterBlockGenerator.level.Bounds.Width / 8;
            ClutterBlockGenerator.rows = ClutterBlockGenerator.level.Bounds.Height / 8 + 1;
            if (ClutterBlockGenerator.tiles == null)
                ClutterBlockGenerator.tiles = new Tile[200, 200];
            for (int index1 = 0; index1 < ClutterBlockGenerator.columns; ++index1)
            {
                for (int index2 = 0; index2 < ClutterBlockGenerator.rows; ++index2)
                {
                    ClutterBlockGenerator.tiles[index1, index2].Color = -1;
                    ClutterBlockGenerator.tiles[index1, index2].Block = null;
                }
            }
            for (int index = 0; index < ClutterBlockGenerator.enabled.Length; ++index)
                ClutterBlockGenerator.enabled[index] = !ClutterBlockGenerator.level.Session.GetFlag("oshiro_clutter_cleared_" + index);
            if (ClutterBlockGenerator.textures == null)
            {
                ClutterBlockGenerator.textures = new List<List<TextureSet>>();
                for (int index = 0; index < 3; ++index)
                {
                    List<TextureSet> textureSetList1 = new List<TextureSet>();
                    foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("objects/resortclutter/" + ((ClutterBlock.Colors) index) + "_"))
                    {
                        int num1 = atlasSubtexture.Width / 8;
                        int num2 = atlasSubtexture.Height / 8;
                        TextureSet textureSet1 = null;
                        foreach (TextureSet textureSet2 in textureSetList1)
                        {
                            if (textureSet2.Columns == num1 && textureSet2.Rows == num2)
                            {
                                textureSet1 = textureSet2;
                                break;
                            }
                        }
                        if (textureSet1 == null)
                        {
                            List<TextureSet> textureSetList2 = textureSetList1;
                            TextureSet textureSet3 = new TextureSet();
                            textureSet3.Columns = num1;
                            textureSet3.Rows = num2;
                            textureSet1 = textureSet3;
                            textureSetList2.Add(textureSet3);
                        }
                        textureSet1.textures.Add(atlasSubtexture);
                    }
                    textureSetList1.Sort((a, b) => -Math.Sign(a.Columns * a.Rows - b.Columns * b.Rows));
                    ClutterBlockGenerator.textures.Add(textureSetList1);
                }
            }
            Point levelSolidOffset = ClutterBlockGenerator.level.LevelSolidOffset;
            for (int index3 = 0; index3 < ClutterBlockGenerator.columns; ++index3)
            {
                for (int index4 = 0; index4 < ClutterBlockGenerator.rows; ++index4)
                    ClutterBlockGenerator.tiles[index3, index4].Wall = ClutterBlockGenerator.level.SolidsData[levelSolidOffset.X + index3, levelSolidOffset.Y + index4] != '0';
            }
        }

        public static void Dispose()
        {
            ClutterBlockGenerator.textures = null;
            ClutterBlockGenerator.tiles = null;
            ClutterBlockGenerator.initialized = false;
        }

        public static void Add(int x, int y, int w, int h, ClutterBlock.Colors color)
        {
            ClutterBlockGenerator.level.Add(new ClutterBlockBase(new Vector2(ClutterBlockGenerator.level.Bounds.X, ClutterBlockGenerator.level.Bounds.Y) + new Vector2(x, y) * 8f, w * 8, h * 8, ClutterBlockGenerator.enabled[(int) color], color));
            if (!ClutterBlockGenerator.enabled[(int) color])
                return;
            int x1 = Math.Max(0, x);
            for (int index1 = Math.Min(ClutterBlockGenerator.columns, x + w); x1 < index1; ++x1)
            {
                int y1 = Math.Max(0, y);
                for (int index2 = Math.Min(ClutterBlockGenerator.rows, y + h); y1 < index2; ++y1)
                {
                    Point point = new Point(x1, y1);
                    ClutterBlockGenerator.tiles[point.X, point.Y].Color = (int) color;
                    ClutterBlockGenerator.active.Add(point);
                }
            }
        }

        public static void Generate()
        {
            if (!ClutterBlockGenerator.initialized)
                return;
            ClutterBlockGenerator.active.Shuffle();
            List<ClutterBlock> clutterBlockList = new List<ClutterBlock>();
            Rectangle bounds = ClutterBlockGenerator.level.Bounds;
            foreach (Point point in ClutterBlockGenerator.active)
            {
                if (ClutterBlockGenerator.tiles[point.X, point.Y].Block == null)
                {
                    int index1 = 0;
                    int color;
                    TextureSet textureSet;
                    while (true)
                    {
                        color = ClutterBlockGenerator.tiles[point.X, point.Y].Color;
                        textureSet = ClutterBlockGenerator.textures[color][index1];
                        bool flag = true;
                        if (point.X + textureSet.Columns <= ClutterBlockGenerator.columns && point.Y + textureSet.Rows <= ClutterBlockGenerator.rows)
                        {
                            int x = point.X;
                            for (int index2 = point.X + textureSet.Columns; flag && x < index2; ++x)
                            {
                                int y = point.Y;
                                for (int index3 = point.Y + textureSet.Rows; flag && y < index3; ++y)
                                {
                                    Tile tile = ClutterBlockGenerator.tiles[x, y];
                                    if (tile.Block != null || tile.Color != color)
                                        flag = false;
                                }
                            }
                            if (flag)
                                break;
                        }
                        ++index1;
                    }
                    ClutterBlock clutterBlock = new ClutterBlock(new Vector2(bounds.X, bounds.Y) + new Vector2(point.X, point.Y) * 8f, Calc.Random.Choose(textureSet.textures), (ClutterBlock.Colors) color);
                    for (int x = point.X; x < point.X + textureSet.Columns; ++x)
                    {
                        for (int y = point.Y; y < point.Y + textureSet.Rows; ++y)
                            ClutterBlockGenerator.tiles[x, y].Block = clutterBlock;
                    }
                    clutterBlockList.Add(clutterBlock);
                    ClutterBlockGenerator.level.Add(clutterBlock);
                }
            }
            for (int index4 = 0; index4 < ClutterBlockGenerator.columns; ++index4)
            {
                for (int index5 = 0; index5 < ClutterBlockGenerator.rows; ++index5)
                {
                    Tile tile1 = ClutterBlockGenerator.tiles[index4, index5];
                    if (tile1.Block != null)
                    {
                        ClutterBlock block = tile1.Block;
                        if (!block.TopSideOpen && (index5 == 0 || ClutterBlockGenerator.tiles[index4, index5 - 1].Empty))
                            block.TopSideOpen = true;
                        if (!block.LeftSideOpen && (index4 == 0 || ClutterBlockGenerator.tiles[index4 - 1, index5].Empty))
                            block.LeftSideOpen = true;
                        if (!block.RightSideOpen && (index4 == ClutterBlockGenerator.columns - 1 || ClutterBlockGenerator.tiles[index4 + 1, index5].Empty))
                            block.RightSideOpen = true;
                        if (!block.OnTheGround && index5 < ClutterBlockGenerator.rows - 1)
                        {
                            Tile tile2 = ClutterBlockGenerator.tiles[index4, index5 + 1];
                            if (tile2.Wall)
                                block.OnTheGround = true;
                            else if (tile2.Block != null && tile2.Block != block && !block.HasBelow.Contains(tile2.Block))
                            {
                                block.HasBelow.Add(tile2.Block);
                                block.Below.Add(tile2.Block);
                                tile2.Block.Above.Add(block);
                            }
                        }
                    }
                }
            }
            foreach (ClutterBlock block in clutterBlockList)
            {
                if (block.OnTheGround)
                    ClutterBlockGenerator.SetAboveToOnGround(block);
            }
            ClutterBlockGenerator.initialized = false;
            ClutterBlockGenerator.level = null;
            ClutterBlockGenerator.active.Clear();
        }

        private static void SetAboveToOnGround(ClutterBlock block)
        {
            foreach (ClutterBlock block1 in block.Above)
            {
                if (!block1.OnTheGround)
                {
                    block1.OnTheGround = true;
                    ClutterBlockGenerator.SetAboveToOnGround(block1);
                }
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
            public List<MTexture> textures = new List<MTexture>();
        }
    }
}
