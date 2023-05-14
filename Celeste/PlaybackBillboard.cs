// Decompiled with JetBrains decompiler
// Type: Celeste.PlaybackBillboard
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class PlaybackBillboard : Entity
    {
        public const int BGDepth = 9010;
        public static readonly Color BackgroundColor = Color.Lerp(Color.DarkSlateBlue, Color.Black, 0.6f);
        public uint Seed;
        private MTexture[,] tiles;

        public PlaybackBillboard(EntityData e, Vector2 offset)
        {
            Position = e.Position + offset;
            Collider = new Hitbox(e.Width, e.Height);
            Depth = 9010;
            Add(new CustomBloom(new Action(RenderBloom)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new PlaybackBillboard.FG(this));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MTexture mtexture = GFX.Game["scenery/tvSlices"];
            tiles = new MTexture[mtexture.Width / 8, mtexture.Height / 8];
            for (int index1 = 0; index1 < mtexture.Width / 8; ++index1)
            {
                for (int index2 = 0; index2 < mtexture.Height / 8; ++index2)
                {
                    tiles[index1, index2] = mtexture.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
                }
            }
            int x1 = (int)((double)Width / 8.0);
            int y1 = (int)((double)Height / 8.0);
            for (int x2 = -1; x2 <= x1; ++x2)
            {
                AutoTile(x2, -1);
                AutoTile(x2, y1);
            }
            for (int y2 = 0; y2 < y1; ++y2)
            {
                AutoTile(-1, y2);
                AutoTile(x1, y2);
            }
        }

        private void AutoTile(int x, int y)
        {
            if (!Empty(x, y))
            {
                return;
            }

            bool flag1 = !Empty(x - 1, y);
            bool flag2 = !Empty(x + 1, y);
            bool flag3 = !Empty(x, y - 1);
            bool flag4 = !Empty(x, y + 1);
            bool flag5 = !Empty(x - 1, y - 1);
            bool flag6 = !Empty(x + 1, y - 1);
            bool flag7 = !Empty(x - 1, y + 1);
            bool flag8 = !Empty(x + 1, y + 1);
            if (((flag2 ? 0 : (!flag4 ? 1 : 0)) & (flag8 ? 1 : 0)) != 0)
            {
                Tile(x, y, tiles[0, 0]);
            }
            else if (((flag1 ? 0 : (!flag4 ? 1 : 0)) & (flag7 ? 1 : 0)) != 0)
            {
                Tile(x, y, tiles[2, 0]);
            }
            else if (((flag3 ? 0 : (!flag2 ? 1 : 0)) & (flag6 ? 1 : 0)) != 0)
            {
                Tile(x, y, tiles[0, 2]);
            }
            else if (((flag3 ? 0 : (!flag1 ? 1 : 0)) & (flag5 ? 1 : 0)) != 0)
            {
                Tile(x, y, tiles[2, 2]);
            }
            else if (flag2 & flag4)
            {
                Tile(x, y, tiles[3, 0]);
            }
            else if (flag1 & flag4)
            {
                Tile(x, y, tiles[4, 0]);
            }
            else if (flag2 & flag3)
            {
                Tile(x, y, tiles[3, 2]);
            }
            else if (flag1 & flag3)
            {
                Tile(x, y, tiles[4, 2]);
            }
            else if (flag4)
            {
                Tile(x, y, tiles[1, 0]);
            }
            else if (flag2)
            {
                Tile(x, y, tiles[0, 1]);
            }
            else if (flag1)
            {
                Tile(x, y, tiles[2, 1]);
            }
            else
            {
                if (!flag3)
                {
                    return;
                }

                Tile(x, y, tiles[1, 2]);
            }
        }

        private void Tile(int x, int y, MTexture tile)
        {
            Monocle.Image image = new(tile)
            {
                Position = new Vector2(x, y) * 8f
            };
            Add(image);
        }

        private bool Empty(int x, int y)
        {
            return !Scene.CollideCheck<PlaybackBillboard>(new Rectangle((int)X + (x * 8), (int)Y + (y * 8), 8, 8));
        }

        public override void Update()
        {
            base.Update();
            if (!Scene.OnInterval(0.1f))
            {
                return;
            }

            ++Seed;
        }

        private void RenderBloom()
        {
            Draw.Rect(Collider, Color.White * 0.4f);
        }

        public override void Render()
        {
            base.Render();
            uint seed = Seed;
            Draw.Rect(Collider, PlaybackBillboard.BackgroundColor);
            PlaybackBillboard.DrawNoise(Collider.Bounds, ref seed, Color.White * 0.1f);
        }

        public static void DrawNoise(Rectangle bounds, ref uint seed, Color color)
        {
            MTexture mtexture = GFX.Game["util/noise"];
            Vector2 vector2_1 = new(PlaybackBillboard.PseudoRandRange(ref seed, 0.0f, mtexture.Width / 2), PlaybackBillboard.PseudoRandRange(ref seed, 0.0f, mtexture.Height / 2));
            Vector2 vector2_2 = new Vector2(mtexture.Width, mtexture.Height) / 2f;
            for (float num1 = 0.0f; (double)num1 < bounds.Width; num1 += vector2_2.X)
            {
                float width = Math.Min(bounds.Width - num1, vector2_2.X);
                for (float num2 = 0.0f; (double)num2 < bounds.Height; num2 += vector2_2.Y)
                {
                    float height = Math.Min(bounds.Height - num2, vector2_2.Y);
                    Rectangle rectangle = new((int)(mtexture.ClipRect.X + (double)vector2_1.X), (int)(mtexture.ClipRect.Y + (double)vector2_1.Y), (int)width, (int)height);
                    Draw.SpriteBatch.Draw(mtexture.Texture.Texture, new Vector2(bounds.X + num1, bounds.Y + num2), new Rectangle?(rectangle), color);
                }
            }
        }

        private static uint PseudoRand(ref uint seed)
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            return seed;
        }

        private static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (float)(PlaybackBillboard.PseudoRand(ref seed) % 1000U / 1000.0 * ((double)max - (double)min));
        }

        private class FG : Entity
        {
            public PlaybackBillboard Parent;

            public FG(PlaybackBillboard parent)
            {
                Parent = parent;
                Depth = Parent.Depth - 5;
            }

            public override void Render()
            {
                uint seed = Parent.Seed;
                PlaybackBillboard.DrawNoise(Parent.Collider.Bounds, ref seed, Color.White * 0.1f);
                for (int y = (int)Parent.Y; y < (double)Parent.Bottom; y += 2)
                {
                    float num = (float)(0.05000000074505806 + ((1.0 + Math.Sin((y / 16.0) + (Scene.TimeActive * 2.0))) / 2.0 * 0.20000000298023224));
                    Draw.Line(Parent.X, y, Parent.X + Parent.Width, y, Color.Teal * num);
                }
            }
        }
    }
}
