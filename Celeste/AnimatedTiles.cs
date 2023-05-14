// Decompiled with JetBrains decompiler
// Type: Celeste.AnimatedTiles
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class AnimatedTiles : Component
    {
        public Camera ClipCamera;
        public Vector2 Position;
        public Color Color = Color.White;
        public float Alpha = 1f;
        public AnimatedTilesBank Bank;
        private readonly VirtualMap<List<Tile>> tiles;

        public AnimatedTiles(int columns, int rows, AnimatedTilesBank bank)
            : base(true, true)
        {
            tiles = new VirtualMap<List<Tile>>(columns, rows);
            Bank = bank;
        }

        public void Set(int x, int y, string name, float scaleX = 1f, float scaleY = 1f)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            AnimatedTilesBank.Animation animation = Bank.AnimationsByName[name];
            (tiles[x, y] ?? (tiles[x, y] = new List<Tile>())).Add(new Tile()
            {
                AnimationID = animation.ID,
                Frame = Calc.Random.Next(animation.Frames.Length),
                Scale = new Vector2(scaleX, scaleY)
            });
        }

        public Rectangle GetClippedRenderTiles(int extend)
        {
            Vector2 vector2 = Entity.Position + Position;
            int val1_1;
            int val1_2;
            int val1_3;
            int val1_4;
            if (ClipCamera == null)
            {
                val1_1 = -extend;
                val1_2 = -extend;
                val1_3 = tiles.Columns + extend;
                val1_4 = tiles.Rows + extend;
            }
            else
            {
                Camera clipCamera = ClipCamera;
                val1_1 = (int)Math.Max(0, Math.Floor((clipCamera.Left - vector2.X) / 8) - extend);
                val1_2 = (int)Math.Max(0, Math.Floor((clipCamera.Top - vector2.Y) / 8) - extend);
                val1_3 = (int)Math.Min(tiles.Columns, Math.Ceiling((clipCamera.Right - vector2.X) / 8) + extend);
                val1_4 = (int)Math.Min(tiles.Rows, Math.Ceiling((clipCamera.Bottom - vector2.Y) / 8) + extend);
            }
            int x = Math.Max(val1_1, 0);
            int y = Math.Max(val1_2, 0);
            int num1 = Math.Min(val1_3, tiles.Columns);
            int num2 = Math.Min(val1_4, tiles.Rows);
            return new Rectangle(x, y, num1 - x, num2 - y);
        }

        public override void Update()
        {
            Rectangle clippedRenderTiles = GetClippedRenderTiles(1);
            for (int left = clippedRenderTiles.Left; left < clippedRenderTiles.Right; ++left)
            {
                for (int top = clippedRenderTiles.Top; top < clippedRenderTiles.Bottom; ++top)
                {
                    List<Tile> tile = tiles[left, top];
                    if (tile != null)
                    {
                        for (int index = 0; index < tile.Count; ++index)
                        {
                            AnimatedTilesBank.Animation animation = Bank.Animations[tile[index].AnimationID];
                            tile[index].Frame += Engine.DeltaTime / animation.Delay;
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            RenderAt(Entity.Position + Position);
        }

        public void RenderAt(Vector2 position)
        {
            Rectangle clippedRenderTiles = GetClippedRenderTiles(1);
            Color color = Color * Alpha;
            for (int left = clippedRenderTiles.Left; left < clippedRenderTiles.Right; ++left)
            {
                for (int top = clippedRenderTiles.Top; top < clippedRenderTiles.Bottom; ++top)
                {
                    List<Tile> tile1 = tiles[left, top];
                    if (tile1 != null)
                    {
                        for (int index = 0; index < tile1.Count; ++index)
                        {
                            Tile tile2 = tile1[index];
                            AnimatedTilesBank.Animation animation = Bank.Animations[tile2.AnimationID];
                            animation.Frames[(int)tile2.Frame % animation.Frames.Length].Draw(position + animation.Offset + (new Vector2(left + 0.5f, top + 0.5f) * 8f), animation.Origin, color, tile2.Scale);
                        }
                    }
                }
            }
        }

        private class Tile
        {
            public int AnimationID;
            public float Frame;
            public Vector2 Scale;
        }
    }
}
