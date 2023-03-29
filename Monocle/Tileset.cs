// Decompiled with JetBrains decompiler
// Type: Monocle.Tileset
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
  public class Tileset
  {
    private MTexture[,] tiles;

    public Tileset(MTexture texture, int tileWidth, int tileHeight)
    {
      Texture = texture;
      TileWidth = tileWidth;
      tiles = new MTexture[Texture.Width / tileWidth, Texture.Height / tileHeight];
      for (int index1 = 0; index1 < Texture.Width / tileWidth; ++index1)
      {
        for (int index2 = 0; index2 < Texture.Height / tileHeight; ++index2)
          tiles[index1, index2] = new MTexture(Texture, index1 * tileWidth, index2 * tileHeight, tileWidth, tileHeight);
      }
    }

    public MTexture Texture { get; private set; }

    public int TileWidth { get; private set; }

    public int TileHeight { get; private set; }

    public MTexture this[int x, int y] => tiles[x, y];

    public MTexture this[int index] => index < 0 ? (MTexture) null : tiles[index % tiles.GetLength(0), index / tiles.GetLength(0)];
  }
}
