﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BackgroundTiles : Entity
    {
        public TileGrid Tiles;

        public BackgroundTiles(Vector2 position, VirtualMap<char> data)
        {
            this.Position = position;
            this.Tag = (int) Tags.Global;
            this.Tiles = GFX.BGAutotiler.GenerateMap(data, false).TileGrid;
            this.Tiles.VisualExtend = 1;
            this.Add((Component) this.Tiles);
            this.Depth = 10000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.Tiles.ClipCamera = this.SceneAs<Level>().Camera;
        }
    }
}
