// Decompiled with JetBrains decompiler
// Type: Monocle.TileGrid
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public class TileGrid : Component
    {
        public Vector2 Position;
        public Color Color = Color.White;
        public int VisualExtend;
        public VirtualMap<MTexture> Tiles;
        public Camera ClipCamera;
        public float Alpha = 1f;

        public TileGrid(int tileWidth, int tileHeight, int tilesX, int tilesY)
            : base(false, true)
        {
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.Tiles = new VirtualMap<MTexture>(tilesX, tilesY);
        }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public int TilesX => this.Tiles.Columns;

        public int TilesY => this.Tiles.Rows;

        public void Populate(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
        {
            for (int index1 = 0; index1 < tiles.GetLength(0) && index1 + offsetX < this.TilesX; ++index1)
            {
                for (int index2 = 0; index2 < tiles.GetLength(1) && index2 + offsetY < this.TilesY; ++index2)
                    this.Tiles[index1 + offsetX, index2 + offsetY] = tileset[tiles[index1, index2]];
            }
        }

        public void Overlay(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
        {
            for (int index1 = 0; index1 < tiles.GetLength(0) && index1 + offsetX < this.TilesX; ++index1)
            {
                for (int index2 = 0; index2 < tiles.GetLength(1) && index2 + offsetY < this.TilesY; ++index2)
                {
                    if (tiles[index1, index2] >= 0)
                        this.Tiles[index1 + offsetX, index2 + offsetY] = tileset[tiles[index1, index2]];
                }
            }
        }

        public void Extend(int left, int right, int up, int down)
        {
            this.Position -= new Vector2((float) (left * this.TileWidth), (float) (up * this.TileHeight));
            int columns = this.TilesX + left + right;
            int rows = this.TilesY + up + down;
            if (columns <= 0 || rows <= 0)
            {
                this.Tiles = new VirtualMap<MTexture>(0, 0);
            }
            else
            {
                VirtualMap<MTexture> virtualMap = new VirtualMap<MTexture>(columns, rows);
                for (int x1 = 0; x1 < this.TilesX; ++x1)
                {
                    for (int y1 = 0; y1 < this.TilesY; ++y1)
                    {
                        int x2 = x1 + left;
                        int y2 = y1 + up;
                        if (x2 >= 0 && x2 < columns && y2 >= 0 && y2 < rows)
                            virtualMap[x2, y2] = this.Tiles[x1, y1];
                    }
                }
                for (int x = 0; x < left; ++x)
                {
                    for (int y = 0; y < rows; ++y)
                        virtualMap[x, y] = this.Tiles[0, Calc.Clamp(y - up, 0, this.TilesY - 1)];
                }
                for (int x = columns - right; x < columns; ++x)
                {
                    for (int y = 0; y < rows; ++y)
                        virtualMap[x, y] = this.Tiles[this.TilesX - 1, Calc.Clamp(y - up, 0, this.TilesY - 1)];
                }
                for (int y = 0; y < up; ++y)
                {
                    for (int x = 0; x < columns; ++x)
                        virtualMap[x, y] = this.Tiles[Calc.Clamp(x - left, 0, this.TilesX - 1), 0];
                }
                for (int y = rows - down; y < rows; ++y)
                {
                    for (int x = 0; x < columns; ++x)
                        virtualMap[x, y] = this.Tiles[Calc.Clamp(x - left, 0, this.TilesX - 1), this.TilesY - 1];
                }
                this.Tiles = virtualMap;
            }
        }

        public void FillRect(int x, int y, int columns, int rows, MTexture tile)
        {
            int num1 = Math.Max(0, x);
            int num2 = Math.Max(0, y);
            int num3 = Math.Min(this.TilesX, x + columns);
            int num4 = Math.Min(this.TilesY, y + rows);
            for (int x1 = num1; x1 < num3; ++x1)
            {
                for (int y1 = num2; y1 < num4; ++y1)
                    this.Tiles[x1, y1] = tile;
            }
        }

        public void Clear()
        {
            for (int x = 0; x < this.TilesX; ++x)
            {
                for (int y = 0; y < this.TilesY; ++y)
                    this.Tiles[x, y] = (MTexture) null;
            }
        }

        public Rectangle GetClippedRenderTiles()
        {
            Vector2 vector2 = this.Entity.Position + this.Position;
            int val1_1;
            int val1_2;
            int val1_3;
            int val1_4;
            if (this.ClipCamera == null)
            {
                val1_1 = -this.VisualExtend;
                val1_2 = -this.VisualExtend;
                val1_3 = this.TilesX + this.VisualExtend;
                val1_4 = this.TilesY + this.VisualExtend;
            }
            else
            {
                Camera clipCamera = this.ClipCamera;
                val1_1 = (int) Math.Max(0.0, Math.Floor(((double) clipCamera.Left - (double) vector2.X) / (double) this.TileWidth) - (double) this.VisualExtend);
                val1_2 = (int) Math.Max(0.0, Math.Floor(((double) clipCamera.Top - (double) vector2.Y) / (double) this.TileHeight) - (double) this.VisualExtend);
                val1_3 = (int) Math.Min((double) this.TilesX, Math.Ceiling(((double) clipCamera.Right - (double) vector2.X) / (double) this.TileWidth) + (double) this.VisualExtend);
                val1_4 = (int) Math.Min((double) this.TilesY, Math.Ceiling(((double) clipCamera.Bottom - (double) vector2.Y) / (double) this.TileHeight) + (double) this.VisualExtend);
            }
            int x = Math.Max(val1_1, 0);
            int y = Math.Max(val1_2, 0);
            int num1 = Math.Min(val1_3, this.TilesX);
            int num2 = Math.Min(val1_4, this.TilesY);
            return new Rectangle(x, y, num1 - x, num2 - y);
        }

        public override void Render() => this.RenderAt(this.Entity.Position + this.Position);

        public void RenderAt(Vector2 position)
        {
            if ((double) this.Alpha <= 0.0)
                return;
            Rectangle clippedRenderTiles = this.GetClippedRenderTiles();
            Color color = this.Color * this.Alpha;
            for (int left = clippedRenderTiles.Left; left < clippedRenderTiles.Right; ++left)
            {
                for (int top = clippedRenderTiles.Top; top < clippedRenderTiles.Bottom; ++top)
                    this.Tiles[left, top]?.Draw(position + new Vector2((float) (left * this.TileWidth), (float) (top * this.TileHeight)), Vector2.Zero, color);
            }
        }
    }
}
