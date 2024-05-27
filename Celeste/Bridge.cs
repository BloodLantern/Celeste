using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Bridge : Entity
    {
        private List<BridgeTile> tiles;
        private Level level;
        private bool canCollapse;
        private bool canEndCollapseA;
        private bool canEndCollapseB;
        private float collapseTimer;
        private int width;
        private List<Rectangle> tileSizes = new List<Rectangle>();
        private bool ending;
        private float gapStartX;
        private float gapEndX;
        private SoundSource collapseSfx;

        public Bridge(Vector2 position, int width, float gapStartX, float gapEndX)
            : base(position)
        {
            this.width = width;
            this.gapStartX = gapStartX;
            this.gapEndX = gapEndX;
            tileSizes.Add(new Rectangle(0, 0, 16, 52));
            tileSizes.Add(new Rectangle(16, 0, 8, 52));
            tileSizes.Add(new Rectangle(24, 0, 8, 52));
            tileSizes.Add(new Rectangle(32, 0, 8, 52));
            tileSizes.Add(new Rectangle(40, 0, 8, 52));
            tileSizes.Add(new Rectangle(48, 0, 8, 52));
            tileSizes.Add(new Rectangle(56, 0, 8, 52));
            tileSizes.Add(new Rectangle(64, 0, 8, 52));
            tileSizes.Add(new Rectangle(72, 0, 8, 52));
            tileSizes.Add(new Rectangle(80, 0, 16, 52));
            tileSizes.Add(new Rectangle(96, 0, 8, 52));
            Add(collapseSfx = new SoundSource());
        }

        public Bridge(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Nodes[0].X, data.Nodes[1].X)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            tiles = new List<BridgeTile>();
            gapStartX += level.Bounds.Left;
            gapEndX += level.Bounds.Left;
            Calc.PushRandom(1);
            Vector2 position = Position;
            int index = 0;
            while (position.X < X + (double) width)
            {
                Rectangle tileSize = index < 2 || index > 7 ? tileSizes[index] : tileSizes[2 + Calc.Random.Next(6)];
                if (position.X < (double) gapStartX || position.X >= (double) gapEndX)
                {
                    BridgeTile bridgeTile = new BridgeTile(position, tileSize);
                    tiles.Add(bridgeTile);
                    level.Add(bridgeTile);
                }
                position.X += tileSize.Width;
                index = (index + 1) % tileSizes.Count;
            }
            Calc.PopRandom();
        }

        public override void Update()
        {
            base.Update();
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity == null || entity.Dead)
                collapseSfx.Stop();
            if (!canCollapse)
            {
                if (entity == null || entity.X < X + 112.0)
                    return;
                Audio.SetMusic("event:/music/lvl0/bridge");
                collapseSfx.Play("event:/game/00_prologue/bridge_rumble_loop");
                canCollapse = true;
                canEndCollapseA = true;
                canEndCollapseB = true;
                for (int index = 0; index < 11; ++index)
                {
                    tiles[0].Fall(Calc.Random.Range(0.1f, 0.5f));
                    tiles.RemoveAt(0);
                }
            }
            else if (tiles.Count > 0)
            {
                if (entity == null)
                    return;
                if (canEndCollapseA && entity.X > X + (double) width - 216.0)
                {
                    canEndCollapseA = false;
                    for (int index = 0; index < 5; ++index)
                    {
                        tiles[tiles.Count - 8].Fall(Calc.Random.Range(0.1f, 0.5f));
                        tiles.RemoveAt(tiles.Count - 8);
                    }
                }
                else if (canEndCollapseB && entity.X > X + (double) width - 104.0)
                {
                    canEndCollapseB = false;
                    for (int index = 0; index < 7 && tiles.Count > 0; ++index)
                    {
                        tiles[tiles.Count - 1].Fall(Calc.Random.Range(0.1f, 0.3f));
                        tiles.RemoveAt(tiles.Count - 1);
                    }
                }
                else if (collapseTimer > 0.0)
                {
                    collapseTimer -= Engine.DeltaTime;
                    if (tiles.Count < 5 || entity.X < (double) tiles[4].X)
                        return;
                    int index = 0;
                    tiles[index].Fall();
                    tiles.RemoveAt(index);
                }
                else
                {
                    tiles[0].Fall();
                    tiles.RemoveAt(0);
                    collapseTimer = 0.2f;
                }
            }
            else
            {
                if (ending)
                    return;
                ending = true;
                StopCollapseLoop();
            }
        }

        public void StopCollapseLoop() => collapseSfx.Stop();
    }
}
