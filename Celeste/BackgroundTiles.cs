// Decompiled with JetBrains decompiler
// Type: Celeste.BackgroundTiles
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            Tag = (int)Tags.Global;
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
