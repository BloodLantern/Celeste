﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    public class Autotiler
    {
        private class TerrainType
        {
            public char ID;
            public HashSet<char> Ignores = new();
            public List<Masked> Masked = new();
            public Tiles Center = new();
            public Tiles Padded = new();

            public TerrainType(char id) => ID = id;

            public bool Ignore(char c)
            {
                if (ID == c)
                    return false;
                return Ignores.Contains(c) || Ignores.Contains('*');
            }
        }

        private class Masked
        {
            public byte[] Mask = new byte[9];
            public Tiles Tiles = new();
        }

        private class Tiles
        {
            public List<MTexture> Textures = new();
            public List<string> OverlapSprites = new();
            public bool HasOverlays;
        }

        public struct Generated
        {
            public TileGrid TileGrid;
            public AnimatedTiles SpriteOverlay;
        }

        public struct Behaviour
        {
            public bool PaddingIgnoreOutOfLevel;
            public bool EdgesIgnoreOutOfLevel;
            public bool EdgesExtend;
        }

        public List<Rectangle> LevelBounds = new();
        private readonly Dictionary<char, TerrainType> lookup = new();
        private readonly byte[] adjacent = new byte[9];

        public Autotiler(string filename)
        {
            Dictionary<char, XmlElement> loadedTilesets = new();
            foreach (XmlElement tilesetXml in Calc.LoadContentXML(filename).GetElementsByTagName("Tileset"))
            {
                char id = tilesetXml.AttrChar("id");
                Tileset tileset = new(GFX.Game["tilesets/" + tilesetXml.Attr("path")], 8, 8);
                TerrainType data = new(id);
                ReadInto(data, tileset, tilesetXml);
                if (tilesetXml.HasAttr("copy"))
                {
                    char key = tilesetXml.AttrChar("copy");
                    if (!loadedTilesets.ContainsKey(key))
                        throw new Exception("Copied tilesets must be defined before the tilesets that copy them!");
                    ReadInto(data, tileset, loadedTilesets[key]);
                }
                if (tilesetXml.HasAttr("ignores"))
                {
                    string ignores = tilesetXml.Attr("ignores");
                    foreach (string ignore in ignores.Split(','))
                    {
                        if (ignore.Length > 0)
                            data.Ignores.Add(ignore[0]);
                    }
                }
                loadedTilesets.Add(id, tilesetXml);
                lookup.Add(id, data);
            }
        }

        private void ReadInto(TerrainType data, Tileset tileset, XmlElement xml)
        {
            foreach (object obj in (XmlNode) xml)
            {
                if (obj is not XmlComment)
                {
                    XmlElement xml1 = obj as XmlElement;
                    string str1 = xml1.Attr("mask");
                    Tiles tiles;
                    if (str1 == "center")
                        tiles = data.Center;
                    else if (str1 == "padding")
                    {
                        tiles = data.Padded;
                    }
                    else
                    {
                        Masked masked = new();
                        tiles = masked.Tiles;
                        int index = 0;
                        int num = 0;
                        for (; index < str1.Length; ++index)
                        {
                            if (str1[index] == '0')
                                masked.Mask[num++] = 0;
                            else if (str1[index] == '1')
                                masked.Mask[num++] = 1;
                            else if (str1[index] is 'x' or 'X')
                                masked.Mask[num++] = 2;
                        }
                        data.Masked.Add(masked);
                    }
                    string str2 = xml1.Attr("tiles");
                    char[] chArray1 = new char[1]{ ';' };
                    foreach (string str3 in str2.Split(chArray1))
                    {
                        char[] chArray2 = new char[1]{ ',' };
                        string[] strArray = str3.Split(chArray2);
                        int x = int.Parse(strArray[0]);
                        int y = int.Parse(strArray[1]);
                        MTexture mtexture = tileset[x, y];
                        tiles.Textures.Add(mtexture);
                    }
                    if (xml1.HasAttr("sprites"))
                    {
                        string str4 = xml1.Attr("sprites");
                        char[] chArray3 = new char[1]{ ',' };
                        foreach (string str5 in str4.Split(chArray3))
                            tiles.OverlapSprites.Add(str5);
                        tiles.HasOverlays = true;
                    }
                }
            }
            data.Masked.Sort((a, b) =>
            {
                int num1 = 0;
                int num2 = 0;
                for (int index = 0; index < 9; ++index)
                {
                    if (a.Mask[index] == 2)
                        ++num1;
                    if (b.Mask[index] == 2)
                        ++num2;
                }
                return num1 - num2;
            });
        }

        public Generated GenerateMap(
            VirtualMap<char> mapData,
            bool paddingIgnoreOutOfLevel)
        {
            Behaviour behaviour = new()
            {
                EdgesExtend = true,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = paddingIgnoreOutOfLevel
            };
            return Generate(mapData, 0, 0, mapData.Columns, mapData.Rows, false, '0', behaviour);
        }

        public Generated GenerateMap(
            VirtualMap<char> mapData,
            Behaviour behaviour)
        {
            return Generate(mapData, 0, 0, mapData.Columns, mapData.Rows, false, '0', behaviour);
        }

        public Generated GenerateBox(char id, int tilesX, int tilesY) => Generate(null, 0, 0, tilesX, tilesY, true, id, new Behaviour());

        public Generated GenerateOverlay(
            char id,
            int x,
            int y,
            int tilesX,
            int tilesY,
            VirtualMap<char> mapData)
        {
            Behaviour behaviour = new()
            {
                EdgesExtend = true,
                EdgesIgnoreOutOfLevel = true,
                PaddingIgnoreOutOfLevel = true
            };
            return Generate(mapData, x, y, tilesX, tilesY, true, id, behaviour);
        }

        private Generated Generate(
            VirtualMap<char> mapData,
            int startX,
            int startY,
            int tilesX,
            int tilesY,
            bool forceSolid,
            char forceID,
            Behaviour behaviour)
        {
            TileGrid tileGrid = new(8, 8, tilesX, tilesY);
            AnimatedTiles animatedTiles = new(tilesX, tilesY, GFX.AnimatedTilesBank);

            Rectangle forceFill = Rectangle.Empty;
            if (forceSolid)
                forceFill = new Rectangle(startX, startY, tilesX, tilesY);

            if (mapData != null)
            {
                for (int x = startX; x < startX + tilesX; x += VirtualMap<char>.SegmentSize)
                {
                    for (int y = startY; y < startY + tilesY; y += VirtualMap<char>.SegmentSize)
                    {
                        if (!mapData.AnyInSegmentAtTile(x, y))
                        {
                            y = y / VirtualMap<char>.SegmentSize * VirtualMap<char>.SegmentSize;
                        }
                        else
                        {
                            int x2 = x;
                            for (int index1 = Math.Min(x + VirtualMap<char>.SegmentSize, startX + tilesX); x2 < index1; ++x2)
                            {
                                int y2 = y;
                                for (int index2 = Math.Min(y + VirtualMap<char>.SegmentSize, startY + tilesY); y2 < index2; ++y2)
                                {
                                    Tiles tiles = TileHandler(mapData, x2, y2, forceFill, forceID, behaviour);
                                    if (tiles != null)
                                    {
                                        tileGrid.Tiles[x2 - startX, y2 - startY] = Calc.Random.Choose(tiles.Textures);
                                        if (tiles.HasOverlays)
                                            animatedTiles.Set(x2 - startX, y2 - startY, Calc.Random.Choose(tiles.OverlapSprites));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = startX; x < startX + tilesX; ++x)
                {
                    for (int y = startY; y < startY + tilesY; ++y)
                    {
                        Tiles tiles = TileHandler(null, x, y, forceFill, forceID, behaviour);
                        if (tiles != null)
                        {
                            tileGrid.Tiles[x - startX, y - startY] = Calc.Random.Choose<MTexture>(tiles.Textures);
                            if (tiles.HasOverlays)
                                animatedTiles.Set(x - startX, y - startY, Calc.Random.Choose<string>(tiles.OverlapSprites));
                        }
                    }
                }
            }

            return new Generated()
            {
                TileGrid = tileGrid,
                SpriteOverlay = animatedTiles
            };
        }

        private Tiles TileHandler(
            VirtualMap<char> mapData,
            int x,
            int y,
            Rectangle forceFill,
            char forceID,
            Behaviour behaviour)
        {
            char tile = GetTile(mapData, x, y, forceFill, forceID, behaviour);
            if (IsEmpty(tile))
                return null;
            TerrainType set = lookup[tile];
            bool flag1 = true;
            int num = 0;
            for (int index1 = -1; index1 < 2; ++index1)
            {
                for (int index2 = -1; index2 < 2; ++index2)
                {
                    bool flag2 = CheckTile(set, mapData, x + index2, y + index1, forceFill, behaviour);
                    if (!flag2 && behaviour.EdgesIgnoreOutOfLevel && !CheckForSameLevel(x, y, x + index2, y + index1))
                        flag2 = true;
                    adjacent[num++] = flag2 ? (byte) 1 : (byte) 0;
                    if (!flag2)
                        flag1 = false;
                }
            }
            if (flag1)
                return (behaviour.PaddingIgnoreOutOfLevel ? !CheckTile(set, mapData, x - 2, y, forceFill, behaviour) && CheckForSameLevel(x, y, x - 2, y) || !CheckTile(set, mapData, x + 2, y, forceFill, behaviour) && CheckForSameLevel(x, y, x + 2, y) || !CheckTile(set, mapData, x, y - 2, forceFill, behaviour) && CheckForSameLevel(x, y, x, y - 2) || !CheckTile(set, mapData, x, y + 2, forceFill, behaviour) && CheckForSameLevel(x, y, x, y + 2) : !CheckTile(set, mapData, x - 2, y, forceFill, behaviour) || !CheckTile(set, mapData, x + 2, y, forceFill, behaviour) || !CheckTile(set, mapData, x, y - 2, forceFill, behaviour) || !CheckTile(set, mapData, x, y + 2, forceFill, behaviour)) ? lookup[tile].Padded : lookup[tile].Center;
            foreach (Masked masked in set.Masked)
            {
                bool flag3 = true;
                for (int index = 0; index < 9 & flag3; ++index)
                {
                    if (masked.Mask[index] != 2 && masked.Mask[index] != adjacent[index])
                        flag3 = false;
                }
                if (flag3)
                    return masked.Tiles;
            }
            return null;
        }

        private bool CheckForSameLevel(int x1, int y1, int x2, int y2)
        {
            foreach (Rectangle levelBound in LevelBounds)
            {
                if (levelBound.Contains(x1, y1) && levelBound.Contains(x2, y2))
                    return true;
            }
            return false;
        }

        private bool CheckTile(
            TerrainType set,
            VirtualMap<char> mapData,
            int x,
            int y,
            Rectangle forceFill,
            Behaviour behaviour)
        {
            if (forceFill.Contains(x, y))
                return true;
            if (mapData == null)
                return behaviour.EdgesExtend;
            if (x < 0 || y < 0 || x >= mapData.Columns || y >= mapData.Rows)
            {
                if (!behaviour.EdgesExtend)
                    return false;
                char ch = mapData[Calc.Clamp(x, 0, mapData.Columns - 1), Calc.Clamp(y, 0, mapData.Rows - 1)];
                return !IsEmpty(ch) && !set.Ignore(ch);
            }
            char ch1 = mapData[x, y];
            return !IsEmpty(ch1) && !set.Ignore(ch1);
        }

        private char GetTile(
            VirtualMap<char> mapData,
            int x,
            int y,
            Rectangle forceFill,
            char forceID,
            Behaviour behaviour)
        {
            if (forceFill.Contains(x, y))
                return forceID;
            if (mapData == null)
                return !behaviour.EdgesExtend ? '0' : forceID;
            if (x >= 0 && y >= 0 && x < mapData.Columns && y < mapData.Rows)
                return mapData[x, y];
            if (!behaviour.EdgesExtend)
                return '0';
            int x1 = Calc.Clamp(x, 0, mapData.Columns - 1);
            int y1 = Calc.Clamp(y, 0, mapData.Rows - 1);
            return mapData[x1, y1];
        }

        private bool IsEmpty(char id) => id is '0' or char.MinValue;
    }
}
