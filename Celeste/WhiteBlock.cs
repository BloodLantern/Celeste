using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class WhiteBlock : JumpThru
    {
        private const float duckDuration = 3f;
        private float playerDuckTimer;
        private bool enabled = true;
        private bool activated;
        private Image sprite;
        private Entity bgSolidTiles;

        public WhiteBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, 48, true)
        {
            Add(sprite = new Image(GFX.Game["objects/whiteblock"]));
            Depth = 8990;
            SurfaceSoundIndex = 27;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!(scene as Level).Session.HeartGem)
                return;
            Disable();
        }

        private void Disable()
        {
            enabled = false;
            sprite.Color = Color.White * 0.25f;
            Collidable = false;
        }

        private void Activate(Player player)
        {
            Audio.Play("event:/game/04_cliffside/whiteblock_fallthru", Center);
            activated = true;
            Collidable = false;
            player.Depth = 10001;
            Depth = -9000;
            Level scene = Scene as Level;
            Rectangle rectangle = new Rectangle(scene.Bounds.Left / 8, scene.Bounds.Y / 8, scene.Bounds.Width / 8, scene.Bounds.Height / 8);
            Rectangle tileBounds = scene.Session.MapData.TileBounds;
            bool[,] data = new bool[rectangle.Width, rectangle.Height];
            for (int index1 = 0; index1 < rectangle.Width; ++index1)
            {
                for (int index2 = 0; index2 < rectangle.Height; ++index2)
                    data[index1, index2] = scene.BgData[index1 + rectangle.Left - tileBounds.Left, index2 + rectangle.Top - tileBounds.Top] != '0';
            }
            Rectangle bounds = scene.Bounds;
            double left = bounds.Left;
            bounds = scene.Bounds;
            double top = bounds.Top;
            bgSolidTiles = new Solid(new Vector2((float) left, (float) top), 1f, 1f, true);
            bgSolidTiles.Collider = new Grid(8f, 8f, data);
            Scene.Add(bgSolidTiles);
        }

        public override void Update()
        {
            base.Update();
            if (!enabled)
                return;
            if (!activated)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (HasPlayerRider() && entity != null && entity.Ducking)
                {
                    playerDuckTimer += Engine.DeltaTime;
                    if (playerDuckTimer >= 3.0)
                        Activate(entity);
                }
                else
                    playerDuckTimer = 0.0f;
                if (!(Scene as Level).Session.HeartGem)
                    return;
                Disable();
            }
            else
            {
                if (Scene.Tracker.GetEntity<HeartGem>() != null)
                    return;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null)
                    return;
                Disable();
                entity.Depth = 0;
                Scene.Remove(bgSolidTiles);
            }
        }
    }
}
