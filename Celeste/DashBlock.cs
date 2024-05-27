using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class DashBlock : Solid
    {
        private bool permanent;
        private EntityID id;
        private char tileType;
        private float width;
        private float height;
        private bool blendIn;
        private bool canDash;

        public DashBlock(
            Vector2 position,
            char tiletype,
            float width,
            float height,
            bool blendIn,
            bool permanent,
            bool canDash,
            EntityID id)
            : base(position, width, height, true)
        {
            Depth = -12999;
            this.id = id;
            this.permanent = permanent;
            this.width = width;
            this.height = height;
            this.blendIn = blendIn;
            this.canDash = canDash;
            tileType = tiletype;
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        }

        public DashBlock(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("blendin"), data.Bool(nameof (permanent), true), data.Bool(nameof (canDash), true), id)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TileGrid tileGrid;
            if (!blendIn)
            {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) width / 8, (int) height / 8).TileGrid;
                Add(new LightOcclude());
            }
            else
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (X / 8.0) - tileBounds.Left;
                int y = (int) (Y / 8.0) - tileBounds.Top;
                int tilesX = (int) Width / 8;
                int tilesY = (int) Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
                Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, true));
            if (!CollideCheck<Player>())
                return;
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Celeste.Freeze(0.05f);
        }

        public void Break(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
        {
            if (playSound)
            {
                if (tileType == '1')
                    Audio.Play("event:/game/general/wall_break_dirt", Position);
                else if (tileType == '3')
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                else if (tileType == '9')
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                else
                    Audio.Play("event:/game/general/wall_break_stone", Position);
            }
            for (int index1 = 0; index1 < Width / 8.0; ++index1)
            {
                for (int index2 = 0; index2 < Height / 8.0; ++index2)
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + index1 * 8, 4 + index2 * 8), tileType, playDebrisSound).BlastFrom(from));
            }
            Collidable = false;
            if (permanent)
                RemoveAndFlagAsGone();
            else
                RemoveSelf();
        }

        public void RemoveAndFlagAsGone()
        {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
                return DashCollisionResults.NormalCollision;
            Break(player.Center, direction);
            return DashCollisionResults.Rebound;
        }

        public enum Modes
        {
            Dash,
            FinalBoss,
            Crusher,
        }
    }
}
