using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class WallBooster : Entity
    {
        public Facings Facing;
        private StaticMover staticMover;
        private ClimbBlocker climbBlocker;
        private SoundSource idleSfx;
        public bool IceMode;
        private bool notCoreMode;
        private List<Sprite> tiles;

        public WallBooster(Vector2 position, float height, bool left, bool notCoreMode)
            : base(position)
        {
            Tag = (int) Tags.TransitionUpdate;
            Depth = 1999;
            this.notCoreMode = notCoreMode;
            if (left)
            {
                Facing = Facings.Left;
                Collider = new Hitbox(2f, height);
            }
            else
            {
                Facing = Facings.Right;
                Collider = new Hitbox(2f, height, 6f);
            }
            Add(new CoreModeListener(OnChangeMode));
            Add(staticMover = new StaticMover());
            Add(climbBlocker = new ClimbBlocker(false));
            Add(idleSfx = new SoundSource());
            tiles = BuildSprite(left);
        }

        public WallBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, data.Bool("left"), data.Bool(nameof (notCoreMode)))
        {
        }

        private List<Sprite> BuildSprite(bool left)
        {
            List<Sprite> spriteList = new List<Sprite>();
            for (int y = 0; y < (double) Height; y += 8)
            {
                string id = y != 0 ? (y + 16 <= (double) Height ? "WallBoosterMid" : "WallBoosterBottom") : "WallBoosterTop";
                Sprite sprite = GFX.SpriteBank.Create(id);
                if (!left)
                {
                    sprite.FlipX = true;
                    sprite.Position = new Vector2(4f, y);
                }
                else
                    sprite.Position = new Vector2(0.0f, y);
                spriteList.Add(sprite);
                Add(sprite);
            }
            return spriteList;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session.CoreModes mode = Session.CoreModes.None;
            if (SceneAs<Level>().CoreMode == Session.CoreModes.Cold || notCoreMode)
                mode = Session.CoreModes.Cold;
            OnChangeMode(mode);
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            IceMode = mode == Session.CoreModes.Cold;
            climbBlocker.Blocking = IceMode;
            tiles.ForEach(t => t.Play(IceMode ? "ice" : "hot"));
            if (IceMode)
            {
                idleSfx.Stop();
            }
            else
            {
                if (idleSfx.Playing)
                    return;
                idleSfx.Play("event:/env/local/09_core/conveyor_idle");
            }
        }

        public override void Update()
        {
            PositionIdleSfx();
            if ((Scene as Level).Transitioning)
                return;
            base.Update();
        }

        private void PositionIdleSfx()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            idleSfx.Position = Calc.ClosestPointOnLine(Position, Position + new Vector2(0.0f, Height), entity.Center) - Position;
            idleSfx.UpdateSfxPosition();
        }
    }
}
