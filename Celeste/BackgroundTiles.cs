using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BackgroundTiles : Entity
    {
        public TileGrid Tiles;

        public BackgroundTiles(Vector2 position, VirtualMap<char> data)
        {
            Position = position;
            Tag = (int) Tags.Global;
            Tiles = GFX.BGAutotiler.GenerateMap(data, false).TileGrid;
            Tiles.VisualExtend = 1;
            Add(Tiles);
            Depth = 10000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Tiles.ClipCamera = SceneAs<Level>().Camera;
        }
    }
}
