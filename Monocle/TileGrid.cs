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
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Tiles = new VirtualMap<MTexture>(tilesX, tilesY);
        }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public int TilesX => Tiles.Columns;

        public int TilesY => Tiles.Rows;

        public void Populate(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
        {
            for (int x = 0; x < tiles.GetLength(0) && x + offsetX < TilesX; x++)
            {
                for (int y = 0; y < tiles.GetLength(1) && y + offsetY < TilesY; y++)
                    Tiles[x + offsetX, y + offsetY] = tileset[tiles[x, y]];
            }
        }

        public void Overlay(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
        {
            for (int x = 0; x < tiles.GetLength(0) && x + offsetX < TilesX; x++)
            {
                for (int y = 0; y < tiles.GetLength(1) && y + offsetY < TilesY; y++)
                {
                    if (tiles[x, y] >= 0)
                        Tiles[x + offsetX, y + offsetY] = tileset[tiles[x, y]];
                }
            }
        }

        public void Extend(int left, int right, int up, int down)
        {
            Position -= new Vector2(left * TileWidth, up * TileHeight);
            int columns = TilesX + left + right;
            int rows = TilesY + up + down;
            if (columns <= 0 || rows <= 0)
            {
                Tiles = new VirtualMap<MTexture>(0, 0);
            }
            else
            {
                VirtualMap<MTexture> virtualMap = new(columns, rows);
                for (int x1 = 0; x1 < TilesX; x1++)
                {
                    for (int y1 = 0; y1 < TilesY; y1++)
                    {
                        int x2 = x1 + left;
                        int y2 = y1 + up;
                        if (x2 >= 0 && x2 < columns && y2 >= 0 && y2 < rows)
                            virtualMap[x2, y2] = Tiles[x1, y1];
                    }
                }
                for (int x = 0; x < left; x++)
                {
                    for (int y = 0; y < rows; y++)
                        virtualMap[x, y] = Tiles[0, Calc.Clamp(y - up, 0, TilesY - 1)];
                }
                for (int x = columns - right; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                        virtualMap[x, y] = Tiles[TilesX - 1, Calc.Clamp(y - up, 0, TilesY - 1)];
                }
                for (int y = 0; y < up; y++)
                {
                    for (int x = 0; x < columns; x++)
                        virtualMap[x, y] = Tiles[Calc.Clamp(x - left, 0, TilesX - 1), 0];
                }
                for (int y = rows - down; y < rows; y++)
                {
                    for (int x = 0; x < columns; x++)
                        virtualMap[x, y] = Tiles[Calc.Clamp(x - left, 0, TilesX - 1), TilesY - 1];
                }
                Tiles = virtualMap;
            }
        }

        public void FillRect(int x, int y, int columns, int rows, MTexture tile)
        {
            int num1 = Math.Max(0, x);
            int num2 = Math.Max(0, y);
            int num3 = Math.Min(TilesX, x + columns);
            int num4 = Math.Min(TilesY, y + rows);
            for (int x1 = num1; x1 < num3; x1++)
            {
                for (int y1 = num2; y1 < num4; y1++)
                    Tiles[x1, y1] = tile;
            }
        }

        public void Clear()
        {
            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                    Tiles[x, y] = null;
            }
        }

        public Rectangle GetClippedRenderTiles()
        {
            Vector2 vector2 = Entity.Position + Position;
            int val1_1;
            int val1_2;
            int val1_3;
            int val1_4;
            if (ClipCamera == null)
            {
                val1_1 = -VisualExtend;
                val1_2 = -VisualExtend;
                val1_3 = TilesX + VisualExtend;
                val1_4 = TilesY + VisualExtend;
            }
            else
            {
                Camera clipCamera = ClipCamera;
                val1_1 = (int) Math.Max(0, Math.Floor(((double) clipCamera.Left - vector2.X) / TileWidth) - VisualExtend);
                val1_2 = (int) Math.Max(0, Math.Floor(((double) clipCamera.Top - vector2.Y) / TileHeight) - VisualExtend);
                val1_3 = (int) Math.Min(TilesX, Math.Ceiling(((double) clipCamera.Right - vector2.X) / TileWidth) + VisualExtend);
                val1_4 = (int) Math.Min(TilesY, Math.Ceiling(((double) clipCamera.Bottom - vector2.Y) / TileHeight) + VisualExtend);
            }
            int x = Math.Max(val1_1, 0);
            int y = Math.Max(val1_2, 0);
            int num1 = Math.Min(val1_3, TilesX);
            int num2 = Math.Min(val1_4, TilesY);
            return new Rectangle(x, y, num1 - x, num2 - y);
        }

        public override void Render() => RenderAt(Entity.Position + Position);

        public void RenderAt(Vector2 position)
        {
            if (Alpha <= 0f)
                return;

            Rectangle clippedRenderTiles = GetClippedRenderTiles();
            Color color = Color * Alpha;
            for (int left = clippedRenderTiles.Left; left < clippedRenderTiles.Right; left++)
            {
                for (int top = clippedRenderTiles.Top; top < clippedRenderTiles.Bottom; top++)
                    Tiles[left, top]?.Draw(position + new Vector2(left * TileWidth, top * TileHeight), Vector2.Zero, color);
            }
        }
    }
}
