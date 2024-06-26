﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class CoverupWall : Entity
    {
        private char fillTile;
        private TileGrid tiles;
        private EffectCutout cutout;

        public CoverupWall(Vector2 position, char tile, float width, float height)
            : base(position)
        {
            fillTile = tile;
            Depth = -13000;
            Collider = new Hitbox(width, height);
            Add(cutout = new EffectCutout());
        }

        public CoverupWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int) Width / 8;
            int tilesY = (int) Height / 8;
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int) X / 8 - tileBounds.Left;
            int y = (int) Y / 8 - tileBounds.Top;
            Add(tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid);
            Add(new TileInterceptor(tiles, false));
        }
    }
}
