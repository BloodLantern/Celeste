// Decompiled with JetBrains decompiler
// Type: Celeste.CrumbleWallOnRumble
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CrumbleWallOnRumble : Solid
    {
        private readonly bool permanent;
        private EntityID id;
        private readonly char tileType;
        private readonly bool blendIn;

        public CrumbleWallOnRumble(
            Vector2 position,
            char tileType,
            float width,
            float height,
            bool blendIn,
            bool persistent,
            EntityID id)
            : base(position, width, height, true)
        {
            Depth = -12999;
            this.id = id;
            this.tileType = tileType;
            this.blendIn = blendIn;
            permanent = persistent;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[this.tileType];
        }

        public CrumbleWallOnRumble(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Char("tiletype", 'm'), data.Width, data.Height, data.Bool("blendin"), data.Bool("persistent"), id)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TileGrid tileGrid;
            if (!blendIn)
            {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            }
            else
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)((double)X / 8.0) - tileBounds.Left;
                int y = (int)((double)Y / 8.0) - tileBounds.Top;
                int tilesX = (int)Width / 8;
                int tilesY = (int)Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, true));
            Add(new LightOcclude());
            if (!CollideCheck<Player>())
            {
                return;
            }

            RemoveSelf();
        }

        public void Break()
        {
            if (!Collidable || Scene == null)
            {
                return;
            }

            _ = Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
            Collidable = false;
            for (int index1 = 0; index1 < (double)Width / 8.0; ++index1)
            {
                for (int index2 = 0; index2 < (double)Height / 8.0; ++index2)
                {
                    if (!Scene.CollideCheck<Solid>(new Rectangle((int)X + (index1 * 8), (int)Y + (index2 * 8), 8, 8)))
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + (index1 * 8), 4 + (index2 * 8)), tileType).BlastFrom(TopCenter));
                    }
                }
            }
            if (permanent)
            {
                _ = SceneAs<Level>().Session.DoNotLoad.Add(id);
            }

            RemoveSelf();
        }
    }
}
